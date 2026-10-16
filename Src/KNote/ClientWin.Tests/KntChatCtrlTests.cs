using System.Net;
using KNote.ClientWin.Tests.Helpers;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for KntChatCtrl, the chat room: feedback when the chat hub url or the sign-in are missing, the threads
/// followed when it connects (and again when it reconnects), new messages (shown, counted as unread, announced),
/// writing in the general thread from a script, a sign-in refused by the server, and the hub's callbacks handled on
/// the UI thread. The hub is a FakeChatRoomConnection: no real one is contacted.
/// </summary>
[TestClass]
public class KntChatCtrlTests
{
    private const string Url = "http://localhost/KNote/chathub";
    private const string UserName = "jdoe";

    private static readonly ChatRoomThreadInfoDto ThreadA = new() { ThreadId = Guid.NewGuid(), NoteNumber = 1, Topic = "A", IsParticipant = true };
    private static readonly ChatRoomThreadInfoDto ThreadB = new() { ThreadId = Guid.NewGuid(), NoteNumber = 2, Topic = "B", IsParticipant = true };
    private static readonly ChatRoomThreadInfoDto ThreadC = new() { ThreadId = Guid.NewGuid(), NoteNumber = 3, Topic = "C" };
    private static readonly ChatRoomThreadInfoDto General = new() { ThreadId = Guid.NewGuid(), NoteNumber = 4, Topic = KntConst.ChatRoomGeneralThreadTopic };

    private sealed record Setup(KntChatCtrl Ctrl, FakeChatView View, FakeChatRoomConnection Connection, Store Store);

    private static Setup CreateCtrl(string? chatHubUrl = Url, string? token = "token-1")
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeChatView();
        factoryViews.Registry.Register<KntChatCtrl, IViewChatRoom>(c => view);

        var store = new Store(factoryViews) { AppUserName = "windows-user" }.GrantAdmin();
        store.Settings.Connectivity.ChatHub.Url = chatHubUrl;
        store.Settings.Connectivity.ChatHub.UserName = UserName;
        store.Settings.Connectivity.ChatHub.Token = token;

        var connection = new FakeChatRoomConnection { InvokeImpl = Hub };
        var ctrl = new KntChatCtrl(store) { ConnectionFactory = (url, tokenProvider) => connection };
        return new Setup(ctrl, view, connection, store);
    }

    // The hub: every thread, each one subscribed with no messages, the general thread, and messages saved as sent.
    private static object Hub(string method, object[] args) => method switch
    {
        ChatRoomHubContract.GetThreads => new Result<List<ChatRoomThreadInfoDto>>(new List<ChatRoomThreadInfoDto> { Copy(ThreadA), Copy(ThreadB), Copy(ThreadC) }),
        ChatRoomHubContract.Subscribe => new Result<ChatRoomThreadDto>(new ChatRoomThreadDto { ThreadId = (Guid)args[0], Topic = "subscribed" }),
        ChatRoomHubContract.Unsubscribe => new Result(),
        ChatRoomHubContract.GetGeneralThread => new Result<ChatRoomThreadInfoDto>(Copy(General)),
        ChatRoomHubContract.SendMessage => new Result<ChatRoomMessageDto>(Message((Guid)args[0], UserName, (string)args[1])),
        _ => throw new NotSupportedException(method)
    };

    private static ChatRoomThreadInfoDto Copy(ChatRoomThreadInfoDto thread) => new()
    {
        ThreadId = thread.ThreadId,
        NoteNumber = thread.NoteNumber,
        Topic = thread.Topic,
        IsParticipant = thread.IsParticipant
    };

    private static ChatRoomMessageDto Message(Guid threadId, string userName, string text) => new()
    {
        MessageId = Guid.NewGuid(),
        ThreadId = threadId,
        UserName = userName,
        Text = text,
        SentDateTime = DateTime.Now
    };

    // Started as the menu does: the view first (so the ctrl refreshes it), then Run. The fake hub answers at once,
    // so Run returns connected.
    private static void Start(Setup setup)
    {
        setup.Ctrl.ShowErrorMessagesOnInitialize = true;
        setup.Ctrl.ShowChatView(false);
        var result = setup.Ctrl.Run();
        Assert.AreEqual(EControllerResult.Executed, result.Entity, result.ErrorMessage);
    }

    [TestMethod]
    public void Run_EmptyUrl_OpenedManually_ShowsErrorToUser()
    {
        var setup = CreateCtrl("");
        setup.Ctrl.ShowErrorMessagesOnInitialize = true;

        var result = setup.Ctrl.Run();

        Assert.AreEqual(EControllerResult.Error, result.Entity);
        Assert.AreEqual(1, setup.View.ShowInfoCallCount);
        StringAssert.Contains(setup.View.LastShownInfo, "url is not defined");
    }

    [TestMethod]
    public void Run_EmptyUrl_AtStartup_StaysSilent()
    {
        var setup = CreateCtrl(null);

        var result = setup.Ctrl.Run();

        Assert.AreEqual(EControllerResult.Error, result.Entity);
        Assert.AreEqual(0, setup.View.ShowInfoCallCount);
    }

    [TestMethod]
    public void Run_NotSignedIn_OpenedManually_AsksToSignInFromOptions()
    {
        var setup = CreateCtrl(token: null);
        setup.Ctrl.ShowErrorMessagesOnInitialize = true;

        var result = setup.Ctrl.Run();

        Assert.AreEqual(EControllerResult.Error, result.Entity);
        StringAssert.Contains(setup.View.LastShownInfo, "Sign in from Options");
    }

    [TestMethod]
    public void Run_NotSignedIn_AtStartup_NotifiesWithoutInterrupting()
    {
        var setup = CreateCtrl(token: "");
        var notifications = new List<string>();
        setup.Store.Events.Subscribe<ControllerNotification>(n => notifications.Add(n.Message));

        var result = setup.Ctrl.Run();

        Assert.AreEqual(EControllerResult.Error, result.Entity);
        Assert.AreEqual(0, setup.View.ShowInfoCallCount);
        Assert.AreEqual(1, notifications.Count);
        StringAssert.Contains(notifications[0], "Sign in from Options");
    }

    [TestMethod]
    public void Connecting_UsesTheTokenOfTheSettings()
    {
        var setup = CreateCtrl();
        Func<string>? tokenProvider = null;
        setup.Ctrl.ConnectionFactory = (url, token) =>
        {
            Assert.AreEqual(Url, url);
            tokenProvider = token;
            return setup.Connection;
        };

        Start(setup);
        setup.Store.Settings.Connectivity.ChatHub.Token = "token-2";

        Assert.AreEqual("token-2", tokenProvider!(), "Read on every connection attempt: signing in again needs no restart.");
    }

    [TestMethod]
    public void Connecting_LoadsTheThreads_AndFollowsTheOnesTheUserTakesPartIn()
    {
        var setup = CreateCtrl();

        Start(setup);

        Assert.AreEqual(ChatRoomConnectionState.Connected, setup.Ctrl.ConnectionState);
        Assert.AreEqual(3, setup.Ctrl.Threads.Count);
        CollectionAssert.AreEquivalent(new[] { ThreadA.ThreadId, ThreadB.ThreadId }, Subscribed(setup));
        Assert.IsTrue(setup.Ctrl.IsFollowed(ThreadA.ThreadId));
        Assert.IsFalse(setup.Ctrl.IsFollowed(ThreadC.ThreadId));
        Assert.IsNull(setup.Ctrl.CurrentThreadId);
        Assert.IsTrue(setup.View.RefreshThreadsCallCount > 0);
    }

    [TestMethod]
    public async Task Reconnected_FollowsTheThreadsAgain_IncludingTheOnesOpened()
    {
        var setup = CreateCtrl();
        Start(setup);
        Assert.IsTrue(await setup.Ctrl.SelectThreadAsync(ThreadC.ThreadId));
        setup.Connection.Invocations.Clear();

        await setup.Connection.RaiseReconnectedAsync();

        CollectionAssert.AreEquivalent(new[] { ThreadA.ThreadId, ThreadB.ThreadId, ThreadC.ThreadId }, Subscribed(setup));
        Assert.AreEqual(ThreadC.ThreadId, setup.Ctrl.CurrentThreadId);
    }

    [TestMethod]
    public async Task AMessage_OfTheThreadShown_IsAppended_OfAnotherOne_IsUnreadAndAnnounced_AndTheUsersOwnIsNot()
    {
        var setup = CreateCtrl();
        Start(setup);
        await setup.Ctrl.SelectThreadAsync(ThreadA.ThreadId);
        var announced = new List<string>();
        setup.Ctrl.ReceiveMessage += (s, e) => announced.Add(e.Entity);

        setup.Connection.Raise(ChatRoomHubContract.MessageReceived, Message(ThreadA.ThreadId, "maria", "On screen"));
        setup.Connection.Raise(ChatRoomHubContract.MessageReceived, Message(ThreadB.ThreadId, "maria", "Elsewhere"));
        setup.Connection.Raise(ChatRoomHubContract.MessageReceived, Message(ThreadB.ThreadId, UserName, "Mine, from another window"));

        Assert.AreEqual(1, setup.View.AppendedMessages.Count);
        Assert.AreEqual("On screen", setup.View.AppendedMessages[0].Text);
        Assert.AreEqual(0, setup.Ctrl.UnreadCount(ThreadA.ThreadId));
        Assert.AreEqual(1, setup.Ctrl.UnreadCount(ThreadB.ThreadId));
        CollectionAssert.AreEqual(new[] { "maria: On screen", "maria: Elsewhere" }, announced);
        Assert.AreEqual(ThreadB.ThreadId, setup.Ctrl.Threads[0].ThreadId, "The thread with the last message comes first.");

        await setup.Ctrl.SelectThreadAsync(ThreadB.ThreadId);
        Assert.AreEqual(0, setup.Ctrl.UnreadCount(ThreadB.ThreadId), "Seen once shown.");
    }

    [TestMethod]
    public void TheActivityOfAThreadNotFollowed_CountsAsUnread()
    {
        var setup = CreateCtrl();
        Start(setup);

        setup.Connection.Raise(ChatRoomHubContract.ThreadActivity,
            new ChatRoomThreadActivityDto { ThreadId = ThreadC.ThreadId, UserName = "maria", ModificationDateTime = DateTime.Now });
        setup.Connection.Raise(ChatRoomHubContract.ThreadActivity,
            new ChatRoomThreadActivityDto { ThreadId = ThreadA.ThreadId, UserName = "maria", ModificationDateTime = DateTime.Now });

        Assert.AreEqual(1, setup.Ctrl.UnreadCount(ThreadC.ThreadId));
        Assert.AreEqual(0, setup.Ctrl.UnreadCount(ThreadA.ThreadId), "Followed: its messages arrive with MessageReceived.");
    }

    [TestMethod]
    public async Task SendMessage_WithNoThreadShown_WritesInTheGeneralThread()
    {
        var setup = CreateCtrl();
        Start(setup);

        var sent = await setup.Ctrl.SendMessageAsync("Hello from a script");

        Assert.IsTrue(sent);
        var send = setup.Connection.Invocations.Single(i => i.Method == ChatRoomHubContract.SendMessage);
        Assert.AreEqual(General.ThreadId, send.Args[0]);
        Assert.AreEqual("Hello from a script", send.Args[1]);
    }

    [TestMethod]
    public async Task CreateThread_ShowsTheNewThread()
    {
        var setup = CreateCtrl();
        var created = new ChatRoomThreadInfoDto { ThreadId = Guid.NewGuid(), NoteNumber = 9, Topic = "New one", IsParticipant = true };
        setup.Connection.InvokeImpl = (method, args) => method == ChatRoomHubContract.CreateThread
            ? new Result<ChatRoomThreadInfoDto>(created)
            : Hub(method, args);
        Start(setup);

        var thread = await setup.Ctrl.CreateThreadAsync("New one");

        Assert.AreEqual(created.ThreadId, thread.ThreadId);
        Assert.AreEqual(created.ThreadId, setup.Ctrl.CurrentThreadId);
        Assert.AreEqual(created.ThreadId, setup.Ctrl.Threads[0].ThreadId);
        Assert.IsTrue(setup.Ctrl.IsFollowed(created.ThreadId));
    }

    [TestMethod]
    public async Task UnfollowCurrentThread_LeavesItInThisConnection()
    {
        var setup = CreateCtrl();
        Start(setup);
        await setup.Ctrl.SelectThreadAsync(ThreadA.ThreadId);

        await setup.Ctrl.UnfollowCurrentThreadAsync();

        Assert.IsNull(setup.Ctrl.CurrentThreadId);
        Assert.IsFalse(setup.Ctrl.IsFollowed(ThreadA.ThreadId));
        Assert.IsTrue(setup.Connection.Invocations.Any(i => i.Method == ChatRoomHubContract.Unsubscribe && (Guid)i.Args[0] == ThreadA.ThreadId));
    }

    [TestMethod]
    public void ASignInRefusedByTheServer_IsReported_AsSigningInAgain()
    {
        var setup = CreateCtrl();
        setup.Connection.StartAsyncImpl = () => Task.FromException(new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized));
        setup.Ctrl.ShowErrorMessagesOnInitialize = true;

        setup.Ctrl.Run();

        Assert.AreEqual(ChatRoomConnectionState.NotAuthorized, setup.Ctrl.ConnectionState);
        StringAssert.Contains(setup.View.LastShownInfo, "Sign in again from Options");
    }

    /// <summary>Queues posted callbacks so a test decides when (and on which thread) they run.</summary>
    private sealed class QueueSynchronizationContext : SynchronizationContext
    {
        public Queue<(SendOrPostCallback callback, object? state)> Pending { get; } = new();

        public override void Post(SendOrPostCallback d, object? state) => Pending.Enqueue((d, state));
    }

    [TestMethod]
    public async Task HubCallbacks_ArePostedToTheUiContext_InsteadOfHandledOnTheCallersThread()
    {
        var setup = CreateCtrl();
        Start(setup);
        await setup.Ctrl.SelectThreadAsync(ThreadA.ThreadId);
        var uiContext = new QueueSynchronizationContext();
        setup.Ctrl.UiContext = uiContext;

        setup.Connection.Raise(ChatRoomHubContract.MessageReceived, Message(ThreadA.ThreadId, "maria", "hi"));

        Assert.AreEqual(0, setup.View.AppendedMessages.Count, "Nothing is handled until the UI context runs the posted callback.");
        Assert.AreEqual(1, uiContext.Pending.Count);

        var (callback, state) = uiContext.Pending.Dequeue();
        callback(state);

        Assert.AreEqual(1, setup.View.AppendedMessages.Count);
    }

    private static List<Guid> Subscribed(Setup setup) =>
        setup.Connection.Invocations.Where(i => i.Method == ChatRoomHubContract.Subscribe).Select(i => (Guid)i.Args[0]).ToList();
}
