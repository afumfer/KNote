using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

/// <summary>
/// The summary of a long document of ClientWin (KNote Atlas, stage 2, CU-43, D18), one at a time in the app and never with another
/// run of KNote Atlas (Store.AtlasJobRunning: one Ollama). Like ResourceProcessingJob, it belongs to the Store, not to the window
/// that starts it; its progress is published on Store.Events (DocumentProcessingProgressChanged, on the UI thread, a few times a
/// second at most). It runs in the thread pool with the repository's service (the app's user and role). Canceled when the app
/// closes: nothing is kept. When it keeps the summary in a repository with a semantic index, it starts the update of the index.
/// </summary>
public class DocumentProcessingJob
{
    private static readonly TimeSpan PublishInterval = TimeSpan.FromMilliseconds(250);

    private readonly DomainEventBus _events;
    private readonly SemanticIndexJob _semanticIndex;
    private readonly Func<bool> _otherRunning;
    private readonly object _lock = new();
    private SynchronizationContext _uiContext;
    private CancellationTokenSource _cancellation;
    private DocumentProcessProgressDto _progress;
    private DateTime _lastPublished;

    public DocumentProcessingJob(DomainEventBus events, SemanticIndexJob semanticIndex, Func<bool> otherRunning)
    {
        _events = events;
        _semanticIndex = semanticIndex;
        _otherRunning = otherRunning;
    }

    public bool IsRunning { get; private set; }

    // The repository of the run (also when it has ended), and its name.
    public IKntService Service { get; private set; }

    public string RepositoryName { get; private set; }

    // The last progress of the run (also when it has ended), or null before the first one.
    public DocumentProcessProgressDto Progress
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
            return progress.Phase is EnumDocumentProcessPhase.Reading or EnumDocumentProcessPhase.Joining or EnumDocumentProcessPhase.Writing
                ? $"Summarizing {progress.FileName}: {progress.CallsDone}/{progress.CallsTotal}"
                : $"Summary of {progress.FileName ?? "a document"}: {progress.Phase.ToString().ToLowerInvariant()}";
        }
    }

    // The task of the run (for the tests).
    internal Task Completion { get; private set; } = Task.CompletedTask;

    // The run itself (the tests give their own; by default, AiDocumentProcessor with a client of the provider).
    internal Func<IKntService, AiProviderRef, DocumentProcessRequestDto, IProgress<DocumentProcessProgressDto>, CancellationToken,
        Task<Result<DocumentProcessProgressDto>>> Runner { get; set; } = (service, provider, request, progress, cancellationToken) =>
            AiDocumentProcessor.SummarizeAsync(service, p => AiChatClientFactory.Create(p), provider, request, progress, service.Logger, cancellationToken);

    public bool IsOf(IKntService service) => Service != null && service != null && Service.IdServiceRef == service.IdServiceRef;

    /// <summary>
    /// Starts the summary of a document of the repository of the service with this model: false if there is a run of KNote Atlas
    /// going on (of any repository). hasSemanticIndex: whether to update the index when it ends.
    /// </summary>
    public bool Start(IKntService service, string repositoryName, AiProviderRef provider, DocumentProcessRequestDto request, bool hasSemanticIndex)
    {
        if (IsRunning || _otherRunning())
            return false;

        _uiContext = SynchronizationContext.Current;
        _cancellation = new CancellationTokenSource();
        Service = service;
        RepositoryName = repositoryName;
        IsRunning = true;
        lock (_lock)
            _progress = new DocumentProcessProgressDto
            {
                RepositoryName = repositoryName, ResourceId = request.ResourceId, Model = provider?.Model, Phase = EnumDocumentProcessPhase.Preparing
            };
        Publish(force: true);

        var cancellation = _cancellation;
        Completion = Task.Run(() => RunAsync(service, provider, request, hasSemanticIndex, cancellation));
        return true;
    }

    public void Cancel() => _cancellation?.Cancel();

    private async Task RunAsync(IKntService service, AiProviderRef provider, DocumentProcessRequestDto request, bool hasSemanticIndex,
        CancellationTokenSource cancellation)
    {
        DocumentProcessProgressDto final;
        try
        {
            var result = await Runner(service, provider, request, new Reporter(this), cancellation.Token);
            final = result.Entity ?? new DocumentProcessProgressDto { Phase = EnumDocumentProcessPhase.Failed, Message = result.ErrorMessage };
            if (!result.IsValid && final.Phase != EnumDocumentProcessPhase.Failed)
            {
                final.Phase = EnumDocumentProcessPhase.Failed;
                final.Message = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            final = new DocumentProcessProgressDto { Phase = EnumDocumentProcessPhase.Failed, Message = ex.InnerException?.Message ?? ex.Message };
        }

        final.RepositoryName = RepositoryName;
        final.ResourceId = request.ResourceId;
        lock (_lock)
            _progress = final;
        IsRunning = false;
        cancellation.Dispose();

        // The new note, for the search by meaning (on the UI thread, as a click).
        if (final.Phase == EnumDocumentProcessPhase.Completed && hasSemanticIndex)
            Post(() => _semanticIndex.Start(service, RepositoryName, rebuild: false));
        Publish(force: true);
    }

    private void Report(DocumentProcessProgressDto progress)
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

        Post(() => _events.Publish(new DocumentProcessingProgressChanged(this)));
    }

    private void Post(Action action)
    {
        if (_uiContext != null)
            _uiContext.Post(_ => action(), null);
        else
            action();
    }

    private sealed class Reporter : IProgress<DocumentProcessProgressDto>
    {
        private readonly DocumentProcessingJob _job;
        public Reporter(DocumentProcessingJob job) => _job = job;
        public void Report(DocumentProcessProgressDto value) => _job.Report(value);
    }
}
