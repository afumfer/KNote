using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntAiSessionService test double: only the members exercised by the tests have a working
/// implementation (via settable delegates); everything else throws, so an unexpectedly-touched
/// member fails loudly instead of silently returning a default value.
/// </summary>
internal class FakeKntAiSessionService : IKntAiSessionService
{
    public Func<Task<Result<List<AiChatSessionInfoDto>>>>? GetUserSessionsAsyncImpl { get; set; }
    public Func<Guid, Task<Result<AiChatSessionDto>>>? GetAsyncImpl { get; set; }
    public Func<AiChatSessionDto, Task<Result<AiChatSessionDto>>>? SaveAsyncImpl { get; set; }
    public Func<AiChatAttachmentUploadDto, Task<Result<AiChatAttachmentDto>>>? AddAttachmentAsyncImpl { get; set; }
    public Func<Guid, Guid, Task<Result<AiChatAttachmentContentDto>>>? GetAttachmentAsyncImpl { get; set; }

    public Task<Result<List<AiChatSessionInfoDto>>> GetUserSessionsAsync() =>
        (GetUserSessionsAsyncImpl ?? throw new NotSupportedException($"{nameof(GetUserSessionsAsync)} not configured for this test"))();

    public Task<Result<AiChatSessionDto>> GetAsync(Guid noteId) =>
        (GetAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAsync)} not configured for this test"))(noteId);

    public Task<Result<AiChatAttachmentDto>> AddAttachmentAsync(AiChatAttachmentUploadDto attachment) =>
        (AddAttachmentAsyncImpl ?? throw new NotSupportedException($"{nameof(AddAttachmentAsync)} not configured for this test"))(attachment);

    public Task<Result<AiChatAttachmentContentDto>> GetAttachmentAsync(Guid sessionNoteId, Guid resourceId) =>
        (GetAttachmentAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAttachmentAsync)} not configured for this test"))(sessionNoteId, resourceId);

    public Task<Result<AiChatSessionDto>> SaveAsync(AiChatSessionDto session) =>
        (SaveAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveAsync)} not configured for this test"))(session);
}
