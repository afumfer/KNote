#!/bin/bash
# Uninstalls KNote Local for the current Linux user: stops the local server and removes the programs, the menu entry and
# the start at login. Your notes and settings (~/.local/share/KNote/Server) are kept: delete that folder by hand if you
# no longer want them.
#
#   bash uninstall-linux.sh
set -euo pipefail

APP_DIR="$HOME/.local/share/knote/app"

if [ -x "$APP_DIR/knote" ]; then
    echo "Stopping KNote..."
    "$APP_DIR/knote" --stop || true
fi
rm -f "$HOME/.local/share/applications/knote.desktop" "$HOME/.config/autostart/knote.desktop"
rm -rf "$HOME/.local/share/knote"

echo "KNote Local is uninstalled. Your notes and settings are still in ~/.local/share/KNote/Server."
