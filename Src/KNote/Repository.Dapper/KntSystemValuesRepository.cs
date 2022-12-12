using System.Data.Common;
using Dapper;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper;

public class KntSystemValuesRepository : KntRepositoryBase, IKntSystemValuesRepository
{        
    public KntSystemValuesRepository(DbConnection singletonConnection, RepositoryRef repositoryRef) 
        : base(singletonConnection, repositoryRef)
    {
    }

    public KntSystemValuesRepository(RepositoryRef repositoryRef)
            : base(repositoryRef)
    {
    }

    public async Task<Result<List<SystemValueDto>>> GetAllAsync()
    {
        var result = new Result<List<SystemValueDto>>();
        try
        {
            var db = GetOpenConnection();

            var sql = @"SELECT SystemValueId, Scope, [Key], [Value] FROM [SystemValues] ORDER BY Scope, [Key];";
            var entity = await db.QueryAsync<SystemValueDto>(sql.ToString(), new { });
            result.Entity = entity.ToList();

            await CloseIsTempConnection(db);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }
        return ResultDomainAction(result);
    }

    public async Task<Result<SystemValueDto>> GetAsync(string scope, string key)
    {
        var result = new Result<SystemValueDto>();
        try
        {
            var db = GetOpenConnection();

            var sql = @"SELECT  SystemValueId, Scope, [Key], [Value] FROM [SystemValues] 
                    WHERE Scope = @Scope and [Key] = @Key ";
            var entity = await db.QueryFirstOrDefaultAsync<SystemValueDto>(sql.ToString(), new { Scope = scope, Key = key });
            if (entity == null)
                result.AddErrorMessage("Entity not found.");
            result.Entity = entity;

            await CloseIsTempConnection(db);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }
        return ResultDomainAction(result);
    }

    public async Task<Result<SystemValueDto>> GetAsync(Guid id)
    {
        var result = new Result<SystemValueDto>();
        try
        {
            var db = GetOpenConnection();

            var sql = @"SELECT  SystemValueId, Scope, [Key], [Value] FROM [SystemValues] 
                    WHERE Id = @Id ";
            var entity = await db.QueryFirstOrDefaultAsync<SystemValueDto>(sql.ToString(), new { Id = id });
            if (entity == null)
                result.AddErrorMessage("Entity not found.");
            result.Entity = entity;

            await CloseIsTempConnection(db);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }
        return ResultDomainAction(result);
    }

    public async Task<Result<SystemValueDto>> AddAsync(SystemValueDto entity)
    {
        var result = new Result<SystemValueDto>();
        try
        {
            var db = GetOpenConnection();

            var sql = @"INSERT INTO SystemValues (SystemValueId, Scope, [Key], [Value])
                        VALUES (@SystemValueId, @Scope, @Key, @Value)";
            var r = await db.ExecuteAsync(sql.ToString(),
                new { entity.SystemValueId, entity.Scope, entity.Key, entity.Value });
            if (r == 0)
                result.AddErrorMessage("Entity not inserted");
            result.Entity = entity;

            await CloseIsTempConnection(db);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }
        return ResultDomainAction(result);
    }

    public async Task<Result<SystemValueDto>> UpdateAsync(SystemValueDto entity)
    {
        var result = new Result<SystemValueDto>();
        try
        {
            var db = GetOpenConnection();

            var sql = @"UPDATE SystemValues SET                     
                    Scope = @Scope, 
                    [Key] = @Key, 
                    [Value] = @Value
                WHERE SystemValueId = @SystemValueId";
            var r = await db.ExecuteAsync(sql.ToString(),
                new { entity.SystemValueId, entity.Scope, entity.Key, entity.Value });
            if (r == 0)
                result.AddErrorMessage("Entity not updated");
            result.Entity = entity;

            await CloseIsTempConnection(db);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }
        return ResultDomainAction(result);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var result = new Result();
        try
        {
            var db = GetOpenConnection();

            var sql = @"DELETE FROM SystemValues WHERE SystemValueId = @Id";
            var r = await db.ExecuteAsync(sql.ToString(), new { Id = id });
            if (r == 0)
                result.AddErrorMessage("Entity not deleted");

            await CloseIsTempConnection(db);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }
        return ResultDomainAction(result);
    }
}

