using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework;

public class KntNoteTypeRepository : KntRepositoryEFBase, IKntNoteTypeRepository
{
    public KntNoteTypeRepository(KntDbContext singletonContext, RepositoryRef repositoryRef)
        : base(singletonContext, repositoryRef)
    {
    }

    public KntNoteTypeRepository(RepositoryRef repositoryRef)
        : base(repositoryRef)
    {
    }

    public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
    {            
        var result = new Result<List<NoteTypeDto>>();

        try
        {
            var ctx = GetOpenConnection();
            var noteTypes = new GenericRepositoryEF<KntDbContext, NoteType>(ctx);
            
            var resGenRep = await noteTypes.GetAllAsync();
            result.Entity = resGenRep.Entity?
                .Select(t => t.GetSimpleDto<NoteTypeDto>())
                .OrderBy(t => t.Name)
                .ToList();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
    {
        var result = new Result<NoteTypeDto>();

        try
        {
            var ctx = GetOpenConnection();
            var noteTypes = new GenericRepositoryEF<KntDbContext, NoteType>(ctx);

            var resGenRep = await noteTypes.GetAsync((object)id);

            result.Entity = resGenRep.Entity?.GetSimpleDto<NoteTypeDto>();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);
            
            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<NoteTypeDto>> AddAsync(NoteTypeDto entity)
    {
        var result = new Result<NoteTypeDto>();

        try
        {
            var ctx = GetOpenConnection();
            var noteTypes = new GenericRepositoryEF<KntDbContext, NoteType>(ctx);

            var newEntity = new NoteType();
            newEntity.SetSimpleDto(entity);

            var resGenRep = await noteTypes.AddAsync(newEntity);

            result.Entity = resGenRep.Entity?.GetSimpleDto<NoteTypeDto>();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<NoteTypeDto>> UpdateAsync(NoteTypeDto entity)
    {
        var result = new Result<NoteTypeDto>();
        var resGenRep = new Result<NoteType>();            

        try
        {
            var ctx = GetOpenConnection();
            var noteTypes = new GenericRepositoryEF<KntDbContext, NoteType>(ctx);

            var resGenRepGet = await noteTypes.GetAsync(entity.NoteTypeId) ;
            NoteType entityForUpdate;

            if (resGenRepGet.IsValid)
            {
                entityForUpdate = resGenRepGet.Entity;
                entityForUpdate.SetSimpleDto(entity);
                resGenRep = await noteTypes.UpdateAsync(entityForUpdate);
            }
            else
            {
                resGenRep.Entity = null;
                resGenRep.AddErrorMessage("Can't find entity for update.");
            }
            
            result.Entity = resGenRep.Entity?.GetSimpleDto<NoteTypeDto>();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
        
        return result;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var result = new Result();

        try
        {
            var ctx = GetOpenConnection();
            var noteTypes = new GenericRepositoryEF<KntDbContext, NoteType>(ctx);

            var resGenRep = await noteTypes.DeleteAsync(id);
            if (!resGenRep.IsValid)
                result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }
}
