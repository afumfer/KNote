using KNote.ClientWin.Core;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ViewFactoryRegistryTests
{
    [TestMethod]
    public void Resolve_RegisteredFactory_ReturnsViewBuiltByThatFactory()
    {
        var registry = new ViewFactoryRegistry();
        var store = new Store(factoryViews: null!);
        var ctrl = new ProbeCtrl(store);
        registry.Register<ProbeCtrl, string>(c => $"view-for-{c.ControllerName}");

        var view = registry.Resolve<ProbeCtrl, string>(ctrl);

        Assert.AreEqual("view-for-Probe", view);
    }

    [TestMethod]
    public void Resolve_NoFactoryRegisteredForType_Throws()
    {
        var registry = new ViewFactoryRegistry();
        var store = new Store(factoryViews: null!);
        var ctrl = new ProbeCtrl(store);

        Assert.ThrowsExactly<InvalidOperationException>(() => registry.Resolve<ProbeCtrl, string>(ctrl));
    }

    [TestMethod]
    public void RegisterAndResolve_WithDifferentKeys_ReturnIndependentFactories()
    {
        var registry = new ViewFactoryRegistry();
        var store = new Store(factoryViews: null!);
        var ctrl = new ProbeCtrl(store);
        registry.Register<ProbeCtrl, string>(c => "main", key: "Main");
        registry.Register<ProbeCtrl, string>(c => "notify", key: "Notify");

        Assert.AreEqual("main", registry.Resolve<ProbeCtrl, string>(ctrl, key: "Main"));
        Assert.AreEqual("notify", registry.Resolve<ProbeCtrl, string>(ctrl, key: "Notify"));
    }

    [TestMethod]
    public void Resolve_RegisteredKeyDoesNotMatchDefaultKey_Throws()
    {
        var registry = new ViewFactoryRegistry();
        var store = new Store(factoryViews: null!);
        var ctrl = new ProbeCtrl(store);
        registry.Register<ProbeCtrl, string>(c => "notify", key: "Notify");

        Assert.ThrowsExactly<InvalidOperationException>(() => registry.Resolve<ProbeCtrl, string>(ctrl));
    }

    [TestMethod]
    public void Register_CalledTwiceForSameTypeAndKey_LastRegistrationWins()
    {
        var registry = new ViewFactoryRegistry();
        var store = new Store(factoryViews: null!);
        var ctrl = new ProbeCtrl(store);
        registry.Register<ProbeCtrl, string>(c => "first");
        registry.Register<ProbeCtrl, string>(c => "second");

        Assert.AreEqual("second", registry.Resolve<ProbeCtrl, string>(ctrl));
    }

    /// <summary>Minimal CtrlBase subclass, just enough to be usable as a dictionary key by type.</summary>
    private class ProbeCtrl : CtrlBase
    {
        public ProbeCtrl(Store store) : base(store)
        {
            ControllerName = "Probe";
        }
    }
}
