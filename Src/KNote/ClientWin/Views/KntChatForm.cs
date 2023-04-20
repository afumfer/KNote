using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace KNote.ClientWin.Views;

public partial class KntChatForm : Form, IViewChat
{
    #region Private fields

    private readonly KntChatComponent _com;
    private bool _viewFinalized = false;

    #endregion

    #region Constructor

    public KntChatForm(KntChatComponent com)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

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

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void RefreshView()
    {
        this.Refresh();
    }

    public void VisibleView(bool visible)
    {
        if (visible)        
            Show();        
        else        
            Hide();
                
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
        {
            if (_com.AutoCloseComponentOnViewExit)
                _com.Finalize();
            else
            {
                Hide();
                e.Cancel = true;
            }
        }
    }

    private void _com_ReceiveMessage(object sender, ComponentEventArgs<string> e)
    {
        if (listMessages.InvokeRequired)
        {
            listMessages.Invoke(new MethodInvoker(delegate
            {
                listMessages.Items.Add(e.Entity.ToString());
                listMessages.Refresh();
            }));
        }
        else
        {
            listMessages.Items.Add(e.Entity.ToString());
            listMessages.Refresh();
        }
    }

    #endregion
}
