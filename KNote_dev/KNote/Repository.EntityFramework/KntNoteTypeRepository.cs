using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework
{
    public class KntNoteTypeRepository : DomainActionBase, IKntNoteTypeRepository
    {
        private IGenericRepositoryEF<KntDbContext, NoteType> _noteTypes;
        
        public KntNoteTypeRepository(KntDbContext context, bool throwKntException) 
        {        
            _noteTypes = new GenericRepositoryEF<KntDbContext, NoteType>(context, throwKntException);
            ThrowKntException = throwKntException;
        }

        public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
        {            
            var response = new Result<List<NoteTypeDto>>();
            try
            {
                var resGenRep = await _noteTypes.GetAllAsync();
                response.Entity = resGenRep.Entity?
                    .Select(t => t.GetSimpleDto<NoteTypeDto>())
                    .OrderBy(t => t.Name)
                    .ToList();
                response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
        {
            var response = new Result<NoteTypeDto>();
            try
            {
                var resGenRep = await _noteTypes.GetAsync((object)id);

                response.Entity = resGenRep.Entity?.GetSimpleDto<NoteTypeDto>();
                response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<NoteTypeDto>> AddAsync(NoteTypeDto entity)
        {
            var response = new Result<NoteTypeDto>();
            try
            {                
                var newEntity = new NoteType();
                newEntity.SetSimpleDto(entity);

                var resGenRep = await _noteTypes.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<NoteTypeDto>();
                response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<NoteTypeDto>> UpdateAsync(NoteTypeDto entity)
        {
            var resGenRep = new Result<NoteType>();            
            var response = new Result<NoteTypeDto>();

            try
            {
                bool flagThrowKntException = false;
                if (_noteTypes.ThrowKntException == true)
                {
                    flagThrowKntException = true;
                    _noteTypes.ThrowKntException = false;
                }

                var resGenRepGet = await _noteTypes.GetAsync(entity.NoteTypeId) ;
                NoteType entityForUpdate;

                if (flagThrowKntException == true)
                    _noteTypes.ThrowKntException = true;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    resGenRep = await _noteTypes.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }
                
                response.Entity = resGenRep.Entity?.GetSimpleDto<NoteTypeDto>();
                response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            
            return ResultDomainAction(response);
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var response = new Result();
            try
            {
                var resGenRep = await _noteTypes.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);

        }

        #region  IDisposable

        public virtual void Dispose()
        {
            _noteTypes.Dispose();
        }

        #endregion
    }
}
