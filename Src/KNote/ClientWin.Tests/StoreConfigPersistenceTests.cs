using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests;

// Store-level behaviour of the configuration files (uses the real DPAPI protector, as production does).
// The details of the file handling live in AppConfigStorageTests.
[TestClass]
public class StoreConfigPersistenceTests
{
    private string _dir = null!;
    private string _file = null!;

    [TestInitialize]
    public void Setup()
    {
        _dir = Path.Combine(Path.GetTempPath(), "KNoteStoreConfigTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _file = Path.Combine(_dir, "KNoteData.config");
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, recursive: true);

    private void CopyFixtureAsCurrentConfig()
        => File.Copy(Path.Combine(AppContext.BaseDirectory, "Fixtures", "KNoteData.v1.config"), _file);

    [TestMethod]
    public void LoadConfig_OldFlatFile_IsMigratedAndPopulatesSettingsAndState()
    {
        CopyFixtureAsCurrentConfig();
        var store = new Store(factoryViews: null!);

        store.LoadConfig(_file);

        Assert.AreEqual(35, store.Settings.General.AlarmSeconds);
        Assert.AreEqual("smtp.example.com", store.Settings.Notifications.Email.Host);
        Assert.AreEqual("test-smtp-password", store.Settings.Notifications.Email.Password);
        Assert.AreEqual(2, store.Settings.Repositories.Items.Count);
        Assert.AreEqual(3929, store.State.Session.RunCounter);
        Assert.AreEqual(1363, store.State.ManagementWindow.Bounds.Width);
        Assert.IsTrue(File.Exists(Path.Combine(_dir, "KNoteState.config")));
        Assert.AreEqual(1, store.TakeConfigNotices().Count, "the user is told about the upgrade, once");
        Assert.AreEqual(0, store.TakeConfigNotices().Count);
    }

    [TestMethod]
    public void SaveConfig_WritesTwoFiles_AndReloadsWithoutLosingValues()
    {
        CopyFixtureAsCurrentConfig();
        var store = new Store(factoryViews: null!);
        store.LoadConfig(_file);
        store.State.Session.RunCounter += 1;
        store.State.ManagementWindow.Bounds.X = 42;
        store.Settings.Notifications.Email.Password = "new-password";

        store.SaveConfig(_file);

        var settingsXml = File.ReadAllText(_file);
        StringAssert.Contains(settingsXml, "<AppUserSettings");
        Assert.IsFalse(settingsXml.Contains("new-password"), "the SMTP password must not be stored in clear text");
        StringAssert.Contains(File.ReadAllText(Path.Combine(_dir, "KNoteState.config")), "<X>42</X>");

        var reloaded = new Store(factoryViews: null!);
        reloaded.LoadConfig(_file);
        Assert.AreEqual(3930, reloaded.State.Session.RunCounter);
        Assert.AreEqual(42, reloaded.State.ManagementWindow.Bounds.X);
        Assert.AreEqual("new-password", reloaded.Settings.Notifications.Email.Password);
        Assert.AreEqual(2, reloaded.Settings.Repositories.Items.Count);
        // The one accepted exception: AppInfoAlarmRow was renamed from AppInfoAlarmRowConfig, and the
        // old element name is deliberately not kept alive (see its doc comment).
        Assert.AreEqual(0, reloaded.State.AppInfoAlarmsWindow.Rows.Count);
        Assert.AreEqual("COM7", reloaded.Settings.Connectivity.ServerCOM.PortName);
        Assert.AreEqual(0, reloaded.TakeConfigNotices().Count, "a normal load has nothing to report");
    }

    [TestMethod]
    public void LoadConfig_MissingFile_LeavesTheDefaults()
    {
        var store = new Store(factoryViews: null!);

        store.LoadConfig(_file);

        Assert.AreEqual(587, store.Settings.Notifications.Email.Port);
        Assert.IsTrue(store.State.ManagementWindow.Panels.Toolbar);
        Assert.AreEqual(0, store.TakeConfigNotices().Count);
    }

    [TestMethod]
    public void SaveConfig_FirstRun_CreatesBothFiles()
    {
        var store = new Store(factoryViews: null!);
        store.Settings.General.AlarmSeconds = 45;

        store.SaveConfig(_file);

        Assert.IsTrue(File.Exists(_file));
        Assert.IsTrue(File.Exists(Path.Combine(_dir, "KNoteState.config")));
    }

    [TestMethod]
    public void ChangeActiveFolder_RemembersRepositoryAndFolderInTheState()
    {
        var store = new Store(factoryViews: null!);
        var folderId = Guid.NewGuid();
        var folder = new FolderWithServiceRef
        {
            ServiceRef = KNote.ClientWin.Tests.Helpers.TestServiceRefFactory.CreateInMemorySqlite("Work"),
            FolderInfo = new KNote.Model.Dto.FolderInfoDto { FolderId = folderId }
        };

        store.ChangeActiveFolderWithServiceRef(folder);

        Assert.AreEqual("Work", store.State.Session.LastActiveRepositoryAlias);
        Assert.AreEqual(folderId, store.State.Session.LastActiveFolderId);
    }
}
