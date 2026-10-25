# KNote Local

KNote Local is the KNote web app running entirely inside your own computer: no internet needed, with your notes kept on your machine, on **Windows, macOS and Linux**. This guide explains in plain words what it is, how it works and how to use it.

## The ways to use KNote

KNote can be used in three ways.

The **desktop app** is a classic Windows program: you install it, open it and work. It keeps your notes in files on your computer or in your company's database. It only runs on Windows.

The **web app** opens in the browser (Chrome, Edge, Safari…) by typing an address. Behind it there is a **server**: a computer that is always on somewhere (an office, the internet) that keeps the notes and hands them to the browser when it asks for them. It works on any system and from anywhere, but somebody has to look after that server and, without a connection, you can't get in.

**KNote Local** brings together the best of both: the same web app, but running inside your own computer, without internet, on Windows, Mac or Linux.

## The main idea: a server that lives in your computer

Although the word "server" sounds like a big machine in a cooled room, a server is simply **a program that waits for requests and answers them**. It can run on any computer, your laptop too.

In KNote Local that server program runs on your machine quietly, without windows. Your browser talks to it as if it were far away, but the conversation never leaves your computer.

For that it uses a special address, **`127.0.0.1`**, also called **`localhost`**, which always means "this very computer". Nobody from outside can reach it: it is like an internal phone that only rings inside the house.

Besides the address, a **port** is needed. A computer can have many programs waiting for connections at the same time, and the port is the number that tells them apart, just as all the flats of a building share the street address but each one has its own door number. KNote Local uses port **5280** (or another free one, if that one is taken), so its address is something like `http://127.0.0.1:5280/KNote`.

The app you see is **exactly the same as the web version**: same screens, same buttons, same features. It isn't a copy or a cut-down version.

## The launcher: the button that starts everything

What you open from the Start menu or the desktop, with the KNote icon, is the **launcher**: a small program that does the following for you in a couple of seconds.

1. It checks whether the KNote server is already running on your computer. To find out, it asks it a very simple question, the **health check**: "are you there, and are you KNote?". If it answers yes, there is no need to start it again.
2. If it isn't running, it starts it in the background, invisibly.
3. It waits until it is ready.
4. It opens your browser straight in KNote.

The server keeps running when you close the browser, just as the music of a player keeps playing when you minimize its window; so the next time KNote appears at once. If you choose it when installing, the server starts by itself when you sign in to the computer, without opening the browser, so it is always ready.

KNote can also open in an **app window**: a browser window without address bar or tabs that looks like a normal program. And from Chrome, Edge or Safari you can **"install KNote as an app"**, with its own icon in the taskbar or in the Mac's Dock.

If something goes wrong, for instance another program is already using KNote's port, the launcher shows a simple notice that explains what happens and how to fix it (for example, by choosing another port number).

## Where your things are kept

KNote Local keeps **the program** apart from **your data**.

The program is installed in an applications folder; when there is a new version, that folder is replaced and that's all.

Your data goes to a personal folder of yours, inside your user profile:

| System | Data folder |
|---|---|
| Windows | `%LocalAppData%\KNote\Server` |
| macOS | `~/Library/Application Support/KNote/Server` |
| Linux | `~/.local/share/KNote/Server` |

Inside it there are:

- **Your databases** (`Data` folder): the files where the notes are kept. KNote uses **SQLite**, a database that fits entirely in a single file, without installing anything else. You can copy it, keep it in a backup or take it to another computer like any other file.
- **The notes' resources** (`Resources` folder): images and attached files.
- **The configuration file**: where your databases are, your artificial intelligence providers, your e-mail account, the port…
- **The encryption keys** (`Keys` folder), explained below.
- **The logs** (`Logs` folder): a diary where the program writes down what it does and the errors it finds. They help to find out what happened if something fails.

As the data is kept apart from the program, **updating or reinstalling KNote never touches your notes**. And if several people use the same computer, each with their own user account, each one has their own folder and their own KNote, without mixing them.

## The first start

The first time you open KNote Local you don't have to set anything up. The server:

- creates your data folder;
- prepares an empty database, ready to use;
- generates a **secret key** of its own for your installation. It signs your entry "pass" when you sign in, so that nobody can forge one. It doesn't come written from the factory: it is created at random on your machine;
- looks for a free port (5280, or the next one if it is taken).

Then it asks you to register. **The first person who registers becomes the administrator**, allowed to configure everything.

The new database comes with a sample administrator user, `adminKNote`, with a well-known password. In KNote Local nobody can sign in except from your computer, so the risk is small, but it is advisable to change its password (or disable it) at the start.

## Repositories: your different notebooks

A **repository** is a complete notes database, like a separate notebook: you can have one for work and another for personal things. When you sign in you choose which one to work with, and you can change whenever you want.

The administrator manages their repositories from the **This computer** screen (see below):

- **Create** a new, empty one.
- **Link** one that already exists, giving where its file is; for example, the one you use in the desktop app. Both programs can use it at the same time without damaging it: the database is prepared to be read and written by several programs at once. When linking it you also give the folder of its resources (images and attachments).
- **Link** an SQL Server database of your company, if you have one.
- **Rename** it as it is shown.
- **Unlink** one you no longer want to see. It only removes it from the list: **it never deletes the file**.

The changes apply at once, without restarting anything.

**If you link the desktop app's database**: in the web app you always sign in with a user name and a password. If in the desktop app you sign in with your Windows account, without a password, first set one there (**My account → Change password**); then you will be able to sign in with it in KNote Local.

## The "This computer" screen

In the admin panel there is a tab, **This computer**, that only appears when KNote runs in local mode and only for an administrator. In the web version of an ordinary server it doesn't appear, because configuring "the computer" makes no sense there.

From it you manage:

- **The repositories**, as explained above.
- **The artificial intelligence providers**: the services (OpenAI, Anthropic or Ollama) used by KNote's assistant, with their **API keys**. An API key is like the password one of those services gives you so that you can use it (and, often, so that it can charge you). Once saved, a key is never shown again.
- **The e-mail account** (the sending server, called **SMTP**) so that KNote can send the alarms by e-mail, and the switch that turns that sending on.
- **The port**, in case you need to change it (it applies the next time the server starts).
- A button to **stop the local server** if you want to turn it off completely.

For security, this screen only works from the computer itself.

## Your passwords and keys, under lock and key

The configuration file holds sensitive data: your installation's secret key, the API keys of the artificial intelligence services, the e-mail password and, if you use them, the passwords of SQL Server databases. Keeping them as they are, in readable text, would be like leaving the house keys under the doormat.

That's why they are kept **encrypted**. Encrypting is turning a text into something unreadable that can only be read again with a special key. The encryption keys are kept in your data folder and survive reinstalling KNote.

There is a difference between systems worth knowing:

- **On Windows**, those encryption keys are in turn protected with your own Windows account's protection: only your user, on your computer, can use them. Even if somebody copied your files to another machine, they couldn't read the secrets.
- **On Mac and Linux**, the protection depends on the permissions of your personal folder: other users of the computer can't get into it, but the keys don't have that extra layer.

## The alarms, also in local

The alarms work as in the web app. The in-app notices (*Application info*) appear in the bell at the top and, as the connection with your own computer counts as secure, the browser allows the system's notifications: you see them even when KNote isn't in front.

The e-mail alarms (*Email*) are sent by the local server itself if you set up your e-mail account and turn the switch on. It is **off by default**, because the desktop app can send them too; if you turn it on, uncheck **Process Email alarms** in the desktop app's options so that they all go out from one place.

## Security without complications

Besides the `127.0.0.1` address, which already stops anybody from outside from connecting, KNote Local has a less obvious protection. There is a trick with which a malicious web page could try to talk to programs on your own computer pretending to be them. The local server only accepts requests addressed expressly to `localhost` or `127.0.0.1` and refuses any other: like a doorman who only opens to whoever calls by the exact name of the house.

## Installing, updating and uninstalling

KNote Local needs no paid installers or third-party tools. The only prerequisites are the **.NET Runtime 10** and the **ASP.NET Core Runtime 10**, two free Microsoft components: the "engine" KNote runs on, just as some games need a particular graphics component. They are downloaded from Microsoft's official website (<https://dotnet.microsoft.com/download/dotnet/10.0>) and installed once. On **Windows**, the simplest is the **Hosting Bundle** of the ASP.NET Core Runtime block, which brings both (the ASP.NET Core Runtime's plain installer doesn't bring the .NET Runtime); on **Mac**, the **SDK 10** installer, which brings both too; on **Linux**, your distribution's package `aspnetcore-runtime-10.0`. The package's `README.md` explains it step by step.

KNote comes as a single package, the same for the three systems, with a few small installation **scripts** (a *script* is a list of instructions the computer carries out automatically, like a recipe):

- **Windows**: the script copies the program, creates the Start menu shortcut with KNote's icon and, if you want, makes it start when you sign in.
- **Mac**: it creates the `KNote.app` application, which you can drag to the Dock, and optionally starts it when you sign in.
- **Linux**: it creates the entry in the applications menu and, optionally, the automatic start.

As the program isn't digitally signed (a signature is a paid certificate that identifies the maker), Windows or Mac may show at first an "unknown publisher" warning. The scripts reduce those warnings; if one still appears, confirm that you want to open it.

**Updating** is simple: the server is stopped, the program folder is replaced by the new one and it is opened again. Your notes aren't touched. **Uninstalling** removes the program and its shortcuts; your data folder is kept, in case you install it again (delete it by hand if you no longer want it).

### The commands, step by step

Unzip the package and, from its folder, run your system's script:

| System | Install | Uninstall | Where KNote is |
|---|---|---|---|
| Windows | `powershell -ExecutionPolicy Bypass -File install-windows.ps1` | `uninstall-windows.ps1` | Start menu, **KNote** |
| Mac | `bash install-macos.sh` | `bash uninstall-macos.sh` | `~/Applications/KNote.app` (and Launchpad) |
| Linux | `bash install-linux.sh` | `bash uninstall-linux.sh` | Applications menu, **KNote** |

To start the server when you sign in, add `-StartAtSignIn` (Windows) or `--start-at-login` (Mac, Linux). On Windows, `-AppWindow` makes the Start menu shortcut open KNote in an app window. To update, install the new package the same way.

## If something goes wrong

- **Another program uses KNote's port**: the launcher tells you. Open KNote once with another port, `KNote.Launcher --port 5300` (on Mac and Linux, `dotnet KNote.Launcher.dll --port 5300` from the folder where it is installed); KNote remembers it from then on.
- **The .NET Runtime 10 or the ASP.NET Core Runtime 10 is missing**: the launcher tells you (or, if the .NET Runtime is missing, Windows says .NET must be installed to run the application); install what is said above (on Windows, the Hosting Bundle) and open KNote again.
- **Stopping the local server**: **Admin panel → This computer → Stop local server**, or `KNote.Launcher --stop`.
- **To find out what happened**: the launcher's (`launcher.log`) and the server's logs are in the `Logs` folder of your data folder.

## Living together with what already exists

**The desktop app doesn't go away.** It is still one more option and can live with KNote Local on the same computer, even using the same databases at the same time.

**The web version on an ordinary server keeps working the same.** Everything that belongs to local mode only turns on when the server starts in local mode.

## What KNote Local doesn't do (for now)

KNote Local is meant to be used **on your own computer**. Other computers of your home connecting to it isn't available: it would first need a secure connection (HTTPS), the firewall and the computer not going to sleep to be sorted out. There is also, for now, no automatic update and no signed installers.

---

In short: **KNote Local is the same web app you already know, running entirely inside your computer.** A launcher starts it with one click, your notes are kept in your personal folder, your keys are encrypted and a screen lets you organize your notebooks and your configuration, all without needing the internet.
