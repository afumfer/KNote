using KntScript;

namespace KNote.ClientWin.Views;

public partial class InOutDeviceForm : Form, IInOutDevice
{
    #region Private methods

    private bool FlagClose = false;
    private int RefreshIndex = 0;

    #endregion

    #region Constructor

    public InOutDeviceForm()
    {
        InitializeComponent();
    }

    #endregion 

    #region IInOutDevice members

    public void Print(string str, bool newLine = false)
    {
        // KntScript drives this from its own thread (RunKntSCodeInNewThread), and now
        // InteractiveScriptSession drives it from Process I/O-completion threads too - neither is
        // the UI thread, so marshal here once instead of every caller having to know that.
        //
        // BeginInvoke (post and return), never Invoke (block until done): closing this window while
        // a cs/py/js script is still running disposes its InteractiveScriptSession synchronously on
        // the UI thread (FormClosing -> Ctrl.Finalize() -> OnFinalized()), which Kills the process
        // and then Disposes it - and Process.Dispose() blocks the UI thread until the ThreadPool
        // callback currently raising (or about to raise) Process.Exited has finished. That callback
        // is exactly what calls into here (via Session_Exited) to print "Process exited". A blocking
        // Invoke from that callback would wait for the UI thread to pump it - but the UI thread is
        // the one blocked inside Process.Dispose() waiting for the callback to finish - a real
        // deadlock, confirmed live (every KNote window freezes), same shape as the one already fixed
        // in KntScriptConsoleForm.Ctrl_ScriptExited.
        if (textOut.InvokeRequired)
        {
            textOut.BeginInvoke(new Action(() => Print(str, newLine)));
            return;
        }

        RefreshIndex++;

        textOut.AppendText(@str);        
        if (newLine)
            textOut.AppendText("\r\n");

        // Experimental
        if (RefreshIndex > 100)
        {
            textOut.Refresh();
            Refresh();
            RefreshIndex = 0;
        }
    }

    public bool ReadVars(List<ReadVarItem> readVarItmes)
    {
        ReadVarForm f = new ReadVarForm(readVarItmes);
        if (f.ShowDialog() == DialogResult.OK)
        {
            readVarItmes = f.ReadVarItems;
            return true;
        }
        else
            return false;

    }

    public void Clear()
    {
        textOut.Clear();
    }

    public void SetEmbeddedMode()
    {
        this.TopLevel = false;
        this.FormBorderStyle = FormBorderStyle.None;
        this.Dock = DockStyle.Fill;
    }

    public string GetOutContent()
    {
        return textOut.Text;
    }

    public void LockForm(bool lockFrm)
    {
        FlagClose = lockFrm;
    }

    #endregion

    #region Form events handlers

    private void InOutDefaultDeviceForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (FlagClose)
        {
            MessageBox.Show("You can not close this window until script execution is finished.", "KntScript");
            e.Cancel = true;
        }
    }

    #endregion 
}
