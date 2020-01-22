using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.DomainModel.Repositories;
using KNote.Shared;
using KNote.DomainModel.Entities;

// TODO: Pendiente de eliminar
//using KNote.DomainModel.Dto;
using KNote.Shared.Dto;


using KNote.DomainModel.Infrastructure;
using System.Linq.Expressions;

namespace KNote.DomainModel.Services
{
    public class KntKAttributeService : DomainActionBase, IKntKAttributeService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        protected internal KntKAttributeService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntAttributeService

        public Result<List<KAttributeInfoDto>> GetAll()
        {
            var resService = new Result<List<KAttributeInfoDto>>();
            try
            {
                var resRep = _repository.KAttributes.GetAll();
                resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<KAttributeInfoDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<KAttributeInfoDto> Get(string key)
        {
            var resService = new Result<KAttributeInfoDto>();
            try
            {
                var resRep = _repository.KAttributes.Get(ka => ka.Key == key);
                resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeInfoDto>();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<KAttributeInfoDto> Get(Guid id)
        {
            var resService = new Result<KAttributeInfoDto>();
            try
            {
                var resRep = _repository.KAttributes.Get((object) id);
                resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeInfoDto>();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<KAttributeDto> GetFull(Guid id)
        {            
            var resService = new Result<KAttributeDto>();
            try
            {
                var resRep = _repository.KAttributes.Get((object)id);
                if (!resRep.IsValid)
                    CopyErrorList(resRep.ErrorList, resService.ErrorList);
                resRep = _repository.KAttributes.LoadCollection(resRep.Entity, tv => tv.KAttributeTabulatedValues);
                if (!resRep.IsValid)
                    CopyErrorList(resRep.ErrorList, resService.ErrorList);
                //
                resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeDto>();
                resService.Entity.KAttributeTabulatedValuesInfo = resRep.Entity?.KAttributeTabulatedValues?
                    .Select(m => m.GetSimpleDto<KAttributeTabulatedValueInfoDto>()).ToList();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<KAttributeDto> New(KAttributeInfoDto kattributeInfo = null)
        {
            var resService = new Result<KAttributeDto>();
            KAttributeDto newAttribute;

            try
            {
                newAttribute = new KAttributeDto();
                if (kattributeInfo != null)
                    newAttribute.SetSimpleDto(kattributeInfo);

                // TODO: load default values
                // for new newAttribute

                if (newAttribute.KAttributeId == Guid.Empty)
                    newAttribute.KAttributeId = Guid.NewGuid();

                if (newAttribute.KAttributeTabulatedValuesInfo == null)
                    newAttribute.KAttributeTabulatedValuesInfo = new List<KAttributeTabulatedValueInfoDto>();

                resService.Entity = newAttribute;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            return ResultDomainAction(resService);            
        }

        public Result<KAttributeInfoDto> Save(KAttributeInfoDto entityInfo)
        {
            Result<KAttribute> resRep = null;
            var resService = new Result<KAttributeInfoDto>();

            try
            {
                if (entityInfo.KAttributeId == Guid.Empty)
                {
                    entityInfo.KAttributeId = Guid.NewGuid();
                    var newEntity = new KAttribute();

                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = _repository.KAttributes.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.KAttributes.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.KAttributes.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.KAttributes.Get(entityInfo.KAttributeId).Entity;

                    if (flagThrowKntException == true)
                        _repository.KAttributes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = _repository.KAttributes.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KAttribute();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = _repository.KAttributes.Add(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            
            resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeInfoDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<KAttributeTabulatedValueInfoDto> SaveTabulateValue(Guid attributeId, KAttributeTabulatedValueInfoDto entityInfo)
        {
            Result<KAttributeTabulatedValue> resRep = null;
            var resService = new Result<KAttributeTabulatedValueInfoDto>();

            try
            {
                if (entityInfo.KAttributeTabulatedValueId == Guid.Empty)
                {
                    entityInfo.KAttributeTabulatedValueId = Guid.NewGuid();
                    var newEntity = new KAttributeTabulatedValue();

                    newEntity.SetSimpleDto(entityInfo);
                    newEntity.KAttributeId = attributeId;
                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = _repository.KAttributeTabulatedValues.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.KAttributeTabulatedValues.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.KAttributeTabulatedValues.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.KAttributeTabulatedValues.Get(entityInfo.KAttributeId).Entity;

                    if (flagThrowKntException == true)
                        _repository.KAttributeTabulatedValues.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = _repository.KAttributeTabulatedValues.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KAttributeTabulatedValue();

                        newEntity.SetSimpleDto(entityInfo);
                        newEntity.KAttributeId = attributeId;
                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = _repository.KAttributeTabulatedValues.Add(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeTabulatedValueInfoDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<KAttributeDto> Delete(Guid id)
        {
            var resService = new Result<KAttributeDto>();
            try
            {
                var resRep = _repository.KAttributes.Get((object) id);
                if (resRep.IsValid)
                {
                    resRep = _repository.KAttributes.Delete(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeDto>();
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

        public Result<KAttributeTabulatedValueInfoDto> AddNewKAttributeTabulatedValue(Guid id, KAttributeTabulatedValueInfoDto entityInfo)
        {
            var resService = new Result<KAttributeTabulatedValueInfoDto>();
            try
            {
                var resRep = _repository.KAttributes.Get((object) id);
                var kattribute = resRep.Entity;
                var tabulatedValue = new KAttributeTabulatedValue();
                tabulatedValue.SetSimpleDto(entityInfo);
                kattribute.KAttributeTabulatedValues.Add(tabulatedValue);

                resService.Entity = tabulatedValue.GetSimpleDto<KAttributeTabulatedValueInfoDto>();                    
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<KAttributeTabulatedValueInfoDto> DeleteKAttributeTabulatedValue(Guid id)
        {
            var resService = new Result<KAttributeTabulatedValueInfoDto>();
            try
            {
                var resRep = _repository.KAttributeTabulatedValues.Get((object)id);
                if (resRep.IsValid)
                {
                    resRep = _repository.KAttributeTabulatedValues.Delete(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeTabulatedValueInfoDto>();
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
