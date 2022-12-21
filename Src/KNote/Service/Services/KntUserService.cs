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
        //return await Repository.Users.GetAllAsync(pagination);
        var command = new KntUsersGetAllAsyncCommand(Service, pagination);
        return await ExecuteCommand(command);
    }

    public async Task<Result<UserDto>> GetAsync(Guid userId)
    {
        //return await Repository.Users.GetAsync(userId);
        var command = new KntUsersGetAsyncCommand(Service, userId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<UserDto>> GetByUserNameAsync(string userName)
    {
        //return await Repository.Users.GetByUserNameAsync(userName);
        var command = new KntUsersGetByUserNameAsyncCommand(Service, userName);
        return await ExecuteCommand(command);
    }

    public async Task<Result<UserDto>> SaveAsync(UserDto user)
    {
        //if (user.UserId == Guid.Empty)
        //{
        //    user.UserId = Guid.NewGuid();
        //    return await Repository.Users.AddAsync(user);
        //}
        //else
        //{
        //    return await Repository.Users.UpdateAsync(user);
        //}
        var command = new KntUsersSaveAsyncCommand(Service, user);
        return await ExecuteCommand(command);
    }

    public async Task<Result<UserDto>> DeleteAsync(Guid id)
    {
        //var result = new Result<UserDto>();

        //var resGetEntity = await GetAsync(id);

        //if (resGetEntity.IsValid)
        //{
        //    var resDelEntity = await Repository.Users.DeleteAsync(id);
        //    if (resDelEntity.IsValid)
        //        result.Entity = resGetEntity.Entity;
        //    else
        //        result.AddListErrorMessage(resDelEntity.ListErrorMessage);
        //}
        //else
        //{
        //    result.AddListErrorMessage(resGetEntity.ListErrorMessage);
        //}

        //return result;
        var command = new KntUsersDeleteAsyncCommand(Service, id);
        return await ExecuteCommand(command);

    }

    public async Task<Result<UserDto>> AuthenticateAsync(UserCredentialsDto userCredentials)   // string username, string password
    {
        //var resService = new Result<UserDto>();

        //if (string.IsNullOrEmpty(userCredentials.UserName) || string.IsNullOrEmpty(userCredentials.Password))
        //{
        //    resService.AddErrorMessage("User not authenticated");
        //    resService.Entity = null;
        //    return resService;
        //}

        //var resRep = await Repository.Users.GetInternalAsync(userCredentials.UserName);

        //if (!resRep.IsValid)
        //{
        //    resService.AddErrorMessage("User not authenticated");
        //    resService.Entity = null;
        //    return resService;
        //}

        //if (!VerifyPasswordHash(userCredentials.Password, resRep.Entity.PasswordHash, resRep.Entity.PasswordSalt))
        //{
        //    resService.AddErrorMessage("User not authenticated");
        //    resService.Entity = null;
        //    return resService;
        //}

        //resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();
        //return resService;
        var command = new KntUsersAuthenticateAsyncCommand(Service, userCredentials);
        return await ExecuteCommand(command);
    }
            
    public async Task<Result<UserDto>> CreateAsync(UserRegisterDto userRegisterInfo)
    {
        //Result<UserDto> resService = new Result<UserDto>();

        //var password = userRegisterInfo.Password;

        //try
        //{
        //    if (string.IsNullOrWhiteSpace(password))
        //        throw new AppException("Password is required");

        //    if ((await Repository.Users.GetInternalAsync(userRegisterInfo.UserName)).Entity != null)
        //            throw new AppException("Username \"" + userRegisterInfo.UserName + "\" is already taken");
        //    else
        //    {                                        
        //        byte[] passwordHash, passwordSalt;
        //        CreatePasswordHash(password, out passwordHash, out passwordSalt);

        //        var newEntity = new UserInternalDto();                    
        //        newEntity.SetSimpleDto(userRegisterInfo);
        //        newEntity.UserId = Guid.NewGuid();
        //        newEntity.PasswordHash = passwordHash;
        //        newEntity.PasswordSalt = passwordSalt;

        //        var resRep = await Repository.Users.AddInternalAsync(newEntity);
        //        resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();
        //        if (!resRep.IsValid)
        //            resService.AddListErrorMessage(resRep.ListErrorMessage);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    if (resService == null)
        //        resService = new Result<UserDto>();
        //    AddExecptionsMessagesToResult(ex, resService);
        //}
        //return ResultDomainAction(resService);
        var command = new KntUsersCreateAsyncCommand(Service, userRegisterInfo);
        return await ExecuteCommand(command);

    }

    #endregion

    #region Private helper methods

    //private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    //{
    //    if (password == null) throw new ArgumentNullException("password");
    //    if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

    //    using (var hmac = new System.Security.Cryptography.HMACSHA512())
    //    {
    //        passwordSalt = hmac.Key;
    //        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

    //        // TODO: remove (for debug and seed data).
    //        //var strSalt = Convert.ToBase64String(passwordSalt);
    //        //var strHash = Convert.ToBase64String(passwordHash);
    //    }
    //}

    //private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    //{
    //    if (password == null) throw new ArgumentNullException("password");
    //    if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
    //    if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
    //    if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

    //    using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
    //    {
    //        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
    //        for (int i = 0; i < computedHash.Length; i++)
    //        {
    //            if (computedHash[i] != storedHash[i]) return false;
    //        }
    //    }

    //    return true;
    //}

    #endregion

}
