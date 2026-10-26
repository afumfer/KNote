using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Repository;

public interface IKntNoteRepository : IDisposable
{
    Task<Result<List<NoteInfoDto>>> GetAllAsync();
    Task<Result<List<NoteMinimalDto>>> GetAllMinimalAsync();
    // The notes Home shows (by priority and topic, 25 at most): those of the Home folder, or of folderId.
    Task<Result<List<NoteInfoDto>>> HomeNotesAsync(Guid? folderId = null);
    Task<Result<NoteDto>> GetAsync(Guid noteId);
    Task<Result<NoteDto>> GetAsync(int noteNumber);
    Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId);
    Task<Result<List<NoteMinimalDto>>> GetByFolderMinimalAsync(Guid folderId);
    Task<Result<List<NoteInfoDto>>> GetFilterAsync(NotesFilterDto notesFilter);
    Task<Result<List<NoteMinimalDto>>> GetFilterMinimalAsync(NotesFilterDto notesFilter);
    Task<Result<List<NoteInfoDto>>> GetSearchAsync(NotesSearchDto notesSearch);
    Task<Result<List<NoteMinimalDto>>> GetSearchMinimalAsync(NotesSearchDto notesSearch);
    Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null);        
    Task<Result<NoteDto>> AddAsync(NoteDto entity);
    Task<Result<NoteDto>> UpdateAsync(NoteDto entity);
    Task<Result> DeleteAsync(Guid id);
    Task<List<NoteKAttributeDto>> CompleteNoteAttributesAsync(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null);
    Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid noteId);
    Task<Result<ResourceDto>> GetResourceAsync(Guid noteResourceId);
    Task<Result<ResourceDto>> AddResourceAsync(ResourceDto entity);
    Task<Result<ResourceDto>> UpdateResourceAsync(ResourceDto entity);
    Task<Result> DeleteResourceAsync(Guid resourceId);
    Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId);
    // The tasks panel (TaskPanelRules in SQL; never the tasks of the query's ExcludeNoteTypeIds): how many tasks each
    // column has in the window (and how many the filters have without it), a page of a column in its order (Pending and
    // In progress by start, Resolved by end, the latest first), and, for a window without tasks, where the nearest are.
    Task<Result<TasksPanelCountsDto>> GetTasksPanelCountsAsync(TasksPanelQueryDto query);
    Task<Result<List<TaskPanelItemDto>>> GetTasksPanelPageAsync(TasksPanelQueryDto query, EnumTaskPanelColumn column, int skip, int take);
    Task<Result<TasksPanelNearestDto>> GetTasksPanelNearestAsync(TasksPanelQueryDto query);
    Task<Result<NoteTaskDto>> GetNoteTaskAsync(Guid noteTaskId);
    Task<Result<NoteTaskDto>> AddNoteTaskAsync(NoteTaskDto entity);
    Task<Result<NoteTaskDto>> UpdateNoteTaskAsync(NoteTaskDto entity);        
    Task<Result> DeleteNoteTaskAsync(Guid noteTaskId);
    Task<Result<List<KMessageDto>>> GetMessagesAsync(Guid noteId);
    Task<Result<KMessageDto>> GetMessageAsync(Guid messageId);
    Task<Result<KMessageDto>> AddMessageAsync(KMessageDto entity);
    Task<Result<KMessageDto>> UpdateMessageAsync(KMessageDto entity);
    Task<Result> DeleteMessageAsync(Guid messageId);
    Task<Result<int>> CountNotesInFolderAsync(Guid folderId);
    Task<Result<int>> CountMessagesByUserAsync(Guid userId);
    Task<Result<int>> CountWindowsByUserAsync(Guid userId);
    Task<Result<int>> CountTasksByUserAsync(Guid userId);
    Task<Result<WindowDto>> GetWindowAsync(Guid noteId, Guid userId);
    Task<Result<WindowDto>> AddWindowAsync(WindowDto entity);
    Task<Result<WindowDto>> UpdateWindowAsync(WindowDto entity);
    Task<Result<List<Guid>>> GetVisibleNotesIdAsync(Guid userId);
    Task<Result<List<Guid>>> GetAlarmNotesIdAsync(Guid userId, EnumNotificationType? notificationType = null);
    // The due alarms of a type (activated, AlarmDateTime <= now, of a note) of a user or, with userId null, of every
    // user, consumed: each one rescheduled or deactivated (ApplyAlarmControl) with a conditional update (still activated
    // and due), so an alarm is returned once even if several consumers check at the same time (ClientWin, the Server,
    // two tabs). They are returned as they were when due (with UserFullName), so a caller can put one back as it was
    // (UpdateMessageAsync), e.g. when its email couldn't be sent.
    Task<Result<List<KMessageDto>>> ConsumeDueMessagesAsync(Guid? userId, EnumNotificationType notificationType);
    // The alarms the alarms calendar may show in the filter's range (of a note, with a date, of the filter's user and
    // types; deactivated ones only if it asks for them): those whose AlarmDateTime is in the range and every activated
    // periodic one, with their note's number, topic and creation date. The Service expands their occurrences.
    Task<Result<List<AlarmCalendarItemDto>>> GetCalendarAlarmsAsync(AlarmsCalendarFilterDto filter);
    Task<Result<bool>> PatchFolderAsync(Guid noteId, Guid folderId);
    Task<Result<bool>> PatchChangeTagsAsync(Guid noteId, string oldTag, string newTag);       
}
