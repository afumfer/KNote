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
        Task <Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId);
        Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter);
        Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch);                        
        Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null);
        Task<Result<NoteDto>> SaveAsync(NoteDto entityInfo);
        Task<Result<NoteDto>> DeleteAsync(Guid id);
        //Task<Result<NoteKAttributeDto>> SaveAttrtibuteAsync(NoteKAttributeDto entity);
        Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity);
        Task<Result<ResourceDto>> DeleteResourceAsync(Guid id);
        Task<Result<List<ResourceDto>>> GetNoteResourcesAsync(Guid idNote);        
        Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo);
        Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid idNote);
        Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid id);

        //Task<Result<WindowDto>> SaveWindowAsync(WindowDto entityInfo);
        //Task<Result<TraceNoteDto>> SaveTraceNoteAsync(TraceNoteDto entityInfo);
    }
}
