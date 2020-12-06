using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using System.Data.Common;
using Dapper;

using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper
{
    public class KntUserRepository : KntRepositoryBase, IKntUserRepository
    {        
        public KntUserRepository(DbConnection singletonConnection, bool throwKntException) : base(singletonConnection, throwKntException)
        {
        }

        public KntUserRepository(string conn, string provider, bool throwKntException = false)
            : base(conn, provider, throwKntException)
        {
        }

        public async Task<Result<List<UserDto>>> GetAllAsync(PaginationDto pagination = null)
        {
            var result = new Result<List<UserDto>>();
            try
            {
                var db = GetOpenConnection();

                IEnumerable<UserDto> entity;

                var sql = @"SELECT [UserId], [UserName], [EMail], [FullName], [RoleDefinition], [Disabled] FROM [Users] ORDER BY UserName ";

                if (pagination != null)
                {
                    if (db.GetType().Name == "SqliteConnection")
                        sql += " LIMIT @NumRecords OFFSET @NumRecords * (@Page - 1) ;";
                    else                        
                        sql += " OFFSET @NumRecords * (@Page - 1) ROWS FETCH NEXT @NumRecords ROWS ONLY;";

                    entity = await db.QueryAsync<UserDto>(sql.ToString(), new { Page = pagination.Page, NumRecords = pagination.NumRecords });
                }
                else
                {
                    entity = await db.QueryAsync<UserDto>(sql.ToString(), new { });
                }
                                                                        
                result.Entity = entity.ToList();

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<int>> GetCount()
        {
            var resService = new Result<int>();

            try
            {
                var db = GetOpenConnection();

                var sql = "SELECT COUNT(*) FROM Users";
                resService.Entity = await db.ExecuteScalarAsync<int>(sql);

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            return ResultDomainAction(resService);
        }

        public async Task<Result<UserDto>> GetAsync(Guid id)
        {            
            var result = new Result<UserDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"SELECT [UserId], [UserName], [EMail], [FullName], [RoleDefinition], [Disabled] FROM [Users]  
                        WHERE UserId = @Id";

                var entity = await db.QueryFirstOrDefaultAsync<UserDto>(sql.ToString(), new { Id = id });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<UserInternalDto>> GetInternalAsync(string userName)
        {            
            var result = new Result<UserInternalDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"SELECT [UserId], [UserName], [EMail], [FullName], [RoleDefinition], [Disabled], [PasswordHash], [PasswordSalt] FROM [Users]  
                        WHERE UserName = @UserName";

                var entity = await db.QueryFirstOrDefaultAsync<UserInternalDto>(sql.ToString(), new { UserName = userName });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<UserDto>> AddAsync(UserDto entity)
        {
            var result = new Result<UserDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"INSERT INTO Users (UserId, UserName, EMail, FullName, RoleDefinition, Disabled )
                            VALUES (@UserId, @UserName, @EMail, @FullName, @RoleDefinition, @Disabled)";

                var r = await db.ExecuteAsync(sql.ToString(),
                    new { entity.UserId, entity.UserName, entity.EMail, entity.FullName, entity.RoleDefinition, entity.Disabled });

                if (r == 0)
                    result.ErrorList.Add("Entity not inserted");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<UserDto>> UpdateAsync(UserDto entity)
        {
            var result = new Result<UserDto>();
            try
            {
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
                    result.ErrorList.Add("Entity not updated");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<UserInternalDto>> AddInternalAsync(UserInternalDto entity)
        {
            var result = new Result<UserInternalDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"INSERT INTO Users (UserId, UserName, EMail, FullName, RoleDefinition, Disabled, PasswordHash, PasswordSalt )
                            VALUES (@UserId, @UserName, @EMail, @FullName, @RoleDefinition, @Disabled, @PasswordHash, @PasswordSalt)";

                var r = await db.ExecuteAsync(sql.ToString(),
                    new { entity.UserId, entity.UserName, entity.EMail, entity.FullName, entity.RoleDefinition, entity.Disabled, entity.PasswordHash, entity.PasswordSalt });

                if (r == 0)
                    result.ErrorList.Add("Entity not inserted");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var result = new Result();
            try
            {
                var db = GetOpenConnection();

                var sql = @"DELETE FROM Users WHERE UserId = @Id";

                var r = await db.ExecuteAsync(sql.ToString(), new { Id = id });

                if (r == 0)
                    result.AddErrorMessage("Entity not deleted");

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public Task<Result<List<KMessageDto>>> GetMessagesAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

    }
}
