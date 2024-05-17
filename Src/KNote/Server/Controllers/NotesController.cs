using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using KNote.Server.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

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
            resApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(resApi);
        }
    }

    [HttpPost("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]        
    public async Task <IActionResult> Filter([FromBody] NotesFilterDto notesFilter)
    {
        try
        {
            _logger.LogTrace("Filter {dateTime}.", DateTime.Now);

            var resApi = await _service.Notes.GetFilter(notesFilter);
                            
            if (resApi.IsValid)
            {
                // Hack, this is temporary to resolve resources in virtual directories in the WebAPI.
                ListNotesUpdateResourceInDescriptionForRead(resApi.Entity, _service.RepositoryRef);
                // ----
                return Ok(resApi);
            }
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Filter at {dateTime}.", DateTime.Now);
            var kresApi = new Result<List<NoteInfoDto>>();
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }
            
    [HttpGet("[action]")]
    public async Task<IActionResult> Search([FromQuery] NotesSearchParam notesSearchParam)  // NotesSearchDto notesSearch
    {
        try
        {
            _logger.LogTrace("Search {dateTime}.", DateTime.Now);
            
            var notesSearch = new NotesSearchDto();
            notesSearch.TextSearch = notesSearchParam.TextSearch;
            if(notesSearchParam.PageNumber > 0)
                notesSearch.PageIdentifier.PageNumber = notesSearchParam.PageNumber;
            notesSearch.PageIdentifier.PageSize = notesSearchParam.PageSize;
            // .....

            var resApi = await _service.Notes.GetSearch(notesSearch);
            
            if (resApi.IsValid)
            {
                // Hack, this is temporary to resolve resources in virtual directories in the WebAPI.
                ListNotesUpdateResourceInDescriptionForRead(resApi.Entity, _service.RepositoryRef);
                // ----
                return Ok(resApi);
            }
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search at {dateTime}.", DateTime.Now);
            var kresApi = new Result<List<NoteInfoDto>>();
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [AllowAnonymous]
    [HttpGet("[action]")]
    public async Task<IActionResult> HomeNotes()
    {
        try
        {
            _logger.LogTrace("HomeNotes {dateTime}.", DateTime.Now);

            var resApi = await _service.Notes.HomeNotesAsync();
            if (resApi.IsValid)
            {
                // Hack, this is temporary to resolve resources in virtual directories in the WebAPI.
                ListNotesUpdateResourceInDescriptionForRead(resApi.Entity, _service.RepositoryRef);
                // ----
                return Ok(resApi);
            }
            else
                return BadRequest(resApi);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HomeNotes at {dateTime}.", DateTime.Now);
            var resApi = new Result<List<NoteInfoDto>>();
            resApi.AddErrorMessage("Generic error: " + ex.Message);
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
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpGet("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]
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
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpPost]
    [HttpPut]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]
    public async Task<IActionResult> Post([FromBody]NoteDto note)
    {            
        try
        {
            _logger.LogTrace("Post/Put {topic} at {dateTime}.", note.Topic?.ToString(), DateTime.Now);

            // Hack to make it compatible with the desktop application. ----------------------------------
            note.Description = _service.Notes.UtilUpdateResourceInDescriptionForWrite(note.Description);
            if(note.Description != null)
            {
                var blockingContentType = note.ContentType.Contains('#');
                if (note.Description.StartsWith(@"<BODY"))
                {
                    blockingContentType = true; // override this value
                    note.ContentType = "html";
                }
                else
                {                
                    note.Description = note.Description.Replace("\n", "\r\n");
                    note.ContentType = "markdown";
                }
                if(blockingContentType)
                    note.ContentType = "#" + note.ContentType;
            }
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
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]
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
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpPost("[action]")]
    [HttpPut("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]
    public async Task<IActionResult> Resources([FromBody]ResourceInfoDto entity)
    {
        try
        {
            _logger.LogTrace("Resources(post/put) {dateTime}.", DateTime.Now);

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
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpGet("{id}/[action]")]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]
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
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpDelete("resources/{id}")]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]
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
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpPost("[action]")]
    [HttpPut("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]
    public async Task<IActionResult> SaveFile(ResourceDto resource)
    {
        Result<ResourceDto> resApi = new Result<ResourceDto>();
        try
        {
            _logger.LogTrace("SaveFile {dateTime}.", DateTime.Now);

            if (!string.IsNullOrWhiteSpace(resource.ContentBase64))
            {
                resource.FullUrl = await _fileStore.SaveFile(resource.ContentBase64, resource.Name, resource.Container);
            }
            resApi.Entity = resource;
            return Ok(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveFile at {dateTime}.", DateTime.Now);
            resApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(resApi);                
        }
    }

    [HttpPost("[action]")]
    [HttpPut("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]
    public async Task<IActionResult> Tasks([FromBody]NoteTaskDto entity)
    {            
        try
        {
            _logger.LogTrace("Tasks(post/put) {dateTime}.", DateTime.Now);

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
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpGet("{id}/[action]")]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]
    public async Task<IActionResult> Tasks(Guid id)
    {
        try
        {
            _logger.LogTrace("Tasks {id} {dateTime}.", id, DateTime.Now);

            var resApi = await _service.Notes.GetNoteTasksAsync(id);
            if (resApi.IsValid)                
            {                    
                return Ok(resApi);
            }
            else
                return BadRequest(resApi);                
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tasks {id} at {dateTime}.", id, DateTime.Now);
            var kresApi = new Result<List<NoteTaskDto>>();
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpGet("[action]")]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]
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
                return Ok(resApi);
            }
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStartedTasksByDateTimeRage at {dateTime}.", DateTime.Now);
            var kresApi = new Result<List<NoteTaskDto>>();
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpDelete("tasks/{id}")]
    [Authorize(Roles = "Admin, Staff, ProjecManager")]
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
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
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

    #endregion
}
