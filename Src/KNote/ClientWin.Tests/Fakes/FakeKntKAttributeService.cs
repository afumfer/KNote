using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntKAttributeService test double: only the members exercised by the tests have a
/// working implementation (via settable delegates); everything else throws, so an unexpectedly-
/// touched member fails loudly instead of silently returning a default value.
/// </summary>
internal class FakeKntKAttributeService : IKntKAttributeService
{
    public Func<Task<Result<List<KAttributeInfoDto>>>>? GetAllAsyncImpl { get; set; }
    public Func<Guid, Task<Result<KAttributeDto>>>? GetAsyncImpl { get; set; }
    public Func<KAttributeDto, Task<Result<KAttributeDto>>>? SaveAsyncImpl { get; set; }
    public Func<Guid, Task<Result<KAttributeInfoDto>>>? DeleteAsyncImpl { get; set; }

    public Task<Result<List<KAttributeInfoDto>>> GetAllAsync() =>
        (GetAllAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAllAsync)} not configured for this test"))();

    public Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId) => throw new NotSupportedException();

    public Task<Result<KAttributeDto>> GetAsync(Guid id) =>
        (GetAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAsync)} not configured for this test"))(id);

    public Task<Result<KAttributeDto>> SaveAsync(KAttributeDto kattribute) =>
        (SaveAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveAsync)} not configured for this test"))(kattribute);

    public Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id) =>
        (DeleteAsyncImpl ?? throw new NotSupportedException($"{nameof(DeleteAsync)} not configured for this test"))(id);

    public Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid id) => throw new NotSupportedException();
}
