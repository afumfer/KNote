using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework;

public class KntTraceNoteTypeRepository : KntRepositoryEFBase, IKntTraceNoteTypeRepository
{
    public KntTraceNoteTypeRepository(KntDbContext singletonContext, RepositoryRef repositoryRef)
        : base(singletonContext, repositoryRef)
    {
    }

    public KntTraceNoteTypeRepository(RepositoryRef repositoryRef)
        : base(repositoryRef)
    {
    }

    public async Task<Result<List<TraceNoteTypeDto>>> GetAllAsync()
    {
        var result = new Result<List<TraceNoteTypeDto>>();

        try
        {
            var ctx = GetOpenConnection();
            var traceNoteTypes = new GenericRepositoryEF<KntDbContext, TraceNoteType>(ctx);

            var resGenRep = await traceNoteTypes.GetAllAsync();
            result.Entity = resGenRep.Entity?
                .Select(t => t.GetSimpleDto<TraceNoteTypeDto>())
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

    public async Task<Result<TraceNoteTypeDto>> GetAsync(Guid id)
    {
        var result = new Result<TraceNoteTypeDto>();

        try
        {
            var ctx = GetOpenConnection();
            var traceNoteTypes = new GenericRepositoryEF<KntDbContext, TraceNoteType>(ctx);

            var resGenRep = await traceNoteTypes.GetAsync((object)id);

            result.Entity = resGenRep.Entity?.GetSimpleDto<TraceNoteTypeDto>();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<TraceNoteTypeDto>> AddAsync(TraceNoteTypeDto entity)
    {
        var result = new Result<TraceNoteTypeDto>();

        try
        {
            var ctx = GetOpenConnection();
            var traceNoteTypes = new GenericRepositoryEF<KntDbContext, TraceNoteType>(ctx);

            var newEntity = new TraceNoteType();
            newEntity.SetSimpleDto(entity);

            var resGenRep = await traceNoteTypes.AddAsync(newEntity);

            result.Entity = resGenRep.Entity?.GetSimpleDto<TraceNoteTypeDto>();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<TraceNoteTypeDto>> UpdateAsync(TraceNoteTypeDto entity)
    {
        var result = new Result<TraceNoteTypeDto>();
        var resGenRep = new Result<TraceNoteType>();

        try
        {
            var ctx = GetOpenConnection();
            var traceNoteTypes = new GenericRepositoryEF<KntDbContext, TraceNoteType>(ctx);

            var resGenRepGet = await traceNoteTypes.GetAsync(entity.TraceNoteTypeId);
            TraceNoteType entityForUpdate;

            if (resGenRepGet.IsValid)
            {
                entityForUpdate = resGenRepGet.Entity;
                entityForUpdate.SetSimpleDto(entity);
                resGenRep = await traceNoteTypes.UpdateAsync(entityForUpdate);
            }
            else
            {
                resGenRep.Entity = null;
                resGenRep.AddErrorMessage("Can't find entity for update.");
            }

            result.Entity = resGenRep.Entity?.GetSimpleDto<TraceNoteTypeDto>();
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
            var traceNoteTypes = new GenericRepositoryEF<KntDbContext, TraceNoteType>(ctx);

            var resGenRep = await traceNoteTypes.DeleteAsync(id);
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
