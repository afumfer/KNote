using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

public interface IKntTraceNoteTypeService
{
    Task<Result<List<TraceNoteTypeDto>>> GetAllAsync();
    Task<Result<TraceNoteTypeDto>> GetAsync(Guid id);
    Task<Result<TraceNoteTypeDto>> SaveAsync(TraceNoteTypeDto entity);
    Task<Result<TraceNoteTypeDto>> DeleteAsync(Guid id);
}
