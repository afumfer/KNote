using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// The history of the notes (api/history). cancellationToken (all of them): the caller no longer wants the answer.
public interface IHistoryWebApiService
{
    Task<Result<NoteHistoryPageDto>> GetNoteHistoryAsync(Guid noteId, int skip = 0, CancellationToken cancellationToken = default);
    Task<Result<NoteVersionViewDto>> GetNoteVersionAsync(Guid noteId, Guid klogId, CancellationToken cancellationToken = default);
}
