using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.DomainModel.Repositories;
using KNote.Shared;
using KNote.DomainModel.Entities;
using KNote.Shared.Dto;
using KNote.Shared.Dto.Info;
using KNote.DomainModel.Infrastructure;

namespace KNote.DomainModel.Services
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

        public Result<List<KMessageInfoDto>> GetAll()
        {
            var resService = new Result<List<KMessageInfoDto>>();
            try
            {
                var resRep = _repository.KMessages.GetAll();
                resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<KMessageInfoDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<List<KMessageInfoDto>> GetAllForUser(Guid id)
        {
            var resService = new Result<List<KMessageInfoDto>>();
            try
            {
                var resRep = _repository.KMessages.GetAll( m => m.UserId == id);
                resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<KMessageInfoDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<List<KMessageInfoDto>> GetAllForNote(Guid id)
        {
            var resService = new Result<List<KMessageInfoDto>>();
            try
            {
                var resRep = _repository.KMessages.GetAll(m => m.NoteId == id);
                resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<KMessageInfoDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<KMessageInfoDto> Get(Guid id)
        {
            var resService = new Result<KMessageInfoDto>();
            try
            {                
                var resRep = _repository.KMessages.Get((object)id);
                resService.Entity = resRep.Entity?.GetSimpleDto<KMessageInfoDto>();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<KMessageDto> New(KMessageInfoDto entityInfo = null)
        {
            var resService = new Result<KMessageDto>();
            KMessageDto newKMessage;

            try
            {
                newKMessage = new KMessageDto();
                if (entityInfo != null)
                    newKMessage.SetSimpleDto(entityInfo);

                if (newKMessage.UserId == Guid.Empty)
                    newKMessage.UserId = Guid.NewGuid();

                // TODO: load default values
                // for newKMessage

                // newKMessage.NoteInfoDto = ...
                // newKMessage.UserInfoDto = ...

                resService.Entity = newKMessage;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            return ResultDomainAction(resService);
        }

        public Result<KMessageInfoDto> Save(KMessageInfoDto entityInfo)
        {
            Result<KMessage> resRep = null;
            var resService = new Result<KMessageInfoDto>();

            try
            {
                if (entityInfo.KMessageId == Guid.Empty)
                {
                    entityInfo.UserId = Guid.NewGuid();
                    var newEntity = new KMessage();
                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = _repository.KMessages.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.KMessages.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.KMessages.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.KMessages.Get(entityInfo.KMessageId).Entity;

                    if (flagThrowKntException == true)
                        _repository.KMessages.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = _repository.KMessages.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KMessage();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = _repository.KMessages.Add(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            
            resService.Entity = resRep.Entity?.GetSimpleDto<KMessageInfoDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<KMessageInfoDto>> SaveAsync(KMessageInfoDto entityInfo)
        {
            Result<KMessage> resRep = null;
            var resService = new Result<KMessageInfoDto>();

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

            resService.Entity = resRep.Entity?.GetSimpleDto<KMessageInfoDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<KMessageInfoDto> Delete(Guid id)
        {
            var resService = new Result<KMessageInfoDto>();
            try
            {
                var resRep = _repository.KMessages.Get(id);
                if (resRep.IsValid)
                {
                    resRep = _repository.KMessages.Delete(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<KMessageInfoDto>();
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

        public async Task<Result<KMessageInfoDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<KMessageInfoDto>();
            try
            {
                var resRep = await _repository.KMessages.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.KMessages.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<KMessageInfoDto>();
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
