using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Client.AppStoreService.ClientDataServices.Interfaces;

public interface IChatGPTService
{
    Task<Result<ChatMessageOutput>> PostAsync(List<ChatMessage> context, string prompt);
}
