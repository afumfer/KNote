using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntResourceContentService test double: only the members exercised by the tests have a working implementation
/// (via settable delegates); everything else throws, so an unexpectedly-touched member fails loudly.
/// </summary>
internal class FakeKntResourceContentService : IKntResourceContentService
{
    public Func<Task<Result<ResourceProcessingStatusDto>>>? GetProcessingStatusAsyncImpl { get; set; }

    public Task<Result<PhotoDescriptionContextDto>> GetPhotoContextAsync(Guid resourceId) => throw new NotSupportedException();

    public Task<Result<ResourceInfoDto>> SaveAiDescriptionAsync(AiDescriptionSaveDto description) => throw new NotSupportedException();

    public Task<Result<ResourceProcessingStatusDto>> GetProcessingStatusAsync() =>
        (GetProcessingStatusAsyncImpl ?? throw new NotSupportedException($"{nameof(GetProcessingStatusAsync)} not configured for this test"))();

    public Task<Result<ResourceProcessingPlanDto>> PrepareProcessingAsync(ResourceProcessingOptionsDto options) => throw new NotSupportedException();

    public Task<Result> SaveProcessingRunAsync(ResourceProcessingRunDto run) => throw new NotSupportedException();
}
