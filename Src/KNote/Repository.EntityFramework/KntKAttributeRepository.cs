using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace KNote.Repository.EntityFramework;

public class KntKAttributeRepository : KntRepositoryEFBase, IKntKAttributeRepository
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
        try
        {
            var result = new Result<List<KAttributeInfoDto>>();

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
                dto.NoteTypeDto = a.NoteType?.GetSimpleDto<NoteTypeDto>();
                listDto.Add(dto);
            }

            result.Entity = listDto;

            await CloseIsTempConnection(ctx);
        
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
    {
        try
        {        
            var result = new Result<List<KAttributeInfoDto>>();

            // TODO: pendiente de poblar la propiedad NoteTypeDto. Coger implementación de GetAllAsync().

            var ctx = GetOpenConnection();
            var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

            var resRep = await kattributes.GetAllAsync(_ => _.NoteTypeId == typeId);

            result.Entity = resRep.Entity?
                .Select(a => a.GetSimpleDto<KAttributeInfoDto>())
                .OrderBy(a => a.Order).ThenBy(a => a.Name)
                .ToList();

            result.AddListErrorMessage(resRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<KAttributeInfoDto>>> GetAllIncludeNullTypeAsync(Guid? typeId)
    {
        try
        {
            var result = new Result<List<KAttributeInfoDto>>();

            // TODO: pendiente de poblar la propiedad NoteTypeDto.  Coger implementación de GetAllAsync().

            var ctx = GetOpenConnection();
            var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

            var resRep = await kattributes.GetAllAsync(_ => _.NoteTypeId == null || _.NoteTypeId == typeId);

            result.Entity = resRep.Entity?
                .Select(a => a.GetSimpleDto<KAttributeInfoDto>())
                .OrderBy(a => a.Order).ThenBy(a => a.Name)
                .ToList();

            result.AddListErrorMessage(resRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<KAttributeDto>> GetAsync(Guid id)
    {
        try
        {
            var result = new Result<KAttributeDto>();

            var ctx = GetOpenConnection();
            var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

            var resRep = await kattributes.GetAsync((object)id);
            if (!resRep.IsValid)
                result.AddListErrorMessage(resRep.ListErrorMessage);
            resRep = kattributes.LoadCollection(resRep.Entity, tv => tv.KAttributeTabulatedValues);
            if (!resRep.IsValid)
                result.AddListErrorMessage(resRep.ListErrorMessage);
            //
            result.Entity = resRep.Entity?.GetSimpleDto<KAttributeDto>();
            result.Entity.KAttributeValues = resRep.Entity?.KAttributeTabulatedValues?
                .Select(_ => _.GetSimpleDto<KAttributeTabulatedValueDto>()).OrderBy(_ => _.Order).ToList();

            await CloseIsTempConnection(ctx);
        
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<KAttributeDto>> AddAsync(KAttributeDto entity)
    {
        try
        {
            var result = new Result<KAttributeDto>();

            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var ctx = GetOpenConnection();
                var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

                var newEntity = new KAttribute();
                newEntity.SetSimpleDto(entity);

                var resGenRep = await kattributes.AddAsync(newEntity);

                result.Entity = resGenRep.Entity?.GetSimpleDto<KAttributeDto>();
                result.AddListErrorMessage(resGenRep.ListErrorMessage);

                foreach (var value in entity.KAttributeValues)
                {
                    var res = await SaveTabulateValueAsync(ctx, result.Entity.KAttributeId, value);
                    if (!res.IsValid)
                        result.AddErrorMessage(res.ErrorMessage);
                    result.Entity.KAttributeValues.Add(res.Entity);
                }

                scope.Complete();

                await CloseIsTempConnection(ctx);
            }
    
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<KAttributeDto>> UpdateAsync(KAttributeDto entity)
    {

        try
        {
            var result = new Result<KAttributeDto>();
            var resGenRep = new Result<KAttribute>();

            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var ctx = GetOpenConnection();
                var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

                var resGenRepGet = await kattributes.GetAsync(entity.KAttributeId);
                KAttribute entityForUpdate;

                if (resGenRepGet.IsValid)
                {
                    // Check notetype in notes.
                    if (entity.NoteTypeId != null && resGenRepGet.Entity.NoteTypeId != entity.NoteTypeId)
                    {
                        var noteKAttributes = new GenericRepositoryEF<KntDbContext, NoteKAttribute>(ctx);
                        var nAttributes = (await noteKAttributes.GetAllAsync(n => n.KAttributeId == entity.KAttributeId)).Entity;
                        if (nAttributes.Count > 0)
                        {
                            result.AddErrorMessage("You can not change the note type for this attribute. This attribute is already being used by several notes. ");
                            result.Entity = entity;                                
                        }
                    }

                    if (result.IsValid)
                    {                            
                        entityForUpdate = resGenRepGet.Entity;
                        entityForUpdate.SetSimpleDto(entity);

                        resGenRep = await kattributes.UpdateAsync(entityForUpdate);

                        result.Entity = resGenRep.Entity?.GetSimpleDto<KAttributeDto>();
                        
                        var guidsUpdated = new List<Guid>();
                        foreach (var value in entity.KAttributeValues)
                        {
                            var res = await SaveTabulateValueAsync(ctx, result.Entity.KAttributeId, value);
                            if (!res.IsValid)
                                result.AddErrorMessage(res.ErrorMessage);
                            result.Entity.KAttributeValues.Add(res.Entity);
                            guidsUpdated.Add(value.KAttributeTabulatedValueId);
                        }

                        await DeleteNoContainsTabulateValueAsync(ctx, result.Entity.KAttributeId, guidsUpdated);

                        result.AddListErrorMessage(resGenRep.ListErrorMessage);
                    }                      
                }
                else
                {
                    result.Entity = entity;
                    result.AddErrorMessage("Can't find entity for update.");
                }
                                                       
                scope.Complete();

                await CloseIsTempConnection(ctx);
            }
    
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var result = new Result();

            var ctx = GetOpenConnection();
            var kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(ctx);

            var resGenRep = await kattributes.DeleteAsync(id);
            if (!resGenRep.IsValid)
                result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
            
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
    {
        try
        {
            var result = new Result<List<KAttributeTabulatedValueDto>>();

            var ctx = GetOpenConnection();                
            var kattributeTabulatedValues = new GenericRepositoryEF<KntDbContext, KAttributeTabulatedValue>(ctx);
            
            var resRep = await kattributeTabulatedValues.GetAllAsync(tv => tv.KAttributeId == attributeId);

            if (resRep.IsValid)
            {
                result.Entity = resRep.Entity.Select(_ => _.GetSimpleDto<KAttributeTabulatedValueDto>()).OrderBy(_ => _.Order).ToList();
            }
            else
                result.AddListErrorMessage(resRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    #region Private methods

    private async Task DeleteNoContainsTabulateValueAsync(KntDbContext ctx, Guid attributeId, List<Guid> guids)
    {
        var kattributeTabulatedValues = new GenericRepositoryEF<KntDbContext, KAttributeTabulatedValue>(ctx);
        var tabValuesForDelete = (await kattributeTabulatedValues.GetAllAsync(v => (v.KAttributeId == attributeId && !guids.Contains(v.KAttributeTabulatedValueId)))).Entity;            
        var res = kattributeTabulatedValues.DeleteRange(tabValuesForDelete);
    }

    private async Task<Result<KAttributeTabulatedValueDto>> SaveTabulateValueAsync(KntDbContext ctx, Guid attributeId, KAttributeTabulatedValueDto entity) 
    {

        try
        {                
            var result = new Result<KAttributeTabulatedValueDto>();
            Result<KAttributeTabulatedValue> resRep = null;

            var kattributeTabulatedValues = new GenericRepositoryEF<KntDbContext, KAttributeTabulatedValue>(ctx);
                         
            if (entity.KAttributeTabulatedValueId == Guid.Empty)
            {
                entity.KAttributeTabulatedValueId = Guid.NewGuid();
                var newEntity = new KAttributeTabulatedValue();
                newEntity.SetSimpleDto(entity);
                newEntity.KAttributeId = attributeId;
                resRep = await kattributeTabulatedValues.AddAsync(newEntity);
            }
            else
            {
                var entityForUpdate = kattributeTabulatedValues.Get(entity.KAttributeTabulatedValueId).Entity;
                if (entityForUpdate != null)
                {
                    entityForUpdate.SetSimpleDto(entity);
                    resRep = await kattributeTabulatedValues.UpdateAsync(entityForUpdate);
                }
                else
                {
                    var newEntity = new KAttributeTabulatedValue();
                    newEntity.SetSimpleDto(entity);
                    newEntity.KAttributeId = attributeId;
                    resRep = await kattributeTabulatedValues.AddAsync(newEntity);
                }
            }             

            result.Entity = resRep.Entity?.GetSimpleDto<KAttributeTabulatedValueDto>();
            result.AddListErrorMessage(resRep.ListErrorMessage);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    #endregion 
}
