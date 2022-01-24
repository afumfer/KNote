using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using Markdig;
using System.Data;
using System.Diagnostics;

namespace KNote.ClientWin.Views;

public partial class NoteEditorForm : Form, IEditorView<NoteExtendedDto>
{
    #region Private fields

    private readonly NoteEditorComponent _com;
    private bool _viewFinalized = false;

    private Guid _selectedFolderId;        
    private ResourceDto _selectedResource;

    #endregion 

    #region Constructor

    public NoteEditorForm(NoteEditorComponent com)
    {
        InitializeComponent();
        _com = com;

        // TODO: options for new versión
        buttonPrint.Visible = false;
        buttonCheck.Visible = false;
        toolStripS3.Visible = false;
        toolStripS4.Visible = false;
        buttonInsertTemplate.Visible = false;
        toolStripToolS1.Visible = false;                        
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

    public Result<EComponentResult> ShowModalView()
    {
        return _com.DialogResultToComponentResult(this.ShowDialog());
    }

    public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
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
        textDescription.Text = "";
        htmlDescription.BodyHtml = "";
        await webView2.NavigateToString(" ");
        textPriority.Text = "";
        textDescriptionResource.Text = "";
        await webViewResource.NavigateToString(" ");
        webViewResource.Visible = true;
        panelPreview.Visible = false;
        textTaskDescription.Text = "";
        textTaskTags.Text = "";
        listViewAttributes.Clear();
        listViewResources.Clear();
        listViewTasks.Clear();
        listViewAlarms.Clear();
    }

    public void RefreshView()
    {
        ModelToControls();
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
        _com.EditMode = false;
    }

    public void ConfigureWindowMode()
    {
        TopLevel = true;
        Dock = DockStyle.None;
        FormBorderStyle = FormBorderStyle.Sizable;
        toolBarNoteEditor.Visible = true;            
        StartPosition = FormStartPosition.CenterScreen;
        _com.EditMode = true;
    }

    #endregion

    #region Form events handlers

    private void NoteEditorForm_Load(object sender, EventArgs e)
    {
        PersonalizeControls();
    }

    private void WebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
    {
        //
    }

    private async void NoteEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var savedOk = await SaveModel();
            if (!savedOk)
                if (MessageBox.Show("Do yo want exit?", "KaNote", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    _com.Finalize();
                else
                {
                    e.Cancel = true;
                    return;
                }
            _com.Finalize();
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
            DeleteModel();
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
            _com.Model.Script = textScriptCode.Text;
            _com.RunScript();
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
        if (htmlDescription.Visible)
        {
            var config = new ReverseMarkdown.Config
            {
                UnknownTags = ReverseMarkdown.Config.UnknownTagsOption.PassThrough, // Include the unknown tag completely in the result (default as well)
                GithubFlavored = true, // generate GitHub flavoured markdown, supported for BR, PRE and table tags
                RemoveComments = false, // will ignore all comments
                SmartHrefHandling = true // remove markdown output for links where appropriate
            };       
            var converter = new ReverseMarkdown.Converter(config);
            string html = htmlDescription.BodyHtml;
            string result = converter.Convert(html);
            textDescription.Text = result;
        }
        else if (webView2.Visible)
        {
            textDescription.Text = webView2.TextUrl;
        }

        _com.Model.ContentType = "markdown";

        EnableMarkdownView();
    }

    private void buttonViewHtml_Click(object sender, EventArgs e)
    {
        var MarkdownContent = textDescription.Text;
        var pipeline = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var HtmlContent = Markdig.Markdown.ToHtml(MarkdownContent, pipeline);

        htmlDescription.BodyHtml = HtmlContent;
        _com.Model.ContentType = "html";

        EnableHtmlView();
    }

    private async void buttonNavigate_Click(object sender, EventArgs e)
    {
        webView2.TextUrl = textDescription.Text;
        await webView2.Navigate();
        _com.Model.ContentType = "navigation";

        EnableWebView2View();
    }

    private void listViewResources_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (listViewResources.SelectedItems.Count > 0)
            {
                // Cursor = Cursors.WaitCursor;
                var idResource = (Guid.Parse(listViewResources.SelectedItems[0].Name));
                var selRes = _com.Model.Resources.Where(_ => _.ResourceId == idResource).FirstOrDefault();
                UpdatePreviewResource(selRes);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OnSelectedResourceItemChanged error: {ex.Message}");
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
            htmlDescription.SelectedHtml = "<h1>Título 1</h1>";
            htmlDescription.Focus();
        }
        if (menuSel == toolDescriptionHtmlTitle2)
        {
            htmlDescription.SelectedHtml = "<h2>Título 1</h2>";
            htmlDescription.Focus();
        }
        if (menuSel == toolDescriptionHtmlTitle3)
        {
            htmlDescription.SelectedHtml = "<h3>Título 1</h3>";
            htmlDescription.Focus();
        }
        if (menuSel == toolDescriptionHtmlTitle4)
        {
            htmlDescription.SelectedHtml = "<h4>Título 1</h4>";
            htmlDescription.Focus();
        }
        if (menuSel == toolDescriptionHtmlEdit)
        {
            htmlDescription.HtmlContentsEdit();
            htmlDescription.Focus();
        }            
    }

    private void toolDescriptionMarkdown_Click(object sender, EventArgs e)
    {
        ToolStripItem menuSel;
        menuSel = (ToolStripItem)sender;

        string tag = "";
        var nl = System.Environment.NewLine;

        if (menuSel == toolDescriptionMarkdownBold)
            tag = " **_**";
        else if (menuSel == toolDescriptionMarkdownStrikethrough)
            tag = " ~~_~~";
        else if (menuSel == toolDescriptionMarkdownItalic)
            tag = " *_*";
        else if (menuSel == toolDescriptionMarkdownH1)
            tag = nl + "# ";
        else if (menuSel == toolDescriptionMarkdownH2)
            tag = nl + "## ";
        else if (menuSel == toolDescriptionMarkdownH3)
            tag = nl + "### ";
        else if (menuSel == toolDescriptionMarkdownH4)
            tag = nl + "#### ";
        else if (menuSel == toolDescriptionMarkdownList)
            tag = "-_";
        else if (menuSel == toolDescriptionMarkdownListOrdered)
            tag = nl + "1._";
        else if (menuSel == toolDescriptionMarkdownLine)
            tag = nl + "------------";
        else if (menuSel == toolDescriptionMarkdownLink)
            tag = nl + "[xx](http://xx 'xx')";
        else if (menuSel == toolDescriptionMarkdownImage)
            tag = nl + "![Title](Address 'Title')](http://url 'Title')";
        else if (menuSel == toolDescriptionMarkdownCode)
            tag = nl + "```_```";
        else if (menuSel == toolDescriptionMarkdownTable)
        {
            tag = nl + nl;
            tag += "|   |   |" + nl;
            tag += "| ------------ | ------------ |" + nl;
            tag += "|   |   |" + nl;
            tag += "|   |   |" + nl;
        }

        var selStart = textDescription.SelectionStart;
        textDescription.Text = textDescription.Text.Insert(selStart, tag);
        textDescription.Focus();
        textDescription.SelectionStart = selStart;
    }

    private void buttonFolderSearch_Click(object sender, EventArgs e)
    {
        var folder = _com.GetFolder();
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
        var changed = await _com.RequestChangeNoteType(_com.Model.NoteTypeId);

        if(changed)
        {
            textNoteType.Text = _com.Model.NoteTypeDto?.Name;                
            ModelToControlsAttributes();
        }
    }

    private async void buttonDeleteType_Click(object sender, EventArgs e)
    {
        var changed = await _com.AplyChangeNoteType(null);
        if (changed)
        {
            textNoteType.Text = _com.Model.NoteTypeDto?.Name;
            ModelToControlsAttributes();
        }
    }

    private void buttonAttributeEdit_Click(object sender, EventArgs e)
    {
        EditNoteAttribute();
    }

    #region Messages managment

    private async void buttonAddAlarm_Click(object sender, EventArgs e)
    {
        var message = await _com.NewMessage();
        if (message != null)
            listViewAlarms.Items.Add(MessageDtoToListViewItem(message));

    }

    private void buttonEditAlarm_Click(object sender, EventArgs e)
    {
        EditAlarm();
    }

    private async void buttonDeleteAlarm_Click(object sender, EventArgs e)
    {
        if (listViewAlarms.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no selected alert.", "KaNote");
            return;
        }
        var messageId = Guid.Parse(listViewAlarms.SelectedItems[0].Name);            
        var res = await _com.DeleteMessage(messageId);
        if (res)
        {
            listViewAlarms.Items[messageId.ToString()].Remove();
        }
    }

    private void listViewAlarms_DoubleClick(object sender, EventArgs e)
    {
        if(_com.EditMode)
            EditAlarm();
    }

    #endregion

    #region Tasks managment

    private async void buttonTaskAdd_Click(object sender, EventArgs e)
    {
        NoteTaskDto task = await _com.NewTask();
        if (task != null)
        {
            listViewTasks.Items.Add(NoteTaskDtoToListViewItem(task));
            listViewTasks.Items[listViewTasks.Items.Count-1].Selected = true;
            textTaskDescription.Text = task.Description;
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
            MessageBox.Show("There is no task selected .", "KaNote");
            return;
        }
        string delTsk = listViewTasks.SelectedItems[0].Name;
        bool res = await _com.DeleteTask(Guid.Parse(delTsk));
        if (res)
        {
            textTaskDescription.Text = "";
            textTaskTags.Text = "";
            listViewTasks.Items[delTsk].Remove();
        }
    }

    private void listViewTasks_DoubleClick(object sender, EventArgs e)
    {
        if (_com.EditMode)
            EditTask();
    }

    #endregion

    #region Resource managment

    private async void buttonResourceAdd_Click(object sender, EventArgs e)
    {
        await AddResource();
    }
       
    private void buttonResourceEdit_Click(object sender, EventArgs e)
    {
        EditResource();
    }

    private async void buttonResourceDelete_Click(object sender, EventArgs e)
    {
        if (listViewResources.SelectedItems.Count == 0)            
        {
            MessageBox.Show("There is no task selected .", "KaNote");
            return;
        }            
        var delRes = listViewResources.SelectedItems[0].Name;            
        var res = await _com.DeleteResource(Guid.Parse(delRes));            
        if (res)
        {
            listViewResources.Items[delRes].Remove();      
            _selectedResource = null;
            await webViewResource.NavigateToString(" ");
            webViewResource.Visible = false;
            panelPreview.Visible = true;
            textDescriptionResource.Text = "";
            if (listViewResources.Items.Count > 0)
                listViewResources.Items[0].Selected = true;                
        }
    }

    private void listViewResources_DoubleClick(object sender, EventArgs e)
    {
        if (_com.EditMode)
            EditResource();
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
            MessageBox.Show("There is no resource selected.", "KaNote");
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
                if(_selectedResource.ContentInDB)
                    File.WriteAllBytes(fileName, _selectedResource.ContentArrayBytes);
                else
                {
                    string fullPath = _com.Service.Notes.GetResourcePath(_selectedResource);
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

    private void listViewTasks_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (listViewTasks.SelectedItems.Count > 0)
            {
                var idTask = (Guid.Parse(listViewTasks.SelectedItems[0].Name));
                var selTask = _com.Model.Tasks.Where(_ => _.NoteTaskId == idTask).FirstOrDefault();
                textTaskDescription.Text = selTask.Description;
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
        if (_com.EditMode)
            EditNoteAttribute();
    }

    #endregion

    #endregion

    #region Private methods

    private void PersonalizeControls()
    {        
        textDescription.Dock = DockStyle.Fill;
        htmlDescription.Dock = DockStyle.Fill;
        webView2.Dock = DockStyle.Fill;

        if (_com.Model?.ContentType == "html")
            EnableHtmlView();
        else if (_com.Model?.ContentType == "navigation")
            EnableWebView2View();
        else
            EnableMarkdownView();

        if (!_com.EditMode)
        {
            foreach (var tab in tabNoteData.TabPages)
            {
                foreach (Control conTmp in ((TabPage)tab).Controls)
                {
                    BlockControl(conTmp);
                }
            }
            htmlDescription.ToolbarVisible = false;
            htmlDescription.ReadOnly = true;
            webView2.EnableUrlBox = false;
        }

        panelDescription.Visible = true;

        // !!!
        //picResource.Location = new Point(396, 36);
        webViewResource.Location = new Point(396, 36);
        panelPreview.Location = new Point(396, 36);
        if (_com.EditMode)
        {
            //picResource.Size = new Size(392, 464);
            webViewResource.Size = new Size(392, 464);
            panelPreview.Size = new Size(392, 464);
        }
        else
        {
            //picResource.Size = new Size(392, 490);
            webViewResource.Size = new Size(392, 490);
            panelPreview.Size = new Size(392, 490);
        }

        textDescriptionResource.ReadOnly = true;
        textDescriptionResource.BackColor = Color.White;
        textTaskDescription.ReadOnly = true;
        textTaskDescription.BackColor = Color.White;
        textTaskTags.ReadOnly = true;
        textTaskTags.BackColor = Color.White;

        PersonalizeListView(listViewAttributes);
        PersonalizeListView(listViewResources);
        PersonalizeListView(listViewTasks);
        PersonalizeListView(listViewAlarms);

        // TODO: remove in this version
        tabNoteData.TabPages.Remove(tabTraceNotes);        
    }

    private async void ModelToControls()
    {
        //TODO: WaitCursor
        //this.Cursor = Cursors.WaitCursor;

        // Basic data            
        Text = $"Note editor [{_com.ServiceRef?.Alias}]";
        textTopic.Text = _com.Model.Topic;                
        textNoteNumber.Text = "#" + _com.Model.NoteNumber.ToString();
        textFolder.Text = _com.Model.FolderDto?.Name;
        textFolderNumber.Text = "#" + _com.Model.FolderDto.FolderNumber.ToString();
        _selectedFolderId = _com.Model.FolderId;                    
        textTags.Text = _com.Model.Tags;
        textStatus.Text = _com.Model.InternalTags;
        textPriority.Text = _com.Model.Priority.ToString();

        if (_com.Model.ContentType == "html")
        {
            labelLoadingHtml.Visible = true;
            labelLoadingHtml.Refresh();
            textDescription.Visible = false;
            webView2.Visible = false;
            htmlDescription.Visible = true;
            htmlDescription.BodyHtml = "";
            htmlDescription.BodyHtml = _com.Model.ModelToViewDescription(_com.Service?.RepositoryRef);
            htmlDescription.Refresh();
            labelLoadingHtml.Visible = false;
        }
        else if (_com.Model.ContentType == "navigation")
        {
            textDescription.Visible = false;
            htmlDescription.Visible = false;
            webView2.Visible = true;            
            webView2.TextUrl = _com.Model.Description;            
            if (!string.IsNullOrEmpty(_com.Model.Description))
            {
                await webView2.Navigate();
            }
        }
        else
        {
            htmlDescription.Visible = false;
            webView2.Visible = false;
            textDescription.Text = _com.Model.ModelToViewDescription(_com.Service?.RepositoryRef);
            textDescription.Visible = true;                
        }

        // KAttributes           
        textNoteType.Text = _com.Model.NoteTypeDto.Name;            
        ModelToControlsAttributes();

        // Resources 
        ModelToControlsResources();
        if (_com.Model.Resources.Count > 0)            
            listViewResources.Items[0].Selected = true;                                            
        else            
            UpdatePreviewResource(null);                
            
        // Tasks
        ModelToControlsTasks();
        if (_com.Model.Tasks.Count > 0)
            listViewTasks.Items[0].Selected = true;
        else
        {                
            textTaskDescription.Text = "";
            textTaskTags.Text = "";
        }

        // Alarms     
        ModelToControlsAlarms();

        // Script             
        textScriptCode.Text = _com.Model.Script;
            
        // TODO: Trace notes
        //From = new List<TraceNote>(),
        //To = new List<TraceNote>()

        //this.Cursor = Cursors.Default;
    }
        
    private void ModelToControlsAttributes()
    {
        listViewAttributes.Clear();

        foreach(var atr in _com.Model.KAttributesDto)
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
        // !!!
        //picResource.Visible = false;
        webViewResource.Visible = false;

        foreach (var res in _com.Model.Resources)
        {
            if(!res.IsDeleted())
                listViewResources.Items.Add(ResourceDtoToListViewItem(res));
        }

        listViewResources.Columns.Add("Name", 200, HorizontalAlignment.Left);
        listViewResources.Columns.Add("File type", 100, HorizontalAlignment.Left);
        listViewResources.Columns.Add("Order", 70, HorizontalAlignment.Left);            
    }

    private void ModelToControlsTasks()
    {
        listViewTasks.Clear();

        foreach (var task in _com.Model.Tasks)
        {
            if(!task.IsDeleted())
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

        foreach (var msg in _com.Model.Messages)
        {
            if(!msg.IsDeleted())
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

        await webViewResource.NavigateToString(" ");
        textDescriptionResource.Text = "";

        if (_selectedResource == null)
            return;

        textDescriptionResource.Text = _selectedResource.Description;
                                                               
        if (_com.Store.IsSupportedFileTypeForPreview(_selectedResource.FileType))
        {
            webViewResource.Visible = true;
            panelPreview.Visible = false;
            await webViewResource.Navigate(_selectedResource.FullUrl);
        }
        else
        {
            _com.Service.Notes.ManageResourceContent(_selectedResource, false);
            webViewResource.Visible = false;
            panelPreview.Visible = true;
            linkViewFile.Visible = true;
        }
    }

    private void ControlsToModel()
    {            
        // Basic data
        _com.Model.Topic = textTopic.Text;
        _com.Model.FolderId = _selectedFolderId;            
        _com.Model.FolderDto.FolderId = _selectedFolderId;
        _com.Model.FolderDto.Name = textFolder.Text;
        _com.Model.FolderDto.FolderNumber = int.Parse(textFolderNumber.Text.Substring(1));
        _com.Model.Tags = textTags.Text;
        _com.Model.InternalTags = textStatus.Text;

        if (_com.Model.ContentType == "html")
            _com.Model.Description = _com.Model.ViewToModelDescription(_com.Service?.RepositoryRef, htmlDescription.BodyHtml);
        else if (_com.Model.ContentType == "navigation")
            _com.Model.Description = webView2.TextUrl;
        else
            _com.Model.Description = _com.Model.ViewToModelDescription(_com.Service?.RepositoryRef, textDescription.Text);

        int p;
        if (int.TryParse(textPriority.Text, out p))
            _com.Model.Priority = p;

        _com.Model.Script = textScriptCode.Text;
    }

    private async Task<bool> SaveModel()
    {            
        buttonUndo.Enabled = false;
        return await _com.SaveModel();
    }

    private async void DeleteModel()
    {            
        var res = await _com.DeleteModel();
        if (res)
            _com.Finalize();
    }

    private void UndoChanges()
    {
        var res = MessageBox.Show("Are you sure you want to undo changes?", "KaNote", MessageBoxButtons.YesNo);
        if(res == DialogResult.Yes)
        {
            ModelToControls();
            buttonUndo.Enabled = false;
        }
    }
        
    private async Task<bool> PostItEdit()
    {            
        var res = await SaveModel();
        _com.FinalizeAndPostItEdit();
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

    private void EnableHtmlView ()
    {
        textDescription.Visible = false;
        webView2.Visible = false;
        htmlDescription.Visible = true;            
        buttonEditMarkdown.Enabled = true;
        buttonViewHtml.Enabled = false;
        buttonNavigate.Enabled = true;
        toolDescription.Visible = true;
        toolDescriptionHtml.Visible = true;
        toolDescriptionMarkdown.Visible = false;
    }

    private void EnableMarkdownView()
    {
        htmlDescription.Visible = false;
        webView2.Visible = false;
        textDescription.Visible = true;
        buttonEditMarkdown.Enabled = false;
        buttonViewHtml.Enabled = true;
        buttonNavigate.Enabled = true;
        toolDescription.Visible = true;
        toolDescriptionHtml.Visible = false;
        toolDescriptionMarkdown.Visible = true;
    }

    private void EnableWebView2View()
    {
        textDescription.Visible = false;
        htmlDescription.Visible = false;
        webView2.Visible = true;
        buttonEditMarkdown.Enabled = true;
        buttonViewHtml.Enabled = true;
        buttonNavigate.Enabled = false;
        toolDescription.Visible = false;
        //toolDescriptionHtml.Visible = false;
        //toolDescriptionMarkdown.Visible = false;
    }


    private void EditAlarm()
    {
        if (listViewAlarms.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no alert selected.", "KaNote");
            return;
        }
        var messageId = Guid.Parse(listViewAlarms.SelectedItems[0].Name);
        var message = _com.EditMessage(messageId);
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

    private void EditTask()
    {
        if (listViewTasks.SelectedItems.Count == 0)
        {
            MessageBox.Show("There is no task selected.", "KaNote");
            return;
        }
        var idTask = Guid.Parse(listViewTasks.SelectedItems[0].Name);
        var task = _com.EditTask(idTask);
        if (task != null)
            UpdateTask(task);
    }

    private void UpdateTask(NoteTaskDto task)
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

        textTaskDescription.Text = task.Description;
        textTaskTags.Text = task.Tags;
    }
        
    private async void EditResource()
    {            
        if (_selectedResource == null)
        {
            MessageBox.Show("There is no resource selected.", "KaNote");
            return;
        }            
        var idResource = _selectedResource.ResourceId;
        var resource = await _com.EditResource(idResource);
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
            MessageBox.Show("There is no attribute selected.", "KaNote");
            return;
        }
        var idAttribute = Guid.Parse(listViewAttributes.SelectedItems[0].Name);
        var noteAttribute = _com.Model.KAttributesDto.Where(_ => _.NoteKAttributeId == idAttribute).SingleOrDefault();
        var noteAttributeEdited = _com.EditAttribute(noteAttribute);
        if (noteAttributeEdited != null)
        {
            // Refrescar listView
            UpdateNoteAttribute(noteAttributeEdited);
            buttonUndo.Enabled = true;
        }
    }

    private async Task<ResourceDto> AddResource() 
    {
        var resource = await _com.NewResource();
        if (resource != null)
        {
            AddItemToListViewResources(resource);
        }
        return resource;
    }

    private ResourceDto AddResourceFromClipboard()
    {
        var resource = _com.NewResourceFromClipboard();
        if (resource != null)
        {
            AddItemToListViewResources(resource);
        }
        return resource;
    }

    private void AddItemToListViewResources(ResourceDto resource)
    {
        listViewResources.Items.Add(ResourceDtoToListViewItem(resource));
        listViewResources.Items[0].Selected = true;
        _selectedResource = resource;
    }

    private void InsertLinkSelectedResource()
    {
        var repRef = _com.Service.RepositoryRef;
        var tmpFile = Path.Combine(_selectedResource.Container, _selectedResource.Name);

        var containerWithRootUrl = Path.Combine(repRef.ResourcesContainerCacheRootUrl, repRef.ResourcesContainer);
        if (!string.IsNullOrEmpty(repRef.ResourcesContainerCacheRootUrl))
            tmpFile = tmpFile.Replace(repRef.ResourcesContainer, containerWithRootUrl);
        else
            tmpFile = Path.Combine(repRef.ResourcesContainerCacheRootPath, tmpFile);

        tmpFile = tmpFile.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (!buttonViewHtml.Enabled)
        {
            string strLink = (_selectedResource.FileType.Contains("image")) ?
                $"<img src='{tmpFile}' alt='{_selectedResource.Description}'/>" :
                $"<a href='{tmpFile}' target='_blank'>{_selectedResource.NameOut}</a>";
            htmlDescription.SelectedHtml = strLink;
            htmlDescription.Focus();
        }
        else
        {
            string strLink = (_selectedResource.FileType.Contains("image")) ?
                $"![alt text]({tmpFile} '{_selectedResource.Description}')" : $"[{_selectedResource.NameOut}]({tmpFile} '{_selectedResource.Description}')";
            var selStart = textDescription.SelectionStart;
            textDescription.Text = textDescription.Text.Insert(selStart, strLink);
        }
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

    #endregion

}

