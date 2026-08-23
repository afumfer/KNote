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
}
