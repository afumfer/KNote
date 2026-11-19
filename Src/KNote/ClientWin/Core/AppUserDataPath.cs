namespace KNote.ClientWin.Core;

/// <summary>
/// Per-user, per-machine folder where ClientWin stores its config, default database/resources and logs
/// (%LocalAppData%\KNote), instead of next to the application binaries. KNOTE_USER_DATA_PATH, read once, puts it
/// elsewhere: ClientWin.Tests sets it to a folder of its own, so a Store.SaveConfig() reached by a test never
/// overwrites the user's real configuration.
/// </summary>
public static class AppUserDataPath
{
    public const string OverrideVariable = "KNOTE_USER_DATA_PATH";

    public static string Directory { get; } = Environment.GetEnvironmentVariable(OverrideVariable) is { Length: > 0 } folder
        ? folder
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KNote");

    public static string ConfigFile => Path.Combine(Directory, "KNoteData.config");

    public static void EnsureExists()
    {
        if (!System.IO.Directory.Exists(Directory))
            System.IO.Directory.CreateDirectory(Directory);
    }
}
