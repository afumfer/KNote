#!/bin/bash
# Installs KNote Local for the current Linux user, from the unzipped package (this script's folder):
#
#   bash install-linux.sh [--start-at-login]
#
# The programs go to ~/.local/share/knote/app, with KNote in the applications menu. --start-at-login starts the local
# server when you sign in (without opening the browser). Your notes and settings stay in ~/.local/share/KNote/Server and
# are never touched (installing again is updating). Needs the ASP.NET Core Runtime 10 (the package aspnetcore-runtime-10.0
# brings the .NET Runtime 10 with it).
set -euo pipefail

SOURCE="$(cd "$(dirname "$0")" && pwd)"
APP_DIR="$HOME/.local/share/knote/app"
MENU="$HOME/.local/share/applications/knote.desktop"
AUTOSTART="$HOME/.config/autostart/knote.desktop"
START_AT_LOGIN=false
[ "${1:-}" = "--start-at-login" ] && START_AT_LOGIN=true

if [ ! -f "$SOURCE/server/KNote.Server.dll" ]; then
    echo "Run this script from the unzipped KNote-Local folder (server/KNote.Server.dll not found in $SOURCE)." >&2
    exit 1
fi

if ! command -v dotnet >/dev/null 2>&1 || ! dotnet --list-runtimes | grep -q '^Microsoft.NETCore.App 10\.' || ! dotnet --list-runtimes | grep -q '^Microsoft.AspNetCore.App 10\.'; then
    echo "Warning: KNote needs the .NET Runtime 10 and the ASP.NET Core Runtime 10: install the package aspnetcore-runtime-10.0 (see https://dotnet.microsoft.com/download/dotnet/10.0) before opening KNote." >&2
fi

# Updating: the running server is stopped first, then the programs are replaced.
if [ -x "$APP_DIR/knote" ]; then
    echo "Stopping KNote..."
    "$APP_DIR/knote" --stop || true
fi
rm -rf "$APP_DIR"

echo "Copying KNote to $APP_DIR..."
mkdir -p "$APP_DIR"
cp -R "$SOURCE/." "$APP_DIR/"

# The launcher, run with dotnet (a menu entry may not get the shell's PATH).
cat > "$APP_DIR/knote" <<'SCRIPT'
#!/bin/bash
APP_DIR="$(cd "$(dirname "$0")" && pwd)"
for DOTNET in "$(command -v dotnet 2>/dev/null)" /usr/share/dotnet/dotnet /usr/lib/dotnet/dotnet /usr/local/bin/dotnet /snap/bin/dotnet "$HOME/.dotnet/dotnet"; do
    [ -n "$DOTNET" ] && [ -x "$DOTNET" ] && exec "$DOTNET" "$APP_DIR/KNote.Launcher.dll" "$@"
done
notify-send --urgency=critical KNote "KNote needs the .NET Runtime 10 and the ASP.NET Core Runtime 10: install the package aspnetcore-runtime-10.0" 2>/dev/null \
    || echo "KNote needs the .NET Runtime 10 and the ASP.NET Core Runtime 10: install the package aspnetcore-runtime-10.0" >&2
exit 1
SCRIPT
chmod +x "$APP_DIR/knote"

desktop_entry() {
    cat <<ENTRY
[Desktop Entry]
Type=Application
Name=KNote
Comment=KNote Local: your notes and tasks
Exec="$APP_DIR/knote" $1
Icon=$APP_DIR/icon-512.png
Terminal=false
Categories=Office;
ENTRY
}

mkdir -p "$(dirname "$MENU")"
desktop_entry "" > "$MENU"
command -v update-desktop-database >/dev/null 2>&1 && update-desktop-database "$(dirname "$MENU")" || true

if $START_AT_LOGIN; then
    mkdir -p "$(dirname "$AUTOSTART")"
    { desktop_entry "--no-browser"; echo "X-GNOME-Autostart-enabled=true"; } > "$AUTOSTART"
    echo "KNote will start when you sign in."
else
    rm -f "$AUTOSTART"
fi

echo "KNote Local is installed: open KNote from the applications menu."
