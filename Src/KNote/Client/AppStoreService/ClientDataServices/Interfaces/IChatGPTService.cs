using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Client.AppStoreService.ClientDataServices.Interfaces;

public interface IChatGPTService
{
    Task<Result<KntChatMessageOutput>> PostAsync(List<KntChatMessage> context, string prompt);
}
