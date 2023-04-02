using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.AspNetCore.SignalR.Client;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Components;

public class KntChatComponent : ComponentBase
{
    private HubConnection _hubConnection;
    private bool _firstStartHubConnection = false;

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
        if (string.IsNullOrEmpty(Store.AppConfig.ChatHubUrl))
        {
            ChatView.ShowInfo("Chat hub url is not defined. Set the chat hub url y Options menú.", "KaNote");
            return new Result<EComponentResult>(EComponentResult.Error);
        }

        try
        {
            _hubConnection = new HubConnectionBuilder()
                           .WithUrl(Store.AppConfig.ChatHubUrl)
                           .Build();

            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var encodeMessage = $"{user}: {message}";
                ReceiveMessage?.Invoke(this, new ComponentEventArgs<string>(encodeMessage));
            });
        
            return new Result<EComponentResult>(EComponentResult.Executed);
        }
        catch (Exception ex)
        {            
            ChatView.ShowInfo($"The connection could not be started. Error: {ex.Message}", "KaNote");
            return new Result<EComponentResult>(EComponentResult.Error);
        }
    }

    #endregion

    #region Public Methods

    public async Task StartHubConnectionAsync()
    {
        // Is disconected => must conected
        if (!_firstStartHubConnection)
        {
            _hubConnection.Closed += async (error) =>
            {
                Thread.Sleep(5000);
                await _hubConnection.StartAsync();            
            };

        }
        _firstStartHubConnection = true;
        await _hubConnection.StartAsync();
    }

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
            ChatView.ShowInfo($"The connection could not be started. Error: {ex.Message}", "KaNote");
        }
    }

    // --------------------------------------------------------------------------
    // Warning: this method can cause a deadlock in single-threaded environments
    // (for example, Windows Forms or WPF applications) or ASP.NET applications.
    // It is recommended to use the asynchronous version of this method.
    // Use only in KntScript
    public void StartHubConnection()
    {
        Task.Run(() => StartHubConnectionAsync()).Wait();
    }

    public void SendMessage(string message)
    {
        Task.Run(() => SendMessageAsync(message)).Wait();
    }
    // --------------------------------------------------------------------------

    #endregion 

    #region IViewBase

    IViewBase _chatView;
    protected IViewBase ChatView
    {
        get
        {
            if (_chatView == null)
                _chatView = Store.FactoryViews.View(this);
            return _chatView;
        }
    }

    public void ShowChatView()
    {
        ChatView.ShowView();
    }

    #endregion 


}
