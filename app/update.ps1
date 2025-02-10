param (
    [string]$AppName,
    [string]$ExtractPath
)

# Function to bring a window to the foreground
function Set-ForegroundWindow {
    param (
        [string]$ProcessName
    )
    $app = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
    if ($app) {
        Add-Type @"
            using System;
            using System.Runtime.InteropServices;
            public class WinAPI {
                [DllImport("user32.dll")]
                public static extern bool SetForegroundWindow(IntPtr hWnd);
            }
"@
        [WinAPI]::SetForegroundWindow($app.MainWindowHandle)
    }
}

# Ensure parameters are not empty
if (-not $AppName -or -not $ExtractPath) {
    Write-Host "Error: Missing parameters. Usage: update.ps1 -AppName 'YourApp.exe' -ExtractPath 'C:\Path\To\App'"
    exit 1
}

# Define the update zip path
$UpdateFile = "$ExtractPath\update.zip"

# Close existing application if running
$runningApp = Get-Process -Name ($AppName -replace "\.exe$") -ErrorAction SilentlyContinue
if ($runningApp) {
    Stop-Process -Name ($AppName -replace "\.exe$") -Force
    Start-Sleep -Seconds 2
}

# Extract the update
Expand-Archive -Path $UpdateFile -DestinationPath $ExtractPath -Force

# Start the updated application
Start-Process -FilePath "$ExtractPath\$AppName"

# Wait for the application to start
Start-Sleep -Seconds 2

# Bring the app to the foreground
Set-ForegroundWindow -ProcessName ($AppName -replace "\.exe$")
