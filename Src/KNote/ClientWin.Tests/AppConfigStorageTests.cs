using System.Xml.Serialization;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;

namespace KNote.ClientWin.Tests;

[TestClass]
public class AppConfigStorageTests
{
    private static readonly string[] FixtureSecrets = { "test-smtp-password", "test-openai-key", "test-anthropic-key" };

    private string _dir = null!;
    private string _file = null!;
    private string _stateFile = null!;
    private FakeSecretProtector _protector = null!;

    [TestInitialize]
    public void Setup()
    {
        _dir = Path.Combine(Path.GetTempPath(), "KNoteAppConfigStorageTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _file = Path.Combine(_dir, "KNoteData.config");
        _stateFile = Path.Combine(_dir, AppConfigStorage.StateFileName);
        _protector = new FakeSecretProtector();
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, recursive: true);

    private AppConfigStorage NewStorage() => new(_file, _protector);

    private void PlaceV1Fixture()
        => File.Copy(Path.Combine(AppContext.BaseDirectory, "Fixtures", "KNoteData.v1.config"), _file);

    private static string Read(string file) => File.ReadAllText(file);

    private IEnumerable<string> AllFilesContaining(string text)
        => Directory.GetFiles(_dir).Where(f => Read(f).Contains(text));

    [TestMethod]
    public void Load_NoFile_ReturnsNull()
    {
        Assert.IsNull(NewStorage().Load());
    }

    [TestMethod]
    public void Load_UnknownRootElement_Throws()
    {
        File.WriteAllText(_file, "<Something><A>1</A></Something>");

        Assert.ThrowsExactly<InvalidOperationException>(() => NewStorage().Load());
    }

    // ---- Migration from the flat V1 file ----

    [TestMethod]
    public void Load_V1File_MigratesIntoTwoFilesKeepingEveryValue()
    {
        PlaceV1Fixture();

        var result = NewStorage().Load()!;

        Assert.AreEqual("smtp.example.com", result.Settings.Notifications.Email.Host);
        Assert.AreEqual("test-smtp-password", result.Settings.Notifications.Email.Password);
        Assert.AreEqual("test-anthropic-key", result.Settings.Ai.Providers[1].ApiKey);
        Assert.AreEqual(3929, result.State.Session.RunCounter);
        Assert.AreEqual(1363, result.State.ManagementWindow.Bounds.Width);
        Assert.AreEqual(1, result.Notices.Count);
        StringAssert.Contains(result.Notices[0], "upgraded");

        StringAssert.Contains(Read(_file), "<AppUserSettings");
        StringAssert.Contains(Read(_stateFile), "<AppUserState");
        Assert.AreEqual(3929, XmlConfigFile.Load<AppUserState>(_stateFile, out _)!.Session.RunCounter);
    }

    [TestMethod]
    public void Load_V1File_LeavesNoSecretInPlainTextInAnyFile()
    {
        PlaceV1Fixture();

        NewStorage().Load();

        foreach (var secret in FixtureSecrets)
            Assert.AreEqual(0, AllFilesContaining(secret).Count(), $"'{secret}' must not remain in clear text in any file");
        Assert.IsTrue(Read(_file).Contains("fake:"), "the settings file holds the secrets encrypted");
    }

    [TestMethod]
    public void Load_V1File_KeepsABackupOfTheOldFileWithoutSecrets()
    {
        PlaceV1Fixture();
        var storage = NewStorage();

        storage.Load();

        var backup = XmlConfigFile.Load<AppConfigV1>(storage.PreMigrationBackupFile, out _)!;
        Assert.IsTrue(storage.PreMigrationBackupFile.EndsWith("KNoteData.config.v1.bak"));
        Assert.IsTrue(string.IsNullOrEmpty(backup.SmtpPassword));
        Assert.IsTrue(backup.AiProviderRefs.All(p => string.IsNullOrEmpty(p.ApiKey)));
        // ...but everything else is there, so it is still a usable old-format config.
        Assert.AreEqual("smtp.example.com", backup.SmtpHost);
        Assert.AreEqual(3, backup.AiProviderRefs.Count);
        Assert.AreEqual(3929, backup.RunCounter);
    }

    [TestMethod]
    public void Load_V1File_DeletesAStaleBackupHoldingThePlainTextSecrets()
    {
        // Builds from earlier phases of the restructuring left the old file, secrets included, as .bak.
        PlaceV1Fixture();
        File.Copy(_file, _file + ".bak");

        NewStorage().Load();

        Assert.IsFalse(File.Exists(_file + ".bak"));
    }

    [TestMethod]
    public void Load_V1File_KeepsAnExistingNewFormatBackup()
    {
        PlaceV1Fixture();
        XmlConfigFile.Save(new AppUserSettings(), _file + ".bak");

        NewStorage().Load();

        Assert.IsTrue(File.Exists(_file + ".bak"));
    }

    [TestMethod]
    public void Load_AfterMigration_ReadsTheNewFormatWithNothingToReport()
    {
        PlaceV1Fixture();
        NewStorage().Load();
        var firstBackupWrite = File.GetLastWriteTimeUtc(_file + ".v1.bak");

        var second = NewStorage().Load()!;

        Assert.AreEqual(0, second.Notices.Count);
        Assert.AreEqual("test-smtp-password", second.Settings.Notifications.Email.Password);
        Assert.AreEqual(3929, second.State.Session.RunCounter);
        Assert.AreEqual(firstBackupWrite, File.GetLastWriteTimeUtc(_file + ".v1.bak"));
    }

    [TestMethod]
    public void Load_MigrationInterruptedAfterWritingTheState_IsSimplyRedone()
    {
        PlaceV1Fixture();
        File.WriteAllText(_stateFile, "<AppUserState schemaVersion=\"2\"><Session><RunCounter>1</RunCounter></Session></AppUserState>");

        var result = NewStorage().Load()!;

        Assert.AreEqual(3929, result.State.Session.RunCounter);
        Assert.AreEqual("test-smtp-password", result.Settings.Notifications.Email.Password);
    }

    [TestMethod]
    public void Load_CorruptV1FileWithABackup_RecoversAndMigratesFromTheBackup()
    {
        PlaceV1Fixture();
        File.Copy(_file, _file + ".bak");
        File.WriteAllText(_file, "<AppConfig><RunCounter>");

        var result = NewStorage().Load()!;

        Assert.AreEqual(3929, result.State.Session.RunCounter);
        Assert.IsTrue(result.Notices.Any(n => n.Contains("backup")));
    }

    // ---- Saving and loading the new format ----

    private static (AppUserSettings, AppUserState) SampleConfig()
    {
        var settings = new AppUserSettings();
        settings.Notifications.Email.Host = "smtp.example.com";
        settings.Notifications.Email.Password = "smtp-pwd";
        settings.Ai.Providers.Add(new AiProviderRef { Alias = "OpenAI", Provider = "OpenAI", Model = "m", ApiKey = "key-1" });
        settings.Repositories.Items.Add(new RepositoryRef { Alias = "Sql", ConnectionString = "Server=db;User Id=sa;Password=s3cr3t" });
        var state = new AppUserState();
        state.Session.RunCounter = 7;
        state.ManagementWindow.Bounds.X = 100;
        return (settings, state);
    }

    [TestMethod]
    public void SaveThenLoad_RoundTripsBothDocuments_WithSecretsEncryptedOnDisk()
    {
        var (settings, state) = SampleConfig();
        NewStorage().Save(settings, state);

        foreach (var secret in new[] { "smtp-pwd", "key-1", "s3cr3t" })
            Assert.AreEqual(0, AllFilesContaining(secret).Count(), $"'{secret}' must not be in clear text");

        var loaded = NewStorage().Load()!;
        Assert.AreEqual("smtp-pwd", loaded.Settings.Notifications.Email.Password);
        Assert.AreEqual("key-1", loaded.Settings.Ai.Providers[0].ApiKey);
        Assert.AreEqual("Server=db;User Id=sa;Password=s3cr3t", loaded.Settings.Repositories.Items[0].ConnectionString);
        Assert.AreEqual(7, loaded.State.Session.RunCounter);
        Assert.AreEqual(100, loaded.State.ManagementWindow.Bounds.X);
        Assert.AreEqual(0, loaded.Notices.Count);
    }

    [TestMethod]
    public void Save_WhenOnlyTheStateChanged_DoesNotRewriteTheSettingsFile()
    {
        var (settings, state) = SampleConfig();
        var storage = NewStorage();
        storage.Save(settings, state);
        File.Delete(_file + ".bak");   // a rewrite of the settings file would leave one behind

        state.Session.RunCounter = 8;
        storage.Save(settings, state);

        Assert.IsFalse(File.Exists(_file + ".bak"), "the settings file must not have been rewritten");
        Assert.IsTrue(File.Exists(_stateFile + ".bak"), "the state file is rewritten every time");
        Assert.AreEqual(8, XmlConfigFile.Load<AppUserState>(_stateFile, out _)!.Session.RunCounter);
    }

    [TestMethod]
    public void Save_WhenTheSettingsChanged_RewritesTheSettingsFile()
    {
        var (settings, state) = SampleConfig();
        var storage = NewStorage();
        storage.Save(settings, state);

        settings.General.AlarmSeconds = 99;
        storage.Save(settings, state);

        Assert.IsTrue(File.Exists(_file + ".bak"));
        Assert.AreEqual(99, NewStorage().Load()!.Settings.General.AlarmSeconds);
    }

    [TestMethod]
    public void Save_AfterALoad_DoesNotRewriteUnchangedSettings()
    {
        var (settings, state) = SampleConfig();
        NewStorage().Save(settings, state);
        File.Delete(_file + ".bak");
        var storage = NewStorage();
        var loaded = storage.Load()!;

        storage.Save(loaded.Settings, loaded.State);

        Assert.IsFalse(File.Exists(_file + ".bak"));
    }

    [TestMethod]
    public void Load_SecretThatCannotBeDecrypted_IsClearedAndReported()
    {
        var (settings, state) = SampleConfig();
        NewStorage().Save(settings, state);
        _protector.Undecryptable.Add(_protector.Protect("key-1"));

        var loaded = NewStorage().Load()!;

        Assert.IsNull(loaded.Settings.Ai.Providers[0].ApiKey);
        Assert.AreEqual("smtp-pwd", loaded.Settings.Notifications.Email.Password);
        Assert.AreEqual(1, loaded.Notices.Count);
        StringAssert.Contains(loaded.Notices[0], "OpenAI");
    }

    [TestMethod]
    public void Load_PlainTextSecretInANewFormatFile_IsEncryptedOnTheNextSave()
    {
        var (settings, state) = SampleConfig();
        XmlConfigFile.Save(settings, _file);   // hand-made: secrets in clear text
        var storage = NewStorage();
        var loaded = storage.Load()!;
        Assert.AreEqual("smtp-pwd", loaded.Settings.Notifications.Email.Password);

        storage.Save(loaded.Settings, loaded.State);

        Assert.IsFalse(Read(_file).Contains("smtp-pwd"));
        Assert.IsTrue(Read(_file).Contains("fake:"));
    }

    [TestMethod]
    public void Load_CorruptSettingsFileWithABackup_LoadsTheBackupAndRepairsTheFileOnSave()
    {
        var (settings, state) = SampleConfig();
        var storage = NewStorage();
        storage.Save(settings, state);
        settings.General.AlarmSeconds = 55;
        storage.Save(settings, state);          // now KNoteData.config.bak holds the previous version
        File.WriteAllText(_file, "<AppUserSettings><General>");

        var loadingStorage = NewStorage();
        var loaded = loadingStorage.Load()!;

        Assert.IsTrue(loaded.Notices.Any(n => n.Contains("backup")));
        loadingStorage.Save(loaded.Settings, loaded.State);
        Assert.IsNotNull(XmlConfigFile.Load<AppUserSettings>(_file, out var recovered));
        Assert.IsFalse(recovered, "the main file was repaired by the save");
    }

    [TestMethod]
    public void Load_CorruptStateFile_StartsWithDefaultStateAndReportsIt()
    {
        var (settings, state) = SampleConfig();
        NewStorage().Save(settings, state);
        File.WriteAllText(_stateFile, "not xml");

        var loaded = NewStorage().Load()!;

        Assert.AreEqual(0, loaded.State.Session.RunCounter);
        Assert.IsTrue(loaded.State.ManagementWindow.Panels.Toolbar);
        Assert.AreEqual("smtp-pwd", loaded.Settings.Notifications.Email.Password);
        Assert.AreEqual(1, loaded.Notices.Count);
        StringAssert.Contains(loaded.Notices[0], AppConfigStorage.StateFileName);
    }

    [TestMethod]
    public void Load_MissingStateFile_StartsWithDefaultStateSilently()
    {
        var (settings, state) = SampleConfig();
        NewStorage().Save(settings, state);
        File.Delete(_stateFile);
        File.Delete(_stateFile + ".bak");

        var loaded = NewStorage().Load()!;

        Assert.AreEqual(0, loaded.State.Session.RunCounter);
        Assert.AreEqual(0, loaded.Notices.Count);
    }

    [TestMethod]
    public void StateFile_IsAlwaysNextToTheSettingsFile()
    {
        Assert.AreEqual(_stateFile, NewStorage().StateFile);
    }
}
