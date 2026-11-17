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

    public Func<Task<Result<NoisePolicyDto>>>? GetNoisePolicyAsyncImpl { get; set; }
    public Func<NoisePolicyDto, Task<Result<NoisePolicyDto>>>? SaveNoisePolicyAsyncImpl { get; set; }
    public Func<NoisePolicyDto, Task<Result<NoiseCountDto>>>? CountNoiseAsyncImpl { get; set; }

    public Task<Result<NoisePolicyDto>> GetNoisePolicyAsync() =>
        (GetNoisePolicyAsyncImpl ?? throw new NotSupportedException($"{nameof(GetNoisePolicyAsync)} not configured for this test"))();

    public Task<Result<NoisePolicyDto>> SaveNoisePolicyAsync(NoisePolicyDto policy) =>
        (SaveNoisePolicyAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveNoisePolicyAsync)} not configured for this test"))(policy);

    public Task<Result<NoiseCountDto>> CountNoiseAsync(NoisePolicyDto policy) =>
        (CountNoiseAsyncImpl ?? throw new NotSupportedException($"{nameof(CountNoiseAsync)} not configured for this test"))(policy);

    public Func<NoteSelectionRequestDto, CancellationToken, Task<Result<NoteSelectionResultDto>>>? PrepareSelectionAsyncImpl { get; set; }
    public Func<RelevanceCorrectionsSaveDto, Task<Result<List<RelevanceCorrectionDto>>>>? SaveRelevanceCorrectionsAsyncImpl { get; set; }

    public Task<Result<NoteSelectionResultDto>> PrepareSelectionAsync(NoteSelectionRequestDto request, CancellationToken cancellationToken = default) =>
        (PrepareSelectionAsyncImpl ?? throw new NotSupportedException($"{nameof(PrepareSelectionAsync)} not configured for this test"))(request, cancellationToken);

    public Task<Result<List<RelevanceCorrectionDto>>> SaveRelevanceCorrectionsAsync(RelevanceCorrectionsSaveDto changes) =>
        (SaveRelevanceCorrectionsAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveRelevanceCorrectionsAsync)} not configured for this test"))(changes);

    public Func<List<Guid>, Task<Result<List<NoteToClassifyDto>>>>? GetNotesToClassifyAsyncImpl { get; set; }
    public Func<RelevanceAiAnswersSaveDto, Task<Result<List<RelevanceAiAnswerDto>>>>? SaveRelevanceAiAnswersAsyncImpl { get; set; }

    public Task<Result<List<NoteToClassifyDto>>> GetNotesToClassifyAsync(List<Guid> noteIds) =>
        (GetNotesToClassifyAsyncImpl ?? throw new NotSupportedException($"{nameof(GetNotesToClassifyAsync)} not configured for this test"))(noteIds);

    public Task<Result<List<RelevanceAiAnswerDto>>> SaveRelevanceAiAnswersAsync(RelevanceAiAnswersSaveDto answers) =>
        (SaveRelevanceAiAnswersAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveRelevanceAiAnswersAsync)} not configured for this test"))(answers);
}
