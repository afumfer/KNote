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
    public class KntKMessageService : DomainActionBase, IKntKMessageService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        protected internal KntKMessageService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntKMessageService

        public async Task<Result<List<KMessageDto>>> GetAllAsync()
        {
            var resService = new Result<List<KMessageDto>>();
            try
            {
                var resRep = await _repository.KMessages.GetAllAsync();
                resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<KMessageDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<List<KMessageDto>>> GetAllForUserAsync(Guid id)
        {
            var resService = new Result<List<KMessageDto>>();
            try
            {
                var resRep = await _repository.KMessages.GetAllAsync( m => m.UserId == id);
                resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<KMessageDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<List<KMessageDto>>> GetAllForNoteAsync(Guid id)
        {
            var resService = new Result<List<KMessageDto>>();
            try
            {
                var resRep = await _repository.KMessages.GetAllAsync(m => m.NoteId == id);
                resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<KMessageDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<KMessageDto>> GetAsync(Guid id)
        {
            var resService = new Result<KMessageDto>();
            try
            {                
                var resRep = await _repository.KMessages.GetAsync((object)id);
                resService.Entity = resRep.Entity?.GetSimpleDto<KMessageDto>();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<KMessageDto>> SaveAsync(KMessageDto entityInfo)
        {
            Result<KMessage> resRep = null;
            var resService = new Result<KMessageDto>();

            try
            {
                if (entityInfo.KMessageId == Guid.Empty)
                {
                    entityInfo.KMessageId = Guid.NewGuid();
                    var newEntity = new KMessage();
                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = await _repository.KMessages.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.KMessages.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.KMessages.ThrowKntException = false;
                    }

                    resRep = await _repository.KMessages.GetAsync(entityInfo.KMessageId);
                    var entityForUpdate = resRep.Entity;

                    if (flagThrowKntException == true)
                        _repository.KMessages.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = await _repository.KMessages.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KMessage();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = await _repository.KMessages.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            resService.Entity = resRep.Entity?.GetSimpleDto<KMessageDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<KMessageDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<KMessageDto>();
            try
            {
                var resRep = await _repository.KMessages.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.KMessages.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<KMessageDto>();
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
