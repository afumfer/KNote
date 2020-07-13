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
    public class KntUserRepository : DomainActionBase, IKntUserRepository
    {
        private IGenericRepositoryEF<KntDbContext, User> _users;

        public KntUserRepository(KntDbContext context, bool throwKntException)
        {
            _users = new GenericRepositoryEF<KntDbContext, User>(context, throwKntException);
        }


        public async Task<Result<List<UserDto>>> GetAllAsync(PaginationDto pagination = null)
        {
            var resService = new Result<List<UserDto>>();
            try
            {
                if (pagination != null)
                {
                    var query = _users.Queryable
                        .OrderBy(u => u.UserName)
                        .Pagination(pagination);
                    resService.Entity = await query
                        .Select(u => u.GetSimpleDto<UserDto>())
                        .ToListAsync();
                }
                else
                {
                    var query = _users.Queryable
                        .OrderBy(u => u.UserName);
                    resService.Entity = await query
                        .Select(u => u.GetSimpleDto<UserDto>())
                        .ToListAsync();
                }

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
                resService.Entity = await _users.Queryable.CountAsync();
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
                var resRep = await _users.GetAsync((object)userId);

                resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();

                resService.ErrorList = resRep.ErrorList;
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
                var resRep = await _users.GetAsync(u => u.UserName == userName);

                resService.Entity = resRep.Entity?.GetSimpleDto<UserInternalDto>();

                resService.ErrorList = resRep.ErrorList;
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
                var newEntity = entity.GetSimpleDto<User>();

                var resRep = await _users.AddAsync(newEntity);

                resService.Entity = resRep.Entity?.GetSimpleDto<UserInternalDto>();

                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<UserDto>> SaveAsync(UserDto entity)
        {
            Result<User> resRep = null;
            var resService = new Result<UserDto>();

            try
            {
                if (entity.UserId == Guid.Empty)
                {
                    entity.UserId = Guid.NewGuid();
                    var newEntity = new User();
                    newEntity.SetSimpleDto(entity);

                    resRep = await _users.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_users.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _users.ThrowKntException = false;
                    }

                    resRep = await _users.GetAsync(entity.UserId);
                    var entityForUpdate = resRep.Entity;

                    if (flagThrowKntException == true)
                        _users.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _users.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new User();
                        newEntity.SetSimpleDto(entity);

                        resRep = await _users.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            
            resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<UserDto>> DeleteAsync(Guid userId)
        {
            var resService = new Result<UserDto>();
            try
            {
                var resRep = await _users.GetAsync(userId);
                if (resRep.IsValid)
                {
                    resRep = await _users.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();
                    else
                        resService.ErrorList = resRep.ErrorList;
                }
                else
                    resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        #region  IDisposable

        public virtual void Dispose()
        {
            _users.Dispose();
        }

        #endregion
    }
}
