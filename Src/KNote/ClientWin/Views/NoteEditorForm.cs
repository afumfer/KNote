using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.EntityFramework.Entities;
using KntScript;
using KntWebView;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KNote.ClientWin.Views;

public partial class NoteEditorForm : Form, IViewNoteEditorEmbeddable<NoteExtendedDto>
{
    #region Private fields

    private readonly NoteEditorCtrl _ctrl;
    private bool _viewFinalized = false;

    private FolderInfoDto _changedFolder = null;
    private ResourceDto _selectedResource;

    private string _textSearch = "";
    private int _indexTextSearch = 0;

    // Primary/growing column per sub-list, for ListViewColumnResizer.
    private const int AttributesPrimaryColumnIndex = 2;  // Value (columns: Order[hidden], Name, Value)
    private const int ResourcesPrimaryColumnIndex = 1;   // Name (columns: Order, Name, File type)
    private const int TasksPrimaryColumnIndex = 1;       // Topic/Tags (columns: Priority, Topic/Tags, ...)
    private const int AlarmsPrimaryColumnIndex = 6;      // Comment
    private const int TraceNotePrimaryColumnIndex = 1;   // Topic

    private ListViewColumnSorter _resourcesSorter;
    private ListViewColumnSorter _tasksSorter;
    private ListViewColumnSorter _alarmsSorter;
    private ListViewColumnSorter _traceNoteFromSorter;
    private ListViewColumnSorter _traceNoteToSorter;

    #endregion

    #region Constructor

    public NoteEditorForm(NoteEditorCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        // Must happen here, not in PersonalizeControls() (which only runs on this Form's Load event):
        // CtrlEditorBase.OnInitialized() calls View.RefreshView() - and hence ModelToControls(), which
        // reads _resourcesSorter/_tasksSorter/etc. via ListViewSortHelper.ApplyInitialOrder - BEFORE
        // CtrlViewBase.Run() gets to View.ShowView(), which is what triggers Load. Attaching here
        // instead guarantees these fields are never null by the time the first RefreshView() runs, the
        // same reasoning that already has the "manage list" Forms (UsersManageForm, ...) do their
        // PersonalizeListView/Attach in the constructor instead of on Load.
        PersonalizeListView(listViewAttributes);
        PersonalizeListView(listViewResources);
        PersonalizeListView(listViewTasks);
        PersonalizeListView(listViewAlarms);
        PersonalizeListView(listViewTraceNoteFrom);
        PersonalizeListView(listViewTraceNoteTo);

        // listViewAttributes has no interactive click-to-sort: it's always shown sorted by its
        // (hidden) Order column - see ModelToControlsAttributes - not by whatever column the user
        // last clicked, so it never gets a ListViewColumnSorter/ColumnClick wiring like the others.
        _resourcesSorter = ListViewSortHelper.Attach(listViewResources);
        _tasksSorter = ListViewSortHelper.Attach(listViewTasks);
        _alarmsSorter = ListViewSortHelper.Attach(listViewAlarms);
        _traceNoteFromSorter = ListViewSortHelper.Attach(listViewTraceNoteFrom);
        _traceNoteToSorter = ListViewSortHelper.Attach(listViewTraceNoteTo);

        foreach (var scriptType in ScriptTypes)
            comboScriptType.Items.Add(scriptType.Text);

        // TODO: options for new versión
        buttonPrint.Visible = false;
        buttonCheck.Visible = false;
        toolStripS3.Visible = false;
        toolStripS4.Visible = false;

        // Anchor=Right is not reliable here: its reference gap gets captured at a point
        // in the layout lifecycle that ends up inconsistent with the DPI-driven
        // SplitterDistance recalculation in FixSplitterDistances(). Reposition explicitly
        // instead, driven by the header panel's own Resize (fires on load, DPI change and
        // splitter drag alike), so it always reflects the panel's real runtime width.
        panelResourcesLeftHeader.Resize += (s, e) => AlignButtonsRight(panelResourcesLeftHeader, 3, 3,
            buttonResourceAdd, buttonResourceDelete, buttonResourceEdit, buttonInsertLink, buttonSaveResource);
        panelTasksHeader.Resize += (s, e) => AlignButtonsRight(panelTasksHeader, 4, 1,
            buttonTaskAdd, buttonTaskDelete, buttonTaskEdit);
        panelTraceFromHeader.Resize += (s, e) => AlignButtonsRight(panelTraceFromHeader, 3, 3,
            buttonTraceFromAdd, buttonTraceFromRemove, buttonTraceFromEdit);
        panelTraceToHeader.Resize += (s, e) => AlignButtonsRight(panelTraceToHeader, 3, 3,
            buttonTraceToAdd, buttonTraceToRemove, buttonTraceToEdit);

        // Drag & drop a file onto the form or the description editor uploads it as a resource,
        // the same way the "upload"/"paste from clipboard" toolbar buttons already do. WinForms
        // drag&drop does not bubble to parent controls, so each real drop surface needs its own
        // AllowDrop + handlers; kntEditView's WebView2 sub-control additionally needs
        // AllowExternalDrop = false (set in KntEditView itself) or the browser would intercept
        // the OS drop before this event ever fires.
        foreach (Control dropTarget in new Control[] { this, kntEditView, kntEditView.MarkdownContentControl, kntEditView.HtmlContentControl, kntEditView.WebViewControl })
        {
            dropTarget.AllowDrop = true;
            dropTarget.DragEnter += Content_DragEnter;
            dropTarget.DragDrop += Content_DragDrop;
        }
    }

    private static void AlignButtonsRight(Control header, int rightMargin, int spacing, params Control[] buttonsLeftToRight)
    {
        int right = header.Width - rightMargin;
        for (int i = buttonsLeftToRight.Length - 1; i >= 0; i--)
        {
            Control btn = buttonsLeftToRight[i];
            btn.Left = right - btn.Width;
            right = btn.Left - spacing;
        }
    }

    #endregion

    #region IEditorView interface

    public Control PanelView()
    {
        return panelForm;
    }

    public void ShowView()
    {
        this.Show();
        if (_ctrl.EditMode == false)
            // for contract extended view
            labelExpandContent_Click(this, new EventArgs());
    }

    public Result<EControllerResult> ShowModalView()
    {
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public async void CleanView()
    {
        // Basic data
        textTopic.Text = "";
        textNoteNumber.Text = "";
        textFolder.Text = "";
        textFolderNumber.Text = "";
        textTags.Text = "";
        textStatus.Text = "";
        await kntEditView.ClearWebView();
        textPriority.Text = "";
        textDescriptionResource.Text = "";
        if (webViewResource.Visible)
            await webViewResource.ClearWebView();
        webViewResource.Visible = true;
        panelPreview.Visible = false;
        await kntEditViewTask.ClearWebView();
        textScriptCode.Text = "";
        listViewAttributes.Clear();
        listViewResources.Clear();
        listViewTasks.Clear();
        listViewAlarms.Clear();
        listViewTraceNoteFrom.Clear();
        listViewTraceNoteTo.Clear();
    }

    public void RefreshView()
    {
        ModelToControls();
    }

    public void RefreshViewOnlyRequiredCtrl()
    {
        ModelToControlsOnlyRequiredComponents();
    }

    public async Task RefreshFolderAndRepositoryDisplayAsync()
    {
        Text = $"Note editor [{_ctrl.ServiceRef?.Alias}]";

        // No note is loaded yet (e.g. KNoteManagmentCtrl.NoteEditorCtrl - the embedded main-window
        // editor - runs its first RefreshView() before any note has been selected into it) or the
        // note genuinely has no folder assigned: Guid.Empty can never resolve to a real folder, so
        // skip the round-trip to Store.GetKNoteFolerPath entirely instead of asking it to look up
        // "no folder" and report back "not found" - same end result (empty path), no wasted DB call.
        textFolder.Text = _ctrl.ServiceRef == null || _ctrl.Model.FolderId == Guid.Empty
            ? string.Empty
            : await _ctrl.Store.GetKNoteFolerPath(_ctrl.ServiceRef, _ctrl.Model.FolderId);
    }

    public async void RefreshModel()
    {
        await ControlsToModel();
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public void ConfigureEmbededMode()
    {
        TopLevel = false;
        Dock = DockStyle.Fill;
        FormBorderStyle = FormBorderStyle.None;
        toolBarNoteEditor.Visible = false;
        kntEditView.MarkdownContentControl.ReadOnly = true;
        kntEditView.MarkdownContentControl.BackColor = SystemColors.Window;
        kntEditView.MarkdownContentControl.BorderStyle = BorderStyle.FixedSingle;
        _ctrl.EditMode = false;
    }

    public void ConfigureWindowMode()
    {
        TopLevel = true;
        Dock = DockStyle.None;
        FormBorderStyle = FormBorderStyle.Sizable;
        toolBarNoteEditor.Visible = true;
        StartPosition = FormStartPosition.CenterScreen;
        _ctrl.EditMode = true;
    }

    #endregion

    #region Form events handlers

    private void NoteEditorForm_Load(object sender, EventArgs e)
    {
        PersonalizeControls();
        FixSplitterDistances();
    }

    // SplitContainer.SplitterDistance is baked as an absolute pixel value by the
    // Designer at whatever DPI it was saved at, and is not reliably rescaled by
    // AutoScaleMode at runtime. Recompute it from the container's real, already
    // auto-scaled Width so the layout is correct at any Windows scale factor.
    private void FixSplitterDistances()
    {
        splitTasksViewer.SplitterDistance = (int)(splitTasksViewer.Width * (372.0 / 797.0));
        splitResourcesViewer.SplitterDistance = (int)(splitResourcesViewer.Width * (395.0 / 797.0));
    }

    private async void NoteEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var savedOk = await SaveModel();
            if (!savedOk)
            {
                if (MessageBox.Show("Do yo want exit?", KntConst.AppName, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _ctrl.Finalize();
                    return;
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }
            _ctrl.Finalize();
        }
    }

    private async void buttonToolBar_Click(object sender, EventArgs e)
    {
        ToolStripItem menuSel;
        menuSel = (ToolStripItem)sender;

        if (menuSel == buttonSave)
        {
            await SaveModel();
        }
        else if (menuSel == buttonDelete)
        {
            await DeleteModel();
        }
        else if (menuSel == buttonUndo)
        {
            UndoChanges();
        }
        else if (menuSel == buttonPostIt)
        {
            await PostItEdit();
        }
        else if (menuSel == buttonExecuteKntScript)
        {
            _ctrl.Model.Script = textScriptCode.Text;
            await _ctrl.RunCode(false);
        }        
        else if (menuSel == buttonExecuteKntScriptInNewTask)
        {
            _ctrl.Model.Script = textScriptCode.Text;
            await _ctrl.RunCode(true);
        }
        else if (menuSel == buttonExecuteKntScriptStdOutConsole)
        {
            _ctrl.Model.Script = textScriptCode.Text;
            await _ctrl.RunCodeInStdOutConsole();
        }
        else if (menuSel == buttonLockFormat)
        {
            var ct = _ctrl.Model.GetContentTypeExt();
            ct.DescriptionBlocked = !ct.DescriptionBlocked;
            buttonLockFormat.Checked = ct.DescriptionBlocked;
            _ctrl.Model.SetContentTypeExt(ct);
            ApplyDescriptionLockUI(ct.DescriptionBlocked);
        }
        else if (menuSel == buttonInsertTemplate)
        {
            await InsertTemplate();
        }
        else if (menuSel == buttonInsertCode)
        {
            await InsertCode();
        }
        else if (menuSel == buttonKNoteAssistant)
        {
            ExecKNoteAssistant();
        }
        else if (menuSel == buttonTextSearch)
        {
            TextSearch();
        }
        else if (menuSel == buttonTextSearchNext)
        {
            TextSearchNext();
        }
        else if (menuSel == buttonAddTaskSelectedText)
        {
            await AddTaskFromSelectedText();
        }
    }

    private void NoteEditorForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            buttonUndo.Enabled = true;
    }

    private void NoteEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        buttonUndo.Enabled = true;
    }

    private void buttonEditMarkdown_Click(object sender, EventArgs e)
    {
        try
        {
            var ct = _ctrl.Model.GetContentTypeExt();
            if (ct.DescriptionBlocked)
            {
                ShowInfo($"This note is locked and cannot be edited.");
                return;
            }

            if (kntEditView.ContentType == "html")
            {
                kntEditView.ShowMarkdownContent(_ctrl.Service.Notes.UtilHtmlToMarkdown(kntEditView.BodyHtml));
            }
            else
                kntEditView.ShowMarkdownContent();

            ct.ForDescription = "markdown";
            _ctrl.Model.SetContentTypeExt(ct);

            EnableMarkdownView();
        }
        catch (Exception ex)
        {
            _ctrl.ShowMessage($"The following error has occurred: {ex.Message}", "Note editor");
        }

    }

    private async void buttonNavigate_Click(object sender, EventArgs e)
    {
        try
        {
            var ct = _ctrl.Model.GetContentTypeExt();
            if (ct.DescriptionBlocked)
            {
                ShowInfo($"This note is locked and cannot be edited.");
                return;
            }

            var url = _ctrl.Store.KntTextUtils.ExtractUrlFromText(kntEditView.MarkdownText);
            if (!string.IsNullOrEmpty(url))
            {
                await kntEditView.ShowNavigationUrlContent(url);
            }
            else
            {
                string content;
                if (kntEditView.ContentType == "html")
                    content = (_ctrl.Service.Notes.UtilHtmlToMarkdown(kntEditView.BodyHtml));
                else
                    content = kntEditView.MarkdownText;

                var htmlContent = _ctrl.Service.Notes.UtilMarkdownToHtml(content.Replace(_ctrl.Service.RepositoryRef.ResourcesContainerRootUrl, KntConst.VirtualHostNameToFolderMapping));

                await kntEditView.SetVirtualHostNameToFolderMapping(_ctrl.Service.RepositoryRef.ResourcesContainerRootPath);
                await kntEditView.ShowNavigationContent(htmlContent + _ctrl.Store.KNoteWebViewStyle);
            }

            ct.ForDescription = "navigation";
            _ctrl.Model.SetContentTypeExt(ct);

            EnableNavigationView();
        }
        catch (Exception ex)
        {
            _ctrl.ShowMessage($"You can not navigate to the indicated address in the description of this note. (The following error has occurred: {ex.Message})", "Note editor");
        }
    }

    private void buttonEditHtml_Click(object sender, EventArgs e)
    {
        try
        {
            var ct = _ctrl.Model.GetContentTypeExt();
            if (ct.DescriptionBlocked)
            {
                ShowInfo($"This note is locked and cannot be edited.");
                return;
            }

            kntEditView.ShowHtmlContent(_ctrl.Service.Notes.UtilMarkdownToHtml(kntEditView.MarkdownText));

            ct.ForDescription = "html";
            _ctrl.Model.SetContentTypeExt(ct);

            EnableHtmlView();
        }
        catch (Exception ex)
        {
            _ctrl.ShowMessage($"The following error has occurred: {ex.Message}", "Note editor");
        }

    }

    private void listViewResources_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (listViewResources.SelectedItems.Count > 0)
            {
                var idResource = (Guid.Parse(listViewResources.SelectedItems[0].Name));
                var selRes = _ctrl.Model.Resources.Where(_ => _.ResourceId == idResource).FirstOrDefault();
                UpdatePreviewResource(selRes);
            }
        }
        catch (Exception ex)
        {
            _ctrl.ShowMessage($"The following error has occurred: {ex.Message}", "Note editor");
        }
    }

    // Shared by listViewAttributes/listViewTasks/listViewAlarms - each grows a different column
    // (its own identifying/descriptive field), so dispatch on which ListView actually resized.
    private void listView_Resize(object sender, EventArgs e)
    {
        var lv = (ListView)sender;
        int primaryColumnIndex = lv == listViewTasks ? TasksPrimaryColumnIndex
            : lv == listViewAlarms ? AlarmsPrimaryColumnIndex
            : AttributesPrimaryColumnIndex;
        ListViewColumnResizer.Resize(lv, primaryColumnIndex);
    }

    private void listViewTraceNote_Resize(object sender, EventArgs e)
    {
        ListViewColumnResizer.Resize((ListView)sender, TraceNotePrimaryColumnIndex);
    }

    private void listViewResources_Resize(object sender, EventArgs e)
    {
        ListViewColumnResizer.Resize(listViewResources, ResourcesPrimaryColumnIndex);
    }

    private void toolDescriptionHtml_Click(object sender, EventArgs e)
    {
        ToolStripItem menuSel;
        menuSel = (ToolStripItem)sender;

        if (menuSel == toolDescriptionHtmlTitle1)
        {
            kntEditView.HtmlContentControl.SelectedHtml = "<h1>Title 1</h1>";
            kntEditView.HtmlContentControl.Focus();
        }
        if (menuSel == toolDescriptionHtmlTitle2)
        {
            kntEditView.HtmlContentControl.SelectedHtml = "<h2>Title 2</h2>";
            kntEditView.HtmlContentControl.Focus();
        }
        if (menuSel == toolDescriptionHtmlTitle3)
        {
            kntEditView.HtmlContentControl.SelectedHtml = "<h3>Title 3</h3>";
            kntEditView.HtmlContentControl.Focus();
        }
        if (menuSel == toolDescriptionHtmlTitle4)
        {
            kntEditView.HtmlContentControl.SelectedHtml = "<h4>Title 4</h4>";
            kntEditView.HtmlContentControl.Focus();
        }
        if (menuSel == toolDescriptionHtmlEdit)
        {
            kntEditView.HtmlContentControl.HtmlContentsEdit();
            kntEditView.HtmlContentControl.Focus();
        }
    }

    private void toolDescriptionMarkdown_Click(object sender, EventArgs e)
    {
        ToolStripItem menuSel;
        menuSel = (ToolStripItem)sender;

        string tag = "";
        var nl = System.Environment.NewLine;

        if (menuSel == toolDescriptionMarkdownBold)
            tag = " **text**";
        else if (menuSel == toolDescriptionMarkdownStrikethrough)
            tag = " ~~text~~";
        else if (menuSel == toolDescriptionMarkdownItalic)
            tag = " *text*";
        else if (menuSel == toolDescriptionMarkdownH1)
            tag = nl + "# ";
        else if (menuSel == toolDescriptionMarkdownH2)
            tag = nl + "## ";
        else if (menuSel == toolDescriptionMarkdownH3)
            tag = nl + "### ";
        else if (menuSel == toolDescriptionMarkdownH4)
            tag = nl + "#### ";
        else if (menuSel == toolDescriptionMarkdownList)
            tag = "- text";
        else if (menuSel == toolDescriptionMarkdownListOrdered)
            tag = nl + "1. text";
        else if (menuSel == toolDescriptionMarkdownLine)
            tag = nl + "------------";
        else if (menuSel == toolDescriptionMarkdownLink)
            tag = nl + "[xx](http://xx 'text info')";
        else if (menuSel == toolDescriptionMarkdownImage)
            tag = nl + "![alt_title)](http://url 'Img description')";
        else if (menuSel == toolDescriptionMarkdownCode)
            tag = nl + $"```{nl}text code{nl}```";
        else if (menuSel == toolDescriptionMarkdownTable)
        {
            tag = nl + nl;
            tag += "|   |   |" + nl;
            tag += "| ------------ | ------------ |" + nl;
            tag += "|   |   |" + nl;
            tag += "|   |   |" + nl;
        }

        var selStart = kntEditView.MarkdownContentControl.SelectionStart;
        kntEditView.MarkdownContentControl.Text = kntEditView.MarkdownContentControl.Text.Insert(selStart, tag);
        kntEditView.MarkdownContentControl.Focus();
        kntEditView.MarkdownContentControl.SelectionStart = selStart + tag.Length;
        kntEditView.MarkdownContentControl.BeginInvoke(new Action(() => kntEditView.MarkdownContentControl.ScrollToCaret()));
    }

    private async void buttonFolderSearch_Click(object sender, EventArgs e)
    {
        var folder = _ctrl.GetFolder(_changedFolder?.FolderId ?? _ctrl.Model.FolderId);
        if (folder != null)
        {
            _changedFolder = folder;
            textFolder.Text = await _ctrl.Store.GetKNoteFolerPath(_ctrl.ServiceRef, _changedFolder.FolderId);
            textFolderNumber.Text = "#" + _changedFolder.FolderNumber.ToString();

            buttonUndo.Enabled = true;
        }
    }

    private async void buttonNoteType_Click(object sender, EventArgs e)
    {
        var changed = await _ctrl.RequestChangeNoteType(_ctrl.Model.NoteTypeId);

        if (changed)
        {
            textNoteType.Text = _ctrl.Model.NoteTypeDto?.Name;
            ModelToControlsAttributes();
        }
    }

    private async void buttonDeleteType_Click(object sender, EventArgs e)
    {
        var changed = await _ctrl.AplyChangeNoteType(null);
        if (changed)
        {
            textNoteType.Text = _ctrl.Model.NoteTypeDto?.Name;
            ModelToControlsAttributes();
        }
    }

    private void buttonAttributeEdit_Click(object sender, EventArgs e)
    {
        EditNoteAttribute();
    }

    private void textDescription_Enter(object sender, EventArgs e)
    {
        TextBox textBox = sender as TextBox;
        textBox.SelectionStart = textBox.Text.Length;
        textBox.SelectionLength = 0;
    }

    private void labelExpandContent_Click(object sender, EventArgs e)
    {
        // panelHeaderData and panelContentHeader (the Content:/buttons row) are both plain
        // Dock=Top siblings of panelDescription (Dock=Fill) in tabBasicData. Toggling
        // panelHeaderData.Visible is enough: Dock stacking automatically moves panelContentHeader
        // up to take its place, and panelDescription automatically reclaims/gives back the rest -
        // no manual Top/Height math needed.
        panelHeaderData.Visible = !panelHeaderData.Visible;
        labelExpandContent.Text = panelHeaderData.Visible ? "▲" : "▼";
    }

    // Display text -> ContentTypeExt.ForScript code, in the order shown in comboScriptType.
    private static readonly (string Code, string Text)[] ScriptTypes =
    {
        ("knt", "KNote Script"),
        ("cs", "C# Script"),
        ("py", "Python Script"),
        ("js", "JavaScript Script"),
        ("ln", "Natural Language"),
    };

    private void comboScriptType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboScriptType.SelectedIndex < 0)
        {
            buttonExecuteKntScriptInNewTask.Enabled = false;
            buttonExecuteKntScriptStdOutConsole.Enabled = false;
            return;
        }

        var forScript = ScriptTypes[comboScriptType.SelectedIndex].Code;

        var ct = _ctrl.Model.GetContentTypeExt();
        ct.ForScript = forScript;
        _ctrl.Model.SetContentTypeExt(ct);

        // "...in new task" only means something distinct from the plain run (F5) for "knt" - for
        // cs/py/js it's now just another name for "...in stdout console", and "ln" ignores it
        // entirely - so it's disabled everywhere it wouldn't do anything new.
        buttonExecuteKntScriptInNewTask.Enabled = Store.SupportsNewTaskMode(forScript);
        // "...in stdout console" only makes sense for engines that shell out to a real OS process
        // (cs/py/js) - disabled for knt/ln, which have no OS console to speak of.
        buttonExecuteKntScriptStdOutConsole.Enabled = Store.SupportsStdOutConsole(forScript);
    }

    private void SetScriptTypeCombo(string forScript)
    {
        var index = Array.FindIndex(ScriptTypes, t => t.Code == forScript);
        comboScriptType.SelectedIndex = index >= 0 ? index : 0;
    }

    #region Messages managment

    private async void buttonAddAlarm_Click(object sender, EventArgs e)
    {
        var message = await _ctrl.NewMessage();
        if (message != null)
        {
            listViewAlarms.Items.Add(MessageDtoToListViewItem(message));
            ListViewSelectionHelper.SelectByKey(listViewAlarms, message.KMessageId.ToString(), _alarmsSorter);
        }
    }

    private void buttonEditAlarm_Click(object sender, EventArgs e)
    {
        EditAlarm();
    }

    private void buttonDeleteAlarm_Click(object sender, EventArgs e)
    {
        if (listViewAlarms.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no selected alert.", KntConst.AppName);
            return;
        }
        var messageId = Guid.Parse(listViewAlarms.SelectedItems[0].Name);
        var res = _ctrl.DeleteMessage(messageId);
        if (res)
        {
            listViewAlarms.Items[messageId.ToString()].Remove();
            ListViewSelectionHelper.SelectFirst(listViewAlarms, _alarmsSorter);
        }
    }

    private void listViewAlarms_DoubleClick(object sender, EventArgs e)
    {
        if (_ctrl.EditMode)
            EditAlarm();
    }

    #endregion

    #region Tasks managment

    private async void buttonTaskAdd_Click(object sender, EventArgs e)
    {
        await AddTask();
    }

    private void buttonTaskEdit_Click(object sender, EventArgs e)
    {
        EditTask();
    }

    private async void buttonTaskDelete_Click(object sender, EventArgs e)
    {
        if (listViewTasks.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no task selected .", KntConst.AppName);
            return;
        }
        string delTsk = listViewTasks.SelectedItems[0].Name;
        bool res = _ctrl.DeleteTask(Guid.Parse(delTsk));
        if (res)
        {
            listViewTasks.Items[delTsk].Remove();
            if (listViewTasks.Items.Count > 0)
                // Triggers listViewTasks_SelectedIndexChanged, which refreshes the task description
                // preview for the newly selected task - no separate ClearWebView()/update needed here.
                ListViewSelectionHelper.SelectFirst(listViewTasks, _tasksSorter);
            else
                await kntEditViewTask.ClearWebView();
        }
    }

    private void listViewTasks_DoubleClick(object sender, EventArgs e)
    {
        if (_ctrl.EditMode)
            EditTask();
    }

    #endregion

    #region Resource managment

    private async void buttonResourceAdd_Click(object sender, EventArgs e)
    {
        await AddResource();
    }

    private async void buttonResourceEdit_Click(object sender, EventArgs e)
    {
        await EditResource();
    }

    private async void buttonResourceDelete_Click(object sender, EventArgs e)
    {
        await ResourceDelete();
    }

    private async void listViewResources_DoubleClick(object sender, EventArgs e)
    {
        if (_ctrl.EditMode)
            await EditResource();
    }

    private void linkViewFile_Click(object sender, EventArgs e)
    {
        if (_selectedResource.FullUrl == null)
            return;

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(_selectedResource.FullUrl) { UseShellExecute = true };
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            ShowInfo("The following error has occurred: " + ex.Message);
        }
    }

    private void buttonInsertLink_Click(object sender, EventArgs e)
    {
        if (listViewResources.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no resource selected.", KntConst.AppName);
            return;
        }

        InsertLinkSelectedResource();
    }

    private void buttonSaveResource_Click(object sender, EventArgs e)
    {
        if (_selectedResource == null)
        {
            ShowInfo("There is no selected resource.");
            return;
        }

        saveFileDialog.Title = "Save resource file as ...";
        saveFileDialog.InitialDirectory = Path.GetTempPath();
        saveFileDialog.FileName = _selectedResource.NameOut;
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                string fileName = saveFileDialog.FileName;
                if (_selectedResource.ContentInDB)
                    File.WriteAllBytes(fileName, _selectedResource.ContentArrayBytes);
                else
                {
                    string fullPath = _ctrl.Service.Notes.UtilGetResourceFilePath(_selectedResource);
                    File.Copy(fullPath, fileName);
                }
            }
            catch (Exception ex)
            {
                ShowInfo($"File could not be saved, the following error has occurred: {ex.Message}.");
            }
        }
    }

    private async void toolDescriptionUploadResource_Click(object sender, EventArgs e)
    {
        var resource = await AddResource();
        if (resource != null)
            InsertLinkSelectedResource();
    }

    private void toolDescriptionUploadResourceFromClipboard_Click(object sender, EventArgs e)
    {
        var resource = AddResourceFromClipboard();
        if (resource != null)
            InsertLinkSelectedResource();
    }

    private async void listViewTasks_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (listViewTasks.SelectedItems.Count > 0)
            {
                var idTask = (Guid.Parse(listViewTasks.SelectedItems[0].Name));
                var selTask = _ctrl.Model.Tasks.Where(_ => _.NoteTaskId == idTask).FirstOrDefault();

                await UpdateTaskDescription(selTask.Description);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OnSelectedTaskItemChanged error: {ex.Message}");
        }
    }

    private void listViewAttributes_DoubleClick(object sender, EventArgs e)
    {
        if (_ctrl.EditMode)
            EditNoteAttribute();
    }

    #endregion

    #endregion

    #region Private methods

    private void PersonalizeControls()
    {
        if (_ctrl.Model is null)
            return;

        kntEditView.Dock = DockStyle.Fill;
        kntEditView.EnableUrlBox = false;
        kntEditView.ShowNavigationTools = false;
        kntEditView.ShowStatusInfo = false;

        kntEditView.HtmlContentControl.BorderStyle = BorderStyle.None;
        kntEditView.MarkdownContentControl.BorderStyle = BorderStyle.FixedSingle;

        kntEditView.ContentType = _ctrl.Model.GetContentTypeExt().ForDescription;

        if (!_ctrl.EditMode)
        {
            // get all controls in principal tab to disable edition
            foreach (var tab in tabNoteData.TabPages)
            {
                foreach (Control conTmp in ((TabPage)tab).Controls)
                {
                    BlockControl(conTmp);
                }
            }

            // other controls out of principal tab
            // (BlockControl above can't reach these: they're nested inside SplitContainer
            // panels, which BlockControl doesn't recurse into)
            kntEditView.HtmlEditorEditMode = false;
            buttonTaskAdd.Visible = false;
            buttonTaskDelete.Visible = false;
            buttonTaskEdit.Visible = false;
            buttonResourceAdd.Visible = false;
            buttonResourceDelete.Visible = false;
            buttonResourceEdit.Visible = false;
            buttonInsertLink.Visible = false;
            buttonSaveResource.Visible = false;
            buttonTraceFromAdd.Visible = false;
            buttonTraceFromRemove.Visible = false;
            buttonTraceFromEdit.Visible = false;
            buttonTraceToAdd.Visible = false;
            buttonTraceToRemove.Visible = false;
            buttonTraceToEdit.Visible = false;
        }

        panelDescription.Visible = true;

        webViewResource.ShowNavigationTools = false;

        textDescriptionResource.ReadOnly = true;
        textDescriptionResource.BackColor = Color.White;

        kntEditViewTask.ShowStatusInfo = false;
        kntEditViewTask.EnableUrlBox = false;
        kntEditViewTask.ShowNavigationTools = false;
        kntEditViewTask.BorderStyle = BorderStyle.FixedSingle;

        // ListView setup (PersonalizeListView + ListViewSortHelper.Attach) happens in the
        // constructor, not here - see the comment there.

        kntEditView.NavigationStart += KntEditView_NavigationStart;
        kntEditView.NavigationEnd += KntEditView_NavigationEnd; 
    }

    private void KntEditView_NavigationStart(object sender, EventArgs e)
    {
        ProgressBarOn();
    }

    private void KntEditView_NavigationEnd(object sender, EventArgs e)
    {
        ProgressBarOff();
    }

    private void ModelToControlsOnlyRequiredComponents()
    {
        textNoteNumber.Text = "#" + _ctrl.Model.NoteNumber.ToString();
        textFolderNumber.Text = "#" + _ctrl.Model.FolderDto.FolderNumber.ToString();
        textStatus.Text = _ctrl.Model.InternalTags;
        var ct = _ctrl.Model.GetContentTypeExt();
        buttonLockFormat.Checked = ct.DescriptionBlocked;
        ApplyDescriptionLockUI(ct.DescriptionBlocked);

        this.Update();
        this.Refresh();
    }

    private async void ModelToControls()
    {
        var ct = _ctrl.Model.GetContentTypeExt();

        // Basic data
        textTopic.Text = _ctrl.Model.Topic;
        textNoteNumber.Text = "#" + _ctrl.Model.NoteNumber.ToString();

        await RefreshFolderAndRepositoryDisplayAsync();

        textFolderNumber.Text = "#" + _ctrl.Model.FolderDto.FolderNumber.ToString();
        textTags.Text = _ctrl.Model.Tags;
        textStatus.Text = _ctrl.Model.InternalTags;
        textPriority.Text = _ctrl.Model.Priority.ToString();

        kntEditView.SetMarkdownContent(_ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForRead(_ctrl.Model?.Description, true));

        if (ct.ForDescription == "html")
        {
            ProgressBarOn();
            kntEditView.ShowHtmlContent(kntEditView.MarkdownText);
            ProgressBarOff();
            EnableHtmlView();
        }
        else if (ct.ForDescription == "navigation")
        {
            if (!string.IsNullOrEmpty(kntEditView.MarkdownText))
            {
                var url = _ctrl.Store.KntTextUtils.ExtractUrlFromText(kntEditView.MarkdownText);
                if (!string.IsNullOrEmpty(url))
                {
                    await kntEditView.ShowNavigationUrlContent(url);
                }
                else
                {
                    var htmlContent = _ctrl.Service.Notes.UtilMarkdownToHtml(kntEditView.MarkdownText.Replace(_ctrl.Service.RepositoryRef.ResourcesContainerRootUrl, KntConst.VirtualHostNameToFolderMapping));
                    await kntEditView.SetVirtualHostNameToFolderMapping(_ctrl.Service.RepositoryRef.ResourcesContainerRootPath);
                    await kntEditView.ShowNavigationContent(htmlContent + _ctrl.Store.KNoteWebViewStyle);
                }
            }
            else
            {
                await kntEditView.ShowNavigationContent("");
            }
            EnableNavigationView();
        }
        else
        {
            kntEditView.ShowMarkdownContent();
            EnableMarkdownView();
        }

        buttonLockFormat.Checked = ct.DescriptionBlocked;
        ApplyDescriptionLockUI(ct.DescriptionBlocked);

        // KAttributes
        textNoteType.Text = _ctrl.Model.NoteTypeDto.Name;
        ModelToControlsAttributes();

        // Resources
        ModelToControlsResources();
        if (_ctrl.Model.Resources.Count > 0)
            ListViewSelectionHelper.SelectFirst(listViewResources, _resourcesSorter);
        else
            UpdatePreviewResource(null);

        // Tasks
        ModelToControlsTasks();
        if (_ctrl.Model.Tasks.Count > 0)
            ListViewSelectionHelper.SelectFirst(listViewTasks, _tasksSorter);
        else
            await kntEditViewTask.ClearWebView();

        // Alarms
        ModelToControlsAlarms();

        // Trace notes
        await ModelToControlsTraceNotes();

        // Script
        textScriptCode.Text = _ctrl.Model.Script;
        SetScriptTypeCombo(ct.ForScript);

        this.Update();
        this.Refresh();
    }

    private void ModelToControlsAttributes()
    {
        listViewAttributes.Clear();

        foreach (var atr in _ctrl.Model.KAttributesDto)
        {
            // Order (column 0) is intentionally hidden (Width=0 below). Unlike every other
            // ListView+CRUD screen, this list has no interactive column-click sort: it must always
            // stay ordered by the attribute's defined display sequence (Order), which editing a
            // note's attribute Value never changes - see the class-level exception noted where
            // listViewAttributes is set up in PersonalizeControls().
            var itemList = new ListViewItem(atr.Order.ToString());
            itemList.Name = atr.NoteKAttributeId.ToString();
            itemList.SubItems.Add(atr.Name);
            itemList.SubItems.Add(atr.Value);
            listViewAttributes.Items.Add(itemList);
        }

        // Width of -2 indicates auto-size.
        listViewAttributes.Columns.Add("Order", 0, HorizontalAlignment.Left);
        listViewAttributes.Columns.Add("Name", 250, HorizontalAlignment.Left);
        listViewAttributes.Columns.Add("Value", -2, HorizontalAlignment.Left);
        // Always ascending by Order (column 0) - see the comment above; no user-driven sort to respect.
        ListViewSortHelper.ApplyDefaultSort(listViewAttributes);
        listView_Resize(listViewAttributes, EventArgs.Empty);
    }

    private void ModelToControlsResources()
    {
        listViewResources.Clear();
        panelPreview.Visible = true;
        linkViewFile.Visible = false;
        webViewResource.Visible = false;

        foreach (var res in _ctrl.Model.Resources)
        {
            if (!res.IsDeleted())
                listViewResources.Items.Add(ResourceDtoToListViewItem(res));
        }

        listViewResources.Columns.Add("Order", 70, HorizontalAlignment.Left);
        listViewResources.Columns.Add("Name", 200, HorizontalAlignment.Left);
        listViewResources.Columns.Add("File type", 100, HorizontalAlignment.Left);
        ListViewSortHelper.ApplyInitialOrder(listViewResources, _resourcesSorter);
        listViewResources_Resize(listViewResources, EventArgs.Empty);
    }

    private void ModelToControlsTasks()
    {
        listViewTasks.Clear();

        foreach (var task in _ctrl.Model.Tasks)
        {
            if (!task.IsDeleted())
                listViewTasks.Items.Add(NoteTaskDtoToListViewItem(task));
        }

        // Width of -2 indicates auto-size.
        listViewTasks.Columns.Add("Priority", 50, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Topic/Tags", 250, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Resolved", 60, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Start", 120, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("End", 120, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Est. time", 50, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Spend time", 50, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Dif.", 50, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Ex start", 120, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Ex end", 120, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("User", 250, HorizontalAlignment.Left);
        ListViewSortHelper.ApplyInitialOrder(listViewTasks, _tasksSorter);
        listView_Resize(listViewTasks, EventArgs.Empty);
    }

    private void ModelToControlsAlarms()
    {
        listViewAlarms.Clear();

        foreach (var msg in _ctrl.Model.Messages)
        {
            if (!msg.IsDeleted())
                listViewAlarms.Items.Add(MessageDtoToListViewItem(msg));
        }

        // Width of -2 indicates auto-size.
        listViewAlarms.Columns.Add("Date time", 130, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("User", 130, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("Activated", 80, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("Alarm periodicity", 120, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("Min", 50, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("Notification type", 120, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("Comment", -2, HorizontalAlignment.Left);
        ListViewSortHelper.ApplyInitialOrder(listViewAlarms, _alarmsSorter);
        listView_Resize(listViewAlarms, EventArgs.Empty);
    }

    private async Task ModelToControlsTraceNotes()
    {
        listViewTraceNoteFrom.Clear();
        listViewTraceNoteTo.Clear();

        // _ctrl.Service can be null the first time ModelToControls() runs (see the same "?."
        // treatment a few lines up, on _ctrl.ServiceRef/_ctrl.Service around textFolder/kntEditView
        // setup) - unlike the Resources/Tasks sections below, this line isn't inside a foreach over
        // an existing collection, so it can't rely on an empty list to skip touching Service.
        if (_ctrl.Service == null)
            return;

        // Fetched once and reused for every row - avoids re-fetching the (small, catalog-like)
        // TraceNoteTypes list once per trace note.
        var traceNoteTypeNames = (await _ctrl.Service.Repository.TraceNoteTypes.GetAllAsync()).Entity?
            .ToDictionary(t => t.TraceNoteTypeId, t => t.Name) ?? new Dictionary<Guid, string>();

        foreach (var traceNote in _ctrl.Model.TraceNotesFrom)
            if (!traceNote.IsDeleted())
                listViewTraceNoteFrom.Items.Add(await TraceNoteDtoToListViewItemAsync(traceNote, traceNote.FromId, traceNoteTypeNames));

        foreach (var traceNote in _ctrl.Model.TraceNotesTo)
            if (!traceNote.IsDeleted())
                listViewTraceNoteTo.Items.Add(await TraceNoteDtoToListViewItemAsync(traceNote, traceNote.ToId, traceNoteTypeNames));

        listViewTraceNoteFrom.Columns.Add("Number", 60, HorizontalAlignment.Left);
        listViewTraceNoteFrom.Columns.Add("Topic", 300, HorizontalAlignment.Left);
        listViewTraceNoteFrom.Columns.Add("Tags", 150, HorizontalAlignment.Left);
        listViewTraceNoteFrom.Columns.Add("Type", 110, HorizontalAlignment.Left);
        listViewTraceNoteFrom.Columns.Add("Order", 55, HorizontalAlignment.Left);
        listViewTraceNoteFrom.Columns.Add("Weight", 55, HorizontalAlignment.Left);

        listViewTraceNoteTo.Columns.Add("Number", 60, HorizontalAlignment.Left);
        listViewTraceNoteTo.Columns.Add("Topic", 300, HorizontalAlignment.Left);
        listViewTraceNoteTo.Columns.Add("Tags", 150, HorizontalAlignment.Left);
        listViewTraceNoteTo.Columns.Add("Type", 110, HorizontalAlignment.Left);
        listViewTraceNoteTo.Columns.Add("Order", 55, HorizontalAlignment.Left);
        listViewTraceNoteTo.Columns.Add("Weight", 55, HorizontalAlignment.Left);

        ListViewSortHelper.ApplyInitialOrder(listViewTraceNoteFrom, _traceNoteFromSorter);
        ListViewSortHelper.ApplyInitialOrder(listViewTraceNoteTo, _traceNoteToSorter);

        listViewTraceNote_Resize(listViewTraceNoteFrom, EventArgs.Empty);
        listViewTraceNote_Resize(listViewTraceNoteTo, EventArgs.Empty);
    }

    // Each row shows the OTHER note in the relation (Number/Topic/Tags), not the TraceNoteDto's own
    // fields - resolved with one lookup per row (trace lists are small, per-note; not worth a batch
    // endpoint yet) - plus Order/Weight/Type, which DO belong to the TraceNoteDto itself (Type only
    // shown when TraceNoteTypeId actually has a value - untyped relations are a valid, common case).
    private async Task<ListViewItem> TraceNoteDtoToListViewItemAsync(TraceNoteDto traceNote, Guid relatedNoteId, Dictionary<Guid, string> traceNoteTypeNames)
    {
        var relatedNote = (await _ctrl.Service.Notes.GetAsync(relatedNoteId)).Entity;

        var itemList = new ListViewItem(relatedNote != null ? "#" + relatedNote.NoteNumber : "?");
        itemList.Name = traceNote.TraceNoteId.ToString();
        itemList.SubItems.Add(relatedNote?.Topic);
        itemList.SubItems.Add(relatedNote?.Tags);
        itemList.SubItems.Add(traceNote.TraceNoteTypeId.HasValue && traceNoteTypeNames.TryGetValue(traceNote.TraceNoteTypeId.Value, out var typeName) ? typeName : "");
        itemList.SubItems.Add(traceNote.Order.ToString());
        itemList.SubItems.Add(traceNote.Weight.ToString());
        return itemList;
    }

    #region Trace notes management

    private async void buttonTraceFromAdd_Click(object sender, EventArgs e)
    {
        var added = await _ctrl.NewTraceNote(ownerIsFromSide: false);
        if (added != null)
        {
            await ModelToControlsTraceNotes();
            ListViewSelectionHelper.SelectByKey(listViewTraceNoteFrom, added.TraceNoteId.ToString(), _traceNoteFromSorter);
        }
    }

    private async void buttonTraceFromEdit_Click(object sender, EventArgs e)
    {
        await EditTraceNote(listViewTraceNoteFrom, ownerIsFromSide: false);
    }

    private async void buttonTraceFromRemove_Click(object sender, EventArgs e)
    {
        await RemoveTraceNote(listViewTraceNoteFrom, ownerIsFromSide: false);
    }

    private async void buttonTraceToAdd_Click(object sender, EventArgs e)
    {
        var added = await _ctrl.NewTraceNote(ownerIsFromSide: true);
        if (added != null)
        {
            await ModelToControlsTraceNotes();
            ListViewSelectionHelper.SelectByKey(listViewTraceNoteTo, added.TraceNoteId.ToString(), _traceNoteToSorter);
        }
    }

    private async void buttonTraceToEdit_Click(object sender, EventArgs e)
    {
        await EditTraceNote(listViewTraceNoteTo, ownerIsFromSide: true);
    }

    private async void buttonTraceToRemove_Click(object sender, EventArgs e)
    {
        await RemoveTraceNote(listViewTraceNoteTo, ownerIsFromSide: true);
    }

    private async Task EditTraceNote(ListView listView, bool ownerIsFromSide)
    {
        if (listView.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no trace note selected.", KntConst.AppName);
            return;
        }
        var traceNoteId = Guid.Parse(listView.SelectedItems[0].Name);
        var edited = await _ctrl.EditTraceNote(traceNoteId, ownerIsFromSide);
        if (edited != null)
        {
            await ModelToControlsTraceNotes();
            ListViewSelectionHelper.SelectByKey(listView, edited.TraceNoteId.ToString(), TraceNoteSorterFor(listView));
        }
    }

    private async Task RemoveTraceNote(ListView listView, bool ownerIsFromSide)
    {
        if (listView.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no trace note selected.", KntConst.AppName);
            return;
        }
        var traceNoteId = Guid.Parse(listView.SelectedItems[0].Name);
        var res = _ctrl.DeleteTraceNote(traceNoteId, ownerIsFromSide);
        if (res)
        {
            await ModelToControlsTraceNotes();
            ListViewSelectionHelper.SelectFirst(listView, TraceNoteSorterFor(listView));
        }
    }

    // EditTraceNote/RemoveTraceNote are shared by both the "From" and "To" lists (called with either
    // as the listView parameter) - each list has its own ListViewColumnSorter, so look up the right one.
    private ListViewColumnSorter TraceNoteSorterFor(ListView listView) =>
        listView == listViewTraceNoteFrom ? _traceNoteFromSorter : _traceNoteToSorter;

    #endregion

    private async void UpdatePreviewResource(ResourceDto resource)
    {
        _selectedResource = resource;

        if (webViewResource.Visible)
            await webViewResource.ClearWebView();
        textDescriptionResource.Text = "";

        if (_selectedResource == null)
            return;

        textDescriptionResource.Text = _selectedResource.Description;

        if (_ctrl.Store.KntTextUtils.IsSupportedFileTypeForPreview(_selectedResource.FileType))
        {
            webViewResource.Visible = true;
            panelPreview.Visible = false;
            if (!string.IsNullOrEmpty(_selectedResource.FullUrl))
                await webViewResource.ShowNavigationUrlContent(_selectedResource.FullUrl);
        }
        else
        {
            _ctrl.Service.Notes.UtilManageResourceContent(_selectedResource, false);
            webViewResource.Visible = false;
            panelPreview.Visible = true;
            linkViewFile.Visible = true;
        }
    }

    private async Task ControlsToModel()
    {
        // Basic data
        var ct = _ctrl.Model.GetContentTypeExt();

        _ctrl.Model.Topic = textTopic.Text;

        if (_changedFolder != null)
        {
            _ctrl.Model.FolderId = _changedFolder.FolderId;
            _ctrl.Model.FolderDto = _changedFolder;
        }

        _ctrl.Model.Tags = textTags.Text;
        _ctrl.Model.InternalTags = textStatus.Text;

        if (!ct.DescriptionBlocked)
        {
            if (ct.ForDescription == "html")
                _ctrl.Model.Description = _ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForWrite(kntEditView.BodyHtml, true);
            else
                _ctrl.Model.Description = _ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForWrite(kntEditView.MarkdownText, true);
        }

        int p;
        if (int.TryParse(textPriority.Text, out p))
            _ctrl.Model.Priority = p;

        _ctrl.Model.Script = textScriptCode.Text;
    }

    private async Task<bool> SaveModel()
    {
        buttonUndo.Enabled = false;
        return await _ctrl.SaveModel();
    }

    private async Task DeleteModel()
    {
        var res = await _ctrl.DeleteModel();
        if (res)
            _ctrl.Finalize();
    }

    private void UndoChanges()
    {
        var res = MessageBox.Show("Are you sure you want to undo changes?", KntConst.AppName, MessageBoxButtons.YesNo);
        if (res == DialogResult.Yes)
        {
            ModelToControls();
            buttonUndo.Enabled = false;
        }
    }

    private async Task<bool> PostItEdit()
    {
        var res = await SaveModel();
        _ctrl.FinalizeAndPostItEdit();
        return res;
    }

    private void BlockControl(Control c)
    {
        if (c is TextBox)
        {
            TextBox t = (TextBox)c;
            t.ReadOnly = true;
            t.BackColor = Color.White;
            return;
        }
        else if (c is Button)
        {
            Button b = (Button)c;
            b.Visible = false;
            return;
        }
        else if (c is CheckBox)
        {
            CheckBox cb = (CheckBox)c;
            cb.Enabled = false;
            return;
        }
        else if (c is RadioButton)
        {
            RadioButton cb = (RadioButton)c;
            cb.Enabled = false;
            return;
        }
        else if (c is ComboBox)
        {
            ComboBox comB = (ComboBox)c;
            comB.Enabled = false;
            return;
        }
        else if (c is ToolStrip)
        {
            ToolStrip tb = (ToolStrip)c;
            tb.Visible = false;
            return;
        }
        else if (c is Panel)
        {
            Panel tb = (Panel)c;
            foreach (Control conTmp in tb.Controls)
            {
                BlockControl(conTmp);
            }
            return;
        }
    }

    private void PersonalizeListView(ListView listView)
    {
        listView.View = View.Details;
        listView.LabelEdit = false;
        listView.AllowColumnReorder = false;
        listView.CheckBoxes = false;
        listView.FullRowSelect = true;
        listView.GridLines = true;
        listView.Sorting = SortOrder.None;
    }

    private ListViewItem MessageDtoToListViewItem(KMessageDto message)
    {
        var itemList = new ListViewItem(message.AlarmDateTime.ToString());
        itemList.Name = message.KMessageId.ToString();
        itemList.SubItems.Add(message.UserFullName);
        itemList.SubItems.Add(message.AlarmActivated.ToString());
        itemList.SubItems.Add(message.AlarmType.ToString());
        itemList.SubItems.Add(message.AlarmMinutes.ToString());
        itemList.SubItems.Add(message.NotificationType.ToString());
        itemList.SubItems.Add(message.Comment.ToString());
        return itemList;
    }

    private ListViewItem NoteTaskDtoToListViewItem(NoteTaskDto task)
    {
        //var itemList = new ListViewItem(task.UserFullName);
        //itemList.Name = task.NoteTaskId.ToString();
        //itemList.SubItems.Add(task.Priority.ToString());
        //itemList.SubItems.Add(task.Resolved.ToString());
        //itemList.SubItems.Add(task.StartDate.ToString());
        //itemList.SubItems.Add(task.EndDate.ToString());
        //itemList.SubItems.Add(task.EstimatedTime.ToString());
        //itemList.SubItems.Add(task.SpentTime.ToString());
        //itemList.SubItems.Add(task.DifficultyLevel.ToString());
        //itemList.SubItems.Add(task.ExpectedStartDate.ToString());
        //itemList.SubItems.Add(task.ExpectedEndDate.ToString());
        //return itemList;

        var itemList = new ListViewItem(task.Priority.ToString());
        itemList.Name = task.NoteTaskId.ToString();
        itemList.SubItems.Add(task.Tags);
        itemList.SubItems.Add(task.Resolved.ToString());
        itemList.SubItems.Add(task.StartDate.ToString());
        itemList.SubItems.Add(task.EndDate.ToString());
        itemList.SubItems.Add(task.EstimatedTime.ToString());
        itemList.SubItems.Add(task.SpentTime.ToString());
        itemList.SubItems.Add(task.DifficultyLevel.ToString());
        itemList.SubItems.Add(task.ExpectedStartDate.ToString());
        itemList.SubItems.Add(task.ExpectedEndDate.ToString());
        itemList.SubItems.Add(task.UserFullName.ToString());
        return itemList;

    }

    private ListViewItem ResourceDtoToListViewItem(ResourceDto resource)
    {
        var itemList = new ListViewItem(resource.Order.ToString());
        itemList.Name = resource.ResourceId.ToString();
        itemList.SubItems.Add(resource.NameOut);
        itemList.SubItems.Add(resource.FileType);
        return itemList;
    }

    // Note: buttonEditMarkdown/buttonViewHtml/buttonNavigate are intentionally left alone here -
    // their Enabled flag doubles as a mode proxy read from several other places (InsertLinkSelectedResource,
    // TextSearch, InsertTemplate, ...), so this method must not repurpose it for the lock state.
    // Mode switching while locked is instead rejected inside each button's own Click handler.
    private const string LockedTitleIndicator = " \U0001F512 Locked";

    private void ApplyDescriptionLockUI(bool locked)
    {
        kntEditView.ContentLocked = locked;
        toolDescription.Enabled = !locked;
        buttonInsertLink.Enabled = !locked;
        buttonInsertTemplate.Enabled = !locked;
        buttonAddTaskSelectedText.Enabled = !locked;

        // RefreshFolderAndRepositoryDisplayAsync() resets Text to its base form on every
        // ModelToControls() run, before this method is called - so this only ever adds/removes
        // the suffix once, regardless of how many times it runs.
        if (locked && !Text.EndsWith(LockedTitleIndicator))
            Text += LockedTitleIndicator;
        else if (!locked && Text.EndsWith(LockedTitleIndicator))
            Text = Text.Substring(0, Text.Length - LockedTitleIndicator.Length);
    }

    private void EnableHtmlView()
    {
        buttonEditMarkdown.Enabled = true;
        buttonViewHtml.Enabled = false;
        buttonNavigate.Enabled = true;
        if (_ctrl.EditMode)
        {
            toolDescription.Visible = true;
            toolDescriptionHtml.Visible = true;
            toolDescriptionMarkdown.Visible = false;
        }
        kntEditView.BorderStyle = BorderStyle.None;
    }

    private void EnableMarkdownView()
    {
        buttonEditMarkdown.Enabled = false;
        buttonViewHtml.Enabled = true;
        buttonNavigate.Enabled = true;
        if (_ctrl.EditMode)
        {
            toolDescription.Visible = true;
            toolDescriptionHtml.Visible = false;
            toolDescriptionMarkdown.Visible = true;
        }
        kntEditView.BorderStyle = BorderStyle.None;
    }

    private void EnableNavigationView()
    {
        buttonEditMarkdown.Enabled = true;
        buttonViewHtml.Enabled = true;
        buttonNavigate.Enabled = false;
        if (_ctrl.EditMode)
        {
            toolDescription.Visible = false;
        }
        kntEditView.BorderStyle = BorderStyle.FixedSingle;
    }

    private void EditAlarm()
    {
        if (listViewAlarms.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no alert selected.", KntConst.AppName);
            return;
        }
        var messageId = Guid.Parse(listViewAlarms.SelectedItems[0].Name);
        var message = _ctrl.EditMessage(messageId);
        if (message != null)
            UpdateMessage(message);
    }

    private void UpdateMessage(KMessageDto message)
    {
        var item = listViewAlarms.Items[message.KMessageId.ToString()];
        // SubItems[1] (User) is left untouched, same as before this reorder - it was never patched
        // here (not editable via the message/alarm editor).
        item.Text = message.AlarmDateTime.ToString();
        item.SubItems[2].Text = message.AlarmActivated.ToString();
        item.SubItems[3].Text = message.AlarmType.ToString();
        item.SubItems[4].Text = message.AlarmMinutes.ToString();
        item.SubItems[5].Text = message.NotificationType.ToString();
        item.SubItems[6].Text = message.Comment.ToString();
    }

    private async void EditTask()
    {
        if (listViewTasks.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no task selected.", KntConst.AppName);
            return;
        }
        var idTask = Guid.Parse(listViewTasks.SelectedItems[0].Name);
        var task = _ctrl.EditTask(idTask);
        if (task != null)
            await UpdateTask(task);
    }

    private async Task UpdateTask(NoteTaskDto task)
    {
        var item = listViewTasks.Items[task.NoteTaskId.ToString()];
        item.SubItems[0].Text = task.Priority.ToString();
        item.SubItems[1].Text = task.Tags.ToString();
        item.SubItems[2].Text = task.Resolved.ToString();
        item.SubItems[3].Text = task.StartDate.ToString();
        item.SubItems[4].Text = task.EndDate.ToString();
        item.SubItems[5].Text = task.EstimatedTime.ToString();
        item.SubItems[6].Text = task.SpentTime.ToString();
        item.SubItems[7].Text = task.DifficultyLevel.ToString();
        item.SubItems[8].Text = task.ExpectedStartDate.ToString();
        item.SubItems[9].Text = task.ExpectedEndDate.ToString();
        item.SubItems[10].Text = task.UserFullName.ToString();
        listViewTasks.Scrollable = true;

        ListViewSelectionHelper.SelectByKey(listViewTasks, task.NoteTaskId.ToString(), _tasksSorter);
        await UpdateTaskDescription(task.Description);
    }

    private async Task EditResource()
    {
        if (_selectedResource == null)
        {
            MessageBox.Show("There is no resource selected.", KntConst.AppName);
            return;
        }
        var idResource = _selectedResource.ResourceId;
        var resource = await _ctrl.EditResource(idResource);
        if (resource != null)
        {
            _selectedResource = resource;
            UpdateResource(resource);
            buttonUndo.Enabled = true;
        }
    }

    private void EditNoteAttribute()
    {
        if (listViewAttributes.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no attribute selected.", KntConst.AppName);
            return;
        }
        var idAttribute = Guid.Parse(listViewAttributes.SelectedItems[0].Name);
        var noteAttribute = _ctrl.Model.KAttributesDto.Where(_ => _.NoteKAttributeId == idAttribute).SingleOrDefault();
        var noteAttributeEdited = _ctrl.EditAttribute(noteAttribute);
        if (noteAttributeEdited != null)
        {
            // Refrescar listView
            UpdateNoteAttribute(noteAttributeEdited);
            buttonUndo.Enabled = true;
        }
    }

    private async Task<ResourceDto> AddResource()
    {
        var resource = await _ctrl.NewResource();
        if (resource != null)
        {
            AddItemToListViewResources(resource);
        }
        return resource;
    }

    private ResourceDto AddResourceFromClipboard()
    {
        var resource = _ctrl.NewResourceFromClipboard();
        if (resource != null)
        {
            AddItemToListViewResources(resource);
        }
        return resource;
    }

    private ResourceDto AddResourceFromFile(string filePath)
    {
        var resource = _ctrl.NewResourceFromFile(filePath);
        if (resource != null)
        {
            AddItemToListViewResources(resource);
        }
        return resource;
    }

    private void Content_DragEnter(object sender, DragEventArgs e)
    {
        // ConfigureEmbededMode() (e.g. the note shown inside KNoteManagment) sets EditMode = false
        // and makes the description read-only - dropping a file there would silently attach a
        // resource nobody could reference into the text, so reject the drop outright. Same reasoning
        // applies when the note itself is locked for editing.
        var allowDrop = _ctrl.EditMode && !_ctrl.Model.GetContentTypeExt().DescriptionBlocked && e.Data.GetDataPresent(DataFormats.FileDrop);
        e.Effect = allowDrop ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void Content_DragDrop(object sender, DragEventArgs e)
    {
        if (!_ctrl.EditMode || _ctrl.Model.GetContentTypeExt().DescriptionBlocked || !e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        for (int i = 0; i < files.Length; i++)
        {
            var resource = AddResourceFromFile(files[i]);
            if (resource != null)
                InsertLinkSelectedResource(prependSeparator: i > 0);
        }
    }

    private void AddItemToListViewResources(ResourceDto resource)
    {
        listViewResources.Items.Add(ResourceDtoToListViewItem(resource));
        _selectedResource = resource;
        ListViewSelectionHelper.SelectByKey(listViewResources, resource.ResourceId.ToString(), _resourcesSorter);
    }

    private void InsertLinkSelectedResource(bool prependSeparator = false)
    {
        // Reached both from toolbar buttons (already disabled while locked) and from
        // Content_DragDrop (a drag&drop is not gated by any button's Enabled state).
        if (_ctrl.Model.GetContentTypeExt().DescriptionBlocked)
        {
            ShowInfo("This note is locked and cannot be edited.");
            return;
        }

        // If navigate mode then msgbox and return
        if (!buttonNavigate.Enabled)
        {
            _ctrl.ShowMessage("Cannot insert a resource into the text when editing mode (markdown or html) is not active", KntConst.AppName);
            return;
        }

        var tmpFile = _ctrl.Service.Notes.UtilGetResourceFileUrl(_selectedResource.Container, _selectedResource.Name);

        tabNoteData.SelectedIndex = 0;

        if (!buttonViewHtml.Enabled)
        {
            string strLink = (_selectedResource.FileType.Contains("image")) ?
                $"<img src='{tmpFile}' alt='{_selectedResource.Description}'/>" :
                $"<a href='{tmpFile}' target='_blank'>{_selectedResource.NameOut}</a>";
            kntEditView.HtmlContentControl.SelectedHtml = strLink;
            kntEditView.HtmlContentControl.Focus();
        }
        else
        {
            string strLink = (_selectedResource.FileType.Contains("image")) ?
                $"![alt text]({tmpFile} '{_selectedResource.Description}')" : $"[{_selectedResource.NameOut}]({tmpFile} '{_selectedResource.Description}')";
            // Dropping several files at once inserts them one after another at the same caret
            // position (each insertion advances it) - a blank line between them (not just after
            // the first) keeps the list readable instead of running the links together. A plain
            // "\n" is not enough here: a WinForms TextBox only renders a line break for "\r\n".
            if (prependSeparator)
                strLink = "\r\n\r\n" + strLink;
            var selStart = kntEditView.MarkdownContentControl.SelectionStart;
            kntEditView.MarkdownContentControl.Text = kntEditView.MarkdownContentControl.Text.Insert(selStart, strLink);
            kntEditView.MarkdownContentControl.SelectionStart = selStart + strLink.Length;
            kntEditView.MarkdownContentControl.BeginInvoke(new Action(() => kntEditView.MarkdownContentControl.ScrollToCaret()));
        }
    }

    private void TextSearch()
    {
        // If navigate mode then msgbox and return
        if (!buttonNavigate.Enabled)
        {
            _ctrl.ShowMessage("Cannot search text when editing mode in markdown format is not active", KntConst.AppName);
            return;
        }

        // Context for new search
        _textSearch = "";
        _indexTextSearch = 0;

        var listVars = new List<ReadVarItem> {new ReadVarItem
        {
            Label = "Text for search",
            VarIdent = "textSearch",
            VarValue = "",
            VarNewValueText = ""
        }};

        // Form for capture text to search
        var formReadVar = new ReadVarForm(listVars);

        formReadVar.Text = "Search for text in the note description";
        formReadVar.Size = new Size(500, 150);

        var result = formReadVar.ShowDialog();

        if (result == DialogResult.Cancel)
            return;
        else
            _textSearch = listVars[0].VarNewValueText;

        if (string.IsNullOrEmpty(_textSearch))
        {
            _ctrl.ShowMessage("Please, insert text for find.", KntConst.AppName);
            return;
        }

        DoTextSearch();
    }

    private void TextSearchNext()
    {
        // If navigate mode then msgbox and return
        if (!buttonNavigate.Enabled)
        {
            _ctrl.ShowMessage("Cannot search text when editing mode in markdown format is not active", KntConst.AppName);
            return;
        }

        if (string.IsNullOrEmpty(_textSearch))
        {
            _ctrl.ShowMessage("You have not entered text to continue a search. Please, insert text for find with Ctrl-F key.", KntConst.AppName);
            return;
        }

        DoTextSearch();
    }

    private void DoTextSearch()
    {
        string text = kntEditView.MarkdownContentControl.Text;
        var _indexTextSearchNew = text.IndexOf(_textSearch, _indexTextSearch, StringComparison.CurrentCultureIgnoreCase);

        if (_indexTextSearchNew != -1)
        {
            // Seleccionar la subcadena encontrada en textBox1
            kntEditView.MarkdownContentControl.Focus();
            kntEditView.MarkdownContentControl.SelectionStart = _indexTextSearchNew;
            kntEditView.MarkdownContentControl.SelectionLength = _textSearch.Length;
            kntEditView.MarkdownContentControl.BeginInvoke(new Action(() => kntEditView.MarkdownContentControl.ScrollToCaret()));
        }
        else
        {
            _ctrl.ShowMessage("Search text not found.", KntConst.AppName);
        }

        _indexTextSearch = _indexTextSearchNew + _textSearch.Length;
    }

    private async Task InsertTemplate()
    {
        if (_ctrl.Model.GetContentTypeExt().DescriptionBlocked)
        {
            ShowInfo("This note is locked and cannot be edited.");
            return;
        }

        // If navigate mode then msgbox and return
        if (!buttonNavigate.Enabled)
        {
            _ctrl.ShowMessage("Cannot insert a template into the text when editing mode (markdown or html) is not active", KntConst.AppName);
            return;
        }

        var noteTemplate = (await _ctrl.GetCatalogTemplate());
        if (noteTemplate == null || string.IsNullOrEmpty(noteTemplate.Description))
            return;

        tabNoteData.SelectedIndex = 0;

        if (!buttonViewHtml.Enabled)
        {
            kntEditView.HtmlContentControl.SelectedHtml = noteTemplate.Description;
            kntEditView.HtmlContentControl.Focus();
        }
        else
        {
            var selStart = kntEditView.MarkdownContentControl.SelectionStart;
            kntEditView.MarkdownContentControl.Text = kntEditView.MarkdownContentControl.Text.Insert(selStart, noteTemplate.Description);
            kntEditView.MarkdownContentControl.Focus();
            kntEditView.MarkdownContentControl.SelectionStart = selStart + noteTemplate.Description.Length;
            kntEditView.MarkdownContentControl.BeginInvoke(new Action(() => kntEditView.MarkdownContentControl.ScrollToCaret()));
        }
    }

    private async Task InsertCode()
    {
        var codeTemplate = (await _ctrl.GetCatalogCode());
        
        if (codeTemplate == null || string.IsNullOrEmpty(codeTemplate.Script))
            return;

        tabNoteData.SelectedIndex = 5;

        var ct = codeTemplate.GetContentTypeExt();
        SetScriptTypeCombo(ct?.ForScript);

        var selStart = textScriptCode.SelectionStart;
        textScriptCode.Text = textScriptCode.Text.Insert(selStart, codeTemplate.Script);
        textScriptCode.SelectionStart = selStart + codeTemplate.Script.Length;
        textScriptCode.BeginInvoke(new Action(() => textScriptCode.ScrollToCaret()));
    }

    private async void ExecKNoteAssistant()
    {
        ProgressBarOn();
        await ControlsToModel();
        await _ctrl.ExecKNoteAssistant();
        RefreshView();
        ProgressBarOff();
    }

    private void UpdateResource(ResourceDto resource)
    {
        var item = listViewResources.Items[resource.ResourceId.ToString()];
        item.Text = resource.Order.ToString();
        item.SubItems[1].Text = resource.NameOut;
        item.SubItems[2].Text = resource.FileType;
        ListViewSelectionHelper.SelectByKey(listViewResources, resource.ResourceId.ToString(), _resourcesSorter);
        UpdatePreviewResource(resource);
    }

    private void UpdateNoteAttribute(NoteKAttributeDto noteAttribute)
    {
        var item = listViewAttributes.Items[noteAttribute.NoteKAttributeId.ToString()];
        // item.Text (column 0, Order) is left untouched - editing a note attribute's Value never
        // changes its defined Order, and this list has no interactive sort to reapply (see
        // ModelToControlsAttributes), so the row's position never needs to move.
        item.SubItems[1].Text = noteAttribute.Name;
        item.SubItems[2].Text = noteAttribute.Value;
        ListViewSelectionHelper.SelectByKey(listViewAttributes, noteAttribute.NoteKAttributeId.ToString());
    }

    private async Task UpdateTaskDescription(string description)
    {
        kntEditViewTask.SetMarkdownContent(_ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForRead(description, true));
        var htmlContent = _ctrl.Service.Notes.UtilMarkdownToHtml(kntEditViewTask.MarkdownText.Replace(_ctrl.Service.RepositoryRef.ResourcesContainerRootUrl, KntConst.VirtualHostNameToFolderMapping));
        await kntEditViewTask.SetVirtualHostNameToFolderMapping(_ctrl.Service.RepositoryRef.ResourcesContainerRootPath);
        await kntEditViewTask.ShowNavigationContent(htmlContent + _ctrl.Store.KNoteWebViewStyle);
    }

    private async Task AddTaskFromSelectedText()
    {
        if (_ctrl.Model.GetContentTypeExt().DescriptionBlocked)
        {
            ShowInfo("This note is locked and cannot be edited.");
            return;
        }

        var selText = kntEditView.MarkdownContentControl.SelectedText;
        if (!string.IsNullOrEmpty(selText))
        {
            selText = _ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForWrite(selText, true);
        }
        var taskSaved = await AddTask(selText);
        if (taskSaved)
        {
            // Remove selected text
            var selStart = kntEditView.MarkdownContentControl.SelectionStart;
            kntEditView.MarkdownContentControl.Text = kntEditView.MarkdownContentControl.Text.Remove(selStart, kntEditView.MarkdownContentControl.SelectedText.Length);
            kntEditView.MarkdownContentControl.Focus();
            kntEditView.MarkdownContentControl.SelectionStart = selStart;
        }
    }

    private async Task<bool> AddTask(string defaultDescription = "")
    {
        NoteTaskDto task = await _ctrl.NewTask(defaultDescription, listViewTasks.Items.Count);
        if (task != null)
        {
            listViewTasks.Items.Add(NoteTaskDtoToListViewItem(task));
            ListViewSelectionHelper.SelectByKey(listViewTasks, task.NoteTaskId.ToString(), _tasksSorter);
            await UpdateTaskDescription(task.Description);
            return true;
        }
        return false;
    }

    private async Task ResourceDelete()
    {
        if (listViewResources.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no task selected .", KntConst.AppName);
            return;
        }
        var delRes = listViewResources.SelectedItems[0].Name;
        var res = _ctrl.DeleteResource(Guid.Parse(delRes));
        if (res)
        {
            listViewResources.Items[delRes].Remove();
            _selectedResource = null;
            await webViewResource.ClearWebView();
            webViewResource.Visible = false;
            panelPreview.Visible = true;
            textDescriptionResource.Text = "";
            if (listViewResources.Items.Count > 0)
                ListViewSelectionHelper.SelectFirst(listViewResources, _resourcesSorter);
            else
            {
                await webViewResource.ShowNavigationContent("");
                webViewResource.Visible = true;
                panelPreview.Visible = false;
            }
        }
    }

    private void ProgressBarOn()
    {
        progressStatus.Visible = true;
        progressStatus.MarqueeAnimationSpeed = 40;
    }

    private void ProgressBarOff()
    {
        progressStatus.Visible = false;
        progressStatus.MarqueeAnimationSpeed = 0;
    }

    #endregion
}