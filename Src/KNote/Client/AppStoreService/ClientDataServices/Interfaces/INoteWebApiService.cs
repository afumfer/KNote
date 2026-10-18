using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// cancellationToken (all of them): the caller no longer wants the answer; see BaseService.SendAsync.
public interface INoteWebApiService
{
    // The notes of Home, or of one of the folders directly inside it (folderId).
    Task<Result<List<NoteInfoDto>>> GetHomeNotesAsync(Guid? folderId = null, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> GetAsync(Guid noteId, CancellationToken cancellationToken = default);
    Task<Result<string>> HtmlAsync(string markdown, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> NewAsync(CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> SaveAsync(NoteDto entity, bool updateStatus = true, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> DeleteAsync(Guid noteId, CancellationToken cancellationToken = default);

    Task<Result<List<ResourceInfoDto>>> GetResourcesAsync(Guid noteId, CancellationToken cancellationToken = default);
    Task<Result<ResourceInfoDto>> SaveResourceAsync(ResourceInfoDto entity, CancellationToken cancellationToken = default);
    Task<Result<ResourceInfoDto>> DeleteResourceAsync(Guid resourceId, CancellationToken cancellationToken = default);
    Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId, CancellationToken cancellationToken = default);
    Task<Result<List<NoteTaskDto>>> GetStartedTasksByDateTimeAsync(DateTime startDateTime, DateTime endDateTime, CancellationToken cancellationToken = default);
    Task<Result<List<NoteTaskDto>>> GetEstimatedTasksByDateTimeAsync(DateTime startDateTime, DateTime endDateTime, CancellationToken cancellationToken = default);
    Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo, CancellationToken cancellationToken = default);
    Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid noteTaskId, CancellationToken cancellationToken = default);

    // Both directions, each one with the other note of the relation and its type resolved by the server.
    Task<Result<NoteTraceNotesDto>> GetTraceNotesAsync(Guid noteId, CancellationToken cancellationToken = default);
    // Saved at once; a new one has an empty TraceNoteId.
    Task<Result<TraceNoteDto>> SaveTraceNoteAsync(TraceNoteDto traceNote, CancellationToken cancellationToken = default);
    Task<Result<TraceNoteDto>> DeleteTraceNoteAsync(Guid traceNoteId, CancellationToken cancellationToken = default);

    Task<Result<List<NoteMinimalDto>>> GetSearch(string queryString, CancellationToken cancellationToken = default);
    Task<Result<List<NoteMinimalDto>>> GetFilter(NotesFilterDto notesFilter, CancellationToken cancellationToken = default);

    //Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId);
    //Task<Result<NoteExtendedDto>> NewExtendedAsync(NoteInfoDto entity = null);
    //Task<Result<NoteExtendedDto>> SaveExtendedAsync(NoteExtendedDto entity);
    //Task<Result<NoteExtendedDto>> DeleteExtendedAsync(Guid noteId);
}
