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

        public void RefreshBindingModel()
        {
            //// Label info
            //labelInfoIdNote.Text = _com.Entity.NoteNumber.ToString();
            ////labelInfoIdFolder.Text = _ctrl.Entity.Folder?.FolderNumber.ToString();
            //textDesTopic.Text = _com.Entity.Topic;
            ////textDesFolder.Text = _ctrl.Entity.Folder?.Name;

            // Basic data
            textTopic.Text = _com.NoteEdit.Topic;
            //textFolder.Text = _ctrl.Entity.Folder?.Name;
            textTags.Text = _com.NoteEdit.Tags;
            textDescription.Text = _com.NoteEdit.Description;
            textPriority.Text = _com.NoteEdit.Priority.ToString();

            // Alarms            
            //dataGridAlarms.DataSource = _ctrl.Entity.KMessages;            

            // KAttributes                                    
            //NoteAttributes = new List<NoteAttribute>(),

            // Tasks
            // NoteTasks = new List<NoteTask>(),

            // Resources 
            //Resources = new List<Resource>(),

            // Trace notes
            //From = new List<TraceNote>(),
            //To = new List<TraceNote>()

            // Code 
            //[ContentBehind]
            //MessagesForScript = new List<KMessage>(),
            //EventsForScript = new List<KEvent>(),

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

        private void NoteEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
                _com.Finalize();
        }
    }
}
