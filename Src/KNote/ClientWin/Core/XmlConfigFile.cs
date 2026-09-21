using System.Xml.Serialization;

namespace KNote.ClientWin.Core;

/// <summary>
/// Reads and writes an XML-serialized config document (KNoteData.config and, in the restructured
/// layout, its sibling state file). Saving is atomic (temp file + replace, keeping the previous
/// version as "&lt;file&gt;.bak") so an interrupted write can't leave a truncated file, and loading
/// falls back to that backup when the main file is unreadable.
/// </summary>
public static class XmlConfigFile
{
    public const string BackupExtension = ".bak";
    private const string TempExtension = ".tmp";

    public static void Save<T>(T config, string file)
    {
        var tempFile = file + TempExtension;

        using (var writer = new StreamWriter(tempFile))
        {
            new XmlSerializer(typeof(T)).Serialize(writer, config);
        }

        if (File.Exists(file))
            File.Replace(tempFile, file, file + BackupExtension);
        else
            File.Move(tempFile, file);
    }

    /// <summary>
    /// Returns null when there is no file. If the file is unreadable, the backup left by the last
    /// Save is tried (recoveredFromBackup = true); when that fails too, the original error is thrown.
    /// </summary>
    public static T Load<T>(string file, out bool recoveredFromBackup) where T : class
    {
        recoveredFromBackup = false;

        if (!File.Exists(file))
            return null;

        try
        {
            return Deserialize<T>(file);
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException)
        {
            var backupFile = file + BackupExtension;
            if (!File.Exists(backupFile))
                throw;

            try
            {
                var config = Deserialize<T>(backupFile);
                recoveredFromBackup = true;
                return config;
            }
            catch (Exception ex2) when (ex2 is InvalidOperationException or IOException)
            {
                throw ex;
            }
        }
    }

    private static T Deserialize<T>(string file)
    {
        using var reader = new StreamReader(file);
        return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
    }
}
