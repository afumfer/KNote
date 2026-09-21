using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace KNote.ClientWin.Controllers;

public class KntChatCtrl : CtrlBase, IDisposable
{
    #region Private fields

    private HubConnection _hubConnection;

    // Bounds how long a connection attempt can block the caller before giving up - an unreachable
    // ChatHubUrl must never be allowed to hang the app (see HandleConnectionFailure).
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(8);

    // SignalR raises hub callbacks and the Closed event on thread-pool threads. Everything that ends
    // up touching the UI (ReceiveMessage subscribers, error notifications) is posted to this context,
    // captured in OnInitialized (always the UI thread). Internal so tests can substitute it.
    internal SynchronizationContext UiContext { get; set; }

    #endregion

    #region Properties 

    public bool AutoCloseCtrlOnViewExit { get; set; } = false;
    public bool ShowErrorMessagesOnInitialize { get; set; } = false;
    public string Tag { get; set; } = "KntChatCtrl v 0.1";

    #endregion

    #region Constructor

    public KntChatCtrl(Store store) : base(store)
    {
        ControllerName = "KntChat controller";
    }

    #endregion

    #region Events 

    public event EventHandler<ControllerEventArgs<string>> ReceiveMessage;

    #endregion 

    #region Protected methods 

    protected override Result<EControllerResult> OnInitialized()
    {
        try
        {
            if (string.IsNullOrEmpty(Store.Settings.Connectivity.ChatHub.Url))
            {
                var res = new Result<EControllerResult>(EControllerResult.Error);
                var message = "Chat hub url is not defined. Set the chat hub url in the Options menu.";
                res.AddErrorMessage(message);

                // Opened manually by the user: tell them why nothing happens. At startup the caller
                // already skips the chat when the url is empty, so this stays silent there.
                if (ShowErrorMessagesOnInitialize)
                    ChatView.ShowInfo(message, KntConst.AppName);

                return res;
            }

            UiContext = SynchronizationContext.Current;

            _hubConnection = new HubConnectionBuilder()
                           .WithUrl(Store.Settings.Connectivity.ChatHub.Url)
                           .Build();

            _hubConnection.On<string, string>("ReceiveMessage", DispatchReceivedMessage);

            _hubConnection.Closed += OnHubConnectionClosedAsync;

            // Fire-and-forget: connecting must never block startup. The await continuations of
            // ConnectWithTimeoutAsync - including HandleConnectionFailure - return to the UI thread
            // through the WinForms SynchronizationContext captured here.
            _ = ConnectWithTimeoutAsync();

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

    public async Task SendMessageAsync(string message)
    {
        try
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
                await _hubConnection.StartAsync();

            if (_hubConnection.State == HubConnectionState.Connected)
                await _hubConnection.SendAsync("SendMessage", Store.AppUserName, message);

        }
        catch (Exception ex)
        {
            ChatView.ShowInfo($"The connection could not be started. Error: {ex.Message}", KntConst.AppName);
        }
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

    // Standalone connection attempt against a candidate url, bounded by the same timeout used at
    // startup. Used by OptionsEditorForm's "Test connection" button so the user can verify a fix
    // and re-enable auto-connect (State.Session.ChatHubAutoConnectDisabled) without restarting the app.
    public static async Task<Result> TestConnectionAsync(string url)
    {
        var result = new Result();
        HubConnection connection = null;
        try
        {
            connection = new HubConnectionBuilder().WithUrl(url).Build();
            using var cts = new CancellationTokenSource(ConnectTimeout);
            await connection.StartAsync(cts.Token);
        }
        catch (Exception ex)
        {
            result.AddErrorMessage(ex.Message);
        }
        finally
        {
            if (connection != null)
                await connection.DisposeAsync();
        }
        return result;
    }

    #endregion

    #region Private methods

    // Runs on a thread-pool thread (SignalR callback): hand the event over to the UI thread so every
    // ReceiveMessage subscriber can safely touch views.
    internal void DispatchReceivedMessage(string user, string message)
    {
        var encodeMessage = $"{user}: {message}";
        PostToUi(() => ReceiveMessage?.Invoke(this, new ControllerEventArgs<string>(encodeMessage)));
    }

    private void PostToUi(Action action)
    {
        if (UiContext != null)
            UiContext.Post(_ => action(), null);
        else
            action();
    }

    private async Task ConnectWithTimeoutAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(ConnectTimeout);
            await _hubConnection.StartAsync(cts.Token);
            HandleConnectionSuccess();
        }
        catch (Exception ex)
        {
            HandleConnectionFailure(ex);
        }
    }

    // Raised on a thread-pool thread, and the awaits below do not return to the UI thread: the
    // outcome handlers touch views and config, so they are posted explicitly.
    private async Task OnHubConnectionClosedAsync(Exception error)
    {
        try
        {
            await Task.Delay(5000);
            using var cts = new CancellationTokenSource(ConnectTimeout);
            await _hubConnection.StartAsync(cts.Token);
            PostToUi(HandleConnectionSuccess);
        }
        catch (Exception ex)
        {
            PostToUi(() => HandleConnectionFailure(ex));
        }
    }

    // A connection that succeeds - whether from the automatic startup attempt or from the user
    // manually reopening chat - proves the url works again, so any previous auto-disable no longer
    // applies.
    private void HandleConnectionSuccess()
    {
        if (Store.State.Session.ChatHubAutoConnectDisabled)
        {
            Store.State.Session.ChatHubAutoConnectDisabled = false;
            Store.SaveConfig();
        }
    }

    // Startup (ShowErrorMessagesOnInitialize == false) must never freeze or interrupt the user
    // again for a chat hub that is known to be unreachable: the failure is reported through the
    // non-blocking notification channel and auto-connect is disabled in the state so the next
    // startup does not retry it - the user re-enables it from Options once the url is fixed (see
    // OptionsEditorForm's "Test connection" button, which clears ChatHubAutoConnectDisabled on
    // success). Opening the chat manually (ShowErrorMessagesOnInitialize == true) keeps showing
    // the error directly, since the user is actively waiting on that action.
    private void HandleConnectionFailure(Exception ex)
    {
        var resMessage = $"KntChat controller. The connection could not be started. Error: {ex.Message}.";

        if (ShowErrorMessagesOnInitialize)
        {
            ChatView.ShowInfo(resMessage, KntConst.AppName);
        }
        else
        {
            Store.State.Session.ChatHubAutoConnectDisabled = true;
            Store.SaveConfig();

            NotifyMessage($"{resMessage} Chat auto-connect has been disabled; fix the chat hub url and test it from Options to re-enable it.");
        }
    }

    #endregion

    #region View

    IViewChat _chatView;
    protected IViewChat ChatView
    {
        get
        {
            if (_chatView == null)
                _chatView = Store.FactoryViews.Registry.Resolve<KntChatCtrl, IViewChat>(this);
            return _chatView;
        }
    }

    public void ShowChatView(bool autoCloseCtrlOnViewExit)
    {
        AutoCloseCtrlOnViewExit = autoCloseCtrlOnViewExit;        
        ChatView.ShowView();
    }

    // For use in KntScript
    public void ShowChatView()
    {
        
        if (ControllerState == EControllerState.Started)
        {
            ChatView.ShowView();
        }
        else
        {
            ChatView.ShowInfo("KntChat controller is no started.");
        }
    }

    public void VisibleView (bool visible)
    {
        ChatView.VisibleView(visible);
    }

    #endregion 

    #region IDisposable

    public override async void Dispose()
    {
        if (_hubConnection != null)
            await _hubConnection.DisposeAsync();

        base.Dispose();
    }

    #endregion 
}
