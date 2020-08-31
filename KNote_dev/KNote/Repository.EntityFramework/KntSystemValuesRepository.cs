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

    // TODO: Pendiente de probar

    public class KntSystemValuesRepository : DomainActionBase, IKntSystemValuesRepository
    {
        private IGenericRepositoryEF<KntDbContext, SystemValue> _systemValues;

        public KntSystemValuesRepository(KntDbContext context, bool throwKntException)
        {
            _systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(context, throwKntException);
            ThrowKntException = throwKntException;
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

        public async Task<Result<SystemValueDto>> GetAsync(string scope, string key)
        {
            var resService = new Result<SystemValueDto>();
            try
            {
                var resRep = await _systemValues.GetAsync(sv => sv.Scope == scope && sv.Key == key);
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

        public async Task<Result<SystemValueDto>> AddAsync(SystemValueDto entity)
        {
            var response = new Result<SystemValueDto>();
            try
            {
                var newEntity = new SystemValue();
                newEntity.SetSimpleDto(entity);

                var resGenRep = await _systemValues.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<SystemValueDto>();
                response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<SystemValueDto>> UpdateAsync(SystemValueDto entity)
        {
            var resGenRep = new Result<SystemValue>();
            var response = new Result<SystemValueDto>();

            try
            {
                bool flagThrowKntException = false;
                if (_systemValues.ThrowKntException == true)
                {
                    flagThrowKntException = true;
                    _systemValues.ThrowKntException = false;
                }

                var resGenRepGet = await _systemValues.GetAsync(entity.SystemValueId);
                SystemValue entityForUpdate;

                if (flagThrowKntException == true)
                    _systemValues.ThrowKntException = true;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    resGenRep = await _systemValues.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }

                response.Entity = resGenRep.Entity?.GetSimpleDto<SystemValueDto>();
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
                var resGenRep = await _systemValues.DeleteAsync(id);
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
            _systemValues.Dispose();
        }

        #endregion
    }
}
