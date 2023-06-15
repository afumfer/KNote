using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;
using KNote.Client.Shared;
using KNote.Repository.EntityFramework.Entities;
using NLog.Filters;

namespace KNote.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FoldersController : ControllerBase
{
    private readonly IKntService _service;
    private readonly ILogger<FoldersController> _logger;

    public FoldersController(IKntService service, IHttpContextAccessor httpContextAccessor, ILogger<FoldersController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            _logger.LogTrace("Get at {dateTime}.", DateTime.Now);

            var resApi = await _service.Folders.GetAllAsync();
            if (resApi.IsValid)
                return Ok(resApi);
            else

                return BadRequest(resApi);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get at {dateTime}.", DateTime.Now);
            var resApi = new Result<List<FolderInfoDto>>();
            resApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(resApi);
        }
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Tree()
    {
        try
        {
            _logger.LogTrace("GetTree at {dateTime}.", DateTime.Now);

            var resApi = await _service.Folders.GetTreeAsync();
            if (resApi.IsValid)
                return Ok(resApi);
            else
                return BadRequest(resApi);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tree at {dateTime}.", DateTime.Now);
            var resApi = new Result<List<FolderInfoDto>>();
            resApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(resApi);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            _logger.LogTrace("Get {id} get at {dateTime}.", id, DateTime.Now);

            var resApi = await _service.Folders.GetAsync(id);
            if (resApi.IsValid)
                return Ok(resApi);
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get {id }at {dateTime}.", id, DateTime.Now);
            var resApi = new Result<FolderInfoDto>();
            resApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(resApi);
        }
    }

    [HttpPost]
    [HttpPut]
    [Authorize(Roles = "Admin, ProjecManager")]
    public async Task<IActionResult> Post([FromBody]FolderDto folder)
    {
        try
        {
            _logger.LogTrace("Post {name} get at {dateTime}.", folder.Name?.ToString(), DateTime.Now);

            var resApi = await _service.Folders.SaveAsync(folder);
            if (resApi.IsValid)
                return Ok(resApi);
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post {name} at {dateTime}.", folder.Name?.ToString(), DateTime.Now);
            var resApi = new Result<FolderInfoDto>();
            resApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(resApi);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, ProjecManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            _logger.LogTrace("Delete {id} get at {dateTime}.", id, DateTime.Now);

            var resApi = await _service.Folders.DeleteAsync(id);
            if (resApi.IsValid)
                return Ok(resApi);
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete {id} at {dateTime}.", id, DateTime.Now);
            var resApi = new Result<FolderInfoDto>();
            resApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(resApi);
        }
    }
}
