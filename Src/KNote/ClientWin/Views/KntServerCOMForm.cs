using KNote.ClientWin.Core;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class KntServerCOMForm : KntForm, IViewServerCOM
{
    #region Private members

    private readonly KntServerCOMCtrl _ctrl;

    #endregion

    #region Constructor

    public KntServerCOMForm(KntServerCOMCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        _ctrl.ReceiveMessage += _com_ReceiveMessage;
    }

    #endregion

    #region IViewChat implementation

    public override void OnClosingView()
    {
        _ctrl.ReceiveMessage -= _com_ReceiveMessage;
        base.OnClosingView();
    }

    public override void RefreshView()
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
            KntMessageBox.Show("The service is not running. Press Start button.");
            return;
        }

        if (_ctrl.MessageSending)
        {
            KntMessageBox.Show("Sending message now ... try later");
            return;
        }

        _ctrl.Send(textBoxSend.Text);
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
        listBoxEcho.Invoke((MethodInvoker)delegate
        {
            // Running on the UI thread                        
            listBoxEcho.Items.Add("Recived: " + e.Entity.ToString());
        });
    }

    #endregion
}
