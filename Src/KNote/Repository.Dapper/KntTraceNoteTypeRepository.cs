using System.Data.Common;
using System.Reflection;
using Dapper;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper;

public class KntTraceNoteTypeRepository : KntRepositoryDapperBase, IKntTraceNoteTypeRepository
{
    public KntTraceNoteTypeRepository(DbConnection singletonConnection, RepositoryRef repositoryRef)
        : base(singletonConnection, repositoryRef)
    {
    }

    public KntTraceNoteTypeRepository(RepositoryRef repositoryRef)
        : base(repositoryRef)
    {
    }

    public async Task<Result<List<TraceNoteTypeDto>>> GetAllAsync()
    {
        try
        {
            var result = new Result<List<TraceNoteTypeDto>>();

            var db = GetOpenConnection();

            var sql = @"SELECT TraceNoteTypeId, Name, Description FROM [TraceNoteTypes] ORDER BY Name;";
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

    public async Task<Result<TraceNoteTypeDto>> GetAsync(Guid id)
    {
        try
        {
            var result = new Result<TraceNoteTypeDto>();

            var db = GetOpenConnection();

            var sql = @"SELECT TraceNoteTypeId, Name, Description FROM TraceNoteTypes
                    WHERE TraceNoteTypeId = @Id";
            var entity = await db.QueryFirstOrDefaultAsync<TraceNoteTypeDto>(sql.ToString(), new { Id = id });
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

    public async Task<Result<TraceNoteTypeDto>> AddAsync(TraceNoteTypeDto entity)
    {
        try
        {
            var result = new Result<TraceNoteTypeDto>();

            var db = GetOpenConnection();

            var sql = @"INSERT INTO TraceNoteTypes (TraceNoteTypeId, Name, Description)
                        VALUES (@TraceNoteTypeId, @Name, @Description)";
            var r = await db.ExecuteAsync(sql.ToString(),
                new { entity.TraceNoteTypeId, entity.Name, entity.Description });
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

    public async Task<Result<TraceNoteTypeDto>> UpdateAsync(TraceNoteTypeDto entity)
    {
        try
        {
            var result = new Result<TraceNoteTypeDto>();

            var db = GetOpenConnection();

            var sql = @"UPDATE TraceNoteTypes SET
                    Name = @Name
                    , Description = @Description
                WHERE TraceNoteTypeId = @TraceNoteTypeId";
            var r = await db.ExecuteAsync(sql.ToString(),
                new { entity.TraceNoteTypeId, entity.Name, entity.Description });
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

            var sql = @"DELETE FROM TraceNoteTypes WHERE TraceNoteTypeId = @Id";
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
}
