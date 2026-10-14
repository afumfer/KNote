using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntSystemValuesService test double: only the members exercised by the tests have a
/// working implementation (via settable delegates); everything else throws, so an unexpectedly-
/// touched member fails loudly instead of silently returning a default value.
/// </summary>
internal class FakeKntSystemValuesService : IKntSystemValuesService
{
    public Func<Task<Result<RepositoryInfoDto>>>? GetRepositoryInfoAsyncImpl { get; set; }
    public Func<string, Task<Result<RepositoryInfoDto>>>? SaveRepositoryNameAsyncImpl { get; set; }

    public Task<Result<List<SystemValueDto>>> GetAllAsync() => throw new NotSupportedException();
    public Task<Result<SystemValueDto>> GetAsync(KeyValuePair<string, string> scopeKey) => throw new NotSupportedException();
    public Task<Result<SystemValueDto>> GetAsync(Guid id) => throw new NotSupportedException();
    public Task<Result<SystemValueDto>> SaveAsync(SystemValueDto entityInfo) => throw new NotSupportedException();
    public Task<Result<SystemValueDto>> DeleteAsync(Guid id) => throw new NotSupportedException();

    public Task<Result<RepositoryInfoDto>> GetRepositoryInfoAsync() =>
        (GetRepositoryInfoAsyncImpl ?? throw new NotSupportedException($"{nameof(GetRepositoryInfoAsync)} not configured for this test"))();

    public Task<Result<RepositoryInfoDto>> SaveRepositoryNameAsync(string name) =>
        (SaveRepositoryNameAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveRepositoryNameAsync)} not configured for this test"))(name);
}
