[CmdletBinding()]
param(
    [string]$Version,
    [string]$Configuration = 'Release',
    [string]$OutputRoot = (Join-Path $PSScriptRoot 'artifacts')
)

$ErrorActionPreference = 'Stop'

$repoRoot = $PSScriptRoot
$solution = Join-Path $repoRoot 'GopsDailySheet.sln'
$assemblyInfoPath = Join-Path $repoRoot 'app\Properties\AssemblyInfo.cs'
$appFolder = Join-Path $OutputRoot 'GopsDailySheet'
$zipPath = Join-Path $OutputRoot 'GopsDailySheet-portable.zip'
$exePath = Join-Path $appFolder 'GopsDailySheet.exe'

function Resolve-MSBuild {
    $onPath = Get-Command 'msbuild' -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path -LiteralPath $vswhere) {
        $found = @(& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe')
        if ($found.Count -gt 0) { return $found[0] }
    }

    throw 'MSBuild not found. Install Visual Studio 2022 or the Visual Studio Build Tools with the .NET desktop build tools.'
}

function Write-AppReadme {
    param([string]$Version)

    $rule = '=' * 40
    $text = @"
GopsDailySheet $Version - portable app
$rule

Install (once)

1. Copy this whole folder out of the shared drive onto the tablet,
   for example to C:\Apps\GopsDailySheet. Copying the folder is the install.
2. Start GopsDailySheet.exe.

Updating

Close the app, replace this folder with a newer copy from the shared drive,
then start GopsDailySheet.exe again.

Notes

- The app never writes to this folder, so it is safe to keep on a shared drive.
- Tabs, urls and font size are configured in GopsDailySheet.exe.config.
- Browsing data (cookies, logins, cache) is kept per Windows user in
  %LOCALAPPDATA%\GopsDailySheet.
- The running version is shown at the bottom right of the app window.

Requirements

- Windows 10 or later. .NET Framework 4.7.2 or later is included with Windows.
- Microsoft Edge WebView2 Runtime:
  https://developer.microsoft.com/microsoft-edge/webview2/
"@
    $text | Set-Content -LiteralPath (Join-Path $appFolder 'README.txt') -Encoding UTF8
}

$Version = ($Version -replace '^v', '')
if ($Version -and $Version -notmatch '^\d+(\.\d+){1,3}$') {
    throw "Version must be 1 to 4 dot-separated numbers (e.g. 1.2.5.0), got '$Version'."
}

$msbuild = Resolve-MSBuild
Write-Host "Using $msbuild"

$originalAssemblyInfo = $null
if ($Version) {
    $originalAssemblyInfo = Get-Content -LiteralPath $assemblyInfoPath -Raw
    Write-Host "Stamping assembly version $Version"
    $originalAssemblyInfo -replace '\[assembly: AssemblyVersion\("[^"]*"\)\]', "[assembly: AssemblyVersion(`"$Version`")]" |
        Set-Content -LiteralPath $assemblyInfoPath -NoNewline
}

try {
    Write-Host 'Restoring NuGet packages...'
    & $msbuild $solution -t:Restore -p:RestorePackagesConfig=true -v:minimal -nologo
    if ($LASTEXITCODE -ne 0) { throw "Restore failed with exit code $LASTEXITCODE." }

    if (Test-Path -LiteralPath $appFolder) {
        Remove-Item -LiteralPath $appFolder -Recurse -Force
    }
    $null = New-Item -ItemType Directory -Path $OutputRoot -Force
    $outDir = "$appFolder\"

    Write-Host "Building $Configuration to $outDir"
    & $msbuild $solution -t:Rebuild -p:Configuration=$Configuration -p:Platform='Any CPU' "-p:OutDir=$outDir" -v:minimal -nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE." }

    if (-not (Test-Path -LiteralPath $exePath)) {
        throw "Expected $exePath to exist after the build."
    }

    $builtVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exePath).FileVersion
    Write-AppReadme -Version $builtVersion

    if (Test-Path -LiteralPath $zipPath) {
        Remove-Item -LiteralPath $zipPath -Force
    }
    Write-Host "Zipping $zipPath"
    Compress-Archive -Path (Join-Path $appFolder '*') -DestinationPath $zipPath

    $sizeMb = [Math]::Round((Get-Item -LiteralPath $zipPath).Length / 1MB, 1)
    Write-Host ''
    Write-Host "Done. Folder: $appFolder"
    Write-Host "Zip: $zipPath ($sizeMb MB, version $builtVersion)"
}
finally {
    if ($null -ne $originalAssemblyInfo) {
        Set-Content -LiteralPath $assemblyInfoPath -Value $originalAssemblyInfo -NoNewline
    }
}