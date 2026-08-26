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
    public Func<PageIdentifier, Task<Result<List<UserDto>>>>? GetAllAsyncImpl { get; set; }
    public Func<Guid, Task<Result<UserDto>>>? GetAsyncImpl { get; set; }
    public Func<UserDto, Task<Result<UserDto>>>? SaveAsyncImpl { get; set; }
    public Func<Guid, Task<Result<UserDto>>>? DeleteAsyncImpl { get; set; }
    public Func<Guid, string, Task<Result<UserDto>>>? SetPasswordAsyncImpl { get; set; }

    public Task<Result<UserDto>> GetByUserNameAsync(string userName) =>
        (GetByUserNameAsyncImpl ?? throw new NotSupportedException($"{nameof(GetByUserNameAsync)} not configured for this test"))(userName);

    public Task<Result<UserDto>> CreateAsync(UserRegisterDto userRegisterInfoDto) =>
        (CreateAsyncImpl ?? throw new NotSupportedException($"{nameof(CreateAsync)} not configured for this test"))(userRegisterInfoDto);

    public Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier pagination = null) =>
        (GetAllAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAllAsync)} not configured for this test"))(pagination);

    public Task<Result<UserDto>> GetAsync(Guid userId) =>
        (GetAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAsync)} not configured for this test"))(userId);

    public Task<Result<UserDto>> SaveAsync(UserDto user) =>
        (SaveAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveAsync)} not configured for this test"))(user);

    public Task<Result<UserDto>> DeleteAsync(Guid userId) =>
        (DeleteAsyncImpl ?? throw new NotSupportedException($"{nameof(DeleteAsync)} not configured for this test"))(userId);

    public Task<Result<UserDto>> SetPasswordAsync(Guid userId, string newPassword) =>
        (SetPasswordAsyncImpl ?? throw new NotSupportedException($"{nameof(SetPasswordAsync)} not configured for this test"))(userId, newPassword);

    public Task<Result<UserDto>> AuthenticateAsync(UserCredentialsDto userCredentials) => throw new NotSupportedException();
}
