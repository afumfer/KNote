using KNote.Server.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The repositories a Server opens at startup (KntRepositories, without a host): the "Repositories" list or the single
/// "RepositoryRef", the default one, and what is left out (a database that can't be opened, a copy of another one,
/// resources that would be shared).
/// </summary>
[TestClass]
public class KntRepositoriesTests
{
    private readonly List<string> _files = new();
    private readonly string _webRoot = Path.Combine(Path.GetTempPath(), $"knote-webroot-{Guid.NewGuid():N}");

    [TestCleanup]
    public void Cleanup()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var file in _files.Where(File.Exists))
            File.Delete(file);
    }

    [TestMethod]
    public async Task Opens_EachRepository_AndTheDefaultOne()
    {
        var repositories = await OpenAsync(Config("a"), Config("b", isDefault: true));

        Assert.HasCount(2, repositories.All);
        Assert.AreEqual(repositories.All[1], repositories.Default);
        Assert.AreNotEqual(repositories.All[0].RepositoryId, repositories.All[1].RepositoryId);
    }

    [TestMethod]
    public async Task WithoutADefault_TheFirstOne()
    {
        var repositories = await OpenAsync(Config("a"), Config("b"));

        Assert.AreEqual(repositories.All[0], repositories.Default);
    }

    [TestMethod]
    public async Task ACopyOfADatabase_IsLeftOut()
    {
        var a = Config("a");
        await OpenAsync(a);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        var copy = Config("copy");
        File.Copy(DatabaseOf(a), DatabaseOf(copy));

        var repositories = await OpenAsync(a, copy);

        Assert.HasCount(1, repositories.All);
        Assert.AreEqual("a", repositories.All[0].RepositoryRef.ResourcesContainer);
    }

    [TestMethod]
    public async Task ARepositoryThatCantBeOpened_IsLeftOut_AndTheOthersOpen()
    {
        var broken = Config("broken", isDefault: true);
        broken.Orm = "NoSuchOrm";

        var repositories = await OpenAsync(broken, Config("b"));

        Assert.HasCount(1, repositories.All);
        Assert.AreEqual("b", repositories.Default.RepositoryRef.ResourcesContainer, "The default one couldn't be opened: the first that could.");
    }

    [TestMethod]
    public async Task NoRepositoryOpened_TheServerDoesNotStart()
    {
        var broken = Config("broken");
        broken.Orm = "NoSuchOrm";

        await Assert.ThrowsAsync<InvalidOperationException>(() => OpenAsync(broken));
    }

    [TestMethod]
    public async Task RepositoriesSharingTheirResources_TheLaterOneIsLeftOut()
    {
        var a = Config("a");
        var b = Config("b");
        b.ResourcesContainer = a.ResourcesContainer;

        var repositories = await OpenAsync(a, b);

        Assert.HasCount(1, repositories.All);
    }

    [TestMethod]
    public void TheConfiguration_IsTheRepositoriesList_OrTheSingleRepositoryRef()
    {
        var single = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["RepositoryRef:ConnectionString"] = "Data Source=one.db",
            ["RepositoryRef:Alias"] = "Old name"
        }).Build();
        var list = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["RepositoryRef:ConnectionString"] = "Data Source=one.db",
            ["Repositories:0:ConnectionString"] = "Data Source=a.db",
            ["Repositories:1:ConnectionString"] = "Data Source=b.db",
            ["Repositories:1:IsDefault"] = "true"
        }).Build();

        var fromSingle = KntRepositories.ReadConfiguration(single);
        var fromList = KntRepositories.ReadConfiguration(list);

        Assert.HasCount(1, fromSingle);
        Assert.IsTrue(fromSingle[0].IsDefault);
        Assert.AreEqual("Old name", fromSingle[0].Name, "LEGACY(repository-alias)");
        Assert.HasCount(2, fromList);
        Assert.IsTrue(fromList[1].IsDefault);
    }

    [TestMethod]
    public async Task Reload_OpensTheNewOnes_AndLeavesOutTheRemovedOnes()
    {
        var a = Config("a");
        var b = Config("b", isDefault: true);
        var repositories = await OpenAsync(a);
        var idA = repositories.Default.RepositoryId;

        var added = await repositories.ReloadAsync([Config("a-again", databaseOf: a), b], NullLogger.Instance);

        Assert.IsEmpty(added);
        Assert.HasCount(2, repositories.All);
        Assert.AreEqual("b", repositories.Default.RepositoryRef.ResourcesContainer);
        Assert.IsNotNull(repositories.Find(idA), "The same database: the same repository.");

        var removed = await repositories.ReloadAsync([Config("b-again", databaseOf: b)], NullLogger.Instance);

        Assert.IsEmpty(removed);
        Assert.IsNull(repositories.Find(idA), "Unlinked: its sessions are refused.");
        Assert.HasCount(1, repositories.All);
    }

    [TestMethod]
    public async Task Reload_NoneCanBeOpened_KeepsTheCurrentOnes_AndSaysWhy()
    {
        var repositories = await OpenAsync(Config("a"));
        var broken = Config("broken");
        broken.Orm = "NoSuchOrm";

        var problems = await repositories.ReloadAsync([broken], NullLogger.Instance);

        Assert.HasCount(1, repositories.All);
        Assert.AreEqual("a", repositories.Default.RepositoryRef.ResourcesContainer);
        Assert.IsTrue(problems.Any(p => p.Contains("broken")), string.Join(" | ", problems));
        Assert.IsTrue(problems.Any(p => p.Contains("current ones are kept")));
    }

    [TestMethod]
    public async Task Reload_ACopyOfADatabase_IsLeftOut_AndSaid()
    {
        var a = Config("a");
        var repositories = await OpenAsync(a);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        var copy = Config("copy");
        File.Copy(DatabaseOf(a), DatabaseOf(copy));

        var problems = await repositories.ReloadAsync([Config("a-again", databaseOf: a), copy], NullLogger.Instance);

        Assert.HasCount(1, repositories.All);
        Assert.IsTrue(problems.Any(p => p.Contains("copy")), string.Join(" | ", problems));
    }

    [TestMethod]
    public async Task ServeResources_EachRepositoryWithoutAUrl_ServedByThisHost()
    {
        var withUrl = Config("with-url");
        withUrl.ResourcesContainerRootUrl = "https://files.example.com";

        var repositories = await KntRepositories.OpenAsync([Config("served"), withUrl], _webRoot, NullLogger.Instance, serveResources: true);

        var served = repositories.All.Single(r => r.RepositoryRef.ResourcesContainer == "served");
        Assert.AreEqual($"/KNote/Resources/{served.RepositoryId}", served.RepositoryRef.ResourcesContainerRootUrl);
        Assert.AreEqual("https://files.example.com", repositories.All.Single(r => r.RepositoryRef.ResourcesContainer == "with-url").RepositoryRef.ResourcesContainerRootUrl);
    }

    private Task<KntRepositories> OpenAsync(params KntRepositoryConfig[] configs) =>
        KntRepositories.OpenAsync(configs, _webRoot, NullLogger.Instance);

    // Each one with its own database and resources (the container's name tells them apart in the asserts).
    // databaseOf: the same database as that one (a configuration read again).
    private KntRepositoryConfig Config(string name, bool isDefault = false, KntRepositoryConfig? databaseOf = null)
    {
        var file = databaseOf != null ? DatabaseOf(databaseOf) : Path.Combine(Path.GetTempPath(), $"knote-repositories-{Guid.NewGuid():N}.db");
        _files.Add(file);
        return new KntRepositoryConfig
        {
            Name = name,
            Orm = "EntityFramework",
            Provider = "Microsoft.Data.Sqlite",
            ConnectionString = $"Data Source={file}",
            ResourcesContainer = name,
            IsDefault = isDefault
        };
    }

    private static string DatabaseOf(KntRepositoryConfig config) => config.ConnectionString["Data Source=".Length..];
}
