using KntScript;

namespace KNote.ClientWin.Views;

public partial class InOutDeviceForm : Form, IInOutDevice
{
    #region Private methods

    private bool FlagClose = false;

    #endregion

    #region Constructor

    public InOutDeviceForm()
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();       
    }

    #endregion 

    #region IInOutDevice members

    public void Print(string str, bool newLine = false)
    {
        textOut.AppendText(@str);
        if (newLine)
            textOut.AppendText("\r\n");
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
