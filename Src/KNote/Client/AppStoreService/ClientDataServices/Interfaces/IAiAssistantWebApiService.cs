using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// cancellationToken (all of them): the caller no longer wants the answer; see BaseService.SendAsync.
public interface IAiAssistantWebApiService
{
    Task<Result<List<AiProviderInfoDto>>> GetProvidersAsync(CancellationToken cancellationToken = default);

    // The answer as it is written (see AiChatStreamEventTypes). A failure, before or during the answer, ends the
    // stream with an Error event, already notified to the user.
    IAsyncEnumerable<AiChatStreamEventDto> ChatAsync(AiChatRequestDto request, CancellationToken cancellationToken = default);

    // Runs what the user accepted in the card: each action comes back saying what happened.
    Task<Result<List<AiActionDto>>> RunActionsAsync(List<AiActionDto> actions, CancellationToken cancellationToken = default);

    Task<Result<List<AiChatSessionInfoDto>>> GetSessionsAsync(CancellationToken cancellationToken = default);

    Task<Result<AiChatSessionDto>> GetSessionAsync(Guid noteId, CancellationToken cancellationToken = default);

    Task<Result<AiChatSessionDto>> SaveSessionAsync(AiChatSessionDto session, CancellationToken cancellationToken = default);
}
