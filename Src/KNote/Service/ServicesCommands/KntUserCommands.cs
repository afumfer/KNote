using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Repository.EntityFramework.Entities;


namespace KNote.Service.ServicesCommands;

public class KntUsersGetAllAsyncCommand : KntCommandServiceBase<PageIdentifier, Result<List<UserDto>>>
{
    public KntUsersGetAllAsyncCommand(IKntService service, PageIdentifier pageIdentifier) : base(service, pageIdentifier)
    {

    }

    public override async Task<Result<List<UserDto>>> Execute()
    {
        return await Repository.Users.GetAllAsync(Param);
    }
}

public class KntUsersGetAsyncCommand : KntCommandServiceBase<Guid, Result<UserDto>>
{
    public KntUsersGetAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<UserDto>> Execute()
    {
        return await Repository.Users.GetAsync(Param);
    }
}

public class KntUsersGetByUserNameAsyncCommand : KntCommandServiceBase<string, Result<UserDto>>
{
    public KntUsersGetByUserNameAsyncCommand(IKntService service, string userName) : base(service, userName)
    {

    }

    public override async Task<Result<UserDto>> Execute()
    {
        return await Repository.Users.GetByUserNameAsync(Param);
    }
}


public class KntUsersSaveAsyncCommand : KntCommandSaveServiceBase<UserDto, Result<UserDto>>
{
    public KntUsersSaveAsyncCommand(IKntService service, UserDto entity) : base(service, entity)
    {

    }

    public override async Task<Result<UserDto>> Execute()
    {
        if (Param.UserId == Guid.Empty)
        {
            Param.UserId = Guid.NewGuid();
            return await Repository.Users.AddAsync(Param);
        }
        else
        {
            return await Repository.Users.UpdateAsync(Param);
        }
    }
}

public class KntUsersDeleteAsyncCommand : KntCommandServiceBase<Guid, Result<UserDto>>
{
    public KntUsersDeleteAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<UserDto>> Execute()
    {
        var result = new Result<UserDto>();

        var resGetEntity = await Repository.Users.GetAsync(Param);

        if (resGetEntity.IsValid)
        {
            var resDelEntity = await Repository.Users.DeleteAsync(Param);
            if (resDelEntity.IsValid)
                result.Entity = resGetEntity.Entity;
            else
                result.AddListErrorMessage(resDelEntity.ListErrorMessage);
        }
        else
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
        }

        return result;
    }
}


public class KntUsersAuthenticateAsyncCommand : KntCommandServiceBase<UserCredentialsDto, Result<UserDto>>
{
    public KntUsersAuthenticateAsyncCommand(IKntService service, UserCredentialsDto userCredentials) : base(service, userCredentials)
    {

    }

    public override async Task<Result<UserDto>> Execute()
    {
        var resService = new Result<UserDto>();

        if (string.IsNullOrEmpty(Param.UserName) || string.IsNullOrEmpty(Param.Password))
        {
            resService.AddErrorMessage("User not authenticated");
            resService.Entity = null;
            return resService;
        }

        var resRep = await Repository.Users.GetInternalAsync(Param.UserName);

        if (!resRep.IsValid)
        {
            resService.AddErrorMessage("User not authenticated");
            resService.Entity = null;
            return resService;
        }

        if (!VerifyPasswordHash(Param.Password, resRep.Entity.PasswordHash, resRep.Entity.PasswordSalt))
        {
            resService.AddErrorMessage("User not authenticated");
            resService.Entity = null;
            return resService;
        }

        resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();
        return resService;
    }

    private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        if (password == null) throw new ArgumentNullException("password");
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
        if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
        if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

        using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
        {
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != storedHash[i]) return false;
            }
        }

        return true;
    }
}

public class KntUsersCreateAsyncCommand : KntCommandSaveServiceBase<UserRegisterDto, Result<UserDto>>
{
    public KntUsersCreateAsyncCommand(IKntService service, UserRegisterDto user) : base(service, user)
    {

    }

    public override async Task<Result<UserDto>> Execute()
    {
        var resService = new Result<UserDto>();
        var password = Param.Password;

        if (string.IsNullOrWhiteSpace(password))
            throw new AppException("Password is required");

        if ((await Repository.Users.GetInternalAsync(Param.UserName)).Entity != null)
            throw new AppException("Username \"" + Param.UserName + "\" is already taken");
        else
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            var newEntity = new UserInternalDto();
            newEntity.SetSimpleDto(Param);
            newEntity.UserId = Guid.NewGuid();
            newEntity.PasswordHash = passwordHash;
            newEntity.PasswordSalt = passwordSalt;

            var resRep = await Repository.Users.AddInternalAsync(newEntity);
            resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();
            if (!resRep.IsValid)
                resService.AddListErrorMessage(resRep.ListErrorMessage);
        }
        return resService;
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        if (password == null) throw new ArgumentNullException("password");
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

        using (var hmac = new System.Security.Cryptography.HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            // TODO: remove (for debug and seed data).
            //var strSalt = Convert.ToBase64String(passwordSalt);
            //var strHash = Convert.ToBase64String(passwordHash);
        }
    }

}

