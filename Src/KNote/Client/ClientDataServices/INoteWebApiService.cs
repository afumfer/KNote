﻿using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public interface INoteWebApiService
    {
        Task<Result<List<NoteInfoDto>>> GetHomeNotesAsync();       
        Task<Result<NoteDto>> GetAsync(Guid noteId);        
        Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null);
        Task<Result<NoteDto>> SaveAsync(NoteDto entity, bool updateStatus = true);
        Task<Result<NoteDto>> DeleteAsync(Guid noteId);

        Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid noteId);
        Task<Result<ResourceDto>> GetResourceAsync(Guid resourceId);
        Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity);
        Task<Result<ResourceDto>> DeleteResourceAsync(Guid resourceId);
        Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId);
        Task<Result<NoteTaskDto>> GetNoteTaskAsync(Guid noteTaskId);
        Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo);
        Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid noteTaskId);


        //Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId);
        //Task<Result<NoteExtendedDto>> NewExtendedAsync(NoteInfoDto entity = null);
        //Task<Result<NoteExtendedDto>> SaveExtendedAsync(NoteExtendedDto entity);
        //Task<Result<NoteExtendedDto>> DeleteExtendedAsync(Guid noteId);
    }
}
