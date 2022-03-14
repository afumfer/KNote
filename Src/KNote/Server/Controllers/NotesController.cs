using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Service;
using KNote.Server.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace KNote.Server.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private IKntService _service { get; set; }
        private readonly AppSettings _appSettings;
        private readonly IFileStore _fileStore;

        public NotesController(IKntService service, IOptions<AppSettings> appSettings, IFileStore fileStore)
        {
            _service = service;
            _appSettings = appSettings.Value;
            _fileStore = fileStore;

            if (string.IsNullOrEmpty(_service.RepositoryRef.ResourcesContainerCacheRootPath))
                _service.RepositoryRef.ResourcesContainerCacheRootPath = _fileStore.GetResourcesContainerRootPath();
            if (string.IsNullOrEmpty(_service.RepositoryRef.ResourcesContainerCacheRootUrl))
                _service.RepositoryRef.ResourcesContainerCacheRootUrl = _fileStore.GetResourcesContainerRootUrl();
        }

        [HttpGet]   // GET api/notes
        public async Task<IActionResult> Get()
        {
            try
            {
                var resApi = await _service.Notes.GetAllAsync();
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var resApi = new Result<List<NoteInfoDto>>();
                resApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(resApi);
            }
        }

        [HttpPost("[action]")]   // PUT api/notes/getfilter
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        //[Authorize]
        public async Task <IActionResult> GetFilter([FromBody] NotesFilterDto notesFilter )
        {
            try
            {                
                var kresApi = await _service.Notes.GetFilter(notesFilter);
                                
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<List<NoteInfoDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }
                
        [HttpGet("[action]")]   // GET api/notes/getsearch
        public async Task<IActionResult> GetSearch([FromQuery] NotesSearchParam notesSearchParam)  // NotesSearchDto notesSearch
        {
            try
            {
                // TODO: tmp,  refactor
                var notesSearch = new NotesSearchDto();
                notesSearch.TextSearch = notesSearchParam.TextSearch;
                if(notesSearchParam.PageNumber > 0)
                    notesSearch.PageIdentifier.PageNumber = notesSearchParam.PageNumber;
                notesSearch.PageIdentifier.PageSize = notesSearchParam.PageSize;
                // .....

                var kresApi = await _service.Notes.GetSearch(notesSearch);
                
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<List<NoteInfoDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        //[Authorize]
        [HttpGet("[action]")]   // GET api/notes/homenotes
        public async Task<IActionResult> HomeNotes()
        {
            try
            {
                var resApi = await _service.Notes.HomeNotesAsync();
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);

            }
            catch (Exception ex)
            {
                var resApi = new Result<List<NoteInfoDto>>();
                resApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(resApi);
            }
        }

        [HttpGet("{noteId}")]    // GET api/notes/guidnote
        public async Task<IActionResult> Get(Guid noteId)
        {
            try
            {
                var resApi = await _service.Notes.GetAsync(noteId);
                if (resApi.IsValid)
                {
                    return Ok(resApi);
                }
                else
                {
                    return BadRequest(resApi);
                }
            }
            catch (Exception ex)
            {
                var kresApi = new Result<NoteInfoDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpGet("[action]")]    // GET api/notes/new
        [Authorize]
        public async Task<IActionResult> New()
        {
            try
            {
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
                var kresApi = new Result<NoteInfoDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpGet("[action]/{folderId}")]    // GET api/notes/GetByFolder/folderId        
        public async Task<IActionResult> GetByFolder(Guid folderId)
        {
            try
            {                
                var resApi = await _service.Notes.GetByFolderAsync(folderId);
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);

            }
            catch (Exception ex)
            {
                var kresApi = new Result<List<NoteInfoDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpPost]   // POST api/notes
        [HttpPut]    // PUT api/notes
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        public async Task<IActionResult> Post([FromBody]NoteDto note)
        {            
            try
            {
                var resApi = await _service.Notes.SaveAsync(note);
                if (resApi.IsValid)
                {                    
                    return Ok(resApi);
                }

                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<NoteInfoDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpDelete("{id}")]    // DELETE api/notes/guid
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                //var resApi = await _service.Notes.DeleteAsync(id);
                var resApi = await _service.Notes.DeleteExtendedAsync(id);
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<NoteInfoDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpPost("[action]")]
        [HttpPut("[action]")]
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        public async Task<IActionResult> SaveResource([FromBody]ResourceInfoDto entity)
        {
            try
            {                             
                var resApi = await _service.Notes.SaveResourceAsync(entity);
                if (resApi.IsValid)                                    
                    return Ok(resApi);                                                              
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<ResourceDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpGet("{id}/[action]")]    // GET api/notes/getresources
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        public async Task<IActionResult> GetResources(Guid id)
        {
            try
            {
                var resApi = await _service.Notes.GetResourcesInfoAsync(id);
                if (resApi.IsValid)                
                    return Ok(resApi);                               
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<List<ResourceDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpDelete("[action]/{id}")]    // DELETE api/notes/deleteresource/guid
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        public async Task<IActionResult> DeleteResource(Guid id)
        {
            try
            {
                var resApi = await _service.Notes.DeleteResourceInfoAsync(id);
                if (resApi.IsValid)                
                    return Ok(resApi);                
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<ResourceDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpPost("[action]")]    // POST api/notes/savefile
        [HttpPut("[action]")]    // PUT api/notes/savefile
        [Authorize]
        public async Task<IActionResult> SaveFile(ResourceDto resource)
        {
            Result<ResourceDto> resApi = new Result<ResourceDto>();
            try
            {
                if (!string.IsNullOrWhiteSpace(resource.ContentBase64))
                {
                    resource.FullUrl = await _fileStore.SaveFile(resource.ContentBase64, resource.Name, resource.Container);
                }
                resApi.Entity = resource;
                return Ok(resApi);
            }
            catch (Exception ex)
            {
                resApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(resApi);                
            }
        }

        [HttpPost("[action]")]
        [HttpPut("[action]")]
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        public async Task<IActionResult> SaveNoteTask([FromBody]NoteTaskDto entity)
        {            
            try
            {
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
                var kresApi = new Result<NoteTaskDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpGet("{id}/[action]")]    // GET api/notes/getnotetasks
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        public async Task<IActionResult> GetNoteTasks(Guid id)
        {
            try
            {
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
                var kresApi = new Result<List<NoteTaskDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpGet("[action]")]    // GET api/notes/getnotetasks
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        public async Task<IActionResult> GetStartedTasksByDateTimeRage([FromQuery] string start, [FromQuery] string end)
        {
            try
            {
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
                var kresApi = new Result<List<NoteTaskDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }


        [HttpDelete("[action]/{id}")]    // DELETE api/notes/deletenotetask/guid
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        public async Task<IActionResult> DeleteNoteTask(Guid id)
        {
            try
            {
                var resApi = await _service.Notes.DeleteNoteTaskAsync(id);
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<NoteTaskDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }
    }
}
