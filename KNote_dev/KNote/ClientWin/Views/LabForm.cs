using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class LabForm : Form
    {
        private Store _store;

        private FolderSelectorComponent _folderSelector;
        private NotesSelectorComponent _notesSelector;

        private FolderWithServiceRef temp;

        public LabForm()
        {
            InitializeComponent();
        }

        public LabForm(Store store) : this()
        {
            _store = store;
            _folderSelector = new FolderSelectorComponent(_store);
            _notesSelector = new NotesSelectorComponent(_store);

            _folderSelector.EntitySelection += _folderSelector_EntitySelection;
            _notesSelector.EntitySelection += _notesSelector_EntitySelection;
        }

        private void _notesSelector_EntitySelection(object sender, ComponentEventArgs<Model.Dto.NoteInfoDto> e)
        {
            if (e.Entity == null)
            {
                labelInfo1.Text = "";
                return;
            }

            labelInfo2.Text = $" {e.Entity.Topic} - {e.Entity.NoteId}";
        }

        private void _folderSelector_EntitySelection(object sender, ComponentEventArgs<FolderWithServiceRef> e)
        {
            if (e.Entity == null)
            {
                labelInfo1.Text = "";
                return;
            }

            labelInfo1.Text = $" {e.Entity.ServiceRef.Alias} - {e.Entity.FolderInfo?.Name}";
            if (_notesSelector != null)
                _notesSelector.GetListNotesAsync(e.Entity);
        }


        //private async Task LoadNotes()  // opción 2
        private async void LoadNotes()   // opción 3
        {
            var service = _store.PersonalServiceRef.Service;
            var notes = (await service.Notes.HomeNotesAsync()).Entity;
            foreach (var note in notes)
                listMessages.Items.Add(note.Topic);
        }

        //private async void buttonTest1_Click(object sender, EventArgs e)  // opción 1 / 2
        private void buttonTest1_Click(object sender, EventArgs e)
        {
            // opción 1
            //var service = _store.PersonalServiceRef.Service;

            //var notes = (await service.Notes.HomeNotesAsync()).Entity;
            //foreach (var note in notes)
            //    listTest.Items.Add(note.Topic);

            // or 

            //await LoadNotes();  // opción 2

            var monitor = new MonitorComponent(_store);
            monitor.Run();

            //LoadNotes();   // opción 3
        }

        private void buttonTest2_Click(object sender, EventArgs e)
        {            
            _folderSelector.Run();            
        }

        private void buttonTest3_Click(object sender, EventArgs e)
        {
            
            _notesSelector.Run();
        }

        private void buttonTest4_Click(object sender, EventArgs e)
        {
            //KNoteManagmentForm f = new KNoteManagmentForm(null);
            //f.Show();

            //NoteEditorForm n = new NoteEditorForm(null);
            //n.Show();

            //SplashForm f = new SplashForm(null);
            //f.Show();

            //NotifyForm nf = new NotifyForm(null);
            //nf.Show();

            ServerCOMForm sc = new ServerCOMForm();
            sc.Show();

        }

        private void Trash()
        {
            //_folderSelector.Finalize();
            //labelInfo2.Text = _folderSelector.SelectedEntity.FolderInfo?.Name;
            // _folderSelector.SelectFolder(temp);

            //temp = _folderSelector.SelectedEntity;

        }

    }
}

