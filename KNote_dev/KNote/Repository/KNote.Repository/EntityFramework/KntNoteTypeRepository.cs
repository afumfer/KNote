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
        }

        public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
        {            
            var resService = new Result<List<NoteTypeDto>>();
            try
            {
                var resRep = await _noteTypes.GetAllAsync();
                resService.Entity = resRep.Entity?
                    .Select(t => t.GetSimpleDto<NoteTypeDto>())
                    .OrderBy(t => t.Name)
                    .ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
        {
            var resService = new Result<NoteTypeDto>();
            try
            {
                var resRep = await _noteTypes.GetAsync((object)id);

                resService.Entity = resRep.Entity?.GetSimpleDto<NoteTypeDto>();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entity)
        {
            Result<NoteType> resRep = null;
            var resService = new Result<NoteTypeDto>();

            try
            {
                if (entity.NoteTypeId == Guid.Empty)
                {
                    entity.NoteTypeId = Guid.NewGuid();
                    var newEntity = new NoteType();
                    newEntity.SetSimpleDto(entity);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = await _noteTypes.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_noteTypes.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _noteTypes.ThrowKntException = false;
                    }

                    //var entityForUpdate = _repository.Users.Get(entityInfo.UserId).Entity;
                    resRep = await _noteTypes.GetAsync(entity.NoteTypeId);
                    var entityForUpdate = resRep.Entity;

                    if (flagThrowKntException == true)
                        _noteTypes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _noteTypes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new NoteType();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = await _noteTypes.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios).
            resService.Entity = resRep.Entity?.GetSimpleDto<NoteTypeDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<NoteTypeDto>();
            try
            {
                var resRep = await _noteTypes.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _noteTypes.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<NoteTypeDto>();
                    else
                        resService.ErrorList = resRep.ErrorList;
                }
                else
                    resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        #region  IDisposable

        public virtual void Dispose()
        {
            _noteTypes.Dispose();
        }

        #endregion
    }
}
