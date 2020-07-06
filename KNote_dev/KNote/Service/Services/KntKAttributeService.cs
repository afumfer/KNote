using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Repository.EntityFramework;
using KNote.Model;
using KNote.Repository.Entities;
using KNote.Model.Dto;

using KNote.Repository.EntityFramework;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using KNote.Service;
using KNote.Repository;

namespace KNote.Service.Services
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

        public async Task<Result<List<KAttributeDto>>> GetAllAsync()
        {            
            var resService = new Result<List<KAttributeDto>>();
            try
            {
                var listAtr = await _repository.KAttributes.DbSet
                    .Include(a => a.NoteType)
                    .OrderBy(a => a.Order).ThenBy(a => a.Name)
                    .ToListAsync();

                List<KAttributeDto> listDto = new List<KAttributeDto>();

                foreach(var a in listAtr)
                {
                    var dto = a.GetSimpleDto<KAttributeDto>();                    
                    dto.NoteTypeDto = a.NoteType?.GetSimpleDto<NoteTypeDto>(); ;
                    listDto.Add(dto);
                }

                resService.Entity = listDto;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
        {
            var resService = new Result<List<KAttributeInfoDto>>();
            try
            {
                var resRep = await _repository.KAttributes.GetAllAsync(_ => _.NoteTypeId == typeId);

                resService.Entity = resRep.Entity?
                    .Select(a => a.GetSimpleDto<KAttributeInfoDto>())
                    .OrderBy(a => a.Order).ThenBy(a => a.Name)
                    .ToList();

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
                if (!resRep.IsValid)
                    CopyErrorList(resRep.ErrorList, resService.ErrorList);
                resRep = _repository.KAttributes.LoadCollection(resRep.Entity, tv => tv.KAttributeTabulatedValues);
                if (!resRep.IsValid)
                    CopyErrorList(resRep.ErrorList, resService.ErrorList);
                //
                resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeDto>();
                resService.Entity.KAttributeValues = resRep.Entity?.KAttributeTabulatedValues?
                    .Select(_ => _.GetSimpleDto<KAttributeTabulatedValueDto>()).OrderBy(_ => _.Order).ToList();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
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
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _repository.KAttributes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KAttribute();
                        newEntity.SetSimpleDto(entity);

                        resRep = await _repository.KAttributes.AddAsync(newEntity);
                    }
                }
               
                resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeDto>();

                foreach(var value in entity.KAttributeValues)
                {
                    var res = await SaveTabulateValueAsync(resService.Entity.KAttributeId, value);
                    resService.Entity.KAttributeValues.Add(res.Entity);
                }

            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            resService.ErrorList = resRep.ErrorList;
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

        public async Task<Result<KAttributeTabulatedValueDto>> SaveTabulateValueAsync(Guid attributeId, KAttributeTabulatedValueDto entity)
        {
            Result<KAttributeTabulatedValue> resRep = null;
            var resService = new Result<KAttributeTabulatedValueDto>();

            try
            {
                if (entity.KAttributeTabulatedValueId == Guid.Empty)
                {
                    entity.KAttributeTabulatedValueId = Guid.NewGuid();
                    var newEntity = new KAttributeTabulatedValue();

                    newEntity.SetSimpleDto(entity);
                    newEntity.KAttributeId = attributeId;
                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = await _repository.KAttributeTabulatedValues.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.KAttributeTabulatedValues.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.KAttributeTabulatedValues.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.KAttributeTabulatedValues.Get(entity.KAttributeTabulatedValueId).Entity;

                    if (flagThrowKntException == true)
                        _repository.KAttributeTabulatedValues.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _repository.KAttributeTabulatedValues.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KAttributeTabulatedValue();

                        newEntity.SetSimpleDto(entity);
                        newEntity.KAttributeId = attributeId;
                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = await _repository.KAttributeTabulatedValues.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeTabulatedValueDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<KAttributeTabulatedValueDto>> AddNewKAttributeTabulatedValueAsync(Guid id, KAttributeTabulatedValueDto entityInfo)
        {
            var resService = new Result<KAttributeTabulatedValueDto>();
            try
            {
                var resRep = await _repository.KAttributes.GetAsync((object) id);
                var kattribute = resRep.Entity;
                var tabulatedValue = new KAttributeTabulatedValue();
                tabulatedValue.SetSimpleDto(entityInfo);
                kattribute.KAttributeTabulatedValues.Add(tabulatedValue);

                resService.Entity = tabulatedValue.GetSimpleDto<KAttributeTabulatedValueDto>();                    
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<KAttributeTabulatedValueDto>> DeleteKAttributeTabulatedValueAsync(Guid id)
        {
            var resService = new Result<KAttributeTabulatedValueDto>();
            try
            {
                var resRep = await _repository.KAttributeTabulatedValues.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.KAttributeTabulatedValues.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeTabulatedValueDto>();
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

        public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
        {
            var resService = new Result<List<KAttributeTabulatedValueDto>>();
            try
            {
                var resRep = await _repository.KAttributeTabulatedValues.GetAllAsync(tv => tv.KAttributeId == attributeId);

                if (resRep.IsValid)
                {
                    resService.Entity = resRep.Entity.Select(_ => _.GetSimpleDto<KAttributeTabulatedValueDto>()).OrderBy(_ => _.Order).ToList();
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
