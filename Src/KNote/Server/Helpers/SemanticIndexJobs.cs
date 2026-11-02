using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Helpers;

public static class KntSemanticIndex
{
    public static IServiceCollection KntAddSemanticIndex(this IServiceCollection services)
    {
        services.AddSingleton<SemanticIndexJobs>();
        services.AddHostedService(provider => provider.GetRequiredService<SemanticIndexJobs>());
        return services;
    }
}

/// <summary>
/// The long runs of the semantic index of the Server's repositories (KNote Atlas: create, update or rebuild an index),
/// in one queue for the whole Server and one at a time, whatever the repository: with one Ollama, two runs at once
/// would only compete for the same model. One run per repository, running or queued: asking for another one gives the
/// one there is. Each run has its own IKntService of its repository, with the user who asked for it (the request's
/// service is gone when the request ends) and the Server's embedding models. A run that fails does not stop the queue.
/// The progress of the last run of each repository is kept (in memory) to be shown after it ends.
/// </summary>
public class SemanticIndexJobs : BackgroundService
{
    // A run of the indexer for a job (the tests give their own).
    public delegate Task<Result<SemanticIndexProgressDto>> Runner(SemanticIndexJob job, IProgress<SemanticIndexProgressDto> progress,
        CancellationToken cancellationToken);

    private readonly object _lock = new();
    private readonly List<SemanticIndexJob> _queue = new();
    private readonly Dictionary<Guid, SemanticIndexJob> _lastJobs = new();
    private readonly SemaphoreSlim _queued = new(0);
    private readonly Runner _runner;
    private readonly ILogger<SemanticIndexJobs> _logger;

    public SemanticIndexJobs(KntRepositories repositories, ILogger<SemanticIndexJobs> logger, IKntEmbeddingGenerators generators = null)
        : this((job, progress, cancellationToken) => RunWithServiceAsync(repositories, generators, logger, job, progress, cancellationToken), logger)
    {
    }

    public SemanticIndexJobs(Runner runner, ILogger<SemanticIndexJobs> logger)
    {
        _runner = runner;
        _logger = logger;
    }

    /// <summary>Queues a run for the repository, or gives the one it already has (running or queued).</summary>
    public SemanticIndexProgressDto Enqueue(Guid repositoryId, string repositoryName, string userName, bool rebuild)
    {
        lock (_lock)
        {
            if (_lastJobs.TryGetValue(repositoryId, out var current) && !current.Progress.IsFinished)
                return Snapshot(current);

            var job = new SemanticIndexJob(repositoryId, userName, rebuild)
            {
                Progress = new SemanticIndexProgressDto
                {
                    RepositoryId = repositoryId,
                    RepositoryName = repositoryName,
                    RequestedBy = userName,
                    Rebuild = rebuild,
                    Phase = EnumSemanticIndexPhase.Queued
                }
            };
            _queue.Add(job);
            _lastJobs[repositoryId] = job;
            _queued.Release();
            return Snapshot(job);
        }
    }

    /// <summary>The progress of the repository's run (running, queued or the last one finished), or null.</summary>
    public SemanticIndexProgressDto GetProgress(Guid repositoryId)
    {
        lock (_lock)
            return _lastJobs.TryGetValue(repositoryId, out var job) ? Snapshot(job) : null;
    }

    /// <summary>Whether the repository has a run running or queued (the search does not bring the index up to date then).</summary>
    public bool IsBusy(Guid repositoryId)
    {
        lock (_lock)
            return _lastJobs.TryGetValue(repositoryId, out var job) && !job.Progress.IsFinished;
    }

    /// <summary>Cancels the repository's run: a queued one leaves the queue, a running one stops after its note.</summary>
    public bool Cancel(Guid repositoryId)
    {
        lock (_lock)
        {
            if (!_lastJobs.TryGetValue(repositoryId, out var job) || job.Progress.IsFinished)
                return false;

            if (_queue.Remove(job))
                job.Progress.Phase = EnumSemanticIndexPhase.Canceled;
            else
                job.Cancellation.Cancel();
            return true;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _queued.WaitAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            SemanticIndexJob job;
            lock (_lock)
            {
                if (_queue.Count == 0)
                    continue;
                job = _queue[0];
                _queue.RemoveAt(0);
                job.Progress.Phase = EnumSemanticIndexPhase.Preparing;
            }

            await RunAsync(job, stoppingToken);
        }
    }

    private async Task RunAsync(SemanticIndexJob job, CancellationToken stoppingToken)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(job.Cancellation.Token, stoppingToken);
        try
        {
            var progress = new JobProgress(this, job);
            var result = await _runner(job, progress, cancellation.Token);
            progress.Report(result.Entity ?? Failed(result.ErrorMessage));
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            Update(job, p => p.Phase = EnumSemanticIndexPhase.Canceled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Semantic index of the repository {repositoryId}", job.RepositoryId);
            Update(job, p =>
            {
                p.Phase = EnumSemanticIndexPhase.Failed;
                p.Message = ex.Message;
            });
        }
    }

    private static async Task<Result<SemanticIndexProgressDto>> RunWithServiceAsync(KntRepositories repositories,
        IKntEmbeddingGenerators generators, ILogger logger, SemanticIndexJob job, IProgress<SemanticIndexProgressDto> progress,
        CancellationToken cancellationToken)
    {
        var repository = repositories.Find(job.RepositoryId);
        if (repository == null)
        {
            var result = new Result<SemanticIndexProgressDto>(Failed("The repository is no longer in this Server."));
            result.AddErrorMessage(result.Entity.Message);
            return result;
        }

        using var service = repositories.CreateService(repository, job.UserName, null);
        service.EmbeddingGenerators = generators;
        service.Logger = logger;
        return await service.SemanticSearch.RunIndexAsync(new SemanticIndexRunRequest
        {
            Rebuild = job.Rebuild,
            Progress = progress,
            CancellationToken = cancellationToken
        });
    }

    private static SemanticIndexProgressDto Failed(string message) =>
        new() { Phase = EnumSemanticIndexPhase.Failed, Message = message };

    private void Update(SemanticIndexJob job, Action<SemanticIndexProgressDto> update)
    {
        lock (_lock)
            update(job.Progress);
    }

    // A copy of the job's progress, with its position in the queue (1: the next one; 0: running or finished).
    private SemanticIndexProgressDto Snapshot(SemanticIndexJob job)
    {
        var progress = job.Progress.Copy();
        progress.QueuePosition = _queue.IndexOf(job) + 1;
        return progress;
    }

    // What the indexer reports, kept with the job's own data (repository and user).
    private sealed class JobProgress : IProgress<SemanticIndexProgressDto>
    {
        private readonly SemanticIndexJobs _jobs;
        private readonly SemanticIndexJob _job;

        public JobProgress(SemanticIndexJobs jobs, SemanticIndexJob job)
        {
            _jobs = jobs;
            _job = job;
        }

        public void Report(SemanticIndexProgressDto value)
        {
            var progress = value.Copy();
            _jobs.Update(_job, current =>
            {
                progress.RepositoryId = current.RepositoryId;
                progress.RepositoryName = current.RepositoryName ?? progress.RepositoryName;
                progress.RequestedBy = current.RequestedBy;
                progress.Rebuild = current.Rebuild;
                _job.Progress = progress;
            });
        }
    }
}

public sealed class SemanticIndexJob
{
    public SemanticIndexJob(Guid repositoryId, string userName, bool rebuild)
    {
        RepositoryId = repositoryId;
        UserName = userName;
        Rebuild = rebuild;
    }

    public Guid RepositoryId { get; }
    public string UserName { get; }
    public bool Rebuild { get; }
    public CancellationTokenSource Cancellation { get; } = new();
    public SemanticIndexProgressDto Progress { get; set; }
}
