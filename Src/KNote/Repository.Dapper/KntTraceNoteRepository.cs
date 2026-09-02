using System.Data.Common;
using System.Reflection;
using Dapper;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper;

public class KntTraceNoteRepository : KntRepositoryDapperBase, IKntTraceNoteRepository
{
    public KntTraceNoteRepository(DbConnection singletonConnection, RepositoryRef repositoryRef)
        : base(singletonConnection, repositoryRef)
    {
    }

    public KntTraceNoteRepository(RepositoryRef repositoryRef)
        : base(repositoryRef)
    {
    }

    public async Task<Result<List<TraceNoteDto>>> GetAllByFromIdAsync(Guid fromId)
    {
        try
        {
            var result = new Result<List<TraceNoteDto>>();

            var db = GetOpenConnection();

            var sql = @"SELECT TraceNoteId, FromId, ToId, [Order], Weight, TraceNoteTypeId FROM TraceNotes
                    WHERE FromId = @FromId ORDER BY [Order]";
            var entity = await db.QueryAsync<TraceNoteDto>(sql.ToString(), new { FromId = fromId });
            result.Entity = entity.ToList();

            await CloseIsTempConnection(db);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<TraceNoteDto>>> GetAllByToIdAsync(Guid toId)
    {
        try
        {
            var result = new Result<List<TraceNoteDto>>();

            var db = GetOpenConnection();

            var sql = @"SELECT TraceNoteId, FromId, ToId, [Order], Weight, TraceNoteTypeId FROM TraceNotes
                    WHERE ToId = @ToId ORDER BY [Order]";
            var entity = await db.QueryAsync<TraceNoteDto>(sql.ToString(), new { ToId = toId });
            result.Entity = entity.ToList();

            await CloseIsTempConnection(db);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<TraceNoteDto>> GetAsync(Guid id)
    {
        try
        {
            var result = new Result<TraceNoteDto>();

            var db = GetOpenConnection();

            var sql = @"SELECT TraceNoteId, FromId, ToId, [Order], Weight, TraceNoteTypeId FROM TraceNotes
                    WHERE TraceNoteId = @Id";
            var entity = await db.QueryFirstOrDefaultAsync<TraceNoteDto>(sql.ToString(), new { Id = id });
            if (entity == null)
                result.AddErrorMessage("Entity not found.");
            result.Entity = entity;

            await CloseIsTempConnection(db);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<TraceNoteDto>> AddAsync(TraceNoteDto entity)
    {
        try
        {
            var result = new Result<TraceNoteDto>();

            var db = GetOpenConnection();

            var sql = @"INSERT INTO TraceNotes (TraceNoteId, FromId, ToId, [Order], Weight, TraceNoteTypeId)
                        VALUES (@TraceNoteId, @FromId, @ToId, @Order, @Weight, @TraceNoteTypeId)";
            var r = await db.ExecuteAsync(sql.ToString(),
                new { entity.TraceNoteId, entity.FromId, entity.ToId, entity.Order, entity.Weight, entity.TraceNoteTypeId });
            if (r == 0)
                result.AddErrorMessage("Entity not inserted");
            result.Entity = entity;

            await CloseIsTempConnection(db);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<TraceNoteDto>> UpdateAsync(TraceNoteDto entity)
    {
        try
        {
            var result = new Result<TraceNoteDto>();

            var db = GetOpenConnection();

            var sql = @"UPDATE TraceNotes SET
                    FromId = @FromId,
                    ToId = @ToId,
                    [Order] = @Order,
                    Weight = @Weight,
                    TraceNoteTypeId = @TraceNoteTypeId
                WHERE TraceNoteId = @TraceNoteId";
            var r = await db.ExecuteAsync(sql.ToString(),
                new { entity.TraceNoteId, entity.FromId, entity.ToId, entity.Order, entity.Weight, entity.TraceNoteTypeId });
            if (r == 0)
                result.AddErrorMessage("Entity not updated");
            result.Entity = entity;

            await CloseIsTempConnection(db);

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

            var db = GetOpenConnection();

            var sql = @"DELETE FROM TraceNotes WHERE TraceNoteId = @Id";
            var r = await db.ExecuteAsync(sql.ToString(), new { Id = id });
            if (r == 0)
                result.AddErrorMessage("Entity not deleted");

            await CloseIsTempConnection(db);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<TraceNoteTypeDto>>> GetAllTraceNoteTypesAsync()
    {
        try
        {
            var result = new Result<List<TraceNoteTypeDto>>();

            var db = GetOpenConnection();

            var sql = @"SELECT TraceNoteTypeId, Name, Description FROM TraceNoteTypes ORDER BY Name";
            var entity = await db.QueryAsync<TraceNoteTypeDto>(sql.ToString(), new { });
            result.Entity = entity.ToList();

            await CloseIsTempConnection(db);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }
}
