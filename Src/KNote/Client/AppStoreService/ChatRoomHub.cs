using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.AspNetCore.SignalR.Client;

namespace KNote.Client.AppStoreService;

public enum ChatRoomConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    // The Server refused the user (the chat room is for Staff users).
    NotAuthorized
}

/// <summary>
/// The chat room as the Web client sees it: the connection to the Server's hub (ChatRoomHubContract) with the user's
/// token, the threads, the ones followed in this connection (subscribed, with their messages) and the one shown.
/// Started by MainLayout for a Staff user once signed in (and stopped when signing out), so the threads the user takes
/// part in are followed while the app is open: their new messages are notified (unless that thread is on screen)
/// and counted as unread. The chat room's own window (ChatRoomWindow) starts it too: the window layout doesn't. Subscriptions are lost with the connection: they are made again after reconnecting. Its
/// changes are told with Changed; the hub's callbacks run on the browser's only thread.
/// </summary>
public class ChatRoomHub : IAsyncDisposable
{
    private readonly AppState _appState;
    private readonly HttpClient _httpClient;
    private readonly Uri _hubUri;
    private HubConnection? _connection;
    private string? _connectedUserName;
    private string? _connectedRepositoryId;

    // The threads followed in this connection, with their messages.
    private readonly Dictionary<Guid, ChatRoomThreadDto> _followed = new();
    private readonly Dictionary<Guid, int> _unread = new();

    public ChatRoomHub(AppState appState, HttpClient httpClient, Uri hubUri)
    {
        _appState = appState;
        _httpClient = httpClient;
        _hubUri = hubUri;
    }

    #region State

    public ChatRoomConnectionState State { get; private set; } = ChatRoomConnectionState.Disconnected;

    // Why the last connection attempt failed.
    public string? ConnectionError { get; private set; }

    public string? UserName => _connectedUserName;

    // Every thread, most recent activity first.
    public List<ChatRoomThreadInfoDto> Threads { get; private set; } = new();

    public Guid? CurrentThreadId { get; private set; }

    public ChatRoomThreadDto? CurrentThread =>
        CurrentThreadId != null && _followed.TryGetValue(CurrentThreadId.Value, out var thread) ? thread : null;

    // The chat room page is on screen: the messages of the current thread are seen, not notified.
    public bool PageVisible { get; set; }

    public bool IsFollowed(Guid threadId) => _followed.ContainsKey(threadId);

    public int UnreadCount(Guid threadId) => _unread.TryGetValue(threadId, out var count) ? count : 0;

    public event Action? Changed;

    #endregion

    #region Connection

    /// <summary>Connects as the user, if they are signed in and a Staff user (the chat room is theirs); otherwise
    /// disconnects. The chat room is the one of the session's repository (the token's).</summary>
    public async Task StartForUserAsync(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(user.Identity.Name)
            && (user.IsInRole("Staff") || user.IsInRole("ProjectManager") || user.IsInRole("Admin")))
            await StartAsync(user.Identity.Name, user.FindFirst(KntConst.RepositoryClaimType)?.Value);
        else
            await StopAsync();
    }

    /// <summary>Connects as the signed-in user in the session's repository (again, if it was connected as another one,
    /// or in another repository).</summary>
    public async Task StartAsync(string userName, string? repositoryId)
    {
        if (_connection != null && _connectedUserName == userName && _connectedRepositoryId == repositoryId
            && State != ChatRoomConnectionState.Disconnected)
            return;

        await StopAsync();

        _connectedUserName = userName;
        _connectedRepositoryId = repositoryId;
        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUri, options =>
                options.AccessTokenProvider = () => Task.FromResult(_httpClient.DefaultRequestHeaders.Authorization?.Parameter))
            .WithAutomaticReconnect()
            .Build();

        _connection.On<ChatRoomMessageDto>(ChatRoomHubContract.MessageReceived, OnMessageReceived);
        _connection.On<ChatRoomThreadInfoDto>(ChatRoomHubContract.ThreadCreated, OnThreadCreated);
        _connection.On<ChatRoomThreadActivityDto>(ChatRoomHubContract.ThreadActivity, OnThreadActivity);
        _connection.On<ChatRoomOnlineUsersDto>(ChatRoomHubContract.OnlineUsersChanged, OnOnlineUsersChanged);
        _connection.Reconnecting += _ => SetStateAsync(ChatRoomConnectionState.Reconnecting);
        _connection.Reconnected += async _ => await OnConnectedAsync();
        _connection.Closed += _ => SetStateAsync(ChatRoomConnectionState.Disconnected);

        await ConnectAsync();
    }

    /// <summary>Connects again after the connection was lost (automatic reconnection gives up after a while).</summary>
    public async Task ReconnectAsync()
    {
        if (_connection == null || State != ChatRoomConnectionState.Disconnected)
            return;
        await ConnectAsync();
    }

    public async Task StopAsync()
    {
        var connection = _connection;
        _connection = null;
        _connectedUserName = null;
        _connectedRepositoryId = null;
        _followed.Clear();
        _unread.Clear();
        Threads = new();
        CurrentThreadId = null;
        ConnectionError = null;
        State = ChatRoomConnectionState.Disconnected;

        if (connection != null)
            await connection.DisposeAsync();
        NotifyChanged();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
            await _connection.DisposeAsync();
    }

    private async Task ConnectAsync()
    {
        if (_connection == null)
            return;

        ConnectionError = null;
        await SetStateAsync(ChatRoomConnectionState.Connecting);
        try
        {
            await _connection.StartAsync();
            await OnConnectedAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            ConnectionError = "The chat room is for Staff users.";
            await SetStateAsync(ChatRoomConnectionState.NotAuthorized);
        }
        catch (Exception ex)
        {
            ConnectionError = ex.Message;
            await SetStateAsync(ChatRoomConnectionState.Disconnected);
        }
    }

    // Connected, or reconnected (a new connection: its subscriptions are made again, and the threads read again, as
    // messages may have been missed meanwhile). The threads the user takes part in are followed too.
    private async Task OnConnectedAsync()
    {
        State = ChatRoomConnectionState.Connected;

        var resThreads = await InvokeAsync<List<ChatRoomThreadInfoDto>>(ChatRoomHubContract.GetThreads);
        if (resThreads != null)
            Threads = resThreads;

        var toFollow = _followed.Keys
            .Union(Threads.Where(t => t.IsParticipant).Select(t => t.ThreadId))
            .ToList();
        foreach (var threadId in toFollow)
            await FollowAsync(threadId);

        NotifyChanged();
    }

    private Task SetStateAsync(ChatRoomConnectionState state)
    {
        State = state;
        NotifyChanged();
        return Task.CompletedTask;
    }

    #endregion

    #region Threads and messages

    // The thread chosen last: the answer for a thread chosen before, if slower, doesn't take its place.
    private int _selection;
    // The threads being followed (their Subscribe on its way): a thread chosen twice meanwhile is followed once.
    private readonly Dictionary<Guid, Task<bool>> _following = new();

    /// <summary>Shows the thread (following it, with its messages, if it wasn't). False if it can't be read, or if
    /// another thread was chosen meanwhile (that one is shown; this one stays followed).</summary>
    public async Task<bool> SelectThreadAsync(Guid threadId)
    {
        var selection = ++_selection;
        if (!_followed.ContainsKey(threadId) && !await FollowOnceAsync(threadId))
            return false;
        if (selection != _selection)
            return false;

        CurrentThreadId = threadId;
        _unread.Remove(threadId);
        NotifyChanged();
        return true;
    }

    /// <summary>Stops following the thread in this connection (its new messages are no longer received).</summary>
    public async Task UnfollowAsync(Guid threadId)
    {
        if (!_followed.Remove(threadId))
            return;

        if (CurrentThreadId == threadId)
            CurrentThreadId = null;
        await InvokeAsync(ChatRoomHubContract.Unsubscribe, threadId);
        NotifyChanged();
    }

    /// <summary>A new thread, then shown. Null if it couldn't be created (already notified).</summary>
    public async Task<ChatRoomThreadInfoDto?> CreateThreadAsync(string topic)
    {
        var thread = await InvokeAsync<ChatRoomThreadInfoDto>(ChatRoomHubContract.CreateThread, topic);
        if (thread == null)
            return null;

        // ThreadCreated may have arrived first, without IsParticipant (the same for everybody).
        Threads.RemoveAll(t => t.ThreadId == thread.ThreadId);
        Threads.Insert(0, thread);
        await SelectThreadAsync(thread.ThreadId);
        return thread;
    }

    /// <summary>Writes in the thread shown. False if it couldn't be sent (already notified).</summary>
    public async Task<bool> SendAsync(string text)
    {
        if (CurrentThreadId == null || string.IsNullOrWhiteSpace(text))
            return false;

        var threadId = CurrentThreadId.Value;
        var message = await InvokeAsync<ChatRoomMessageDto>(ChatRoomHubContract.SendMessage, threadId, text);
        if (message == null)
            return false;

        // The message arrives with MessageReceived too (the thread is followed); this covers a reconnection between
        // both, and makes the user a participant.
        AddMessage(message);
        var info = Threads.FirstOrDefault(t => t.ThreadId == threadId);
        if (info != null)
            info.IsParticipant = true;
        NotifyChanged();
        return true;
    }

    /// <summary>
    /// Sends an image to the thread shown: kept by the Server as a resource of the thread (REST, not the hub) and then
    /// sent as a message with its Markdown. False if it couldn't be sent (already notified).
    /// </summary>
    public async Task<bool> SendImageAsync(string fileName, string contentType, string contentBase64)
    {
        if (CurrentThreadId is not Guid threadId)
            return false;

        Result<ChatRoomImageDto>? res;
        try
        {
            var httpRes = await _httpClient.PostAsJsonAsync(ChatRoomHubContract.ImagesPath(threadId), new ChatRoomImageDto
            {
                FileName = fileName,
                ContentType = contentType,
                ContentBase64 = contentBase64
            });
            // The Server answers with a Result, also when it refuses the image; not when the request is refused (403).
            res = await ReadResultAsync(httpRes);
            if (res == null)
            {
                _appState.NotifyError("Chat room", $"The image could not be sent ({(int)httpRes.StatusCode} {httpRes.ReasonPhrase}).");
                return false;
            }
        }
        catch (Exception ex)
        {
            _appState.NotifyError("Chat room", $"The image could not be sent: {ex.Message}");
            return false;
        }

        if (res?.IsValid != true)
        {
            _appState.NotifyError("Chat room", res?.ErrorMessage ?? "The image could not be sent.");
            return false;
        }

        return await SendAsync(res.Entity.Markdown);
    }

    private static async Task<Result<ChatRoomImageDto>?> ReadResultAsync(HttpResponseMessage httpRes)
    {
        try
        {
            return await httpRes.Content.ReadFromJsonAsync<Result<ChatRoomImageDto>>();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<bool> FollowOnceAsync(Guid threadId)
    {
        if (!_following.TryGetValue(threadId, out var following))
        {
            following = FollowAsync(threadId);
            _following[threadId] = following;
        }

        try
        {
            return await following;
        }
        finally
        {
            _following.Remove(threadId);
        }
    }

    private async Task<bool> FollowAsync(Guid threadId)
    {
        var thread = await InvokeAsync<ChatRoomThreadDto>(ChatRoomHubContract.Subscribe, threadId);
        if (thread == null)
        {
            _followed.Remove(threadId);
            return false;
        }

        _followed[threadId] = thread;
        return true;
    }

    private void OnMessageReceived(ChatRoomMessageDto message)
    {
        if (!AddMessage(message))
            return;

        TouchThread(message.ThreadId, message.SentDateTime);

        if (message.UserName != _connectedUserName && !IsOnScreen(message.ThreadId))
        {
            _unread[message.ThreadId] = UnreadCount(message.ThreadId) + 1;
            var topic = Threads.FirstOrDefault(t => t.ThreadId == message.ThreadId)?.Topic ?? "Chat room";
            _appState.NotifySuccess($"Chat room - {topic}", $"{message.UserName}: {Shorten(message.Text)}");
        }
        NotifyChanged();
    }

    private void OnThreadCreated(ChatRoomThreadInfoDto thread)
    {
        if (Threads.Any(t => t.ThreadId == thread.ThreadId))
            return;
        Threads.Insert(0, thread);
        NotifyChanged();
    }

    // A message in any thread: the followed ones get it with MessageReceived; the others are marked as unread.
    private void OnThreadActivity(ChatRoomThreadActivityDto activity)
    {
        TouchThread(activity.ThreadId, activity.ModificationDateTime);
        if (!_followed.ContainsKey(activity.ThreadId) && activity.UserName != _connectedUserName)
            _unread[activity.ThreadId] = UnreadCount(activity.ThreadId) + 1;
        NotifyChanged();
    }

    private void OnOnlineUsersChanged(ChatRoomOnlineUsersDto online)
    {
        if (!_followed.TryGetValue(online.ThreadId, out var thread))
            return;
        thread.OnlineUserNames = online.UserNames;
        foreach (var userName in online.UserNames.Where(u => !thread.Participants.Contains(u)))
            thread.Participants.Add(userName);
        NotifyChanged();
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

    private bool IsOnScreen(Guid threadId) => PageVisible && CurrentThreadId == threadId;

    private static string Shorten(string text)
    {
        var line = (text ?? "").ReplaceLineEndings(" ");
        return line.Length <= 120 ? line : line[..119] + "…";
    }

    #endregion

    #region Hub calls

    // The entity of the call's result, or null when it fails (notified, as the Web API services do).
    private async Task<TEntity?> InvokeAsync<TEntity>(string method, params object[] args) where TEntity : class
    {
        var res = await InvokeResultAsync<Result<TEntity>>(method, args);
        return res?.IsValid == true ? res.Entity : null;
    }

    private async Task InvokeAsync(string method, params object[] args) =>
        await InvokeResultAsync<Result>(method, args);

    private async Task<TResult?> InvokeResultAsync<TResult>(string method, object[] args) where TResult : Result
    {
        if (_connection == null || _connection.State != HubConnectionState.Connected)
        {
            _appState.NotifyError("Chat room", "Not connected to the chat room.");
            return null;
        }

        try
        {
            var res = await _connection.InvokeCoreAsync<TResult>(method, args);
            if (!res.IsValid)
                _appState.NotifyError("Chat room", res.ErrorMessage);
            return res;
        }
        catch (Exception ex)
        {
            _appState.NotifyError("Chat room", ex.Message);
            return null;
        }
    }

    private void NotifyChanged() => Changed?.Invoke();

    #endregion
}
