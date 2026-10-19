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

    // The account of the current user (UserIdentityName), for the user themselves.
    Task<Result<UserProfileDto>> GetProfileAsync();
    Task<Result<UserProfileDto>> SaveProfileAsync(UserProfileDto profile);
    Task<Result<UserDto>> ChangePasswordAsync(UserChangePasswordDto passwords);
}
