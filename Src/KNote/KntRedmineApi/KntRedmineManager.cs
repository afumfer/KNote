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

namespace KntRedmineApi;

public class KntRedmineManager
{
    private RedmineManager _manager;
    private NameValueCollection _parameters = new () { { "include", "attachments,relations,journals" } };
    
    private List<FolderInfoDto> _folders = new();
    private FolderDto _parentFolder = new();

    public KntRedmineManager(IKntService service)
    {
        _service = service;
    }

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

            //_manager = new RedmineManager(_host, _apiKey);

            _folders = (await _service.Folders.GetAllAsync()).Entity;

            //int rootFolNum = 1;
            //if (!string.IsNullOrEmpty(_rootFolderForImport))
            //    int.TryParse(_rootFolderForImport, out rootFolNum);
            //_parentFolder = (await _service.Folders.GetAsync(rootFolNum)).Entity;

            //var pandocEngine = new PandocEngine($"{_store.AppConfig.ToolsPath}/pandoc.exe");        
            PandocInstance.SetPandocPath($"{_toolsPath}/pandoc.exe");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> IssueToNoteDto(string id, NoteExtendedDto noteDto, bool loadAttachments = true)
    {
        try
        {
            if (noteDto == null)
                throw new ArgumentException("Note and Manager cannot be null");

            _manager = new RedmineManager(_host, _apiKey);

            int rootFolNum = 1;
            if (!string.IsNullOrEmpty(_rootFolderForImport))
                int.TryParse(_rootFolderForImport, out rootFolNum);
            _parentFolder = (await _service.Folders.GetAsync(rootFolNum)).Entity;

            var issue = _manager.GetObject<Issue>(id, _parameters);

            if (issue == null)
                return false;

            noteDto.Topic = issue.Subject;                        
            noteDto.Tags = $"HU#{issue.Id}";
            noteDto.ContentType = "markdown";
            noteDto.Description = issue.Description;
            var customFields = issue?.CustomFields;

            if(noteDto.KAttributesDto.Count > 0)
            {
                // TODO: hack for extract folder name.
                if (customFields != null)
                {
                    if (customFields[0]?.Values != null)
                        noteDto.KAttributesDto[2].Value = customFields[0]?.Values[0]?.Info?.ToString();
                    if (customFields[1]?.Values != null)
                        noteDto.KAttributesDto[5].Value = customFields[1]?.Values[0]?.Info?.ToString();
                    if(customFields[2]?.Values != null)
                        noteDto.KAttributesDto[14].Value = customFields[2]?.Values[0]?.Info?.ToString();
                    if (customFields[3]?.Values != null)
                        noteDto.KAttributesDto[15].Value = customFields[3]?.Values[0]?.Info?.ToString();
                }
            
                noteDto.KAttributesDto[0].Value = issue?.Author.Name;
                noteDto.KAttributesDto[1].Value = issue?.Project.Name;
                noteDto.KAttributesDto[3].Value = issue?.Priority.Name;
                noteDto.KAttributesDto[4].Value = issue?.Status.Name;
                noteDto.KAttributesDto[6].Value = issue?.TotalEstimatedHours.ToString();
                noteDto.KAttributesDto[7].Value = issue?.TotalSpentHours.ToString();
                noteDto.KAttributesDto[8].Value = issue?.CreatedOn.ToString();
                noteDto.KAttributesDto[9].Value = issue?.UpdatedOn.ToString();
                noteDto.KAttributesDto[10].Value = issue?.DueDate.ToString();
                noteDto.KAttributesDto[11].Value = issue?.StartDate.ToString();
                noteDto.KAttributesDto[12].Value = issue?.ClosedOn.ToString();
                noteDto.KAttributesDto[13].Value = issue?.DoneRatio.ToString();
                noteDto.KAttributesDto[16].Value = issue?.FixedVersion?.Name;
            }

            var annotations = issue?.Journals;
            if (annotations != null)
            {
                var nl = $"{Environment.NewLine} ";
                var annString = nl;
                foreach (var an in annotations)
                {
                    if (!string.IsNullOrEmpty(an.Notes))
                    {
                        annString += $"User: {an.User?.Name} {nl}";
                        annString += $"Created on: {an.CreatedOn}{nl}{nl}";
                        annString += $"{an.Notes}  {nl} {nl}";
                        annString += $"----------------------------------{nl}{nl}";
                    }
                }

                var notes = $"{nl}{nl}{nl}{nl}{nl}///////////////////////////////////{nl}";
                notes += $"  NOTES: {nl}";
                notes += $"///////////////////////////////////{nl}";
                notes += annString;
                noteDto.Description += notes;
            }

            if (issue?.Attachments != null && loadAttachments)
            {
                foreach (var atch in issue.Attachments)
                {
                    var findRes = noteDto.Resources.FirstOrDefault(r => r.Name.IndexOf(atch.FileName)>-1);

                    if (findRes == null)
                    {
                        findRes = new ResourceDto();
                        findRes.ResourceId = Guid.NewGuid();                        
                        findRes.ContentInDB = false;
                        findRes.Name = $"{findRes.ResourceId}_{atch.FileName}";
                        findRes.Description = atch.Description;
                        findRes.Order = 0;
                        findRes.ContentArrayBytes = _manager.DownloadFile(atch.ContentUrl);                    
                        noteDto.Resources.Add(findRes);                        
                    }
                }
            }
            
            noteDto.Tags = $"HU#{id}";
            var folderName = noteDto.KAttributesDto[2].Value;

            foreach (var r in noteDto.Resources)
            {
                r.FileType = ExtensionFileToFileType(Path.GetExtension(r.Name));
            }

            var folder = _folders.FirstOrDefault(f => f.Name == folderName);

            if (folder != null)
            {
                noteDto.FolderId = folder.FolderId;
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
                {
                    _folders = (await _service.Folders.GetAllAsync()).Entity;
                    noteDto.FolderId = resSave.Entity.FolderId;
                }
                else
                    noteDto.FolderId = _parentFolder.FolderId;
            }

            foreach (var r in noteDto.Resources)
            {
                if (string.IsNullOrEmpty(r.Container))
                {
                    r.Container = $"{_service.RepositoryRef.ResourcesContainer}/{DateTime.Now.Year.ToString()}";
                }

                r.Container = r.Container.Replace('\\', '/');

                var org = $"!{r.NameOut}!";
                var dest = $"![alt text]({r.Container}/{r.Name})";

                noteDto.Description = noteDto.Description.Replace(org, dest, true, CultureInfo.CurrentCulture);
            }

            noteDto.Description = await TextToMarkdown(_toolsPath, noteDto.Description);

            // This pandoc code version has encoding issue ...
            //note.Description = await pandocEngine.ConvertToText<TextileIn, CommonMarkOut>(note.Description);                
            //note.Description = note.Description.Replace("\\[", "[");
            //note.Description = note.Description.Replace("\\]", "]");

            return true;
        }
        catch (Exception)
        {            
            return false;
        }
    }

    public string PredictPH(string gestion, string tema, string descripcion)
    {        
        RedMinePunHis.ModelInput dataInput = new RedMinePunHis.ModelInput()
        {
            Gestion = gestion,
            Tema = tema,
            Descripcion = descripcion
        };

        var predictionResult = RedMinePunHis.Predict(dataInput);

        return predictionResult.PredictedLabel;
    }

    public string PredictGestion(string tema, string descripcion)
    {
        RedMineGestion.ModelInput dataInput = new RedMineGestion.ModelInput()
        {            
            Tema = tema,
            Descripcion = descripcion
        };

        var predictionResult = RedMineGestion.Predict(dataInput);

        return predictionResult.PredictedLabel;
    }

    public async Task<string> TextToMarkdown(string pathUtils, string text)
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

    public string ExtensionFileToFileType(string extension)
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

        var valueDto = await _service.SystemValues.GetAsync("KNT_REDMINEEDUCA_PLUGIN", variable);
        if (valueDto.IsValid)
            return valueDto.Entity.Value;
        else
            return "";
    }

    public async Task<string> SaveSystemPlugInVariable(string variable, string value)
    {        
        if (_service == null)
            return "";

        var valueDto = await _service.SystemValues.GetAsync("KNT_REDMINEEDUCA_PLUGIN", variable);
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

}