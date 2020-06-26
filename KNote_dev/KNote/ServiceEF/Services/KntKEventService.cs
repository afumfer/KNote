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
using KNote.ServiceEF.Infrastructure;
using KNote.Model.Services;

namespace KNote.ServiceEF.Services
{
    public class KntKEventService : DomainActionBase, IKntKEventService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        protected internal KntKEventService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntKEventService

        public Result<List<KEventInfoDto>> GetAll()
        {
            var resService = new Result<List<KEventInfoDto>>();
            try
            {
                var resRep = _repository.KEvents.GetAll();
                resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<KEventInfoDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<KEventInfoDto> Get(Guid eventId)
        {
            var resService = new Result<KEventInfoDto>();
            try
            {
                var resRep = _repository.KEvents.Get((object)eventId);
                resService.Entity = resRep.Entity?.GetSimpleDto<KEventInfoDto>();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<KEventDto> New(KEventInfoDto entityInfo = null)
        {
            var resService = new Result<KEventDto>();
            KEventDto newKEvent;
            try
            {
                newKEvent = new KEventDto();
                if (entityInfo != null)
                    newKEvent.SetSimpleDto(entityInfo);

                // TODO: load default values
                // for newKEvent

                if (newKEvent.KEventId == Guid.Empty)
                    newKEvent.KEventId = Guid.NewGuid();

                resService.Entity = newKEvent;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            return ResultDomainAction(resService);
        }

        public Result<KEventInfoDto> Save(KEventInfoDto entityInfo)
        {
            Result<KEvent> resRep = null;
            var resService = new Result<KEventInfoDto>();

            try
            {
                if (entityInfo.KEventId == Guid.Empty)
                {
                    entityInfo.KEventId = Guid.NewGuid();
                    var newEntity = new KEvent();
                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = _repository.KEvents.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.KEvents.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.KEvents.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.KEvents.Get(entityInfo.KEventId).Entity;

                    if (flagThrowKntException == true)
                        _repository.KEvents.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = _repository.KEvents.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KEvent();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = _repository.KEvents.Add(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            
            resService.Entity = resRep.Entity?.GetSimpleDto<KEventInfoDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<KEventInfoDto> Delete(Guid id)
        {
            var resService = new Result<KEventInfoDto>();
            try
            {
                var resRep = _repository.KEvents.Get(id);
                if (resRep.IsValid)
                {
                    resRep = _repository.KEvents.Delete(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<KEventInfoDto>();
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
