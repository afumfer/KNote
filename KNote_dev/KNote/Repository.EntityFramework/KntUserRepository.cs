using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework
{
    public class KntUserRepository : KntRepositoryBase, IKntUserRepository
    {
        public KntUserRepository(KntDbContext singletonContext, RepositoryRef repositoryRef, bool throwKntException)
            : base(singletonContext, repositoryRef, throwKntException)
        {
        }

        public KntUserRepository(RepositoryRef repositoryRef, bool throwKntException = false)
            : base(repositoryRef, throwKntException)
        {
        }

        public async Task<Result<List<UserDto>>> GetAllAsync(PaginationDto pagination = null)
        {
            var resService = new Result<List<UserDto>>();
            try
            {
                var ctx = GetOpenConnection();
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx, ThrowKntException);

                if (pagination != null)
                {
                    var query = users.Queryable
                        .OrderBy(u => u.UserName)
                        .Pagination(pagination);
                    resService.Entity = await query
                        .Select(u => u.GetSimpleDto<UserDto>())
                        .ToListAsync();
                }
                else
                {
                    var query = users.Queryable
                        .OrderBy(u => u.UserName);
                    resService.Entity = await query
                        .Select(u => u.GetSimpleDto<UserDto>())
                        .ToListAsync();
                }

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<int>> GetCount()
        {
            var resService = new Result<int>();
            try
            {
                var ctx = GetOpenConnection();
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx, ThrowKntException);

                resService.Entity = await users.Queryable.CountAsync();

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<UserDto>> GetAsync(Guid userId)
        {
            var resService = new Result<UserDto>();
            try
            {
                var ctx = GetOpenConnection();
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx, ThrowKntException);

                var resRep = await users.GetAsync((object)userId);

                resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();

                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<UserDto>> GetByUserNameAsync(string userName)
        {
            var resService = new Result<UserDto>();
            try
            {
                var ctx = GetOpenConnection();
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx, ThrowKntException);

                var resRep = await users.GetAsync(_ => _.UserName == userName);

                resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();

                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<UserInternalDto>> GetInternalAsync(string userName)
        {
            var resService = new Result<UserInternalDto>();
            try
            {
                var ctx = GetOpenConnection();
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx, ThrowKntException);

                var resRep = await users.GetAsync(u => u.UserName == userName);

                resService.Entity = resRep.Entity?.GetSimpleDto<UserInternalDto>();

                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<UserInternalDto>> AddInternalAsync(UserInternalDto entity)
        {
            var resService = new Result<UserInternalDto>();
            try
            {
                var ctx = GetOpenConnection();
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx, ThrowKntException);

                var newEntity = entity.GetSimpleDto<User>();

                var resRep = await users.AddAsync(newEntity);

                resService.Entity = resRep.Entity?.GetSimpleDto<UserInternalDto>();

                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<UserDto>> AddAsync(UserDto entity)
        {
            var response = new Result<UserDto>();
            try
            {
                var ctx = GetOpenConnection();
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx, ThrowKntException);

                var newEntity = new User();
                newEntity.SetSimpleDto(entity);

                var resGenRep = await users.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<UserDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<UserDto>> UpdateAsync(UserDto entity)
        {
            var resGenRep = new Result<User>();
            var response = new Result<UserDto>();

            try
            {
                var ctx = GetOpenConnection();
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx, ThrowKntException);

                bool flagThrowKntException = false;
                if (users.ThrowKntException == true)
                {
                    flagThrowKntException = true;
                    users.ThrowKntException = false;
                }

                var resGenRepGet = await users.GetAsync(entity.UserId);
                User entityForUpdate;

                if (flagThrowKntException == true)
                    users.ThrowKntException = true;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    resGenRep = await users.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }

                response.Entity = resGenRep.Entity?.GetSimpleDto<UserDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }

            return ResultDomainAction(response);
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var response = new Result();
            try
            {
                var ctx = GetOpenConnection();
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx, ThrowKntException);

                var resGenRep = await users.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public Task<Result<List<KMessageDto>>> GetMessagesAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
