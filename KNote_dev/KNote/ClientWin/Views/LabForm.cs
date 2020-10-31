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

        private FolderWithServiceRef temp;

        public LabForm()
        {
            InitializeComponent();
        }

        public LabForm(Store store) : this()
        {
            _store = store;
            _folderSelector = new FolderSelectorComponent(_store);
            _folderSelector.EntitySelection += _folderSelector_EntitySelection;
        }

        private void _folderSelector_EntitySelection(object sender, ComponentEventArgs<FolderWithServiceRef> e)
        {
            labelInfo1.Text = $" {e.Entity.ServiceRef.Alias} - {e.Entity.FolderInfo?.Name}";
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
            //var folderSelector = new FolderSelectorComponent(_store);
            _folderSelector.Run();            
        }

        private void buttonTest3_Click(object sender, EventArgs e)
        {
            //_folderSelector.Finalize();
            //labelInfo2.Text = _folderSelector.SelectedEntity.FolderInfo?.Name;
            _folderSelector.SelectN(temp);

        }

        private void buttonTest4_Click(object sender, EventArgs e)
        {
            temp = _folderSelector.SelectedEntity;
        }
    }
}

