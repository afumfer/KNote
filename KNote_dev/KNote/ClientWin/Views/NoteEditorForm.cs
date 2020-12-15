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
            _com.Model.Note.ContentType = "markdown";

            EnableMarkdownView();
        }

        private void buttonViewHtml_Click(object sender, EventArgs e)
        {
            var MarkdownContent = textDescription.Text;
            var pipeline = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var HtmlContent = Markdig.Markdown.ToHtml(MarkdownContent, pipeline);

            htmlDescription.BodyHtml = HtmlContent;
            _com.Model.Note.ContentType = "html";

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

        #endregion

        #region Private methods

        private void ModelToControls()
        {
            this.Cursor = Cursors.WaitCursor;

            // Basic data
            textTopic.Text = _com.Model.Note.Topic;                
            textNoteNumber.Text = "#" + _com.Model.Note.NoteNumber.ToString();
            textFolder.Text = _com.Model.Note.FolderDto?.Name;
            textFolderNumber.Text = "#" + _com.Model.Note.FolderDto.FolderNumber.ToString();
            _selectedFolderId = _com.Model.Note.FolderId;
            textTags.Text = _com.Model.Note.Tags;            
            textPriority.Text = _com.Model.Note.Priority.ToString();

            if (_com.Model.Note.HtmlFormat)
            {
                labelLoadingHtml.Visible = true;
                labelLoadingHtml.Refresh();
                textDescription.Visible = false;
                htmlDescription.Visible = true;
                htmlDescription.BodyHtml = "";                
                htmlDescription.BodyHtml = _com.Model.Note.Description;               
                //                
                htmlDescription.Refresh();
                labelLoadingHtml.Visible = false;
            }
            else
            {
                htmlDescription.Visible = false;
                textDescription.Text = _com.Model.Note.Description;
                textDescription.Visible = true;                
            }

            // KAttributes           
            textNoteType.Text = _com.Model.Note.NoteTypeDto.Name;            
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
            textScriptCode.Text = _com.Model.Note.Script;

            // ........

            // Trace notes
            //From = new List<TraceNote>(),
            //To = new List<TraceNote>()

            this.Cursor = Cursors.Default;
        }

        private void ControlsToModel()
        {
            // TODO: !!! ojo ... completar implementación 

            // Basic data
            _com.Model.Note.Topic = textTopic.Text;
            _com.Model.Note.FolderId = _selectedFolderId;
            _com.Model.Note.FolderDto.FolderId = _selectedFolderId;
            _com.Model.Note.FolderDto.Name = textFolder.Text;
            _com.Model.Note.FolderDto.FolderNumber = int.Parse(textFolderNumber.Text.Substring(1));
            _com.Model.Note.Tags = textTags.Text;

            if (_com.Model.Note.ContentType == "html")
                _com.Model.Note.Description = htmlDescription.BodyHtml;
            else
                _com.Model.Note.Description = textDescription.Text;
            
            int p;
            if (int.TryParse(textPriority.Text, out p))
                _com.Model.Note.Priority = p;
        }

        private void PersonalizeControls()
        {
            this.panelDescription.Location = new System.Drawing.Point(6, 130);
            if (_com.EditMode)
                this.panelDescription.Size = new System.Drawing.Size(780, 432);
            else
                this.panelDescription.Size = new System.Drawing.Size(780, 458);

            this.textDescription.Dock = DockStyle.Fill;
            this.htmlDescription.Dock = DockStyle.Fill;

            if(_com.Model?.Note?.ContentType == "html")            
                EnableHtmlView();            
            else            
                EnableMarkdownView();
            
            if (!_com.EditMode)            
            {
                foreach(var tab in tabNoteData.TabPages)
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

        private void ModelToControlsAttributes()
        {
            listViewAttributes.Clear();

            foreach(var atr in _com.Model.Note.KAttributesDto)
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
                var itemList = new ListViewItem(res.NameOut);
                itemList.Name = res.ResourceId.ToString();                
                itemList.SubItems.Add(res.FileType);
                itemList.SubItems.Add(res.Order.ToString());
                itemList.SubItems.Add(res.Description);
                listViewResources.Items.Add(itemList);
            }

            // Width of -2 indicates auto-size.
            listViewResources.Columns.Add("Name", 200, HorizontalAlignment.Left);
            listViewResources.Columns.Add("File type", 100, HorizontalAlignment.Left);
            listViewResources.Columns.Add("Order", 100, HorizontalAlignment.Left);
            listViewResources.Columns.Add("Description", -2, HorizontalAlignment.Left);
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

        private void ModelToControlsTasks()
        {
            listViewTasks.Clear();

            foreach (var task in _com.Model.Tasks)
            {
                var itemList = new ListViewItem(task.UserFullName.ToString());
                itemList.Name = task.NoteTaskId.ToString();
                //itemList.BackColor = Color.LightGray;                
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
                listViewTasks.Items.Add(itemList);
            }

            // Width of -2 indicates auto-size.
            listViewTasks.Columns.Add("User", 150, HorizontalAlignment.Left);           

            listViewTasks.Columns.Add("Tags", 100, HorizontalAlignment.Left);

            listViewTasks.Columns.Add("Priority", 50, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Resolved", 50, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Est. time", 50, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Spend time", 50, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Dif.", 50, HorizontalAlignment.Left);
            
            listViewTasks.Columns.Add("Ex start", 50, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Ex end", 50, HorizontalAlignment.Left);

            listViewTasks.Columns.Add("Start", 50, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("End", 50, HorizontalAlignment.Left);
            listViewTasks.Columns.Add("Description", -2, HorizontalAlignment.Left);
        }

        private void ModelToControlsAlarms()
        {
            listViewAlarms.Clear();

            foreach (var msg in _com.Model.Messages)
            {
                var itemList = new ListViewItem(msg.UserFullName);
                itemList.Name = msg.KMessageId.ToString();
                //itemList.BackColor = Color.LightGray;
                itemList.SubItems.Add(msg.AlarmDateTime.ToString());
                itemList.SubItems.Add(msg.AlarmActivated.ToString());
                listViewAlarms.Items.Add(itemList);
            }

            // Width of -2 indicates auto-size.
            listViewAlarms.Columns.Add("User", 200, HorizontalAlignment.Left);
            listViewAlarms.Columns.Add("Date time", 200 , HorizontalAlignment.Left);
            listViewAlarms.Columns.Add("Activated", 200, HorizontalAlignment.Left);
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

        #endregion

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

        private void buttonAddAlarmForMe_Click(object sender, EventArgs e)
        {
            _com.NewMessage();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

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
