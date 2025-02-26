using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using KNote.Model;
using KNote.Model.Dto;
using System.Collections.Specialized;
using System.Diagnostics;
using Pandoc;
using KNote.Repository.EntityFramework.Entities;
using System.Globalization;
using KNote.Service.Core;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Net.Mail;
using NLog.Filters;
using System.Text;
using static Azure.Core.HttpHeader;

namespace KntRedmineApi;

public class KntRedmineManager
{
    #region Private fields 

    private RedmineManager _redMineManager;
    private NameValueCollection _parameters = new () { { "include", "attachments,relations,journals" } };
    
    private List<FolderInfoDto> _folders = new();
    private FolderDto _parentFolder = new();

    private Dictionary<string, Guid> _cacheUsers = new Dictionary<string, Guid>();
    private Dictionary<string, FolderDto> _cacheFolders = new Dictionary<string, FolderDto>();

    #endregion

    #region Constructor 

    public KntRedmineManager(IKntService service)
    {
        _service = service;
    }

    #endregion 

    #region Properties

    private readonly IKntService _service;
    public IKntService Service
    {
        get { return _service; }
    }

    private string _host;
    public string Host
    {
        get { return _host; }
        set { _host = value; }
    }

    private string _apiKey;
    public string ApiKey
    {
        get { return _apiKey; } 
        set { _apiKey = value; }
    }
    
    private string _toolsPath = "";
    public string ToolsPath
    {
        get { return _toolsPath; }
        set { _toolsPath = value; }
    }

    private string _importFile;
    public string ImportFile
    {
        get { return _importFile; }
        set { _importFile = value; }
    }

    private string _rootFolderForImport;
    public string RootFolderForImport
    {
        get { return _rootFolderForImport; }
        set { _rootFolderForImport = value; }   
    }

    private string _appUserName;
    public string AppUserName   
    {
        get { return _appUserName; }
        set { _appUserName = value; }
    }


    #endregion

    #region Public methods 
    public async Task InitManager()
    {
        try
        {
            _host = await GetSystemPlugInVariable("HOST");
            _apiKey = await GetSystemPlugInVariable("APIKEY");
            _importFile = await GetSystemPlugInVariable("IMPORTFILE");
            _toolsPath = await GetSystemPlugInVariable("TOOLSPATH");
            _rootFolderForImport = await GetSystemPlugInVariable("ROOTFOLDERFORIMPORT");
            _appUserName = await GetSystemPlugInVariable("APPUSERNAME");

            _redMineManager = new RedmineManager(_host, _apiKey);

            _folders = (await _service.Folders.GetAllAsync()).Entity;

            int rootFolNum = 1;
            if (!string.IsNullOrEmpty(_rootFolderForImport))
                int.TryParse(_rootFolderForImport, out rootFolNum);
            _parentFolder = (await _service.Folders.GetAsync(rootFolNum)).Entity;

            //var pandocEngine = new PandocEngine($"{_store.AppConfig.ToolsPath}/pandoc.exe");        
            PandocInstance.SetPandocPath($"{_toolsPath}/pandoc.exe");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<NoteExtendedDto> IssueToNoteDto(string id)
    {
        try
        {
            NoteExtendedDto noteDto = null!;
            int priority = 0;                        

            if (_redMineManager == null)
                throw new ArgumentException("Manager cannot be null");

            var issue = _redMineManager.GetObject<Issue>(id, _parameters);

            if (issue == null)
                return null;

            #region  Issue basic data

            var filter = new NotesFilterDto();
            filter.SearchInDescription = false;
            if (issue.Tracker.Id == 20)
                filter.Tags = $"HU#{id}";
            else if (issue.Tracker.Id == 21)
                filter.Tags = $"PS#{id}";
            else if (issue.Tracker.Id == 23)
                filter.Tags = $"INC#{id}";

            var notes = (await Service.Notes.GetFilter(filter)).Entity;

            if (notes != null)
            {
                if (notes.Count > 0)
                    noteDto = (await Service.Notes.GetExtendedAsync(notes[0].NoteId)).Entity;
                else
                {
                    if (issue.Tracker.Id == 20)  // HU 
                    {
                        noteDto = (await Service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = Guid.Parse("4A3E0AE2-005D-44F0-8BF0-7E0D2A60F6C7") })).Entity;
                        LoadKAttributesTrackerHU(noteDto, issue);
                    }
                    else if (issue.Tracker.Id == 21)  // PS 
                    {
                        noteDto = (await Service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = Guid.Parse("BB4930BD-7065-4A41-8A9A-E5854EDE5024") })).Entity;
                        LoadKAttributesTrackerPS(noteDto, issue);
                    }
                    else if (issue.Tracker.Id == 23)  // INC
                    {
                        noteDto = (await Service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = Guid.Parse("63D8B399-D7F4-48D6-8CCE-228FEFEDE4F0") })).Entity;
                        LoadKAttributesTrackerINC(noteDto, issue);
                    }                    
                }
            }

            noteDto.Topic = issue.Subject;
            noteDto.ContentType = "navigation";
            noteDto.Description = issue.Description;
            var linkIssue = $"{Environment.NewLine}{Environment.NewLine}---{Environment.NewLine}{Environment.NewLine}[Redmine issue: #{id}](https://www3.gobiernodecanarias.org/aplicaciones/educacion/rm/issues/{id})";
            noteDto.Description += linkIssue;
            noteDto.Tags = filter.Tags;
            noteDto.CreationDateTime = issue.CreatedOn ?? DateTime.Now;
            noteDto.ModificationDateTime = issue.UpdatedOn ?? DateTime.Now;

            #endregion

            #region  Activity (Tasks/Actions) 

            foreach (var n in noteDto.Tasks)
                n.SetIsDeleted(true);
            
            var au = new NoteTaskDto();
            au.CreationDateTime = (DateTime)issue?.CreatedOn;
            au.ModificationDateTime = DateTime.Now;
            au.Priority = priority++;
            au.EndDate = issue?.CreatedOn;
            au.Resolved = issue?.CreatedOn != null ? true : false;            
            au.Description = $"Creación y redacción de la HU ({issue?.Author.Name}). ";
            au.UserId = await GetKNoteUserId(issue?.Author.Name);
            noteDto.Tasks.Add(au);

            if (issue?.AssignedTo != null)
            {
                au = new NoteTaskDto();
                au.CreationDateTime = (DateTime)issue?.CreatedOn;
                au.ModificationDateTime = DateTime.Now;
                au.Priority = priority++;
                au.EndDate = issue?.ClosedOn;
                au.Resolved = issue?.ClosedOn != null ? true : false; ;                
                au.Description = $"Tareas derivadas del seguimiento y supervisión de la HU ({issue?.AssignedTo.Name}).";
                au.UserId = await GetKNoteUserId(issue?.AssignedTo.Name);
                noteDto.Tasks.Add(au);
            }

            var annotations = issue?.Journals;
            if (annotations != null)
            {            
                foreach (var an in annotations)
                {
                    if (!string.IsNullOrEmpty(an.Notes))
                    {
                        var n = new NoteTaskDto();
                        n.CreationDateTime = (DateTime) an.CreatedOn;
                        n.ModificationDateTime = DateTime.Now;
                        n.Priority = priority++;
                        n.EndDate = an.CreatedOn;                        
                        n.Resolved = true;                        
                        n.Description = an.Notes;
                        n.UserId = await GetKNoteUserId(an.User.Name);
                        noteDto.Tasks.Add(n);
                    }
                }
            }

            #endregion

            #region  Attachments

            if (issue?.Attachments != null)
            {                
                int resOrder = 0;

                foreach (var atch in issue.Attachments)
                {
                    var existRes = false;

                    foreach (var res in noteDto.Resources)
                    {
                        if (res.Name.IndexOf(atch.FileName) > -1) 
                        { 
                            existRes = true;
                            res.Description = atch.Description;
                            res.Order = resOrder++;
                            res.Container = ($"{_service.RepositoryRef.ResourcesContainer}/{issue.CreatedOn.Value.Year.ToString()}").Replace('\\', '/');
                            res.ContentArrayBytes = _redMineManager.DownloadFile(atch.ContentUrl);
                            break;
                        }
                    }

                    if (!existRes)
                    {
                        var newRes = new ResourceDto();
                        newRes.ResourceId = Guid.NewGuid();
                        newRes.ContentInDB = false;
                        newRes.Name = $"{newRes.ResourceId}_{atch.FileName}";
                        newRes.FileType = ExtensionFileToFileType(Path.GetExtension(newRes.Name));
                        newRes.Description = atch.Description;
                        newRes.Order = resOrder++;
                        newRes.Container = ($"{_service.RepositoryRef.ResourcesContainer}/{issue.CreatedOn.Value.Year.ToString()}").Replace('\\', '/');
                        newRes.ContentArrayBytes = _redMineManager.DownloadFile(atch.ContentUrl);
                        noteDto.Resources.Add(newRes);
                    }

                    // TODO: Deleted resources need to be processed.
                }
            }

            #endregion

            #region  Relations
            
            // TODO: ...
            var relations = issue?.Relations;

            if (relations != null)
            {
                StringBuilder sbRelations = new StringBuilder();
                string strRelations = "";
                foreach (var rel in relations) 
                { 
                    if(rel.Type == IssueRelationType.Blocks)
                    {
                        sbRelations.Append($"BID#{rel.IssueId}-BTOID#{rel.IssueToId};");
                    }
                }
                strRelations = sbRelations.ToString() ?? "";
                if(sbRelations.Length > 5) 
                    noteDto.Tags += $"  [{strRelations.Substring(0,strRelations.Length - 1)}]";
            }

            #endregion

            #region Folder

            var f = await GetKNoteFolderDto(issue, noteDto);
            noteDto.FolderId = f.FolderId;
            noteDto.FolderDto = f;

            #endregion

            #region Personalize path resources

            foreach (var r in noteDto.Resources)
            {                               
                var org = $"!{r.NameOut}!";
                var dest = $"![alt text]({r.Container}/{r.Name})";

                noteDto.Description = noteDto.Description.Replace(org, dest, true, CultureInfo.CurrentCulture);

                foreach(var t in noteDto.Tasks)
                {
                    t.Description = t.Description.Replace(org, dest, true, CultureInfo.CurrentCulture);
                }
            }

            #endregion

            #region  Convert to markdown format

            // This pandoc code version has encoding issue ...
            //note.Description = await pandocEngine.ConvertToText<TextileIn, CommonMarkOut>(note.Description);                
            //note.Description = note.Description.Replace("\\[", "[");
            //note.Description = note.Description.Replace("\\]", "]");
            // ---

            // Use this for now.
            noteDto.Description = await TextToMarkdown(_toolsPath, noteDto.Description);

            foreach (var t in noteDto.Tasks)
            {
                t.Description = await TextToMarkdown(_toolsPath, t.Description);
            }

            #endregion 

            return noteDto;
        }
        catch (Exception)
        {            
            return null;
        }
    }

    public async Task<bool> IsValidService()
    {
        var value = await GetSystemPlugInVariable("PLUGIN_SUPPORT");

        if (value.ToLower() == "true")
            return true;
        else
            return false;
    }

    public async Task<string> GetSystemPlugInVariable(string variable)
    {
        if (_service == null)
            return "";
        
        var valueDto = await _service.SystemValues.GetAsync(new KeyValuePair<string, string> ("KNT_REDMINEEDUCA_PLUGIN", variable ));
        if (valueDto.IsValid)
            return valueDto.Entity.Value;
        else
            return "";
    }

    public async Task<string> SaveSystemPlugInVariable(string variable, string value)
    {        
        if (_service == null)
            return "";

        var valueDto = await _service.SystemValues.GetAsync(new KeyValuePair<string, string>("KNT_REDMINEEDUCA_PLUGIN", variable));
        if (valueDto.IsValid)
        {
            valueDto.Entity.Value = value;
            var resSave = await _service.SystemValues.SaveAsync(valueDto.Entity);
            if (resSave.IsValid)
                return resSave.Entity.Value;
            else
                return "";
        }
        else
            return "";
    }

    #endregion

    #region Private Methods

    private async Task<FolderDto> GetKNoteFolderDto(Issue issue, NoteExtendedDto noteDto)
    {
        string folderName = "";

        if (issue.Tracker.Id == 20)
            folderName = noteDto.KAttributesDto[3].Value;
        else if (issue.Tracker.Id == 21)
            folderName = $"Peticiones de servicio {(noteDto.KAttributesDto[2].Value).Substring(0, 5)}";
        else if (issue.Tracker.Id == 23)
            folderName = $"Incidencias {(noteDto.KAttributesDto[2].Value).Substring(0, 5)}";

        if (_cacheFolders.TryGetValue(folderName, out FolderDto folderDto))
        {
            return folderDto;
        }
        else
        {
            FolderDto newFolderDto;
            var folder = _folders.FirstOrDefault(f => f.Name == folderName);

            if (folder != null)
            {
                newFolderDto = folder.GetSimpleDto<FolderDto>();
            }
            else
            {
                FolderDto newFolder = new FolderDto
                {
                    Name = folderName,
                    ParentId = _parentFolder.FolderId
                };

                var resSave = await _service.Folders.SaveAsync(newFolder);

                if (resSave.IsValid)
                    newFolderDto = resSave.Entity;                
                else
                    newFolderDto = _parentFolder;                 
            }
            _cacheFolders.Add(folderName, newFolderDto);
            return newFolderDto;
        }       
    }

    private async Task<Guid> GetKNoteUserId(string name)
    {
        string fullName = name;
        if(name.Length > 32)
            name = name.Substring(0, 32);

        if (_cacheUsers.TryGetValue(name, out Guid userId))
        {
            return userId;
        }
        else
        {
            Guid newUserId;
            var resGetUser = await _service.Users.GetByUserNameAsync(name);
            if (!resGetUser.IsValid)
            {
                var resNewUser = await _service.Users.CreateAsync(
                    new UserRegisterDto 
                        { UserId = userId ,
                         Disabled = false ,
                         EMail = $"{fullName}@{fullName}.org", 
                         FullName = fullName,
                         Password = GetRandomString(10), 
                         RoleDefinition = "Public",
                         UserName = name 
                    });
                if (resNewUser.IsValid)
                {
                    newUserId = resNewUser.Entity.UserId;
                }
                else
                {
                    throw new Exception("Can't create new user");
                }
            }
            else
            {
                newUserId = resGetUser.Entity.UserId;
            }
            _cacheUsers.Add(name, newUserId);
            return newUserId;
        }
    }

    private void LoadKAttributesTrackerHU(NoteExtendedDto noteDto, Issue issue)
    {
        if (noteDto.KAttributesDto.Count < 15)
            return;

        var customFields = issue?.CustomFields;        

        noteDto.KAttributesDto[0].Value = issue?.Author.Name;                     // Añadido por 
        if (issue?.AssignedTo != null)                                            // Asignado a 
            noteDto.KAttributesDto[1].Value = issue?.AssignedTo.Name;
        noteDto.KAttributesDto[2].Value = issue?.Project.Name;                    // Proyecto
        // ..
        noteDto.KAttributesDto[4].Value = issue?.Priority.Name;                   // Prioridad
        noteDto.KAttributesDto[5].Value = issue?.Status.Name;                     // Estado
        noteDto.KAttributesDto[6].Value = issue?.TotalEstimatedHours.ToString();  // Tiempo estimado
        noteDto.KAttributesDto[7].Value = issue?.TotalSpentHours.ToString();      // Tiempo dedicado
        noteDto.KAttributesDto[8].Value = issue?.CreatedOn.ToString();            // Fecha creación
        noteDto.KAttributesDto[9].Value = issue?.UpdatedOn.ToString();            // Fecha modificación
        noteDto.KAttributesDto[10].Value = issue?.DueDate.ToString();             // Fecha vencimiento
        noteDto.KAttributesDto[11].Value = issue?.StartDate.ToString();           // Fecha de inicio
        noteDto.KAttributesDto[12].Value = issue?.ClosedOn.ToString();            // Fecha fin
        noteDto.KAttributesDto[13].Value = issue?.DoneRatio.ToString();           // Porcentaje realizado
        noteDto.KAttributesDto[14].Value = issue?.FixedVersion?.Name;             // Versión prevista 

        // TODO: hack for extract folder name.
        if (customFields != null && (noteDto.KAttributesDto.Count > 19))
        {
            if (customFields[0]?.Values != null)  // Gestión 
                noteDto.KAttributesDto[3].Value = customFields[0]?.Values[0]?.Info?.ToString();
            if (customFields[1]?.Values != null)  // Expediente de contratación 
                noteDto.KAttributesDto[15].Value = customFields[1]?.Values[0]?.Info?.ToString();
            if (customFields[2]?.Values != null)   // Motivo de la no penalización
                noteDto.KAttributesDto[16].Value = customFields[2]?.Values[0]?.Info?.ToString();
            if (customFields[3]?.Values != null)   // Solución adoptada
                noteDto.KAttributesDto[17].Value = customFields[3]?.Values[0]?.Info?.ToString();
            if (customFields[4]?.Values != null)   // Código fuente
                noteDto.KAttributesDto[18].Value = customFields[4]?.Values[0]?.Info?.ToString();
            if (customFields[5]?.Values != null)   // Observaciones 
                noteDto.KAttributesDto[19].Value = customFields[5]?.Values[0]?.Info?.ToString();            
        }
    }

    private void LoadKAttributesTrackerPS(NoteExtendedDto noteDto, Issue issue)
    {
        if (noteDto.KAttributesDto.Count < 14)
            return;

        var customFields = issue?.CustomFields;

        noteDto.KAttributesDto[0].Value = issue?.Author.Name;                     // Añadido por 
        if (issue?.AssignedTo != null)                                            // Asignado a 
            noteDto.KAttributesDto[1].Value = issue?.AssignedTo.Name;
        noteDto.KAttributesDto[2].Value = issue?.Project.Name;                    // Proyecto        
        noteDto.KAttributesDto[3].Value = issue?.Priority.Name;                   // Prioridad
        noteDto.KAttributesDto[4].Value = issue?.Status.Name;                     // Estado
        noteDto.KAttributesDto[5].Value = issue?.TotalEstimatedHours.ToString();  // Tiempo estimado
        noteDto.KAttributesDto[6].Value = issue?.TotalSpentHours.ToString();      // Tiempo dedicado
        noteDto.KAttributesDto[7].Value = issue?.CreatedOn.ToString();            // Fecha creación
        noteDto.KAttributesDto[8].Value = issue?.UpdatedOn.ToString();            // Fecha modificación
        noteDto.KAttributesDto[9].Value = issue?.DueDate.ToString();              // Fecha vencimiento
        noteDto.KAttributesDto[10].Value = issue?.StartDate.ToString();           // Fecha de inicio
        noteDto.KAttributesDto[11].Value = issue?.ClosedOn.ToString();            // Fecha fin
        noteDto.KAttributesDto[12].Value = issue?.DoneRatio.ToString();           // Porcentaje realizado
        noteDto.KAttributesDto[13].Value = issue?.FixedVersion?.Name;             // Versión prevista

        // TODO: hack for extract folder name.
        if (customFields != null && (noteDto.KAttributesDto.Count > 32))
        {
            if (customFields[0]?.Values != null)   // Código de usuario
                noteDto.KAttributesDto[14].Value = customFields[0]?.Values[0]?.Info?.ToString();
            if (customFields[1]?.Values != null)   // Nombre y apellidos
                noteDto.KAttributesDto[15].Value = customFields[1]?.Values[0]?.Info?.ToString();
            if (customFields[2]?.Values != null)   // Correo electrónico
                noteDto.KAttributesDto[16].Value = customFields[2]?.Values[0]?.Info?.ToString();
            if (customFields[3]?.Values != null)   // Servicio
                noteDto.KAttributesDto[17].Value = customFields[3]?.Values[0]?.Info?.ToString();
            if (customFields[4]?.Values != null)   // Cargo
                noteDto.KAttributesDto[18].Value = customFields[4]?.Values[0]?.Info?.ToString();
            if (customFields[5]?.Values != null)   // Centro educativo
                noteDto.KAttributesDto[19].Value = customFields[5]?.Values[0]?.Info?.ToString();
            if (customFields[6]?.Values != null)   // Teléfono(s) de contacto
                noteDto.KAttributesDto[20].Value = customFields[6]?.Values[0]?.Info?.ToString();
            if (customFields[7]?.Values != null)   // Centro directivo
                noteDto.KAttributesDto[21].Value = customFields[7]?.Values[0]?.Info?.ToString();
            if (customFields[8]?.Values != null)   // Agrupación funcional
                noteDto.KAttributesDto[22].Value = customFields[8]?.Values[0]?.Info?.ToString();
            if (customFields[9]?.Values != null)   // Prioridad funcional
                noteDto.KAttributesDto[23].Value = customFields[9]?.Values[0]?.Info?.ToString();
            if (customFields[10]?.Values != null)   // Nº de ticket externo (OTRS)
                noteDto.KAttributesDto[24].Value = customFields[10]?.Values[0]?.Info?.ToString();
            if (customFields[11]?.Values != null)   // Nº de ticket externo (OTRS-ID)
                noteDto.KAttributesDto[25].Value = customFields[11]?.Values[0]?.Info?.ToString();
            if (customFields[12]?.Values != null)   // Gestión / Proceso
                noteDto.KAttributesDto[26].Value = customFields[12]?.Values[0]?.Info?.ToString();
            if (customFields[13]?.Values != null)   // Aplicación
                noteDto.KAttributesDto[27].Value = customFields[13]?.Values[0]?.Info?.ToString();
            if (customFields[14]?.Values != null)   // Fecha límite
                noteDto.KAttributesDto[28].Value = customFields[14]?.Values[0]?.Info?.ToString();
            if (customFields[15]?.Values != null)   // Comunicación a sistema externo
                noteDto.KAttributesDto[29].Value = customFields[15]?.Values[0]?.Info?.ToString();
            if (customFields[16]?.Values != null)   // Solución adoptada
                noteDto.KAttributesDto[30].Value = customFields[16]?.Values[0]?.Info?.ToString();
            if (customFields[17]?.Values != null)   // Comunicación al solicitante
                noteDto.KAttributesDto[31].Value = customFields[17]?.Values[0]?.Info?.ToString();
            if (customFields[18]?.Values != null)   // Observaciones
                noteDto.KAttributesDto[32].Value = customFields[18]?.Values[0]?.Info?.ToString();
        }
    }

    private void LoadKAttributesTrackerINC(NoteExtendedDto noteDto, Issue issue)
    {
        if (noteDto.KAttributesDto.Count < 14)
            return;

        var customFields = issue?.CustomFields;

        noteDto.KAttributesDto[0].Value = issue?.Author.Name;                     // Añadido por 
        if (issue?.AssignedTo != null)                                            // Asignado a 
            noteDto.KAttributesDto[1].Value = issue?.AssignedTo.Name;
        noteDto.KAttributesDto[2].Value = issue?.Project.Name;                    // Proyecto        
        noteDto.KAttributesDto[3].Value = issue?.Priority.Name;                   // Prioridad
        noteDto.KAttributesDto[4].Value = issue?.Status.Name;                     // Estado
        noteDto.KAttributesDto[5].Value = issue?.TotalEstimatedHours.ToString();  // Tiempo estimado
        noteDto.KAttributesDto[6].Value = issue?.TotalSpentHours.ToString();      // Tiempo dedicado
        noteDto.KAttributesDto[7].Value = issue?.CreatedOn.ToString();            // Fecha creación
        noteDto.KAttributesDto[8].Value = issue?.UpdatedOn.ToString();            // Fecha modificación
        noteDto.KAttributesDto[9].Value = issue?.DueDate.ToString();              // Fecha vencimiento
        noteDto.KAttributesDto[10].Value = issue?.StartDate.ToString();           // Fecha de inicio
        noteDto.KAttributesDto[11].Value = issue?.ClosedOn.ToString();            // Fecha fin
        noteDto.KAttributesDto[12].Value = issue?.DoneRatio.ToString();           // Porcentaje realizado
        noteDto.KAttributesDto[13].Value = issue?.FixedVersion?.Name;             // Versión prevista

        // TODO: hack for extract folder name.
        if (customFields != null && (noteDto.KAttributesDto.Count > 35))
        {
            if (customFields[0]?.Values != null)   // Código de usuario
                noteDto.KAttributesDto[14].Value = customFields[0]?.Values[0]?.Info?.ToString();
            if (customFields[1]?.Values != null)   // Nombre y apellidos
                noteDto.KAttributesDto[15].Value = customFields[1]?.Values[0]?.Info?.ToString();
            if (customFields[2]?.Values != null)   // Correo electrónico
                noteDto.KAttributesDto[16].Value = customFields[2]?.Values[0]?.Info?.ToString();
            if (customFields[3]?.Values != null)   // Servicio
                noteDto.KAttributesDto[17].Value = customFields[3]?.Values[0]?.Info?.ToString();
            if (customFields[4]?.Values != null)   // Cargo
                noteDto.KAttributesDto[18].Value = customFields[4]?.Values[0]?.Info?.ToString();
            if (customFields[5]?.Values != null)   // Centro educativo
                noteDto.KAttributesDto[19].Value = customFields[5]?.Values[0]?.Info?.ToString();
            if (customFields[6]?.Values != null)   // Teléfono(s) de contacto
                noteDto.KAttributesDto[20].Value = customFields[6]?.Values[0]?.Info?.ToString();
            if (customFields[7]?.Values != null)   // Centro directivo
                noteDto.KAttributesDto[21].Value = customFields[7]?.Values[0]?.Info?.ToString();
            if (customFields[8]?.Values != null)   // Agrupación funcional
                noteDto.KAttributesDto[22].Value = customFields[8]?.Values[0]?.Info?.ToString();
            if (customFields[9]?.Values != null)   // Nº de ticket externo (OTRS)
                noteDto.KAttributesDto[23].Value = customFields[9]?.Values[0]?.Info?.ToString();
            if (customFields[10]?.Values != null)   // Nº de ticket externo (OTRS-ID)
                noteDto.KAttributesDto[24].Value = customFields[10]?.Values[0]?.Info?.ToString();            
            if (customFields[11]?.Values != null)   // Referencia origen del problema
                noteDto.KAttributesDto[25].Value = customFields[11]?.Values[0]?.Info?.ToString();
            if (customFields[12]?.Values != null)   // Gestión/Proceso
                noteDto.KAttributesDto[26].Value = customFields[12]?.Values[0]?.Info?.ToString();
            if (customFields[13]?.Values != null)   // Aplicación
                noteDto.KAttributesDto[27].Value = customFields[13]?.Values[0]?.Info?.ToString();
            if (customFields[14]?.Values != null)   // Fecha límite
                noteDto.KAttributesDto[28].Value = customFields[14]?.Values[0]?.Info?.ToString();
            if (customFields[15]?.Values != null)   // Comunicación a sistema externo
                noteDto.KAttributesDto[29].Value = customFields[15]?.Values[0]?.Info?.ToString();
            if (customFields[16]?.Values != null)   // Solución adoptada
                noteDto.KAttributesDto[30].Value = customFields[16]?.Values[0]?.Info?.ToString();
            if (customFields[17]?.Values != null)   // Comunicación al solicitante
                noteDto.KAttributesDto[31].Value = customFields[17]?.Values[0]?.Info?.ToString();
            if (customFields[18]?.Values != null)   // ANS_Man_TResp_Pdte
                noteDto.KAttributesDto[32].Value = customFields[18]?.Values[0]?.Info?.ToString();
            if (customFields[19]?.Values != null)   // ANS_Man_TResp_Fin
                noteDto.KAttributesDto[33].Value = customFields[19]?.Values[0]?.Info?.ToString();
            if (customFields[20]?.Values != null)   // ANS_Man_TResol_Pdte
                noteDto.KAttributesDto[34].Value = customFields[20]?.Values[0]?.Info?.ToString();
            if (customFields[21]?.Values != null)   // ANS_Man_TResol_Fin
                noteDto.KAttributesDto[35].Value = customFields[21]?.Values[0]?.Info?.ToString();
        }
    }

    private async Task<string> TextToMarkdown(string pathUtils, string text)
    {
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

    private string ExtensionFileToFileType(string extension)
    {
        // TODO: Refactor this method

        var ext = extension.ToLower();

        if (ext == ".jpg")
            return @"image/jpeg";
        else if (ext == ".jpeg")
            return @"image/jpeg";
        else if (ext == ".png")
            return "image/png";
        else if (ext == ".pdf")
            return "application/pdf";
        else if (ext == ".mp4")
            return "video/mp4";
        else if (ext == ".mp3")
            return "audio/mp3";
        else if (ext == ".txt")
            return "text/plain";
        else if (ext == ".text")
            return "text/plain";
        else if (ext == ".htm")
            return "text/plain";
        else if (ext == ".html")
            return "text/plain";
        else
            return "";
    }

    private string GetRandomString(int size)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder();
        Random random = new Random();

        for (int i = 0; i < size; i++)
        {
            int indice = random.Next(chars.Length);
            result.Append(chars[indice]);
        }

        return result.ToString();
    }

    #endregion 
}