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

    private HubConnection _hubConnection;

    #endregion

    #region Constructor

    public KntChatForm(KntChatComponent com)
    {
        InitializeComponent();

        _com = com;
    }

    #endregion

    #region IView implementation

    public void ShowView()
    {
        Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        return _com.DialogResultToComponentResult(ShowDialog());
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

    #region Form events handlers

    private void ChatForm_Load(object sender, EventArgs e)
    {
        _com.ReceiveMessage += _com_ReceiveMessage;

        Text += $" [{_com.Store.AppUserName}]";
        labelServer.Text = _com.Store.AppConfig.ChatHubUrl;
    }

    private async void buttonSend_Click(object sender, EventArgs e)
    {
        await _com.SendMessageAsync(textMessage.Text);        
        textMessage.Text = "";
    }

    private void KntChatForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _com.Finalize();
    }

    #endregion

    #region Private methods

    private void _com_ReceiveMessage(object sender, ComponentEventArgs<string> e)
    {
        listMessages.Items.Add(e.Entity.ToString());
        listMessages.Refresh();
    }

    #endregion 
}
