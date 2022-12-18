﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Server.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using KNote.Service.Core;
using Microsoft.AspNetCore.Http;

namespace KNote.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FoldersController : ControllerBase
    {
        private readonly IKntService _service;

        public FoldersController(IKntService service, IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
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

        [HttpGet("[action]")]   // GET api/folders/gettree
        public async Task<IActionResult> Tree()
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
