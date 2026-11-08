using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntAtlasContextService test double: the members exercised by the tests work through settable delegates;
/// everything else throws, so an unexpectedly-touched member fails loudly instead of silently returning a default value.
/// </summary>
internal class FakeKntAtlasContextService : IKntAtlasContextService
{
    public Func<Task<Result<AtlasProfileInfoDto>>>? GetProfileInfoAsyncImpl { get; set; }
    public Func<Task<Result<AtlasGlossaryInfoDto>>>? GetGlossaryInfoAsyncImpl { get; set; }
    public Func<Task<Result<List<AtlasTaskPromptStatusDto>>>>? GetTaskPromptsAsyncImpl { get; set; }
    public Func<AiComposeRequestDto, Task<Result<AiComposedPromptDto>>>? ComposeAsyncImpl { get; set; }
    public Func<Task<Result<NoteDto>>>? CreateProfileAsyncImpl { get; set; }
    public Func<List<string>, Task<Result<List<GlossaryTermDto>>>>? FindGlossaryTermsAsyncImpl { get; set; }

    public Task<Result<AtlasProfileInfoDto>> GetProfileInfoAsync() =>
        (GetProfileInfoAsyncImpl ?? throw new NotSupportedException($"{nameof(GetProfileInfoAsync)} not configured for this test"))();

    public Task<Result<AtlasGlossaryInfoDto>> GetGlossaryInfoAsync() =>
        (GetGlossaryInfoAsyncImpl ?? throw new NotSupportedException($"{nameof(GetGlossaryInfoAsync)} not configured for this test"))();

    public Task<Result<List<GlossaryTermDto>>> FindGlossaryTermsAsync(List<string> texts) =>
        (FindGlossaryTermsAsyncImpl ?? throw new NotSupportedException($"{nameof(FindGlossaryTermsAsync)} not configured for this test"))(texts);

    public Task<Result<List<AtlasTaskPromptStatusDto>>> GetTaskPromptsAsync() =>
        (GetTaskPromptsAsyncImpl ?? throw new NotSupportedException($"{nameof(GetTaskPromptsAsync)} not configured for this test"))();

    public Task<Result<AiComposedPromptDto>> ComposeAsync(AiComposeRequestDto request) =>
        (ComposeAsyncImpl ?? throw new NotSupportedException($"{nameof(ComposeAsync)} not configured for this test"))(request);

    public Task<Result<NoteDto>> CreateProfileAsync() =>
        (CreateProfileAsyncImpl ?? throw new NotSupportedException($"{nameof(CreateProfileAsync)} not configured for this test"))();

    public Task<Result<NoteDto>> FillProfileFoldersAndTypesAsync() => throw new NotSupportedException();
    public Task<Result<NoteDto>> CreateGlossaryAsync() => throw new NotSupportedException();
    public Task<Result<NoteDto>> CustomizeTaskAsync(string taskKey) => throw new NotSupportedException();
}
