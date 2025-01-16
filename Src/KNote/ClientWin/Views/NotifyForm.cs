using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class NotifyForm : Form, IViewBase
{
    #region Private fields

    private readonly KNoteManagmentCtrl _ctrl;

    #endregion

    #region Constructor

    public NotifyForm(KNoteManagmentCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();
        notifyKNote.Text = KntConst.AppName;
        menuShowKNoteManagment.Text = $"Show {KntConst.AppName} managment ...";

        _ctrl = ctrl;
    }

    #endregion 

    #region IViewBase implementation

    public void ShowView()
    {
        this.Show();
    }

    Result<EComponentResult> IViewBase.ShowModalView()
    {
        return _ctrl.DialogResultToComponentResult(this.ShowDialog());
    }

    public DialogResult ShowInfo(string info, string caption = "KeyNotex", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void OnClosingView()
    {
    }

    #endregion

    #region Menu events handlers 

    private async void notifyKNote_DoubleClick(object sender, EventArgs e)
    {
        await _ctrl.AddDefaultNotePostIt();
    }

    private async void menuNewNote_Click(object sender, EventArgs e)
    {
        await _ctrl.AddDefaultNotePostIt();
    }

    private void menuShowKNoteManagment_Click(object sender, EventArgs e)
    {
        _ctrl.ShowKNoteManagment();
    }

    private void menuPostItsVisibles_Click(object sender, EventArgs e)
    {
        if (menuPostItsVisibles.Checked)
            _ctrl.Store.ActivatePostIts();
        else
            _ctrl.Store.HidePostIts();
    }

    private void menuKNoteOptions_Click(object sender, EventArgs e)
    {
        _ctrl.Options();
    }

    private void menuHelp_Click(object sender, EventArgs e)
    {
        _ctrl.Help();
    }

    private void menuAbout_Click(object sender, EventArgs e)
    {
        _ctrl.About();
    }

    private void menuExit_Click(object sender, EventArgs e)
    {
        _ctrl?.FinalizeApp();
    }

    public void RefreshView()
    {
        this.Refresh();        
    }

    #endregion
}
