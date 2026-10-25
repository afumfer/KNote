# KNote Local — installation

KNote Local is KNote's web app running entirely inside your computer, without internet, on **Windows, macOS and Linux**.
This folder is its installation package. The complete guide (what it is, how it works, the **This computer** screen...)
is in KNote's documentation: `Docs/KNoteLocal.md` (in Spanish, `README_es.md` and `Docs/KNoteLocal_es.md`).

## 1. Before installing (once)

KNote needs the **ASP.NET Core Runtime 10**, a free Microsoft component:

1. Go to <https://dotnet.microsoft.com/download/dotnet/10.0>.
2. In the **ASP.NET Core Runtime 10.0.x** block, download the installer of your system (Windows x64, macOS Arm64 or
   x64, or the one of your Linux distribution) and install it. The SDK isn't needed.
3. To check it, open a terminal and type `dotnet --list-runtimes`: a line `Microsoft.AspNetCore.App 10.0.x` must appear.

## 2. Preparing the package

- **Windows**: if the zip comes from the internet or another computer, unblock it before unzipping it: right click on
  the zip → **Properties** → check **Unblock** → **OK**. Then right click → **Extract All**.
- **Mac and Linux**: unzip it (double click or `unzip`).

You get a folder with the launcher (`KNote.Launcher`), the `server` folder and the installation scripts.

## 3. Installing

Open a terminal **in the unzipped folder** (on Windows: right click on an empty space of the folder → **Open in
Terminal**) and run your system's script:

| System | Command | Then |
|---|---|---|
| Windows | `powershell -ExecutionPolicy Bypass -File .\install-windows.ps1` | Open **KNote** from the Start menu |
| Mac | `bash install-macos.sh` | Open **KNote** from `~/Applications` (or Launchpad); you can drag it to the Dock |
| Linux | `bash install-linux.sh` | Open **KNote** from the applications menu |

Options:

- `-StartAtSignIn` (Windows) or `--start-at-login` (Mac, Linux): the local server starts by itself when you sign in, so
  KNote opens at once.
- `-AppWindow` (Windows): the Start menu shortcut opens KNote in an app window, without address bar.

The programs are installed in your user profile (no need to be an administrator of the computer):

| System | Programs | Your data (notes, settings, logs) |
|---|---|---|
| Windows | `%LocalAppData%\Programs\KNote` | `%LocalAppData%\KNote\Server` |
| Mac | `~/Applications/KNote.app` | `~/Library/Application Support/KNote/Server` |
| Linux | `~/.local/share/knote/app` | `~/.local/share/KNote/Server` |

## 4. First use

1. Open **KNote**. The browser opens at `http://127.0.0.1:5280/KNote/` (or another port, if that one was taken).
2. **Register**: the first person who registers becomes the **administrator**.
3. From **Admin panel → This computer** you can create or link repositories (for example, the desktop app's database),
   set up the artificial intelligence providers and the e-mail account of the alarms.
4. If you want, install it as an app from the browser (the install icon of the address bar of Chrome or Edge, or
   **File → Add to Dock** in Safari).

If Windows shows **"Windows protected your PC"** (the program isn't signed), click **More info → Run anyway**. On a Mac,
if it says it can't open it, open **System Settings → Privacy & Security** and click **Open Anyway**.

## 5. Updating

Unzip the new package and install it with the same command: the server is stopped, the programs replaced and **your
notes and settings kept**.

## 6. Uninstalling

| System | Command |
|---|---|
| Windows | `powershell -ExecutionPolicy Bypass -File "$env:LOCALAPPDATA\Programs\KNote\uninstall-windows.ps1"` |
| Mac | `bash ~/Applications/KNote.app/Contents/Resources/app/uninstall-macos.sh` |
| Linux | `bash ~/.local/share/knote/app/uninstall-linux.sh` |

The programs and shortcuts are removed. Your data folder is kept in case you install it again: delete it by hand if
you no longer want it.

## If something goes wrong

| What happens | What to do |
|---|---|
| Notice that the ASP.NET Core Runtime 10 is missing | Install it (step 1) and open KNote again |
| Notice that another program uses the port | Open KNote once with another port: on Windows `& "$env:LOCALAPPDATA\Programs\KNote\KNote.Launcher.exe" --port 5300`; on Mac and Linux, `dotnet KNote.Launcher.dll --port 5300` from the programs folder. KNote remembers it |
| Nothing opens | Look at `Logs\launcher.log` and the server's logs in your data folder |
| You want to turn the local server off | **Admin panel → This computer → Stop local server**, or the launcher with `--stop` |

---

**For whoever builds the package**: in Visual Studio, right click on the **KNote.Launcher** project → **Publish** →
profile **LocalServerPackage** → **Publish** (console: `dotnet publish Launcher/KNote.Launcher.csproj
-p:PublishProfile=LocalServerPackage`). The package is left in `DeployLocalServer/out/KNote-Local` and its zip,
`KNote-Local-<version>.zip`, next to it (see `KNote.LocalServer.targets`).
