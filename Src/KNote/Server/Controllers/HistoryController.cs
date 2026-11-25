using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Service.Core;

namespace KNote.Server.Controllers;

/// <summary>
/// The history of the notes (03-Historial.md, H.3): the «History» tab of the note editor. Reading it is for any user with a
/// session, like reading the note (the commands are Guest).
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly IKntService _service;
    private readonly ILogger<HistoryController> _logger;

    public HistoryController(IKntService service, IHttpContextAccessor httpContextAccessor, ILogger<HistoryController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _logger = logger;
    }

    // GET api/history/notes/{noteId}?skip=0&take=100
    [HttpGet("notes/{noteId}")]
    public async Task<IActionResult> GetNoteHistory(Guid noteId, int skip = 0, int take = 0)
    {
        try
        {
            var resApi = await _service.History.GetNoteHistoryAsync(noteId, skip, take);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetNoteHistory {noteId} at {dateTime}.", noteId, DateTime.Now);
            var resApi = new Result<NoteHistoryPageDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // GET api/history/notes/{noteId}/versions/{klogId}
    [HttpGet("notes/{noteId}/versions/{klogId}")]
    public async Task<IActionResult> GetNoteVersion(Guid noteId, Guid klogId)
    {
        try
        {
            var resApi = await _service.History.GetNoteVersionAsync(noteId, klogId);
            // As a note read by the web app: its resources by the URL of this host.
            if (resApi.IsValid && resApi.Entity != null)
                resApi.Entity.Description = _service.Notes.UtilUpdateResourceInDescriptionForRead(resApi.Entity.Description);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetNoteVersion {noteId} {klogId} at {dateTime}.", noteId, klogId, DateTime.Now);
            var resApi = new Result<NoteVersionViewDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }
}
