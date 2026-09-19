using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for KntChatCtrl: feedback when the chat hub url is not configured, and delivery of hub
/// messages on the UI thread. No real hub is contacted (the connection is never started).
/// </summary>
[TestClass]
public class KntChatCtrlTests
{
    private static (KntChatCtrl ctrl, FakeChatView view) CreateCtrl(string? chatHubUrl)
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeChatView();
        factoryViews.Registry.Register<KntChatCtrl, IViewChat>(c => view);

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        store.AppConfig.ChatHubUrl = chatHubUrl;

        return (new KntChatCtrl(store), view);
    }

    [TestMethod]
    public void Run_EmptyUrl_OpenedManually_ShowsErrorToUser()
    {
        var (ctrl, view) = CreateCtrl("");
        ctrl.ShowErrorMessagesOnInitialize = true;

        var result = ctrl.Run();

        Assert.AreEqual(EControllerResult.Error, result.Entity);
        Assert.AreEqual(1, view.ShowInfoCallCount);
        StringAssert.Contains(view.LastShownInfo, "url is not defined");
    }

    /// <summary>Queues posted callbacks so a test decides when (and on which thread) they run.</summary>
    private sealed class QueueSynchronizationContext : SynchronizationContext
    {
        public Queue<(SendOrPostCallback callback, object? state)> Pending { get; } = new();

        public override void Post(SendOrPostCallback d, object? state) => Pending.Enqueue((d, state));
    }

    [TestMethod]
    public void DispatchReceivedMessage_PostsToUiContext_InsteadOfRaisingOnCallerThread()
    {
        var (ctrl, _) = CreateCtrl("http://localhost/chathub");
        var uiContext = new QueueSynchronizationContext();
        ctrl.UiContext = uiContext;
        string? received = null;
        ctrl.ReceiveMessage += (s, e) => received = e.Entity;

        ctrl.DispatchReceivedMessage("ana", "hi");

        Assert.IsNull(received, "The event must not be raised until the UI context runs the posted callback.");
        Assert.AreEqual(1, uiContext.Pending.Count);

        var (callback, state) = uiContext.Pending.Dequeue();
        callback(state);

        Assert.AreEqual("ana: hi", received);
    }

    [TestMethod]
    public void DispatchReceivedMessage_WithoutUiContext_RaisesDirectly()
    {
        var (ctrl, _) = CreateCtrl("http://localhost/chathub");
        string? received = null;
        ctrl.ReceiveMessage += (s, e) => received = e.Entity;

        ctrl.DispatchReceivedMessage("ana", "hi");

        Assert.AreEqual("ana: hi", received);
    }

    [TestMethod]
    public void Run_EmptyUrl_AtStartup_StaysSilent()
    {
        var (ctrl, view) = CreateCtrl(null);

        var result = ctrl.Run();

        Assert.AreEqual(EControllerResult.Error, result.Entity);
        Assert.AreEqual(0, view.ShowInfoCallCount);
    }
}
