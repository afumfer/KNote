using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Repository;
using KNote.Service.Core;
using KNote.Service.Interfaces;

namespace KNote.Service.Services
{
    public class KntKEventService : KntServiceBase, IKntKEventService
    {
        #region Fields

        // private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        //protected internal KntKEventService(IKntRepository repository)
        //{
        //    _repository = repository;
        //}

        public KntKEventService(IKntService service) : base(service)
        {

        }

        #endregion

        #region IKntKEventService  (Pendiente de implementar) 

        //public async Task<Result<List<KEventDto>>> GetAllAsync()
        //{
        //    var resService = new Result<List<KEventDto>>();
        //    try
        //    {
        //        var resRep = await _repository.KEvents.GetAllAsync();
        //        resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<KEventDto>()).ToList();
        //        resService.AddListErrorMessage(resRep.ListErrorMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        AddExecptionsMessagesToErrorsList(ex, resService);
        //    }
        //    return ResultDomainAction(resService);
        //}

        //public async Task<Result<KEventDto>> GetAsync(Guid eventId)
        //{
        //    var resService = new Result<KEventDto>();
        //    try
        //    {
        //        var resRep = await _repository.KEvents.GetAsync((object)eventId);
        //        resService.Entity = resRep.Entity?.GetSimpleDto<KEventDto>();
        //        resService.AddListErrorMessage(resRep.ListErrorMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        AddExecptionsMessagesToErrorsList(ex, resService);
        //    }
        //    return ResultDomainAction(resService);
        //}

        //public async Task<Result<KEventDto>> SaveAsync(KEventDto entity)
        //{
        //    Result<KEvent> resRep = null;
        //    var resService = new Result<KEventDto>();

        //    try
        //    {
        //        if (entity.KEventId == Guid.Empty)
        //        {
        //            entity.KEventId = Guid.NewGuid();
        //            var newEntity = new KEvent();
        //            newEntity.SetSimpleDto(entity);

        //            // TODO: update standard control values to newEntity
        //            // ...

        //            resRep = await _repository.KEvents.AddAsync(newEntity);
        //        }
        //        else
        //        {
        //            bool flagThrowKntException = false;

        //            if (_repository.KEvents.ThrowKntException == true)
        //            {
        //                flagThrowKntException = true;
        //                _repository.KEvents.ThrowKntException = false;
        //            }

        //            var entityForUpdate = (await _repository.KEvents.GetAsync(entity.KEventId)).Entity;

        //            if (flagThrowKntException == true)
        //                _repository.KEvents.ThrowKntException = true;

        //            if (entityForUpdate != null)
        //            {
        //                // TODO: update standard control values to entityForUpdate
        //                // ...
        //                entityForUpdate.SetSimpleDto(entity);
        //                resRep = await  _repository.KEvents.UpdateAsync(entityForUpdate);
        //            }
        //            else
        //            {
        //                var newEntity = new KEvent();
        //                newEntity.SetSimpleDto(entity);

        //                // TODO: update standard control values to newEntity
        //                // ...
        //                resRep = await _repository.KEvents.AddAsync(newEntity);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AddExecptionsMessagesToErrorsList(ex, resService);
        //    }

        //    resService.Entity = resRep.Entity?.GetSimpleDto<KEventDto>();
        //    resService.AddListErrorMessage(resRep.ListErrorMessage);

        //    return ResultDomainAction(resService);
        //}

        //public async Task<Result<KEventDto>> DeleteAsync(Guid id)
        //{
        //    var resService = new Result<KEventDto>();
        //    try
        //    {
        //        var resRep = await _repository.KEvents.GetAsync(id);
        //        if (resRep.IsValid)
        //        {
        //            resRep = await _repository.KEvents.DeleteAsync(resRep.Entity);
        //            if (resRep.IsValid)
        //                resService.Entity = resRep.Entity?.GetSimpleDto<KEventDto>();
        //            else
        //                resService.AddListErrorMessage(resRep.ListErrorMessage);
        //        }
        //        else
        //            resService.AddListErrorMessage(resRep.ListErrorMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        AddExecptionsMessagesToErrorsList(ex, resService);
        //    }
        //    return ResultDomainAction(resService);
        //}

        #endregion
    }
}
