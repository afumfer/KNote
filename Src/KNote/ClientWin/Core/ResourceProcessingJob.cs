using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

/// <summary>
/// The run of «Process pending resources» of ClientWin (KNote Atlas, stage 4, D10/D11), one at a time in the app and never
/// with the semantic index's (Store.AtlasJobRunning: one Ollama). Like SemanticIndexJob, it belongs to the Store, not to the
/// window that starts it; its progress is published on Store.Events (ResourceProcessingProgressChanged, on the
/// UI thread, a few times a second at most). It runs in the thread pool with the repository's service, so with the app's
/// user and role (preparing the run is an Admin command). Canceled when the app closes (what is done is kept). When it
/// describes something in a repository with a semantic index, it starts the update of the index.
/// </summary>
public class ResourceProcessingJob
{
    private static readonly TimeSpan PublishInterval = TimeSpan.FromMilliseconds(250);

    private readonly DomainEventBus _events;
    private readonly SemanticIndexJob _semanticIndex;
    private readonly object _lock = new();
    private SynchronizationContext _uiContext;
    private CancellationTokenSource _cancellation;
    private ResourceProcessingProgressDto _progress;
    private DateTime _lastPublished;

    public ResourceProcessingJob(DomainEventBus events, SemanticIndexJob semanticIndex)
    {
        _events = events;
        _semanticIndex = semanticIndex;
    }

    public bool IsRunning { get; private set; }

    // The repository of the run (also when it has ended), and its name.
    public IKntService Service { get; private set; }

    public string RepositoryName { get; private set; }

    // The last progress of the run (also when it has ended), or null before the first one.
    public ResourceProcessingProgressDto Progress
    {
        get { lock (_lock) return _progress?.Copy(); }
    }

    // The run of the main window's status bar: null when there is none.
    public string StatusBarText
    {
        get
        {
            var progress = Progress;
            if (!IsRunning || progress == null)
                return null;
            if (progress.Phase != EnumResourceProcessingPhase.Processing)
                return $"Resources of {RepositoryName}: {progress.Phase.ToString().ToLowerInvariant()}";
            var (done, total) = ResourceProcessingTexts.Bar(progress);
            var what = progress.Step switch
            {
                EnumResourceProcessingStep.ReadingDocuments => "Reading documents",
                EnumResourceProcessingStep.Pages => "Transcribing scanned pages",
                EnumResourceProcessingStep.Documents => "Summarizing documents",
                _ => "Describing photos"
            };
            return $"{what} of {RepositoryName}: {done}/{total}";
        }
    }

    // The task of the run (for the tests).
    internal Task Completion { get; private set; } = Task.CompletedTask;

    // The run itself (the tests give their own; by default, AiResourceProcessor with a client of the provider).
    internal Func<IKntService, AiProviderRef, ResourceProcessingOptionsDto, IProgress<ResourceProcessingProgressDto>, CancellationToken,
        Task<Result<ResourceProcessingProgressDto>>> Runner { get; set; } = (service, provider, options, progress, cancellationToken) =>
            AiResourceProcessor.ProcessAsync(service, AiChatClientFactory.Create(provider), provider, options, progress, service.Logger, cancellationToken);

    public bool IsOf(IKntService service) => Service != null && service != null && Service.IdServiceRef == service.IdServiceRef;

    /// <summary>
    /// Starts a run for the repository of the service with this model: false if there is a run of KNote Atlas going on (of any
    /// repository). hasSemanticIndex: whether to update the index when it ends.
    /// </summary>
    public bool Start(IKntService service, string repositoryName, AiProviderRef provider, ResourceProcessingOptionsDto options, bool hasSemanticIndex)
    {
        if (IsRunning || _semanticIndex.IsRunning)
            return false;

        _uiContext = SynchronizationContext.Current;
        _cancellation = new CancellationTokenSource();
        Service = service;
        RepositoryName = repositoryName;
        IsRunning = true;
        lock (_lock)
            _progress = new ResourceProcessingProgressDto
            {
                RepositoryName = repositoryName, Model = provider?.Model, Phase = EnumResourceProcessingPhase.Preparing
            };
        Publish(force: true);

        var cancellation = _cancellation;
        Completion = Task.Run(() => RunAsync(service, provider, options, hasSemanticIndex, cancellation));
        return true;
    }

    public void Cancel() => _cancellation?.Cancel();

    private async Task RunAsync(IKntService service, AiProviderRef provider, ResourceProcessingOptionsDto options, bool hasSemanticIndex,
        CancellationTokenSource cancellation)
    {
        ResourceProcessingProgressDto final;
        try
        {
            var result = await Runner(service, provider, options, new Reporter(this), cancellation.Token);
            final = result.Entity ?? new ResourceProcessingProgressDto { Phase = EnumResourceProcessingPhase.Failed, Message = result.ErrorMessage };
            if (!result.IsValid && final.Phase != EnumResourceProcessingPhase.Failed)
            {
                final.Phase = EnumResourceProcessingPhase.Failed;
                final.Message = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            final = new ResourceProcessingProgressDto { Phase = EnumResourceProcessingPhase.Failed, Message = ex.InnerException?.Message ?? ex.Message };
        }

        final.RepositoryName = RepositoryName;
        lock (_lock)
            _progress = final;
        IsRunning = false;
        cancellation.Dispose();

        // The descriptions written change the notes' fragments: the index is brought up to date (on the UI thread, as a click).
        // Something the index has to read again: a description, a summary or the text of a document.
        if ((final.PhotosDescribed > 0 || final.DocumentsSummarized > 0 || final.DocumentsRead > 0 || final.PagesTranscribed > 0) && hasSemanticIndex)
            Post(() => _semanticIndex.Start(service, RepositoryName, rebuild: false));
        Publish(force: true);
    }

    private void Report(ResourceProcessingProgressDto progress)
    {
        progress.RepositoryName = RepositoryName;
        lock (_lock)
            _progress = progress;
        Publish(force: false);
    }

    private void Publish(bool force)
    {
        if (!force && DateTime.UtcNow - _lastPublished < PublishInterval)
            return;
        _lastPublished = DateTime.UtcNow;

        Post(() => _events.Publish(new ResourceProcessingProgressChanged(this)));
    }

    private void Post(Action action)
    {
        if (_uiContext != null)
            _uiContext.Post(_ => action(), null);
        else
            action();
    }

    private sealed class Reporter : IProgress<ResourceProcessingProgressDto>
    {
        private readonly ResourceProcessingJob _job;
        public Reporter(ResourceProcessingJob job) => _job = job;
        public void Report(ResourceProcessingProgressDto value) => _job.Report(value);
    }
}
