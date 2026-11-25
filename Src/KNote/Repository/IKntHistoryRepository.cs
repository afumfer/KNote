using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository;

/// <summary>
/// The history of the notes and of the structure of the repository (KLogs, schema revision 11): one row per change, or per
/// session of changes (H5), with, if the change touched a note's own data, how the note was before (PreviousContent, the
/// version). The rows are written with the change, in its transaction, by the methods that make it (KLogWriteDto, see
/// IKntNoteRepository). PreviousContent is not in the EF model: only GetPreviousContentAsync reads it, never the lists.
/// </summary>
public interface IKntHistoryRepository : IDisposable
{
    /// <summary>The latest row of an entity (the last change: LastDateTime), without its version, or null.</summary>
    Task<Result<KLogDto>> GetLastAsync(string entityName, Guid entityId);

    /// <summary>The rows of an entity, the newest first, without their versions.</summary>
    Task<Result<List<KLogDto>>> GetEntityHistoryAsync(string entityName, Guid entityId);

    /// <summary>The version a row keeps (JSON), or null if it has none.</summary>
    Task<Result<string>> GetPreviousContentAsync(Guid klogId);

    /// <summary>Empties the version of these rows (the retention): the rows stay.</summary>
    Task<Result> ClearPreviousContentAsync(List<Guid> klogIds);

    /// <summary>
    /// The entities deleted before a moment (their latest row is Deleted, older than it) that still keep some version: the trash
    /// to empty (H8).
    /// </summary>
    Task<Result<List<Guid>>> GetDeletedWithContentAsync(string entityName, DateTime deletedBefore);

    /// <summary>Empties every version of an entity.</summary>
    Task<Result> ClearEntityContentAsync(string entityName, Guid entityId);

    /// <summary>
    /// Writes a row of the history on its own, just after the change it notes: the structure of the repository, whose changes
    /// don't run in a transaction of their own (H10). The merge into the row of its session and its row are one transaction.
    /// </summary>
    Task<Result> WriteAsync(KLogWriteDto write);
}
