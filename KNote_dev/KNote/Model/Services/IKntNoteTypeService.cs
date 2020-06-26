using KNote.Model;
using KNote.Model.Dto;
using KNote.Model.Dto.Info;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KNote.Model.Services
{
    public interface IKntNoteTypeService
    {        
        Result<List<NoteTypeInfoDto>> GetAll();
        Task<Result<NoteTypeDto>> GetAsync(Guid id);
        Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entityInfo);
        Task<Result<NoteTypeInfoDto>> DeleteAsync(Guid id);
    }
}