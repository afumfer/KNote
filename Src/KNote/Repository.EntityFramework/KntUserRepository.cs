using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework;

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
        var result = new Result<List<UserDto>>();

        try
        {
            var ctx = GetOpenConnection();
            var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

            if (pagination != null)
            {
                var query = users.Queryable
                    .OrderBy(u => u.UserName)
                    .Pagination(pagination);
                result.Entity = await query
                    .Select(u => u.GetSimpleDto<UserDto>())
                    .ToListAsync();
            }
            else
            {
                var query = users.Queryable
                    .OrderBy(u => u.UserName);
                result.Entity = await query
                    .Select(u => u.GetSimpleDto<UserDto>())
                    .ToListAsync();
            }

            result.TotalCount = (await GetCount()).Entity;

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<long>> GetCount()
    {
        var result = new Result<long>();

        try
        {
            var ctx = GetOpenConnection();
            var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

            result.Entity = await users.Queryable.CountAsync();

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<UserDto>> GetAsync(Guid userId)
    {
        var result = new Result<UserDto>();

        try
        {
            var ctx = GetOpenConnection();
            var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

            var resRep = await users.GetAsync((object)userId);

            result.Entity = resRep.Entity?.GetSimpleDto<UserDto>();

            result.AddListErrorMessage(resRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<UserDto>> GetByUserNameAsync(string userName)
    {
        var result = new Result<UserDto>();

        try
        {
            var ctx = GetOpenConnection();
            var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

            var resRep = await users.GetAsync(_ => _.UserName == userName);

            result.Entity = resRep.Entity?.GetSimpleDto<UserDto>();

            result.AddListErrorMessage(resRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<UserInternalDto>> GetInternalAsync(string userName)
    {
        var result = new Result<UserInternalDto>();

        try
        {
            var ctx = GetOpenConnection();
            var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

            var resRep = await users.GetAsync(u => u.UserName == userName);

            result.Entity = resRep.Entity?.GetSimpleDto<UserInternalDto>();

            result.AddListErrorMessage(resRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<UserInternalDto>> AddInternalAsync(UserInternalDto entity)
    {
        var result = new Result<UserInternalDto>();

        try
        {
            var ctx = GetOpenConnection();
            var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

            var newEntity = entity.GetSimpleDto<User>();

            var resRep = await users.AddAsync(newEntity);

            result.Entity = resRep.Entity?.GetSimpleDto<UserInternalDto>();

            result.AddListErrorMessage(resRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<UserDto>> AddAsync(UserDto entity)
    {
        var result = new Result<UserDto>();

        try
        {
            var ctx = GetOpenConnection();
            var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

            var newEntity = new User();
            newEntity.SetSimpleDto(entity);

            var resGenRep = await users.AddAsync(newEntity);

            result.Entity = resGenRep.Entity?.GetSimpleDto<UserDto>();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result<UserDto>> UpdateAsync(UserDto entity)
    {
        var result = new Result<UserDto>();
        var resGenRep = new Result<User>();

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

            result.Entity = resGenRep.Entity?.GetSimpleDto<UserDto>();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var result = new Result();

        try
        {
            var ctx = GetOpenConnection();
            var users = new GenericRepositoryEF<KntDbContext, User>(ctx);

            var resGenRep = await users.DeleteAsync(id);
            if (!resGenRep.IsValid)
                result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        return result;
    }

}
