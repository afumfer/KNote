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
            ThrowKntException = throwKntException;
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

        public async Task<Result<UserDto>> AddAsync(UserDto entity)
        {
            var response = new Result<UserDto>();
            try
            {
                var newEntity = new User();
                newEntity.SetSimpleDto(entity);

                var resGenRep = await _users.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<UserDto>();
                response.ErrorList = resGenRep.ErrorList;
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
                bool flagThrowKntException = false;
                if (_users.ThrowKntException == true)
                {
                    flagThrowKntException = true;
                    _users.ThrowKntException = false;
                }

                var resGenRepGet = await _users.GetAsync(entity.UserId);
                User entityForUpdate;

                if (flagThrowKntException == true)
                    _users.ThrowKntException = true;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    resGenRep = await _users.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }

                response.Entity = resGenRep.Entity?.GetSimpleDto<UserDto>();
                response.ErrorList = resGenRep.ErrorList;
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
                var resGenRep = await _users.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;
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

        #region  IDisposable

        public virtual void Dispose()
        {
            _users.Dispose();
        }

        #endregion
    }
}
