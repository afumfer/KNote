using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Server.Helpers;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TraceNoteTypesController : ControllerBase
{
    private readonly IKntService _service;
    private readonly ILogger<TraceNoteTypesController> _logger;

    public TraceNoteTypesController(IKntService service, IHttpContextAccessor httpContextAccessor, ILogger<TraceNoteTypesController> logger)
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

            var kresApi = await _service.TraceNoteTypes.GetAllAsync();
            if (kresApi.IsValid)
                return Ok(kresApi);
            else
                return BadRequest(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get at {dateTime}.", DateTime.Now);
            var kresApi = new Result<List<TraceNoteTypeDto>>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            _logger.LogTrace("Get {id} at {dateTime}.", id, DateTime.Now);

            var resApi = await _service.TraceNoteTypes.GetAsync(id);
            if (resApi.IsValid)
                return Ok(resApi);
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get {id} at {dateTime}.", id, DateTime.Now);
            var kresApi = new Result<TraceNoteTypeDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpPost]
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] TraceNoteTypeDto traceNoteType)
    {
        try
        {
            _logger.LogTrace("Post {name} at {dateTime}.", traceNoteType.Name?.ToString(), DateTime.Now);

            var kresApi = await _service.TraceNoteTypes.SaveAsync(traceNoteType);
            if (kresApi.IsValid)
                return Ok(kresApi);
            else
                return BadRequest(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post {name} at {dateTime}.", traceNoteType.Name?.ToString(), DateTime.Now);
            var kresApi = new Result<TraceNoteTypeDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    // A type still used by trace notes is not deleted: the database refuses it (foreign key) and the error
    // comes back in the Result, as in ClientWin.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            _logger.LogTrace("Delete {id} at {dateTime}.", id, DateTime.Now);

            var resApi = await _service.TraceNoteTypes.DeleteAsync(id);
            if (resApi.IsValid)
                return Ok(resApi);
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete {id} at {dateTime}.", id, DateTime.Now);
            var resApi = new Result<TraceNoteTypeDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

}
