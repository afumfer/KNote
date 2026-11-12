using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Data.Sqlite;

namespace KNote.ClientWin.Tests;

/// <summary>
/// RepositoryEditorCtrl and the repository's name and id, which live in its database: the name is required
/// and can't be the one of another linked repository when creating or editing it, editing it renames the
/// repository in its database, and a database already linked (or a copy of one: same RepositoryId) can't be
/// linked again. Only the paths that end before Store.SaveConfig, which always writes the real
/// %LocalAppData%\KNote configuration (see ClientWin.Tests/CLAUDE.md).
/// </summary>
[TestClass]
public class RepositoryEditorCtrlTests
{
    private string _folder = null!;

    [TestInitialize]
    public void Initialize()
    {
        _folder = Path.Combine(Path.GetTempPath(), $"knote-repository-editor-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_folder);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
            // Best effort: the OS temp folder reclaims it eventually.
        }
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task Create_WithoutName_IsRefusedBeforeCreatingTheDatabase(string? name)
    {
        var (ctrl, view, _) = CreateCtrl(EnumRepositoryEditorMode.Create);
        await ctrl.NewModel();
        FillModel(ctrl.Model, "new.db", name);

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        StringAssert.Contains(view.LastShownInfo, "name is required");
        Assert.IsFalse(File.Exists(DatabaseFile("new.db")));
    }

    [TestMethod]
    public async Task Create_WithTheNameOfALinkedRepository_IsRefused()
    {
        var (ctrl, view, store) = CreateCtrl(EnumRepositoryEditorMode.Create);
        Link(store, "Personal", "personal.db");
        await ctrl.NewModel();
        FillModel(ctrl.Model, "new.db", " PERSONAL ");

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        StringAssert.Contains(view.LastShownInfo, "already a linked repository named 'Personal'");
        Assert.IsFalse(File.Exists(DatabaseFile("new.db")));
    }

    [TestMethod]
    public async Task Create_OnADatabaseThatIsAlreadyLinked_IsRefused()
    {
        var (ctrl, view, store) = CreateCtrl(EnumRepositoryEditorMode.Create);
        Link(store, "Work", "work.db");
        await ctrl.NewModel();
        FillModel(ctrl.Model, "work.db", "Another name");

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        StringAssert.Contains(view.LastShownInfo, "already linked as 'Work'");
        Assert.AreEqual(1, store.GetAllServiceRef().Count);
    }

    [TestMethod]
    public async Task AddLink_ADatabaseAlreadyLinked_IsRefused()
    {
        var (ctrl, view, store) = CreateCtrl(EnumRepositoryEditorMode.AddLink);
        Link(store, "Work", "work.db");
        await ctrl.NewModel();
        FillModel(ctrl.Model, "work.db", name: null);

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        StringAssert.Contains(view.LastShownInfo, "already linked as 'Work'");
        Assert.AreEqual(1, store.GetAllServiceRef().Count);
    }

    [TestMethod]
    public async Task AddLink_ACopyOfALinkedDatabase_IsRefused()
    {
        var (ctrl, view, store) = CreateCtrl(EnumRepositoryEditorMode.AddLink);
        Link(store, "Work", "work.db");
        SqliteConnection.ClearAllPools();
        File.Copy(DatabaseFile("work.db"), DatabaseFile("copy.db"));
        await ctrl.NewModel();
        FillModel(ctrl.Model, "copy.db", name: null);

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        StringAssert.Contains(view.LastShownInfo, "already linked as 'Work'");
        Assert.AreEqual(1, store.GetAllServiceRef().Count);
    }

    [TestMethod]
    public async Task Edit_WithTheNameOfAnotherLinkedRepository_IsRefused()
    {
        var (ctrl, view, store) = CreateCtrl(EnumRepositoryEditorMode.Management);
        Link(store, "Work", "work.db");
        var (service, serviceRef) = LinkFake(store, "Home");
        var renamed = false;
        service.SystemValuesFake.SaveRepositoryNameAsyncImpl = name => { renamed = true; return Task.FromResult(new Result<RepositoryInfoDto>()); };
        await ctrl.LoadModelById(service, service.IdServiceRef, refreshView: false);
        ctrl.Model.Name = "work";

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        StringAssert.Contains(view.LastShownInfo, "already a linked repository named 'Work'");
        Assert.IsFalse(renamed);
        Assert.AreEqual("Home", serviceRef.Name);
    }

    [TestMethod]
    public async Task Edit_WithoutName_IsRefused()
    {
        var (ctrl, view, store) = CreateCtrl(EnumRepositoryEditorMode.Management);
        var (service, serviceRef) = LinkFake(store, "Home");
        await ctrl.LoadModelById(service, service.IdServiceRef, refreshView: false);
        ctrl.Model.Name = "";

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        StringAssert.Contains(view.LastShownInfo, "name is required");
        Assert.AreEqual("Home", serviceRef.Name);
    }

    [TestMethod]
    public async Task Edit_NewName_IsSavedInTheRepository_AndNotAppliedWhenRefused()
    {
        var (ctrl, view, store) = CreateCtrl(EnumRepositoryEditorMode.Management);
        var (service, serviceRef) = LinkFake(store, "Home");
        string? savedName = null;
        service.SystemValuesFake.SaveRepositoryNameAsyncImpl = name =>
        {
            savedName = name;
            var refused = new Result<RepositoryInfoDto>();
            refused.AddErrorMessage("Only an Admin can rename the repository.");
            return Task.FromResult(refused);
        };
        await ctrl.LoadModelById(service, service.IdServiceRef, refreshView: false);
        ctrl.Model.Name = "Home renamed";

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("Home renamed", savedName);
        StringAssert.Contains(view.LastShownInfo, "Only an Admin can rename the repository.");
        Assert.AreEqual("Home", serviceRef.Name);
    }

    private (RepositoryEditorCtrl ctrl, FakeRepositoryEditorView view, Store store) CreateCtrl(EnumRepositoryEditorMode mode)
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeRepositoryEditorView();
        factoryViews.Registry.Register<RepositoryEditorCtrl, IViewEditor<RepositoryRef>>(c => view);

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var ctrl = new RepositoryEditorCtrl(store) { EditorMode = mode };

        return (ctrl, view, store);
    }

    private string DatabaseFile(string file) => Path.Combine(_folder, file);

    // What the form would leave in the model: a valid repository on a Sqlite file of the test's own folder.
    private void FillModel(RepositoryRef model, string file, string? name)
    {
        model.Name = name;
        model.Provider = "Microsoft.Data.Sqlite";
        model.Orm = "EntityFramework";
        model.ConnectionString = $"Data Source={DatabaseFile(file)}";
        model.ResourcesContainer = "KntCntResources";
        model.ResourcesContainerRootPath = _folder;
        model.ResourcesContainerRootUrl = "file:///" + _folder.Replace('\\', '/');
    }

    // A real repository, linked in the store, on a Sqlite file of the test's folder (named after "name").
    private ServiceRef Link(Store store, string name, string file)
    {
        var repositoryRef = new RepositoryRef();
        FillModel(repositoryRef, file, name);
        var serviceRef = new ServiceRef(repositoryRef, "jdoe");
        store.AddServiceRef(serviceRef);
        return serviceRef;
    }

    // A linked repository whose service is a fake, to see what the editor asks it.
    private (FakeKntService service, ServiceRef serviceRef) LinkFake(Store store, string name)
    {
        var service = new FakeKntService();
        var serviceRef = TestServiceRefFactory.CreateWithFakeService(service);
        FillModel(serviceRef.RepositoryRef, "fake.db", name);
        serviceRef.RepositoryRef.RepositoryId = Guid.NewGuid();
        store.AddServiceRef(serviceRef);
        return (service, serviceRef);
    }
}
