using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

public interface IKntUserService
{
    Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier pagination = null);
    Task<Result<UserDto>> GetAsync(Guid userId);
    Task<Result<UserDto>> GetByUserNameAsync(string userName);
    Task<Result<UserDto>> SaveAsync(UserDto user);
    Task<Result<UserDto>> DeleteAsync(Guid userId);
    Task<Result<UserDto>> SetPasswordAsync(Guid userId, string newPassword);
    Task<Result<UserDto>> AuthenticateAsync(UserCredentialsDto userCredentials);
    Task<Result<UserDto>> CreateAsync(UserRegisterDto userRegisterInfoDto);
    Task<Result<UserDto>> RegisterAsync(UserRegisterDto userRegisterInfoDto);
    Task<Result<string>> GetPasswordStampAsync(string userName);

    // "Forgot your password?": a token for a link to set a new one (to be sent to the user's email, never in an
    // answer), signed with the caller's key, and the new password set with it.
    Task<Result<UserPasswordResetTokenDto>> CreatePasswordResetAsync(string userNameOrEMail, string signingKey, TimeSpan lifetime);
    Task<Result<UserDto>> ResetPasswordAsync(UserResetPasswordDto reset, string signingKey);

    // The account of the current user (UserIdentityName), for the user themselves.
    Task<Result<UserProfileDto>> GetProfileAsync();
    Task<Result<UserProfileDto>> SaveProfileAsync(UserProfileDto profile);
    Task<Result<UserDto>> ChangePasswordAsync(UserChangePasswordDto passwords);
}
