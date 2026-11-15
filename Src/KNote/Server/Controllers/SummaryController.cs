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

// KNote Atlas, stage 2: the summaries, in the session's repository. 2.1a: the summary of a long document of a note (CU-43), a run of
// KntAtlasJobs of the user who asks (one per user, D18), with a chat model of the Server (the default one, or the alias asked
// for). The roles of the commands apply (EnforceAuthorization), as in the assistant.
[Authorize]
[ApiController]
[Route("api/summaries")]
public class SummaryController : ControllerBase
{
    private const string StaffRoles = "Staff, ProjectManager, Admin";

    private readonly IKntService _service;
    private readonly AiProvidersCatalog _providers;
    private readonly KntRepositories _repositories;
    private readonly KntAtlasJobs _jobs;
    private readonly ILogger<SummaryController> _logger;

    public SummaryController(IKntService service, IHttpContextAccessor httpContextAccessor, AiProvidersCatalog providers,
        KntRepositories repositories, KntAtlasJobs jobs, ILogger<SummaryController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _providers = providers;
        _repositories = repositories;
        _jobs = jobs;
        _logger = logger;
    }

    // What the summary of a document would take (pages, calls and time).
    [HttpPost("documents/estimate")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> EstimateDocument([FromBody] DocumentProcessRequestDto request, CancellationToken cancellationToken) =>
        RunAsync(nameof(EstimateDocument), async () =>
        {
            var (provider, error) = await ProviderAsync(request);
            if (error != null)
                return error.As<DocumentProcessEstimateDto>();

            _service.EnforceAuthorization = true;
            return await AiDocumentProcessor.EstimateAsync(_service, provider, request, _logger, cancellationToken);
        });

    // Queues the summary of a document for the user (or gives the one the user already has in this repository).
    [HttpPost("documents")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> SummarizeDocument([FromBody] DocumentProcessRequestDto request) =>
        RunAsync(nameof(SummarizeDocument), async () =>
        {
            var (provider, error) = await ProviderAsync(request);
            if (error != null)
                return error.As<DocumentProcessProgressDto>();

            var repository = _repositories.ForUser(User);
            return new Result<DocumentProcessProgressDto>(
                _jobs.EnqueueDocument(repository.RepositoryId, repository.RepositoryRef.Name, User?.Identity?.Name, provider, request));
        });

    // The user's summary of a document in this repository: running, queued or the last one (null, none).
    [HttpGet("documents/progress")]
    [Authorize(Roles = StaffRoles)]
    public IActionResult GetDocumentProgress() =>
        Ok(new Result<DocumentProcessProgressDto>(_jobs.GetDocumentProgress(_repositories.ForUser(User).RepositoryId, User?.Identity?.Name)));

    [HttpPost("documents/cancel")]
    [Authorize(Roles = StaffRoles)]
    public IActionResult CancelDocument() =>
        Ok(new Result<bool>(_jobs.CancelDocument(_repositories.ForUser(User).RepositoryId, User?.Identity?.Name)));

    // The chat provider asked for or, without one, the repository's recommended LLM if this Server has it, else the default one.
    private async Task<(AiProviderRef Provider, Result Error)> ProviderAsync(DocumentProcessRequestDto request)
    {
        var error = new Result();
        if (request == null || request.ResourceId == Guid.Empty)
        {
            error.AddErrorMessage("The document is required.");
            return (null, error);
        }

        AiProviderRef provider;
        if (string.IsNullOrWhiteSpace(request.ProviderAlias))
        {
            var models = await _service.SystemValues.GetRepositoryAiModelsAsync();
            provider = AiProviderSelection.ForRepository(_providers.ChatProviders, models.Entity?.LlmModel, _providers.Default);
        }
        else
            provider = _providers.Find(request.ProviderAlias);
        if (provider == null)
        {
            error.AddErrorMessage(string.IsNullOrWhiteSpace(request.ProviderAlias)
                ? "This Server has no AI model to summarize: configure a chat provider."
                : $"The AI provider «{request.ProviderAlias}» is not configured in this Server.");
            return (null, error);
        }
        return (provider, null);
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

internal static class SummaryControllerResults
{
    // The errors of a Result as a Result<T>, for an action that answers with T.
    public static Result<T> As<T>(this Result result)
    {
        var typed = new Result<T>();
        typed.AddListErrorMessage(result.ListErrorMessage);
        return typed;
    }
}
