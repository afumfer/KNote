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
                
                resService.Entity = resRep.Entity?
                    .Select(a => a.GetSimpleDto<KAttributeInfoDto>())
                    .OrderBy(a => a.Order )
                    .ToList();

                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<KAttributeDto> Get(Guid id)
        {
            var resService = new Result<KAttributeDto>();
            try
            {
                var resRep = _repository.KAttributes.Get((object) id);

                resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeDto>();
                // KNote template ... load here aditionals properties for UserDto
                // ... 

                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<KAttributeDto>> GetAsync(Guid id)
        {
            var resService = new Result<KAttributeDto>();
            try
            {
                var resRep = await _repository.KAttributes.GetAsync((object)id);
                
                resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeDto>();
                // KNote template ... load here aditionals properties for UserDto
                // ... 

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

        public Result<KAttributeDto> Save(KAttributeDto entity)
        {
            Result<KAttribute> resRep = null;
            var resService = new Result<KAttributeDto>();

            try
            {
                if (entity.KAttributeId == Guid.Empty)
                {
                    entity.KAttributeId = Guid.NewGuid();
                    var newEntity = new KAttribute();

                    newEntity.SetSimpleDto(entity);

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

                    var entityForUpdate = _repository.KAttributes.Get(entity.KAttributeId).Entity;

                    if (flagThrowKntException == true)
                        _repository.KAttributes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = _repository.KAttributes.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KAttribute();
                        newEntity.SetSimpleDto(entity);

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
            
            resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<KAttributeDto>> SaveAsync(KAttributeDto entity)
        {
            Result<KAttribute> resRep = null;
            var resService = new Result<KAttributeDto>();

            try
            {
                if (entity.KAttributeId == Guid.Empty)
                {
                    entity.KAttributeId = Guid.NewGuid();
                    var newEntity = new KAttribute();

                    newEntity.SetSimpleDto(entity);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = await _repository.KAttributes.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.KAttributes.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.KAttributes.ThrowKntException = false;
                    }

                    var entityForUpdate = (await _repository.KAttributes.GetAsync(entity.KAttributeId)).Entity;

                    if (flagThrowKntException == true)
                        _repository.KAttributes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _repository.KAttributes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KAttribute();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = await _repository.KAttributes.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<KAttributeInfoDto> Delete(Guid id)
        {
            var resService = new Result<KAttributeInfoDto>();
            try
            {
                var resRep = _repository.KAttributes.Get((object)id);
                if (resRep.IsValid)
                {
                    resRep = _repository.KAttributes.Delete(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeInfoDto>();
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

        public async Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<KAttributeInfoDto>();
            try
            {
                var resRep = await _repository.KAttributes.GetAsync((object)id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.KAttributes.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeInfoDto>();
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


        // TODO: Pendiente de refactorizar los tres siguientes métodos 
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
