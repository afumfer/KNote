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
    public class KntSystemValuesRepository : DomainActionBase, IKntSystemValuesRepository
    {
        private IGenericRepositoryEF<KntDbContext, SystemValue> _systemValues;

        public KntSystemValuesRepository(KntDbContext context, bool throwKntException)
        {
            _systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(context, throwKntException);
        }

        public async Task<Result<List<SystemValueDto>>> GetAllAsync()
        {
            var resService = new Result<List<SystemValueDto>>();
            try
            {
                var resRep = await _systemValues.GetAllAsync();
                resService.Entity = resRep.Entity?.Select(sv => sv.GetSimpleDto<SystemValueDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<SystemValueDto>> GetAsync(string key)
        {
            var resService = new Result<SystemValueDto>();
            try
            {
                var resRep = await _systemValues.GetAsync(sv => sv.Key == key);
                resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueDto>();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<SystemValueDto>> GetAsync(Guid id)
        {
            var resService = new Result<SystemValueDto>();
            try
            {
                var resRep = await _systemValues.GetAsync((object)id);
                resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueDto>();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<SystemValueDto>> SaveAsync(SystemValueDto entity)
        {
            Result<SystemValue> resRep = null;
            var resService = new Result<SystemValueDto>();

            try
            {
                if (entity.SystemValueId == Guid.Empty)
                {
                    entity.SystemValueId = Guid.NewGuid();
                    var newEntity = new SystemValue();
                    newEntity.SetSimpleDto(entity);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = await _systemValues.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_systemValues.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _systemValues.ThrowKntException = false;
                    }

                    var entityForUpdate = (await _systemValues.GetAsync(entity.SystemValueId)).Entity;

                    if (flagThrowKntException == true)
                        _systemValues.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _systemValues.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new SystemValue();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = await _systemValues.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<SystemValueDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<SystemValueDto>();
            try
            {
                var resRep = await _systemValues.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _systemValues.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueDto>();
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
            _systemValues.Dispose();
        }

        #endregion
    }
}
