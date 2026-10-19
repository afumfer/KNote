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
        Service.ResetCurrentUser();
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
        Service.ResetCurrentUser();

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

        if (!KntPasswordHasher.Verify(Param.Password, resRep.Entity.PasswordHash, resRep.Entity.PasswordSalt))
        {
            resService.AddErrorMessage("User not authenticated");
            resService.Entity = null;
            return resService;
        }

        resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();
        return resService;
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

        var passwordError = KntPasswordPolicy.Validate(password, Param.UserName);
        if (passwordError != null)
        {
            resService.AddErrorMessage(passwordError);
            return resService;
        }

        if ((await Repository.Users.GetInternalAsync(Param.UserName)).Entity != null)
            throw new Exception("Username \"" + Param.UserName + "\" is already taken");
        else if ((await Repository.Users.GetByEMailAsync(Param.EMail)).Entity != null)
            throw new Exception("Email \"" + Param.EMail + "\" is already in use");
        else
        {
            byte[] passwordHash, passwordSalt;
            KntPasswordHasher.CreateHash(password, out passwordHash, out passwordSalt);

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
            Service.ResetCurrentUser();
        }
        return resService;
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

        var resGetEntity = await Repository.Users.GetAsync(Param.UserId);
        if (!resGetEntity.IsValid)
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
            return result;
        }

        var passwordError = KntPasswordPolicy.Validate(Param.NewPassword, resGetEntity.Entity.UserName);
        if (passwordError != null)
        {
            result.AddErrorMessage(passwordError);
            return result;
        }

        KntPasswordHasher.CreateHash(Param.NewPassword, out var passwordHash, out var passwordSalt);

        var resRep = await Repository.Users.UpdatePasswordAsync(Param.UserId, passwordHash, passwordSalt);
        if (resRep.IsValid)
            result.Entity = resGetEntity.Entity;
        else
            result.AddListErrorMessage(resRep.ListErrorMessage);

        return result;
    }
}

/// <summary>
/// The fingerprint of the password of a user (KntPasswordHasher.Stamp): the Server's tokens carry it, and a token
/// whose fingerprint is no longer the user's (the password changed since) is refused. Only the Server uses it,
/// where the role isn't checked; Admin, so nobody else can read it.
/// </summary>
[KntAuthorize(EnumRoles.Admin)]
public class KntUsersGetPasswordStampAsyncCommand : KntCommandServiceBase<string, Result<string>>
{
    public KntUsersGetPasswordStampAsyncCommand(IKntService service, string userName) : base(service, userName)
    {

    }

    public override async Task<Result<string>> Execute()
    {
        var result = new Result<string>();

        var resUser = string.IsNullOrWhiteSpace(Param) ? null : await Repository.Users.GetInternalAsync(Param);
        if (resUser?.Entity == null)
            result.AddErrorMessage($"The user '{Param}' is not registered.");
        else
            result.Entity = KntPasswordHasher.Stamp(resUser.Entity.PasswordHash);

        return result;
    }
}

#region The user's own account

// The commands below act on the account of the user running the service (UserIdentityName), never on a
// user id given by the caller: whoever runs them can only reach their own account. Guest, since every
// user has one; a disabled user (no role) is refused by each of them too, also where the role isn't checked
// (Server, IKntService.EnforceAuthorization off).

internal static class KntCurrentUserAccount
{
    public static async Task<(UserInternalDto User, string Error)> GetAsync(IKntService service)
    {
        var userName = service.UserIdentityName;
        if (string.IsNullOrWhiteSpace(userName))
            return (null, "There is no user signed in.");

        var resUser = await service.Repository.Users.GetInternalAsync(userName);
        if (resUser.Entity == null)
            return (null, $"The user '{userName}' is not registered in the repository '{service.RepositoryRef?.Name}'.");
        if (resUser.Entity.Disabled)
            return (null, $"The user '{userName}' is disabled in the repository '{service.RepositoryRef?.Name}'.");

        return (resUser.Entity, null);
    }

    public static UserProfileDto ToProfile(UserDto user) =>
        new UserProfileDto { UserName = user.UserName, EMail = user.EMail, FullName = user.FullName };
}

[KntAuthorize(EnumRoles.Guest)]
public class KntUsersGetProfileAsyncCommand : KntCommandServiceBase<Result<UserProfileDto>>
{
    public KntUsersGetProfileAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<UserProfileDto>> Execute()
    {
        var result = new Result<UserProfileDto>();

        var (user, error) = await KntCurrentUserAccount.GetAsync(Service);
        if (error != null)
            result.AddErrorMessage(error);
        else
            result.Entity = KntCurrentUserAccount.ToProfile(user);

        return result;
    }
}

/// <summary>
/// Saves the full name and the email of the current user. The user name isn't taken from the profile. A new
/// email asks for the current password and must not be another user's.
/// </summary>
[KntAuthorize(EnumRoles.Guest)]
public class KntUsersSaveProfileAsyncCommand : KntCommandSaveServiceBase<UserProfileDto, Result<UserProfileDto>>
{
    public KntUsersSaveProfileAsyncCommand(IKntService service, UserProfileDto profile) : base(service, profile)
    {

    }

    public override async Task<Result<UserProfileDto>> Execute()
    {
        var result = new Result<UserProfileDto>();

        var (user, error) = await KntCurrentUserAccount.GetAsync(Service);
        if (error != null)
        {
            result.AddErrorMessage(error);
            return result;
        }

        var eMail = Param.EMail.Trim();
        if (!string.Equals(eMail, user.EMail, StringComparison.OrdinalIgnoreCase))
        {
            if (!KntPasswordHasher.IsCurrentPassword(user, Param.CurrentPassword))
            {
                result.AddErrorMessage("The current password is not correct. It is needed to change the email.");
                return result;
            }

            var otherUser = (await Repository.Users.GetByEMailAsync(eMail)).Entity;
            if (otherUser != null && otherUser.UserId != user.UserId)
            {
                result.AddErrorMessage($"Email \"{eMail}\" is already in use.");
                return result;
            }
        }

        // A plain UserDto, so that the update never carries the password hash.
        var userDto = user.GetSimpleDto<UserDto>();
        userDto.EMail = eMail;
        userDto.FullName = Param.FullName.Trim();

        var resRep = await Repository.Users.UpdateAsync(userDto);
        if (resRep.IsValid)
            result.Entity = KntCurrentUserAccount.ToProfile(resRep.Entity);
        else
            result.AddListErrorMessage(resRep.ListErrorMessage);

        Service.ResetCurrentUser();
        return result;
    }
}

/// <summary>
/// The current user changes their password: the current one is checked (unless they have none yet, see
/// KntPasswordHasher.IsCurrentPassword) and the new one must follow KntPasswordPolicy and be a different one.
/// </summary>
[KntAuthorize(EnumRoles.Guest)]
public class KntUsersChangePasswordAsyncCommand : KntCommandServiceBase<UserChangePasswordDto, Result<UserDto>>
{
    public KntUsersChangePasswordAsyncCommand(IKntService service, UserChangePasswordDto passwords) : base(service, passwords)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null)
            result.AddErrorMessage("The passwords are required.");
        return result;
    }

    public override async Task<Result<UserDto>> Execute()
    {
        var result = new Result<UserDto>();

        var (user, error) = await KntCurrentUserAccount.GetAsync(Service);
        if (error != null)
        {
            result.AddErrorMessage(error);
            return result;
        }

        if (!KntPasswordHasher.IsCurrentPassword(user, Param.CurrentPassword))
        {
            result.AddErrorMessage("The current password is not correct.");
            return result;
        }

        var passwordError = KntPasswordPolicy.Validate(Param.NewPassword, user.UserName);
        if (passwordError != null)
        {
            result.AddErrorMessage(passwordError);
            return result;
        }

        if (Param.NewPassword == Param.CurrentPassword)
        {
            result.AddErrorMessage("The new password must be different from the current one.");
            return result;
        }

        KntPasswordHasher.CreateHash(Param.NewPassword, out var passwordHash, out var passwordSalt);

        var resRep = await Repository.Users.UpdatePasswordAsync(user.UserId, passwordHash, passwordSalt);
        if (resRep.IsValid)
            result.Entity = user.GetSimpleDto<UserDto>();
        else
            result.AddListErrorMessage(resRep.ListErrorMessage);

        return result;
    }
}

#endregion

