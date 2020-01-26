using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.DomainModel.Services;
using KNote.Server.Helpers;
using KNote.Shared;
using KNote.Shared.Dto;
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
        public IActionResult Get()
        {
            try
            {
                var resApi = _service.Folders.GetAll();
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

        [HttpGet("getroots")]   // GET api/folders/getroots
        public IActionResult GeRoots()
        {
            try
            {
                var resApi = _service.Folders.GetRoots();
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
        public IActionResult GeTree()
        {
            try
            {
                var resApi = _service.Folders.GetTree();
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
        public IActionResult Get(Guid folderId)
        {
            try
            {
                var resApi = _service.Folders.Get(folderId);
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

        [HttpGet("[action]/{folderNumber}")]    // GET api/folders/GetByNumber/xx        
        public IActionResult GetByNumber(int folderNumber)
        {
            try
            {
                var resApi = _service.Folders.Get(folderNumber);
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
        //[Authorize(Roles = "Administrators")]
        public IActionResult GetNotes(Guid folderId)
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
                var kresApi = new Result<List<FolderInfoDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpPost]   // POST api/folders
        [HttpPut]    // PUT api/folders
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
