using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>
/// What a fragment of the semantic index comes from (NoteChunks.SourceType; persisted by value, never renumber).
/// </summary>
public enum NoteChunkSourceType
{
    Data = 0,
    Text = 1,
    Task = 2,
    Resource = 3
}

/// <summary>
/// A row of the semantic index (NoteChunks, IKntSemanticIndexRepository): a fragment of a note (NoteChunkBuilder) with
/// its vector, calculated with Model. SourceId is the task or the resource of the fragment, Guid.Empty for the note's
/// own fragments (data and text). Vector is null where only the content is read (GetChunkContentsAsync).
/// </summary>
public class NoteChunkDto
{
    public Guid NoteChunkId { get; set; }
    public Guid NoteId { get; set; }
    public NoteChunkSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; }
    public string ContentHash { get; set; }
    public string SourceHash { get; set; }
    public bool Stale { get; set; }
    public string Model { get; set; }
    public int Dimensions { get; set; }
    public float[] Vector { get; set; }
    public DateTime IndexedDateTime { get; set; }
}

/// <summary>
/// The part of a row of the semantic index the search keeps in memory (IKntSemanticIndexRepository.GetVectorsAsync).
/// </summary>
public class NoteChunkVectorDto
{
    public Guid NoteChunkId { get; set; }
    public Guid NoteId { get; set; }
    public NoteChunkSourceType SourceType { get; set; }
    public float[] Vector { get; set; }
}

/// <summary>
/// How the semantic index of a model is (IKntSemanticIndexRepository.GetIndexWatermarkAsync): the number of rows and the
/// last time one was written, which tell the cache of vectors whether it has to read anything again.
/// LastIndexedDateTime is null without rows.
/// </summary>
public class SemanticIndexWatermarkDto
{
    public int Count { get; set; }
    public DateTime? LastIndexedDateTime { get; set; }
}

public enum EnumSemanticIndexStatus
{
    // No index of the repository's embedding model (or no embedding model).
    None,
    // Being created: some notes have no fragments yet (also when a creation was canceled: the next one goes on).
    Building,
    // Created: from then on, only the notes that change are pending.
    Ready
}

/// <summary>
/// What KNote keeps of the semantic index of a repository (SystemValues AI / AI_INDEX_STATE, as JSON): for which index
/// key, whether its creation was completed, how long it took (to estimate the next one) and, during a rebuild, since
/// when: the vectors written before RebuildSince are not reused.
/// </summary>
public class SemanticIndexStateDto
{
    public string IndexKey { get; set; }
    public EnumSemanticIndexStatus Status { get; set; }
    public DateTime? CompletedDateTime { get; set; }
    public double? LastBuildSeconds { get; set; }
    public double? NotesPerMinute { get; set; }
    public DateTime? RebuildSince { get; set; }
}

/// <summary>
/// How the semantic index of a repository is (IKntSemanticSearchService.GetIndexStatusAsync): its model, whether it
/// exists, its notes (system notes apart), the notes indexed with the current rules or pending, and the indexes of
/// other models kept in the repository.
/// </summary>
public class SemanticIndexStatusDto
{
    public string EmbeddingModel { get; set; }
    public string IndexKey { get; set; }
    // Whether this installation has the embedding model (without it, the index cannot be created or updated here).
    public bool EmbeddingModelAvailable { get; set; }
    public EnumSemanticIndexStatus Status { get; set; }
    public bool Rebuilding { get; set; }
    public int TotalNotes { get; set; }
    public int IndexedNotes { get; set; }
    public int PendingNotes { get; set; }
    public int Chunks { get; set; }
    public DateTime? CompletedDateTime { get; set; }
    public double? LastBuildSeconds { get; set; }
    // The pace of the last complete creation (null before the first one), to estimate how long the pending ones take.
    public double? NotesPerMinute { get; set; }
    public List<SemanticIndexModelDto> OtherIndexes { get; set; } = new();
}

/// <summary>The index of a model kept in the repository: its key, its fragments and its notes.</summary>
public class SemanticIndexModelDto
{
    public string IndexKey { get; set; }
    public int Chunks { get; set; }
    public int Notes { get; set; }
}

/// <summary>The notes of a repository for the index of a model (IKntSemanticIndexRepository.GetIndexSummaryAsync).</summary>
public class SemanticIndexSummaryDto
{
    public int TotalNotes { get; set; }
    public int IndexedNotes { get; set; }
    public int PendingNotes { get; set; }
    public int Chunks { get; set; }
}

public enum EnumSemanticIndexPhase
{
    Queued,
    Preparing,
    Indexing,
    Completed,
    Canceled,
    Failed
}

/// <summary>
/// The progress of a run of the indexer (creating, updating or rebuilding the index): its phase (in a Server's queue,
/// with its position), the notes done of the total, the fragments calculated or reused, the errors (the notes that
/// failed are tried again in the next run) and the time left, from the pace so far.
/// </summary>
public class SemanticIndexProgressDto
{
    public const int MaxErrors = 20;

    public Guid RepositoryId { get; set; }
    public string RepositoryName { get; set; }
    public string IndexKey { get; set; }
    public bool Rebuild { get; set; }
    public EnumSemanticIndexPhase Phase { get; set; }
    public int QueuePosition { get; set; }
    public string RequestedBy { get; set; }
    public DateTime? StartedDateTime { get; set; }
    public double ElapsedSeconds { get; set; }
    public double? RemainingSeconds { get; set; }
    public int NotesTotal { get; set; }
    public int NotesDone { get; set; }
    public int ChunksEmbedded { get; set; }
    public int ChunksReused { get; set; }
    public int ErrorsCount { get; set; }
    // The first MaxErrors errors.
    public List<string> Errors { get; set; } = new();
    // Why the run failed (Phase Failed).
    public string Message { get; set; }

    public bool IsFinished => Phase is EnumSemanticIndexPhase.Completed or EnumSemanticIndexPhase.Canceled or EnumSemanticIndexPhase.Failed;

    public SemanticIndexProgressDto Copy()
    {
        var copy = (SemanticIndexProgressDto)MemberwiseClone();
        copy.Errors = new List<string>(Errors);
        return copy;
    }
}

/// <summary>What is asked to the Server's semantic index (POST api/semantic/index): create or update it, or rebuild it.</summary>
public class SemanticIndexRunDto
{
    public bool Rebuild { get; set; }
}
