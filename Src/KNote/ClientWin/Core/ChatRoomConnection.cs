using Microsoft.AspNetCore.SignalR.Client;

namespace KNote.ClientWin.Core;

public enum ChatRoomConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    // The Server refused the sign-in (expired token, or the user is not Staff there).
    NotAuthorized
}

/// <summary>
/// The connection of KntChatCtrl to the chat room hub (ChatRoomHubContract): what the ctrl uses of a SignalR
/// HubConnection, so its tests can give it a fake one. Its callbacks and events are raised on thread-pool threads.
/// </summary>
public interface IChatRoomConnection : IAsyncDisposable
{
    bool IsConnected { get; }

    Task StartAsync(CancellationToken cancellationToken);

    Task<TResult> InvokeAsync<TResult>(string method, params object[] args);

    void On<T>(string method, Action<T> handler);

    event Func<Exception, Task> Reconnecting;

    event Func<string, Task> Reconnected;

    event Func<Exception, Task> Closed;
}

/// <summary>The SignalR connection to the hub at url, with the token given (read on every connection attempt).</summary>
public sealed class ChatRoomConnection : IChatRoomConnection
{
    private readonly HubConnection _connection;

    public ChatRoomConnection(string url, Func<string> token)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(url, options => options.AccessTokenProvider = () => Task.FromResult(token()))
            .WithAutomaticReconnect()
            .Build();
    }

    public bool IsConnected => _connection.State == HubConnectionState.Connected;

    public Task StartAsync(CancellationToken cancellationToken) => _connection.StartAsync(cancellationToken);

    public Task<TResult> InvokeAsync<TResult>(string method, params object[] args) =>
        _connection.InvokeCoreAsync<TResult>(method, args);

    public void On<T>(string method, Action<T> handler) => _connection.On(method, handler);

    public event Func<Exception, Task> Reconnecting
    {
        add => _connection.Reconnecting += value;
        remove => _connection.Reconnecting -= value;
    }

    public event Func<string, Task> Reconnected
    {
        add => _connection.Reconnected += value;
        remove => _connection.Reconnected -= value;
    }

    public event Func<Exception, Task> Closed
    {
        add => _connection.Closed += value;
        remove => _connection.Closed -= value;
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
