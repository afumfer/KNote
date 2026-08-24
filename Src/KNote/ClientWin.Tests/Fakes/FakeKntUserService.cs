using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntUserService test double: only the members exercised by the tests have a working
/// implementation (via settable delegates); everything else throws, so an unexpectedly-touched
/// member fails loudly instead of silently returning a default value.
/// </summary>
internal class FakeKntUserService : IKntUserService
{
    public Func<string, Task<Result<UserDto>>>? GetByUserNameAsyncImpl { get; set; }
    public Func<UserRegisterDto, Task<Result<UserDto>>>? CreateAsyncImpl { get; set; }

    public Task<Result<UserDto>> GetByUserNameAsync(string userName) =>
        (GetByUserNameAsyncImpl ?? throw new NotSupportedException($"{nameof(GetByUserNameAsync)} not configured for this test"))(userName);

    public Task<Result<UserDto>> CreateAsync(UserRegisterDto userRegisterInfoDto) =>
        (CreateAsyncImpl ?? throw new NotSupportedException($"{nameof(CreateAsync)} not configured for this test"))(userRegisterInfoDto);

    public Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier pagination = null) => throw new NotSupportedException();
    public Task<Result<UserDto>> GetAsync(Guid userId) => throw new NotSupportedException();
    public Task<Result<UserDto>> SaveAsync(UserDto user) => throw new NotSupportedException();
    public Task<Result<UserDto>> DeleteAsync(Guid userId) => throw new NotSupportedException();
    public Task<Result<UserDto>> AuthenticateAsync(UserCredentialsDto userCredentials) => throw new NotSupportedException();
}
