using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// The history of the notes (api/history). cancellationToken (all of them): the caller no longer wants the answer.
public interface IHistoryWebApiService
{
    Task<Result<NoteHistoryPageDto>> GetNoteHistoryAsync(Guid noteId, int skip = 0, CancellationToken cancellationToken = default);
    Task<Result<NoteVersionViewDto>> GetNoteVersionAsync(Guid noteId, Guid klogId, CancellationToken cancellationToken = default);
    Task<Result<NoteComparisonDto>> GetNoteComparisonAsync(Guid noteId, Guid klogId, bool withCurrentNote, CancellationToken cancellationToken = default);
    Task<Result<NoteRestoreResultDto>> RestoreNoteVersionAsync(Guid noteId, Guid klogId);
    Task<Result<List<DeletedNoteDto>>> GetDeletedNotesAsync(CancellationToken cancellationToken = default);
    Task<Result<NoteRecoverResultDto>> RecoverNoteAsync(Guid noteId);
    Task<Result> RemoveDeletedNoteAsync(Guid noteId);
    Task<Result<RepositoryChangesPageDto>> GetRepositoryChangesAsync(RepositoryChangesQueryDto query, CancellationToken cancellationToken = default);
    Task<Result<List<string>>> GetRepositoryChangesUsersAsync(RepositoryChangesQueryDto query, CancellationToken cancellationToken = default);
    Task<Result<HistoryRetentionInfoDto>> GetRetentionAsync(CancellationToken cancellationToken = default);
    Task<Result<HistoryRetentionDto>> SaveRetentionAsync(HistoryRetentionDto retention);
}
