using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ServiceRefRegistryTests
{
    // Building a real ServiceRef lazily creates its IKntRepository (Dapper), but that only stores
    // the RepositoryRef - no connection is opened until a repository method is actually called.
    private static ServiceRef CreateServiceRef(string alias) => new(
        new RepositoryRef
        {
            Alias = alias,
            Orm = "Dapper",
            Provider = "Microsoft.Data.Sqlite",
            ConnectionString = "Data Source=:memory:"
        },
        userIdentityName: "test-user");

    [TestMethod]
    public void GetAll_NoServiceRefsAdded_ReturnsEmpty()
    {
        var registry = new ServiceRefRegistry();

        Assert.IsEmpty(registry.GetAll());
    }

    [TestMethod]
    public void Add_ThenGetAll_ReturnsAddedServiceRef()
    {
        var registry = new ServiceRefRegistry();
        var serviceRef = CreateServiceRef("main");

        registry.Add(serviceRef);

        CollectionAssert.Contains(registry.GetAll(), serviceRef);
    }

    [TestMethod]
    public void Remove_RemovesServiceRefFromRegistry()
    {
        var registry = new ServiceRefRegistry();
        var serviceRef = CreateServiceRef("main");
        registry.Add(serviceRef);

        registry.Remove(serviceRef);

        Assert.IsEmpty(registry.GetAll());
    }

    [TestMethod]
    public void GetByAlias_ReturnsMatchingServiceRef()
    {
        var registry = new ServiceRefRegistry();
        var main = CreateServiceRef("main");
        var secondary = CreateServiceRef("secondary");
        registry.Add(main);
        registry.Add(secondary);

        Assert.AreSame(secondary, registry.GetByAlias("secondary"));
    }

    [TestMethod]
    public void GetByAlias_NoMatch_ReturnsNull()
    {
        var registry = new ServiceRefRegistry();
        registry.Add(CreateServiceRef("main"));

        Assert.IsNull(registry.GetByAlias("does-not-exist"));
    }

    [TestMethod]
    public void GetById_ReturnsMatchingServiceRef()
    {
        var registry = new ServiceRefRegistry();
        var serviceRef = CreateServiceRef("main");
        registry.Add(serviceRef);

        Assert.AreSame(serviceRef, registry.GetById(serviceRef.IdServiceRef));
    }

    [TestMethod]
    public void GetFirst_ReturnsFirstAddedServiceRef()
    {
        var registry = new ServiceRefRegistry();
        var main = CreateServiceRef("main");
        var secondary = CreateServiceRef("secondary");
        registry.Add(main);
        registry.Add(secondary);

        Assert.AreSame(main, registry.GetFirst());
    }

    [TestMethod]
    public void GetFirst_EmptyRegistry_ReturnsNull()
    {
        var registry = new ServiceRefRegistry();

        Assert.IsNull(registry.GetFirst());
    }

    // Regression coverage for ServiceRefRegistry's underlying List<ServiceRef> not being
    // synchronized: Add/Remove normally run on the UI thread, but a KntScript running on its own
    // background Thread (Store.RunKntSCodeInNewThread) is not prevented from reaching a code path
    // that adds/removes a ServiceRef, while MessagesManagmentCtrl's autosave/alarm timers (UI
    // thread) call GetAll/GetById/GetByAlias/GetFirst - see the class-level comment in
    // ServiceRefRegistry.cs. This is a plain in-memory class with no Control/STA dependency, so this
    // genuinely exercises real OS thread parallelism (via Task.Run), not just async interleaving on
    // one thread - the same technique used for DomainEventBusTests/ControllerRegistryTests.
    [TestMethod]
    public async Task ConcurrentAddRemoveReads_FromManyThreads_DoesNotThrow()
    {
        var registry = new ServiceRefRegistry();
        const int threadCount = 20;
        const int iterationsPerThread = 200;

        using var startGate = new ManualResetEventSlim(false);

        var tasks = Enumerable.Range(0, threadCount)
            .Select(threadIndex => Task.Run(() =>
            {
                startGate.Wait();

                for (var i = 0; i < iterationsPerThread; i++)
                {
                    var serviceRef = CreateServiceRef($"thread {threadIndex}, iteration {i}");
                    registry.Add(serviceRef);
                    _ = registry.GetAll().Count;
                    _ = registry.GetById(serviceRef.IdServiceRef);
                    _ = registry.GetByAlias(serviceRef.Alias);
                    _ = registry.GetFirst();
                    registry.Remove(serviceRef);
                }
            }))
            .ToArray();

        startGate.Set();

        // Should not throw - a pre-fix run reliably throws (InvalidOperationException from the
        // List, or occasionally an IndexOutOfRangeException from a torn read) well before all
        // threads finish.
        await Task.WhenAll(tasks);
    }
}
