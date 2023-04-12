using KNote.ClientWin.Core;
using KNote.ClientWin.Components;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class MonitorForm : Form, IViewBase
{
    #region Private fields 

    private readonly MonitorComponent _com;
    private bool _viewFinalized = false;

    #endregion

    #region Constructor

    public MonitorForm(MonitorComponent com)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _com = com;
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

    Result<EComponentResult> IViewBase.ShowModalView()
    {
        return null;
    }

    private void MonitorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _com.Finalize();           
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
