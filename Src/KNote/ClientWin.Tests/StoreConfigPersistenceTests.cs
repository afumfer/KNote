using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests;

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

    [TestMethod]
    public void LoadConfig_V1Fixture_PopulatesSettingsAndState()
    {
        var store = new Store(factoryViews: null!);

        store.LoadConfig(Path.Combine(AppContext.BaseDirectory, "Fixtures", "KNoteData.v1.config"));

        Assert.AreEqual(35, store.Settings.General.AlarmSeconds);
        Assert.AreEqual("smtp.example.com", store.Settings.Notifications.Email.Host);
        Assert.AreEqual(2, store.Settings.Repositories.Items.Count);
        Assert.AreEqual(3929, store.State.Session.RunCounter);
        Assert.AreEqual(1363, store.State.ManagementWindow.Bounds.Width);
    }

    [TestMethod]
    public void SaveConfig_KeepsTheFlatV1FormatOnDisk_AndReloadsWithoutLosingValues()
    {
        var store = new Store(factoryViews: null!);
        store.LoadConfig(Path.Combine(AppContext.BaseDirectory, "Fixtures", "KNoteData.v1.config"));
        store.State.Session.RunCounter += 1;
        store.State.ManagementWindow.Bounds.X = 42;
        store.Settings.Notifications.Email.Password = "new-password";

        store.SaveConfig(_file);

        var xml = File.ReadAllText(_file);
        StringAssert.Contains(xml, "<AppConfig");           // still the pre-V2 file: no format change yet
        StringAssert.Contains(xml, "<ManagmentLocX>42</ManagmentLocX>");
        var reloaded = new Store(factoryViews: null!);
        reloaded.LoadConfig(_file);
        Assert.AreEqual(3930, reloaded.State.Session.RunCounter);
        Assert.AreEqual(42, reloaded.State.ManagementWindow.Bounds.X);
        Assert.AreEqual("new-password", reloaded.Settings.Notifications.Email.Password);
        Assert.AreEqual(2, reloaded.Settings.Repositories.Items.Count);
        Assert.AreEqual(2, reloaded.State.AppInfoAlarmsWindow.Rows.Count);
        Assert.AreEqual("COM7", reloaded.Settings.Connectivity.ServerCOM.PortName);
    }

    [TestMethod]
    public void LoadConfig_MissingFile_LeavesTheDefaults()
    {
        var store = new Store(factoryViews: null!);

        store.LoadConfig(_file);

        Assert.AreEqual(587, store.Settings.Notifications.Email.Port);
        Assert.IsTrue(store.State.ManagementWindow.Panels.Toolbar);
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
