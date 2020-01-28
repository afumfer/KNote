using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using KNote.Shared;
// TODO: Pendiente de eliminar
//using KNote.DomainModel.Dto;
using KNote.Shared.Dto;

using KNote.DomainModel.Entities;

namespace KNote.DomainModel.Services
{
    public interface IKntNoteService
    {
        Result<List<NoteInfoDto>> GetAll();
        Result<NoteDto> Get(int noteNumber);
        Result<NoteDto> Get(Guid noteId);
        int GetNextNoteNumber();
        Result<List<NoteInfoDto>> GetByFolder(Guid folderId);
        Result<List<NoteItemDto>> GetNoteItemList(Guid? folderId);
        Result<List<NoteInfoDto>> GetFilter(int _page, int _limit, Guid folderId, string query);
        Result<NoteDto> New(NoteInfoDto entity = null);
        Result<NoteDto> Save(NoteDto entity);
        Task<Result<NoteDto>> SaveAsync(NoteDto entityInfo);

        Result<NoteKAttributeInfoDto> SaveAttrtibute(NoteKAttributeInfoDto entityInfo);
        Result<ResourceInfoDto> SaveResource(ResourceInfoDto entityInfo);
        Result<NoteTaskInfoDto> SaveNoteTask(NoteTaskInfoDto entityInfo);
        Result<WindowInfoDto> SaveWindow(WindowInfoDto entityInfo);
        Result<TraceNoteInfoDto> SaveTraceNote(TraceNoteInfoDto entityInfo);
        
        Result<NoteInfoDto> Delete(Guid id);
        Task<Result<NoteInfoDto>> DeleteAsync(Guid id);
        Result<List<NoteInfoDto>> RecentNotes();

        //Result<NoteDto> LoadAllCollections(Note note);
        //Result<List<NoteDto>> GetAllFull(Expression<Func<Note, bool>> predicate);
    }
}
