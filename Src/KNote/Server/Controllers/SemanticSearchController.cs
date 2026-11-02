using System;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Ai;
using KNote.Server.Helpers;
using KNote.Service.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Controllers;

// KNote Atlas: the repository's AI models and, in the next deliveries of stage 0, its semantic index and the search by
// meaning. The models are SystemValues of the session's repository (Service: SystemValues.Get/SaveRepositoryAiModelsAsync);
// which models this Server has are its AI providers (AiProvidersCatalog).
[Authorize]
[ApiController]
[Route("api/semantic")]
public class SemanticSearchController : ControllerBase
{
    private readonly IKntService _service;
    private readonly AiProvidersCatalog _providers;
    private readonly ILogger<SemanticSearchController> _logger;

    public SemanticSearchController(IKntService service, IHttpContextAccessor httpContextAccessor, AiProvidersCatalog providers,
        ILogger<SemanticSearchController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _providers = providers;
        _logger = logger;
    }

    // The repository's models, with whether this Server has its embedding model. Any user (as the command).
    [HttpGet("models")]
    public async Task<IActionResult> GetModels()
    {
        try
        {
            var resApi = await _service.SystemValues.GetRepositoryAiModelsAsync();
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetModels at {dateTime}.", DateTime.Now);
            var resApi = new Result<RepositoryAiModelsDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    [HttpPost("models")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SaveModels([FromBody] RepositoryAiModelsDto models)
    {
        try
        {
            var resApi = await _service.SystemValues.SaveRepositoryAiModelsAsync(models);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveModels at {dateTime}.", DateTime.Now);
            var resApi = new Result<RepositoryAiModelsDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // The embedding and chat models of this Server's AI providers, to choose the repository's ones (never hosts or keys).
    [HttpGet("installation-models")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetInstallationModels() =>
        Ok(new Result<AiInstallationModelsDto>(_providers.GetModels()));
}
