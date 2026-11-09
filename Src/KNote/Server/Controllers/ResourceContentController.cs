using System;
using System.Threading;
using System.Threading.Tasks;
using KNote.Ai;
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

// KNote Atlas, stage 4: what the AI does with the resources of the notes in the session's repository. 4.2: describing a
// photo, one per request (the Web asks for them one after another, with its progress and its cancel), with the model of
// AiPhotoDescriber.ChooseAsync. The roles of the commands apply, as in the assistant (EnforceAuthorization).
[Authorize(Roles = "Staff, ProjectManager, Admin")]
[ApiController]
[Route("api/resources")]
public class ResourceContentController : ControllerBase
{
    private readonly IKntService _service;
    private readonly AiProvidersCatalog _providers;
    private readonly IAiChatClientProvider _chatClients;
    private readonly ILogger<ResourceContentController> _logger;

    public ResourceContentController(IKntService service, IHttpContextAccessor httpContextAccessor, AiProvidersCatalog providers,
        IAiChatClientProvider chatClients, ILogger<ResourceContentController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _providers = providers;
        _chatClients = chatClients;
        _logger = logger;
    }

    // The model that would describe the photos (what the Web shows before describing).
    [HttpGet("describer")]
    public Task<IActionResult> GetDescriber(CancellationToken cancellationToken) =>
        RunAsync(nameof(GetDescriber), async () =>
        {
            var (_, info) = await ChooseAsync(cancellationToken);
            return new Result<ResourceDescriberDto> { Entity = info };
        });

    // Describes a photo and saves its description. replace: also if a person wrote it (the Web asks first).
    [HttpPost("{id:guid}/describe")]
    public Task<IActionResult> Describe(Guid id, [FromQuery] bool replace, CancellationToken cancellationToken) =>
        RunAsync(nameof(Describe), async () =>
        {
            var (provider, info) = await ChooseAsync(cancellationToken);
            if (!info.Available)
            {
                var res = new Result<ResourceInfoDto>();
                res.AddErrorMessage(info.Message);
                return res;
            }

            _service.EnforceAuthorization = true;
            var chatClient = _chatClients.Create(provider, []);
            return await AiPhotoDescriber.DescribeAsync(_service, chatClient, provider, id, replace, _logger, cancellationToken);
        });

    private async Task<(AiProviderRef Provider, ResourceDescriberDto Info)> ChooseAsync(CancellationToken cancellationToken)
    {
        var models = await _service.SystemValues.GetRepositoryAiModelsAsync();
        return await AiPhotoDescriber.ChooseAsync(_providers.ChatProviders, _providers.Default, models.Entity?.LlmModel, cancellationToken);
    }

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
