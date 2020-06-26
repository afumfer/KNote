using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.ServiceEF.Repositories;
using KNote.Model;
using KNote.Model.Entities;
using KNote.Model.Dto;
using KNote.Model.Dto.Info;
using KNote.Model.Services;

namespace KNote.ServiceEF.Services
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

        public Result<List<SystemValueInfoDto>> GetAll()
        {
            var resService = new Result<List<SystemValueInfoDto>>();
            try
            {
                var resRep = _repository.SystemValues.GetAll();
                resService.Entity = resRep.Entity?.Select(sv => sv.GetSimpleDto<SystemValueInfoDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<SystemValueInfoDto> Get(string key)
        {
            var resService = new Result<SystemValueInfoDto>();
            try
            {
                var resRep = _repository.SystemValues.Get(sv => sv.Key == key);
                resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueInfoDto>();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<SystemValueInfoDto> Get(Guid id)
        {
            var resService = new Result<SystemValueInfoDto>();
            try
            {
                var resRep = _repository.SystemValues.Get((object) id);
                resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueInfoDto>();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<SystemValueInfoDto> Save(SystemValueInfoDto entityInfo)
        {
            Result<SystemValue> resRep = null;
            var resService = new Result<SystemValueInfoDto>();

            try
            {
                if (entityInfo.SystemValueId == Guid.Empty)
                {
                    entityInfo.SystemValueId = Guid.NewGuid();
                    var newEntity = new SystemValue();
                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = _repository.SystemValues.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.SystemValues.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.SystemValues.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.SystemValues.Get(entityInfo.SystemValueId).Entity;

                    if (flagThrowKntException == true)
                        _repository.SystemValues.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = _repository.SystemValues.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new SystemValue();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = _repository.SystemValues.Add(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            
            resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueInfoDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<SystemValueInfoDto> Delete(Guid id)
        {
            var resService = new Result<SystemValueInfoDto>();
            try
            {
                var resRep = _repository.SystemValues.Get(id);
                if (resRep.IsValid)
                {
                    resRep = _repository.SystemValues.Delete(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueInfoDto>();
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
