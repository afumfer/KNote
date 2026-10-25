# KNote Local's package: the launcher and the server published framework-dependent (they need the ASP.NET Core Runtime
# 10), the same for Windows, macOS and Linux, with the installation scripts. Run it from any folder (PowerShell on
# Windows, or pwsh on macOS and Linux):
#
#   ./publish-local.ps1 [-Output <folder>] [-Configuration Release]
#
# Result: <Output>/KNote-Local/ (the package) and <Output>/KNote-Local-<version>.zip.
#   KNote-Local/
#     KNote.Launcher(.exe)   the launcher (the .exe only runs on Windows; macOS and Linux run the .dll with dotnet)
#     server/                the local server, with the web app (wwwroot)
#     install-*, uninstall-* the installation scripts of each system
#     KNote.ico, icon-512.png, README.md

param(
    [string]$Output = (Join-Path $PSScriptRoot 'out'),
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$sources = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$package = Join-Path $Output 'KNote-Local'

if (Test-Path $package) { Remove-Item -Recurse -Force $package }
New-Item -ItemType Directory -Force $package | Out-Null

Write-Host "Publishing the launcher..."
dotnet publish (Join-Path $sources 'Launcher\KNote.Launcher.csproj') -c $Configuration -o $package --nologo
if ($LASTEXITCODE -ne 0) { throw 'The launcher could not be published.' }

Write-Host "Publishing the server..."
dotnet publish (Join-Path $sources 'Server\KNote.Server.csproj') -c $Configuration -o (Join-Path $package 'server') --nologo
if ($LASTEXITCODE -ne 0) { throw 'The server could not be published.' }

# Only what KNote Local needs of the server's configuration: its appsettings.Local.json and the user's data folder do
# the rest. The server's own logs go to the data folder.
Remove-Item -Force -ErrorAction SilentlyContinue (Join-Path $package 'server\appsettings.Development.json')

foreach ($script in 'install-windows.ps1', 'uninstall-windows.ps1', 'install-macos.sh', 'uninstall-macos.sh', 'install-linux.sh', 'uninstall-linux.sh', 'README.md') {
    Copy-Item (Join-Path $PSScriptRoot $script) $package
}
Copy-Item (Join-Path $sources 'ClientWin\Resources\Icons\KNote.ico') $package
Copy-Item (Join-Path $sources 'Client\wwwroot\icons\icon-512.png') $package

# The shell scripts keep LF line ends (bash doesn't accept CR LF).
foreach ($script in Get-ChildItem $package -Filter '*.sh') {
    $text = [IO.File]::ReadAllText($script.FullName) -replace "`r`n", "`n"
    [IO.File]::WriteAllText($script.FullName, $text, (New-Object Text.UTF8Encoding $false))
}

$version = (Get-Item (Join-Path $package 'server\KNote.Server.dll')).VersionInfo.FileVersion
$zip = Join-Path $Output "KNote-Local-$version.zip"
if (Test-Path $zip) { Remove-Item -Force $zip }
Compress-Archive -Path $package -DestinationPath $zip

Write-Host "Package: $package"
Write-Host "Zip:     $zip"
