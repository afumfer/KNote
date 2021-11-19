﻿using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.EntityFramework.Entities;
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
        public KntUserRepository(KntDbContext singletonContext, RepositoryRef repositoryRef)
            : base(singletonContext, repositoryRef)
        {
        }

        public KntUserRepository(RepositoryRef repositoryRef)
            : base(repositoryRef)
        {
        }

        public async Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier pagination = null)
        {
            var resService = new Result<List<UserDto>>();
            try
            {
                var ctx = GetOpenConnection();
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

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

                resService.TotalCount = (await GetCount()).Entity;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<long>> GetCount()
        {
            var resService = new Result<long>();
            try
            {
                var ctx = GetOpenConnection();
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

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
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

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
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

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
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

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
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

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
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

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
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

                var resGenRepGet = await users.GetAsync(entity.UserId);
                User entityForUpdate;

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
                var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

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
