using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository;

/// <summary>
/// The semantic index of KNote Atlas: the fragments of each note with their vector, one set per embedding model
/// (NoteChunks, schema revision 7). The rows are derived data: the service writes them from the notes (NoteChunkBuilder)
/// and they go away with their note (cascade delete).
/// </summary>
public interface IKntSemanticIndexRepository : IDisposable
{
    /// <summary>The rows of a note for a model, with their vectors, in order (SourceType, SourceId, ChunkIndex).</summary>
    Task<Result<List<NoteChunkDto>>> GetNoteChunksAsync(Guid noteId, string model);

    /// <summary>
    /// Replaces the rows of a note for a model with these ones, at once (in a transaction). Each row is written with
    /// this NoteId and Model, its NoteChunkId (a new one if it is empty) and Stale = false.
    /// </summary>
    Task<Result> ReplaceNoteChunksAsync(Guid noteId, string model, List<NoteChunkDto> chunks);

    /// <summary>Marks the rows of a note, of every model, to be calculated again (Stale = true).</summary>
    Task<Result> MarkStaleAsync(Guid noteId);

    /// <summary>Removes the index of a model.</summary>
    Task<Result> DeleteModelAsync(string model);

    /// <summary>
    /// The notes the index of a model has to calculate (again), max at most, the last modified first: those without
    /// rows, with rows marked as Stale, or modified after they were indexed. Not the ones of the excluded note types.
    /// </summary>
    Task<Result<List<Guid>>> GetPendingNoteIdsAsync(string model, int max, List<Guid> excludedNoteTypeIds = null);

    /// <summary>
    /// The vectors of a model, for the search's cache: all, or the ones written since a time (IndexedDateTime equal or
    /// later).
    /// </summary>
    Task<Result<List<NoteChunkVectorDto>>> GetVectorsAsync(string model, DateTime? changedSince = null);

    /// <summary>The number of rows of a model and the last time one was written.</summary>
    Task<Result<SemanticIndexWatermarkDto>> GetIndexWatermarkAsync(string model);

    /// <summary>These rows without their vectors (to show what was found).</summary>
    Task<Result<List<NoteChunkDto>>> GetChunkContentsAsync(List<Guid> noteChunkIds);
}
