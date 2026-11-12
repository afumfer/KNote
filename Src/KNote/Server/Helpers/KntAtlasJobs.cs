using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Ai;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Helpers;

public static class KntAtlasJobsExtensions
{
    public static IServiceCollection KntAddAtlasJobs(this IServiceCollection services)
    {
        services.AddSingleton<KntAtlasJobs>();
        services.AddHostedService(provider => provider.GetRequiredService<KntAtlasJobs>());
        return services;
    }
}

public enum EnumAtlasJobKind
{
    SemanticIndex,
    Resources
}

/// <summary>
/// The long runs of KNote Atlas in the Server's repositories (D11): the semantic index (create, update or rebuild) and
/// «Process pending resources» (stage 4), in one queue for the whole Server and one at a time, whatever the repository and
/// the kind: with one Ollama, two runs at once would only compete for the same model. One run per repository and kind,
/// running or queued: asking for another one gives the one there is. Each run has its own IKntService of its repository,
/// with the user who asked for it (the request's service is gone when the request ends). A run that fails does not stop
/// the queue. The progress of the last run of each repository and kind is kept (in memory) to be shown after it ends.
/// When a run of resources describes something in a repository with a semantic index, its update is queued.
/// </summary>
public class KntAtlasJobs : BackgroundService
{
    // A run of the indexer or of the resources for a job (the tests give their own).
    public delegate Task<Result<SemanticIndexProgressDto>> IndexRunner(SemanticIndexJob job, IProgress<SemanticIndexProgressDto> progress,
        CancellationToken cancellationToken);
    public delegate Task<Result<ResourceProcessingProgressDto>> ResourcesRunner(ResourceProcessingJob job,
        IProgress<ResourceProcessingProgressDto> progress, CancellationToken cancellationToken);

    private readonly object _lock = new();
    private readonly List<KntAtlasJob> _queue = new();
    private readonly Dictionary<(Guid, EnumAtlasJobKind), KntAtlasJob> _lastJobs = new();
    private readonly SemaphoreSlim _queued = new(0);
    private readonly IndexRunner _indexRunner;
    private readonly ResourcesRunner _resourcesRunner;
    private readonly ILogger<KntAtlasJobs> _logger;

    public KntAtlasJobs(KntRepositories repositories, IAiChatClientProvider chatClients, ILogger<KntAtlasJobs> logger,
        IKntEmbeddingGenerators generators = null, IKntDocumentReader documentReader = null)
    {
        _logger = logger;
        _documentReader = documentReader;
        _indexRunner = (job, progress, cancellationToken) => RunIndexWithServiceAsync(repositories, generators, job, progress, cancellationToken);
        _resourcesRunner = (job, progress, cancellationToken) =>
            RunResourcesWithServiceAsync(repositories, generators, chatClients, job, progress, cancellationToken);
    }

    private readonly IKntDocumentReader _documentReader;

    public KntAtlasJobs(IndexRunner indexRunner, ResourcesRunner resourcesRunner, ILogger<KntAtlasJobs> logger)
    {
        _indexRunner = indexRunner;
        _resourcesRunner = resourcesRunner;
        _logger = logger;
    }

    #region Semantic index

    /// <summary>Queues a run of the repository's index, or gives the one it already has (running or queued).</summary>
    public SemanticIndexProgressDto EnqueueIndex(Guid repositoryId, string repositoryName, string userName, bool rebuild) =>
        (SemanticIndexProgressDto)Enqueue(new SemanticIndexJob(repositoryId, userName, rebuild)
        {
            Progress = new SemanticIndexProgressDto
            {
                RepositoryId = repositoryId,
                RepositoryName = repositoryName,
                RequestedBy = userName,
                Rebuild = rebuild,
                Phase = EnumSemanticIndexPhase.Queued
            }
        });

    /// <summary>The progress of the repository's index run (running, queued or the last one finished), or null.</summary>
    public SemanticIndexProgressDto GetIndexProgress(Guid repositoryId) =>
        (SemanticIndexProgressDto)GetProgress(repositoryId, EnumAtlasJobKind.SemanticIndex);

    /// <summary>Whether the repository has an index run running or queued (the search does not bring the index up to date then).</summary>
    public bool IsIndexBusy(Guid repositoryId) => IsBusy(repositoryId, EnumAtlasJobKind.SemanticIndex);

    /// <summary>Cancels the repository's index run: a queued one leaves the queue, a running one stops after its note.</summary>
    public bool CancelIndex(Guid repositoryId) => Cancel(repositoryId, EnumAtlasJobKind.SemanticIndex);

    #endregion

    #region Resources

    /// <summary>Queues a run of «Process pending resources» with this model, or gives the one the repository already has.</summary>
    public ResourceProcessingProgressDto EnqueueResources(Guid repositoryId, string repositoryName, string userName,
        AiProviderRef provider, ResourceProcessingOptionsDto options) =>
        (ResourceProcessingProgressDto)Enqueue(new ResourceProcessingJob(repositoryId, userName, provider, options)
        {
            Progress = new ResourceProcessingProgressDto
            {
                RepositoryId = repositoryId,
                RepositoryName = repositoryName,
                RequestedBy = userName,
                Model = provider?.Model,
                Phase = EnumResourceProcessingPhase.Queued
            }
        });

    public ResourceProcessingProgressDto GetResourcesProgress(Guid repositoryId) =>
        (ResourceProcessingProgressDto)GetProgress(repositoryId, EnumAtlasJobKind.Resources);

    public bool IsResourcesBusy(Guid repositoryId) => IsBusy(repositoryId, EnumAtlasJobKind.Resources);

    /// <summary>Cancels the repository's run of resources: a queued one leaves the queue, a running one stops at once.</summary>
    public bool CancelResources(Guid repositoryId) => Cancel(repositoryId, EnumAtlasJobKind.Resources);

    #endregion

    private object Enqueue(KntAtlasJob job)
    {
        lock (_lock)
        {
            var key = (job.RepositoryId, job.Kind);
            if (_lastJobs.TryGetValue(key, out var current) && !current.IsFinished)
                return Snapshot(current);

            _queue.Add(job);
            _lastJobs[key] = job;
            _queued.Release();
            return Snapshot(job);
        }
    }

    private object GetProgress(Guid repositoryId, EnumAtlasJobKind kind)
    {
        lock (_lock)
            return _lastJobs.TryGetValue((repositoryId, kind), out var job) ? Snapshot(job) : null;
    }

    private bool IsBusy(Guid repositoryId, EnumAtlasJobKind kind)
    {
        lock (_lock)
            return _lastJobs.TryGetValue((repositoryId, kind), out var job) && !job.IsFinished;
    }

    private bool Cancel(Guid repositoryId, EnumAtlasJobKind kind)
    {
        lock (_lock)
        {
            if (!_lastJobs.TryGetValue((repositoryId, kind), out var job) || job.IsFinished)
                return false;

            if (_queue.Remove(job))
                job.MarkCanceled();
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

            KntAtlasJob job;
            lock (_lock)
            {
                if (_queue.Count == 0)
                    continue;
                job = _queue[0];
                _queue.RemoveAt(0);
                job.MarkPreparing();
            }

            await RunAsync(job, stoppingToken);
        }
    }

    private async Task RunAsync(KntAtlasJob job, CancellationToken stoppingToken)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(job.Cancellation.Token, stoppingToken);
        try
        {
            switch (job)
            {
                case SemanticIndexJob index:
                {
                    var progress = new JobProgress<SemanticIndexProgressDto>(this, index, (current, value) =>
                    {
                        value.RepositoryId = current.RepositoryId;
                        value.RepositoryName = current.RepositoryName ?? value.RepositoryName;
                        value.RequestedBy = current.RequestedBy;
                        value.Rebuild = current.Rebuild;
                        index.Progress = value;
                    }, () => index.Progress, p => p.Copy());
                    var result = await _indexRunner(index, progress, cancellation.Token);
                    progress.Report(result.Entity ?? new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Failed, Message = result.ErrorMessage });
                    break;
                }
                case ResourceProcessingJob resources:
                {
                    var progress = new JobProgress<ResourceProcessingProgressDto>(this, resources, (current, value) =>
                    {
                        value.RepositoryId = current.RepositoryId;
                        value.RepositoryName = current.RepositoryName ?? value.RepositoryName;
                        value.RequestedBy = current.RequestedBy;
                        value.Model ??= current.Model;
                        resources.Progress = value;
                    }, () => resources.Progress, p => p.Copy());
                    var result = await _resourcesRunner(resources, progress, cancellation.Token);
                    progress.Report(result.Entity ?? new ResourceProcessingProgressDto { Phase = EnumResourceProcessingPhase.Failed, Message = result.ErrorMessage });
                    // Something the index has to read again: a description, a summary or the text of a document.
                    if (result.Entity is { } done && (done.PhotosDescribed > 0 || done.DocumentsSummarized > 0 || done.DocumentsRead > 0 || done.PagesTranscribed > 0)
                        && resources.HasSemanticIndex)
                        EnqueueIndex(resources.RepositoryId, resources.Progress.RepositoryName, resources.UserName, false);
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            Update(job.MarkCanceled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KNote Atlas, {kind} of the repository {repositoryId}", job.Kind, job.RepositoryId);
            Update(() => job.MarkFailed(ex.Message));
        }
    }

    private async Task<Result<SemanticIndexProgressDto>> RunIndexWithServiceAsync(KntRepositories repositories,
        IKntEmbeddingGenerators generators, SemanticIndexJob job, IProgress<SemanticIndexProgressDto> progress,
        CancellationToken cancellationToken)
    {
        var repository = repositories.Find(job.RepositoryId);
        if (repository == null)
            return NoRepository(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Failed });

        using var service = repositories.CreateService(repository, job.UserName, null);
        service.EmbeddingGenerators = generators;
        service.DocumentReader = _documentReader;
        service.Logger = _logger;
        return await service.SemanticSearch.RunIndexAsync(new SemanticIndexRunRequest
        {
            Rebuild = job.Rebuild,
            Progress = progress,
            CancellationToken = cancellationToken
        });
    }

    private async Task<Result<ResourceProcessingProgressDto>> RunResourcesWithServiceAsync(KntRepositories repositories,
        IKntEmbeddingGenerators generators, IAiChatClientProvider chatClients, ResourceProcessingJob job,
        IProgress<ResourceProcessingProgressDto> progress, CancellationToken cancellationToken)
    {
        var repository = repositories.Find(job.RepositoryId);
        if (repository == null)
            return NoRepository(new ResourceProcessingProgressDto { Phase = EnumResourceProcessingPhase.Failed });

        using var service = repositories.CreateService(repository, job.UserName, null);
        service.EmbeddingGenerators = generators;
        service.DocumentReader = _documentReader;
        service.Logger = _logger;
        // The roles of the commands apply, as in a request of the assistant.
        service.EnforceAuthorization = true;

        var index = await service.SemanticSearch.GetIndexStatusAsync();
        job.HasSemanticIndex = index.IsValid && index.Entity?.Status != EnumSemanticIndexStatus.None;

        var chatClient = chatClients.Create(job.Provider, []);
        return await AiResourceProcessor.ProcessAsync(service, chatClient, job.Provider, job.Options, progress, _logger, cancellationToken);
    }

    private static Result<T> NoRepository<T>(T failed)
    {
        var result = new Result<T>(failed);
        result.AddErrorMessage("The repository is no longer in this Server.");
        return result;
    }

    private void Update(Action update)
    {
        lock (_lock)
            update();
    }

    // A copy of the job's progress, with its position in the queue (1: the next one; 0: running or finished).
    private object Snapshot(KntAtlasJob job) => job.Snapshot(_queue.IndexOf(job) + 1);

    // What the runner reports, kept with the job's own data (repository and user).
    private sealed class JobProgress<T> : IProgress<T>
    {
        private readonly KntAtlasJobs _jobs;
        private readonly Action<T, T> _keep;
        private readonly Func<T> _current;
        private readonly Func<T, T> _copy;

        public JobProgress(KntAtlasJobs jobs, KntAtlasJob job, Action<T, T> keep, Func<T> current, Func<T, T> copy)
        {
            _jobs = jobs;
            _keep = keep;
            _current = current;
            _copy = copy;
        }

        public void Report(T value)
        {
            var progress = _copy(value);
            _jobs.Update(() => _keep(_current(), progress));
        }
    }
}

/// <summary>A run of KntAtlasJobs: its repository, the user who asked for it, its cancellation and its progress.</summary>
public abstract class KntAtlasJob
{
    protected KntAtlasJob(Guid repositoryId, string userName)
    {
        RepositoryId = repositoryId;
        UserName = userName;
    }

    public abstract EnumAtlasJobKind Kind { get; }
    public Guid RepositoryId { get; }
    public string UserName { get; }
    public CancellationTokenSource Cancellation { get; } = new();

    internal abstract bool IsFinished { get; }
    internal abstract void MarkPreparing();
    internal abstract void MarkCanceled();
    internal abstract void MarkFailed(string message);
    internal abstract object Snapshot(int queuePosition);
}

public sealed class SemanticIndexJob : KntAtlasJob
{
    public SemanticIndexJob(Guid repositoryId, string userName, bool rebuild) : base(repositoryId, userName)
    {
        Rebuild = rebuild;
    }

    public override EnumAtlasJobKind Kind => EnumAtlasJobKind.SemanticIndex;
    public bool Rebuild { get; }
    public SemanticIndexProgressDto Progress { get; set; }

    internal override bool IsFinished => Progress.IsFinished;
    internal override void MarkPreparing() => Progress.Phase = EnumSemanticIndexPhase.Preparing;
    internal override void MarkCanceled() => Progress.Phase = EnumSemanticIndexPhase.Canceled;

    internal override void MarkFailed(string message)
    {
        Progress.Phase = EnumSemanticIndexPhase.Failed;
        Progress.Message = message;
    }

    internal override object Snapshot(int queuePosition)
    {
        var progress = Progress.Copy();
        progress.QueuePosition = queuePosition;
        return progress;
    }
}

public sealed class ResourceProcessingJob : KntAtlasJob
{
    public ResourceProcessingJob(Guid repositoryId, string userName, AiProviderRef provider, ResourceProcessingOptionsDto options)
        : base(repositoryId, userName)
    {
        Provider = provider;
        Options = options;
    }

    public override EnumAtlasJobKind Kind => EnumAtlasJobKind.Resources;
    public AiProviderRef Provider { get; }
    public ResourceProcessingOptionsDto Options { get; }
    public ResourceProcessingProgressDto Progress { get; set; }
    // Whether the repository has a semantic index (its update is queued when the run describes something).
    public bool HasSemanticIndex { get; set; }

    internal override bool IsFinished => Progress.IsFinished;
    internal override void MarkPreparing() => Progress.Phase = EnumResourceProcessingPhase.Preparing;
    internal override void MarkCanceled() => Progress.Phase = EnumResourceProcessingPhase.Canceled;

    internal override void MarkFailed(string message)
    {
        Progress.Phase = EnumResourceProcessingPhase.Failed;
        Progress.Message = message;
    }

    internal override object Snapshot(int queuePosition)
    {
        var progress = Progress.Copy();
        progress.QueuePosition = queuePosition;
        return progress;
    }
}
