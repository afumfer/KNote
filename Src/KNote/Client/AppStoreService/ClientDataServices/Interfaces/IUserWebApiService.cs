using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Client.AppStoreService.ClientDataServices;

// cancellationToken (all of them): the caller no longer wants the answer; see BaseService.SendAsync.
public interface IUserWebApiService
{
    Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier? pagination = null, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> SaveAsync(UserDto user, CancellationToken cancellationToken = default);
    Task<UserTokenDto?> RegisterAsync(UserRegisterDto user, CancellationToken cancellationToken = default);
    Task<UserTokenDto?> LoginAsync(UserCredentialsDto user, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> SetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default);

    // The signed in user's own account. Changing the password closes the user's sessions: the answer is the new
    // token of this one.
    Task<Result<UserProfileDto>> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<Result<UserProfileDto>> SaveProfileAsync(UserProfileDto profile, CancellationToken cancellationToken = default);
    Task<Result<UserTokenDto>> ChangePasswordAsync(UserChangePasswordDto passwords, CancellationToken cancellationToken = default);
}
