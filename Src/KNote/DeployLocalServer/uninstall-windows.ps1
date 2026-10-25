# Uninstalls KNote Local for the current Windows user: stops the local server and removes the programs and the
# shortcuts. Your notes and settings (%LocalAppData%\KNote\Server) are kept: delete that folder by hand if you no
# longer want them.
#
#   powershell -ExecutionPolicy Bypass -File uninstall-windows.ps1

param(
    [string]$InstallPath = (Join-Path $env:LOCALAPPDATA 'Programs\KNote'),
    [string]$ShortcutsFolder = [Environment]::GetFolderPath('Programs'),
    [string]$StartupFolder = [Environment]::GetFolderPath('Startup')
)

$ErrorActionPreference = 'Stop'
$launcher = Join-Path $InstallPath 'KNote.Launcher.exe'

if (Test-Path $launcher) {
    Write-Host 'Stopping KNote...'
    Start-Process -FilePath $launcher -ArgumentList '--stop' -Wait -WindowStyle Hidden
}

foreach ($shortcut in (Join-Path $ShortcutsFolder 'KNote.lnk'), (Join-Path $StartupFolder 'KNote.lnk')) {
    if (Test-Path $shortcut) { Remove-Item -Force $shortcut }
}

# This script may be running from the folder it removes: it is removed once the script ends.
if (Test-Path $InstallPath) {
    if ($PSScriptRoot -and $PSScriptRoot.StartsWith($InstallPath, [StringComparison]::OrdinalIgnoreCase)) {
        Start-Process -FilePath 'cmd.exe' -ArgumentList "/c timeout /t 2 >nul & rmdir /s /q `"$InstallPath`"" -WindowStyle Hidden
    } else {
        Remove-Item -Recurse -Force $InstallPath
    }
}

Write-Host "KNote Local is uninstalled. Your notes and settings are still in $(Join-Path $env:LOCALAPPDATA 'KNote\Server')."
