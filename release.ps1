[CmdletBinding()]
param(
    [string]$Tag,
    [string]$Configuration = 'Release',
    [string]$OutputRoot = (Join-Path $PSScriptRoot 'artifacts')
)

$ErrorActionPreference = 'Stop'

$repoRoot = $PSScriptRoot
$assemblyInfoPath = Join-Path $repoRoot 'app\Properties\AssemblyInfo.cs'
$assemblyInfoRepoPath = 'app/Properties/AssemblyInfo.cs'
$buildScript = Join-Path $repoRoot 'build-portable.ps1'
$versionPattern = '\[assembly: AssemblyVersion\("(?<version>\d+(\.\d+){1,3})"\)\]'

function Get-AssemblyVersionText {
    $match = [regex]::Match((Get-Content -LiteralPath $assemblyInfoPath -Raw), $versionPattern)
    if (-not $match.Success) {
        throw "No [assembly: AssemblyVersion(""1.2.3.4"")] found in $assemblyInfoPath"
    }

    return $match.Groups['version'].Value
}

function Get-LatestTagVersionText {
    $versions = @(
        git -C $repoRoot tag --list 'v*.*.*.*' |
            Where-Object { $_ -match '^v\d+\.\d+\.\d+\.\d+$' } |
            ForEach-Object { $_.Substring(1) }
    )
    if ($versions.Count -eq 0) { return $null }

    return ($versions | ForEach-Object { [version]$_ } | Sort-Object -Descending)[0].ToString(4)
}

if ($Tag) {
    if ($Tag -notmatch '^v\d+\.\d+\.\d+\.\d+$') {
        throw "Tag must look like v1.2.3.4, got '$Tag'."
    }

    $versionText = $Tag.Substring(1)
}
else {
    $known = @((Get-AssemblyVersionText), (Get-LatestTagVersionText)) |
        Where-Object { $_ } |
        ForEach-Object { [version]$_ } |
        Sort-Object -Descending |
        Select-Object -First 1

    $next = [version]::new($known.Major, $known.Minor, [Math]::Max($known.Build, 0), ([Math]::Max($known.Revision, 0) + 1))
    $versionText = $next.ToString(4)
    $Tag = "v$versionText"
}

# An already tagged tip means there is nothing new to release, so stop before
# committing a version bump on top of it: that would only add an empty release.
# Re-running with the tag that is already on HEAD is still allowed, which is how
# a release is rebuilt from a tag that was pushed from a working copy.
$headTags = @(git -C $repoRoot tag --points-at HEAD | Where-Object { $_ -ne $Tag })
if ($headTags.Count -gt 0) {
    throw "HEAD is already tagged $($headTags -join ', '), so there is nothing new to release. Commit something first, or pass -Tag $($headTags[0]) to rebuild that release."
}

$uncommitted = @(git -C $repoRoot status --porcelain | Where-Object { $_ -notmatch 'app/Properties/AssemblyInfo\.cs$' })
if ($uncommitted.Count -gt 0) {
    Write-Warning "$($uncommitted.Count) uncommitted change(s) will be built into the zip but will not be part of tag $Tag."
}

if (@(git -C $repoRoot tag --list $Tag).Count -gt 0) {
    $tagCommit = (git -C $repoRoot rev-list -n 1 $Tag).Trim()
    $headCommit = (git -C $repoRoot rev-parse HEAD).Trim()
    if ($tagCommit -ne $headCommit) {
        throw "Tag $Tag already exists on $tagCommit and will not be moved."
    }

    $taggedAssemblyInfo = git -C $repoRoot show "${Tag}:$assemblyInfoRepoPath" | Out-String
    $taggedVersion = if ($taggedAssemblyInfo -match $versionPattern) { $Matches['version'] } else { 'unknown' }
    if ($taggedVersion -ne $versionText) {
        throw "Tag $Tag points at a commit whose assembly version is $taggedVersion, not $versionText."
    }

    Write-Host "Tag $Tag already exists at HEAD with version $versionText, reusing it."
}
else {
    if ((Get-AssemblyVersionText) -ne $versionText) {
        $assemblyInfo = Get-Content -LiteralPath $assemblyInfoPath -Raw
        $updatedAssemblyInfo = $assemblyInfo -replace $versionPattern, "[assembly: AssemblyVersion(`"$versionText`")]"
        [System.IO.File]::WriteAllText($assemblyInfoPath, $updatedAssemblyInfo)
        git -C $repoRoot add -- $assemblyInfoPath
        git -C $repoRoot commit -q -m "bump version to $versionText" -- $assemblyInfoPath
        if ($LASTEXITCODE -ne 0) { throw "Could not commit the version bump." }
        Write-Host "Committed version $versionText."
    }

    git -C $repoRoot tag -a $Tag -m "GopsDailySheet $versionText"
    if ($LASTEXITCODE -ne 0) { throw "Could not create tag $Tag." }
    Write-Host "Tagged $Tag."
}

& $buildScript -Version $versionText -Configuration $Configuration -OutputRoot $OutputRoot
if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE." }

$branch = (git -C $repoRoot rev-parse --abbrev-ref HEAD).Trim()
$null = New-Item -ItemType Directory -Path $OutputRoot -Force
Set-Content -LiteralPath (Join-Path $OutputRoot 'GopsDailySheet.tag') -Value $Tag -NoNewline

if ($env:GITHUB_OUTPUT) {
    [System.IO.File]::AppendAllText($env:GITHUB_OUTPUT, "tag=$Tag$([System.Environment]::NewLine)")
}

Write-Host ''
Write-Host "$Tag is ready in $OutputRoot."
Write-Host "Publish it with: git push origin $branch $Tag"