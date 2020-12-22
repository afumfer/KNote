using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Service
{
    public interface IKntNoteService
    {
        Task<Result<List<NoteInfoDto>>> GetAllAsync();
        Task<Result<List<NoteInfoDto>>> HomeNotesAsync();
        Task<Result<NoteDto>> GetAsync(Guid noteId);
        Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId);
        Task <Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId);
        Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter);
        Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch);                        
        Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null);
        Task<Result<NoteExtendedDto>> NewExtendedAsync(NoteInfoDto entity = null);
        Task<Result<NoteDto>> SaveAsync(NoteDto entity);
        Task<Result<NoteExtendedDto>> SaveExtendedAsync(NoteExtendedDto entity);
        Task<Result<NoteDto>> DeleteAsync(Guid noteId);
        Task<Result<NoteExtendedDto>> DeleteExtendedAsync(Guid noteId);
        Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid noteId);
        Task<Result<ResourceDto>> GetResourceAsync(Guid resourceId); 
        Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity);
        Task<Result<ResourceDto>> DeleteResourceAsync(Guid noteId);
        Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId);
        Task<Result<NoteTaskDto>> GetNoteTaskAsync(Guid noteTaskId);
        Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo, bool forceNew = false);       
        Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid noteId);
        Task<Result<List<KMessageDto>>> GetMessagesAsync(Guid noteId);
        Task<Result<KMessageDto>> GetMessageAsync(Guid messageId);
        Task<Result<KMessageDto>> SaveMessageAsync(KMessageDto entity, bool forceNew = false);        
        Task<Result<KMessageDto>> DeleteMessageAsync(Guid messageId);


        //Task<Result<WindowDto>> SaveWindowAsync(WindowDto entityInfo);
        //Task<Result<TraceNoteDto>> SaveTraceNoteAsync(TraceNoteDto entityInfo);
    }
}
