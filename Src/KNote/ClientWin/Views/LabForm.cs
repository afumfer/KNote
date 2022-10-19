﻿using System.Data;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Runtime.InteropServices;

using KNote.ClientWin.Core;
using KNote.ClientWin.Components;
using KNote.Service;
using KNote.Model;
using KNote.Model.Dto;
using KntScript;

using KntRedmineApi;
using Pandoc;
using CliWrap;
using System.Globalization;
using System.Reflection;

namespace KNote.ClientWin.Views;

public partial class LabForm : Form
{
    #region Private fields

    private string _pathSampleScripts = @"..\..\..\AutoKntScripts\";
    private string _selectedFile;

    private Store _store;

    #endregion

    #region Constructors and FormLoad

    public LabForm()
    {
        InitializeComponent();
    }

    public LabForm(Store store) : this()
    {
        _store = store;
    }
    private async void LabForm_Load(object sender, EventArgs e)
    {
        // KntScript
        if (Directory.Exists(_pathSampleScripts))
            LoadListScripts(_pathSampleScripts);

        // WebView2
        webView2.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;
        webView2.NavigationStarting += WebView2_NavigationStarting;
        webView2.NavigationCompleted += WebView2_NavigationCompleted;

        await webView2.EnsureCoreWebView2Async(null);

        if ((webView2 == null) || (webView2.CoreWebView2 == null))
        {
            textStatusWebView2.Text = "webView2 not ready";
        }

        // RedMineAPI
        textHost.Text = _store.AppConfig.HostRedmine;
        textApiKey.Text = _store.AppConfig.ApiKeyRedmine;
        textIssuesImportFile.Text = _store.AppConfig.IssuesImportFile;

        try
        {
            textIssuesId.Text = File.ReadAllText(textIssuesImportFile.Text);
        }
        catch { }

    }

    #endregion

    #region Form events handlers (KntScript)


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
        com.CodeFile = Path.Combine(_pathSampleScripts, _selectedFile);

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

    private void buttonSelectScriptDirectory_Click(object sender, EventArgs e)
    {
        using (var fbd = new FolderBrowserDialog())
        {
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                _pathSampleScripts = fbd.SelectedPath;
                listSamples.Items.Clear();
                LoadListScripts(_pathSampleScripts);
            }
        }
    }

    private void buttonReadVar_Click(object sender, EventArgs e)
    {
        var listVars = new List<ReadVarItem>();

        listVars.Add(new ReadVarItem
        {
            Label = "Type new tag:",
            VarIdent = "Tag",
            VarValue = "",
            VarNewValueText = ""
        });

        var formReadVar = new ReadVarForm(listVars);
        formReadVar.Text = "Tags for selected notes";
        formReadVar.Size = new Size(500, 150);
        var result = formReadVar.ShowDialog();

        if (result == DialogResult.Cancel)
            MessageBox.Show("Cancel");
        else
        {
            var xx = listVars[0].VarNewValueText;
            MessageBox.Show(xx);
        }

    }

    private void buttonProcessStart_Click(object sender, EventArgs e)
    {
        string url = @"https://github.com/afumfer/kntscript/blob/master/README.md";

        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    private void buttonReflection_Click(object sender, EventArgs e)
    {
        var attribute = new KAttributeDto();

        attribute.NoteTypeDto = new NoteTypeDto { Description = "aaa", Name = "bbbbb", NoteTypeId = Guid.NewGuid(), ParenNoteTypeId = null };

        attribute.KAttributeValues.Add(new KAttributeTabulatedValueDto
        {
            Description = "bbb",
            KAttributeId = Guid.NewGuid()
            ,
            KAttributeTabulatedValueId = Guid.NewGuid(),
            Order = 1,
            Value = "111"
        });

        listMessages.Items.Add("TEST IsDirty: ");
        listMessages.Items.Add("----------------");
        listMessages.Items.Add("Original value IsDirty: " + attribute.IsDirty());
        listMessages.Items.Add("Original value NoteType IsDirty: " + attribute.NoteTypeDto.IsDirty());
        foreach (var a in attribute.KAttributeValues)
            listMessages.Items.Add("Original value KAttributeValue IsDirty: " + a.IsDirty());

        attribute.SetIsDirty(false);
        listMessages.Items.Add("==== changed IsDirty attribute to false with SetIsDirty()");

        listMessages.Items.Add("Changed value IsDirty: " + attribute.IsDirty());
        listMessages.Items.Add("Changed value NoteType IsDirty: " + attribute.NoteTypeDto.IsDirty());
        foreach (var a in attribute.KAttributeValues)
            listMessages.Items.Add("Changed value KAttributeValue IsDirty: " + a.IsDirty());

        attribute.KAttributeValues[0].Value = "222";
        listMessages.Items.Add("==== changed value to child object");

        listMessages.Items.Add("Changed value IsDirty: " + attribute.IsDirty());
        listMessages.Items.Add("Changed value NoteType IsDirty: " + attribute.NoteTypeDto.IsDirty());
        foreach (var a in attribute.KAttributeValues)
            listMessages.Items.Add("Changed value KAttributeValue IsDirty: " + a.IsDirty());

        listMessages.Items.Add("");
        listMessages.Items.Add("TEST IsValid: ");
        listMessages.Items.Add("----------------");
        listMessages.Items.Add("Original value IsValid: " + attribute.IsValid());
        listMessages.Items.Add("Original value NoteType IsValid: " + attribute.NoteTypeDto.IsValid());
        foreach (var a in attribute.KAttributeValues)
            listMessages.Items.Add("Original value KAttributeValue IsValid: " + a.IsValid());

        attribute.Name = "ZZZZZZZZZZ";

        listMessages.Items.Add("--OK --------------");
        listMessages.Items.Add("Original value IsValid: " + attribute.IsValid());
        listMessages.Items.Add("Original value NoteType IsValid: " + attribute.NoteTypeDto.IsValid());
        foreach (var a in attribute.KAttributeValues)
            listMessages.Items.Add("Original value KAttributeValue IsValid: " + a.IsValid());

        attribute.KAttributeValues[0].Value = "";

        listMessages.Items.Add("--Error --------------");
        listMessages.Items.Add("Original value IsValid: " + attribute.IsValid());
        listMessages.Items.Add("Original value NoteType IsValid: " + attribute.NoteTypeDto.IsValid());
        foreach (var a in attribute.KAttributeValues)
            listMessages.Items.Add("Original value KAttributeValue IsValid: " + a.IsValid());

        attribute.NoteTypeDto.Name = "";
        attribute.Name = "";
        listMessages.Items.Add("--GetErrorMessage --------------");
        var errMsg = attribute.GetErrorMessage(false);
        listMessages.Items.Add(errMsg);
        errMsg = attribute.GetErrorMessage();
        listMessages.Items.Add(errMsg);
    }

    private void buttonRunMonitor_Click(object sender, EventArgs e)
    {
        var monitor = new MonitorComponent(_store);
        monitor.Run();
    }

    #endregion

    #region ANotas import

    private async void buttonImportAnotasXML_Click(object sender, EventArgs e)
    {
        if (_store.ActiveFolderWithServiceRef == null)
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

        if (userId == null)
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
            TextReader reader = new StreamReader(xmlFile, Encoding.Unicode);
            XmlSerializer serializer = new XmlSerializer(typeof(ANotasExport));
            anotasImport = (ANotasExport)serializer.Deserialize(reader);
            reader.Close();

            var etiquetas = anotasImport.Etiquetas
                .Select(e =>
                    {
                        string desEtiqueta = "";
                        if (e.DesEtiqueta[0] == '!')
                            desEtiqueta = e.DesEtiqueta.Substring(1, e.DesEtiqueta.Length - 1);
                        else
                            desEtiqueta = e.DesEtiqueta;

                        return new EtiquetaExport { CodEtiqueta = e.CodEtiqueta, DesEtiqueta = desEtiqueta, CodPadre = e.CodPadre };
                    }
                )
                .OrderBy(e => e.DesEtiqueta).ToList();


            // Import tags / attributes
            await ImportTags(service, etiquetas);

            // Import folders and notes
            foreach (var c in anotasImport.Carpetas)
            {
                // listMessages.Items.Add($"Folder: {c.NombreCarpeta}");
                await SaveFolderDto(service, userId, c, null, etiquetas);
            }

            // Update attributes
            var resAtrProc = await UpdateAttributes(service, etiquetas);

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        MessageBox.Show("Process finished ");

    }

    private async Task<bool> UpdateAttributes(IKntService service, List<EtiquetaExport> etiquetas)
    {
        var allNotes = (await service.Notes.GetAllAsync()).Entity;
        var i = 0;
        var nRegs = allNotes.Count;
        foreach (var n in allNotes)
        {
            i++;

            if (!string.IsNullOrEmpty(n.Tags))
            {
                var tags = ProcessTag(n.Tags);
                var note = (await service.Notes.GetAsync(n.NoteId)).Entity;
                foreach (var t in tags)
                {
                    foreach (var atr in note.KAttributesDto)
                    {
                        if (atr.Description.Contains(t.Key))
                        {

                            // TODO: hay que quitar el ! inicial
                            var etiqueta = etiquetas.Find(e => e.CodEtiqueta == t.Value);
                            if (etiqueta != null)
                            {
                                if (!string.IsNullOrEmpty(atr.Value))
                                    atr.Value += ", ";
                                atr.Value += etiqueta.DesEtiqueta;
                            }
                            break;
                        }
                    }
                }

                label1.Text = $"{note.NoteId} - {i}/{nRegs}";
                label2.Text = note.Tags.ToString();
                var resSave = await service.Notes.SaveAsync(note);
            }
        }

        return await Task.FromResult<bool>(true);
    }

    private List<TagKeyValue> ProcessTag(string tags)
    {
        List<TagKeyValue> listTags = new List<TagKeyValue>();

        var items = tags.Split(';');
        foreach (var i in items)
        {
            var item = i.Trim();
            var values = item.Split('=');
            if (values.Length > 1)
            {
                listTags.Add(new TagKeyValue { Key = values[0], Value = values[1] });
            }
        }

        return listTags;
    }

    private class TagKeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }


    private async Task<bool> ImportTags(IKntService service, List<EtiquetaExport> etiquetas)
    {
        int orderAtrTab = 0;
        int orderAtr = 0;

        var filtroEtiquetas = etiquetas
            .Where(e => (e.CodPadre == "[!EtiquetaRaiz]" || e.CodPadre == "UU") &&
            (e.CodEtiqueta != "TB" && e.CodEtiqueta != "ND" && e.CodEtiqueta != "OP" && e.CodEtiqueta != "TR" && e.CodEtiqueta != "TU" && e.CodEtiqueta != "UU"))
            .Select(e => e).OrderBy(e => e.DesEtiqueta).ToList();

        foreach (var e in filtroEtiquetas)
        {

            listMessages.Items.Add($"{e.DesEtiqueta} - {e.CodEtiqueta} - {e.CodPadre?.ToString()}");
            KAttributeDto attributeDto = new KAttributeDto
            {
                Description = $"[{e.CodEtiqueta}] - " + e.DesEtiqueta,
                Name = e.DesEtiqueta,
                KAttributeDataType = EnumKAttributeDataType.TagsValue,
                Disabled = false,
                Order = orderAtr++,
                RequiredValue = false
            };

            var tabulatedValues = etiquetas.Where(ev => ev.CodPadre == e.CodEtiqueta).Select(ev => ev).ToList();
            List<KAttributeTabulatedValueDto> tabulatedValuesAtr = new List<KAttributeTabulatedValueDto>();
            orderAtrTab = 0;
            foreach (var t in tabulatedValues)
            {

                KAttributeTabulatedValueDto atrValue = new KAttributeTabulatedValueDto
                {
                    Value = t.DesEtiqueta,
                    Description = $"[{t.CodEtiqueta}] - " + t.DesEtiqueta,
                    Order = orderAtrTab++
                };
                tabulatedValuesAtr.Add(atrValue);
            }

            attributeDto.KAttributeValues = tabulatedValuesAtr;

            // Hack import TareasDesarrolloDB
            if (attributeDto.Name == "Consejería de Educación")
            {
                attributeDto.Name = "00 - Usuario Consejería de Educación";
                attributeDto.Description = $"[{e.CodEtiqueta}] - " + attributeDto.Name;
                attributeDto.Order = 0;
            }
            if (attributeDto.Name == "Empresa de Servicios TIC")
            {
                attributeDto.Name = "00 - Usuarios Empresa de Servicios TIC";
                attributeDto.Description = $"[{e.CodEtiqueta}] - " + attributeDto.Name;
                attributeDto.Order = 0;
            }
            //if (attributeDto.Name[0] == '!')
            //    attributeDto.Name = attributeDto.Name.Substring(1, attributeDto.Name.Length - 1 );

            // Save Data
            var res = await service.KAttributes.SaveAsync(attributeDto);
        }

        return await Task.FromResult<bool>(true);
    }

    private async Task<bool> SaveFolderDto(IKntService service, Guid? userId, CarpetaExport carpetaExport, Guid? parent, List<EtiquetaExport> etiquetas)
    {
        string r11 = "\r\n";
        string r12 = "\n";

        string r21 = "&#x";
        string r22 = "$$$";

        int nErrors = 0;

        #region Import customization 

        //// afumfer
        //// .......
        //string r31 = @"D:\KaNote\Resources\ImgsEditorHtml";
        //string r32 = @"D:\Anotas\Docs\__Imgs_!!ANTHtmlEditor!!_";

        //string r41 = @"D:\KaNote\Resources\ImgsEditorHtml";
        //string r42 = @"C:\Anotas\Docs\__Imgs_!!ANTHtmlEditor!!_";

        //string r51 = @"_KNTERRORTRAP";
        //string r52 = @"_ANTERRORTRAP";
        //string r61 = @"_KNTERRORCODE";
        //string r62 = @"_ANTERRORCODE";
        //string r71 = @"_KNTERRORDESCRIPTION";
        //string r72 = @"_ANTERRORDESCRIPTION";
        //string r81 = "";
        //string r82 = "[!ExecuteAnTScriptBGroundThread]";
        //string r91 = "";
        //string r92 = "_ANTForm.Exit();";

        #endregion

        string r31 = @"KntResCon/";
        string r32 = @"\\educacion.org\Almacen\Pincel\TareasTM\Doc\";

        string r41 = @"KntResCon/";
        string r42 = @"\\educacion.org\Almacen\Pincel\tareasTM\Doc\";

        string r51 = @"KntResCon/";
        string r52 = @"\\Educacion.org\Almacen\Pincel\TareasTM\Doc\";

        var newFolderDto = new FolderDto
        {
            FolderNumber = carpetaExport.IdCarpeta,
            //FolderNumber = 0,
            Name = carpetaExport.NombreCarpeta,
            Order = carpetaExport.Orden,
            OrderNotes = carpetaExport.OrdenNotas,
            ParentId = parent
        };

        var resNewFolder = await service.Folders.SaveAsync(newFolderDto);
        label1.Text = $"Added folder: {resNewFolder.Entity?.Name}";
        label1.Refresh();

        var folder = resNewFolder.Entity;

        foreach (var n in carpetaExport.Notas)
        {
            try
            {
                if (n.DescripcionNota.Contains(r12))
                {
                    // Hack multiples CR LF 
                    n.DescripcionNota = n.DescripcionNota.Replace(r12, r11);
                    // Hack for problems in deserialization
                    n.DescripcionNota = n.DescripcionNota.Replace(r22, r21);
                }

                #region Import customization 

                //// afumfer
                //// .......
                //// Hack inserted resources change
                n.DescripcionNota = n.DescripcionNota.Replace(r32, r31);
                n.DescripcionNota = n.DescripcionNota.Replace(r42, r41);
                n.DescripcionNota = n.DescripcionNota.Replace(r52, r51);
                //n.DescripcionNota = n.DescripcionNota.Replace(r62, r61);
                //n.DescripcionNota = n.DescripcionNota.Replace(r72, r71);
                //n.DescripcionNota = n.DescripcionNota.Replace(r82, r81);
                //n.DescripcionNota = n.DescripcionNota.Replace(r92, r91);

                #endregion

                (string descriptionNew, string scriptCode) = ExtractAnTScriptCode(n.DescripcionNota);

                DateTime fecMod;
                if (n.FechaModificacion > n.FechaHoraCreacion)
                    fecMod = n.FechaModificacion;
                else
                    fecMod = n.FechaHoraCreacion;

                var newNote = new NoteExtendedDto
                {
                    FolderId = folder.FolderId,
                    NoteNumber = n.IdNota,
                    //NoteNumber = 0,                    

                    Description = descriptionNew,
                    Script = scriptCode,

                    Topic = n.Asunto,
                    CreationDateTime = n.FechaHoraCreacion,
                    ModificationDateTime = fecMod,
                    Tags = n.PalabrasClave,
                    InternalTags = n.Vinculo,
                    Priority = n.Prioridad
                };

                if (!newNote.Tags.Contains("UC="))
                {
                    if ("afumfer fdomher jurivmar sesther sleoare dgoddelw".Contains(n.Usuario))
                    {
                        if (!string.IsNullOrEmpty(newNote.Tags))
                            newNote.Tags += "; ";
                        newNote.Tags += "UC=" + n.Usuario;
                    }
                }

                // Add task
                if (n.FechaInicio > new DateTime(1901, 1, 1) ||
                    n.FechaResolucion > new DateTime(1901, 1, 1) ||
                    n.FechaPrevistaInicio > new DateTime(1901, 1, 1) ||
                    n.FechaPrevistaFin > new DateTime(1901, 1, 1) ||
                    n.Resuelto == true)
                {
                    NoteTaskDto task = new NoteTaskDto
                    {
                        NoteId = Guid.Empty,
                        UserId = (Guid)userId,
                        CreationDateTime = n.FechaHoraCreacion,
                        ModificationDateTime = fecMod,
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

                // Add alarm
                if (n.Alarma > new DateTime(1901, 1, 1))
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
                    #region Import resources for ContendInDB = true

                    //// TODO: Refactor this line
                    //var root = @"D:\ANotas\Docs";

                    //var fileFullName = $"{root}{n.NotaEx}";

                    //if (File.Exists(fileFullName))
                    //{
                    //    var fileArrayBytes = File.ReadAllBytes(fileFullName);
                    //    var contentBase64 = Convert.ToBase64String(fileArrayBytes);
                    //    ResourceDto resource = new ResourceDto
                    //    {
                    //        NoteId = Guid.Empty,
                    //        ContentInDB = true,
                    //        Order = 1,
                    //        Container = service.RepositoryRef.ResourcesContainer + "\\" + DateTime.Now.Year.ToString(),
                    //        Name = $"{Guid.NewGuid()}_{Path.GetFileName(fileFullName)}",
                    //        Description = $"(ANotas import {n.NotaEx})",
                    //        ContentArrayBytes = fileArrayBytes,
                    //        ContentBase64 = contentBase64,
                    //        FileType = _store.ExtensionFileToFileType(Path.GetExtension(fileFullName))
                    //    };
                    //    newNote.Resources.Add(resource);
                    //}

                    #endregion

                    #region Import resources for ContendInDB = false

                    if (n.NotaEx[0] == '\\')
                        n.NotaEx = n.NotaEx.Substring(1);

                    string fileImport = Path.Combine(new[] { service.RepositoryRef.ResourcesContainerCacheRootPath, service.RepositoryRef.ResourcesContainer, n.NotaEx });

                    ResourceDto resource = new ResourceDto
                    {
                        NoteId = Guid.Empty,
                        ContentInDB = false,
                        Order = 1,
                        Container = service.RepositoryRef.ResourcesContainer,
                        Name = n.NotaEx,
                        Description = $"(ANotas import {n.NotaEx})",
                        ContentArrayBytes = null,
                        ContentBase64 = null,
                        FileType = _store.ExtensionFileToFileType(Path.GetExtension(fileImport))
                    };
                    newNote.Resources.Add(resource);

                    #endregion 

                }

                // Save note and PostIt
                Result<NoteExtendedDto> resNewNote = null;
                if (newNote.IsValid())
                {
                    resNewNote = await service.Notes.SaveExtendedAsync(newNote);
                    label2.Text = $"Added note: {resNewNote.Entity?.Topic} - {resNewNote.Entity?.NoteId.ToString()}";

                    // Add Window
                    WindowDto windowPostIt = new WindowDto
                    {
                        NoteId = resNewNote.Entity.NoteId,
                        UserId = (Guid)userId,
                        Visible = false,  //n.Visible,
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

                }
                else
                {
                    label2.Text = $"Added note: ERROR invalid note.";
                    var msgErr = newNote.GetErrorMessage();
                    MessageBox.Show($"ERROR invalid note: {msgErr}");
                }

                label2.Refresh();

            }
            catch (Exception ex)
            {
                // TODO: hack, hay un registro erróneo en la exportación. 
                nErrors++;
                if (nErrors > 1)
                    MessageBox.Show($"Más de error. Error: {ex.Message}");
                //throw;
            }

        }

        // For each folder child all recursively  to this method
        foreach (var c in carpetaExport.CarpetasHijas)
        {
            await SaveFolderDto(service, userId, c, folder.FolderId, etiquetas);
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

    private (string, string) ExtractAnTScriptCode(string descriptionIn)
    {
        int indFrom, indTo, lenCode;

        if (string.IsNullOrEmpty(descriptionIn))
            return (descriptionIn, null);

        indFrom = descriptionIn.IndexOf("[!AnTScript]");
        indFrom = (indFrom >= 0) ? indFrom + 12 : -1;
        if (indFrom < 0)
            return (descriptionIn, null);

        indTo = descriptionIn.IndexOf("[/AnTScript]", indFrom);
        indTo = (indTo < 0) ? descriptionIn.Length : indTo;

        lenCode = indTo - indFrom;

        if (lenCode <= 0)
            return (descriptionIn, null);
        else
        {
            string DescriptionOut = "";
            string ScriptOut = "";
            ScriptOut = descriptionIn.Substring(indFrom, lenCode);

            DescriptionOut = descriptionIn.Replace("[!AnTScript]" + ScriptOut + "[/AnTScript]", "");

            return (DescriptionOut, ScriptOut);
        }

    }

    #endregion

    #region WebView2

    private void btnNavigate_Click(object sender, EventArgs e)
    {
        //webView2.NavigateToString("https://www.elpais.es");
        webView2.CoreWebView2.Navigate(textUrlWebView2.Text);
    }

    private void btnGoBack_Click(object sender, EventArgs e)
    {
        webView2.CoreWebView2.GoBack();
    }

    private void WebView2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
    {
        //textStatusWebView2.Text = webView2.Source.ToString();
    }

    private void WebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
    {
        //String uri = e.Uri;
        //if (!uri.StartsWith("https://"))
        //{
        //    webView2.CoreWebView2.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link')");
        //    e.Cancel = true;
        //}

        textStatusWebView2.Text = e.Uri;
    }

    private void WebView2_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
    {
        textStatusWebView2.Text = "WebView_CoreWebView2InitializationCompleted";
    }

    #endregion

    #region KntRedmineApi

    private async void buttonTestKntRedmineApi_Click(object sender, EventArgs e)
    {
        if (_store.ActiveFolderWithServiceRef == null)
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

        if (userId == null)
        {
            MessageBox.Show("There is no valid user to import data ");
            return;
        }

        if (string.IsNullOrEmpty(textIssuesId.Text))
        {
            MessageBox.Show("Issues ID not selected.");
            return;
        }

        // TODO: Refactor this ....
        _store.AppConfig.HostRedmine = textHost.Text;
        _store.AppConfig.ApiKeyRedmine = textApiKey.Text;
        _store.AppConfig.IssuesImportFile = textIssuesImportFile.Text;
        _store.AppConfig.ToolsPath = Path.GetDirectoryName(_store.AppConfig.IssuesImportFile);

        var manager = new KntRedmineManager(_store.AppConfig.HostRedmine, _store.AppConfig.ApiKeyRedmine);
        //var pandocEngine = new PandocEngine($"{_store.AppConfig.ToolsPath}/pandoc.exe");        
        PandocInstance.SetPandocPath($"{_store.AppConfig.ToolsPath}/pandoc.exe");

        // ----

        var filter = new NotesFilterDto();

        int rootFolNum = 1;
        if (!string.IsNullOrEmpty(textFolderNumForImportIssues.Text))
            int.TryParse(textFolderNumForImportIssues.Text, out rootFolNum);
        var parentFolder = (await service.Folders.GetAsync(rootFolNum)).Entity;
        var folders = (await service.Folders.GetAllAsync()).Entity;

        var hhuu = GetHUs(textIssuesId.Text);
        var i = 1;
        listInfoRedmine.Items.Clear();

        foreach (var hu in hhuu)
        {
            string folderName = "";
            NoteExtendedDto note = (await service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = Guid.Parse("4A3E0AE2-005D-44F0-8BF0-7E0D2A60F6C7") })).Entity;

            filter.Tags = $"HU#{hu}";

            var notes = (await service.Notes.GetFilter(filter)).Entity;

            if (notes != null)
            {
                if (notes.Count > 0)
                    note = (await service.Notes.GetExtendedAsync(notes[0].NoteId)).Entity;

            }

            var res = manager.IssueToNoteDto(hu, note);

            if (res)
            {
                note.Tags = filter.Tags;
                folderName = note.KAttributesDto[2].Value;

                foreach (var r in note.Resources)
                {
                    r.FileType = _store.ExtensionFileToFileType(Path.GetExtension(r.Name));
                }

                var folder = folders.FirstOrDefault(f => f.Name == folderName);

                if (folder != null)
                {
                    note.FolderId = folder.FolderId;
                }
                else
                {
                    FolderDto newFolder = new FolderDto
                    {
                        Name = folderName,
                        ParentId = parentFolder.FolderId
                    };

                    var resSave = await service.Folders.SaveAsync(newFolder);

                    if (resSave.IsValid)
                    {
                        folders = (await service.Folders.GetAllAsync()).Entity;
                        note.FolderId = resSave.Entity.FolderId;
                    }
                    else
                        note.FolderId = parentFolder.FolderId;
                }

                foreach (var r in note.Resources)
                {
                    if (string.IsNullOrEmpty(r.Container))
                    {
                        r.Container = $"{service.RepositoryRef.ResourcesContainer}/{DateTime.Now.Year.ToString()}";
                    }

                    r.Container = r.Container.Replace('\\', '/');

                    var org = $"!{r.NameOut}!";
                    var dest = $"![alt text]({r.Container}/{r.Name})";

                    note.Description = note.Description.Replace(org, dest, true, CultureInfo.CurrentCulture);
                }

                // iIefficient version
                //note.Description = TextToMarkdown(_store.AppConfig.ToolsPath, note.Description);

                // Other version
                note.Description = await TextToMarkdown2(_store.AppConfig.ToolsPath, note.Description);

                // This version, pending encoding issue ...
                //note.Description = await pandocEngine.ConvertToText<TextileIn, CommonMarkOut>(note.Description);                
                //note.Description = note.Description.Replace("\\[", "[");
                //note.Description = note.Description.Replace("\\]", "]");

                var resSaveNote = await service.Notes.SaveExtendedAsync(note);
                listInfoRedmine.Items.Add($"{i++} - #{note.Tags}: {note.Topic}");
                listInfoRedmine.Refresh();
            }
            else
            {
                listInfoRedmine.Items.Add($"Error");
                listInfoRedmine.Refresh();
            }
        }

        MessageBox.Show("End import");
    }

    private string[] GetHUs(string strIssuesId)
    {
        return strIssuesId.Split("\r\n");
    }

    private void buttonIssuesImportFile_Click(object sender, EventArgs e)
    {
        openFileDialog.Title = "Select file Issues ID file";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            var fileTmp = openFileDialog.FileName;
            textIssuesImportFile.Text = fileTmp;
            textIssuesId.Text = File.ReadAllText(fileTmp);
            _store.AppConfig.IssuesImportFile = textIssuesImportFile.Text;
        }
    }

    private async void buttonFindIssue_Click(object sender, EventArgs e)
    {
        if (_store.ActiveFolderWithServiceRef == null)
        {
            MessageBox.Show("There is no archive selected ");
            return;
        }

        var serviceRef = _store.ActiveFolderWithServiceRef.ServiceRef;
        var service = serviceRef.Service;

        NoteExtendedDto note = (await service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = Guid.Parse("4A3E0AE2-005D-44F0-8BF0-7E0D2A60F6C7") })).Entity;
        var manager = new KntRedmineManager(_store.AppConfig.HostRedmine, _store.AppConfig.ApiKeyRedmine);

        textPredictSubject.Text = "";
        textPredictDescription.Text = "";
        textPredictCategory.Text = "";
        textPredictionGestion.Text = "";
        
        textPredictionPH.Text = "";

        var res = manager.IssueToNoteDto(textPredictFindIssue.Text, note, false);

        textPredictSubject.Text = note.Topic;
        textPredictDescription.Text = note.Description;

        if (note.KAttributesDto.Count < 3)
            MessageBox.Show("You do not have the experimental database selected with the RedMine Educa import.");
        else
            textPredictCategory.Text = note.KAttributesDto[2].Value;
    }

    private void buttonPredictGestion_Click(object sender, EventArgs e)
    {
        textPredictionGestion.Text = "";
        var manager = new KntRedmineManager(_store.AppConfig.HostRedmine, _store.AppConfig.ApiKeyRedmine);
        textPredictionGestion.Text = manager.PredictGestion(textPredictSubject.Text, textPredictDescription.Text);
    }

    private void buttonPredictPH_Click(object sender, EventArgs e)
    {
        textPredictionPH.Text = "";
        var manager = new KntRedmineManager(_store.AppConfig.HostRedmine, _store.AppConfig.ApiKeyRedmine);
        textPredictionPH.Text = manager.PredictPH(textPredictCategory.Text, textPredictSubject.Text, textPredictDescription.Text);
    }

    private string TextToMarkdown(string pathUtils, string text)
    {
        // TODO: refactor this method

        // pandoc -f textile -t markdown --wrap=preserve prueba1.text -o pruebaS1.md

        var textOut = "";

        if (!Directory.Exists(pathUtils))
            return text;

        string fileIn = Path.Combine(pathUtils, "input.text");
        string fileOut = Path.Combine(pathUtils, "output.md");
        string exPandoc = Path.Combine(pathUtils, "pandoc.exe");
        string param = $" -f textile -t markdown --wrap=preserve {fileIn} -o {fileOut}";

        if (System.IO.File.Exists(fileIn))
            System.IO.File.Delete(fileIn);

        if (System.IO.File.Exists(fileOut))
            System.IO.File.Delete(fileOut);

        System.IO.File.WriteAllText(fileIn, text);

        var process = Process.Start(exPandoc, param);
        process.WaitForExit();
        var exitCode = process.ExitCode;

        if (System.IO.File.Exists(fileOut))
            textOut = System.IO.File.ReadAllText(fileOut);

        textOut = textOut.Replace("\\[", "[");
        textOut = textOut.Replace("\\]", "]");
        return textOut;

    }

    private async Task<string> TextToMarkdown2(string pathUtils, string text)
    {
        // TODO: refactor this method
        var textOut = "";

        if (!Directory.Exists(pathUtils))
            return text;

        string fileIn = Path.Combine(pathUtils, "__input.text");
        string fileOut = Path.Combine(pathUtils, "__output.md");

        if (System.IO.File.Exists(fileIn))
            System.IO.File.Delete(fileIn);

        if (System.IO.File.Exists(fileOut))
            System.IO.File.Delete(fileOut);

        System.IO.File.WriteAllText(fileIn, text);

        await PandocInstance.Convert<TextileIn, CommonMarkOut>(fileIn, fileOut, new TextileIn { }, new CommonMarkOut { Wrap = Wrap.Preserve }, default);

        if (System.IO.File.Exists(fileOut))
            textOut = System.IO.File.ReadAllText(fileOut);

        textOut = textOut.Replace("\\[", "[");
        textOut = textOut.Replace("\\]", "]");
        return textOut;
    }

    #endregion

    #region Plugin

    private void buttonPlugin_Click(object sender, EventArgs e)
    {
        var c = new KntRedminePluginCommand();
        c.InjectService(_store.ActiveFolderWithServiceRef?.ServiceRef.Service);
        c.Execute();
    }

    private void buttonGetPluginFile_Click(object sender, EventArgs e)
    {
        //var relativePath = @"KntRedmineApi\bin\Debug\net6.0-windows\KntRedmineApi.exe";
        //var root = _store.GetVsSolutionRootPath();
        //string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
        //textPlugin.Text = pluginLocation;
    }

    #endregion

}