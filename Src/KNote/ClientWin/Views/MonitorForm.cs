using KNote.ClientWin.Core;
using KNote.ClientWin.Controllers;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class MonitorForm : Form, IViewBase
{
    #region Private fields 

    private readonly MonitorCtrl _ctrl;
    private bool _viewFinalized = false;

    #endregion

    #region Constructor

    public MonitorForm(MonitorCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IViewBase implementation

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        listBoxMessages.Items.Add(info);
        int visibleItems = listBoxMessages.ClientSize.Height / listBoxMessages.ItemHeight;
        listBoxMessages.TopIndex = Math.Max(listBoxMessages.Items.Count - visibleItems + 1, 0);
        return DialogResult.OK;
    }

    public void ShowView()
    {
        this.Show();
    }

    Result<EControllerResult> IViewBase.ShowModalView()
    {
        return null;
    }

    private void MonitorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
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
