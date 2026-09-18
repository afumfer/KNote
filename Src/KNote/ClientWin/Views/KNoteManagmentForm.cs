using KNote.ClientWin.Core;
using KNote.ClientWin.Controllers;
using KNote.Model;
using KntScript;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class KNoteManagmentForm : KntForm, IViewKNoteManagment
{
    #region Private methods

    private readonly KNoteManagmentCtrl _ctrl;

    #endregion

    #region IViewKNoteManagment events

    public event EventHandler ViewShown;

    #endregion

    #region Constructor

    public KNoteManagmentForm(KNoteManagmentCtrl ctrl)
    {
        InitializeComponent();
        menuMangment.Text = $"{KntConst.AppName} menu managment";
        menuHide.Text = $"&Hide {KntConst.AppName} managment";
        menuKNoteLab.Text = $"{KntConst.AppName} &lab ...";
        Text = $"{KntConst.AppName} Managment";

        _ctrl = ctrl;

        _ctrl.Store.Events.Subscribe<ControllerNotification>(Store_ComponentNotification);

        Shown += (s, e) => ViewShown?.Invoke(this, e);

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

    public override void ShowView()
    {
        LinkComponents();
        ApplyNotesFilterSetting();
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

    public override Result<EControllerResult> ShowModalView()
    {
        LinkComponents();
        Application.DoEvents();
        return base.ShowModalView();
    }

    public override DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        if (info != null)
            return base.ShowInfo(info, caption, buttons, icon);

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

        statusLabel1.Text = $"Notes: {_ctrl.CountNotes?.ToString()}";

        return DialogResult.OK;
    }

    public override void RefreshView()
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

    public string PromptForValue(string label, string caption)
    {
        var listVars = new List<ReadVarItem> { new ReadVarItem
        {
            Label = label,
            VarIdent = "Value",
            VarValue = "",
            VarNewValueText = ""
        }};

        var formReadVar = new ReadVarForm(listVars);
        formReadVar.Text = caption;
        formReadVar.Size = new Size(500, 150);

        return formReadVar.ShowDialog() == DialogResult.OK ? listVars[0].VarNewValueText : null;
    }

    #endregion

    #region Form events handlers

    private void KNoteManagmentForm_Load(object sender, EventArgs e)
    {
        SetViewPositionAndSize();
        ApplyStartupPanelVisibility();
    }

    private async void KNoteManagmentForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!ViewFinalized)
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
        else if (menuSel == menuExecuteCode)
        {
            await _ctrl.RunCodeSelectedNotes(false);
        }
        else if (menuSel == menuExecuteCodeInNewTask)
        {
            await _ctrl.RunCodeSelectedNotes(true);
        }
        else if (menuSel == menuExecuteCodeStdOutConsole)
        {
            await _ctrl.RunCodeSelectedNotesInStdOutConsole();
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
            _ctrl.Store.AppConfig.ShowHeaderPanel = panelSupManagment.Visible;
        }
        else if (menuSel == menuMainVisible)
        {
            menuMangment.Visible = !menuMangment.Visible;
            menuMainVisible.Checked = menuMangment.Visible;
            _ctrl.Store.AppConfig.ShowMainMenu = menuMangment.Visible;
            UpdateMenuHintVisibility();
        }
        else if (menuSel == menuToolbarVisible)
        {
            menuToolbarVisible.Checked = !menuToolbarVisible.Checked;
            toolBarManagment.Visible = menuToolbarVisible.Checked;
            _ctrl.Store.AppConfig.ShowToolbar = menuToolbarVisible.Checked;
        }
        else if (menuSel == menuVerticalPanelForNotes)
        {
            SetVerticalPanelForNotes(splitContainer2.Orientation == Orientation.Horizontal);
        }
        else if (menuSel == menuListFilterVisible)
        {
            _ctrl.ToggleNotesListFilter();
            menuListFilterVisible.Checked = _ctrl.NotesSelectorCtrl.EnableTextFilter;
            _ctrl.Store.AppConfig.ShowListFilter = menuListFilterVisible.Checked;
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
        else if (menuSel == menuAIAssistant)
        {
            _ctrl.ShowKNoteAIAssistantConsole();
        }
        else if (menuSel == menuAppInfoAlarms)
        {
            _ctrl.ShowAppInfoAlarms();
        }
        else if (menuSel == menuAIProviders)
        {
            await _ctrl.ManageAiProviders();
        }
        else if (menuSel == menuCOMPortServer)
        {
            _ctrl.ShowKntCOMPortServerConsole();
        }
        else
            KntMessageBox.Show("In construction ... ");
    }

    private void menuView_DropDownOpening(object sender, EventArgs e)
    {
        // Kept in sync with the notes grid's own context menu entry (same toggle, two entry points).
        menuListFilterVisible.Checked = _ctrl.NotesSelectorCtrl.EnableTextFilter;
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

    private void Store_ComponentNotification(ControllerNotification e)
    {
        string comName;
        if (!string.IsNullOrEmpty(e?.Message))
            comName = e.Controller?.ControllerName + ": ";
        else
            comName = "";
        statusLabel2.Text = $" {comName} {e?.Message}";
        statusBarManagment.Refresh();
    }

    #endregion

    #region Private methods

    // Applies the View menu's persisted state (Store.AppConfig) as early as possible - Form.Load,
    // before this window is ever shown - so panels don't visibly flash from their Designer defaults
    // to their configured state right after startup. Everything here touches only this Form's own
    // native controls (menu items, toolbar, header panel, tab selection, splitter orientation), none
    // of which need _ctrl's sub-controllers to exist yet. Only the visible tab itself is restored
    // here: the actual "active folder" content is already handled independently via
    // Store.AppConfig.LastActiveFolderId (see KNoteManagmentCtrl.OnInitialized), and there is no
    // persisted "active filter" state to restore for the other tab.
    // See ApplyNotesFilterSetting() for the one View menu setting that does need a sub-controller.
    private void ApplyStartupPanelVisibility()
    {
        var cfg = _ctrl.Store.AppConfig;

        tabExplorers.SelectedTab = cfg.ShowFoldersExplorerTab ? tabExplorers.TabPages[0] : tabExplorers.TabPages[1];
        menuFoldersExplorer.Checked = cfg.ShowFoldersExplorerTab;
        menuSearchPanel.Checked = !cfg.ShowFoldersExplorerTab;

        panelSupManagment.Visible = cfg.ShowHeaderPanel;
        menuHeaderPanelVisible.Checked = cfg.ShowHeaderPanel;

        toolBarManagment.Visible = cfg.ShowToolbar;
        menuToolbarVisible.Checked = cfg.ShowToolbar;

        menuMangment.Visible = cfg.ShowMainMenu;
        menuMainVisible.Checked = cfg.ShowMainMenu;
        UpdateMenuHintVisibility();

        SetVerticalPanelForNotes(cfg.VerticalPanelForNotes);
    }

    // The one View menu setting that needs _ctrl.NotesSelectorCtrl to already exist (created by
    // KNoteManagmentCtrl.OnInitialized()) - called from ShowView(), after LinkComponents() has docked
    // it. See ApplyStartupPanelVisibility() for everything else, applied earlier from Form.Load.
    private void ApplyNotesFilterSetting()
    {
        var cfg = _ctrl.Store.AppConfig;

        // EnableTextFilter alone only takes visible effect the next time NotesSelectorCtrl's View
        // actually refreshes with real data (see NotesSelectorForm.RefreshView) - which, depending on
        // how far the LastActiveFolderId restoration has already gotten by the time this method runs,
        // may have already happened (with the panel's previous, pre-restore visibility) rather than
        // happening afterwards. Calling RefreshView() here covers both orderings: it's a no-op while
        // ListEntities is still null (not loaded yet - the later real refresh applies it then), and
        // immediately re-applies the correct visibility if the notes have already loaded.
        _ctrl.NotesSelectorCtrl.EnableTextFilter = cfg.ShowListFilter;
        menuListFilterVisible.Checked = cfg.ShowListFilter;
        _ctrl.NotesSelectorCtrl.View.RefreshView();
    }

    // Shared by the menu handler and ApplyStartupPanelVisibility (startup restore). Switching TO vertical
    // gives the folder tree/notes list/note detail a fixed 15%/35%/50% split of the total width, per
    // request; switching back to horizontal (list on top, detail below) leaves whatever sizes are
    // already there untouched.
    private void SetVerticalPanelForNotes(bool vertical)
    {
        splitContainer2.Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal;

        if (vertical)
        {
            splitContainer1.SplitterDistance = (int)(splitContainer1.Width * 0.15);
            splitContainer2.SplitterDistance = (int)(splitContainer2.Width * (35.0 / 85.0));
        }

        menuVerticalPanelForNotes.Checked = vertical;
        _ctrl.Store.AppConfig.VerticalPanelForNotes = vertical;
    }

    // Reminder shown in the status bar while the main menu is hidden, so Shift+F12 (the only way to
    // bring it back) isn't forgotten - a status bar hint was chosen over decorating the window
    // caption, since the caption is meant to identify the window, not carry usage hints, and a
    // status bar message is the more conventional place for this kind of transient reminder.
    private void UpdateMenuHintVisibility()
    {
        statusLabelMenuHint.Visible = !menuMangment.Visible;
    }

    private async Task SelectTab(int tabIndex)
    {
        if (tabIndex == 0)
        {
            tabExplorers.SelectedTab = tabExplorers.TabPages[0];
            menuFoldersExplorer.Checked = true;
            menuSearchPanel.Checked = false;
            _ctrl.Store.AppConfig.ShowFoldersExplorerTab = true;
            await _ctrl.GoActiveFolder();
        }
        else if (tabIndex == 1)
        {
            tabExplorers.SelectedTab = tabExplorers.TabPages[1];
            menuFoldersExplorer.Checked = false;
            menuSearchPanel.Checked = true;
            _ctrl.Store.AppConfig.ShowFoldersExplorerTab = false;
            await _ctrl.GoActiveFilter();
        }
    }

    private Control _quickSearchPanel;
    private Control _filterPanel;

    private void LinkComponents()
    {
        tabTreeFolders.Controls.Add(_ctrl.FoldersSelectorCtrl.View.PanelView());

        _quickSearchPanel = _ctrl.NotesSearchParamCtrl.View.PanelView();
        _filterPanel = _ctrl.NotesFilterParamCtrl.View.PanelView();
        _quickSearchPanel.Dock = DockStyle.Fill;
        _filterPanel.Dock = DockStyle.Fill;
        panelSearchContent.Controls.Add(_filterPanel);
        panelSearchContent.Controls.Add(_quickSearchPanel);
        SetSearchModePanel(quickSearch: true);

        splitContainer2.Panel1.Controls.Add(_ctrl.NotesSelectorCtrl.View.PanelView());
        splitContainer2.Panel2.Controls.Add(_ctrl.NoteEditorCtrl.View.PanelView());
    }

    private void SetSearchModePanel(bool quickSearch)
    {
        _quickSearchPanel.Visible = quickSearch;
        _filterPanel.Visible = !quickSearch;
        (quickSearch ? _quickSearchPanel : _filterPanel).BringToFront();

        buttonQuickSearchMode.BackColor = quickSearch ? SystemColors.ControlLight : SystemColors.Control;
        buttonFilterMode.BackColor = quickSearch ? SystemColors.Control : SystemColors.ControlLight;
    }

    private void buttonQuickSearchMode_Click(object sender, EventArgs e)
    {
        SetSearchModePanel(quickSearch: true);
    }

    private void buttonFilterMode_Click(object sender, EventArgs e)
    {
        SetSearchModePanel(quickSearch: false);
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
