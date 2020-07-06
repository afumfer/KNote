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

        [HttpPost("getfilter")]   // PUT api/notes/getfilter
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        public async Task <IActionResult> GetFilter([FromBody] NotesFilterDto notesFilter )
        {
            try
            {                
                var kresApi = await _service.Notes.GetFilter(notesFilter);

                HttpContext.InsertPaginationParamInResponse(kresApi.CountEntity, notesFilter.NumRecords);
                
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

        [HttpGet("getsearch")]   // GET api/notes/getfilter
        public async Task<IActionResult> GetSearch([FromQuery] NotesSearchDto notesSearch)
        {
            try
            {
                var kresApi = await _service.Notes.GetSearch(notesSearch);

                HttpContext.InsertPaginationParamInResponse(kresApi.CountEntity, notesSearch.NumRecords);

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
        [HttpGet("homenotes")]   // GET api/notes/homenotes
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

        [HttpGet("new")]    // GET api/notes/new
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

        [HttpGet("[action]/{folderId}")]    // GET api/notes/GetByFolder/xxxxxxxxxx        
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
                var resApi = await _service.Notes.DeleteAsync(id);
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
        public async Task<IActionResult> SaveResource([FromBody]ResourceDto entity)
        {
            try
            {
                var resApi = await _service.Notes.SaveResourceAsync(entity);
                if (resApi.IsValid)
                {
                    resApi.Entity.FullUrl = await _fileStore.SaveFile(resApi.Entity.ContentBase64, resApi.Entity.Name, resApi.Entity.Container, resApi.Entity.NoteId);
                    resApi.Entity.RelativeUrl = _fileStore.GetRelativeUrl(resApi.Entity.Name, resApi.Entity.Container, resApi.Entity.NoteId);
                    return Ok(resApi);                
                }
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

        [HttpGet("[action]/{id}")]    // GET api/notes/getnoteresources
        [Authorize(Roles = "Admin, Staff, ProjecManager")]
        public async Task<IActionResult> GetNoteResources(Guid id)
        {
            try
            {
                var resApi = await _service.Notes.GetNoteResourcesAsync(id);
                if (resApi.IsValid)
                {
                    foreach (var r in resApi.Entity)
                    {
                        r.ContentBase64 = Convert.ToBase64String(r.ContentArrayBytes);
                        r.ContentArrayBytes = null;
                        r.RelativeUrl = _fileStore.GetRelativeUrl(r.Name, r.Container, id);
                        r.FullUrl = await _fileStore.SaveFile(r.ContentBase64, r.Name, r.Container, id);
                    }
                    return Ok(resApi);
                }
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
                var resApi = await _service.Notes.DeleteResourceAsync(id);
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

        [HttpPost("savefile")]    // POST api/notes/savefile
        [HttpPut("savefile")]    // PUT api/notes/savefile
        public async Task<IActionResult> SaveFile(ResourceDto resource)
        {
            Result<ResourceDto> resApi = new Result<ResourceDto>();
            try
            {
                if (!string.IsNullOrWhiteSpace(resource.ContentBase64))
                {                                                            
                    resource.FullUrl = await _fileStore.SaveFile(resource.ContentBase64, resource.Name, resource.Container, resource.NoteId);
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

        [HttpGet("[action]/{id}")]    // GET api/notes/getnoteresources
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


        #region Código candidato a eliminar

        #region GetByNumber
        //[HttpGet("[action]/{noteNumber}")]    // GET api/notes/GetByNumber/xx        
        //public IActionResult GetByNumber(int noteNumber)
        //{
        //    try
        //    {
        //        var resApi = _service.Notes.Get(noteNumber);
        //        if (resApi.IsValid)
        //            return Ok(resApi);
        //        else
        //        {
        //            return BadRequest(resApi);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var kresApi = new Result<NoteInfoDto>();
        //        kresApi.AddErrorMessage("Generic error: " + ex.Message);
        //        return BadRequest(kresApi);
        //    }
        //}
        #endregion

        #region GetFilter (old)
        //[HttpGet("getfilter")]   // GET api/notes/getfilter
        //public IActionResult GetFilter(int _page, int _limit, Guid folderId, string q)
        //{
        //    try
        //    {
        //        var resApi = _service.Notes.GetFilter(_page, _limit, folderId, q);
        //        if (resApi.IsValid)
        //            return Ok(resApi);
        //        else
        //            return BadRequest(resApi);

        //    }
        //    catch (Exception ex)
        //    {
        //        var resApi = new Result<List<NoteInfoDto>>();
        //        resApi.AddErrorMessage("Generic error: " + ex.Message);
        //        return BadRequest(resApi);
        //    }
        //}
        #endregion 

        #endregion
    }
}
