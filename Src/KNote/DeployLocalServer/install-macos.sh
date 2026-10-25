#!/bin/bash
# Installs KNote Local for the current macOS user, from the unzipped package (this script's folder):
#
#   bash install-macos.sh [--start-at-login]
#
# Creates ~/Applications/KNote.app (drag it to the Dock if you want). Its only native program is dotnet, signed by
# Microsoft: the app runs "dotnet KNote.Launcher.dll". --start-at-login starts the local server when you sign in
# (without opening the browser). Your notes and settings stay in ~/Library/Application Support/KNote/Server and are never
# touched (installing again is updating). Needs the .NET Runtime 10 and the ASP.NET Core Runtime 10: on macOS the
# ASP.NET Core Runtime has no installer, the .NET 10 SDK's installer has both.
set -euo pipefail

SOURCE="$(cd "$(dirname "$0")" && pwd)"
APP="$HOME/Applications/KNote.app"
RESOURCES="$APP/Contents/Resources"
AGENT="$HOME/Library/LaunchAgents/io.github.afumfer.knote.plist"
START_AT_LOGIN=false
[ "${1:-}" = "--start-at-login" ] && START_AT_LOGIN=true

if [ ! -f "$SOURCE/server/KNote.Server.dll" ]; then
    echo "Run this script from the unzipped KNote-Local folder (server/KNote.Server.dll not found in $SOURCE)." >&2
    exit 1
fi

find_dotnet() {
    for candidate in "$(command -v dotnet 2>/dev/null || true)" /usr/local/share/dotnet/dotnet /opt/homebrew/opt/dotnet/libexec/dotnet /usr/local/bin/dotnet "$HOME/.dotnet/dotnet"; do
        [ -n "$candidate" ] && [ -x "$candidate" ] && { echo "$candidate"; return; }
    done
}
DOTNET="$(find_dotnet)"
if [ -z "$DOTNET" ] || ! "$DOTNET" --list-runtimes | grep -q '^Microsoft.NETCore.App 10\.' || ! "$DOTNET" --list-runtimes | grep -q '^Microsoft.AspNetCore.App 10\.'; then
    echo "Warning: KNote needs the .NET Runtime 10 and the ASP.NET Core Runtime 10: install the .NET 10 SDK (it has both) from https://dotnet.microsoft.com/download/dotnet/10.0 before opening KNote." >&2
fi

# Updating: the running server is stopped first, then the programs are replaced.
if [ -f "$RESOURCES/app/KNote.Launcher.dll" ] && [ -n "$DOTNET" ]; then
    echo "Stopping KNote..."
    "$DOTNET" "$RESOURCES/app/KNote.Launcher.dll" --stop || true
fi
rm -rf "$APP"

echo "Creating $APP..."
mkdir -p "$APP/Contents/MacOS" "$RESOURCES/app"
cp -R "$SOURCE/." "$RESOURCES/app/"

cat > "$APP/Contents/MacOS/KNote" <<'SCRIPT'
#!/bin/bash
# KNote.app: runs KNote's launcher with dotnet (an app opened from Finder doesn't get the shell's PATH).
APP_DIR="$(cd "$(dirname "$0")/../Resources/app" && pwd)"
for DOTNET in "$(command -v dotnet 2>/dev/null)" /usr/local/share/dotnet/dotnet /opt/homebrew/opt/dotnet/libexec/dotnet /usr/local/bin/dotnet "$HOME/.dotnet/dotnet"; do
    [ -n "$DOTNET" ] && [ -x "$DOTNET" ] && exec "$DOTNET" "$APP_DIR/KNote.Launcher.dll" "$@"
done
osascript -e 'display alert "KNote" message "KNote needs the .NET Runtime 10 and the ASP.NET Core Runtime 10: install the .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0" as warning'
exit 1
SCRIPT
chmod +x "$APP/Contents/MacOS/KNote"

# LSUIElement: the launcher ends in a moment, so it doesn't show in the Dock.
cat > "$APP/Contents/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key><string>KNote</string>
    <key>CFBundleDisplayName</key><string>KNote</string>
    <key>CFBundleIdentifier</key><string>io.github.afumfer.knote</string>
    <key>CFBundleExecutable</key><string>KNote</string>
    <key>CFBundleIconFile</key><string>KNote</string>
    <key>CFBundlePackageType</key><string>APPL</string>
    <key>CFBundleShortVersionString</key><string>1.0</string>
    <key>LSUIElement</key><true/>
</dict>
</plist>
PLIST

# The icon, from the package's PNG with the tools of macOS.
ICONSET="$(mktemp -d)/KNote.iconset"
mkdir -p "$ICONSET"
for size in 16 32 128 256 512; do
    sips -z $size $size "$SOURCE/icon-512.png" --out "$ICONSET/icon_${size}x${size}.png" >/dev/null
    double=$((size * 2))
    [ $double -le 512 ] && sips -z $double $double "$SOURCE/icon-512.png" --out "$ICONSET/icon_${size}x${size}@2x.png" >/dev/null
done
iconutil -c icns "$ICONSET" -o "$RESOURCES/KNote.icns" || echo "The icon could not be created (the app works without it)."

# Downloaded from the internet: without this, Gatekeeper refuses to open it.
xattr -dr com.apple.quarantine "$APP" 2>/dev/null || true

if $START_AT_LOGIN; then
    mkdir -p "$(dirname "$AGENT")"
    cat > "$AGENT" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key><string>io.github.afumfer.knote</string>
    <key>ProgramArguments</key>
    <array>
        <string>$APP/Contents/MacOS/KNote</string>
        <string>--no-browser</string>
    </array>
    <key>RunAtLoad</key><true/>
</dict>
</plist>
PLIST
    echo "KNote will start when you sign in."
else
    rm -f "$AGENT"
fi

echo "KNote Local is installed: open KNote from ~/Applications (or Launchpad)."
