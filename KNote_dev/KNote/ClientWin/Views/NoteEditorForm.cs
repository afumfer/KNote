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
    public partial class NoteEditorForm : Form, IEditorView<NoteDto>
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

        public void CleanView()
        {
            // Labels top section
            labelInfoIdNote.Text = "";
            labelInfoIdFolder.Text = "";
            textDesTopic.Text = "" ;
            textDesFolder.Text = "";

            // Basic data
            textTopic.Text = "";
            textFolder.Text = "";
            textTags.Text = "";
            textDescription.Text = "";
            textPriority.Text = "";
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
            panelTop.Visible = false;
        }

        public void ConfigureWindowMode()
        {
            TopLevel = true;
            Dock = DockStyle.None;
            FormBorderStyle = FormBorderStyle.Sizable;
            toolBarNoteEditor.Visible = true;
            panelTop.Visible = true;
            StartPosition = FormStartPosition.CenterScreen;
        }


        public void RefreshView()
        {
            RefreshBindingModel();
        }

        public DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            return  MessageBox.Show(info, caption, buttons);
        }

        public void ShowView()
        {
            this.Show();
        }

        public Result<EComponentResult> ShowModalView()
        {
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }
        
        public void RefreshBindingModel()
        {
            ModelToControls();
        }

        #endregion

        #region Form events handlers

        private void NoteEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                SaveModel();
                _com.Finalize();
            }
        }

        private void NoteEditorForm_Load(object sender, EventArgs e)
        {
            PersonalizeControls();
        }

        private void dataGridResources_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectedResourceItemChanged();
        }

        private void buttonToolBar_Click(object sender, EventArgs e)
        {
            ToolStripItem menuSel;
            menuSel = (ToolStripItem)sender;

            if (menuSel == buttonSave)
            {
                SaveModel();
            }
            else if (menuSel == buttonDelete)
            {
                DeleteModel();
            }

            //
        }


        #endregion

        #region Private methods

        private void ModelToControls()
        {
            // Label info
            labelInfoIdNote.Text = _com.NoteEdit.NoteNumber.ToString();
            labelInfoIdFolder.Text = _com.NoteEdit.FolderDto.FolderNumber.ToString();
            textDesTopic.Text = _com.NoteEdit.Topic;
            textDesFolder.Text = _com.NoteEdit.FolderDto.Name;

            // Basic data
            textTopic.Text = _com.NoteEdit.Topic;
            textFolder.Text = _com.NoteEdit.FolderDto?.Name;
            textTags.Text = _com.NoteEdit.Tags;
            textDescription.Text = _com.NoteEdit.Description;
            textPriority.Text = _com.NoteEdit.Priority.ToString();

            // KAttributes           
            textNoteType.Text = _com.NoteEdit.NoteTypeDto.Name;
            dataGridAttributes.DataSource = _com.NoteEdit.KAttributesDto.OrderBy(_ => _.Order).Select(_ => new { _.Name, _.Value }).ToList();

            // Resources 
            dataGridResources.DataSource = _com.NoteEditResources.OrderBy(_ => _.Order).Select(_ =>
              new { Id = _.ResourceId, Name = _.NameOut, Description = _.Description, Order = _.Order, Tupe = _.FileType }).ToList();

            if (_com.NoteEditResources.Count > 0)
                UpdatePicResource(_com.NoteEditResources[0].ContentArrayBytes, _com.NoteEditResources[0].FileType);
            else
                UpdatePicResource(null, null);

            // Tasks
            dataGridTasks.DataSource = _com.NoteEditTasks.Select(_ => new {
                User = _.UserFullName,
                Description = _.Description,
                Tags = _.Tags,
                Priority = _.Priority,
                Resolved = _.Resolved,
                EstimatedTime = _.EstimatedTime,
                SependTime = _.SpentTime,
                DifficultyLeve = _.DifficultyLevel,
                ExpectedStarDate = _.ExpectedStartDate,
                ExpectedEndDate = _.ExpectedEndDate,
                StartDate = _.StartDate,
                EndDate = _.EndDate
            }).ToList();

            // Alarms            
            dataGridAlarms.DataSource = _com.NoteEditMessages.Select(_ => new {
                User = _.UserFullName,
                AlarmDateTime = _.AlarmDateTime,
                Content = _.Content,
                Activated = _.AlarmActivated
            }).ToList();

            // Script             
            textScriptCode.Text = _com.NoteEdit.Script;

            // ........

            // Trace notes
            //From = new List<TraceNote>(),
            //To = new List<TraceNote>()

        }

        private void ControlsToModel()
        {
            // Basic data
            _com.NoteEdit.Topic = textTopic.Text;
            //_com.NoteEdit.FolderDto.Name = textFolder.Text;
            _com.NoteEdit.Tags = textTags.Text;
            _com.NoteEdit.Description = textDescription.Text;
            int p;
            if (int.TryParse(textPriority.Text, out p))
                _com.NoteEdit.Priority = p;
        }

        private void PersonalizeControls()
        {
            dataGridAttributes.Columns[0].Width = 400;  // Attribute name
            dataGridAttributes.Columns[1].Width = 200;  // Value

            dataGridResources.Columns[0].Visible = false;  // Id
        }

        private void OnSelectedResourceItemChanged()
        {
            try
            {
                if (dataGridResources.SelectedRows.Count > 0)
                {
                    Cursor = Cursors.WaitCursor;
                    var sr = dataGridResources.SelectedRows[0];                    
                    var idResource = (Guid)sr.Cells[0].Value;
                    var content = _com.NoteEditResources.Where(_ => _.ResourceId == idResource).Select(_ => _.ContentArrayBytes).FirstOrDefault();
                    var type = _com.NoteEditResources.Where(_ => _.ResourceId == idResource).Select(_ => _.FileType).FirstOrDefault();
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

        private void UpdatePicResource(byte[] content, string type)
        {
            if (content == null || !type.Contains("image"))
            {
                picResource.Image = null;
                return;
            }

            picResource.Image = Image.FromStream(new MemoryStream(content));
        }

        private void SaveModel()
        {
            ControlsToModel();
            _com.SaveNote();
        }

        private async void DeleteModel()
        {            
            var res = await _com.DeleteNote();
            if (res)
                _com.Finalize();
        }

        #endregion

    }
}
