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
    Task<Result<List<NoteTaskDto>>> GetStartedTasksByDateTimeRageAsync(DateTime startDateTime, DateTime endDateTime);
    Task<Result<List<NoteTaskDto>>> GetEstimatedTasksByDateTimeRageAsync(DateTime startDateTime, DateTime endDateTime);
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
    Task<Result<bool>> PatchFolderAsync(Guid noteId, Guid folderId);
    Task<Result<bool>> PatchChangeTagsAsync(Guid noteId, string oldTag, string newTag);       
}
