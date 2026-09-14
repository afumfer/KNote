using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ControllerRegistryTests
{
    // ControllerRegistry.Add requires a real CtrlBase instance, but CtrlBase's constructor needs a
    // Store (it self-registers via Store.AddController). That self-registration happens on the
    // Store passed in here, a throwaway instance separate from the ControllerRegistry under test in
    // each of these tests, so it does not interfere with them.
    private class TestCtrl : CtrlBase
    {
        public TestCtrl(Store store) : base(store) { }
    }

    private static Store CreateStore() => new(new TestFactoryViews());

    [TestMethod]
    public void All_NoControllersAdded_ReturnsEmpty()
    {
        var registry = new ControllerRegistry();

        Assert.IsEmpty(registry.All);
    }

    [TestMethod]
    public void Add_ThenAll_ReturnsAddedController()
    {
        var registry = new ControllerRegistry();
        var ctrl = new TestCtrl(CreateStore());

        registry.Add(ctrl);

        CollectionAssert.Contains(registry.All.ToList(), ctrl);
    }

    [TestMethod]
    public void Remove_RemovesControllerFromRegistry()
    {
        var registry = new ControllerRegistry();
        var ctrl = new TestCtrl(CreateStore());
        registry.Add(ctrl);

        registry.Remove(ctrl);

        Assert.IsEmpty(registry.All);
    }

    [TestMethod]
    public void All_ReturnsASnapshot_NotTheLiveList()
    {
        // The bug this guards against: .All used to return the live list itself, so a foreach over
        // it would throw "Collection was modified" (or worse, under real concurrency) if Add/Remove
        // happened during that same enumeration - even on a single thread, e.g. a handler invoked
        // mid-iteration adding another controller.
        var registry = new ControllerRegistry();
        registry.Add(new TestCtrl(CreateStore()));

        var snapshot = registry.All;
        registry.Add(new TestCtrl(CreateStore()));

        Assert.AreEqual(1, snapshot.Count, "A previously taken snapshot must not see a later Add.");
        Assert.AreEqual(2, registry.All.Count);
    }

    // Regression coverage for ControllerRegistry's underlying List<CtrlBase> not being
    // synchronized: Add/Remove normally run on the UI thread, but KNoteScriptLibrary constructs
    // controllers directly from a KntScript's own background Thread (see the class-level comment in
    // ControllerRegistry.cs), while Store.SaveActiveNotes() (the autosave timer, UI thread) iterates
    // All. This is a plain in-memory class with no Control/STA dependency, so this genuinely
    // exercises real OS thread parallelism (via Task.Run), not just async interleaving on one
    // thread - the same technique used for DomainEventBusTests.
    [TestMethod]
    public async Task ConcurrentAddRemoveAll_FromManyThreads_DoesNotThrow()
    {
        var registry = new ControllerRegistry();
        var store = CreateStore();
        const int threadCount = 20;
        const int iterationsPerThread = 200;

        using var startGate = new ManualResetEventSlim(false);

        var tasks = Enumerable.Range(0, threadCount)
            .Select(_ => Task.Run(() =>
            {
                startGate.Wait();

                for (var i = 0; i < iterationsPerThread; i++)
                {
                    var ctrl = new TestCtrl(store);
                    registry.Add(ctrl);
                    _ = registry.All.Count;
                    registry.Remove(ctrl);
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
