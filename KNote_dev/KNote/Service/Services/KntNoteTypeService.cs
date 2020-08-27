using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using System.Linq.Expressions;
using KNote.Service;
using KNote.Repository;

namespace KNote.Service.Services
{
    public class KntNoteTypeService : DomainActionBase, IKntNoteTypeService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        public KntNoteTypeService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntNoteTypes

        public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
        {
            return await _repository.NoteTypes.GetAllAsync();
        }

        public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
        {
            return await _repository.NoteTypes.GetAsync(id);
        }

        public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entity)
        {
            // !!! Refactorizar. 
            //return await _repository.NoteTypes.SaveAsync(entity);

            if (entity.NoteTypeId == Guid.Empty)
            {
                entity.NoteTypeId = Guid.NewGuid();
                return await _repository.NoteTypes.AddAsync(entity);
            }
            else
            {
                return await _repository.NoteTypes.UpdateAsync(entity);
            }
        }

        public async Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
        {
            // return await _repository.NoteTypes.DeleteAsync(id);

            var result = new Result<NoteTypeDto>();

            var resGetEntity = await GetAsync(id);

            if (resGetEntity.IsValid) 
            { 
                var resDelEntity = await _repository.NoteTypes.DeleteAsync(id);
                if (resDelEntity.IsValid)                
                    result.Entity = resGetEntity.Entity;                                    
                else                
                    result.ErrorList = resDelEntity.ErrorList;
                                    
            }
            else
            {
                result.ErrorList = resGetEntity.ErrorList;
            }

            return result;
        }

        #endregion

    }
}
