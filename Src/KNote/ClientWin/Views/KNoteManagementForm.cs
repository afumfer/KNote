using KNote.ClientWin.Core;
using KNote.ClientWin.Controllers;
using KNote.Model;
using KntScript;
using KntIcons;
using KNote.ClientWin.Utils;
using System.Runtime.InteropServices;

namespace KNote.ClientWin.Views;

public partial class KNoteManagementForm : KntForm, IViewKNoteManagement
{
    #region Private methods

    private readonly KNoteManagementCtrl _ctrl;

    #endregion

    #region IViewKNoteManagement events

    public event EventHandler ViewShown;

    #endregion

    #region Constructor

    public KNoteManagementForm(KNoteManagementCtrl ctrl)
    {
        InitializeComponent();
        menuManagement.Text = $"{KntConst.AppName} menu management";
        menuHide.Text = $"&Hide {KntConst.AppName} management";
        menuKNoteLab.Text = $"{KntConst.AppName} &lab ...";
        Text = $"{KntConst.AppName} Management";

        _ctrl = ctrl;

        _ctrl.Store.Events.Subscribe<ControllerNotification>(Store_ComponentNotification);

        Shown += KNoteManagementForm_Shown;

#if DEBUG
        menuKNoteLab.Visible = true;
#endif

        SetIcons();

        // The auto-sized toolbar fits its buttons too tightly at high scaling (noticeable at 200%):
        // two extra logical pixels above and below.
        int extra = toolBarManagement.LogicalToDeviceUnits(2);
        var padding = toolBarManagement.Padding;
        toolBarManagement.Padding = new Padding(padding.Left, padding.Top + extra, padding.Right, padding.Bottom + extra);
    }

    private void SetIcons()
    {
        toolNewNote.SetKntIcon(KntIcon.NewNote);
        toolEditNote.SetKntIcon(KntIcon.Edit);
        toolDeleteNote.SetKntIcon(KntIcon.Delete);
        toolPrintNotesList.SetKntIcon(KntIcon.Print);
        toolConfiguration.SetKntIcon(KntIcon.Settings);

        // Over the dark folder header: a light icon, as large as the folder caption next to it.
        pictureBoxFolder.SetKntIcon(KntIcon.FolderOpen, 32, Color.WhiteSmoke);

        imageTabExplorer.SetKntIcons(KntIconProvider.DefaultSize, DeviceDpi, KntIcon.FolderOpen, KntIcon.Search);
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

        msg2 = $"{_ctrl.FolderPath?.ToString()}  [{_ctrl.SelectedFolderInfo?.FolderNumber.ToString()}]  ·  Notes: {_ctrl.CountNotes?.ToString()}";

        // Updated even while the header panel is hidden, so it is current as soon as it is shown again.
        labelFolder.Text = msg1;
        labelFolderDetail.Text = msg2;
        labelRepAliasCon.Text = $"{_ctrl.SelectedFolderWithServiceRef?.ServiceRef?.RepositoryRef?.Alias} ({_ctrl.SelectedFolderWithServiceRef?.ServiceRef?.RepositoryRef?.Provider})";
        labelReResources.Text = $"{_ctrl.SelectedFolderWithServiceRef?.ServiceRef?.RepositoryRef?.ResourcesContainerRootPath}\\{_ctrl.SelectedFolderWithServiceRef?.ServiceRef?.RepositoryRef?.ResourcesContainer}";

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

    public void ReportProgressKNoteManagement(int porcentaje)
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
        formReadVar.Size = formReadVar.LogicalToDeviceUnits(new Size(500, 150));

        return formReadVar.ShowDialog() == DialogResult.OK ? listVars[0].VarNewValueText : null;
    }

    public string PromptForSaveFile(string title, string filter, string initialDirectory, string fileName)
    {
        using var dialog = new SaveFileDialog
        {
            Title = title,
            Filter = filter,
            AddExtension = true,
            OverwritePrompt = true,
            InitialDirectory = initialDirectory,
            FileName = fileName
        };

        return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
    }

    #endregion

    #region Form events handlers

    private void KNoteManagementForm_Load(object sender, EventArgs e)
    {
        SetViewPositionAndSize();
        ApplyStartupPanelVisibility();

        // Hidden from the screen until Shown has built its panels (see KNoteManagementForm_Shown).
        SetCloaked(true);
    }

    // ViewShown runs KNoteManagementCtrl.Run() (Program.cs), which creates the embedded views and docks
    // them here (ShowView/LinkComponents) synchronously. It has to wait for Shown (a running message loop,
    // see IViewKNoteManagement.ViewShown), but meanwhile the window would show its empty panels being
    // filled in: it stays cloaked until then, and appears once fully painted.
    private void KNoteManagementForm_Shown(object sender, EventArgs e)
    {
        try
        {
            ViewShown?.Invoke(this, e);
        }
        finally
        {
            RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, RDW_UPDATENOW | RDW_ALLCHILDREN);
            SetCloaked(false);
        }
    }

    // A cloaked window is shown, laid out and painted as usual, but DWM keeps it off the screen; uncloaked,
    // it appears with what is already painted. Unlike Opacity (a layered window), nothing is repainted when
    // it appears, which flashed the window's empty background for a moment.
    private void SetCloaked(bool cloaked)
    {
        int value = cloaked ? 1 : 0;
        DwmSetWindowAttribute(Handle, DWMWA_CLOAK, ref value, sizeof(int));
    }

    private const int DWMWA_CLOAK = 13;
    private const uint RDW_ALLCHILDREN = 0x0080;
    private const uint RDW_UPDATENOW = 0x0100;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("user32.dll")]
    private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

    private async void KNoteManagementForm_FormClosing(object sender, FormClosingEventArgs e)
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
        else if (menuSel == menuManagementRepository)
        {
            await _ctrl.ManagementRepository();
        }
        else if (menuSel == menuRefreshTreeFolders)
        {
            Text = $"{KntConst.AppName} Management";
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
            _ctrl.HideKNoteManagement();
        }
        else if (menuSel == menuPrintNotesList)
        {
            await _ctrl.PrintNotesList();
        }
        else if (menuSel == menuPrintNotesBook)
        {
            await _ctrl.PrintNotesBook();
        }
        else if (menuSel == menuExportNotesListCsv)
        {
            await _ctrl.ExportNotesListToCsv();
        }
        else if (menuSel == menuPrintSelectedNote)
        {
            await _ctrl.PrintSelectedNote();
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
        else if (menuSel == menuTraceSelectedNotesTo)
        {
            await _ctrl.TraceSelectedNotes(selectedAreFromSide: true);
        }
        else if (menuSel == menuTraceSelectedNotesFrom)
        {
            await _ctrl.TraceSelectedNotes(selectedAreFromSide: false);
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
            SaveViewSizeAndPosition();
            await _ctrl.Options();
        }
        else if (menuSel == menuDarkMode)
        {
            SaveViewSizeAndPosition();
            await _ctrl.ToggleDarkMode();
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
            if (!panelSupManagement.Visible)
                Text = $"{KntConst.AppName} Management";
            panelSupManagement.Visible = !panelSupManagement.Visible;
            _ctrl.Store.State.ManagementWindow.Panels.Header = panelSupManagement.Visible;
        }
        else if (menuSel == menuMainVisible)
        {
            menuManagement.Visible = !menuManagement.Visible;
            menuMainVisible.Checked = menuManagement.Visible;
            _ctrl.Store.State.ManagementWindow.Panels.MainMenu = menuManagement.Visible;
            UpdateMenuHintVisibility();
        }
        else if (menuSel == menuCompactViewNotesList)
        {
            var notesList = _ctrl.Store.State.ManagementWindow.NotesList;
            notesList.CompactView = !notesList.CompactView;
            menuCompactViewNotesList.Checked = notesList.CompactView;
            _ctrl.Store.Events.Publish(new NotesListViewOptionsChanged());
        }
        else if (menuSel == menuToolbarVisible)
        {
            menuToolbarVisible.Checked = !menuToolbarVisible.Checked;
            toolBarManagement.Visible = menuToolbarVisible.Checked;
            _ctrl.Store.State.ManagementWindow.Panels.Toolbar = menuToolbarVisible.Checked;
        }
        else if (menuSel == menuVerticalPanelForNotes)
        {
            SetVerticalPanelForNotes(splitContainer2.Orientation == Orientation.Horizontal);
        }
        else if (menuSel == menuListFilterVisible)
        {
            _ctrl.ToggleNotesListFilter();
            menuListFilterVisible.Checked = _ctrl.NotesSelectorCtrl.EnableTextFilter;
            _ctrl.Store.State.ManagementWindow.NotesList.ShowFilter = menuListFilterVisible.Checked;
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
        menuDarkMode.Checked = _ctrl.IsDarkModeConfigured;
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
        else if (menuSel == toolPrintNotesList)
            await _ctrl.PrintNotesList();
        else if (menuSel == toolConfiguration)
            await _ctrl.ManagementRepository();
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
        statusBarManagement.Refresh();
    }

    #endregion

    #region Private methods

    // Applies the View menu's persisted state (Store.State) as early as possible - Form.Load,
    // before this window is ever shown - so panels don't visibly flash from their Designer defaults
    // to their configured state right after startup. Everything here touches only this Form's own
    // native controls (menu items, toolbar, header panel, tab selection, splitter orientation), none
    // of which need _ctrl's sub-controllers to exist yet. Only the visible tab itself is restored
    // here: the actual "active folder" content is already handled independently via
    // Store.State.Session.LastActiveFolderId (see KNoteManagementCtrl.OnInitialized), and there is no
    // persisted "active filter" state to restore for the other tab.
    // See ApplyNotesFilterSetting() for the one View menu setting that does need a sub-controller.
    private void ApplyStartupPanelVisibility()
    {
        var panels = _ctrl.Store.State.ManagementWindow.Panels;

        tabExplorers.SelectedTab = panels.FoldersExplorer ? tabExplorers.TabPages[0] : tabExplorers.TabPages[1];
        menuFoldersExplorer.Checked = panels.FoldersExplorer;
        menuSearchPanel.Checked = !panels.FoldersExplorer;

        panelSupManagement.Visible = panels.Header;
        menuHeaderPanelVisible.Checked = panels.Header;

        toolBarManagement.Visible = panels.Toolbar;
        menuToolbarVisible.Checked = panels.Toolbar;

        menuManagement.Visible = panels.MainMenu;
        menuMainVisible.Checked = panels.MainMenu;
        UpdateMenuHintVisibility();

        menuCompactViewNotesList.Checked = _ctrl.Store.State.ManagementWindow.NotesList.CompactView;

        SetVerticalPanelForNotes(panels.VerticalNotesPanel);
    }

    // The one View menu setting that needs _ctrl.NotesSelectorCtrl to already exist (created by
    // KNoteManagementCtrl.OnInitialized()) - called from ShowView(), after LinkComponents() has docked
    // it. See ApplyStartupPanelVisibility() for everything else, applied earlier from Form.Load.
    private void ApplyNotesFilterSetting()
    {
        var notesList = _ctrl.Store.State.ManagementWindow.NotesList;

        // EnableTextFilter alone only takes visible effect the next time NotesSelectorCtrl's View
        // actually refreshes with real data (see NotesSelectorForm.RefreshView) - which, depending on
        // how far the LastActiveFolderId restoration has already gotten by the time this method runs,
        // may have already happened (with the panel's previous, pre-restore visibility) rather than
        // happening afterwards. Calling RefreshView() here covers both orderings: it's a no-op while
        // ListEntities is still null (not loaded yet - the later real refresh applies it then), and
        // immediately re-applies the correct visibility if the notes have already loaded.
        _ctrl.NotesSelectorCtrl.EnableTextFilter = notesList.ShowFilter;
        menuListFilterVisible.Checked = notesList.ShowFilter;
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
        _ctrl.Store.State.ManagementWindow.Panels.VerticalNotesPanel = vertical;
    }

    // Reminder shown in the status bar while the main menu is hidden, so Shift+F12 (the only way to
    // bring it back) isn't forgotten - a status bar hint was chosen over decorating the window
    // caption, since the caption is meant to identify the window, not carry usage hints, and a
    // status bar message is the more conventional place for this kind of transient reminder.
    private void UpdateMenuHintVisibility()
    {
        statusLabelMenuHint.Visible = !menuManagement.Visible;
    }

    private async Task SelectTab(int tabIndex)
    {
        if (tabIndex == 0)
        {
            tabExplorers.SelectedTab = tabExplorers.TabPages[0];
            menuFoldersExplorer.Checked = true;
            menuSearchPanel.Checked = false;
            _ctrl.Store.State.ManagementWindow.Panels.FoldersExplorer = true;
            await _ctrl.GoActiveFolder();
        }
        else if (tabIndex == 1)
        {
            tabExplorers.SelectedTab = tabExplorers.TabPages[1];
            menuFoldersExplorer.Checked = false;
            menuSearchPanel.Checked = true;
            _ctrl.Store.State.ManagementWindow.Panels.FoldersExplorer = false;
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

        // The embedded views' panels arrive after this form's Load (where KntForm adjusts its controls for
        // dark mode), and their own forms are never loaded: adjust them here.
        AppTheme.AdjustControlsForDarkMode(this);
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

        var bounds = _ctrl.Store.State.ManagementWindow.Bounds;
        bounds.X = Location.X;
        bounds.Y = Location.Y;
        bounds.Width = Width;
        bounds.Height = Height;
    }

    private void SetViewPositionAndSize()
    {
        // 0 means "never saved" (keep the default); any other value, negative included (a monitor left of /
        // above the primary one), is restored - moved back on screen if it no longer falls on one.
        var bounds = _ctrl.Store.State.ManagementWindow.Bounds;
        var location = new Point(bounds.X != 0 ? bounds.X : Left, bounds.Y != 0 ? bounds.Y : Top);
        var size = new Size(bounds.Width > 0 ? bounds.Width : Width, bounds.Height > 0 ? bounds.Height : Height);

        Bounds = WindowPlacement.EnsureVisible(new Rectangle(location, size));
    }

    #endregion

    private void labelFolderDetail_Click(object sender, EventArgs e)
    {

    }
}
