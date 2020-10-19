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
    public class KntUserRepository : DomainActionBase, IKntUserRepository
    {
        protected DbConnection _db;

        public KntUserRepository(DbConnection db, bool throwKntException)
        {
            _db = db;
            ThrowKntException = throwKntException;
        }

        public async Task<Result<List<UserDto>>> GetAllAsync(PaginationDto pagination = null)
        {
            var result = new Result<List<UserDto>>();
            try
            {
                IEnumerable<UserDto> entity;

                var sql = @"SELECT [UserId], [UserName], [EMail], [FullName], [RoleDefinition], [Disabled] FROM [Users] ORDER BY UserName ";

                if (pagination != null)
                {
                    sql += "OFFSET @NumRecords * (@Page - 1) ROWS FETCH NEXT @NumRecords ROWS ONLY;";
                    entity = await _db.QueryAsync<UserDto>(sql.ToString(), new { Page = pagination.Page, NumRecords = pagination.NumRecords });
                }
                else
                {
                    entity = await _db.QueryAsync<UserDto>(sql.ToString(), new { });
                }
                                                                        
                result.Entity = entity.ToList();
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
                var sql = "SELECT COUNT(*) FROM Users";
                resService.Entity = await _db.ExecuteScalarAsync<int>(sql);
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
                var sql = @"SELECT [UserId], [UserName], [EMail], [FullName], [RoleDefinition], [Disabled] FROM [Users]  
                        WHERE UserId = @Id";

                var entity = await _db.QueryFirstOrDefaultAsync<UserDto>(sql.ToString(), new { Id = id });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = entity;
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
                var sql = @"SELECT [UserId], [UserName], [EMail], [FullName], [RoleDefinition], [Disabled], [PasswordHash], [PasswordSalt] FROM [Users]  
                        WHERE UserName = @UserName";

                var entity = await _db.QueryFirstOrDefaultAsync<UserInternalDto>(sql.ToString(), new { UserName = userName });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = entity;
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
                var sql = @"INSERT INTO Users (UserId, UserName, EMail, FullName, RoleDefinition, Disabled )
                            VALUES (@UserId, @UserName, @EMail, @FullName, @RoleDefinition, @Disabled)";

                var r = await _db.ExecuteAsync(sql.ToString(),
                    new { entity.UserId, entity.UserName, entity.EMail, entity.FullName, entity.RoleDefinition, entity.Disabled });

                if (r == 0)
                    result.ErrorList.Add("Entity not inserted");

                result.Entity = entity;
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
                var sql = @"UPDATE Users SET 
                        UserName = @UserName, 
                        EMail = @EMail, 
                        FullName = @FullName, 
                        RoleDefinition = @RoleDefinition, 
                        Disabled = @Disabled
                    WHERE UserId = @UserId";

                var r = await _db.ExecuteAsync(sql.ToString(),
                    new { entity.UserId, entity.UserName, entity.EMail, entity.FullName, entity.RoleDefinition, entity.Disabled });

                if (r == 0)
                    result.ErrorList.Add("Entity not updated");

                result.Entity = entity;
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
                var sql = @"INSERT INTO Users (UserId, UserName, EMail, FullName, RoleDefinition, Disabled, PasswordHash, PasswordSalt )
                            VALUES (@UserId, @UserName, @EMail, @FullName, @RoleDefinition, @Disabled, @PasswordHash, @PasswordSalt)";

                var r = await _db.ExecuteAsync(sql.ToString(),
                    new { entity.UserId, entity.UserName, entity.EMail, entity.FullName, entity.RoleDefinition, entity.Disabled, entity.PasswordHash, entity.PasswordSalt });

                if (r == 0)
                    result.ErrorList.Add("Entity not inserted");

                result.Entity = entity;
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
                var sql = @"DELETE FROM Users WHERE UserId = @Id";

                var r = await _db.ExecuteAsync(sql.ToString(), new { Id = id });

                if (r == 0)
                    result.AddErrorMessage("Entity not deleted");

            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public void Dispose()
        {
            //
        }
    }
}
