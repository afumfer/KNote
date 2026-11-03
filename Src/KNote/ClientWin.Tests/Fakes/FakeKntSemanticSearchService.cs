using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntSemanticSearchService test double: only the members exercised by the tests have a working
/// implementation (via settable delegates); everything else throws, so an unexpectedly-touched
/// member fails loudly instead of silently returning a default value.
/// </summary>
internal class FakeKntSemanticSearchService : IKntSemanticSearchService
{
    public Func<Task<Result<SemanticIndexStatusDto>>>? GetIndexStatusAsyncImpl { get; set; }
    public Func<SemanticIndexRunRequest, Task<Result<SemanticIndexProgressDto>>>? RunIndexAsyncImpl { get; set; }
    public Func<SemanticIndexRefreshRequest, Task<Result<SemanticIndexProgressDto>>>? RefreshPendingAsyncImpl { get; set; }
    public Func<string, Task<Result>>? DeleteModelIndexAsyncImpl { get; set; }
    public Func<SemanticSearchRequestDto, Task<Result<SemanticSearchResultDto>>>? SearchAsyncImpl { get; set; }

    public Task<Result<SemanticIndexStatusDto>> GetIndexStatusAsync() =>
        (GetIndexStatusAsyncImpl ?? throw new NotSupportedException($"{nameof(GetIndexStatusAsync)} not configured for this test"))();

    public Task<Result<SemanticIndexProgressDto>> RunIndexAsync(SemanticIndexRunRequest request) =>
        (RunIndexAsyncImpl ?? throw new NotSupportedException($"{nameof(RunIndexAsync)} not configured for this test"))(request);

    public Task<Result<SemanticIndexProgressDto>> RefreshPendingAsync(SemanticIndexRefreshRequest request) =>
        (RefreshPendingAsyncImpl ?? throw new NotSupportedException($"{nameof(RefreshPendingAsync)} not configured for this test"))(request);

    public Task<Result> DeleteModelIndexAsync(string indexKey) =>
        (DeleteModelIndexAsyncImpl ?? throw new NotSupportedException($"{nameof(DeleteModelIndexAsync)} not configured for this test"))(indexKey);

    public Task<Result<SemanticSearchResultDto>> SearchAsync(SemanticSearchRequestDto request, CancellationToken cancellationToken = default) =>
        (SearchAsyncImpl ?? throw new NotSupportedException($"{nameof(SearchAsync)} not configured for this test"))(request);
}
