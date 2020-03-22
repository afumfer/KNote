﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.DomainModel.Services;
using KNote.Server.Helpers;
using KNote.Shared;
using KNote.Shared.Dto;
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
        public IActionResult Get()
        {
            try
            {
                var resApi = _service.Notes.GetAll();
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

        [HttpGet("getfilter")]   // GET api/notes/getfilter
        public IActionResult GetFilter(int _page, int _limit, Guid folderId, string q)
        {
            try
            {
                var resApi = _service.Notes.GetFilter(_page, _limit, folderId, q);
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
        
        [HttpGet("getfilter2")]   // GET api/notes/getfilter2
        public async Task <IActionResult> GetFilter2([FromQuery] NotesFilterDto notesFilter )
        {
            try
            {                
                var kresApi = await _service.Notes.GetFilter2(notesFilter);

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

        //[Authorize]
        [HttpGet("recentnotes")]   // GET api/notes/recentnotes
        public IActionResult RecentNotes()
        {
            try
            {
                var resApi = _service.Notes.RecentNotes();
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

        [HttpGet("new")]    // GET api/notes/new
        public IActionResult New()
        {
            try
            {
                var resApi = _service.Notes.New();
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

        [HttpGet("[action]/{noteNumber}")]    // GET api/notes/GetByNumber/xx        
        public IActionResult GetByNumber(int noteNumber)
        {
            try
            {
                var resApi = _service.Notes.Get(noteNumber);
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
        public IActionResult GetByFolder(Guid folderId)
        {

            try
            {
                var resApi = _service.Notes.GetByFolder(folderId);
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
                    foreach(var resource in resApi.Entity.ResourcesDto)
                    {
                        if (!string.IsNullOrWhiteSpace(resource.ContentBase64))
                        {                            
                            resource.FullPath = await _fileStore.SaveFile(resource.ContentBase64, resource.Name, resource.Container);
                        }
                    }
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

        [HttpPost("savefile")]    // POST api/notes/savefile
        [HttpPut("savefile")]    // PUT api/notes/savefile
        public async Task<IActionResult> SaveFile(ResourceDto resource)
        {
            Result<ResourceDto> resApi = new Result<ResourceDto>();
            try
            {
                if (!string.IsNullOrWhiteSpace(resource.ContentBase64))
                {                                                            
                    resource.FullPath = await _fileStore.SaveFile(resource.ContentBase64, resource.Name, resource.Container);
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
    }
}
