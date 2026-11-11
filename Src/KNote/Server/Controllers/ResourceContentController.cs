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
// AiPhotoDescriber.ChooseAsync. The roles of the commands apply, as in the assistant (EnforceAuthorization). 4.3: «Process
// pending resources» (Admin, as the semantic index), a run of KntAtlasJobs of the session's repository and with its user.
// 4.4b: reading a document (Staff) and its text (anyone signed in, as the notes).
[Authorize]
[ApiController]
[Route("api/resources")]
public class ResourceContentController : ControllerBase
{
    private const string StaffRoles = "Staff, ProjectManager, Admin";

    private readonly IKntService _service;
    private readonly AiProvidersCatalog _providers;
    private readonly IAiChatClientProvider _chatClients;
    private readonly KntRepositories _repositories;
    private readonly KntAtlasJobs _jobs;
    private readonly ILogger<ResourceContentController> _logger;

    public ResourceContentController(IKntService service, IHttpContextAccessor httpContextAccessor, AiProvidersCatalog providers,
        IAiChatClientProvider chatClients, KntRepositories repositories, KntAtlasJobs jobs, ILogger<ResourceContentController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _providers = providers;
        _chatClients = chatClients;
        _repositories = repositories;
        _jobs = jobs;
        _logger = logger;
    }

    // The model that would describe the photos (what the Web shows before describing).
    [HttpGet("describer")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> GetDescriber(CancellationToken cancellationToken) =>
        RunAsync(nameof(GetDescriber), async () =>
        {
            var (_, info) = await ChooseAsync(cancellationToken);
            return new Result<ResourceDescriberDto> { Entity = info };
        });

    // Describes a photo and saves its description. replace: also if a person wrote it (the Web asks first).
    [HttpPost("{id:guid}/describe")]
    [Authorize(Roles = StaffRoles)]
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

    // Summarizes a document and saves its summary (4.4c), reading it first if it was not read. replace: also if a person wrote its
    // description (the Web asks first). Any model: it doesn't need to see images.
    [HttpPost("{id:guid}/summarize")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> Summarize(Guid id, [FromQuery] bool replace, CancellationToken cancellationToken) =>
        RunAsync(nameof(Summarize), async () =>
        {
            var (provider, info) = await ChooseAsync(cancellationToken);
            if (!info.CanSummarize)
            {
                var res = new Result<ResourceInfoDto>();
                res.AddErrorMessage(info.Message);
                return res;
            }

            _service.EnforceAuthorization = true;
            var chatClient = _chatClients.Create(provider, []);
            return await AiDocumentSummarizer.SummarizeAsync(_service, chatClient, provider, id, replace, _logger, cancellationToken);
        });

    // What «Process pending resources» would process, and the pace of its last run.
    [HttpGet("status")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> GetStatus() =>
        RunAsync(nameof(GetStatus), () => _service.ResourceContent.GetProcessingStatusAsync());

    // Reads the text of a document and keeps it (also when it was read: the reader may read it better now).
    [HttpPost("{id:guid}/read")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> ReadDocument(Guid id) =>
        RunAsync(nameof(ReadDocument), () => _service.ResourceContent.ReadDocumentAsync(id));

    // The text read of a document: its beginning (whole pages, DocumentTextRules.Cut), up to maxLength or, without it, what
    // the Web shows (MaxViewerLength).
    [HttpGet("{id:guid}/text")]
    public Task<IActionResult> GetDocumentText(Guid id, [FromQuery] int? maxLength) =>
        RunAsync(nameof(GetDocumentText), () => _service.ResourceContent.GetDocumentTextAsync(id,
            maxLength is > 0 and <= DocumentTextRules.MaxTextLength ? maxLength.Value : DocumentTextRules.MaxViewerLength));

    // Queues «Process pending resources» (or gives the run the repository already has), with the model of «Describe photos».
    [HttpPost("process")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Process([FromBody] ResourceProcessingOptionsDto options, CancellationToken cancellationToken) =>
        RunAsync(nameof(Process), async () =>
        {
            var result = new Result<ResourceProcessingProgressDto>();
            if (options == null || !options.Any)
            {
                result.AddErrorMessage("Choose what to process.");
                return result;
            }
            // The photos and the scanned pages need a model that sees images; the documents, any model.
            var (provider, info) = await ChooseAsync(cancellationToken);
            if (options.NeedsImages ? !info.Available : !info.CanSummarize)
            {
                result.AddErrorMessage(info.Message);
                return result;
            }

            var repository = _repositories.ForUser(User);
            result.Entity = _jobs.EnqueueResources(repository.RepositoryId, repository.RepositoryRef.Name, User?.Identity?.Name, provider, options);
            return result;
        });

    [HttpGet("process/progress")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetProcessProgress() =>
        Ok(new Result<ResourceProcessingProgressDto>(_jobs.GetResourcesProgress(_repositories.ForUser(User).RepositoryId)));

    [HttpPost("process/cancel")]
    [Authorize(Roles = "Admin")]
    public IActionResult CancelProcess() =>
        Ok(new Result<bool>(_jobs.CancelResources(_repositories.ForUser(User).RepositoryId)));

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
