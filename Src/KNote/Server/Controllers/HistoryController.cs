using System;
using System.Collections.Generic;
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

    // GET api/history/notes/{noteId}/compare/{klogId}?withCurrentNote=false  (H22: the text as it is kept, not drawn)
    [HttpGet("notes/{noteId}/compare/{klogId}")]
    public async Task<IActionResult> GetNoteComparison(Guid noteId, Guid klogId, bool withCurrentNote = false)
    {
        try
        {
            var resApi = await _service.History.GetNoteComparisonAsync(noteId, klogId, withCurrentNote);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetNoteComparison {noteId} {klogId} at {dateTime}.", noteId, klogId, DateTime.Now);
            var resApi = new Result<NoteComparisonDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // GET api/history/deleted  (H24: «Recover deleted notes», who deletes notes)
    [HttpGet("deleted")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> GetDeletedNotes()
    {
        try
        {
            var resApi = await _service.History.GetDeletedNotesAsync();
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDeletedNotes at {dateTime}.", DateTime.Now);
            var resApi = new Result<List<DeletedNoteDto>>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // POST api/history/deleted/{noteId}/recover  (H24)
    [HttpPost("deleted/{noteId}/recover")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> RecoverNote(Guid noteId)
    {
        try
        {
            var resApi = await _service.History.RecoverNoteAsync(noteId);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RecoverNote {noteId} at {dateTime}.", noteId, DateTime.Now);
            var resApi = new Result<NoteRecoverResultDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // DELETE api/history/deleted/{noteId}  (H27: «Remove permanently», Admin)
    [HttpDelete("deleted/{noteId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveDeletedNote(Guid noteId)
    {
        try
        {
            var resApi = await _service.History.RemoveDeletedNoteAsync(noteId);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RemoveDeletedNote {noteId} at {dateTime}.", noteId, DateTime.Now);
            var resApi = new Result();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // POST api/history/changes  (H28, «Repository changes»: the structure only for ProjectManager and up, checked by the command)
    [HttpPost("changes")]
    public async Task<IActionResult> GetRepositoryChanges(RepositoryChangesQueryDto query)
    {
        try
        {
            var resApi = await _service.History.GetRepositoryChangesAsync(query);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRepositoryChanges at {dateTime}.", DateTime.Now);
            var resApi = new Result<RepositoryChangesPageDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // POST api/history/changes/users  (H28: the users of the period, for its filter)
    [HttpPost("changes/users")]
    public async Task<IActionResult> GetRepositoryChangesUsers(RepositoryChangesQueryDto query)
    {
        try
        {
            var resApi = await _service.History.GetRepositoryChangesUsersAsync(query);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRepositoryChangesUsers at {dateTime}.", DateTime.Now);
            var resApi = new Result<List<string>>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // GET api/history/retention  (H25)
    [HttpGet("retention")]
    public async Task<IActionResult> GetRetention()
    {
        try
        {
            var resApi = await _service.History.GetRetentionAsync();
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRetention at {dateTime}.", DateTime.Now);
            var resApi = new Result<HistoryRetentionInfoDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // PUT api/history/retention  (H25, Admin)
    [HttpPut("retention")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SaveRetention(HistoryRetentionDto retention)
    {
        try
        {
            var resApi = await _service.History.SaveRetentionAsync(retention);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveRetention at {dateTime}.", DateTime.Now);
            var resApi = new Result<HistoryRetentionDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // POST api/history/notes/{noteId}/versions/{klogId}/restore  (H23: who edits the notes)
    [HttpPost("notes/{noteId}/versions/{klogId}/restore")]
    [Authorize(Roles = "Admin, Staff, ProjectManager")]
    public async Task<IActionResult> RestoreNoteVersion(Guid noteId, Guid klogId)
    {
        try
        {
            var resApi = await _service.History.RestoreNoteVersionAsync(noteId, klogId);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RestoreNoteVersion {noteId} {klogId} at {dateTime}.", noteId, klogId, DateTime.Now);
            var resApi = new Result<NoteRestoreResultDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }
}
