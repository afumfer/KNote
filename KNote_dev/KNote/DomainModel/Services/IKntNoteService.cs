﻿using System;
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
        Task<Result<NoteDto>> GetAsync(Guid noteId);
        int GetNextNoteNumber();
        Result<List<NoteInfoDto>> GetByFolder(Guid folderId);
        Result<List<NoteItemDto>> GetNoteItemList(Guid? folderId);
        Result<List<NoteInfoDto>> GetFilter(int _page, int _limit, Guid folderId, string query);
        Result<NoteDto> New(NoteInfoDto entity = null);
        Task<Result<NoteDto>> SaveAsync(NoteDto entityInfo);
        Result<NoteKAttributeDto> SaveAttrtibute(NoteKAttributeDto entity);
        Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity);
        Task<Result<ResourceDto>> DeleteResourceAsync(Guid id);
        Result<List<ResourceDto>> GetNoteResources(Guid idNote);
        Result<NoteTaskInfoDto> SaveNoteTask(NoteTaskInfoDto entityInfo);
        Result<WindowInfoDto> SaveWindow(WindowInfoDto entityInfo);
        Result<TraceNoteInfoDto> SaveTraceNote(TraceNoteInfoDto entityInfo);        
        Result<NoteInfoDto> Delete(Guid id);
        Task<Result<NoteInfoDto>> DeleteAsync(Guid id);
        Result<List<NoteInfoDto>> RecentNotes();
        Task<Result<List<NoteInfoDto>>> GetFilter2(NotesFilterDto notesFilter);


        //Result<NoteDto> LoadAllCollections(Note note);
        //Result<List<NoteDto>> GetAllFull(Expression<Func<Note, bool>> predicate);
    }
}
