using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace KNote.ClientWin.Views;

public partial class KntChatForm : Form, IViewBase
{
    #region Private fields

    private readonly KntChatComponent _com;
    private bool _viewFinalized = false;

    private HubConnection hubConnection;    

    #endregion

    #region Constructor

    public KntChatForm(KntChatComponent com)
    {
        InitializeComponent();

        _com = com;
    }

    #endregion

    #region Form events handlers

    private async void ChatForm_Load(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_com.Store.AppConfig.ChatHubUrl))
        {
            MessageBox.Show("Chat hub url is not defined. Set the chat hub url y Options menú.", "KaNote");
            this.Close();
            return;
        }

        try
        {
            hubConnection = new HubConnectionBuilder()
                           .WithUrl(_com.Store.AppConfig.ChatHubUrl)
                           .Build();

            // Is disconected => must conected
            hubConnection.Closed += async (error) =>
            {
                Thread.Sleep(5000);
                await hubConnection.StartAsync();
            };

            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var encodeMessage = $"{user}: {message}";
                listMessages.Items.Add(encodeMessage);
                listMessages.Refresh();
            });

            Text += $" [{_com.Store.AppUserName}]";
            labelServer.Text = _com.Store.AppConfig.ChatHubUrl;

            await hubConnection.StartAsync();
        }
        catch (Exception)
        {
            listMessages.Items.Add($"The connection could not be started.");
        }
    }

    private async void buttonSend_Click(object sender, EventArgs e)
    {
        try
        {
            UseWaitCursor = true;
            if (hubConnection.State == HubConnectionState.Disconnected)
                await hubConnection.StartAsync();

            if (hubConnection.State == HubConnectionState.Connected)
                await hubConnection.SendAsync("SendMessage", _com.Store.AppUserName, textMessage.Text);

            textMessage.Text = "";
        }
        catch (Exception ex)
        {
            listMessages.Items.Add($"The message coundn't be sent. Error: {ex.Message}");
            UseWaitCursor = false;
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void KntChatForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _com.Finalize();
    }

    #endregion

    #region IView implementation

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        return _com.DialogResultToComponentResult(this.ShowDialog());
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show("KaNote", caption, buttons, icon);
    }

    public void RefreshView()
    {
        throw new NotImplementedException();
    }

    #endregion

}
