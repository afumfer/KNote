﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Repository
{
    public interface IKntNoteRepository : IDisposable
    {
        Task<Result<List<NoteInfoDto>>> GetAllAsync();
        Task<Result<List<NoteInfoDto>>> HomeNotesAsync();
        Task<Result<NoteDto>> GetAsync(Guid noteId);        
        Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId);
        Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter);
        Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch);
        Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null);        
        Task<Result<NoteDto>> AddAsync(NoteDto entity);
        Task<Result<NoteDto>> UpdateAsync(NoteDto entity);
        Task<Result> DeleteAsync(Guid id);
        Task<Result<List<ResourceDto>>> GetNoteResourcesAsync(Guid idNote);
        Task<Result<ResourceDto>> GetNoteResourceAsync(Guid idNoteResource);
        Task<Result<ResourceDto>> AddResourceAsync(ResourceDto entity);
        Task<Result<ResourceDto>> UpdateResourceAsync(ResourceDto entity);
        Task<Result> DeleteResourceAsync(Guid id);
        Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid idNote);
        Task<Result<NoteTaskDto>> GetNoteTaskAsync(Guid idNoteTask);
        Task<Result<NoteTaskDto>> AddNoteTaskAsync(NoteTaskDto entity);
        Task<Result<NoteTaskDto>> UpdateNoteTaskAsync(NoteTaskDto entity);        
        Task<Result> DeleteNoteTaskAsync(Guid id);

        //Task<Result<WindowDto>> SaveWindowAsync(WindowDto entityInfo);
        //Task<Result<TraceNoteDto>> SaveTraceNoteAsync(TraceNoteDto entityInfo);
    }
}
