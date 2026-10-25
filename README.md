# KNote

English | [Español](README_es.md)

KNote is a minimalist note and task manager. Tasks or notes are stored in databases and can be organized in thematic folders. These notes can contain different resources (images, attachments, ...), link various alarms and be enriched with custom attributes. KNote also integrates a minimalist language with which you can automate tasks. In addition to the task manager, you can give KNote other uses, for example a small content manager.

Two different database engines are supported, Sqlite for personal tasks and SQL server for corporate use.

KNote also has two different front-ends, an old-school Windows desktop application (WinForm app) and a Web application (Blazor app). 

Main features:

- Notes organized in a folders tree, with tags, priorities, custom attributes, tasks, alarms, resources (images, files) and trace notes (relations between notes).
- Descriptions written in HTML or Markdown, with [Mermaid](https://mermaid.js.org/) diagrams.
- Several repositories (databases) open at the same time in the desktop app, each one with its own users and roles (Guest, Staff, Project manager, Admin).
- Printable reports (notes list, book, note details) and CSV export.
- An AI assistant (OpenAI, Anthropic or Ollama) that can search your notes and create tasks, with its conversations saved as notes.
- A chat room shared by the desktop and web apps.
- KntScript, a minimalist automation language, and a script console.

The user's guide is in [Docs/Manual.md](Docs/Manual.md) (in Spanish, [Docs/Manual_es.md](Docs/Manual_es.md)).


## Compilation and use 

**Requirements**

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- Windows, for the desktop app (it is a WinForms app), with the [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (already included in Windows 11).
- To develop and debug: Visual Studio 2026 (workload *.NET desktop development* and *ASP.NET and web development*) or Visual Studio Code with the C# Dev Kit extension.

**Solutions**

The source code is in `Src/KNote`. There isn't a single `.sln`, but several `.slnx` solutions:

| Solution | Contents |
|---|---|
| `KNote.slnx` | The whole application: server, web client, desktop client and their libraries. |
| `KNoteTest.slnx` | The backend integration tests. |

**Compiling and running from the command line** (from `Src/KNote`):

```powershell
# Compile the whole application
dotnet build KNote.slnx

# Desktop app
dotnet run --project ClientWin/KNote.ClientWin.csproj

# Web server (it also serves the web app, at https://localhost:44339/KNote)
dotnet run --project Server/KNote.Server.csproj --launch-profile KNote.Server
```

**Visual Studio.** Open `Src/KNote/KNote.slnx` and choose the startup project: `KNote.ClientWin` for the desktop app, or `KNote.Server` (profile `KNote.Server`) for the web app; debugging the server also debugs the web client.

**Visual Studio Code.** Open the `Src/KNote` folder. The *Run and Debug* view has the configurations `KNote.ClientWin` (desktop app), `KNote.Web (Server + Client WASM)` (web server and web client) and `KNote.Server` (server only).

**First run of the desktop app.** The first time it starts, the desktop app creates a personal Sqlite database in `%LocalAppData%\KNote\Data` and signs you in with your Windows account, as the Admin of that repository. From the **Repository** menu you can create or link other repositories (Sqlite or SQL Server).

**Configuring the web server.** The server's settings are in `Server/appsettings.json`, which only holds placeholders. Write the real values (database, JWT secret, API keys, e-mail password...) in the server's user secrets, never in `appsettings.json`:

```powershell
dotnet user-secrets set "AppSettings:Secret" "<a long random key>" --project Server/KNote.Server.csproj
dotnet user-secrets set "RepositoryRef:Orm" "EntityFramework" --project Server/KNote.Server.csproj
dotnet user-secrets set "RepositoryRef:Provider" "Microsoft.Data.Sqlite" --project Server/KNote.Server.csproj
dotnet user-secrets set "RepositoryRef:ConnectionString" "Data Source=D:\KNoteData\knote.db" --project Server/KNote.Server.csproj
```

| Setting | Meaning |
|---|---|
| `AppSettings:Secret` | The key that signs the users' sessions (JWT). |
| `RepositoryRef:Orm` | `EntityFramework` or `Dapper`. |
| `RepositoryRef:Provider` | `Microsoft.Data.Sqlite` or `Microsoft.Data.SqlClient` (SQL Server). |
| `RepositoryRef:ConnectionString` | The connection string of the database. |
| `RepositoryRef:Name` | The name given to a new database. |
| `RepositoryRef:ResourcesContainer`, `ResourcesContainerRootPath`, `ResourcesContainerRootUrl` | Where the notes' resources (images, files) are kept and the url they are served from (empty: the server's defaults). |

The database is created, or updated to the current version, when the server starts. The first user who registers in a new database becomes its Admin; the next ones register as Guest until an Admin raises their role. The AI assistant and the password reset by e-mail have their own settings, described in the manual.

**Tests.** `dotnet test ClientWin.Tests/KNote.ClientWin.Tests.csproj` runs the desktop app's unit tests. `dotnet test KNoteTest.slnx` runs the backend tests; the ones in `Tests/WebApiIntegrationTests` need a running server (see `Tests/appsettings.json`).


## Installation and deployment 

**Desktop app.** Publish it with:

```powershell
dotnet publish ClientWin/KNote.ClientWin.csproj -c Release -o <output folder>
```

The desktop app doesn't need an installer:

1. On the computer where it will be used, install the [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) (Windows x64) and, on Windows 10, the [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/).
2. Copy the output folder anywhere (for example, `C:\Program Files\KNote` or a folder of your user) and run `KNote.ClientWin.exe`. You can create a shortcut to it on the desktop or the start menu.
3. To upgrade, close KNote and replace the folder's files with the new ones. Each Windows user keeps their configuration and personal database in `%LocalAppData%\KNote`, which is not touched, and the databases are updated automatically the first time the new version opens them (keep a copy of a shared database before).

To uninstall it, delete the folder; delete `%LocalAppData%\KNote` too if you also want to remove your configuration and your personal database.

**Web app.** Publish the server, which includes the web app:

```powershell
dotnet publish Server/KNote.Server.csproj -c Release -o <output folder>
```

The web app is always served under the path `/KNote` (e.g. `https://myserver/KNote`). To host it in IIS:

1. Install the [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/10.0) for .NET 10.
2. Copy the published folder to the server and create an IIS application with the alias `KNote` pointing to it, with an application pool with *No Managed Code*.
3. Give the application pool's identity write permission on the `Log` folder, on the folder of the Sqlite database (if you use Sqlite) and on the resources folder.
4. Configure the settings described in *Compilation and use* with environment variables (for example, `RepositoryRef__ConnectionString`, in the `environmentVariables` of `web.config` or of the application pool) or with your hosting's secret store. Don't write secrets in `appsettings.json`.

It can also run without IIS, as a standalone process: `KNote.Server.exe --urls "https://*:5001"`.

Keep a Sqlite database on a local disk of the server, not in a network folder. For a shared database with many users, SQL Server is the recommended engine.

**KNote Local.** The web app can also run entirely in your own computer, without internet, on Windows, macOS and Linux: the same server and the same app, listening only at `http://127.0.0.1`, started by a launcher. Make its package, the same for the three systems, by publishing the launcher with its profile: in Visual Studio, right click on the **KNote.Launcher** project → **Publish** → **LocalServerPackage** → **Publish**, or:

```powershell
dotnet publish Src/KNote/Launcher/KNote.Launcher.csproj -p:PublishProfile=LocalServerPackage
```

The package (and its zip) is left in `Src/KNote/DeployLocalServer/out`. Then install it with the script of each system included in the package, as its `README.md` explains (it needs the [ASP.NET Core Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0)). See [Docs/KNoteLocal.md](Docs/KNoteLocal.md) (in Spanish, [Docs/KNoteLocal_es.md](Docs/KNoteLocal_es.md)).


## AI-assisted development

Since version 0.0.10.11, this software has been developed with strong assistance from AI coding agents ([Claude Code](https://claude.com/claude-code)) and with humans leading the ideas, design, testing, and debugging. Most of the code, tests and technical documentation are generated by Claude Code under the author's direction, and every change is reviewed before it is merged. We say this openly because it shaped how the project was built.


## License

This project is under the GNU General Public License, version 2 or (at your option) any later version - see the LICENSE.txt file for details.

(This program is under construction and the documentation isn't ready yet. Thanks for your patience!)
