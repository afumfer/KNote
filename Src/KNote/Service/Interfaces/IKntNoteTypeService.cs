using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

public interface IKntNoteTypeService
{
    Task<Result<List<NoteTypeDto>>> GetAllAsync();
    Task<Result<NoteTypeDto>> GetAsync(Guid id);
    Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entity);
    Task<Result<NoteTypeDto>> DeleteAsync(Guid id);
}