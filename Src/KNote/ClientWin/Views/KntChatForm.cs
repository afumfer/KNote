using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class KntChatForm : KntForm, IViewChat
{
    #region Private fields

    private readonly KntChatCtrl _ctrl;

    #endregion

    #region Constructor

    public KntChatForm(KntChatCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IView implementation

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
        _ctrl.ReceiveMessage += _com_ReceiveMessage;

        Text += $" [{_ctrl.Store.AppUserName}]";
        labelServer.Text = _ctrl.Store.AppConfig.ChatHubUrl;
    }

    private async void buttonSend_Click(object sender, EventArgs e)
    {
        await _ctrl.SendMessageAsync(textMessage.Text);
        textMessage.Text = "";
    }

    protected override void OnUserClosing(FormClosingEventArgs e)
    {
        if (_ctrl.AutoCloseCtrlOnViewExit)
            _ctrl.Finalize();
        else
        {
            Hide();
            e.Cancel = true;
        }
    }

    private void _com_ReceiveMessage(object sender, ControllerEventArgs<string> e)
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
