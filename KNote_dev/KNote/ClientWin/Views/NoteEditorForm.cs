using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

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

        #endregion

        #region Private methods

        private void ModelToControls()
        {
            // Basic data
            textTopic.Text = _com.Model.Note.Topic;
            textNoteNumber.Text = "#" + _com.Model.Note.NoteNumber.ToString();
            textFolder.Text = _com.Model.Note.FolderDto?.Name;
            textFolderNumber.Text = "#" + _com.Model.Note.FolderDto.FolderNumber.ToString();
            textTags.Text = _com.Model.Note.Tags;
            textDescription.Text = _com.Model.Note.Description;
            textPriority.Text = _com.Model.Note.Priority.ToString();

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

        }

        private void ControlsToModel()
        {
            // TODO: !!! ojo ... completar implementación 

            // Basic data
            _com.Model.Note.Topic = textTopic.Text;
            //_com.NoteEdit.FolderDto.Name = textFolder.Text;
            _com.Model.Note.Tags = textTags.Text;
            _com.Model.Note.Description = textDescription.Text;
            int p;
            if (int.TryParse(textPriority.Text, out p))
                _com.Model.Note.Priority = p;
        }

        private void PersonalizeControls()
        {
            

            if (_com.EditMode)
            {
                textTopic.ReadOnly = false;                
                textDescription.ReadOnly = false;
                textDescription.BackColor = Color.White;
            }
            else
            {
                foreach (Control conTmp in tabBasicData.Controls)
                {
                    BlockControl(conTmp);
                }
                foreach (Control conTmp in tabAttributes.Controls)
                {
                    BlockControl(conTmp);
                }
                foreach (Control conTmp in tabResources.Controls)
                {
                    BlockControl(conTmp);
                }
                foreach (Control conTmp in tabTasks.Controls)
                {
                    BlockControl(conTmp);
                }
                foreach (Control conTmp in tabAlarms.Controls)
                {
                    BlockControl(conTmp);
                }
                foreach (Control conTmp in tabCode.Controls)
                {
                    BlockControl(conTmp);
                }
                foreach (Control conTmp in tabTraceNotes.Controls)
                {
                    BlockControl(conTmp);
                }
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
            }
            else if (c is Button)
            {
                Button b = (Button)c;
                b.Enabled = false;
            }
            else if (c is CheckBox)
            {
                CheckBox cb = (CheckBox)c;
                cb.Enabled = false;
            }
            else if (c is ComboBox)
            {
                ComboBox comB = (ComboBox)c;
                comB.Enabled = false;
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

        #endregion

        private void listViewResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedResourceItemChanged();
        }

        private void OnSelectedResourceItemChanged()
        {
            try
            {
                if (listViewResources.SelectedItems.Count > 0)
                {
                    Cursor = Cursors.WaitCursor;
                    //var sr = dataGridResources.SelectedRows[0];
                    var idResource = (Guid.Parse(listViewResources.SelectedItems[0].Name));    // (Guid)sr.Cells[0].Value;
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
                var itemList = new ListViewItem(task.UserFullName);
                itemList.Name = task.NoteTaskId.ToString();
                //itemList.BackColor = Color.LightGray;
                itemList.SubItems.Add(task.Description);
                itemList.SubItems.Add(task.Tags);
                itemList.SubItems.Add(task.Priority.ToString());
                itemList.SubItems.Add(task.Resolved.ToString());
                itemList.SubItems.Add(task.EstimatedTime.ToString());
                itemList.SubItems.Add(task.SpentTime.ToString());
                itemList.SubItems.Add(task.DifficultyLevel.ToString());
                itemList.SubItems.Add(task.ExpectedStartDate.ToString());
                itemList.SubItems.Add(task.ExpectedEndDate.ToString());
                itemList.SubItems.Add(task.StartDate.ToString());
                itemList.SubItems.Add(task.EndDate.ToString());
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
            listViewTasks.Columns.Add("Es end", 50, HorizontalAlignment.Left);
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
            lv.Columns[lv.Columns.Count - 1].Width = -2;
        }

        private void listView_Resize(object sender, EventArgs e)
        {
            SizeLastColumn((ListView)sender);
        }

    }
}
