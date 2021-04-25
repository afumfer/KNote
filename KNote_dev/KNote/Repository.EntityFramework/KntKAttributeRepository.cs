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
    public class KntKAttributeRepository : KntRepositoryBase, IKntKAttributeRepository
    {
        public KntKAttributeRepository(KntDbContext singletonContext, RepositoryRef repositoryRef)
            : base(singletonContext, repositoryRef)
        {
        }

        public KntKAttributeRepository(RepositoryRef repositoryRef, bool throwKntException = false)
            : base(repositoryRef)
        {
        }

        public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync()
        {
            var resService = new Result<List<KAttributeInfoDto>>();
            try
            {
                var ctx = GetOpenConnection();
                var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

                var listAtr = await kattributes.DbSet                    
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

                await CloseIsTempConnection(ctx);
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

                var ctx = GetOpenConnection();
                var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

                var resRep = await kattributes.GetAllAsync(_ => _.NoteTypeId == typeId);

                resService.Entity = resRep.Entity?
                    .Select(a => a.GetSimpleDto<KAttributeInfoDto>())
                    .OrderBy(a => a.Order).ThenBy(a => a.Name)
                    .ToList();

                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
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

                var ctx = GetOpenConnection();
                var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

                var resRep = await kattributes.GetAllAsync(_ => _.NoteTypeId == null || _.NoteTypeId == typeId);

                resService.Entity = resRep.Entity?
                    .Select(a => a.GetSimpleDto<KAttributeInfoDto>())
                    .OrderBy(a => a.Order).ThenBy(a => a.Name)
                    .ToList();

                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();
                var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

                var resRep = await kattributes.GetAsync((object)id);
                if (!resRep.IsValid)
                    CopyErrorList(resRep.ErrorList, resService.ErrorList);
                resRep = kattributes.LoadCollection(resRep.Entity, tv => tv.KAttributeTabulatedValues);
                if (!resRep.IsValid)
                    CopyErrorList(resRep.ErrorList, resService.ErrorList);
                //
                resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeDto>();
                resService.Entity.KAttributeValues = resRep.Entity?.KAttributeTabulatedValues?
                    .Select(_ => _.GetSimpleDto<KAttributeTabulatedValueDto>()).OrderBy(_ => _.Order).ToList();

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<KAttributeDto>> AddAsync(KAttributeDto entity)
        {
            var response = new Result<KAttributeDto>();
            try
            {
                var ctx = GetOpenConnection();
                var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

                var newEntity = new KAttribute();
                newEntity.SetSimpleDto(entity);

                var resGenRep = await kattributes.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<KAttributeDto>();
                response.ErrorList = resGenRep.ErrorList;

                foreach (var value in entity.KAttributeValues)
                {
                    var res = await SaveTabulateValueAsync(response.Entity.KAttributeId, value);
                    response.Entity.KAttributeValues.Add(res.Entity);
                }

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<KAttributeDto>> UpdateAsync(KAttributeDto entity)
        {
            var resGenRep = new Result<KAttribute>();
            var response = new Result<KAttributeDto>();

            try
            {
                var ctx = GetOpenConnection();
                var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

                var resGenRepGet = await kattributes.GetAsync(entity.KAttributeId);
                KAttribute entityForUpdate;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    resGenRep = await kattributes.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }

                response.Entity = resGenRep.Entity?.GetSimpleDto<KAttributeDto>();

                foreach (var value in entity.KAttributeValues)
                {
                    var res = await SaveTabulateValueAsync(response.Entity.KAttributeId, value);
                    response.Entity.KAttributeValues.Add(res.Entity);
                }

                // TODO: hay que acumular los posibles errores del guardado de los hijos ??
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }

            return ResultDomainAction(response);
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var response = new Result();
            try
            {
                var ctx = GetOpenConnection();
                var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

                var resGenRep = await kattributes.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);

        }

        public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
        {
            var result = new Result<List<KAttributeTabulatedValueDto>>();
            try
            {
                var ctx = GetOpenConnection();                
                var kattributeTabulatedValues = new GenericRepositoryEF<KntDbContext, KAttributeTabulatedValue>(ctx);
                
                var resRep = await kattributeTabulatedValues.GetAllAsync(tv => tv.KAttributeId == attributeId);

                if (resRep.IsValid)
                {
                    result.Entity = resRep.Entity.Select(_ => _.GetSimpleDto<KAttributeTabulatedValueDto>()).OrderBy(_ => _.Order).ToList();
                }
                else
                    result.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<KAttributeTabulatedValueDto>> GetKAttributeTabulatedValueAsync(Guid attributeTabulateValueId)
        {
            var result = new Result<KAttributeTabulatedValueDto>();
            try
            {
                var ctx = GetOpenConnection();
                var kattributeTabulatedValues = new GenericRepositoryEF<KntDbContext, KAttributeTabulatedValue>(ctx);

                var resRep = await kattributeTabulatedValues.GetAsync(tv => tv.KAttributeTabulatedValueId == attributeTabulateValueId);

                if (resRep.IsValid)
                {
                    result.Entity = resRep.Entity.GetSimpleDto<KAttributeTabulatedValueDto>();
                }
                else
                    result.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result> DeleteKAttributeTabulatedValueAsync(Guid id)
        {
            var response = new Result();
            try
            {
                var ctx = GetOpenConnection();
                var kattributeTabulatedValues = new GenericRepositoryEF<KntDbContext, KAttributeTabulatedValue>(ctx);

                var resGenRep = await kattributeTabulatedValues.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);

        }


        #region Private methods

        private async Task<Result<KAttributeTabulatedValueDto>> SaveTabulateValueAsync(Guid attributeId, KAttributeTabulatedValueDto entity)
        {
            Result<KAttributeTabulatedValue> resRep = null;
            var resService = new Result<KAttributeTabulatedValueDto>();

            try
            {
                var ctx = GetOpenConnection();
                var kattributeTabulatedValues = new GenericRepositoryEF<KntDbContext, KAttributeTabulatedValue>(ctx);

                if (entity.KAttributeTabulatedValueId == Guid.Empty)
                {
                    entity.KAttributeTabulatedValueId = Guid.NewGuid();
                    var newEntity = new KAttributeTabulatedValue();

                    newEntity.SetSimpleDto(entity);
                    newEntity.KAttributeId = attributeId;
                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = await kattributeTabulatedValues.AddAsync(newEntity);
                }
                else
                {
                    var entityForUpdate = kattributeTabulatedValues.Get(entity.KAttributeTabulatedValueId).Entity;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await kattributeTabulatedValues.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new KAttributeTabulatedValue();

                        newEntity.SetSimpleDto(entity);
                        newEntity.KAttributeId = attributeId;
                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = await kattributeTabulatedValues.AddAsync(newEntity);
                    }
                }

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            resService.Entity = resRep.Entity?.GetSimpleDto<KAttributeTabulatedValueDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        #endregion 
    }
}
