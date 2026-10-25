namespace KNote.Launcher;

/// <summary>
/// The launcher's arguments:
/// --no-browser    starts the server without opening the browser (when the user signs in to the computer);
/// --app-window    opens KNote in an app window (Edge or Chrome without address bar) instead of the default browser;
/// --stop          stops the local server (the installer and the updates use it);
/// --port N        the port of the local server from now on (when another program uses the current one);
/// --data PATH     another data folder (by default the user's, as the server's);
/// --server PATH   the server to start (by default server/KNote.Server.dll next to the launcher).
/// Error: why they aren't valid (null when they are).
/// </summary>
public sealed record LauncherOptions(bool NoBrowser, bool AppWindow, bool Stop, int? Port, string? DataPath, string? ServerPath, string? Error)
{
    public const string Usage = "Usage: KNote.Launcher [--no-browser] [--app-window] [--stop] [--port N] [--data PATH] [--server PATH]";

    public static LauncherOptions Parse(string[] args)
    {
        bool noBrowser = false, appWindow = false, stop = false;
        int? port = null;
        string? dataPath = null, serverPath = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--no-browser":
                    noBrowser = true;
                    break;
                case "--app-window":
                    appWindow = true;
                    break;
                case "--stop":
                    stop = true;
                    break;
                case "--port":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var value) && value is >= 1024 and <= 65535)
                        port = value;
                    else
                        return Invalid("--port needs a number between 1024 and 65535.");
                    break;
                case "--data":
                    if (i + 1 >= args.Length)
                        return Invalid("--data needs a folder.");
                    dataPath = args[++i];
                    break;
                case "--server":
                    if (i + 1 >= args.Length)
                        return Invalid("--server needs the path of KNote.Server.dll.");
                    serverPath = args[++i];
                    break;
                default:
                    return Invalid($"Unknown argument: {args[i]}.");
            }
        }
        return new LauncherOptions(noBrowser, appWindow, stop, port, dataPath, serverPath, null);
    }

    private static LauncherOptions Invalid(string error) => new(false, false, false, null, null, null, $"{error} {Usage}");
}
