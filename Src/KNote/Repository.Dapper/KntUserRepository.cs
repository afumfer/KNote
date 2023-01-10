using System.Data.Common;
using System.Reflection;
using Dapper;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper;

public class KntUserRepository : KntRepositoryBase, IKntUserRepository
{        
    public KntUserRepository(DbConnection singletonConnection, RepositoryRef repositoryRef) 
        : base(singletonConnection, repositoryRef)
    {
    }

    public KntUserRepository(RepositoryRef repositoryRef)
        : base(repositoryRef)
    {
    }

    public async Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier pagination = null)
    {
        try
        {
            var result = new Result<List<UserDto>>();

            var db = GetOpenConnection();

            IEnumerable<UserDto> entity;

            var sql = @"SELECT [UserId], [UserName], [EMail], [FullName], [RoleDefinition], [Disabled] FROM [Users] ORDER BY UserName ";

            if (pagination != null)
            {
                if (db.GetType().Name == "SqliteConnection")
                    sql += " LIMIT @NumRecords OFFSET @Offset ;";
                else
                    sql += " OFFSET @Offset ROWS FETCH NEXT @NumRecords ROWS ONLY;";

                entity = await db.QueryAsync<UserDto>(sql.ToString(), new { Offset = pagination.Offset, NumRecords = pagination.PageSize });
            }
            else
            {
                entity = await db.QueryAsync<UserDto>(sql.ToString(), new { });
            }
                                                                        
            result.Entity = entity.ToList();
            result.TotalCount = (await GetCount()).Entity;

            await CloseIsTempConnection(db);
            
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<long>> GetCount()
    {
        try
        {
            var result = new Result<long>();

            var db = GetOpenConnection();

            var sql = "SELECT COUNT(*) FROM Users";
            result.Entity = await db.ExecuteScalarAsync<long>(sql);

            await CloseIsTempConnection(db);
        
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<UserDto>> GetAsync(Guid id)
    {            
        try
        {
            var result = new Result<UserDto>();

            var db = GetOpenConnection();

            var sql = @"SELECT [UserId], [UserName], [EMail], [FullName], [RoleDefinition], [Disabled] FROM [Users]  
                    WHERE UserId = @Id";

            var entity = await db.QueryFirstOrDefaultAsync<UserDto>(sql.ToString(), new { Id = id });

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

    public async Task<Result<UserDto>> GetByUserNameAsync(string userName)
    {
        try
        {
            var result = new Result<UserDto>();

            var db = GetOpenConnection();

            var sql = @"SELECT [UserId], [UserName], [EMail], [FullName], [RoleDefinition], [Disabled] FROM [Users]  
                    WHERE UserName = @userName";

            var entity = await db.QueryFirstOrDefaultAsync<UserDto>(sql.ToString(), new { userName });

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

    public async Task<Result<UserInternalDto>> GetInternalAsync(string userName)
    {            
        try
        {
            var result = new Result<UserInternalDto>();

            var db = GetOpenConnection();

            var sql = @"SELECT [UserId], [UserName], [EMail], [FullName], [RoleDefinition], [Disabled], [PasswordHash], [PasswordSalt] FROM [Users]  
                    WHERE UserName = @UserName";

            var entity = await db.QueryFirstOrDefaultAsync<UserInternalDto>(sql.ToString(), new { UserName = userName });

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

    public async Task<Result<UserDto>> AddAsync(UserDto entity)
    {
        try
        {
            var result = new Result<UserDto>();

            var db = GetOpenConnection();

            var sql = @"INSERT INTO Users (UserId, UserName, EMail, FullName, RoleDefinition, Disabled )
                        VALUES (@UserId, @UserName, @EMail, @FullName, @RoleDefinition, @Disabled)";

            var r = await db.ExecuteAsync(sql.ToString(),
                new { entity.UserId, entity.UserName, entity.EMail, entity.FullName, entity.RoleDefinition, entity.Disabled });

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

    public async Task<Result<UserDto>> UpdateAsync(UserDto entity)
    {
        try
        {
            var result = new Result<UserDto>();

            var db = GetOpenConnection();

            var sql = @"UPDATE Users SET 
                    UserName = @UserName, 
                    EMail = @EMail, 
                    FullName = @FullName, 
                    RoleDefinition = @RoleDefinition, 
                    Disabled = @Disabled
                WHERE UserId = @UserId";

            var r = await db.ExecuteAsync(sql.ToString(),
                new { entity.UserId, entity.UserName, entity.EMail, entity.FullName, entity.RoleDefinition, entity.Disabled });

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

    public async Task<Result<UserInternalDto>> AddInternalAsync(UserInternalDto entity)
    {
        try
        {
            var result = new Result<UserInternalDto>();

            var db = GetOpenConnection();

            var sql = @"INSERT INTO Users (UserId, UserName, EMail, FullName, RoleDefinition, Disabled, PasswordHash, PasswordSalt )
                        VALUES (@UserId, @UserName, @EMail, @FullName, @RoleDefinition, @Disabled, @PasswordHash, @PasswordSalt)";

            var r = await db.ExecuteAsync(sql.ToString(),
                new { entity.UserId, entity.UserName, entity.EMail, entity.FullName, entity.RoleDefinition, entity.Disabled, entity.PasswordHash, entity.PasswordSalt });

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

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var result = new Result();

            var db = GetOpenConnection();

            var sql = @"DELETE FROM Users WHERE UserId = @Id";

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

