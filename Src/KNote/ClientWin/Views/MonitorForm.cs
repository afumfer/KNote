using KNote.ClientWin.Core;
using KNote.ClientWin.Controllers;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class MonitorForm : KntForm, IViewBase
{
    #region Private fields 

    private readonly MonitorCtrl _ctrl;

    #endregion

    #region Constructor

    public MonitorForm(MonitorCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IViewBase implementation

    public override DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        // MonitorCtrl.Store_ServiceCommandExecuting/Executed fire synchronously on whatever thread
        // executed the command - including a KntScript running via Store.RunKntSCodeInNewThread's
        // own background Thread, never the UI thread - so this can be entered from a non-UI thread.
        // BeginInvoke (post and return), not Invoke (block until done), for the same reason as
        // InOutDeviceForm.Print: a blocking Invoke risks deadlocking if the calling thread is ever
        // one the UI thread wants to wait on. The return value is not meaningful here (this is a
        // log line, not an interactive dialog), so returning immediately instead of waiting for the
        // marshaled update to actually run is fine.
        if (listBoxMessages.InvokeRequired)
        {
            listBoxMessages.BeginInvoke(new Action(() => ShowInfo(info, caption, buttons, icon)));
            return DialogResult.OK;
        }

        listBoxMessages.Items.Add(info);
        int visibleItems = listBoxMessages.ClientSize.Height / listBoxMessages.ItemHeight;
        listBoxMessages.TopIndex = Math.Max(listBoxMessages.Items.Count - visibleItems + 1, 0);
        return DialogResult.OK;
    }

    public override Result<EControllerResult> ShowModalView()
    {
        return null;
    }

    private void MonitorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!ViewFinalized)
            _ctrl.Finalize();           
    }

    private void buttonClearMessages_Click(object sender, EventArgs e)
    {
        listBoxMessages.Items.Clear();
    }

    public void RefreshView()
    {
        //
    }

    #endregion 
}
