using KNote.ClientWin.Core;
using KNote.Model;
using KNote.ClientWin.Controllers;

namespace KNote.ClientWin.Views;

public partial class KntServerCOMForm : Form, IViewServerCOM
{
    #region Private members

    private readonly KntServerCOMCtrl _ctrl;
    private bool _viewFinalized = false;

    #endregion

    #region Constructor

    public KntServerCOMForm()
    {
        InitializeComponent();
    }

    public KntServerCOMForm(KntServerCOMCtrl com) : this()
    {
        _ctrl = com;

        _ctrl.ReceiveMessage += _com_ReceiveMessage;
    }

    #endregion

    #region IViewChat implementation

    public void ShowView()
    {
        Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        return _ctrl.DialogResultToComponentResult(ShowDialog());
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        _ctrl.ReceiveMessage -= _com_ReceiveMessage;
        Close();
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void RefreshView()
    {
        Refresh();
        RefreshStatus();
    }

    public void VisibleView(bool visible)
    {
        if (visible)
            Show();
        else
            Hide();
    }

    public void RefreshStatus()
    {
        statusInfo.Invoke((MethodInvoker)delegate
        {
            // Running on the UI thread                        
            statusLabelInfo.Text = $"Runing service: {_ctrl.RunningService} | Message sending: {_ctrl.MessageSending}";
        });

    }

    #endregion

    #region Form and component event handler

    private void KntServerCOMForm_Load(object sender, EventArgs e)
    {
        RefreshStatus();
    }

    private void buttonStart_Click(object sender, EventArgs e)
    {
        _ctrl.StartService();
    }

    private void buttonStop_Click(object sender, EventArgs e)
    {
        _ctrl.StopService();
    }

    private void buttonSend_Click(object sender, EventArgs e)
    {
        if (!_ctrl.RunningService)
        {
            MessageBox.Show("The service is not running. Press Start button.");
            return;
        }

        if (_ctrl.MessageSending)
        {
            MessageBox.Show("Sending message now ... try later");
            return;
        }

        _ctrl.Send(textBoxSend.Text);
    }

    private void KntServerCOMForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            if (_ctrl.AutoCloseComponentOnViewExit)
                _ctrl.Finalize();
            else
            {
                Hide();
                e.Cancel = true;
            }
        }
    }

    private void _com_ReceiveMessage(object sender, ComponentEventArgs<string> e)
    {
        listBoxEcho.Invoke((MethodInvoker)delegate
        {
            // Running on the UI thread                        
            listBoxEcho.Items.Add("Recived: " + e.Entity.ToString());
        });
    }

    #endregion
}
