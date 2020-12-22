using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using Markdig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class NoteEditorForm : Form, IEditorView<NoteExtendedDto>
    {
        #region Private fields

        private readonly NoteEditorComponent _com;
        private bool _viewFinalized = false;

        private Guid _selectedFolderId;

        #endregion 

        #region Constructor

        public NoteEditorForm(NoteEditorComponent com)
        {
            InitializeComponent();
            _com = com;
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

        public DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            return MessageBox.Show(info, caption, buttons);
        }

        public Result<EComponentResult> ShowModalView()
        {
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public void CleanView()
        {
            // Basic data
            textTopic.Text = "";
            textNoteNumber.Text = "";
            textFolder.Text = "";
            textFolderNumber.Text = "";
            textTags.Text = "";
            textDescription.Text = "";
            htmlDescription.BodyHtml = "";
            textPriority.Text = "";
            listViewAttributes.Clear();
            listViewResources.Clear();
            listViewTasks.Clear();
            listViewAlarms.Clear();
        }

        public void RefreshView()
        {
            ModelToControls();
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
            //toolDescription.Visible = false;
            _com.EditMode = false;
        }

        public void ConfigureWindowMode()
        {
            TopLevel = true;
            Dock = DockStyle.None;
            FormBorderStyle = FormBorderStyle.Sizable;
            toolBarNoteEditor.Visible = true;
            //toolDescription.Visible = true;
            StartPosition = FormStartPosition.CenterScreen;
            _com.EditMode = true;
        }

        #endregion

        #region Form events handlers

        private async void NoteEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                var savedOk = await SaveModel();
                if (!savedOk)
                    if (MessageBox.Show("Do yo want exit?", "KeyNote", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        _com.Finalize();
                    else
                        e.Cancel = true;
            }
        }

        private void NoteEditorForm_Load(object sender, EventArgs e)
        {
            PersonalizeControls();
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

        private void listViewResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedResourceItemChanged();
        }

        private void listView_Resize(object sender, EventArgs e)
        {
            SizeLastColumn((ListView)sender);
        }

        private void toolDescriptionHtmlTitle_Click(object sender, EventArgs e)
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
                tag = nl + "[![Title](Address 'Title')](http://url 'Title')";
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
                MessageBox.Show("There is no selected alert.", "KeyNote");
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
            var task = await _com.NewTask();
            if (task != null)
                listViewTasks.Items.Add(NoteTaskDtoToListViewItem(task));
        }

        private void buttonTaskEdit_Click(object sender, EventArgs e)
        {
            EditTask();
        }

        private async void buttonTaskDelete_Click(object sender, EventArgs e)
        {
            if (listViewTasks.SelectedItems.Count == 0)
            {
                MessageBox.Show("There is no task selected .", "KeyNote");
                return;
            }
            //var delTsk = GetNoteTaskFromSelectedListView();
            var delTsk = listViewTasks.SelectedItems[0].Name;
            var res = await _com.DeleteTask(Guid.Parse(delTsk));
            if (res)
            {
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
            var resource = await _com.NewResource();
            if (resource != null)
                listViewResources.Items.Add(ResourceDtoToListViewItem(resource));
        }
       
        private void buttonResourceEdit_Click(object sender, EventArgs e)
        {
            EditResource();
        }

        private async void buttonResourceDelete_Click(object sender, EventArgs e)
        {
            if (listViewResources.SelectedItems.Count == 0)
            {
                MessageBox.Show("There is no task selected .", "KeyNote");
                return;
            }
            //var delTsk = GetNoteTaskFromSelectedListView();
            var delRes = listViewResources.SelectedItems[0].Name;
            var res = await _com.DeleteResource(Guid.Parse(delRes));
            if (res)
            {
                listViewResources.Items[delRes].Remove();
            }
        }


        private void listViewResources_DoubleClick(object sender, EventArgs e)
        {
            if (_com.EditMode)
                EditResource();
        }

        #endregion 


        #endregion

        #region Private methods

        private void PersonalizeControls()
        {
            this.panelDescription.Location = new System.Drawing.Point(6, 130);
            if (_com.EditMode)
                this.panelDescription.Size = new System.Drawing.Size(780, 432);
            else
                this.panelDescription.Size = new System.Drawing.Size(780, 458);

            this.textDescription.Dock = DockStyle.Fill;
            this.htmlDescription.Dock = DockStyle.Fill;

            if (_com.Model?.ContentType == "html")
                EnableHtmlView();
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
            }

            PersonalizeListView(listViewAttributes);
            PersonalizeListView(listViewResources);
            PersonalizeListView(listViewTasks);
            PersonalizeListView(listViewAlarms);

            // TODO: remove in this version
            tabNoteData.TabPages.Remove(tabTraceNotes);
        }

        private void ModelToControls()
        {
            this.Cursor = Cursors.WaitCursor;

            // Basic data
            textTopic.Text = _com.Model.Topic;                
            textNoteNumber.Text = "#" + _com.Model.NoteNumber.ToString();
            textFolder.Text = _com.Model.FolderDto?.Name;
            textFolderNumber.Text = "#" + _com.Model.FolderDto.FolderNumber.ToString();
            _selectedFolderId = _com.Model.FolderId;
            textTags.Text = _com.Model.Tags;            
            textPriority.Text = _com.Model.Priority.ToString();

            if (_com.Model.HtmlFormat)
            {
                labelLoadingHtml.Visible = true;
                labelLoadingHtml.Refresh();
                textDescription.Visible = false;
                htmlDescription.Visible = true;
                htmlDescription.BodyHtml = "";                
                htmlDescription.BodyHtml = _com.Model.Description;               
                //                
                htmlDescription.Refresh();
                labelLoadingHtml.Visible = false;
            }
            else
            {
                htmlDescription.Visible = false;
                textDescription.Text = _com.Model.Description;
                textDescription.Visible = true;                
            }

            // KAttributes           
            textNoteType.Text = _com.Model.NoteTypeDto.Name;            
            ModelToControlsAttributes();

            // Resources 
            ModelToControlsResources();
            if (_com.Model.Resources.Count > 0)
                UpdatePicResource(_com.Model.Resources[0].ContentArrayBytes, _com.Model.Resources[0].FileType);
            else
                UpdatePicResource(null, null);

            // Tasks
            ModelToControlsTasks();

            // Alarms     
            ModelToControlsAlarms();

            // Script             
            textScriptCode.Text = _com.Model.Script;

            // ........

            // Trace notes
            //From = new List<TraceNote>(),
            //To = new List<TraceNote>()

            this.Cursor = Cursors.Default;
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
            listViewAttributes.Columns.Add("Name", 400, HorizontalAlignment.Left);
            listViewAttributes.Columns.Add("Value", -2, HorizontalAlignment.Left);
        }

        private void ModelToControlsResources()
        {
            listViewResources.Clear();

            foreach (var res in _com.Model.Resources)
            {
                listViewResources.Items.Add(ResourceDtoToListViewItem(res));
            }

            listViewResources.Columns.Add("Name", 200, HorizontalAlignment.Left);
            listViewResources.Columns.Add("File type", 100, HorizontalAlignment.Left);
            listViewResources.Columns.Add("Order", 100, HorizontalAlignment.Left);
            listViewResources.Columns.Add("Description", -2, HorizontalAlignment.Left);
        }

        private void ModelToControlsTasks()
        {
            listViewTasks.Clear();

            foreach (var task in _com.Model.Tasks)
            {
                listViewTasks.Items.Add(NoteTaskDtoToListViewItem(task));
            }

            // Width of -2 indicates auto-size.
            listViewTasks.Columns.Add("User", 150, HorizontalAlignment.Left);

            listViewTasks.Columns.Add("Tags", 100, HorizontalAlignment.Left);

            listViewTasks.Columns.Add("Priority", 50, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Resolved", 50, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Est. time", 50, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Spend time", 50, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Dif.", 50, HorizontalAlignment.Left);

            listViewTasks.Columns.Add("Ex start", 120, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Ex end", 120, HorizontalAlignment.Left);

            listViewTasks.Columns.Add("Start", 120, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("End", 120, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Description", -2, HorizontalAlignment.Left);
        }

        private void ModelToControlsAlarms()
        {
            listViewAlarms.Clear();

            foreach (var msg in _com.Model.Messages)
            {
                listViewAlarms.Items.Add(MessageDtoToListViewItem(msg));
            }

            // Width of -2 indicates auto-size.            
            listViewAlarms.Columns.Add("User", 130, HorizontalAlignment.Left);
            listViewAlarms.Columns.Add("Activated", 80, HorizontalAlignment.Left);
            listViewAlarms.Columns.Add("Date time", 130, HorizontalAlignment.Left);
            listViewAlarms.Columns.Add("Alarm periodicity", 120, HorizontalAlignment.Left);
            listViewAlarms.Columns.Add("Notification type", 120, HorizontalAlignment.Left);
            listViewAlarms.Columns.Add("Comment", -2, HorizontalAlignment.Left);            
        }

        private void UpdatePicResource(byte[] content, string type)
        {
            if (content == null || !type.Contains("image"))
            {
                picResource.Image = null;
                return;
            }

            picResource.Image = Image.FromStream(new MemoryStream(content));
        }

        private void ControlsToModel()
        {
            // TODO: !!! ojo ... completar implementación 

            // Basic data
            _com.Model.Topic = textTopic.Text;
            _com.Model.FolderId = _selectedFolderId;
            _com.Model.FolderDto.FolderId = _selectedFolderId;
            _com.Model.FolderDto.Name = textFolder.Text;
            _com.Model.FolderDto.FolderNumber = int.Parse(textFolderNumber.Text.Substring(1));
            _com.Model.Tags = textTags.Text;

            if (_com.Model.ContentType == "html")
                _com.Model.Description = htmlDescription.BodyHtml;
            else
                _com.Model.Description = textDescription.Text;

            int p;
            if (int.TryParse(textPriority.Text, out p))
                _com.Model.Priority = p;
        }

        private async Task<bool> SaveModel()
        {
            ControlsToModel();            
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
            var res = MessageBox.Show("Are you sure you want to undo changes?", "KeyNote", MessageBoxButtons.YesNo);
            if(res == DialogResult.Yes)
            {
                ModelToControls();
                buttonUndo.Enabled = false;
            }
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
            if (c is Button)
            {
                Button b = (Button)c;
                b.Enabled = false;
                return;
            }
            if (c is CheckBox)
            {
                CheckBox cb = (CheckBox)c;
                cb.Enabled = false;
                return;
            }
            if (c is ComboBox)
            {
                ComboBox comB = (ComboBox)c;
                comB.Enabled = false;
                return;
            }
            if (c is ToolStrip)
            {
                ToolStrip tb = (ToolStrip)c;
                tb.Visible = false;
                return;
            }
            if (c is Panel)
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
       
        private void OnSelectedResourceItemChanged()
        {
            try
            {
                if (listViewResources.SelectedItems.Count > 0)
                {
                    Cursor = Cursors.WaitCursor;                    
                    var idResource = (Guid.Parse(listViewResources.SelectedItems[0].Name));
                    var content = _com.Model.Resources.Where(_ => _.ResourceId == idResource).Select(_ => _.ContentArrayBytes).FirstOrDefault();
                    var type = _com.Model.Resources.Where(_ => _.ResourceId == idResource).Select(_ => _.FileType).FirstOrDefault();
                    UpdatePicResource(content, type);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"OnSelectedResourceItemChanged error: {ex.Message}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private ListViewItem MessageDtoToListViewItem(KMessageDto message)
        {
            var itemList = new ListViewItem(message.UserFullName);
            itemList.Name = message.KMessageId.ToString();
            itemList.SubItems.Add(message.AlarmActivated.ToString());
            itemList.SubItems.Add(message.AlarmDateTime.ToString());
            itemList.SubItems.Add(message.AlarmType.ToString());
            itemList.SubItems.Add(message.NotificationType.ToString());
            itemList.SubItems.Add(message.Content.ToString());
            return itemList;
        }

        private ListViewItem NoteTaskDtoToListViewItem(NoteTaskDto task)
        {
            var itemList = new ListViewItem(task.UserFullName);
            itemList.Name = task.NoteTaskId.ToString();                        
            itemList.SubItems.Add(task.Tags?.ToString());
            itemList.SubItems.Add(task.Priority.ToString());
            itemList.SubItems.Add(task.Resolved.ToString());
            itemList.SubItems.Add(task.EstimatedTime.ToString());
            itemList.SubItems.Add(task.SpentTime.ToString());
            itemList.SubItems.Add(task.DifficultyLevel.ToString());
            itemList.SubItems.Add(task.ExpectedStartDate.ToString());
            itemList.SubItems.Add(task.ExpectedEndDate.ToString());
            itemList.SubItems.Add(task.StartDate.ToString());
            itemList.SubItems.Add(task.EndDate.ToString());
            itemList.SubItems.Add(task.Description.ToString());
            return itemList;
        }

        private ListViewItem ResourceDtoToListViewItem(ResourceDto resource)
        {
            var itemList = new ListViewItem(resource.NameOut);
            itemList.Name = resource.ResourceId.ToString();
            itemList.SubItems.Add(resource.FileType);
            itemList.SubItems.Add(resource.Order.ToString());
            itemList.SubItems.Add(resource.Description); 
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
            htmlDescription.Visible = true;
            buttonEditMarkdown.Enabled = true;
            buttonViewHtml.Enabled = false;
            toolDescriptionHtmlTitles.Visible = true;
            toolDescriptionMarkdown.Visible = false;
        }

        private void EnableMarkdownView()
        {
            htmlDescription.Visible = false;
            textDescription.Visible = true;
            buttonEditMarkdown.Enabled = false;
            buttonViewHtml.Enabled = true;
            toolDescriptionHtmlTitles.Visible = false;
            toolDescriptionMarkdown.Visible = true;
        }

        private void EditAlarm()
        {
            if (listViewAlarms.SelectedItems.Count == 0)
            {
                MessageBox.Show("There is no alert selected.", "KeyNote");
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
            item.SubItems[4].Text = message.NotificationType.ToString();
            item.SubItems[5].Text = message.Content.ToString();

        }

        private void EditTask()
        {
            if (listViewTasks.SelectedItems.Count == 0)
            {
                MessageBox.Show("There is no task selected.", "KeyNote");
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
            item.SubItems[1].Text = task.Tags;
            item.SubItems[2].Text = task.Priority.ToString();
            item.SubItems[3].Text = task.Resolved.ToString();
            item.SubItems[4].Text = task.EstimatedTime.ToString();
            item.SubItems[5].Text = task.SpentTime.ToString();
            item.SubItems[6].Text = task.DifficultyLevel.ToString();
            item.SubItems[7].Text = task.ExpectedStartDate.ToString();
            item.SubItems[8].Text = task.ExpectedEndDate.ToString();
            item.SubItems[9].Text = task.StartDate.ToString();
            item.SubItems[10].Text = task.EndDate.ToString();
            item.SubItems[11].Text = task.Description;            
        }

        #endregion

        private void EditResource()
        {
            if (listViewResources.SelectedItems.Count == 0)
            {
                MessageBox.Show("There is no resource selected.", "KeyNote");
                return;
            }
            var idResource = Guid.Parse(listViewResources.SelectedItems[0].Name);
            var resource = _com.EditResource(idResource);
            if (resource != null)
                UpdateResource(resource);
        }

        private void UpdateResource(ResourceDto resource)
        {
            var item = listViewResources.Items[resource.ResourceId.ToString()];
            item.SubItems[1].Text = resource.FileType;
            item.SubItems[2].Text = resource.Order.ToString();
            item.SubItems[3].Text = resource.Description;
        }


        //private void toolInsertarImagenClipboard_Click(object sender, EventArgs e)
        //{
        //    Bitmap bm;
        //    string destino = ObtenerNombreFicheroImagenDestino();

        //    if (string.IsNullOrEmpty(destino))
        //        return;

        //    destino += ".png";

        //    try
        //    {
        //        if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Bitmap))
        //        {
        //            bm = (Bitmap)Clipboard.GetDataObject().GetData(DataFormats.Bitmap);
        //            bm.Save(destino, ImageFormat.Png);
        //            this.htmlEditor.InsertImage(destino);
        //        }
        //        else
        //            MessageBox.Show("No dispone de imágenes en el Portapapeles para insertar en esta nota.", "ANotas");

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("No se ha podido copiar el fichero seleccionado en el contenedor de imágenes" +
        //            " de este archivador (" + ex.Message + ")");
        //    }

        //    this.htmlEditor.Focus();
        //}

        //private void toolInsertarImagen_Click(object sender, EventArgs e)
        //{
        //    string origen = string.Empty;
        //    string destino = ObtenerNombreFicheroImagenDestino();

        //    if (string.IsNullOrEmpty(destino))
        //        return;

        //    using (OpenFileDialog dialog = new OpenFileDialog())
        //    {
        //        dialog.Title = "Seleccionar Imagen";
        //        dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";
        //        dialog.FilterIndex = 1;
        //        dialog.RestoreDirectory = true;
        //        dialog.CheckFileExists = true;
        //        // dialog.InitialDirectory = directorioTrabajo;
        //        if (dialog.ShowDialog() == DialogResult.OK)
        //        {
        //            origen = Path.GetFullPath(dialog.FileName);
        //            destino += Path.GetExtension(origen);

        //            if (origen != "")
        //            {
        //                try
        //                {
        //                    // Copiar la imágen en el almacén de imágenes
        //                    File.Copy(origen, destino);
        //                    this.htmlEditor.InsertImage(destino);
        //                }
        //                catch (Exception ex)
        //                {
        //                    MessageBox.Show("No se ha podido copiar el fichero seleccionado en el contenedor de imágenes" +
        //                        " de este archivador (" + ex.Message + ")");
        //                }
        //            }
        //        }
        //    }
        //    this.htmlEditor.Focus();
        //}

        //private string ObtenerNombreFicheroImagenDestino()
        //{
        //    string destino = string.Empty;

        //    // Debe estar definido el directorio raíz de los recursos del
        //    // archivador
        //    if (string.IsNullOrEmpty(NotaEdicion.CarpetaNota.Archivador.PathRecursos))
        //    {
        //        MessageBox.Show("No se ha definido la ruta de recursos para el repositorio de ficheros de este archivador." +
        //            "En las propiedades del archivador debe definir esa ruta raíz.", "ANotas");
        //        return "";
        //    }

        //    destino = NotaEdicion.CarpetaNota.Archivador.PathRecursosImgs;

        //    DirectoryInfo d = new DirectoryInfo(destino);
        //    if (!d.Exists)
        //        d.Create();

        //    destino += Guid.NewGuid().ToString();

        //    return destino;
        //}

    }
}
