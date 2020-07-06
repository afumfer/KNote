using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Repository.EntityFramework;
using KNote.Model;
using KNote.Repository.Entities;
using KNote.Model.Dto;
using KNote.Service;
using KNote.Repository;

namespace KNote.Service.Services
{
    public class KntSystemValuesService : DomainActionBase, IKntSystemValuesService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        protected internal KntSystemValuesService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntSystemValuesService

        public async Task<Result<List<SystemValueDto>>> GetAllAsync()
        {
            var resService = new Result<List<SystemValueDto>>();
            try
            {
                var resRep = await _repository.SystemValues.GetAllAsync();
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
                var resRep = await _repository.SystemValues.GetAsync(sv => sv.Key == key);
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
                var resRep = await _repository.SystemValues.GetAsync((object) id);
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

                    resRep = await _repository.SystemValues.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.SystemValues.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.SystemValues.ThrowKntException = false;
                    }

                    var entityForUpdate = (await _repository.SystemValues.GetAsync(entity.SystemValueId)).Entity;

                    if (flagThrowKntException == true)
                        _repository.SystemValues.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _repository.SystemValues.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new SystemValue();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = await _repository.SystemValues.AddAsync(newEntity);
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
                var resRep = await _repository.SystemValues.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.SystemValues.DeleteAsync(resRep.Entity);
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

        #endregion
    }
}
