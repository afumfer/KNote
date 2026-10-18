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
}
