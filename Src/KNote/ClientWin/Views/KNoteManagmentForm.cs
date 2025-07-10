using KNote.ClientWin.Core;
using KNote.ClientWin.Controllers;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class KNoteManagmentForm : Form, IViewKNoteManagment
{
    #region Private methods 

    private readonly KNoteManagmentCtrl _ctrl;
    private bool _viewFinalized = false;

    #endregion

    #region Constructor

    public KNoteManagmentForm(KNoteManagmentCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();
        menuMangment.Text = $"{KntConst.AppName} menu managment";
        menuHide.Text = $"&Hide {KntConst.AppName} managment";
        menuKNoteLab.Text = $"{KntConst.AppName} &lab ...";
        Text = $"{KntConst.AppName} Managment";

        _ctrl = ctrl;

        _ctrl.Store.ControllerNotification += Store_ComponentNotification;

        // TODO: options ... for next version
        menuReports.Visible = false;
        menuToolsS1.Visible = false;
        toolPrintReports.Visible = false;
        toolStripSeparator2.Visible = false;
#if DEBUG
        menuKNoteLab.Visible = true;
#endif
    }

    #endregion 

    #region IViewBase interface 

    public void ShowView()
    {
        LinkComponents();
        Application.DoEvents();
        this.Show();
    }

    public void HideView()
    {
        this.Hide();
    }

    public void ActivateView()
    {
        this.Show();
        this.Select();
        if (this.WindowState == FormWindowState.Minimized)
            this.WindowState = FormWindowState.Normal;
    }

    Result<EControllerResult> IViewBase.ShowModalView()
    {
        LinkComponents();
        Application.DoEvents();
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public DialogResult ShowInfo(string info, string caption = "KeyNoteX", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        if (info != null)
            return MessageBox.Show(info, caption, buttons, icon);

        string msg1;
        string msg2;

        if (_ctrl.SelectMode == EnumSelectMode.Folders)
        {
            if (string.IsNullOrEmpty(_ctrl.SelectedFolderInfo?.Name))
                msg1 = "(No folder selected)";
            else
                msg1 = $"{_ctrl.SelectedFolderInfo?.Name}";
        }
        else
            msg1 = "(Filtered notes)";

        msg2 = $"{_ctrl.FolderPath?.ToString()}  [{_ctrl.SelectedFolderInfo?.FolderNumber.ToString()}]";

        if (menuHeaderPanelVisible.Checked)
        {
            labelFolder.Text = msg1;
            labelFolderDetail.Text = msg2;
            labelRepAliasCon.Text = $"{_ctrl.SelectedFolderWithServiceRef?.ServiceRef?.RepositoryRef?.Alias} ({_ctrl.SelectedFolderWithServiceRef?.ServiceRef?.RepositoryRef?.Provider})";
            labelReResources.Text = $"{_ctrl.SelectedFolderWithServiceRef?.ServiceRef?.RepositoryRef?.ResourcesContainerRootPath}\\{_ctrl.SelectedFolderWithServiceRef?.ServiceRef?.RepositoryRef?.ResourcesContainer}";
        }

        statusLabel1.Text = $"Notes: {_ctrl.CountNotes.ToString()}";

        return DialogResult.OK;
    }

    public void RefreshView()
    {
        throw new NotImplementedException();
    }

    public void ActivateWaitState()
    {
        this.Cursor = Cursors.WaitCursor;
    }

    public void DeactivateWaitState()
    {
        this.Cursor = Cursors.Default;
    }

    public void ReportProgressKNoteManagment(int porcentaje)
    {
        progressBar.Value = porcentaje;
    }

    public void SetVisibleProgressBar(bool visible)
    {
        progressBar.Visible = visible;
    }

    #endregion

    #region Form events handlers

    private void KNoteManagmentForm_Load(object sender, EventArgs e)
    {
        SetViewPositionAndSize();
    }

    private async void KNoteManagmentForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            this.Hide();
            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                SaveViewSizeAndPosition();
                await _ctrl.FinalizeAppForce();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }

    private async void menu_Click(object sender, EventArgs e)
    {
        ToolStripMenuItem menuSel;
        menuSel = (ToolStripMenuItem)sender;

        if (menuSel == menuKNoteLab)
        {
            _ctrl.Lab();
        }
        else if (menuSel == menuNewFolder)
        {
            _ctrl.NewFolder();
        }
        else if (menuSel == menuEditFolder)
        {
            await _ctrl.EditFolder();
        }
        else if (menuSel == menuDeleteFolder)
        {
            await _ctrl.DeleteFolder();
        }
        else if (menuSel == menuRemoveRepositoryLink)
        {
            await _ctrl.RemoveRepositoryLink();
        }
        else if (menuSel == menuAddRepositoryLink)
        {
            await _ctrl.AddRepositoryLink();
        }
        else if (menuSel == menuCreateRepository)
        {
            await _ctrl.CreateRepository();
        }
        else if (menuSel == menuManagmentRepository)
        {
            await _ctrl.ManagmentRepository();
        }
        else if (menuSel == menuRefreshTreeFolders)
        {
            Text = $"{KntConst.AppName} Managment";
            _ctrl.RefreshRepositoryAndFolderTree();
        }
        else if (menuSel == menuEditNote)
        {
            await _ctrl.EditNote();
        }
        else if (menuSel == menuEditNoteAsPostIt)
        {
            await _ctrl.EditNotePostIt();
        }
        else if (menuSel == menuNewNote)
        {
            await _ctrl.AddNote();
        }
        else if (menuSel == menuNewNoteAsPostIt)
        {
            await _ctrl.AddNotePostIt();
        }
        else if (menuSel == menuDeleteNote)
        {
            await _ctrl.DeleteNote();
        }
        else if (menuSel == menuKntScriptConsole)
        {
            _ctrl.ShowKntScriptConsole();
        }
        else if (menuSel == menuHide)
        {
            _ctrl.HideKNoteManagment();
        }
        else if (menuSel == menuAbout)
        {
            _ctrl.About();
        }
        else if (menuSel == menuHelpDoc)
        {
            _ctrl.Help();
        }
        else if (menuSel == menuMoveSelectedNotes)
        {
            await _ctrl.MoveSelectedNotes();
        }
        else if (menuSel == menuAddTags)
        {
            await _ctrl.ChangeTags(EnumChangeTag.Add);
        }
        else if (menuSel == menuRemoveTags)
        {
            await _ctrl.ChangeTags(EnumChangeTag.Remove);
        }
        else if (menuSel == menuExecuteKntScript)
        {
            _ctrl.RunScriptSelectedNotes();
        }
        else if (menuSel == menuOptions)
        {
            _ctrl.Options();
        }
        else if (menuSel == menuFoldersExplorer)
        {
            if (tabExplorers.SelectedIndex == 0)
                return;
            await SelectTab(0);
        }
        else if (menuSel == menuSearchPanel)
        {
            if (tabExplorers.SelectedIndex == 1)
                return;
            await SelectTab(1);
        }
        else if (menuSel == menuHeaderPanelVisible)
        {
            if (!panelSupManagment.Visible)
                Text = $"{KntConst.AppName} Managment";
            panelSupManagment.Visible = !panelSupManagment.Visible;
        }
        else if (menuSel == menuMainVisible)
        {
            menuMangment.Visible = !menuMangment.Visible;
            menuMainVisible.Checked = !menuMainVisible.Checked;
        }
        else if (menuSel == menuToolbarVisible)
        {
            menuToolbarVisible.Checked = !menuToolbarVisible.Checked;
            toolBarManagment.Visible = menuToolbarVisible.Checked;
        }
        else if (menuSel == menuVerticalPanelForNotes)
        {
            if (splitContainer2.Orientation == Orientation.Horizontal)
            {
                splitContainer2.Orientation = Orientation.Vertical;
                menuVerticalPanelForNotes.Checked = true;
            }
            else
            {
                splitContainer2.Orientation = Orientation.Horizontal;
                menuVerticalPanelForNotes.Checked = false;
            }
        }
        else if (menuSel == menuExit)
        {
            SaveViewSizeAndPosition();
            await _ctrl.FinalizeApp();
        }
        else if (menuSel == menuChat)
        {
            _ctrl.ShowKntChatConsole();
        }
        else if (menuSel == menuChatGPT)
        {
            _ctrl.ShowKntChatGPTConsole();
        }
        else if (menuSel == menuCOMPortServer)
        {
            _ctrl.ShowKntCOMPortServerConsole();
        }
        else
            MessageBox.Show("In construction ... ");
    }

    private async void buttonToolBar_Click(object sender, EventArgs e)
    {
        ToolStripItem menuSel;
        menuSel = (ToolStripItem)sender;

        if (menuSel == toolEditNote)
            await _ctrl.EditNote();
        else if (menuSel == toolNewNote)
            await _ctrl.AddNote();
        else if (menuSel == toolDeleteNote)
            await _ctrl.DeleteNote();
        else if (menuSel == toolConfiguration)
            await _ctrl.ManagmentRepository();
    }

    private async void tabExplorers_SelectedIndexChanged(object sender, EventArgs e)
    {
        await SelectTab(tabExplorers.SelectedIndex);
    }

    private void Store_ComponentNotification(object sender, ControllerEventArgs<string> e)
    {
        string comName;
        if (!string.IsNullOrEmpty(e?.Entity.ToString()))
            comName = ((CtrlBase)sender)?.ControllerName + ": ";
        else
            comName = "";
        statusLabel2.Text = $" {comName} {e?.Entity.ToString()}";
        statusBarManagment.Refresh();
    }

    #endregion

    #region Private methods

    private async Task SelectTab(int tabIndex)
    {
        if (tabIndex == 0)
        {
            tabExplorers.SelectedTab = tabExplorers.TabPages[0];
            menuFoldersExplorer.Checked = true;
            menuSearchPanel.Checked = false;
            await _ctrl.GoActiveFolder();
        }
        else if (tabIndex == 1)
        {
            tabExplorers.SelectedTab = tabExplorers.TabPages[1];
            menuFoldersExplorer.Checked = false;
            menuSearchPanel.Checked = true;
            await _ctrl.GoActiveFilter();
        }
    }

    private void LinkComponents()
    {
        tabTreeFolders.Controls.Add(_ctrl.FoldersSelectorCtrl.View.PanelView());
        tabSearch.Controls.Add(_ctrl.FilterParamCtrl.View.PanelView());
        splitContainer2.Panel1.Controls.Add(_ctrl.NotesSelectorCtrl.View.PanelView());
        splitContainer2.Panel2.Controls.Add(_ctrl.NoteEditorCtrl.View.PanelView());
    }

    private void SaveViewSizeAndPosition()
    {
        if (WindowState == FormWindowState.Minimized)
            return;

        _ctrl.Store.AppConfig.ManagmentLocX = Location.X;
        _ctrl.Store.AppConfig.ManagmentLocY = Location.Y;
        _ctrl.Store.AppConfig.ManagmentWidth = Width;
        _ctrl.Store.AppConfig.ManagmentHeight = Height;
    }

    private void SetViewPositionAndSize()
    {
        if (_ctrl.Store.AppConfig.ManagmentLocX > SystemInformation.VirtualScreen.Width - 100)
            _ctrl.Store.AppConfig.ManagmentLocX = 100;
        if (_ctrl.Store.AppConfig.ManagmentLocY > SystemInformation.VirtualScreen.Height - 100)
            _ctrl.Store.AppConfig.ManagmentLocY = 100;

        if (_ctrl.Store.AppConfig.ManagmentLocY > 0)
            Top = _ctrl.Store.AppConfig.ManagmentLocY;
        if (_ctrl.Store.AppConfig.ManagmentLocX > 0)
            Left = _ctrl.Store.AppConfig.ManagmentLocX;
        if (_ctrl.Store.AppConfig.ManagmentWidth > 0)
            Width = _ctrl.Store.AppConfig.ManagmentWidth;
        if (_ctrl.Store.AppConfig.ManagmentHeight > 0)
            Height = _ctrl.Store.AppConfig.ManagmentHeight;
    }

    #endregion

    private void labelFolderDetail_Click(object sender, EventArgs e)
    {

    }
}
