using System.Data.Common;
using Dapper;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper;

public class KntNoteTypeRepository : KntRepositoryBase, IKntNoteTypeRepository
{        
    public KntNoteTypeRepository(DbConnection singletonConnection, RepositoryRef repositoryRef) 
        : base(singletonConnection, repositoryRef)
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
            var db = GetOpenConnection();

            var sql = @"SELECT NoteTypeId, Name, Description, ParenNoteTypeId FROM [NoteTypes] ORDER BY Name;";
            var entity = await db.QueryAsync<NoteTypeDto>(sql.ToString(), new { });
            result.Entity = entity.ToList();

            await CloseIsTempConnection(db);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }
        return ResultDomainAction(result);
    }

    public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
    {
        var result = new Result<NoteTypeDto>();
        try
        {
            var db = GetOpenConnection();

            var sql = @"SELECT NoteTypeId, Name, Description, ParenNoteTypeId FROM NoteTypes 
                    WHERE NoteTypeId = @Id";
            var entity =  await db.QueryFirstOrDefaultAsync<NoteTypeDto>(sql.ToString(), new { Id = id });
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

    public async Task<Result<NoteTypeDto>> AddAsync(NoteTypeDto entity)
    {
        var result = new Result<NoteTypeDto>();
        try
        {
            var db = GetOpenConnection();

            var sql = @"INSERT INTO NoteTypes (NoteTypeId, Name, Description, ParenNoteTypeId)
                        VALUES (@NoteTypeId, @Name, @Description, @ParenNoteTypeId)";
            var r = await db.ExecuteAsync(sql.ToString(), 
                new { entity.NoteTypeId, entity.Name, entity.Description, entity.ParenNoteTypeId });
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

    public async Task<Result<NoteTypeDto>> UpdateAsync(NoteTypeDto entity)
    {
        var result = new Result<NoteTypeDto>();
        try
        {
            var db = GetOpenConnection();

            var sql = @"UPDATE NoteTypes SET                     
                    Name = @Name
                    , Description = @Description
                    , ParenNoteTypeId = @ParenNoteTypeId
                WHERE NoteTypeId = @NoteTypeId";                
            var r = await db.ExecuteAsync(sql.ToString(),
                new { entity.NoteTypeId, entity.Name, entity.Description, entity.ParenNoteTypeId });
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

            var sql = @"DELETE FROM NoteTypes WHERE NoteTypeId = @Id";
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

