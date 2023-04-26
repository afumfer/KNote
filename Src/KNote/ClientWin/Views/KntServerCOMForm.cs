using KNote.ClientWin.Core;
using KNote.Model;
using KNote.ClientWin.Components;

namespace KNote.ClientWin.Views;

public partial class KntServerCOMForm : Form, IViewServerCOM
{
    #region Private members

    private readonly KntServerCOMComponent _com;
    private bool _viewFinalized = false;

    #endregion

    #region Constructor

    public KntServerCOMForm()
    {
        InitializeComponent();
    }

    public KntServerCOMForm(KntServerCOMComponent com) : this()
    {
        _com = com;

        _com.ReceiveMessage += _com_ReceiveMessage;
    }

    #endregion

    #region IViewChat implementation

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
            statusLabelInfo.Text = $"Runing service: {_com.RunningService} | Message sending: {_com.MessageSending}";
        });
        
    }

    #endregion 

    #region Form and component event handler

    private void _com_ReceiveMessage(object sender, ComponentEventArgs<string> e)
    {
        listBoxEcho.Invoke((MethodInvoker)delegate
        {
            // Running on the UI thread                        
            listBoxEcho.Items.Add("Recived: " + e.Entity.ToString());
        });
    }

    private void buttonStart_Click(object sender, EventArgs e)
    {
        _com.StartService();
    }

    private void buttonStop_Click(object sender, EventArgs e)
    {
        _com.StopService();
    }

    private void buttonSend_Click(object sender, EventArgs e)
    {
        if (!_com.RunningService)
        {
            MessageBox.Show("The service is not running. Press Start button.");
            return;
        }

        if (_com.MessageSending)
        {
            MessageBox.Show("Sending message now ... try later");
            return;
        }

        _com.Send(textBoxSend.Text);
    }

    private void KntServerCOMForm_FormClosing(object sender, FormClosingEventArgs e)
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

    private void button1_Click(object sender, EventArgs e)
    {
        _com.TestSendBinary2();
    }
    private void KntServerCOMForm_Load(object sender, EventArgs e)
    {
        RefreshStatus();
    }

    #endregion

}
