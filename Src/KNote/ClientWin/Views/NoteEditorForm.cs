using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class NoteEditorForm : Form, IViewEditorEmbeddable<NoteExtendedDto>
{
    #region Private fields

    private readonly NoteEditorCtrl _ctrl;
    private bool _viewFinalized = false;

    private Guid _selectedFolderId;
    private ResourceDto _selectedResource;

    #endregion 

    #region Constructor

    public NoteEditorForm(NoteEditorCtrl ctrl)
    {
        //AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = ctrl;

        // TODO: options for new versión
        buttonPrint.Visible = false;
        buttonCheck.Visible = false;
        toolStripS3.Visible = false;
        toolStripS4.Visible = false;                
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
        //textTaskDescription.Text = "";  // !!!
        await kntEditViewTask.ClearWebView();
        textTaskTags.Text = "";
        textScriptCode.Text = "";
        listViewAttributes.Clear();
        listViewResources.Clear();
        listViewTasks.Clear();
        listViewAlarms.Clear();
    }

    public void RefreshView()
    {
        ModelToControls();
    }

    public void RefreshViewOnlyRequiredCtrl()
    {
        ModelToControlsOnlyRequiredComponents();
    }


    public void RefreshModel()
    {
        ControlsToModel();
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
            _ctrl.RunScript();
        }
        else if (menuSel == buttonLockFormat)
        {
            if (buttonLockFormat.Checked)
            {
                _ctrl.Model.ContentType = _ctrl.Model.ContentType.Replace("#", "");
                buttonLockFormat.Checked = false;
            }
            else
            {
                _ctrl.Model.ContentType = "#" + _ctrl.Model.ContentType;
                buttonLockFormat.Checked = true;
            }
        }
        else if (menuSel == buttonInsertTemplate)
        {
            await InsertTemplate();
        }
        else if (menuSel == buttonInsertCode)
        {
            await InsertCode();
        }

        // 
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
            if (kntEditView.ContentType == "html")
            {
                if (_ctrl.Model.ContentType.Contains('#'))
                {
                    ShowInfo($"This note cannot be changed to another format, the format is locked.");
                    return;
                }

                kntEditView.ShowMarkdownContent(_ctrl.Service.Notes.UtilHtmlToMarkdown(kntEditView.BodyHtml));
            }
            else
                kntEditView.ShowMarkdownContent();

            _ctrl.Model.ContentType = "markdown";

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
            if (_ctrl.Model.ContentType.Contains('#'))
            {
                ShowInfo($"This note cannot be changed to another format, the format is locked.");
                return;
            }

            var url = _ctrl.Store.ExtractUrlFromText(kntEditView.MarkdownText);
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
                await kntEditView.ShowNavigationContent(htmlContent);
            }

            _ctrl.Model.ContentType = "navigation";

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
            if (_ctrl.Model.ContentType.Contains('#'))
            {
                ShowInfo($"This note cannot be changed to another format, the format is locked.");
                return;
            }

            kntEditView.ShowHtmlContent(_ctrl.Service.Notes.UtilMarkdownToHtml(kntEditView.MarkdownText));

            _ctrl.Model.ContentType = "html";

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

    private void listView_Resize(object sender, EventArgs e)
    {
        SizeLastColumn((ListView)sender);
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
        kntEditView.MarkdownContentControl.SelectionStart = selStart;
    }

    private void buttonFolderSearch_Click(object sender, EventArgs e)
    {
        var folder = _ctrl.GetFolder();
        if (folder != null)
        {
            _selectedFolderId = folder.FolderId;
            textFolder.Text = folder?.Name;
            textFolderNumber.Text = "#" + folder.FolderNumber.ToString();

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

    #region Messages managment

    private async void buttonAddAlarm_Click(object sender, EventArgs e)
    {
        var message = await _ctrl.NewMessage();
        if (message != null)
            listViewAlarms.Items.Add(MessageDtoToListViewItem(message));

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
        NoteTaskDto task = await _ctrl.NewTask();
        if (task != null)
        {
            listViewTasks.Items.Add(NoteTaskDtoToListViewItem(task));
            listViewTasks.Items[listViewTasks.Items.Count - 1].Selected = true;

            await UpdateTaskDescription(task.Description);

            textTaskTags.Text = task.Tags;
        }
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
            await kntEditViewTask.ClearWebView();
            textTaskTags.Text = "";
            listViewTasks.Items[delTsk].Remove();
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
                listViewResources.Items[0].Selected = true;
        }
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

                textTaskTags.Text = selTask.Tags;
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

        kntEditView.ContentType = _ctrl.Model.ContentType;

        if (!_ctrl.EditMode)
        {
            foreach (var tab in tabNoteData.TabPages)
            {
                foreach (Control conTmp in ((TabPage)tab).Controls)
                {
                    BlockControl(conTmp);
                }
            }
            kntEditView.HtmlEditorEditMode = false;
        }

        panelDescription.Visible = true;

        webViewResource.ShowNavigationTools = false;
        webViewResource.Location = new Point(396, 36);
        panelPreview.Location = new Point(396, 36);
        if (_ctrl.EditMode)
        {
            webViewResource.Size = new Size(392, 464);
            panelPreview.Size = new Size(392, 464);
        }
        else
        {
            webViewResource.Size = new Size(392, 490);
            panelPreview.Size = new Size(392, 490);
        }

        textDescriptionResource.ReadOnly = true;
        textDescriptionResource.BackColor = Color.White;

        // TODO: !!! refactor this
        //textTaskDescription.ReadOnly = true;
        //textTaskDescription.BackColor = Color.White;
        kntEditViewTask.ShowStatusInfo = false;
        kntEditViewTask.EnableUrlBox = false;
        kntEditViewTask.ShowNavigationTools = false;
        kntEditViewTask.BorderStyle = BorderStyle.FixedSingle;


        textTaskTags.ReadOnly = true;
        textTaskTags.BackColor = Color.White;

        PersonalizeListView(listViewAttributes);
        PersonalizeListView(listViewResources);
        PersonalizeListView(listViewTasks);
        PersonalizeListView(listViewAlarms);

        // TODO: remove in this version
        tabNoteData.TabPages.Remove(tabTraceNotes);
    }

    private void ModelToControlsOnlyRequiredComponents()
    {
        textNoteNumber.Text = "#" + _ctrl.Model.NoteNumber.ToString();
        textFolderNumber.Text = "#" + _ctrl.Model.FolderDto.FolderNumber.ToString();
        textStatus.Text = _ctrl.Model.InternalTags;
        buttonLockFormat.Checked = _ctrl.Model.ContentType != null && _ctrl.Model.ContentType.Contains('#');

        this.Update();
        this.Refresh();
    }

    private async void ModelToControls()
    {
        // Basic data            
        Text = $"Note editor [{_ctrl.ServiceRef?.Alias}]";
        textTopic.Text = _ctrl.Model.Topic;
        textNoteNumber.Text = "#" + _ctrl.Model.NoteNumber.ToString();
        textFolder.Text = _ctrl.Model.FolderDto?.Name;
        textFolderNumber.Text = "#" + _ctrl.Model.FolderDto.FolderNumber.ToString();
        _selectedFolderId = _ctrl.Model.FolderId;
        textTags.Text = _ctrl.Model.Tags;
        textStatus.Text = _ctrl.Model.InternalTags;
        textPriority.Text = _ctrl.Model.Priority.ToString();

        kntEditView.SetMarkdownContent(_ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForRead(_ctrl.Model?.Description, true));

        if (_ctrl.Model.ContentType.Contains("html"))
        {
            labelLoadingHtml.Visible = true;
            labelLoadingHtml.Refresh();
            kntEditView.ShowHtmlContent(kntEditView.MarkdownText);
            labelLoadingHtml.Visible = false;
            EnableHtmlView();
        }
        else if (_ctrl.Model.ContentType.Contains("navigation"))
        {
            if (!string.IsNullOrEmpty(kntEditView.MarkdownText))
            {
                var url = _ctrl.Store.ExtractUrlFromText(kntEditView.MarkdownText);
                if (!string.IsNullOrEmpty(url))
                {
                    await kntEditView.ShowNavigationUrlContent(url);
                }
                else
                {
                    var htmlContent = _ctrl.Service.Notes.UtilMarkdownToHtml(kntEditView.MarkdownText.Replace(_ctrl.Service.RepositoryRef.ResourcesContainerRootUrl, KntConst.VirtualHostNameToFolderMapping));
                    await kntEditView.SetVirtualHostNameToFolderMapping(_ctrl.Service.RepositoryRef.ResourcesContainerRootPath);
                    await kntEditView.ShowNavigationContent(htmlContent);
                }
            }
            EnableNavigationView();
        }
        else
        {
            kntEditView.ShowMarkdownContent();
            EnableMarkdownView();
        }

        buttonLockFormat.Checked = _ctrl.Model.ContentType != null && _ctrl.Model.ContentType.Contains('#');

        // KAttributes           
        textNoteType.Text = _ctrl.Model.NoteTypeDto.Name;
        ModelToControlsAttributes();

        // Resources 
        ModelToControlsResources();
        if (_ctrl.Model.Resources.Count > 0)
            listViewResources.Items[0].Selected = true;
        else
            UpdatePreviewResource(null);

        // Tasks
        ModelToControlsTasks();
        if (_ctrl.Model.Tasks.Count > 0)
            listViewTasks.Items[0].Selected = true;
        else
        {
            //textTaskDescription.Text = "";  // !!!
            await kntEditViewTask.ClearWebView();
            textTaskTags.Text = "";
        }

        // Alarms     
        ModelToControlsAlarms();

        // Script             
        textScriptCode.Text = _ctrl.Model.Script;

        this.Update();
        this.Refresh();
    }

    private void ModelToControlsAttributes()
    {
        listViewAttributes.Clear();

        foreach (var atr in _ctrl.Model.KAttributesDto)
        {
            var itemList = new ListViewItem(atr.Name);
            itemList.Name = atr.NoteKAttributeId.ToString();
            //itemList.BackColor = Color.LightGray;
            itemList.SubItems.Add(atr.Value);
            listViewAttributes.Items.Add(itemList);
        }

        // Width of -2 indicates auto-size.
        listViewAttributes.Columns.Add("Name", 250, HorizontalAlignment.Left);
        listViewAttributes.Columns.Add("Value", -2, HorizontalAlignment.Left);
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

        listViewResources.Columns.Add("Name", 200, HorizontalAlignment.Left);
        listViewResources.Columns.Add("File type", 100, HorizontalAlignment.Left);
        listViewResources.Columns.Add("Order", 70, HorizontalAlignment.Left);
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
        listViewTasks.Columns.Add("User", 150, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Priority", 50, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Resolved", 60, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Start", 120, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("End", 120, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Est. time", 50, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Spend time", 50, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Dif.", 50, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Ex start", 120, HorizontalAlignment.Left);
        listViewTasks.Columns.Add("Ex end", 120, HorizontalAlignment.Left);
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
        listViewAlarms.Columns.Add("User", 130, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("Activated", 80, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("Date time", 130, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("Alarm periodicity", 120, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("Min", 50, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("Notification type", 120, HorizontalAlignment.Left);
        listViewAlarms.Columns.Add("Comment", -2, HorizontalAlignment.Left);
    }

    private async void UpdatePreviewResource(ResourceDto resource)
    {
        _selectedResource = resource;

        if (webViewResource.Visible)
            await webViewResource.ClearWebView();
        textDescriptionResource.Text = "";

        if (_selectedResource == null)
            return;

        textDescriptionResource.Text = _selectedResource.Description;

        if (_ctrl.Store.IsSupportedFileTypeForPreview(_selectedResource.FileType))
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

    private void ControlsToModel()
    {
        // Basic data
        _ctrl.Model.Topic = textTopic.Text;
        _ctrl.Model.FolderId = _selectedFolderId;
        _ctrl.Model.FolderDto.FolderId = _selectedFolderId;
        _ctrl.Model.FolderDto.Name = textFolder.Text;
        _ctrl.Model.FolderDto.FolderNumber = int.Parse(textFolderNumber.Text.Substring(1));
        _ctrl.Model.Tags = textTags.Text;
        _ctrl.Model.InternalTags = textStatus.Text;

        if (_ctrl.Model.ContentType.Contains("html"))
            _ctrl.Model.Description = _ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForWrite(kntEditView.BodyHtml, true);
        else
            _ctrl.Model.Description = _ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForWrite(kntEditView.MarkdownText, true);

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
        var itemList = new ListViewItem(message.UserFullName);
        itemList.Name = message.KMessageId.ToString();
        itemList.SubItems.Add(message.AlarmActivated.ToString());
        itemList.SubItems.Add(message.AlarmDateTime.ToString());
        itemList.SubItems.Add(message.AlarmType.ToString());
        itemList.SubItems.Add(message.AlarmMinutes.ToString());
        itemList.SubItems.Add(message.NotificationType.ToString());
        itemList.SubItems.Add(message.Comment.ToString());
        return itemList;
    }

    private ListViewItem NoteTaskDtoToListViewItem(NoteTaskDto task)
    {
        var itemList = new ListViewItem(task.UserFullName);
        itemList.Name = task.NoteTaskId.ToString();
        itemList.SubItems.Add(task.Priority.ToString());
        itemList.SubItems.Add(task.Resolved.ToString());
        itemList.SubItems.Add(task.StartDate.ToString());
        itemList.SubItems.Add(task.EndDate.ToString());
        itemList.SubItems.Add(task.EstimatedTime.ToString());
        itemList.SubItems.Add(task.SpentTime.ToString());
        itemList.SubItems.Add(task.DifficultyLevel.ToString());
        itemList.SubItems.Add(task.ExpectedStartDate.ToString());
        itemList.SubItems.Add(task.ExpectedEndDate.ToString());
        return itemList;
    }

    private ListViewItem ResourceDtoToListViewItem(ResourceDto resource)
    {
        var itemList = new ListViewItem(resource.NameOut);
        itemList.Name = resource.ResourceId.ToString();
        itemList.SubItems.Add(resource.FileType);
        itemList.SubItems.Add(resource.Order.ToString());
        return itemList;
    }

    private void SizeLastColumn(ListView lv)
    {
        // Hack for control undeterminated error
        try
        {
            lv.Columns[lv.Columns.Count - 1].Width = -2;
        }
        catch (Exception)
        {
        }
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
        item.SubItems[1].Text = message.AlarmActivated.ToString();
        item.SubItems[2].Text = message.AlarmDateTime.ToString();
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
        item.SubItems[1].Text = task.Priority.ToString();
        item.SubItems[2].Text = task.Resolved.ToString();
        item.SubItems[3].Text = task.StartDate.ToString();
        item.SubItems[4].Text = task.EndDate.ToString();
        item.SubItems[5].Text = task.EstimatedTime.ToString();
        item.SubItems[6].Text = task.SpentTime.ToString();
        item.SubItems[7].Text = task.DifficultyLevel.ToString();
        item.SubItems[8].Text = task.ExpectedStartDate.ToString();
        item.SubItems[9].Text = task.ExpectedEndDate.ToString();
        listViewTasks.Scrollable = true;

        await UpdateTaskDescription(task.Description);

        textTaskTags.Text = task.Tags;
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

    private void AddItemToListViewResources(ResourceDto resource)
    {
        listViewResources.Items.Add(ResourceDtoToListViewItem(resource));
        _selectedResource = resource;
        listViewResources.Items[resource.ResourceId.ToString()].Selected = true;
    }

    private void InsertLinkSelectedResource()
    {
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
            var selStart = kntEditView.MarkdownContentControl.SelectionStart;
            kntEditView.MarkdownContentControl.Text = kntEditView.MarkdownContentControl.Text.Insert(selStart, strLink);
            kntEditView.MarkdownContentControl.Focus();
            kntEditView.MarkdownContentControl.Select(selStart + strLink.Length, 0);
        }
    }

    private async Task InsertTemplate()
    {
        // If navigate mode then msgbox and return
        if (!buttonNavigate.Enabled)
        {
            _ctrl.ShowMessage("Cannot insert a template into the text when editing mode (markdown or html) is not active", KntConst.AppName);
            return;
        }

        var strContent = await _ctrl.GetCatalogTemplate();
        if (string.IsNullOrEmpty(strContent))
            return;

        tabNoteData.SelectedIndex = 0;

        if (!buttonViewHtml.Enabled)
        {
            kntEditView.HtmlContentControl.SelectedHtml = strContent;
            kntEditView.HtmlContentControl.Focus();
        }
        else
        {
            var selStart = kntEditView.MarkdownContentControl.SelectionStart;
            kntEditView.MarkdownContentControl.Text = kntEditView.MarkdownContentControl.Text.Insert(selStart, strContent);
            kntEditView.MarkdownContentControl.Focus();
            kntEditView.MarkdownContentControl.Select(selStart + strContent.Length, 0);
        }
    }

    private async Task InsertCode()
    {
        var strContent = await _ctrl.GetCatalogCode();
        if (string.IsNullOrEmpty(strContent))
            return;

        tabNoteData.SelectedIndex = 5;

        var selStart = textScriptCode.SelectionStart;
        textScriptCode.Text = textScriptCode.Text.Insert(selStart, strContent);
        textScriptCode.Focus();
        textScriptCode.Select(selStart + strContent.Length, 0);
    }


    private void UpdateResource(ResourceDto resource)
    {
        var item = listViewResources.Items[resource.ResourceId.ToString()];
        item.Text = resource.NameOut;
        item.SubItems[1].Text = resource.FileType;
        item.SubItems[2].Text = resource.Order.ToString();
        UpdatePreviewResource(resource);
    }

    private void UpdateNoteAttribute(NoteKAttributeDto noteAttribute)
    {
        var item = listViewAttributes.Items[noteAttribute.NoteKAttributeId.ToString()];
        item.Text = noteAttribute.Name;
        item.SubItems[1].Text = noteAttribute.Value;
    }

    private async Task UpdateTaskDescription(string description)
    {
        kntEditViewTask.SetMarkdownContent(_ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForRead(description, true));
        var htmlContent = _ctrl.Service.Notes.UtilMarkdownToHtml(kntEditViewTask.MarkdownText.Replace(_ctrl.Service.RepositoryRef.ResourcesContainerRootUrl, KntConst.VirtualHostNameToFolderMapping));
        await kntEditViewTask.SetVirtualHostNameToFolderMapping(_ctrl.Service.RepositoryRef.ResourcesContainerRootPath);
        await kntEditViewTask.ShowNavigationContent(htmlContent);
    }

    #endregion

}