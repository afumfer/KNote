using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Core;

/// <summary>
/// The run of the semantic index of ClientWin (KNote Atlas: create, update or rebuild the index of a repository), one
/// at a time in the app. It belongs to the Store, not to the repository editor that starts it: the editor can be
/// closed and the app goes on being used; its progress is published on Store.Events (SemanticIndexProgressChanged, on
/// the UI thread, a few times a second at most) for the KNote Atlas tab and the status bar. It runs in the thread pool
/// with the repository's service, so with the app's user and role (the run is an Admin command). Canceled when the app
/// closes (what is done is kept).
/// </summary>
public class SemanticIndexJob
{
    private static readonly TimeSpan PublishInterval = TimeSpan.FromMilliseconds(250);

    private readonly DomainEventBus _events;
    private readonly object _lock = new();
    private SynchronizationContext _uiContext;
    private CancellationTokenSource _cancellation;
    private SemanticIndexProgressDto _progress;
    private DateTime _lastPublished;

    public SemanticIndexJob(DomainEventBus events)
    {
        _events = events;
    }

    public bool IsRunning { get; private set; }

    // The repository of the run (also when it has ended), and its name.
    public IKntService Service { get; private set; }

    public string RepositoryName { get; private set; }

    // The last progress of the run (also when it has ended), or null before the first one.
    public SemanticIndexProgressDto Progress
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
            return progress.Phase == EnumSemanticIndexPhase.Indexing
                ? $"Semantic index of {RepositoryName}: {progress.NotesDone}/{progress.NotesTotal}"
                : $"Semantic index of {RepositoryName}: {progress.Phase.ToString().ToLowerInvariant()}";
        }
    }

    // The task of the run (for the tests).
    internal Task Completion { get; private set; } = Task.CompletedTask;

    public bool IsOf(IKntService service) => Service != null && service != null && Service.IdServiceRef == service.IdServiceRef;

    /// <summary>Starts a run for the repository of the service: false if there is one running (of any repository).</summary>
    public bool Start(IKntService service, string repositoryName, bool rebuild)
    {
        if (IsRunning)
            return false;

        _uiContext = SynchronizationContext.Current;
        _cancellation = new CancellationTokenSource();
        Service = service;
        RepositoryName = repositoryName;
        IsRunning = true;
        lock (_lock)
            _progress = new SemanticIndexProgressDto { RepositoryName = repositoryName, Rebuild = rebuild, Phase = EnumSemanticIndexPhase.Preparing };
        Publish(force: true);

        var cancellation = _cancellation;
        Completion = Task.Run(() => RunAsync(service, rebuild, cancellation));
        return true;
    }

    public void Cancel() => _cancellation?.Cancel();

    private async Task RunAsync(IKntService service, bool rebuild, CancellationTokenSource cancellation)
    {
        SemanticIndexProgressDto final;
        try
        {
            var result = await service.SemanticSearch.RunIndexAsync(new SemanticIndexRunRequest
            {
                Rebuild = rebuild,
                Progress = new Reporter(this),
                CancellationToken = cancellation.Token
            });
            final = result.Entity ?? new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Failed, Message = result.ErrorMessage };
            if (!result.IsValid && final.Phase != EnumSemanticIndexPhase.Failed)
            {
                final.Phase = EnumSemanticIndexPhase.Failed;
                final.Message = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            final = new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Failed, Message = ex.InnerException?.Message ?? ex.Message };
        }

        final.RepositoryName = RepositoryName;
        lock (_lock)
            _progress = final;
        IsRunning = false;
        cancellation.Dispose();
        Publish(force: true);
    }

    private void Report(SemanticIndexProgressDto progress)
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

        if (_uiContext != null)
            _uiContext.Post(_ => _events.Publish(new SemanticIndexProgressChanged(this)), null);
        else
            _events.Publish(new SemanticIndexProgressChanged(this));
    }

    private sealed class Reporter : IProgress<SemanticIndexProgressDto>
    {
        private readonly SemanticIndexJob _job;
        public Reporter(SemanticIndexJob job) => _job = job;
        public void Report(SemanticIndexProgressDto value) => _job.Report(value);
    }
}
