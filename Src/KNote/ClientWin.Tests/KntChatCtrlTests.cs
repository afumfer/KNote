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

    [TestMethod]
    public void Run_EmptyUrl_AtStartup_StaysSilent()
    {
        var (ctrl, view) = CreateCtrl(null);

        var result = ctrl.Run();

        Assert.AreEqual(EControllerResult.Error, result.Entity);
        Assert.AreEqual(0, view.ShowInfoCallCount);
    }
}
