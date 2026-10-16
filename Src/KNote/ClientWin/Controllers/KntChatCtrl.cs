using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// The chat room of a KNote Server (ChatRoomHubContract): its threads, the ones followed in this connection (with
/// their messages) and the one shown, and writing in them. The user signs in to that Server once, from Options
/// (ChatHubConfig: only its token is kept), and the hub is reached with that token. Started at startup (hidden, it
/// shows itself when a message arrives) or from the menu. The threads the user takes part in are followed when it
/// connects; a thread opened is followed too. Subscriptions belong to the connection: they are made again after it
/// reconnects. The hub's callbacks arrive on thread-pool threads and are handled on the UI thread (UiContext).
/// </summary>
[KntAuthorize(EnumRoles.Staff, AuthorizationScope.Application)]
public class KntChatCtrl : CtrlBase, IDisposable
{
    #region Private fields

    private IChatRoomConnection _connection;
    // The connection attempt in progress (or the last one): sending waits for it.
    private Task _connecting = Task.CompletedTask;

    // The threads followed in this connection, with their messages.
    private readonly Dictionary<Guid, ChatRoomThreadDto> _followed = new();
    private readonly Dictionary<Guid, int> _unread = new();

    // SignalR raises hub callbacks and the connection's events on thread-pool threads. Everything that ends up
    // touching the UI or this ctrl's state is posted to this context, captured in OnInitialized (always the UI
    // thread, except for a script, which has none). Internal so tests can substitute it.
    internal SynchronizationContext UiContext { get; set; }

    // Builds the connection to the hub (url, token): the SignalR one, or a fake in the tests.
    internal Func<string, Func<string>, IChatRoomConnection> ConnectionFactory { get; set; } =
        (url, token) => new ChatRoomConnection(url, token);

    // Sends an image to the Server (REST, not the hub): the real one, or a fake in the tests.
    internal Func<ChatRoomImageDto, Task<Result<ChatRoomImageDto>>> ImageUploader { get; set; }

    #endregion

    #region Properties

    public bool AutoCloseCtrlOnViewExit { get; set; } = false;
    public bool ShowErrorMessagesOnInitialize { get; set; } = false;
    public string Tag { get; set; } = "KntChatCtrl v 0.2";

    public ChatRoomConnectionState ConnectionState { get; private set; } = ChatRoomConnectionState.Disconnected;

    // Why the last connection attempt failed.
    public string ConnectionError { get; private set; }

    public string ServerUrl => Store.Settings.Connectivity.ChatHub.Url;

    // The base of the Server's application: the urls of its resources (the images of the messages) may be relative to it.
    public Uri ServerBaseUrl => ChatRoomServer.BaseUrl(ServerUrl);

    // The user in the chat room: the one signed in to its Server (not necessarily the app's one).
    public string UserName => Store.Settings.Connectivity.ChatHub.UserName;

    // Every thread, most recent activity first.
    public List<ChatRoomThreadInfoDto> Threads { get; private set; } = new();

    public Guid? CurrentThreadId { get; private set; }

    // ChatRoomImageDto.SupportedContentTypes, by the extensions of their files (for a file dialog).
    public const string ImageFilesFilter = "Images (*.png;*.jpg;*.jpeg;*.gif;*.webp)|*.png;*.jpg;*.jpeg;*.gif;*.webp";

    public ChatRoomThreadDto CurrentThread =>
        CurrentThreadId != null && _followed.TryGetValue(CurrentThreadId.Value, out var thread) ? thread : null;

    public bool IsFollowed(Guid threadId) => _followed.ContainsKey(threadId);

    // Messages of the thread that arrived while another one was shown.
    public int UnreadCount(Guid threadId) => _unread.TryGetValue(threadId, out var count) ? count : 0;

    #endregion

    #region Constructor

    public KntChatCtrl(Store store) : base(store)
    {
        ControllerName = "KntChat controller";
    }

    #endregion

    #region Events

    // A message of another user in a followed thread ("user: text"): KNoteManagementCtrl shows the chat room.
    public event EventHandler<ControllerEventArgs<string>> ReceiveMessage;

    #endregion

    #region Protected methods

    protected override Result<EControllerResult> OnInitialized()
    {
        try
        {
            var chatHub = Store.Settings.Connectivity.ChatHub;
            if (string.IsNullOrEmpty(chatHub.Url))
                return InitializationError("Chat hub url is not defined. Set the chat hub url in the Options menu.", notifyAtStartup: false);
            if (string.IsNullOrEmpty(chatHub.Token))
                return InitializationError("You are not signed in to the chat room server. Sign in from Options (Chat room tab).", notifyAtStartup: true);

            UiContext = SynchronizationContext.Current;

            _connection = ConnectionFactory(chatHub.Url, () => Store.Settings.Connectivity.ChatHub.Token);
            _connection.On<ChatRoomMessageDto>(ChatRoomHubContract.MessageReceived, m => PostToUi(() => OnMessageReceived(m)));
            _connection.On<ChatRoomThreadInfoDto>(ChatRoomHubContract.ThreadCreated, t => PostToUi(() => OnThreadCreated(t)));
            _connection.On<ChatRoomThreadActivityDto>(ChatRoomHubContract.ThreadActivity, a => PostToUi(() => OnThreadActivity(a)));
            _connection.On<ChatRoomOnlineUsersDto>(ChatRoomHubContract.OnlineUsersChanged, o => PostToUi(() => OnOnlineUsersChanged(o)));
            _connection.Reconnecting += _ => PostToUiAsync(() => SetConnectionState(ChatRoomConnectionState.Reconnecting));
            // A new connection: its subscriptions are made again (and the threads read again, as messages may have
            // been missed meanwhile).
            _connection.Reconnected += connectionId => PostToUiAsync(() => _ = OnConnectedAsync());
            _connection.Closed += OnConnectionClosedAsync;

            // Fire-and-forget: connecting must never block startup. Its continuations return to the UI thread
            // through the WinForms SynchronizationContext captured here.
            _connecting = ConnectWithTimeoutAsync();

            return new Result<EControllerResult>(EControllerResult.Executed);
        }
        catch (Exception ex)
        {
            var res = new Result<EControllerResult>(EControllerResult.Error);
            res.AddErrorMessage($"KntChat controller. The connection could not be started. Error: {ex.Message}.");
            HandleConnectionFailure(ex);
            return res;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Writes in the thread shown or, with none (a script), in the general thread. False if it couldn't be sent
    /// (already reported).
    /// </summary>
    public async Task<bool> SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        await _connecting;
        if (_connection == null || !_connection.IsConnected)
        {
            ShowError("You are not connected to the chat room.");
            return false;
        }

        var threadId = CurrentThreadId;
        if (threadId == null)
        {
            var general = await InvokeAsync<ChatRoomThreadInfoDto>(ChatRoomHubContract.GetGeneralThread);
            if (!general.IsValid)
            {
                ShowError(general.ErrorMessage);
                return false;
            }
            threadId = general.Entity.ThreadId;
        }

        var res = await InvokeAsync<ChatRoomMessageDto>(ChatRoomHubContract.SendMessage, threadId.Value, message);
        if (!res.IsValid)
        {
            ShowError(res.ErrorMessage);
            return false;
        }

        // The message arrives with MessageReceived too when the thread is followed; this covers a reconnection between
        // both, and makes the user a participant.
        if (AddMessage(res.Entity))
            View?.AppendMessage(res.Entity);
        var info = Threads.FirstOrDefault(t => t.ThreadId == threadId);
        if (info != null)
            info.IsParticipant = true;
        return true;
    }

    /// <summary>
    /// Sends an image to the thread shown: kept by the Server as a resource of the thread (ChatRoomServer.UploadImageAsync)
    /// and then sent as a message with its Markdown. Only images (ChatRoomImageDto.SupportedContentTypes, by the file's
    /// extension) of KntConst.MaxLenResourceFile bytes at most. False if it couldn't be sent (already reported).
    /// </summary>
    public async Task<bool> SendImageAsync(string fileName, byte[] content)
    {
        var contentType = ChatRoomImageDto.ContentTypeOf(fileName);
        if (contentType == null)
        {
            ShowError($"{Path.GetFileName(fileName)}: only images (png, jpg, gif, webp) can be sent to the chat room.");
            return false;
        }
        if (content == null || content.Length == 0 || content.Length > KntConst.MaxLenResourceFile)
        {
            ShowError($"{Path.GetFileName(fileName)}: the image is empty or too big ({KntConst.MaxLenResourceFile / 1_000_000.0:0.#} MB at most).");
            return false;
        }
        if (CurrentThreadId is not Guid threadId || ConnectionState != ChatRoomConnectionState.Connected)
        {
            ShowError("Choose a thread first: the image is sent to the thread shown.");
            return false;
        }

        var image = new ChatRoomImageDto
        {
            ThreadId = threadId,
            FileName = Path.GetFileName(fileName),
            ContentType = contentType,
            ContentBase64 = Convert.ToBase64String(content)
        };
        var uploader = ImageUploader ?? (dto => ChatRoomServer.UploadImageAsync(ServerUrl, Store.Settings.Connectivity.ChatHub.Token, dto));
        var res = await uploader(image);
        if (!res.IsValid)
        {
            ShowError($"The image could not be sent. {res.ErrorMessage}");
            return false;
        }

        return await SendMessageAsync(res.Entity.Markdown);
    }

    // --------------------------------------------------------------------------
    // Warning: this method can cause a deadlock in single-threaded environments
    // (for example, Windows Forms or WPF applications) or ASP.NET applications.
    // It is recommended to use the asynchronous version of this method.
    // Use only in KntScript
    public void SendMessage(string message)
    {
        Task.Run(() => SendMessageAsync(message)).Wait();
    }
    // --------------------------------------------------------------------------

    /// <summary>Shows the thread (following it, with its messages, if it wasn't). False if it can't be read.</summary>
    public async Task<bool> SelectThreadAsync(Guid threadId)
    {
        if (!_followed.ContainsKey(threadId) && !await FollowAsync(threadId))
            return false;

        CurrentThreadId = threadId;
        _unread.Remove(threadId);
        View?.RefreshConversation();
        View?.RefreshThreads();
        return true;
    }

    /// <summary>A new thread, then shown. Null if it couldn't be created (already reported).</summary>
    public async Task<ChatRoomThreadInfoDto> CreateThreadAsync(string topic)
    {
        var res = await InvokeAsync<ChatRoomThreadInfoDto>(ChatRoomHubContract.CreateThread, topic);
        if (!res.IsValid)
        {
            ShowError(res.ErrorMessage);
            return null;
        }

        // ThreadCreated may have arrived first, without IsParticipant (the same for everybody).
        Threads.RemoveAll(t => t.ThreadId == res.Entity.ThreadId);
        Threads.Insert(0, res.Entity);
        await SelectThreadAsync(res.Entity.ThreadId);
        return res.Entity;
    }

    /// <summary>Stops following the thread shown in this connection (its new messages are no longer received).</summary>
    public async Task UnfollowCurrentThreadAsync()
    {
        if (CurrentThreadId is not Guid threadId || !_followed.Remove(threadId))
            return;

        CurrentThreadId = null;
        await InvokeResultAsync<Result>(ChatRoomHubContract.Unsubscribe, threadId);
        View?.RefreshConversation();
        View?.RefreshThreads();
    }

    /// <summary>Connects again after the connection was lost (automatic reconnection gives up after a while).</summary>
    public async Task ReconnectAsync()
    {
        if (_connection == null || ConnectionState is ChatRoomConnectionState.Connected or ChatRoomConnectionState.Connecting
            or ChatRoomConnectionState.Reconnecting)
            return;

        _connecting = ConnectWithTimeoutAsync();
        await _connecting;
    }

    #endregion

    #region Private methods

    private Result<EControllerResult> InitializationError(string message, bool notifyAtStartup)
    {
        var res = new Result<EControllerResult>(EControllerResult.Error);
        res.AddErrorMessage(message);

        // Opened manually by the user: tell them why nothing happens. At startup the caller already skips the chat
        // when the url is empty, so that stays silent; a missing sign-in is told without interrupting.
        if (ShowErrorMessagesOnInitialize)
            ChatView.ShowInfo(message, KntConst.AppName);
        else if (notifyAtStartup)
            NotifyMessage(message);

        return res;
    }

    private void PostToUi(Action action)
    {
        if (UiContext != null)
            UiContext.Post(_ => action(), null);
        else
            action();
    }

    private Task PostToUiAsync(Action action)
    {
        PostToUi(action);
        return Task.CompletedTask;
    }

    private async Task ConnectWithTimeoutAsync()
    {
        SetConnectionState(ChatRoomConnectionState.Connecting);
        try
        {
            using var cts = new CancellationTokenSource(ChatRoomServer.ConnectTimeout);
            await _connection.StartAsync(cts.Token);
            await OnConnectedAsync();
            HandleConnectionSuccess();
        }
        catch (Exception ex)
        {
            HandleConnectionFailure(ex);
        }
    }

    // Raised on a thread-pool thread once automatic reconnection gave up: one more attempt a moment later.
    private async Task OnConnectionClosedAsync(Exception error)
    {
        PostToUi(() => SetConnectionState(ChatRoomConnectionState.Disconnected));
        try
        {
            await Task.Delay(5000);
            if (_connection == null)
                return;
            using var cts = new CancellationTokenSource(ChatRoomServer.ConnectTimeout);
            await _connection.StartAsync(cts.Token);
            PostToUi(async () =>
            {
                await OnConnectedAsync();
                HandleConnectionSuccess();
            });
        }
        catch (Exception ex)
        {
            PostToUi(() => HandleConnectionFailure(ex));
        }
    }

    // Connected (or reconnected): the threads, and the followed ones subscribed again with the ones the user takes
    // part in.
    private async Task OnConnectedAsync()
    {
        ConnectionError = null;
        SetConnectionState(ChatRoomConnectionState.Connected);

        var resThreads = await InvokeAsync<List<ChatRoomThreadInfoDto>>(ChatRoomHubContract.GetThreads);
        if (resThreads.IsValid)
            Threads = resThreads.Entity;

        var toFollow = _followed.Keys
            .Union(Threads.Where(t => t.IsParticipant).Select(t => t.ThreadId))
            .ToList();
        foreach (var threadId in toFollow)
            await FollowAsync(threadId);

        if (CurrentThreadId != null && !_followed.ContainsKey(CurrentThreadId.Value))
            CurrentThreadId = null;

        View?.RefreshThreads();
        View?.RefreshConversation();
    }

    // A connection that succeeds - whether from the automatic startup attempt or from the user manually reopening
    // chat - proves the url works again, so any previous auto-disable no longer applies.
    private void HandleConnectionSuccess()
    {
        if (Store.State.Session.ChatHubAutoConnectDisabled)
        {
            Store.State.Session.ChatHubAutoConnectDisabled = false;
            Store.SaveConfig();
        }
    }

    // Startup (ShowErrorMessagesOnInitialize == false) must never freeze or interrupt the user again for a chat hub
    // that is known to be unreachable: the failure is reported through the non-blocking notification channel and
    // auto-connect is disabled in the state so the next startup does not retry it - the user re-enables it from
    // Options once the url is fixed or signing in again (both clear ChatHubAutoConnectDisabled). Opening the chat
    // manually (ShowErrorMessagesOnInitialize == true) keeps showing the error directly, since the user is actively
    // waiting on that action.
    private void HandleConnectionFailure(Exception ex)
    {
        ConnectionError = ChatRoomServer.ConnectionErrorMessage(ex);
        SetConnectionState(ChatRoomServer.IsNotAuthorized(ex) ? ChatRoomConnectionState.NotAuthorized : ChatRoomConnectionState.Disconnected);

        var resMessage = $"KntChat controller. The connection could not be started. Error: {ConnectionError}";

        if (ShowErrorMessagesOnInitialize)
        {
            ChatView.ShowInfo(resMessage, KntConst.AppName);
        }
        else
        {
            Store.State.Session.ChatHubAutoConnectDisabled = true;
            Store.SaveConfig();

            NotifyMessage($"{resMessage} Chat auto-connect has been disabled; fix the chat hub settings in Options to re-enable it.");
        }
    }

    private void SetConnectionState(ChatRoomConnectionState state)
    {
        ConnectionState = state;
        View?.RefreshConnectionState();
    }

    private async Task<bool> FollowAsync(Guid threadId)
    {
        var res = await InvokeAsync<ChatRoomThreadDto>(ChatRoomHubContract.Subscribe, threadId);
        if (!res.IsValid)
        {
            _followed.Remove(threadId);
            return false;
        }

        _followed[threadId] = res.Entity;
        return true;
    }

    internal void OnMessageReceived(ChatRoomMessageDto message)
    {
        if (!AddMessage(message))
            return;

        TouchThread(message.ThreadId, message.SentDateTime);
        if (message.ThreadId == CurrentThreadId)
            View?.AppendMessage(message);
        else if (!IsOwn(message.UserName))
            _unread[message.ThreadId] = UnreadCount(message.ThreadId) + 1;
        View?.RefreshThreads();

        if (!IsOwn(message.UserName))
            ReceiveMessage?.Invoke(this, new ControllerEventArgs<string>($"{message.UserName}: {message.Text}"));
    }

    private void OnThreadCreated(ChatRoomThreadInfoDto thread)
    {
        if (Threads.Any(t => t.ThreadId == thread.ThreadId))
            return;
        Threads.Insert(0, thread);
        View?.RefreshThreads();
    }

    // A message in any thread: the followed ones get it with MessageReceived; the others are marked as unread.
    private void OnThreadActivity(ChatRoomThreadActivityDto activity)
    {
        TouchThread(activity.ThreadId, activity.ModificationDateTime);
        if (!_followed.ContainsKey(activity.ThreadId) && !IsOwn(activity.UserName))
            _unread[activity.ThreadId] = UnreadCount(activity.ThreadId) + 1;
        View?.RefreshThreads();
    }

    private void OnOnlineUsersChanged(ChatRoomOnlineUsersDto online)
    {
        if (!_followed.TryGetValue(online.ThreadId, out var thread))
            return;
        thread.OnlineUserNames = online.UserNames;
        foreach (var userName in online.UserNames.Where(u => !thread.Participants.Contains(u)))
            thread.Participants.Add(userName);
        if (online.ThreadId == CurrentThreadId)
            View?.RefreshOnlineUsers();
    }

    // False if the thread isn't followed or the message was already there.
    private bool AddMessage(ChatRoomMessageDto message)
    {
        if (!_followed.TryGetValue(message.ThreadId, out var thread) || thread.Messages.Any(m => m.MessageId == message.MessageId))
            return false;

        thread.Messages.Add(message);
        if (!thread.Participants.Contains(message.UserName))
            thread.Participants.Add(message.UserName);
        return true;
    }

    // The thread had activity: first in the list.
    private void TouchThread(Guid threadId, DateTime modificationDateTime)
    {
        var info = Threads.FirstOrDefault(t => t.ThreadId == threadId);
        if (info == null)
            return;
        info.ModificationDateTime = modificationDateTime;
        Threads.Remove(info);
        Threads.Insert(0, info);
    }

    private bool IsOwn(string userName) => string.Equals(userName, UserName, StringComparison.OrdinalIgnoreCase);

    // The result of a call to the hub, invalid with the reason when it fails (never an exception).
    private async Task<Result<TEntity>> InvokeAsync<TEntity>(string method, params object[] args) =>
        await InvokeResultAsync<Result<TEntity>>(method, args);

    private async Task<TResult> InvokeResultAsync<TResult>(string method, params object[] args) where TResult : Result, new()
    {
        try
        {
            if (_connection == null || !_connection.IsConnected)
            {
                var notConnected = new TResult();
                notConnected.AddErrorMessage("You are not connected to the chat room.");
                return notConnected;
            }
            return await _connection.InvokeAsync<TResult>(method, args) ?? new TResult();
        }
        catch (Exception ex)
        {
            var res = new TResult();
            res.AddErrorMessage(ex.Message);
            return res;
        }
    }

    // In the view, when there is one; a script has none.
    private void ShowError(string message)
    {
        if (View != null)
            View.ShowInfo(message, KntConst.AppName);
        else
            NotifyMessage(message);
    }

    #endregion

    #region View

    IViewChatRoom _chatView;
    protected IViewChatRoom ChatView
    {
        get
        {
            if (_chatView == null)
                _chatView = Store.FactoryViews.Registry.Resolve<KntChatCtrl, IViewChatRoom>(this);
            return _chatView;
        }
    }

    // The view, only if it was already created: the hub's events never create it (a script's ctrl has none).
    private IViewChatRoom View => _chatView;

    public void ShowChatView(bool autoCloseCtrlOnViewExit)
    {
        // Refused by Run() (CheckPreconditions): already finalized, nothing to show.
        if (!PreconditionsMet)
            return;

        AutoCloseCtrlOnViewExit = autoCloseCtrlOnViewExit;
        ChatView.ShowView();
    }

    // For use in KntScript
    public void ShowChatView()
    {
        if (!PreconditionsMet)
            return;

        if (ControllerState == EControllerState.Started)
        {
            ChatView.ShowView();
        }
        else
        {
            ChatView.ShowInfo("KntChat controller is no started.");
        }
    }

    public void VisibleView(bool visible)
    {
        if (!PreconditionsMet)
            return;

        ChatView.VisibleView(visible);
    }

    #endregion

    #region IDisposable

    public override async void Dispose()
    {
        var connection = _connection;
        _connection = null;
        if (connection != null)
            await connection.DisposeAsync();

        base.Dispose();
    }

    #endregion
}
