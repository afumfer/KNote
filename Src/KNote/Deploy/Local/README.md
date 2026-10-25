# KNote Local

KNote's web app running entirely in your computer, without internet. The complete guide is in KNote's documentation
(`Docs/KNoteLocal.md`, in Spanish `Docs/KNoteLocal_es.md`).

## Before installing

Install the **ASP.NET Core Runtime 10**, a free Microsoft component, from
<https://dotnet.microsoft.com/download/dotnet/10.0> (once).

## Install

Unzip the package and, from its folder:

| System | Command | Then |
|---|---|---|
| Windows | `powershell -ExecutionPolicy Bypass -File install-windows.ps1` | Open **KNote** from the Start menu |
| macOS | `bash install-macos.sh` | Open **KNote** from `~/Applications` (or Launchpad) |
| Linux | `bash install-linux.sh` | Open **KNote** from the applications menu |

Options: `-StartAtSignIn` (Windows) or `--start-at-login` (macOS, Linux) starts the local server when you sign in;
`-AppWindow` (Windows) opens KNote in an app window.

## Update

Install the new package the same way: the server is stopped, the programs replaced, and your notes and settings kept.

## Uninstall

`uninstall-windows.ps1`, `uninstall-macos.sh` or `uninstall-linux.sh`. Your data folder is kept (delete it by hand if you
no longer want it):

| System | Data folder |
|---|---|
| Windows | `%LocalAppData%\KNote\Server` |
| macOS | `~/Library/Application Support/KNote/Server` |
| Linux | `~/.local/share/KNote/Server` |

## If something goes wrong

- **The port is in use**: start KNote with another one, `KNote.Launcher --port 5300` (on macOS and Linux,
  `dotnet KNote.Launcher.dll --port 5300` from the installed folder).
- **Stop the local server**: `KNote.Launcher --stop`, or **Admin panel → This computer → Stop local server**.
- The launcher's and the server's logs are in the data folder's `Logs`.
