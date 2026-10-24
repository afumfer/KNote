using KNote.ClientWin.Core;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// IChatRoomConnection test double for KntChatCtrl: StartAsync and the hub calls answer with what the test configures
/// (StartAsyncImpl, InvokeImpl), the calls are recorded, and the test raises the hub's callbacks with Raise.
/// </summary>
internal class FakeChatRoomConnection : IChatRoomConnection
{
    private readonly Dictionary<string, Action<object>> _handlers = new();

    public bool IsConnected { get; set; }

    public Func<Task>? StartAsyncImpl { get; set; }

    // (method, args) -> the Result the hub answers.
    public Func<string, object[], object>? InvokeImpl { get; set; }

    public List<(string Method, object[] Args)> Invocations { get; } = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var start = StartAsyncImpl?.Invoke() ?? Task.CompletedTask;
        return start.ContinueWith(t =>
        {
            t.GetAwaiter().GetResult();
            IsConnected = true;
        }, TaskContinuationOptions.ExecuteSynchronously);
    }

    public Task<TResult> InvokeAsync<TResult>(string method, params object[] args)
    {
        Invocations.Add((method, args));
        var impl = InvokeImpl ?? throw new NotSupportedException($"{nameof(InvokeAsync)} not configured for this test");
        return Task.FromResult((TResult)impl(method, args));
    }

    public void On<T>(string method, Action<T> handler) => _handlers[method] = value => handler((T)value);

    // A callback of the hub, as SignalR would raise it.
    public void Raise(string method, object value) => _handlers[method](value);

    public event Func<Exception, Task>? Reconnecting;
    public event Func<string, Task>? Reconnected;
    public event Func<Exception, Task>? Closed;

    public Task RaiseReconnectedAsync() => Reconnected?.Invoke("new-connection") ?? Task.CompletedTask;

    public bool Disposed { get; private set; }

    public bool HasClosedHandler => Closed != null;

    public ValueTask DisposeAsync()
    {
        Disposed = true;
        return ValueTask.CompletedTask;
    }
}
