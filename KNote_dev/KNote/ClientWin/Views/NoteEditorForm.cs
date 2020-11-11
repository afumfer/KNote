using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class NoteEditorForm : Form, IEditorView<NoteDto>
    {
        private readonly NoteEditorComponent _com;
        private bool _viewFinalized = false;

        public NoteEditorForm(NoteEditorComponent com)
        {
            InitializeComponent();

            _com = com;
        }

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

        public void ShowInfo(string info)
        {
            
        }

        public void ShowView()
        {
            this.Show();
        }

        Result<EComponentResult> IViewBase.ShowModalView()
        {
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public void RefreshBindingModel()
        {
            ModelToControls();
        }

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
            dataGridAttributes.DataSource = _com.NoteEdit.KAttributesDto.OrderBy(_ => _.Order).Select( _ => new { _.Name, _.Value }).ToList();

            // Resources 
            //dataGridResources = _com.Xxxx -> Implementar un LazyLoad() de los recursos.    !!!!

            // Tasks
            // dataGridTasks = _com.Xxxx->Implementar un LazyLoad() de las tareas. 

            // ........

            // Alarms            
            //dataGridAlarms.DataSource = _ctrl.Entity.KMessages;            

            // Trace notes
            //From = new List<TraceNote>(),
            //To = new List<TraceNote>()

            // Script 
            //[ContentBehind]
            //MessagesForScript = new List<KMessage>(),
            //EventsForScript = new List<KEvent>(),

        }

        private void PersonalizeControls()
        {            
            dataGridAttributes.Columns[0].Width = 400;  // Attribute name
            dataGridAttributes.Columns[1].Width = 200;  // Value
        }


        private void NoteEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
                _com.Finalize();
        }

        private void NoteEditorForm_Load(object sender, EventArgs e)
        {
            PersonalizeControls();
        }
    }
}
