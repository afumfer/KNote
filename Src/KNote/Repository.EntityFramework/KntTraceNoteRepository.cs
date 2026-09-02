using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework;

public class KntTraceNoteRepository : KntRepositoryEFBase, IKntTraceNoteRepository
{
    public KntTraceNoteRepository(KntDbContext singletonContext, RepositoryRef repositoryRef)
        : base(singletonContext, repositoryRef)
    {
    }

    public KntTraceNoteRepository(RepositoryRef repositoryRef)
        : base(repositoryRef)
    {
    }

    public async Task<Result<List<TraceNoteDto>>> GetAllByFromIdAsync(Guid fromId)
    {
        var result = new Result<List<TraceNoteDto>>();

        try
        {
            var ctx = GetOpenConnection();
            var traceNotes = new GenericRepositoryEF<KntDbContext, TraceNote>(ctx);

            var resGenRep = await traceNotes.GetAllAsync(t => t.FromId == fromId);
            result.Entity = resGenRep.Entity?
                .Select(t => t.GetSimpleDto<TraceNoteDto>())
                .OrderBy(t => t.Order)
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

    public async Task<Result<List<TraceNoteDto>>> GetAllByToIdAsync(Guid toId)
    {
        var result = new Result<List<TraceNoteDto>>();

        try
        {
            var ctx = GetOpenConnection();
            var traceNotes = new GenericRepositoryEF<KntDbContext, TraceNote>(ctx);

            var resGenRep = await traceNotes.GetAllAsync(t => t.ToId == toId);
            result.Entity = resGenRep.Entity?
                .Select(t => t.GetSimpleDto<TraceNoteDto>())
                .OrderBy(t => t.Order)
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

    public async Task<Result<TraceNoteDto>> GetAsync(Guid id)
    {
        var result = new Result<TraceNoteDto>();

        try
        {
            var ctx = GetOpenConnection();
            var traceNotes = new GenericRepositoryEF<KntDbContext, TraceNote>(ctx);

            var resGenRep = await traceNotes.GetAsync((object)id);

            result.Entity = resGenRep.Entity?.GetSimpleDto<TraceNoteDto>();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<TraceNoteDto>> AddAsync(TraceNoteDto entity)
    {
        var result = new Result<TraceNoteDto>();

        try
        {
            var ctx = GetOpenConnection();
            var traceNotes = new GenericRepositoryEF<KntDbContext, TraceNote>(ctx);

            var newEntity = new TraceNote();
            newEntity.SetSimpleDto(entity);

            var resGenRep = await traceNotes.AddAsync(newEntity);

            result.Entity = resGenRep.Entity?.GetSimpleDto<TraceNoteDto>();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<TraceNoteDto>> UpdateAsync(TraceNoteDto entity)
    {
        var result = new Result<TraceNoteDto>();
        var resGenRep = new Result<TraceNote>();

        try
        {
            var ctx = GetOpenConnection();
            var traceNotes = new GenericRepositoryEF<KntDbContext, TraceNote>(ctx);

            var resGenRepGet = await traceNotes.GetAsync(entity.TraceNoteId);
            TraceNote entityForUpdate;

            if (resGenRepGet.IsValid)
            {
                entityForUpdate = resGenRepGet.Entity;
                entityForUpdate.SetSimpleDto(entity);
                resGenRep = await traceNotes.UpdateAsync(entityForUpdate);
            }
            else
            {
                resGenRep.Entity = null;
                resGenRep.AddErrorMessage("Can't find entity for update.");
            }

            result.Entity = resGenRep.Entity?.GetSimpleDto<TraceNoteDto>();
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
            var traceNotes = new GenericRepositoryEF<KntDbContext, TraceNote>(ctx);

            var resGenRep = await traceNotes.DeleteAsync(id);
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

    public async Task<Result<List<TraceNoteTypeDto>>> GetAllTraceNoteTypesAsync()
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
}
