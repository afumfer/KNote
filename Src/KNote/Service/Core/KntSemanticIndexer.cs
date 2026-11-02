using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository;
using Microsoft.Extensions.Logging;

namespace KNote.Service.Core;

/// <summary>
/// The engine of the semantic index (KNote Atlas), independent of the host: it calculates the fragments of the pending
/// notes of a repository (NoteChunkBuilder) and their vectors with this installation's generator of the repository's
/// embedding model, and replaces their rows. It runs with the IKntService it receives: its user only authorizes the
/// command, every note (not the system ones) is indexed, the search applies the permissions.
/// <list type="bullet">
/// <item>Per note: read through the Service (the note with its attributes, its tasks and its resources without their
/// content), HTML converted to Markdown, the vectors of the previous rows of the same source and hash reused (not the
/// ones written before a rebuild started), the others asked to the provider in one call, cut and normalized
/// (EmbeddingVector.Normalize). IndexedDateTime is when the note was read: a change while it is indexed leaves it
/// pending.</item>
/// <item>Canceled between notes: what is done is kept. A note that fails is counted, its error goes to the progress
/// and it is tried again in the next run, MaxFailuresPerProcess times per process.</item>
/// <item>One run at a time per database in a process: a run waits for the one in progress; a refresh (before a search)
/// does nothing while another one runs.</item>
/// </list>
/// Only the runs that keep the state (RunIndexAsync, Admin) change AI_INDEX_STATE: building, ready, rebuilding.
/// </summary>
internal static class KntSemanticIndexer
{
    public const int MaxFailuresPerProcess = 3;
    private const int PendingPerQuery = 200;
    // A pace is measured from this many notes on.
    private const int NotesForPace = 5;

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> RunLocks = new();
    private static readonly ConcurrentDictionary<string, int> Failures = new();

    public sealed class Options
    {
        public bool Rebuild { get; init; }
        public bool KeepState { get; init; }
        public bool SkipIfBusy { get; init; }
        public int? MaxNotes { get; init; }
        public TimeSpan? MaxDuration { get; init; }
        public IProgress<SemanticIndexProgressDto> Progress { get; init; }
        public CancellationToken CancellationToken { get; init; }
    }

    public static async Task<Result<SemanticIndexProgressDto>> RunAsync(IKntService service, Options options)
    {
        var result = new Result<SemanticIndexProgressDto>();
        var progress = new SemanticIndexProgressDto
        {
            RepositoryName = service.RepositoryRef?.Name,
            Rebuild = options.Rebuild,
            Phase = EnumSemanticIndexPhase.Preparing,
            StartedDateTime = DateTime.Now
        };
        result.Entity = progress;

        var runLock = RunLocks.GetOrAdd(service.RepositoryRef?.ConnectionString ?? "", _ => new SemaphoreSlim(1, 1));
        if (options.SkipIfBusy)
        {
            if (!await runLock.WaitAsync(0))
            {
                progress.Phase = EnumSemanticIndexPhase.Completed;
                progress.Message = "The semantic index is being updated by another run.";
                return result;
            }
        }
        else
            await runLock.WaitAsync(options.CancellationToken);

        try
        {
            Report(options, progress);
            await RunLockedAsync(service, options, progress, result);
        }
        catch (OperationCanceledException) when (options.CancellationToken.IsCancellationRequested)
        {
            progress.Phase = EnumSemanticIndexPhase.Canceled;
        }
        catch (Exception ex)
        {
            service.Logger?.LogError(ex, "Semantic index of {repository}", service.RepositoryRef?.Name);
            progress.Phase = EnumSemanticIndexPhase.Failed;
            progress.Message = ex.Message;
            result.AddErrorMessage(ex.Message);
        }
        finally
        {
            runLock.Release();
        }

        Report(options, progress);
        return result;
    }

    private static async Task RunLockedAsync(IKntService service, Options options, SemanticIndexProgressDto progress,
        Result<SemanticIndexProgressDto> result)
    {
        var repository = service.Repository.SemanticIndex;

        var resModels = await service.SystemValues.GetRepositoryAiModelsAsync();
        if (!resModels.IsValid)
        {
            Fail(progress, result, resModels.ErrorMessage);
            return;
        }
        var models = resModels.Entity;
        var indexKey = models.GetIndexKey();
        if (indexKey == null)
        {
            Fail(progress, result, "The repository has no embedding model (KNote Atlas, repository's AI models).");
            return;
        }
        var generator = service.EmbeddingGenerators?.Find(models.EmbeddingModel);
        if (generator == null)
        {
            Fail(progress, result, $"This installation has no embeddings provider with the model {models.EmbeddingModel} (AI providers).");
            return;
        }
        progress.IndexKey = indexKey;

        var excluded = await KntSystemNotes.GetSystemNoteTypeIdsAsync(service.Repository);
        var state = await KntSemanticIndexState.ReadAsync(service);
        if (state?.IndexKey != indexKey)
            state = new SemanticIndexStateDto { IndexKey = indexKey, Status = EnumSemanticIndexStatus.None };
        var building = state.Status != EnumSemanticIndexStatus.Ready || state.RebuildSince != null || options.Rebuild;

        if (options.KeepState)
        {
            if (options.Rebuild)
            {
                Check(await repository.MarkModelStaleAsync(indexKey));
                state.RebuildSince = DateTime.Now;
            }
            if (state.Status == EnumSemanticIndexStatus.None)
                state.Status = EnumSemanticIndexStatus.Building;
            Check(await KntSemanticIndexState.WriteAsync(service, state));
        }

        var summary = (await repository.GetIndexSummaryAsync(indexKey, excluded)).Entity;
        progress.NotesTotal = options.MaxNotes is int maxNotes ? Math.Min(summary.PendingNotes, maxNotes) : summary.PendingNotes;
        progress.Phase = EnumSemanticIndexPhase.Indexing;
        Report(options, progress);

        var run = new Run(service, repository, generator, indexKey, models.EmbeddingDimensions, state.RebuildSince);
        var clock = Stopwatch.StartNew();
        var processed = new HashSet<Guid>();
        var finished = false;

        while (!finished)
        {
            var resPending = await repository.GetPendingNoteIdsAsync(indexKey, PendingPerQuery + processed.Count, excluded);
            Check(resPending);
            var noteIds = resPending.Entity.Where(id => !processed.Contains(id)).Take(PendingPerQuery).ToList();
            if (noteIds.Count == 0)
            {
                finished = true;
                break;
            }

            foreach (var noteId in noteIds)
            {
                options.CancellationToken.ThrowIfCancellationRequested();
                if (options.MaxNotes is int max && progress.NotesDone >= max
                    || options.MaxDuration is TimeSpan maxDuration && clock.Elapsed >= maxDuration)
                {
                    progress.Phase = EnumSemanticIndexPhase.Completed;
                    Report(options, progress, clock, state);
                    return;
                }

                processed.Add(noteId);
                var failureKey = $"{service.RepositoryRef?.ConnectionString}|{indexKey}|{noteId}";
                if (Failures.TryGetValue(failureKey, out var failures) && failures >= MaxFailuresPerProcess)
                {
                    progress.NotesTotal = Math.Max(progress.NotesDone, progress.NotesTotal - 1);
                    continue;
                }

                try
                {
                    var (embedded, reused) = await run.IndexNoteAsync(noteId, options.CancellationToken);
                    progress.ChunksEmbedded += embedded;
                    progress.ChunksReused += reused;
                    Failures.TryRemove(failureKey, out _);
                }
                catch (OperationCanceledException) when (options.CancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Failures.AddOrUpdate(failureKey, 1, (_, count) => count + 1);
                    service.Logger?.LogWarning(ex, "Semantic index of the note {noteId} of {repository}", noteId, service.RepositoryRef?.Name);
                    progress.ErrorsCount++;
                    if (progress.Errors.Count < SemanticIndexProgressDto.MaxErrors)
                        progress.Errors.Add($"{run.Describe(noteId)}: {ex.Message}");
                }

                progress.NotesDone++;
                progress.NotesTotal = Math.Max(progress.NotesTotal, progress.NotesDone);
                Report(options, progress, clock, state);
            }
        }

        progress.Phase = EnumSemanticIndexPhase.Completed;
        Report(options, progress, clock, state);

        if (options.KeepState)
        {
            if (building)
            {
                state.Status = EnumSemanticIndexStatus.Ready;
                state.CompletedDateTime = DateTime.Now;
                state.LastBuildSeconds = clock.Elapsed.TotalSeconds;
                state.RebuildSince = null;
            }
            if (progress.NotesDone >= NotesForPace && clock.Elapsed.TotalMinutes > 0)
                state.NotesPerMinute = progress.NotesDone / clock.Elapsed.TotalMinutes;
            Check(await KntSemanticIndexState.WriteAsync(service, state));
        }
    }

    #region Marking notes as changed

    /// <summary>
    /// Marks the rows of a note to be calculated again, after a command changed it. A failure does not fail the
    /// command: it is logged (the note's ModificationDateTime still makes it pending, if it changed).
    /// </summary>
    public static Task MarkStaleAsync(IKntService service, Guid noteId) =>
        MarkStaleAsync(service, r => r.MarkStaleAsync(noteId));

    /// <summary>The same for the notes of a folder and of its subfolders.</summary>
    public static Task MarkFolderStaleAsync(IKntService service, Guid folderId) =>
        MarkStaleAsync(service, async r =>
        {
            var resFolders = await service.Repository.Folders.GetAllAsync();
            if (!resFolders.IsValid)
                return resFolders;

            var children = resFolders.Entity.ToLookup(f => f.ParentId);
            var folderIds = new List<Guid> { folderId };
            for (var i = 0; i < folderIds.Count; i++)
                folderIds.AddRange(children[folderIds[i]].Select(f => f.FolderId).Where(id => !folderIds.Contains(id)));
            return await r.MarkStaleByFoldersAsync(folderIds);
        });

    public static async Task MarkStaleAsync(IKntService service, Func<IKntSemanticIndexRepository, Task<Result>> mark)
    {
        try
        {
            var result = await mark(service.Repository.SemanticIndex);
            if (!result.IsValid)
                service.Logger?.LogWarning("Semantic index: the notes could not be marked as changed: {error}", result.ErrorMessage);
        }
        catch (Exception ex)
        {
            service.Logger?.LogWarning(ex, "Semantic index: the notes could not be marked as changed.");
        }
    }

    #endregion

    #region Private methods

    private static void Fail(SemanticIndexProgressDto progress, ResultBase result, string message)
    {
        progress.Phase = EnumSemanticIndexPhase.Failed;
        progress.Message = message;
        result.AddErrorMessage(message);
    }

    private static void Check(ResultBase result)
    {
        if (!result.IsValid)
            throw new KntServiceException(result.ErrorMessage);
    }

    private static void Report(Options options, SemanticIndexProgressDto progress, Stopwatch clock = null, SemanticIndexStateDto state = null)
    {
        if (clock != null)
        {
            progress.ElapsedSeconds = clock.Elapsed.TotalSeconds;
            var left = progress.NotesTotal - progress.NotesDone;
            double? perSecond = progress.NotesDone >= NotesForPace && clock.Elapsed.TotalSeconds > 0
                ? progress.NotesDone / clock.Elapsed.TotalSeconds
                : state?.NotesPerMinute / 60;
            progress.RemainingSeconds = progress.IsFinished || left <= 0 ? 0 : perSecond > 0 ? left / perSecond : null;
        }
        options.Progress?.Report(progress.Copy());
    }

    // What a run needs per note, and the paths of the folders it has read.
    private sealed class Run
    {
        private readonly IKntService _service;
        private readonly IKntSemanticIndexRepository _repository;
        private readonly IKntEmbeddingGenerator _generator;
        private readonly string _indexKey;
        private readonly int? _dimensions;
        private readonly DateTime? _rebuildSince;
        private readonly Dictionary<Guid, string> _folderPaths = new();
        private readonly Dictionary<Guid, string> _topics = new();

        public Run(IKntService service, IKntSemanticIndexRepository repository, IKntEmbeddingGenerator generator,
            string indexKey, int? dimensions, DateTime? rebuildSince)
        {
            _service = service;
            _repository = repository;
            _generator = generator;
            _indexKey = indexKey;
            _dimensions = dimensions;
            _rebuildSince = rebuildSince;
        }

        public string Describe(Guid noteId) => _topics.TryGetValue(noteId, out var topic) ? topic : noteId.ToString();

        // The fragments of the note calculated and reused.
        public async Task<(int Embedded, int Reused)> IndexNoteAsync(Guid noteId, CancellationToken cancellationToken)
        {
            var readAt = DateTime.Now;
            var resNote = await _service.Notes.GetAsync(noteId);
            if (resNote.Entity == null)
                return (0, 0); // Deleted meanwhile: its rows went with it.
            Check(resNote);
            var note = resNote.Entity;
            _topics[noteId] = $"#{note.NoteNumber} {note.Topic}";

            var resTasks = await _service.Notes.GetNoteTasksAsync(noteId);
            Check(resTasks);
            var resResources = await _service.Notes.GetResourcesInfoAsync(noteId);
            Check(resResources);

            var description = note.Description ?? "";
            if (string.Equals(note.GetContentTypeExt().ForDescription, "html", StringComparison.OrdinalIgnoreCase))
                description = _service.Notes.UtilHtmlToMarkdown(description);

            var source = NoteChunkSource.From(note, resTasks.Entity, resResources.Entity, await FolderPathAsync(note.FolderId), description);
            var set = NoteChunkBuilder.Build(source);

            var resPrevious = await _repository.GetNoteChunksAsync(noteId, _indexKey);
            Check(resPrevious);
            var reusable = resPrevious.Entity
                .Where(r => _rebuildSince == null || r.IndexedDateTime >= _rebuildSince)
                .GroupBy(r => (r.SourceType, r.SourceId, r.ContentHash))
                .ToDictionary(g => g.Key, g => g.First().Vector);

            var missing = set.Chunks.Where(c => !reusable.ContainsKey((c.SourceType, c.SourceId, c.ContentHash))).ToList();
            var vectors = new Dictionary<NoteChunkDraft, float[]>();
            if (missing.Count > 0)
            {
                var generated = await _generator.GenerateAsync(missing.Select(c => c.TextForEmbedding).ToList(), cancellationToken);
                if (generated == null || generated.Count != missing.Count)
                    throw new KntServiceException($"The model {_generator.Model} gave {generated?.Count ?? 0} vectors for {missing.Count} texts.");
                for (var i = 0; i < missing.Count; i++)
                    vectors[missing[i]] = EmbeddingVector.Normalize(generated[i], _dimensions);
            }

            var rows = set.Chunks.Select(c => new NoteChunkDto
            {
                SourceType = c.SourceType,
                SourceId = c.SourceId,
                ChunkIndex = c.ChunkIndex,
                Content = c.Content,
                ContentHash = c.ContentHash,
                SourceHash = set.SourceHash,
                Vector = vectors.TryGetValue(c, out var vector) ? vector : reusable[(c.SourceType, c.SourceId, c.ContentHash)],
                IndexedDateTime = readAt
            }).ToList();
            Check(await _repository.ReplaceNoteChunksAsync(noteId, _indexKey, rows));

            return (missing.Count, set.Chunks.Count - missing.Count);
        }

        private async Task<string> FolderPathAsync(Guid folderId)
        {
            if (!_folderPaths.TryGetValue(folderId, out var path))
            {
                var resPath = await _service.Folders.GetPathAsync(folderId);
                path = resPath.IsValid ? resPath.Entity : null;
                _folderPaths[folderId] = path;
            }
            return path;
        }
    }

    #endregion
}
