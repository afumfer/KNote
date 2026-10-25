# Installs KNote Local for the current Windows user, from the unzipped package (this script's folder):
#
#   powershell -ExecutionPolicy Bypass -File install-windows.ps1 [-StartAtSignIn] [-AppWindow]
#
# -StartAtSignIn  the local server starts when you sign in to Windows (without opening the browser).
# -AppWindow      the Start menu shortcut opens KNote in an app window (Edge or Chrome without address bar).
# -InstallPath / -ShortcutsFolder / -StartupFolder: other places (by default the user's programs folder, Start menu and
#                 startup folder).
#
# The programs go to %LocalAppData%\Programs\KNote; your notes and settings stay in %LocalAppData%\KNote\Server and are
# never touched (installing again is updating). Needs the .NET Runtime 10 and the ASP.NET Core Runtime 10: on Windows,
# the ASP.NET Core Hosting Bundle has both (the ASP.NET Core Runtime's installer alone doesn't bring the .NET Runtime).

param(
    [switch]$StartAtSignIn,
    [switch]$AppWindow,
    [string]$InstallPath = (Join-Path $env:LOCALAPPDATA 'Programs\KNote'),
    [string]$ShortcutsFolder = [Environment]::GetFolderPath('Programs'),
    [string]$StartupFolder = [Environment]::GetFolderPath('Startup')
)

$ErrorActionPreference = 'Stop'
$source = $PSScriptRoot
$launcher = Join-Path $InstallPath 'KNote.Launcher.exe'

if (-not (Test-Path (Join-Path $source 'server\KNote.Server.dll'))) {
    throw "Run this script from the unzipped KNote-Local folder (server\KNote.Server.dll not found in $source)."
}

# The runtimes KNote runs on.
$runtimes = try { & dotnet --list-runtimes 2>$null } catch { $null }
if (-not ($runtimes -match '^Microsoft\.NETCore\.App 10\.') -or -not ($runtimes -match '^Microsoft\.AspNetCore\.App 10\.')) {
    Write-Warning 'KNote needs the .NET Runtime 10 and the ASP.NET Core Runtime 10: install the ASP.NET Core Hosting Bundle (it has both) from https://dotnet.microsoft.com/download/dotnet/10.0 before opening KNote.'
}

# Updating: the running server is stopped first, then the programs are replaced.
if (Test-Path $launcher) {
    Write-Host 'Stopping KNote...'
    Start-Process -FilePath $launcher -ArgumentList '--stop' -Wait -WindowStyle Hidden
    Remove-Item -Recurse -Force $InstallPath
}

Write-Host "Copying KNote to $InstallPath..."
New-Item -ItemType Directory -Force $InstallPath | Out-Null
Copy-Item -Path (Join-Path $source '*') -Destination $InstallPath -Recurse -Force
# Downloaded from the internet: without this, Windows asks before running each program.
Get-ChildItem $InstallPath -Recurse -File | Unblock-File

$shell = New-Object -ComObject WScript.Shell
function New-Shortcut([string]$path, [string]$arguments) {
    $shortcut = $shell.CreateShortcut($path)
    $shortcut.TargetPath = $launcher
    $shortcut.Arguments = $arguments
    $shortcut.WorkingDirectory = $InstallPath
    $shortcut.IconLocation = Join-Path $InstallPath 'KNote.ico'
    $shortcut.Description = 'KNote Local'
    $shortcut.Save()
}

New-Item -ItemType Directory -Force $ShortcutsFolder | Out-Null
New-Shortcut (Join-Path $ShortcutsFolder 'KNote.lnk') $(if ($AppWindow) { '--app-window' } else { '' })
Write-Host 'Start menu shortcut created: KNote.'

$startup = Join-Path $StartupFolder 'KNote.lnk'
if ($StartAtSignIn) {
    New-Item -ItemType Directory -Force $StartupFolder | Out-Null
    New-Shortcut $startup '--no-browser'
    Write-Host 'KNote will start when you sign in to Windows.'
} elseif (Test-Path $startup) {
    Remove-Item -Force $startup
}

Write-Host 'KNote Local is installed. Open it from the Start menu (KNote).'
