using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework
{
    public class KntKAttributeRepository : DomainActionBase, IKntKAttributeRepository
    {
        private IGenericRepositoryEF<KntDbContext, KAttribute > _kattributes;
        private IGenericRepositoryEF<KntDbContext, KAttributeTabulatedValue> _kattributeTabulatedValues;

        public KntKAttributeRepository(KntDbContext context, bool throwKntException)
        {
            _kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(context, throwKntException);
            _kattributeTabulatedValues = new GenericRepositoryEF<KntDbContext, KAttributeTabulatedValue>(context, throwKntException);
            ThrowKntException = throwKntException;
        }



        public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync()
        {
            var resService = new Result<List<KAttributeInfoDto>>();
            try
            {
                var listAtr = await _kattributes.DbSet                    
                    .Include(a => a.NoteType)
                    .OrderBy(a => a.Order).ThenBy(a => a.Name)
                    .ToListAsync();

                List<KAttributeInfoDto> listDto = new List<KAttributeInfoDto>();

                foreach (var a in listAtr)
                {
                    var dto = a.GetSimpleDto<KAttributeInfoDto>();
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
                // TODO: pendiente de poblar la propiedad NoteTypeDto. Coger implementación de GetAllAsync().

                var resRep = await _kattributes.GetAllAsync(_ => _.NoteTypeId == typeId);

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

        public async Task<Result<List<KAttributeInfoDto>>> GetAllIncludeNullTypeAsync(Guid? typeId)
        {
            var resService = new Result<List<KAttributeInfoDto>>();
            try
            {
                // TODO: pendiente de poblar la propiedad NoteTypeDto.  Coger implementación de GetAllAsync().

                var resRep = await _kattributes.GetAllAsync(_ => _.NoteTypeId == null || _.NoteTypeId == typeId);

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
                var resRep = await _kattributes.GetAsync((object)id);
                if (!resRep.IsValid)
                    CopyErrorList(resRep.ErrorList, resService.ErrorList);
                resRep = _kattributes.LoadCollection(resRep.Entity, tv => tv.KAttributeTabulatedValues);
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

                    resRep = await _kattributes.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_kattributes.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _kattributes.ThrowKntException = false;
                    }

                    var entityForUpdate = (await _kattributes.GetAsync(entity.KAttributeId)).Entity;

                    if (flagThrowKntException == true)
                        _kattributes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _kattributes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KAttribute();
                        newEntity.SetSimpleDto(entity);

                        resRep = await _kattributes.AddAsync(newEntity);
                    }
                }

                resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeDto>();

                foreach (var value in entity.KAttributeValues)
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
                var resRep = await _kattributes.GetAsync((object)id);
                if (resRep.IsValid)
                {
                    resRep = await _kattributes.DeleteAsync(resRep.Entity);
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

                    resRep = await _kattributeTabulatedValues.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_kattributeTabulatedValues.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _kattributeTabulatedValues.ThrowKntException = false;
                    }

                    var entityForUpdate = _kattributeTabulatedValues.Get(entity.KAttributeTabulatedValueId).Entity;

                    if (flagThrowKntException == true)
                        _kattributeTabulatedValues.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _kattributeTabulatedValues.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KAttributeTabulatedValue();

                        newEntity.SetSimpleDto(entity);
                        newEntity.KAttributeId = attributeId;
                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = await _kattributeTabulatedValues.AddAsync(newEntity);
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

        public async Task<Result<KAttributeTabulatedValueDto>> AddNewKAttributeTabulatedValueAsync(Guid id, KAttributeTabulatedValueDto entity)
        {
            var resService = new Result<KAttributeTabulatedValueDto>();
            try
            {
                var resRep = await _kattributes.GetAsync((object)id);
                var kattribute = resRep.Entity;
                var tabulatedValue = new KAttributeTabulatedValue();
                tabulatedValue.SetSimpleDto(entity);
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
                var resRep = await _kattributeTabulatedValues.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _kattributeTabulatedValues.DeleteAsync(resRep.Entity);
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
                var resRep = await _kattributeTabulatedValues.GetAllAsync(tv => tv.KAttributeId == attributeId);

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

        #region  IDisposable

        public virtual void Dispose()
        {
            _kattributes.Dispose();
            _kattributeTabulatedValues.Dispose();
        }

        #endregion
    }
}
