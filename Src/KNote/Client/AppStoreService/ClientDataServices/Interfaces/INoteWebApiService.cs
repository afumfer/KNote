using KNote.Model;
using KNote.Model.Dto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KNote.Client.AppStoreService.ClientDataServices;

// cancellationToken (all of them): the caller no longer wants the answer; see BaseService.SendAsync.
public interface INoteWebApiService
{
    // The notes of Home, or of one of the folders directly inside it (folderId).
    Task<Result<List<NoteInfoDto>>> GetHomeNotesAsync(Guid? folderId = null, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> GetAsync(Guid noteId, CancellationToken cancellationToken = default);
    // The note of a citation (#123): no entity when it does not exist or is a system note.
    Task<Result<NoteMinimalDto>> GetCitedAsync(int noteNumber, CancellationToken cancellationToken = default);
    Task<Result<string>> HtmlAsync(string markdown, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> NewAsync(CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> SaveAsync(NoteDto entity, bool updateStatus = true, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> DeleteAsync(Guid noteId, CancellationToken cancellationToken = default);

    Task<Result<List<ResourceInfoDto>>> GetResourcesAsync(Guid noteId, CancellationToken cancellationToken = default);
    Task<Result<ResourceInfoDto>> SaveResourceAsync(ResourceInfoDto entity, CancellationToken cancellationToken = default);
    Task<Result<ResourceInfoDto>> DeleteResourceAsync(Guid resourceId, CancellationToken cancellationToken = default);
    // KNote Atlas, stage 4: the model that describes photos, and describing one (replace: also a person's description).
    Task<Result<ResourceDescriberDto>> GetPhotoDescriberAsync(CancellationToken cancellationToken = default);
    Task<Result<ResourceInfoDto>> DescribePhotoAsync(Guid resourceId, bool replace, CancellationToken cancellationToken = default);
    Task<Result<ResourceInfoDto>> SummarizeDocumentAsync(Guid resourceId, bool replace, CancellationToken cancellationToken = default);

    // KNote Atlas 4.5b: the scanned pages of a document pending of a transcription (Entity null: none), and transcribing one.
    Task<Result<ScannedDocumentDto>> GetScannedPagesAsync(Guid resourceId, CancellationToken cancellationToken = default);

    Task<Result<DocumentReadDto>> TranscribePageAsync(Guid resourceId, int page, CancellationToken cancellationToken = default);

    /// <summary>
    /// A new resource of a note (resource: its NoteId, description, lock and order) with the file chosen in an InputFile,
    /// sent by the browser from the disk (js/kntUpload.js; KNote Atlas, stage 4, D6), with its progress (sent, total bytes).
    /// Canceling stops the upload: nothing is saved.
    /// </summary>
    Task<Result<ResourceInfoDto>> UploadResourceAsync(IJSRuntime js, ElementReference input, ResourceInfoDto resource,
        IProgress<(long Sent, long Total)>? progress = null, CancellationToken cancellationToken = default);
    Task<Result<DocumentReadDto>> ReadDocumentAsync(Guid resourceId, CancellationToken cancellationToken = default);
    // Its beginning: what the Web shows (DocumentTextRules.MaxViewerLength).
    Task<Result<ResourceTextDto>> GetDocumentTextAsync(Guid resourceId, CancellationToken cancellationToken = default);
    // «Process pending resources» of the session's repository (Admin to run): what it would process, and its run.
    Task<Result<ResourceProcessingStatusDto>> GetResourceProcessingStatusAsync(CancellationToken cancellationToken = default);
    Task<Result<ResourceProcessingProgressDto>> ProcessResourcesAsync(ResourceProcessingOptionsDto options);
    Task<Result<ResourceProcessingProgressDto>> GetResourceProcessingProgressAsync(CancellationToken cancellationToken = default);
    Task<Result<bool>> CancelResourceProcessingAsync();
    Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId, CancellationToken cancellationToken = default);
    // The tasks panel: the tasks of a time window by column, or a page of one column.
    Task<Result<TasksPanelDto>> GetTasksPanelAsync(TasksPanelFilterDto filter, CancellationToken cancellationToken = default);
    Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo, CancellationToken cancellationToken = default);
    Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid noteTaskId, CancellationToken cancellationToken = default);

    // Both directions, each one with the other note of the relation and its type resolved by the server.
    Task<Result<NoteTraceNotesDto>> GetTraceNotesAsync(Guid noteId, CancellationToken cancellationToken = default);
    // Saved at once; a new one has an empty TraceNoteId.
    Task<Result<TraceNoteDto>> SaveTraceNoteAsync(TraceNoteDto traceNote, CancellationToken cancellationToken = default);
    Task<Result<TraceNoteDto>> DeleteTraceNoteAsync(Guid traceNoteId, CancellationToken cancellationToken = default);

    // The note's alarms (KMessages), saved at once (api/notes/{id}/alarms, api/notes/alarms).
    Task<Result<List<KMessageDto>>> GetAlarmsAsync(Guid noteId, CancellationToken cancellationToken = default);
    Task<Result<KMessageDto>> NewAlarmAsync(Guid noteId, CancellationToken cancellationToken = default);
    Task<Result<KMessageDto>> SaveAlarmAsync(KMessageDto alarm, CancellationToken cancellationToken = default);
    Task<Result<KMessageDto>> DeleteAlarmAsync(Guid alarmId, CancellationToken cancellationToken = default);
    // The alarms calendar: the alarms of the filter and their occurrences in its range.
    Task<Result<AlarmsCalendarDto>> GetAlarmsCalendarAsync(AlarmsCalendarFilterDto filter, CancellationToken cancellationToken = default);

    Task<Result<List<NoteMinimalDto>>> GetSearch(string queryString, CancellationToken cancellationToken = default);
    Task<Result<List<NoteMinimalDto>>> GetFilter(NotesFilterDto notesFilter, CancellationToken cancellationToken = default);

    //Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId);
    //Task<Result<NoteExtendedDto>> NewExtendedAsync(NoteInfoDto entity = null);
    //Task<Result<NoteExtendedDto>> SaveExtendedAsync(NoteExtendedDto entity);
    //Task<Result<NoteExtendedDto>> DeleteExtendedAsync(Guid noteId);
}
