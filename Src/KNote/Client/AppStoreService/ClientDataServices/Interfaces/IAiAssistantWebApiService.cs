using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public interface IAiAssistantWebApiService
{
    Task<Result<List<AiProviderInfoDto>>> GetProvidersAsync();

    // The answer as it is written (see AiChatStreamEventTypes). A failure, before or during the answer, ends the
    // stream with an Error event, already notified to the user.
    IAsyncEnumerable<AiChatStreamEventDto> ChatAsync(AiChatRequestDto request, CancellationToken cancellationToken = default);

    Task<Result<List<AiChatSessionInfoDto>>> GetSessionsAsync();

    Task<Result<AiChatSessionDto>> GetSessionAsync(Guid noteId);

    Task<Result<AiChatSessionDto>> SaveSessionAsync(AiChatSessionDto session);
}
