using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

internal class FakeKntSummaryService : IKntSummaryService
{
    public Func<Guid, Task<Result<DocumentToSummarizeDto>>>? PrepareDocumentAsyncImpl { get; set; }
    public Func<Task<Result<SummaryStateDto>>>? GetStateAsyncImpl { get; set; }
    public Func<DocumentSummarySaveDto, Task<Result<NoteDto>>>? SaveDocumentSummaryAsyncImpl { get; set; }

    public Task<Result<DocumentToSummarizeDto>> PrepareDocumentAsync(Guid resourceId) =>
        (PrepareDocumentAsyncImpl ?? throw new NotSupportedException($"{nameof(PrepareDocumentAsync)} not configured for this test"))(resourceId);

    public Task<Result<SummaryStateDto>> GetStateAsync() =>
        (GetStateAsyncImpl ?? throw new NotSupportedException($"{nameof(GetStateAsync)} not configured for this test"))();

    public Task<Result<NoteDto>> SaveDocumentSummaryAsync(DocumentSummarySaveDto summary) =>
        (SaveDocumentSummaryAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveDocumentSummaryAsync)} not configured for this test"))(summary);

    public Func<DocumentTranslationSaveDto, Task<Result<NoteDto>>>? SaveDocumentTranslationAsyncImpl { get; set; }
    public Func<Guid, Task<Result<List<DocumentTranslationDto>>>>? GetDocumentTranslationsAsyncImpl { get; set; }

    public Task<Result<NoteDto>> SaveDocumentTranslationAsync(DocumentTranslationSaveDto translation) =>
        (SaveDocumentTranslationAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveDocumentTranslationAsync)} not configured for this test"))(translation);

    public Task<Result<List<DocumentTranslationDto>>> GetDocumentTranslationsAsync(Guid resourceId) =>
        (GetDocumentTranslationsAsyncImpl ?? throw new NotSupportedException($"{nameof(GetDocumentTranslationsAsync)} not configured for this test"))(resourceId);
}
