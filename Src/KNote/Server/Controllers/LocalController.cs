using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Server.Local;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Controllers;

/// <summary>
/// The settings of KNote Local's computer (the "This computer" tab of the admin panel): its repositories, AI providers,
/// e-mail account, port, and stopping the server. They are of the computer, not of a repository, so they take an Admin
/// of the session's repository (in KNote Local, the person who uses the computer) and a request from the computer
/// itself (KntLocalOnly): if the server is ever opened to other computers, they still can't change it. Not there out of
/// local mode.
/// </summary>
[Authorize(Roles = "Admin")]
[KntLocalOnly]
[ApiController]
[Route("api/[controller]")]
public class LocalController : ControllerBase
{
    private readonly KntLocalSettings _settings;
    private readonly ILogger<LocalController> _logger;

    public LocalController(KntLocalSettings settings, ILogger<LocalController> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    [HttpGet("repositories")]
    public Task<IActionResult> GetRepositories() =>
        RunAsync("Get repositories", async () => new Result<List<LocalRepositoryDto>>(await _settings.GetRepositoriesAsync()));

    [HttpPost("repositories")]
    public Task<IActionResult> AddRepository([FromBody] LocalRepositoryAddDto repository) =>
        RunAsync("Add repository", () => _settings.AddRepositoryAsync(repository));

    [HttpPut("repositories/{index}/default")]
    public Task<IActionResult> SetDefaultRepository(int index) =>
        RunAsync("Set default repository", () => _settings.SetDefaultRepositoryAsync(index));

    [HttpPut("repositories/{index}/name")]
    public Task<IActionResult> RenameRepository(int index, [FromBody] string name) =>
        RunAsync("Rename repository", () => _settings.RenameRepositoryAsync(index, name));

    [HttpDelete("repositories/{index}")]
    public Task<IActionResult> UnlinkRepository(int index) =>
        RunAsync("Unlink repository", () => _settings.UnlinkRepositoryAsync(index, KntRepositories.RepositoryIdOf(User)));

    [HttpGet("ai")]
    public Task<IActionResult> GetAiProviders() =>
        RunAsync("Get AI providers", () => Task.FromResult(new Result<List<LocalAiProviderDto>>(_settings.GetAiProviders())));

    [HttpPut("ai")]
    public Task<IActionResult> SaveAiProviders([FromBody] List<LocalAiProviderDto> providers) =>
        RunAsync("Save AI providers", () => _settings.SaveAiProvidersAsync(providers));

    [HttpGet("email")]
    public Task<IActionResult> GetEmail() =>
        RunAsync("Get e-mail", () => Task.FromResult(new Result<LocalEmailSettingsDto>(_settings.GetEmail())));

    [HttpPut("email")]
    public Task<IActionResult> SaveEmail([FromBody] LocalEmailSettingsDto email) =>
        RunAsync("Save e-mail", () => _settings.SaveEmailAsync(email));

    [HttpGet("server")]
    public Task<IActionResult> GetServer() =>
        RunAsync("Get server", () => Task.FromResult(new Result<LocalServerSettingsDto>(_settings.GetServer())));

    [HttpPut("server/port")]
    public Task<IActionResult> SavePort([FromBody] int port) =>
        RunAsync("Save port", () => _settings.SavePortAsync(port));

    [HttpPost("stop")]
    public IActionResult Stop([FromServices] IHostApplicationLifetime lifetime)
    {
        _logger.LogInformation("KNote Local stopped by {user} at {dateTime}.", User?.Identity?.Name, DateTime.Now);
        lifetime.StopApplication();
        return Ok(new Result<bool>(true));
    }

    private async Task<IActionResult> RunAsync<T>(string action, Func<Task<Result<T>>> run)
    {
        try
        {
            _logger.LogTrace("{action} at {dateTime}.", action, DateTime.Now);
            var result = await run();
            return result.IsValid ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{action} at {dateTime}.", action, DateTime.Now);
            var result = new Result<T>();
            result.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(result);
        }
    }
}

/// <summary>
/// Only in KNote Local (404 otherwise) and only for requests from the computer itself (403 otherwise). A resource filter:
/// it runs before the controller is created, whose services only exist in local mode.
/// </summary>
public class KntLocalOnlyAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        if (context.HttpContext.RequestServices.GetService<KntLocalData>() == null)
            context.Result = new NotFoundResult();
        else if (!KntLocal.IsLocalRequest(context.HttpContext))
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
