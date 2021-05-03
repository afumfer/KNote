using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Server.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KNote.Server.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FoldersController : ControllerBase
    {
        private IKntService _service { get; set; }
        private readonly AppSettings _appSettings;

        public FoldersController(IKntService service, IOptions<AppSettings> appSettings)
        {
            _service = service;
            _appSettings = appSettings.Value;
        }

        [HttpGet]   // GET api/folders
        public async Task<IActionResult> Get()
        {
            try
            {
                var resApi = await _service.Folders.GetAllAsync();
                if (resApi.IsValid)
                    return Ok(resApi);
                else

                    return BadRequest(resApi);

            }
            catch (Exception ex)
            {
                var resApi = new Result<List<FolderInfoDto>>();
                resApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(resApi);
            }
        }

        [HttpGet("gettree")]   // GET api/folders/gettree
        public async Task<IActionResult> GeTree()
        {
            try
            {
                var resApi = await _service.Folders.GetTreeAsync();
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);

            }
            catch (Exception ex)
            {
                var resApi = new Result<List<FolderInfoDto>>();
                resApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(resApi);
            }
        }

        [HttpGet("{folderId}")]    // GET api/folders/guidfolder
        public async Task<IActionResult> Get(Guid folderId)
        {
            try
            {
                var resApi = await _service.Folders.GetAsync(folderId);
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var resApi = new Result<FolderInfoDto>();
                resApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(resApi);
            }
        }

        [HttpGet("[action]/{folderId}")]    // GET api/folders/GetNotes/xxxxxxxxxx        
        public async Task<IActionResult> GetNotes(Guid folderId)
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
                var kresApi = new Result<List<FolderInfoDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpPost]   // POST api/folders
        [HttpPut]    // PUT api/folders
        [Authorize(Roles = "Admin, ProjecManager")]
        public async Task<IActionResult> Post([FromBody]FolderDto folder)
        {
            try
            {
                var resApi = await _service.Folders.SaveAsync(folder);
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var resApi = new Result<FolderInfoDto>();
                resApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(resApi);
            }
        }

        [HttpDelete("{id}")]    // DELETE api/folders/guid     
        [Authorize(Roles = "Admin, ProjecManager")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var resApi = await _service.Folders.DeleteAsync(id);
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var resApi = new Result<FolderInfoDto>();
                resApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(resApi);
            }
        }
    }
}
