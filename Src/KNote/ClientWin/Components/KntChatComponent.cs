using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace KNote.ClientWin.Components;

public class KntChatComponent : ComponentBase, IDisposable
{
    #region Private fields

    private HubConnection _hubConnection;

    #endregion

    #region Properties 

    public bool AutoCloseComponentOnViewExit { get; set; } = false;
    public bool ShowErrorMessagesOnInitialize { get; set; } = false;
    public string Tag { get; set; } = "KntChatComponent v 0.1";

    #endregion

    #region Constructor

    public KntChatComponent(Store store) : base(store)
    {
        ComponentName = "KntChat Component";
    }

    #endregion

    #region Protected methods 

    public event EventHandler<ComponentEventArgs<string>> ReceiveMessage;
    protected override Result<EComponentResult> OnInitialized()
    {
        try
        {
            if (string.IsNullOrEmpty(Store.AppConfig.ChatHubUrl))
            {
                var res = new Result<EComponentResult>(EComponentResult.Error);
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
                ReceiveMessage?.Invoke(this, new ComponentEventArgs<string>(encodeMessage));
            });

            StartHubConnection();

            return new Result<EComponentResult>(EComponentResult.Executed);
        }
        catch (Exception ex)
        {
            var res = new Result<EComponentResult>(EComponentResult.Error);
            var resMessage = $"KntChat component. The connection could not be started. Error: {ex.Message}.";
            res.AddErrorMessage(resMessage);
            if(ShowErrorMessagesOnInitialize)
                ChatView.ShowInfo(resMessage, KntConst.AppName);
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

    #endregion

    #region Private methods

    private async Task StartHubConnectionAsync()
    {
        _hubConnection.Closed += async (error) =>
        {
            Thread.Sleep(5000);
            await _hubConnection.StartAsync();
        };

        await _hubConnection.StartAsync();
    }

    // --------------------------------------------------------------------------
    // Warning: this method can cause a deadlock in single-threaded environments
    // (for example, Windows Forms or WPF applications) or ASP.NET applications.
    // It is recommended to use the asynchronous version of this method.
    // Use only in KntScript
    private void StartHubConnection()
    {
        Task.Run(() => StartHubConnectionAsync()).Wait();
    }  
    // --------------------------------------------------------------------------

    #endregion 

    #region IViewBase

    IViewChat _chatView;
    protected IViewChat ChatView
    {
        get
        {
            if (_chatView == null)
                _chatView = Store.FactoryViews.View(this);
            return _chatView;
        }
    }

    public void ShowChatView(bool autoCloseComponentOnViewExit)
    {
        AutoCloseComponentOnViewExit = autoCloseComponentOnViewExit;        
        ChatView.ShowView();
    }

    public void ShowChatView()
    {
        
        if (ComponentState == EComponentState.Started)
        {
            ChatView.ShowView();
        }
        else
        {
            ChatView.ShowInfo("KntChat component is no started.");
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
