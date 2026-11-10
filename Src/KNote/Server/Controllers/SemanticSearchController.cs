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

// KNote Atlas: the repository's AI models, its semantic index and the search by meaning (the most similar notes, not
// paged). The models are SystemValues of the session's repository (Service: SystemValues.Get/SaveRepositoryAiModelsAsync);
// which models this Server has are its AI providers (AiProvidersCatalog). Creating, updating or rebuilding the index is
// a run of KntAtlasJobs (one queue for the Server), of the session's repository and with its user: each user sees
// and cancels only the run of the repository of their session.
[Authorize]
[ApiController]
[Route("api/semantic")]
public class SemanticSearchController : ControllerBase
{
    private readonly IKntService _service;
    private readonly AiProvidersCatalog _providers;
    private readonly KntRepositories _repositories;
    private readonly KntAtlasJobs _jobs;
    private readonly ILogger<SemanticSearchController> _logger;

    public SemanticSearchController(IKntService service, IHttpContextAccessor httpContextAccessor, AiProvidersCatalog providers,
        KntRepositories repositories, KntAtlasJobs jobs, ILogger<SemanticSearchController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _providers = providers;
        _repositories = repositories;
        _jobs = jobs;
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

    // How the index of the session's repository is. Any user (as the command).
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var resApi = await _service.SemanticSearch.GetIndexStatusAsync();
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStatus at {dateTime}.", DateTime.Now);
            var resApi = new Result<SemanticIndexStatusDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // Creates, updates or rebuilds the index of the session's repository in the background: the progress of its run (the
    // one it already had, if it had one running or queued).
    [HttpPost("index")]
    [Authorize(Roles = "Admin")]
    public IActionResult RunIndex([FromBody] SemanticIndexRunDto run)
    {
        var repository = _repositories.ForUser(User);
        var progress = _jobs.EnqueueIndex(repository.RepositoryId, repository.RepositoryRef.Name, User?.Identity?.Name, run?.Rebuild == true);
        return Ok(new Result<SemanticIndexProgressDto>(progress));
    }

    // The progress of the run of the session's repository (running, queued or the last one), or none.
    [HttpGet("index/progress")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetIndexProgress() =>
        Ok(new Result<SemanticIndexProgressDto>(_jobs.GetIndexProgress(_repositories.ForUser(User).RepositoryId)));

    [HttpPost("index/cancel")]
    [Authorize(Roles = "Admin")]
    public IActionResult CancelIndex() =>
        Ok(new Result<bool>(_jobs.CancelIndex(_repositories.ForUser(User).RepositoryId)));

    // Searches by meaning in the session's repository (the search, or the filter with «Talks about…»): the most similar
    // notes, not paged. It brings the index up to date before, within the request and with its user (D8). Any user.
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SemanticSearchRequestDto request)
    {
        try
        {
            var resApi = await _service.SemanticSearch.SearchAsync(request, HttpContext.RequestAborted);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search at {dateTime}.", DateTime.Now);
            var resApi = new Result<SemanticSearchResultDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // The notes most similar to a note, not related to it yet (calculated, not saved). Any user.
    [HttpGet("notes/{noteId}/similar")]
    public async Task<IActionResult> GetSimilarNotes(Guid noteId)
    {
        try
        {
            var resApi = await _service.SemanticSearch.GetSimilarNotesAsync(noteId, HttpContext.RequestAborted);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSimilarNotes at {dateTime}.", DateTime.Now);
            var resApi = new Result<SemanticSearchResultDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // The search evaluation (KNote Atlas): the note of the test queries (created the first time), its queries and the
    // indexes to evaluate. The Web evaluates the queries one by one (short requests, with progress and cancellation) and
    // saves the report it builds (SearchEvaluationReport). ProjectManager, as the commands.
    [HttpGet("evaluation")]
    [Authorize(Roles = "ProjectManager, Admin")]
    public async Task<IActionResult> GetSearchEvaluation()
    {
        try
        {
            var resApi = await _service.SemanticSearch.GetSearchEvaluationAsync();
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSearchEvaluation at {dateTime}.", DateTime.Now);
            var resApi = new Result<SearchEvaluationSetupDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    [HttpPost("evaluation/query")]
    [Authorize(Roles = "ProjectManager, Admin")]
    public async Task<IActionResult> EvaluateQuery([FromBody] SearchEvaluationQueryRequestDto request)
    {
        try
        {
            var resApi = await _service.SemanticSearch.EvaluateQueryAsync(request, HttpContext.RequestAborted);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EvaluateQuery at {dateTime}.", DateTime.Now);
            var resApi = new Result<SearchEvaluationQueryResultDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    [HttpPost("evaluation/report")]
    [Authorize(Roles = "ProjectManager, Admin")]
    public async Task<IActionResult> SaveSearchEvaluationReport([FromBody] SearchEvaluationReportDto report)
    {
        try
        {
            var resApi = await _service.SemanticSearch.SaveSearchEvaluationReportAsync(report);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveSearchEvaluationReport at {dateTime}.", DateTime.Now);
            var resApi = new Result<NoteDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // Removes the index of another model (?key=, the index key: it has ':' and '@').
    [HttpDelete("index")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteModelIndex([FromQuery] string key)
    {
        try
        {
            var resApi = await _service.SemanticSearch.DeleteModelIndexAsync(key);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteModelIndex at {dateTime}.", DateTime.Now);
            var resApi = new Result();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }
}
