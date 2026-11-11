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
    public Func<Guid, Task<Result<DocumentSummaryContextDto>>>? GetDocumentContextAsyncImpl { get; set; }
    public Func<Guid?, Task<Result<List<ScannedDocumentDto>>>>? GetScannedDocumentsAsyncImpl { get; set; }
    public Func<Guid, int, Task<Result<PageTranscriptionContextDto>>>? GetPageContextAsyncImpl { get; set; }
    public Func<PageTranscriptionSaveDto, Task<Result<DocumentReadDto>>>? SaveTranscriptionAsyncImpl { get; set; }
    public Func<AiDescriptionSaveDto, Task<Result<ResourceInfoDto>>>? SaveAiDescriptionAsyncImpl { get; set; }

    public Task<Result<PhotoDescriptionContextDto>> GetPhotoContextAsync(Guid resourceId) => throw new NotSupportedException();

    public Task<Result<DocumentSummaryContextDto>> GetDocumentContextAsync(Guid resourceId) =>
        (GetDocumentContextAsyncImpl ?? throw new NotSupportedException($"{nameof(GetDocumentContextAsync)} not configured for this test"))(resourceId);

    public Task<Result<List<ScannedDocumentDto>>> GetScannedDocumentsAsync(Guid? resourceId = null) =>
        (GetScannedDocumentsAsyncImpl ?? throw new NotSupportedException($"{nameof(GetScannedDocumentsAsync)} not configured for this test"))(resourceId);

    public Task<Result<PageTranscriptionContextDto>> GetPageContextAsync(Guid resourceId, int page) =>
        (GetPageContextAsyncImpl ?? throw new NotSupportedException($"{nameof(GetPageContextAsync)} not configured for this test"))(resourceId, page);

    public Task<Result<DocumentReadDto>> SaveTranscriptionAsync(PageTranscriptionSaveDto transcription) =>
        (SaveTranscriptionAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveTranscriptionAsync)} not configured for this test"))(transcription);

    public Task<Result<ResourceInfoDto>> SaveAiDescriptionAsync(AiDescriptionSaveDto description) =>
        (SaveAiDescriptionAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveAiDescriptionAsync)} not configured for this test"))(description);

    public Task<Result<ResourceProcessingStatusDto>> GetProcessingStatusAsync() =>
        (GetProcessingStatusAsyncImpl ?? throw new NotSupportedException($"{nameof(GetProcessingStatusAsync)} not configured for this test"))();

    public Task<Result<ResourceProcessingPlanDto>> PrepareProcessingAsync(ResourceProcessingOptionsDto options) => throw new NotSupportedException();

    public Task<Result> SaveProcessingRunAsync(ResourceProcessingRunDto run) => throw new NotSupportedException();

    public Task<Result<DocumentReadDto>> ReadDocumentAsync(Guid resourceId) => throw new NotSupportedException();

    public Task<Result<ResourceTextDto>> GetDocumentTextAsync(Guid resourceId, int maxLength = 0) => throw new NotSupportedException();
}
