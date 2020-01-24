using KNote.Shared;
using KNote.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KNote.DomainModel.Services
{
    public interface IKntNoteTypeService
    {
        Task<Result<NoteTypeDto>> DeleteAsync(Guid id);
        Result<List<NoteTypeInfoDto>> GetAll();
        Task<Result<NoteTypeInfoDto>> GetAsync(Guid id);
        Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entityInfo);
    }
}