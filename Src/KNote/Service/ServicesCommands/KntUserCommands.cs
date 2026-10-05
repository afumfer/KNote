using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

[KntAuthorize(EnumRoles.Guest)]
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

[KntAuthorize(EnumRoles.Guest)]
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

[KntAuthorize(EnumRoles.Guest)]
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


[KntAuthorize(EnumRoles.Admin)]
public class KntUsersSaveAsyncCommand : KntCommandSaveServiceBase<UserDto, Result<UserDto>>
{
    public KntUsersSaveAsyncCommand(IKntService service, UserDto entity) : base(service, entity)
    {

    }

    public override async Task<Result<UserDto>> Execute()
    {
        Result<UserDto> result;
        if (Param.UserId == Guid.Empty)
        {
            Param.UserId = Guid.NewGuid();
            result = await Repository.Users.AddAsync(Param);
        }
        else
        {
            result = await Repository.Users.UpdateAsync(Param);
        }

        // The saved user may be the current one (its roles, or whether it is disabled).
        Service.ResetCurrentUserRole();
        return result;
    }
}

[KntAuthorize(EnumRoles.Admin)]
public class KntUsersDeleteAsyncCommand : KntCommandServiceBase<Guid, Result<UserDto>>
{
    public KntUsersDeleteAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<UserDto>> Execute()
    {
        var result = new Result<UserDto>();

        var resGetEntity = await Repository.Users.GetAsync(Param);

        if (!resGetEntity.IsValid)
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
            return result;
        }

        // Business rule, checked here instead of letting it surface as a raw FK-constraint DB
        // error: a user still referenced by alarms/messages, post-it windows or tasks can't be
        // deleted. Living in the command (not in a caller like ClientWin's UserEditorCtrl) means
        // every consumer of this service - ClientWin and Server/Blazor's UsersController alike -
        // gets the same clear message. Mirrors KntNoteTypeDeleteAsyncCommand/
        // KntKAttributesDeleteAsyncCommand's in-use check.
        var messagesCount = await Repository.Notes.CountMessagesByUserAsync(Param);
        var windowsCount = await Repository.Notes.CountWindowsByUserAsync(Param);
        var tasksCount = await Repository.Notes.CountTasksByUserAsync(Param);

        var inUseReasons = new List<string>();
        if (messagesCount.IsValid && messagesCount.Entity > 0)
            inUseReasons.Add($"{messagesCount.Entity} alarm/message(s)");
        if (windowsCount.IsValid && windowsCount.Entity > 0)
            inUseReasons.Add($"{windowsCount.Entity} open note window(s)");
        if (tasksCount.IsValid && tasksCount.Entity > 0)
            inUseReasons.Add($"{tasksCount.Entity} task(s)");

        if (inUseReasons.Count > 0)
        {
            result.AddErrorMessage($"Can't delete this user: still referenced by {string.Join(", ", inUseReasons)}.");
            return result;
        }

        var resDelEntity = await Repository.Users.DeleteAsync(Param);
        if (resDelEntity.IsValid)
            result.Entity = resGetEntity.Entity;
        else
            result.AddListErrorMessage(resDelEntity.ListErrorMessage);

        // The deleted user may be the current one.
        Service.ResetCurrentUserRole();

        return result;
    }
}


[KntAllowAnonymous]
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
        if (storedHash == null) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "storedHash");
        if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "storedHash");
        if (storedSalt == null) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "storedSalt");
        if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "storedSalt");

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

[KntAuthorize(EnumRoles.Admin)]
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
            throw new Exception("Password is required");

        if ((await Repository.Users.GetInternalAsync(Param.UserName)).Entity != null)
            throw new Exception("Username \"" + Param.UserName + "\" is already taken");
        else if ((await Repository.Users.GetByEMailAsync(Param.EMail)).Entity != null)
            throw new Exception("Email \"" + Param.EMail + "\" is already in use");
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

            // The new user may be the current one, until now not registered (no role).
            Service.ResetCurrentUserRole();
        }
        return resService;
    }

    // Not private: reused by KntUsersSetPasswordAsyncCommand below (same file/namespace) so a
    // "reset password" action for an existing user hashes exactly the same way as account creation.
    internal static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
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

/// <summary>
/// Self-registration of a new user (ClientWin's "Register user" dialog, Server's api/users/register):
/// same as KntUsersCreateAsyncCommand, except that the role is decided here and never taken from the
/// caller - the RoleDefinition of the given UserRegisterDto is overwritten with it. A user created by
/// an Admin from the users management screen goes through KntUsersCreateAsyncCommand instead, keeping
/// the roles that Admin chose.
/// </summary>
[KntAllowAnonymous]
public class KntUsersRegisterAsyncCommand : KntUsersCreateAsyncCommand
{
    public KntUsersRegisterAsyncCommand(IKntService service, UserRegisterDto user) : base(service, user)
    {

    }

    public override async Task<Result<UserDto>> Execute()
    {
        var resUsers = await Repository.Users.GetAllAsync();
        if (!resUsers.IsValid)
        {
            var resService = new Result<UserDto>();
            resService.AddListErrorMessage(resUsers.ListErrorMessage);
            return resService;
        }

        var adminCount = resUsers.Entity.Count(u => KntRoles.IsInRole(u.RoleDefinition, EnumRoles.Admin));
        Param.RoleDefinition = RoleForNewUser(adminCount).ToString();

        return await base.Execute();
    }

    /// <summary>
    /// While a repository still has a single Admin (typically the seeded adminKNote, or none at all),
    /// whoever registers next becomes an Admin too, so the first real user of a new database can manage
    /// it. From the second real Admin on, new users start as Guests until an Admin raises their role.
    /// </summary>
    public static EnumRoles RoleForNewUser(int adminCount)
        => adminCount <= 1 ? EnumRoles.Admin : EnumRoles.Guest;
}

/// <summary>
/// Sets/resets an existing user's password - the counterpart missing for the "New user" flow
/// (KntUsersCreateAsyncCommand), which is the only place that has ever hashed a password: SaveAsync/
/// KntUsersSaveAsyncCommand's plain UserDto has no Password field, so a user created via the Admin
/// panel had no way to log in until either this command or the create flow set one explicitly.
/// </summary>
[KntAuthorize(EnumRoles.Admin)]
public class KntUsersSetPasswordAsyncCommand : KntCommandServiceBase<(Guid UserId, string NewPassword), Result<UserDto>>
{
    public KntUsersSetPasswordAsyncCommand(IKntService service, Guid userId, string newPassword)
        : base(service, (userId, newPassword))
    {

    }

    public override async Task<Result<UserDto>> Execute()
    {
        var result = new Result<UserDto>();

        if (string.IsNullOrWhiteSpace(Param.NewPassword))
            throw new Exception("Password is required");

        var resGetEntity = await Repository.Users.GetAsync(Param.UserId);
        if (!resGetEntity.IsValid)
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
            return result;
        }

        KntUsersCreateAsyncCommand.CreatePasswordHash(Param.NewPassword, out var passwordHash, out var passwordSalt);

        var resRep = await Repository.Users.UpdatePasswordAsync(Param.UserId, passwordHash, passwordSalt);
        if (resRep.IsValid)
            result.Entity = resGetEntity.Entity;
        else
            result.AddListErrorMessage(resRep.ListErrorMessage);

        return result;
    }
}

