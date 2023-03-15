using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Client.AppStoreService.ClientDataServices.Interfaces;

public interface IChatGPTService
{
    Task<Result<string>> PostAsync(string? prompt);
}
