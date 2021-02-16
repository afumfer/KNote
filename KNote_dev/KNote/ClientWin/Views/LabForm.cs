﻿using System;
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
using KNote.Model;
using KNote.Model.Dto;
using KntScript;
using System.Xml.Serialization;
using KNote.Service;

namespace KNote.ClientWin.Views
{
    public partial class LabForm : Form
    {
        #region Private fields

        private string _pathSampleScripts = @"..\..\..\AutoKntScripts\";
        private string _selectedFile;

        private Store _store;

        #endregion

        #region Constructors

        public LabForm()
        {
            InitializeComponent();            
        }

        public LabForm(Store store) : this()
        {
            _store = store;
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

        private void buttonTest1_Click(object sender, EventArgs e)
        {
            var monitor = new MonitorComponent(_store);
            monitor.Run();
        }

        private async void buttonTest4_Click(object sender, EventArgs e)
        {
            if(_store.ActiveFolderWithServiceRef == null)
            {
                MessageBox.Show("There is no archive selected ");
                return;
            }

            var serviceRef = _store.ActiveFolderWithServiceRef.ServiceRef;
            var service = serviceRef.Service;


            Guid? userId = null;
            var userDto = (await service.Users.GetByUserNameAsync(_store.AppUserName)).Entity;
            if (userDto != null)
                userId = userDto.UserId;
            
            if(userId == null)
            {
                MessageBox.Show("There is no valid user to import data ");
                return;
            }

            var xmlFile = "";
            openFileDialog.Title = "Get import ANotas xml file";
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "fichero xml (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                xmlFile = openFileDialog.FileName;
            }
            
            try
            {
                if (!File.Exists(xmlFile))
                {
                    MessageBox.Show("Invalid file");
                    return;
                }

                ANotasExport anotasImport;
                TextReader reader = new StreamReader(xmlFile);
                XmlSerializer serializer = new XmlSerializer(typeof(ANotasExport));
                anotasImport = (ANotasExport)serializer.Deserialize(reader);
                reader.Close();

                foreach(var c in anotasImport.Carpetas)
                {
                    // listMessages.Items.Add($"Folder: {c.NombreCarpeta}");
                    await SaveFolderDto(service, userId, c, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);                
            }

            MessageBox.Show("Process finished ");

        }

        private async Task<bool> SaveFolderDto(IKntService service, Guid? userId, CarpetaExport carpetaExport, Guid? parent)
        {
            string c1 = "\r\n";
            string c2 = "\n";

            var newFolderDto = new FolderDto
            {
                FolderNumber = carpetaExport.IdCarpeta,
                Name = carpetaExport.NombreCarpeta,
                Order = carpetaExport.Orden,
                OrderNotes = carpetaExport.OrdenNotas,
                ParentId = parent
            };
            
            var resNewFolder = await service.Folders.SaveAsync(newFolderDto);
            label1.Text = $"Added folder: {resNewFolder.Entity?.Name}";
            label1.Refresh();

            var folder = resNewFolder.Entity;

            foreach(var n in carpetaExport.Notas)
            {
                if (n.DescripcionNota.Contains(c2))
                {
                    //MessageBox.Show(c2.ToString());
                    n.DescripcionNota = n.DescripcionNota.Replace(c2, c1);
                }

                var newNote = new NoteExtendedDto
                {                   
                    FolderId = folder.FolderId,
                    NoteNumber = n.IdNota,
                    Description = n.DescripcionNota,
                    Topic = n.Asunto, 
                    CreationDateTime = n.FechaHoraCreacion,
                    ModificationDateTime = n.FechaModificacion,
                    Tags = n.PalabrasClave,
                    InternalTags = n.Vinculo,
                    Priority = n.Prioridad                    
                };
              

                // Add alarm
                if (n.Alarma > new DateTime(1901,1,1) )
                {
                    var message = new KMessageDto
                    {
                        NoteId = Guid.Empty,
                        UserId = userId,
                        AlarmActivated = n.ActivarAlarma,
                        ActionType = EnumActionType.UserAlarm,
                        Comment = "(import ANotas)",
                        AlarmDateTime = n.Alarma
                    };

                    // Alarm type
                    switch (n.TipoAlarma)
                    {
                        case 0:  // estandar
                            message.AlarmType = EnumAlarmType.Standard;    
                            break;
                        case 1:  // diaria
                            message.AlarmType = EnumAlarmType.Daily;
                            break;
                        case 2:  // semanal
                            message.AlarmType = EnumAlarmType.Weekly;
                            break;
                        case 3:  // mensual
                            message.AlarmType = EnumAlarmType.Monthly;
                            break;
                        case 4:  // anual
                            message.AlarmType = EnumAlarmType.Annual;
                            break;
                        case 5:  // cada hora
                            message.AlarmType = EnumAlarmType.InMinutes;
                            message.AlarmMinutes = 60;
                            break;
                        case 6:  // 4 horas
                            message.AlarmType = EnumAlarmType.InMinutes;
                            message.AlarmMinutes = 60 * 4;
                            break;
                        case 7:  // 8 horas
                            message.AlarmType = EnumAlarmType.InMinutes;
                            message.AlarmMinutes = 60 * 8;
                            break;
                        case 8:  // 12 diaria
                            message.AlarmType = EnumAlarmType.InMinutes;
                            message.AlarmMinutes = 60 * 12;
                            break;
                        default:
                            message.AlarmType = EnumAlarmType.Standard;
                            break;
                    }

                    newNote.Messages.Add(message);
                }

                // Add resource                
                if (!string.IsNullOrEmpty(n.NotaEx))
                {
                    // TODO: Refactor this line
                    var root = @"C:\ANotas\Docs";

                    var fileFullName = $"{root}{n.NotaEx}";

                    if (File.Exists(fileFullName))
                    {
                        var fileArrayBytes = File.ReadAllBytes(fileFullName);
                        var contentBase64 = Convert.ToBase64String(fileArrayBytes);
                        ResourceDto resource = new ResourceDto
                        {
                            NoteId = Guid.Empty,
                            ContentInDB = true,
                            Order = 1,                        
                            Container = KntConst.ContainerResources + "\\" + DateTime.Now.Year.ToString(),
                            Name = $"{Guid.NewGuid()}_{Path.GetFileName(fileFullName)}",
                            Description = $"(ANotas import {n.NotaEx})",
                            ContentArrayBytes = fileArrayBytes,
                            ContentBase64 = contentBase64,
                            FileType = _store.ExtensionFileToFileType(Path.GetExtension(fileFullName))
                        };
                        newNote.Resources.Add(resource);
                    }
                }

                // Add task
                if (n.FechaInicio > new DateTime(1901, 1, 1) ||
                    n.FechaResolucion > new DateTime(1901, 1, 1) ||
                    n.FechaPrevistaInicio > new DateTime(1901, 1, 1) ||
                    n.FechaPrevistaFin > new DateTime(1901, 1, 1) ||
                    n.Resuelto == true )
                {
                    NoteTaskDto task = new NoteTaskDto
                    {
                        NoteId = Guid.Empty,
                        UserId = (Guid)userId,
                        CreationDateTime = n.FechaHoraCreacion,
                        ModificationDateTime = n.FechaModificacion,
                        Description = newNote.Topic,
                        Tags = "(ANotas import)",
                        Priority = 1,
                        Resolved = n.Resuelto,
                        EstimatedTime = n.TiempoEstimado,
                        SpentTime = n.TiempoInvertido,
                        DifficultyLevel = n.NivelDificultad,
                        ExpectedEndDate = n.FechaPrevistaFin,
                        ExpectedStartDate = n.FechaPrevistaInicio,
                        EndDate = n.FechaResolucion,
                        StartDate = n.FechaInicio
                    };
                    newNote.Tasks.Add(task);
                }
                
                // Save note and PostIt
                var resNewNote = await service.Notes.SaveExtendedAsync(newNote);

                // Add Window
                WindowDto windowPostIt = new WindowDto
                {
                    NoteId = resNewNote.Entity.NoteId,
                    UserId = (Guid)userId,
                    Visible = n.Visible,
                    AlwaysOnTop = n.SiempreArriba,
                    PosX = n.PosX,
                    PosY = n.PosY,
                    Width = n.Ancho,
                    Height = n.Alto,
                    FontName = n.FontName,
                    FontSize = n.FontSize,
                    FontBold = n.FontBold,
                    FontItalic = n.FontItalic,
                    FontUnderline = n.FontUnderline,
                    FontStrikethru = n.FontStrikethru,
                    ForeColor = ColorTranslator.ToHtml(ColorTranslator.FromOle(n.ForeColor)),
                    TitleColor = ColorTranslator.ToHtml(ColorTranslator.FromOle(n.ColorBanda)),
                    TextTitleColor = ColorTranslator.ToHtml(ColorTranslator.FromOle(n.ForeColor)),
                    NoteColor = ColorTranslator.ToHtml(ColorTranslator.FromOle(n.ColorNota)),
                    TextNoteColor = ColorTranslator.ToHtml(ColorTranslator.FromOle(n.ForeColor))
                };
                var resNewPostIt = await service.Notes.SaveWindowAsync(windowPostIt);

                label2.Text = $"Added note: {resNewNote.Entity?.Topic} - {resNewPostIt.Entity?.WindowId.ToString()}";
                label2.Refresh();
            }

            // For each folder child all recursively  to this method
            foreach(var c in carpetaExport.CarpetasHijas)
            {
                await SaveFolderDto(service, userId, c, folder.FolderId);
            }

            return await Task.FromResult<bool>(true);
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

    }

}
