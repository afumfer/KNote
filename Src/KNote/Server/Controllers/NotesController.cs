using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using KNote.Server.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;
using KNote.Repository.EntityFramework.Entities;

namespace KNote.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly IKntService _service;
    private readonly IFileStore _fileStore;
    private readonly ILogger<NotesController> _logger;

    public NotesController(IKntService service, IFileStore fileStore, IHttpContextAccessor httpContextAccessor, ILogger<NotesController> logger)
    {
        _service = service;            
        _service.Logger = logger;
        _fileStore = fileStore;
        _logger = logger;

        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;

        if (string.IsNullOrEmpty(_service.RepositoryRef.ResourcesContainerRootPath))
            _service.RepositoryRef.ResourcesContainerRootPath = _fileStore.GetResourcesContainerRootPath();
        if (string.IsNullOrEmpty(_service.RepositoryRef.ResourcesContainerRootUrl))
            _service.RepositoryRef.ResourcesContainerRootUrl = _fileStore.GetResourcesContainerRootUrl();
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            _logger.LogTrace("GetAll {dateTime}.", DateTime.Now);

            var resApi = await _service.Notes.GetAllAsync();
            if (resApi.IsValid)
            {
                // Hack, this is temporary, resolve resources in virtual directories in the WebAPI.
                ListNotesUpdateResourceInDescriptionForRead(resApi.Entity, _service.RepositoryRef); 
                // ----
                return Ok(resApi);
            }
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAll at {dateTime}.", DateTime.Now);
            var resApi = new Result<List<NoteInfoDto>>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    [HttpPost("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]        
    public async Task <IActionResult> Filter([FromBody] NotesFilterDto notesFilter)
    {
        try
        {
            _logger.LogTrace("Filter {dateTime}.", DateTime.Now);

            var resApi = await _service.Notes.GetFilterMinimalAsync(notesFilter);
                            
            if (resApi.IsValid)            
                return Ok(resApi);            
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Filter at {dateTime}.", DateTime.Now);
            var kresApi = new Result<List<NoteInfoDto>>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }
            
    [HttpGet("[action]")]
    public async Task<IActionResult> Search([FromQuery] NotesSearchParam notesSearchParam)
    {
        try
        {
            _logger.LogTrace("Search {dateTime}.", DateTime.Now);
            
            var notesSearch = new NotesSearchDto();
            notesSearch.TextSearch = notesSearchParam.TextSearch;
            if(notesSearchParam.PageNumber > 0)
                notesSearch.PageIdentifier.PageNumber = notesSearchParam.PageNumber;
            notesSearch.PageIdentifier.PageSize = notesSearchParam.PageSize;
            notesSearch.OrderColumn = notesSearchParam.OrderColumn;
            notesSearch.OrderAscending = notesSearchParam.OrderAscending;
            // .....

            var resApi = await _service.Notes.GetSearchMinimalAsync(notesSearch);
            
            if (resApi.IsValid)            
                return Ok(resApi);            
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search at {dateTime}.", DateTime.Now);
            var kresApi = new Result<List<NoteInfoDto>>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    // The HTML of a note's description (Markdown with HTML of its own) made safe to show: the web app shows notes
    // with it (Shared/ViewMarkdown), so that both apps use the same rules (KntHtmlSanitizer, in Service).
    [HttpPost("[action]")]
    public IActionResult Html([FromBody] string markdown)
    {
        try
        {
            return Ok(new Result<string>(_service.Notes.UtilMarkdownToHtml(markdown)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Html at {dateTime}.", DateTime.Now);
            var resApi = new Result<string>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // folderId: one of the folders directly inside Home (see FoldersController.HomeChildren) instead of Home.
    [AllowAnonymous]
    [HttpGet("[action]")]
    public async Task<IActionResult> HomeNotes([FromQuery] Guid? folderId)
    {
        try
        {
            _logger.LogTrace("HomeNotes {dateTime}.", DateTime.Now);

            var resApi = folderId == null
                ? await _service.Notes.HomeNotesAsync()
                : await _service.Notes.HomeNotesAsync(folderId.Value);
            if (resApi.IsValid)
            {
                // Hack, this is temporary to resolve resources in virtual directories in the WebAPI.
                ListNotesUpdateResourceInDescriptionForRead(resApi.Entity, _service.RepositoryRef);
                // ----
                // Home is shown without signing in, where api/notes/html can't be used: the descriptions go as
                // HTML made safe (Shared/ViewMarkdown's SafeHtml in Client).
                foreach (var note in resApi.Entity)
                    note.Description = _service.Notes.UtilMarkdownToHtml(note.Description);
                return Ok(resApi);
            }
            else
                return BadRequest(resApi);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HomeNotes at {dateTime}.", DateTime.Now);
            var resApi = new Result<List<NoteInfoDto>>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            _logger.LogTrace("Get id {id} at {dateTime}.", id, DateTime.Now);

            var resApi = await _service.Notes.GetAsync(id);
            if (resApi.IsValid)
            {
                // Hack, this is temporary to resolve resources in virtual directories in the WebAPI.
                resApi.Entity.Description = _service.Notes.UtilUpdateResourceInDescriptionForRead(resApi.Entity.Description); 
                // ---
                return Ok(resApi);
            }
            else
            {
                return BadRequest(resApi);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get {id} at {dateTime}.", id, DateTime.Now);
            var kresApi = new Result<NoteInfoDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpGet("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> New()
    {
        try
        {
            _logger.LogTrace("New {dateTime}.", DateTime.Now);

            var resApi = await _service.Notes.NewAsync();
            if (resApi.IsValid)
                return Ok(resApi);
            else
            {
                return BadRequest(resApi);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "New at {dateTime}.", DateTime.Now);
            var kresApi = new Result<NoteInfoDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpPost]
    [HttpPut]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> Post([FromBody]NoteDto note)
    {            
        try
        {
            _logger.LogTrace("Post/Put {topic} at {dateTime}.", note.Topic?.ToString(), DateTime.Now);
            
            // ContentType (as ClientWin keeps it): a JSON with the description's view in ClientWin ("des"), the
            // script's language ("scr") and whether the description is locked ("blk"). A note in the old plain format
            // ("markdown", "navigation#"...) is read into it (GetContentTypeExt) and always saved in the JSON one;
            // only the view is decided here, the rest is kept as it is.
            var ct = note.GetContentTypeExt();

            // Hack to make it compatible with the desktop application. ----------------------------------
            note.Description = _service.Notes.UtilUpdateResourceInDescriptionForWrite(note.Description);
            if(note.Description != null)
            {
                if (note.Description.StartsWith(@"<BODY"))
                    ct.ForDescription = "html";
                else
                {                
                    // Line breaks as ClientWin writes them (CR LF), whatever the browser or the editor sent: LF, CR LF or
                    // CR CR LF (a CR LF made CR CR LF by a former version of this line).
                    note.Description = Regex.Replace(note.Description, @"\r*\n|\r", "\r\n");
                    // Markdown, shown as text ("markdown") or rendered ("navigation"), as the user left it in ClientWin.
                    if (ct.ForDescription != "navigation")
                        ct.ForDescription = "markdown";
                }
            }
            if (string.IsNullOrEmpty(ct.ForDescription))
                ct.ForDescription = "markdown";
            note.SetContentTypeExt(ct);
            // -------------------------------------------------------------------------------------------

            var resApi = await _service.Notes.SaveAsync(note);
            if (resApi.IsValid)
            {      
                resApi.Entity.Description = _service.Notes.UtilUpdateResourceInDescriptionForRead(resApi.Entity.Description);
                return Ok(resApi);
            }

            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post/Put {topic} at {dateTime}.", note.Topic?.ToString(), DateTime.Now);
            var kresApi = new Result<NoteInfoDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            _logger.LogTrace("Delete {id} at {dateTime}.", id, DateTime.Now);

            var resApi = await _service.Notes.DeleteExtendedAsync(id);
            if (resApi.IsValid)
                return Ok(resApi);
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete {id} at {dateTime}.", id, DateTime.Now);
            var kresApi = new Result<NoteInfoDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpPost("[action]")]
    [HttpPut("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> Resources([FromBody]ResourceInfoDto entity)
    {
        try
        {
            _logger.LogTrace("Resources(post/put) {dateTime}.", DateTime.Now);

            entity.Name = _service.ReplaceSpecialCharacters(entity.Name);

            var resApi = await _service.Notes.SaveResourceAsync(entity);
            if (resApi.IsValid)                                    
                return Ok(resApi);                                                              
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resources(post/put) at {dateTime}.", DateTime.Now);
            var kresApi = new Result<ResourceDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpGet("{id}/[action]")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> Resources(Guid id)
    {
        try
        {
            _logger.LogTrace("Resources {id} at {dateTime}.", id, DateTime.Now);

            var resApi = await _service.Notes.GetResourcesInfoAsync(id);
            if (resApi.IsValid)                
                return Ok(resApi);                               
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resources {id} at {dateTime}.", id, DateTime.Now);
            var kresApi = new Result<List<ResourceDto>>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpDelete("resources/{id}")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> DeleteResources(Guid id)
    {
        try
        {
            _logger.LogTrace("DeleteResources {if} at {dateTime}.", id, DateTime.Now);

            var resApi = await _service.Notes.DeleteResourceInfoAsync(id);
            if (resApi.IsValid)                
                return Ok(resApi);                
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteResources {id} at {dateTime}.", id, DateTime.Now);
            var kresApi = new Result<ResourceDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    // TODO: ###  deprecated... 
    [HttpPost("[action]")]
    [HttpPut("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> SaveFile(ResourceDto resource)
    {
        Result<ResourceDto> resApi = new Result<ResourceDto>();
        try
        {
            _logger.LogTrace("SaveFile {dateTime}.", DateTime.Now);

            if (!string.IsNullOrWhiteSpace(resource.ContentBase64))
            {
                resource.Name = _service.ReplaceSpecialCharacters(resource.Name);
                resource.FullUrl = await _fileStore.SaveFile(resource.ContentBase64, resource.Name, resource.Container);
            }
            resApi.Entity = resource;
            return Ok(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveFile at {dateTime}.", DateTime.Now);
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    [HttpPost("[action]")]
    [HttpPut("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> Tasks([FromBody]NoteTaskDto entity)
    {            
        try
        {
            _logger.LogTrace("Tasks(post/put) {dateTime}.", DateTime.Now);

            // As a note's description (Post): its resources as stored, and its lines as ClientWin writes them.
            entity.Description = _service.Notes.UtilUpdateResourceInDescriptionForWrite(entity.Description);
            if (entity.Description != null)
                entity.Description = Regex.Replace(entity.Description, @"\r*\n|\r", "\r\n");

            var resApi = await _service.Notes.SaveNoteTaskAsync(entity);
            if (resApi.IsValid)
            {                    
                return Ok(resApi);
            }
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tasks at {dateTime}.", DateTime.Now);
            var kresApi = new Result<NoteTaskDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpGet("{id}/[action]")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> Tasks(Guid id)
    {
        try
        {
            _logger.LogTrace("Tasks {id} {dateTime}.", id, DateTime.Now);

            var resApi = await _service.Notes.GetNoteTasksAsync(id);
            if (resApi.IsValid)                
            {                    
                TasksUpdateResourceInDescriptionForRead(resApi.Entity);
                return Ok(resApi);
            }
            else
                return BadRequest(resApi);                
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tasks {id} at {dateTime}.", id, DateTime.Now);
            var kresApi = new Result<List<NoteTaskDto>>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpGet("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> GetStartedTasksByDateTimeRage([FromQuery] string start, [FromQuery] string end)
    {
        try
        {
            _logger.LogTrace("GetStartedTasksByDateTimeRage {dateTime}.", DateTime.Now);

            var satartDateTime = DateTime.ParseExact(start, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var endDateTime = DateTime.ParseExact(end, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

            var resApi = await _service.Notes.GetStartedTasksByDateTimeRageAsync(satartDateTime, endDateTime);
            if (resApi.IsValid)
            {
                TasksUpdateResourceInDescriptionForRead(resApi.Entity);
                return Ok(resApi);
            }
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStartedTasksByDateTimeRage at {dateTime}.", DateTime.Now);
            var kresApi = new Result<List<NoteTaskDto>>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpDelete("tasks/{id}")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        try
        {
            _logger.LogTrace("DeleteTask {dateTime}.", DateTime.Now);

            var resApi = await _service.Notes.DeleteNoteTaskAsync(id);
            if (resApi.IsValid)
                return Ok(resApi);
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteTask at {dateTime}.", DateTime.Now);
            var kresApi = new Result<NoteTaskDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    #region Private methods

    private void ListNotesUpdateResourceInDescriptionForRead(List<NoteInfoDto>notes, RepositoryRef repositoryRef)
    {
        foreach(var n in notes)
        {
            n.Description = _service.Notes.UtilUpdateResourceInDescriptionForRead(n.Description);
        }
    }

    // A task's description is Markdown like a note's: its resources with their URLs.
    private void TasksUpdateResourceInDescriptionForRead(List<NoteTaskDto> tasks)
    {
        foreach (var t in tasks)
            t.Description = _service.Notes.UtilUpdateResourceInDescriptionForRead(t.Description);
    }

    #endregion
}
