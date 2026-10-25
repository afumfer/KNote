using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Server.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KNote.Server.Controllers;

/// <summary>
/// The Application info alarms of the web app (the alarms panel of the Client). Check consumes the user's alarms
/// that have become due (each one is returned once, also with several tabs or ClientWin checking at the same time) and
/// says when to check again; notices gives the current data of the ones the panel keeps (in the browser), leaving out
/// those that are no longer the user's notices (deleted, or changed to another user or kind).
/// </summary>
[Authorize(Roles = "Staff, ProjectManager, Admin")]
[ApiController]
[Route("api/[controller]")]
public class AlarmsController : ControllerBase
{
    private readonly IKntService _service;
    private readonly IOptions<AlarmsConfig> _config;
    private readonly ILogger<AlarmsController> _logger;

    public AlarmsController(IKntService service, IOptions<AlarmsConfig> config, IHttpContextAccessor httpContextAccessor, ILogger<AlarmsController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _config = config;
        _logger = logger;
    }

    [HttpPost("check")]
    public async Task<IActionResult> Check()
    {
        try
        {
            _logger.LogTrace("Check at {dateTime}.", DateTime.Now);

            var resApi = new Result<AlarmCheckDto>(new AlarmCheckDto { CheckSeconds = _config.Value.CheckSeconds });
            var due = await _service.Notes.ConsumeDueAlarmsAsync(EnumNotificationType.AppInfo);
            if (!due.IsValid)
            {
                resApi.AddErrorMessage(due.ErrorMessage);
                return BadRequest(resApi);
            }

            if (due.Entity.Count > 0)
            {
                var notices = await _service.Notes.GetAlarmNoticesAsync(due.Entity.Select(m => m.KMessageId).ToList());
                if (notices.IsValid)
                    resApi.Entity.Notices = notices.Entity;
                else
                    resApi.AddErrorMessage(notices.ErrorMessage);
            }
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Check at {dateTime}.", DateTime.Now);
            var resApi = new Result<AlarmCheckDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    [HttpPost("notices")]
    public async Task<IActionResult> Notices([FromBody] List<Guid> messageIds)
    {
        try
        {
            _logger.LogTrace("Notices at {dateTime}.", DateTime.Now);

            var resApi = await _service.Notes.GetAlarmNoticesAsync(messageIds ?? new());
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notices at {dateTime}.", DateTime.Now);
            var resApi = new Result<List<AlarmNoticeDto>>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }
}
