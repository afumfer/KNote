using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class NotifyForm : KntForm, IViewBase
{
    #region Private fields

    private readonly KNoteManagementCtrl _ctrl;

    #endregion

    #region Constructor

    public NotifyForm(KNoteManagementCtrl ctrl)
    {
        InitializeComponent();
        notifyKNote.Text = KntConst.AppName;
        menuShowKNoteManagement.Text = $"Show {KntConst.AppName} management ...";

        _ctrl = ctrl;
    }

    #endregion 

    #region IViewBase implementation

    public override void OnClosingView()
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

    private void menuShowKNoteManagement_Click(object sender, EventArgs e)
    {
        _ctrl.ShowKNoteManagement();
    }

    private void menuPostItsVisibles_Click(object sender, EventArgs e)
    {
        if (menuPostItsVisibles.Checked)
            _ctrl.Store.ActivatePostIts();
        else
            _ctrl.Store.HidePostIts();
    }

    private void menuAppInfoAlarms_Click(object sender, EventArgs e)
    {
        _ctrl.ShowAppInfoAlarms();
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

    public override void RefreshView()
    {
        this.Refresh();        
    }

    #endregion
}
