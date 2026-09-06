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
            if (string.IsNullOrEmpty(Store.AppConfig.ChatHubUrl))
            {
                var res = new Result<EControllerResult>(EControllerResult.Error);
                var message = "Chat hub url is not defined. Set the chat hub url y Options menú.";
                res.AddErrorMessage(message);
                return res;
            }

            _hubConnection = new HubConnectionBuilder()
                           .WithUrl(Store.AppConfig.ChatHubUrl)
                           .Build();

            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var encodeMessage = $"{user}: {message}";
                ReceiveMessage?.Invoke(this, new ControllerEventArgs<string>(encodeMessage));
            });

            _hubConnection.Closed += OnHubConnectionClosedAsync;

            // Fire-and-forget: connecting must never block startup. Relies on the WinForms
            // SynchronizationContext (captured here, on the UI thread) to bring the continuation
            // - including HandleConnectionFailure - back to the UI thread, same as the rest of
            // this codebase's async/await usage.
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
    // and re-enable auto-connect (AppConfig.ChatHubAutoConnectDisabled) without restarting the app.
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

    private async Task OnHubConnectionClosedAsync(Exception error)
    {
        try
        {
            await Task.Delay(5000);
            using var cts = new CancellationTokenSource(ConnectTimeout);
            await _hubConnection.StartAsync(cts.Token);
            HandleConnectionSuccess();
        }
        catch (Exception ex)
        {
            HandleConnectionFailure(ex);
        }
    }

    // A connection that succeeds - whether from the automatic startup attempt or from the user
    // manually reopening chat - proves the url works again, so any previous auto-disable no longer
    // applies.
    private void HandleConnectionSuccess()
    {
        if (Store.AppConfig.ChatHubAutoConnectDisabled)
        {
            Store.AppConfig.ChatHubAutoConnectDisabled = false;
            Store.SaveConfig();
        }
    }

    // Startup (ShowErrorMessagesOnInitialize == false) must never freeze or interrupt the user
    // again for a chat hub that is known to be unreachable: the failure is reported through the
    // non-blocking notification channel and auto-connect is disabled in AppConfig so the next
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
            Store.AppConfig.ChatHubAutoConnectDisabled = true;
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
