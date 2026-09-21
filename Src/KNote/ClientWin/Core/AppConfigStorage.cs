using System.Xml;
using System.Xml.Serialization;
using KNote.Model;

namespace KNote.ClientWin.Core;

/// <summary>What <see cref="AppConfigStorage.Load"/> found, plus messages the user should be told about.</summary>
public sealed class AppConfigLoadResult
{
    public AppUserSettings Settings { get; init; }
    public AppUserState State { get; init; }
    public IReadOnlyList<string> Notices { get; init; }
}

/// <summary>
/// Reads and writes the two configuration files next to each other in the user data folder:
/// KNoteData.config (what the user configures, secrets encrypted) and KNoteState.config (what the
/// application remembers by itself). A KNoteData.config in the old flat format (root &lt;AppConfig&gt;) is
/// migrated on load: the two files are rewritten in the new format and a copy of the old one, with every
/// secret emptied, is kept as "KNoteData.config.v1.bak".
/// </summary>
public sealed class AppConfigStorage
{
    public const string StateFileName = "KNoteState.config";
    public const string PreMigrationBackupSuffix = ".v1.bak";

    private const string V1Root = "AppConfig";
    private const string V2SettingsRoot = "AppUserSettings";

    private readonly AppUserSettingsSecrets _secrets;

    // The settings exactly as last read from / written to disk (plain text, serialized). The settings file
    // is only rewritten when they differ, so day-to-day use (which only changes the state) never touches
    // the file that holds the secrets. Null forces the next Save to write it.
    private string _savedSettingsXml;

    public AppConfigStorage(string settingsFile, ISecretProtector protector)
    {
        SettingsFile = settingsFile ?? throw new ArgumentNullException(nameof(settingsFile));
        StateFile = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(settingsFile)), StateFileName);
        _secrets = new AppUserSettingsSecrets(protector);
    }

    public string SettingsFile { get; }

    public string StateFile { get; }

    public string PreMigrationBackupFile => SettingsFile + PreMigrationBackupSuffix;

    /// <summary>Returns null when there is no configuration yet (first run).</summary>
    public AppConfigLoadResult Load()
    {
        if (!File.Exists(SettingsFile))
            return null;

        var notices = new List<string>();
        AppUserSettings settings;
        AppUserState state;

        var root = ReadRootElementName();
        if (root == V1Root)
            (settings, state) = MigrateFromV1(notices);
        else if (root == V2SettingsRoot)
        {
            settings = LoadSettings(notices);
            state = LoadState(notices);
        }
        else
            throw new InvalidOperationException(
                $"'{SettingsFile}' is not a KNote configuration file (unexpected root element '{root}').");

        return new AppConfigLoadResult { Settings = settings, State = state, Notices = notices };
    }

    public void Save(AppUserSettings settings, AppUserState state)
    {
        XmlConfigFile.Save(state, StateFile);

        var settingsXml = Serialize(settings);
        if (settingsXml != _savedSettingsXml)
        {
            XmlConfigFile.Save(_secrets.Protect(settings), SettingsFile);
            _savedSettingsXml = settingsXml;
        }
    }

    #region Loading

    private AppUserSettings LoadSettings(List<string> notices)
    {
        var settings = XmlConfigFile.Load<AppUserSettings>(SettingsFile, out var recoveredFromBackup);
        if (recoveredFromBackup)
            notices.Add($"'{Path.GetFileName(SettingsFile)}' could not be read, so its last backup was loaded instead.");

        var read = _secrets.Unprotect(settings);
        if (read.Undecryptable.Count > 0)
            notices.Add("These secrets could not be decrypted (they were saved by another Windows user or on "
                + "another computer) and must be entered again:" + Environment.NewLine
                + string.Join(Environment.NewLine, read.Undecryptable.Select(s => "  - " + s)));

        // A plain-text secret (hand-edited file) or a file recovered from its backup is rewritten on the next save.
        _savedSettingsXml = read.FoundPlainText || recoveredFromBackup ? null : Serialize(settings);
        return settings;
    }

    private AppUserState LoadState(List<string> notices)
    {
        try
        {
            return XmlConfigFile.Load<AppUserState>(StateFile, out _) ?? new AppUserState();
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException)
        {
            // Only remembered window positions and the like: never worth failing the startup.
            notices.Add($"'{StateFileName}' could not be read; the window positions and other remembered state were reset.");
            return new AppUserState();
        }
    }

    // First element of the settings file (or, if that is unreadable, of its backup) tells the format apart.
    private string ReadRootElementName()
    {
        foreach (var candidate in new[] { SettingsFile, SettingsFile + XmlConfigFile.BackupExtension })
        {
            if (!File.Exists(candidate))
                continue;

            try
            {
                using var reader = XmlReader.Create(candidate, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
                reader.MoveToContent();
                return reader.LocalName;
            }
            catch (Exception ex) when (ex is XmlException or IOException)
            {
            }
        }

        throw new InvalidOperationException($"'{SettingsFile}' cannot be read and has no readable backup.");
    }

    #endregion

    #region Migration from the flat V1 file

    // Ordered so that being interrupted at any point just means doing it again on the next start: the old
    // file is only replaced (atomically) by the last step, and its plain-text content is never copied to a
    // "<file>.bak" - the only copy kept is the one with the secrets emptied.
    private (AppUserSettings, AppUserState) MigrateFromV1(List<string> notices)
    {
        var v1 = XmlConfigFile.Load<AppConfigV1>(SettingsFile, out var recoveredFromBackup);
        if (recoveredFromBackup)
            notices.Add($"'{Path.GetFileName(SettingsFile)}' could not be read, so its last backup was loaded instead.");

        if (!File.Exists(PreMigrationBackupFile))
            XmlConfigFile.Save(AppConfigMigrator.WithoutSecrets(v1), PreMigrationBackupFile, keepBackup: false);

        var (settings, state) = AppConfigMigrator.FromV1(v1);

        XmlConfigFile.Save(state, StateFile);
        XmlConfigFile.Save(_secrets.Protect(settings), SettingsFile, keepBackup: false);
        DeleteBackupHoldingOldSecrets();
        _savedSettingsXml = Serialize(settings);

        notices.Add("The configuration was upgraded to the new format (KNoteData.config for settings, "
            + $"{StateFileName} for window state). A copy of the previous file, without passwords or API keys, "
            + $"was kept as '{Path.GetFileName(PreMigrationBackupFile)}'.");

        return (settings, state);
    }

    // Earlier builds left "KNoteData.config.bak" holding the old file, secrets in plain text. Anything that
    // is not a readable new-format settings file is removed.
    private void DeleteBackupHoldingOldSecrets()
    {
        var backup = SettingsFile + XmlConfigFile.BackupExtension;
        if (!File.Exists(backup))
            return;

        try
        {
            using (var reader = XmlReader.Create(backup, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit }))
            {
                reader.MoveToContent();
                if (reader.LocalName == V2SettingsRoot)
                    return;
            }
        }
        catch (Exception ex) when (ex is XmlException or IOException)
        {
        }

        try
        {
            File.Delete(backup);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Non-fatal: the next migration/save attempt leaves it in the same state.
        }
    }

    #endregion

    private static string Serialize(AppUserSettings settings)
    {
        using var writer = new StringWriter();
        new XmlSerializer(typeof(AppUserSettings)).Serialize(writer, settings);
        return writer.ToString();
    }
}
