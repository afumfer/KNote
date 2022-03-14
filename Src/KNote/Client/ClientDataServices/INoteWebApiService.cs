using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.ClientDataServices;

public interface INoteWebApiService
{
    Task<Result<List<NoteInfoDto>>> GetHomeNotesAsync();       
    Task<Result<NoteDto>> GetAsync(Guid noteId);        
    Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null);
    Task<Result<NoteDto>> SaveAsync(NoteDto entity, bool updateStatus = true);
    Task<Result<NoteDto>> DeleteAsync(Guid noteId);

    Task<Result<List<ResourceInfoDto>>> GetResourcesAsync(Guid noteId);        
    Task<Result<ResourceInfoDto>> SaveResourceAsync(ResourceInfoDto entity);
    Task<Result<ResourceInfoDto>> DeleteResourceAsync(Guid resourceId);
    Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId);
    Task<Result<List<NoteTaskDto>>> GetStartedTasksByDateTimeAsync(DateTime startDateTime, DateTime endDateTime);
    Task<Result<List<NoteTaskDto>>> GetEstimatedTasksByDateTimeAsync(DateTime startDateTime, DateTime endDateTime);
    Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo);
    Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid noteTaskId);

    Task<Result<List<NoteInfoDto>>> GetSearch(string queryString);
    Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter);

    //Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId);
    //Task<Result<NoteExtendedDto>> NewExtendedAsync(NoteInfoDto entity = null);
    //Task<Result<NoteExtendedDto>> SaveExtendedAsync(NoteExtendedDto entity);
    //Task<Result<NoteExtendedDto>> DeleteExtendedAsync(Guid noteId);
}

