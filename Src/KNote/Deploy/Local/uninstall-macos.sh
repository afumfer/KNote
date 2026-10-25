#!/bin/bash
# Uninstalls KNote Local for the current macOS user: stops the local server and removes KNote.app and its start at login.
# Your notes and settings (~/Library/Application Support/KNote/Server) are kept: delete that folder by hand if you no
# longer want them.
#
#   bash uninstall-macos.sh
set -euo pipefail

APP="$HOME/Applications/KNote.app"
AGENT="$HOME/Library/LaunchAgents/io.github.afumfer.knote.plist"

if [ -x "$APP/Contents/MacOS/KNote" ]; then
    echo "Stopping KNote..."
    "$APP/Contents/MacOS/KNote" --stop || true
fi
rm -f "$AGENT"
rm -rf "$APP"

echo "KNote Local is uninstalled. Your notes and settings are still in ~/Library/Application Support/KNote/Server."
