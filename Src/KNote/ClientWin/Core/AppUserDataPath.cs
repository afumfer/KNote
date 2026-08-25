namespace KNote.ClientWin.Core;

/// <summary>
/// Per-user, per-machine folder where ClientWin stores its config, default database/resources and logs
/// (%LocalAppData%\KNote), instead of next to the application binaries.
/// </summary>
public static class AppUserDataPath
{
    public static string Directory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KNote");

    public static string ConfigFile => Path.Combine(Directory, "KNoteData.config");

    public static void EnsureExists()
    {
        if (!System.IO.Directory.Exists(Directory))
            System.IO.Directory.CreateDirectory(Directory);
    }
}
