using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using KNote.ClientWin.Core;
using KNote.ClientWin.Components;
using KNote.Model.Dto;
using KntScript;



namespace KNote.ClientWin.Views
{
    public partial class LabForm : Form
    {
        #region Private fields

        private string _pathSampleScripts = @"..\..\..\AutoKntScripts\";
        private string _selectedFile;

        private Store _store;

        private FoldersSelectorComponent _folderSelector;
        private NotesSelectorComponent _notesSelector;

        private KNoteManagmentComponent _knoteManagment;

        private NoteEditorComponent _noteEditor;

        private FolderWithServiceRef temp;


        #endregion

        #region Constructors

        public LabForm()
        {
            InitializeComponent();            
        }

        public LabForm(Store store) : this()
        {
            _store = store;
            _folderSelector = new FoldersSelectorComponent(_store);
            _notesSelector = new NotesSelectorComponent(_store);

            _knoteManagment = new KNoteManagmentComponent(_store);

            _noteEditor = new NoteEditorComponent(_store);

            _folderSelector.EntitySelection += _folderSelector_EntitySelection;
            _notesSelector.EntitySelection += _notesSelector_EntitySelection;

        }

        #endregion 

        #region Form events handlers (KntScript)

        private void DemoForm_Load(object sender, EventArgs e)
        {
            LoadListScripts(_pathSampleScripts);
        }

        private void buttonRunScript_Click(object sender, EventArgs e)
        {
            var kntScript = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(_store));

            kntScript.Run(@"
                            var i = 1;
                            var str = ""Hello world "";
                            for i = 1 to 10
                                printline str + i;
                            end for;                            
                            printline """";
                            str = ""(type text here ...) "";
                            readvar {""Example input str var:"": str };
                            printline str;
                            printline ""<< end >>"";                            
                        ");            
        }

        private void buttonInteract_Click(object sender, EventArgs e)
        {
            //
            // Demo, inject variables and personalized api library 
            //

            var kntScript = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(_store));

            var a = new FolderDto();
            a.Name = "My folder, to inject in script.";
            a.Tags = "my tags";
            a.CreationDateTime = DateTime.Now;

            // inject variable
            kntScript.AddVar("_a", a);

            var code = @"printline ""Demo external variables / KNoteScriptLibrary injected"";

                        ' This variable (_a) comes from the host application
                        printline _a.Name;
                        printline _a.Tags;
                        printline _a.CreationDateTime;                        

                        _a.Name = ""KntScript - changed description property !!"";                                                
                        printline _a.Name;
                        printline """";

                        printline ""<< end >>""; 
                        ";

            kntScript.Run(code);

            var b = (FolderDto)kntScript.GetVar("_a");  // -> a 
            MessageBox.Show(a.Name + " <==> " + b.Name);
        }

        private void buttonRunBackground_Click(object sender, EventArgs e)
        {
            var kntScript = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(_store));

            var code = @"
                        var i = 1;
                        var str = ""Hello world "";
                        for i = 1 to 1000
                            printline str + i;
                        end for;                            
                        printline ""<< end >>"";
                    ";

            // --- Synchronous version
            // kntScript.Run(code);

            // --- Asynchronous version
            var t = new Thread(() => kntScript.Run(code));
            t.IsBackground = false;
            t.Start();            
        }

        private void buttonShowConsole_Click(object sender, EventArgs e)
        {
            var kntEngine = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(_store));

            var com = new KntScriptConsoleComponent(_store);
            com.KntSEngine = kntEngine;

            com.Run();
            //KntScriptConsoleForm f = new KntScriptConsoleForm(com);
            //f.Show();
        }

        private void buttonShowSample_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile))
            {
                MessageBox.Show("File no seleted.");
                return;
            }

            var kntEngine = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(_store));

            var com = new KntScriptConsoleComponent(_store);
            com.KntSEngine = kntEngine;
            com.CodeFile = _pathSampleScripts + _selectedFile;

            com.Run();
            //KntScriptConsoleForm f = new KntScriptConsoleForm(com);
            //f.Show();
        }

        private void buttonRunSample_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile))
            {
                MessageBox.Show("File no seleted.");
                return;                   
            }

            var kntScript = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(_store));

            kntScript.RunFile(_pathSampleScripts + _selectedFile);
        }

        private void listSamples_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedFile = listSamples.SelectedItem.ToString();
        }

        #endregion

        #region Form events handlers (app lab)

        private async void _notesSelector_EntitySelection(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            if (e.Entity == null)
            {
                labelInfo1.Text = "";
                return;
            }

            labelInfo2.Text = $" {e.Entity.Topic} - {e.Entity.NoteId}";

            await _noteEditor.LoadModelById(temp.ServiceRef.Service, e.Entity.NoteId);
        }

        private async void _folderSelector_EntitySelection(object sender, ComponentEventArgs<FolderWithServiceRef> e)
        {
            if (e.Entity == null)
            {
                labelInfo1.Text = "";
                return;
            }

            labelInfo1.Text = $" {e.Entity.ServiceRef.Alias} - {e.Entity.FolderInfo?.Name}";
            if (_notesSelector != null)
                await _notesSelector.LoadEntities(e.Entity.ServiceRef.Service, e.Entity.FolderInfo);

            temp = e.Entity;
        }

        private void buttonTest1_Click(object sender, EventArgs e)
        {
            #region Old code 
            // opción 1
            //var service = _store.PersonalServiceRef.Service;

            //var notes = (await service.Notes.HomeNotesAsync()).Entity;
            //foreach (var note in notes)
            //    listTest.Items.Add(note.Topic);

            // or 

            //await LoadNotes();  // opción 2

            //LoadNotes();   // opción 3
            #endregion 

            var monitor = new MonitorComponent(_store);
            monitor.Run();
        }

        private void buttonTest2_Click(object sender, EventArgs e)
        {
            _folderSelector.Run();
            _notesSelector.Run();
            _noteEditor.Run();
        }

        private void buttonTest3_Click(object sender, EventArgs e)
        {
            _knoteManagment.Run();
        }

        private void buttonTest4_Click(object sender, EventArgs e)
        {
            var noteEditor = new NoteEditorComponent(_store);
            noteEditor.RunModal();            
        }

        #endregion 

        #region Private methods

        private void LoadListScripts(string pathSampleScripts)
        {
            if (Directory.Exists(pathSampleScripts))
            {
                DirectoryInfo directory = new DirectoryInfo(pathSampleScripts);
                FileInfo[] files = directory.GetFiles("*.knts");
                
                foreach (var file in files)
                    listSamples.Items.Add(file.Name);
            }
            else            
                MessageBox.Show("{0} is not a valid directory.", _pathSampleScripts);            
        }

        #endregion

        #region Trash

        ////private async Task LoadNotes()  // opción 2
        //private async void LoadNotes()   // opción 3
        //{
        //    var service = _store.PersonalServiceRef.Service;
        //    var notes = (await service.Notes.HomeNotesAsync()).Entity;
        //    foreach (var note in notes)
        //        listMessages.Items.Add(note.Topic);
        //}

        private void Trash_oldCode()
        {

            //var res1 = _folderSelector.RunModal();
            //labelInfo1.Text = res1.Entity.ToString();

            //var res2 = _notesSelector.RunModal();
            //labelInfo2.Text = res2.Entity.ToString();

            //var res3 = _knoteManagment.RunModal();
            //labelInfo3.Text = res3.Entity.ToString();


            //_folderSelector.EmbededMode = true;
            //_folderSelector.ModalMode = false;

            //panelTest1.Controls.Add((Control)_folderSelector.View.PanelView());
            //_folderSelector.Run();

            //Form1 f = new Form1();
            //panelTest1.Controls.Add((Control)f.p1);
            //f.Show();

            //_notesSelector.Finalize();
            //labelInfo3.Text = "***" + _notesSelector.SelectedEntity?.Topic;

            // _folderSelector.Finalize();
            // labelInfo2.Text = _folderSelector.SelectedEntity.FolderInfo?.Name;
            // _folderSelector.SelectFolder(temp);

            //temp = _folderSelector.SelectedEntity;

            //KNoteManagmentForm f = new KNoteManagmentForm(null);
            //f.Show();

            //NoteEditorForm n = new NoteEditorForm(null);
            //n.Show();

            //SplashForm f = new SplashForm(null);
            //f.Show();

            //NotifyForm nf = new NotifyForm(null);
            //nf.Show();

            //ServerCOMForm sc = new ServerCOMForm();
            //sc.Show();

        }

        #endregion 
    }
}
