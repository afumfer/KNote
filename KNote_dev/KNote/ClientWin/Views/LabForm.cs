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

        private FolderSelectorComponent folderSelector;

        public LabForm()
        {
            InitializeComponent();
        }

        public LabForm(Store store) : this()
        {
            _store = store;
            folderSelector = new FolderSelectorComponent(_store);
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

            //var folderSelector = new FolderSelectorComponent(_store);
            folderSelector.Run();

            //LoadNotes();   // opción 3
        }

        private void buttonTest2_Click(object sender, EventArgs e)
        {
            folderSelector.Finalize();
        }
    }
}

