using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using System.Linq.Expressions;
using KNote.Repository;
using KNote.Service.Interfaces;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;
using KNote.Repository.EntityFramework.Entities;

namespace KNote.Service.Services;

public class KntUserService : KntServiceBase, IKntUserService
{
    #region Constructor

    public KntUserService(IKntService service) : base(service)
    {

    }

    #endregion

    #region IKntUserService  

    public async Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier pagination = null)
    {
        var command = new KntUsersGetAllAsyncCommand(Service, pagination);
        return await ExecuteCommand(command);
    }

    public async Task<Result<UserDto>> GetAsync(Guid userId)
    {        
        var command = new KntUsersGetAsyncCommand(Service, userId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<UserDto>> GetByUserNameAsync(string userName)
    {
        var command = new KntUsersGetByUserNameAsyncCommand(Service, userName);
        return await ExecuteCommand(command);
    }

    public async Task<Result<UserDto>> SaveAsync(UserDto user)
    {
        var command = new KntUsersSaveAsyncCommand(Service, user);
        return await ExecuteCommand(command);
    }

    public async Task<Result<UserDto>> DeleteAsync(Guid id)
    {
        var command = new KntUsersDeleteAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<UserDto>> AuthenticateAsync(UserCredentialsDto userCredentials)   // string username, string password
    {
        var command = new KntUsersAuthenticateAsyncCommand(Service, userCredentials);
        return await ExecuteCommand(command);
    }
            
    public async Task<Result<UserDto>> CreateAsync(UserRegisterDto userRegisterInfo)
    {
        var command = new KntUsersCreateAsyncCommand(Service, userRegisterInfo);
        return await ExecuteCommand(command);

    }

    #endregion
}
