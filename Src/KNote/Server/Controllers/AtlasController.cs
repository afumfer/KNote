using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Service.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Controllers;

// KNote Atlas: the context the AI receives in the session's repository (stage 1, chapters 19–21; IKntAtlasContextService).
// The windows of the profile, the glossary and the task instructions are for any user; creating their notes, for
// ProjectManager and Admin (who also save them with the API of notes: a rule of the Service); what the AI receives, Staff.
[Authorize]
[ApiController]
[Route("api/atlas")]
public class AtlasController : ControllerBase
{
    private readonly IKntService _service;
    private readonly ILogger<AtlasController> _logger;

    public AtlasController(IKntService service, IHttpContextAccessor httpContextAccessor, ILogger<AtlasController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _logger = logger;
    }

    [HttpGet("profile")]
    public Task<IActionResult> GetProfile() =>
        RunAsync(nameof(GetProfile), () => _service.AtlasContext.GetProfileInfoAsync());

    [HttpPost("profile")]
    [Authorize(Roles = "ProjectManager, Admin")]
    public Task<IActionResult> CreateProfile() =>
        RunAsync(nameof(CreateProfile), () => _service.AtlasContext.CreateProfileAsync());

    [HttpPost("profile/fill")]
    [Authorize(Roles = "ProjectManager, Admin")]
    public Task<IActionResult> FillProfile() =>
        RunAsync(nameof(FillProfile), () => _service.AtlasContext.FillProfileFoldersAndTypesAsync());

    [HttpGet("glossary")]
    public Task<IActionResult> GetGlossary() =>
        RunAsync(nameof(GetGlossary), () => _service.AtlasContext.GetGlossaryInfoAsync());

    [HttpPost("glossary")]
    [Authorize(Roles = "ProjectManager, Admin")]
    public Task<IActionResult> CreateGlossary() =>
        RunAsync(nameof(CreateGlossary), () => _service.AtlasContext.CreateGlossaryAsync());

    [HttpGet("tasks")]
    public Task<IActionResult> GetTaskPrompts() =>
        RunAsync(nameof(GetTaskPrompts), () => _service.AtlasContext.GetTaskPromptsAsync());

    [HttpPost("tasks/{taskKey}/customize")]
    [Authorize(Roles = "ProjectManager, Admin")]
    public Task<IActionResult> CustomizeTask(string taskKey) =>
        RunAsync(nameof(CustomizeTask), () => _service.AtlasContext.CustomizeTaskAsync(taskKey));

    // «What the AI receives» (with a test text for the glossary).
    [HttpPost("compose")]
    [Authorize(Roles = "Staff, ProjectManager, Admin")]
    public Task<IActionResult> Compose([FromBody] AiComposeRequestDto request) =>
        RunAsync(nameof(Compose), () => _service.AtlasContext.ComposeAsync(request ?? new AiComposeRequestDto()));

    private async Task<IActionResult> RunAsync<T>(string action, Func<Task<Result<T>>> run)
    {
        try
        {
            var resApi = await run();
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{action} at {dateTime}.", action, DateTime.Now);
            var resApi = new Result<T>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }
}
