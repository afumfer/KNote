using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// The computer's settings of KNote Local (api/local, Admin): repositories, AI providers, e-mail and port, and stopping
// the local server. A repository is named by its position in the configuration (LocalRepositoryDto.Index).
public interface ILocalWebApiService
{
    Task<Result<List<LocalRepositoryDto>>> GetRepositoriesAsync(CancellationToken cancellationToken = default);
    Task<Result<List<LocalRepositoryDto>>> AddRepositoryAsync(LocalRepositoryAddDto repository);
    Task<Result<List<LocalRepositoryDto>>> SetDefaultRepositoryAsync(int index);
    Task<Result<List<LocalRepositoryDto>>> RenameRepositoryAsync(int index, string name);
    Task<Result<List<LocalRepositoryDto>>> UnlinkRepositoryAsync(int index);

    Task<Result<List<LocalAiProviderDto>>> GetAiProvidersAsync(CancellationToken cancellationToken = default);
    // The whole list, in its order (the first one is the default).
    Task<Result<List<LocalAiProviderDto>>> SaveAiProvidersAsync(List<LocalAiProviderDto> providers);

    Task<Result<LocalEmailSettingsDto>> GetEmailAsync(CancellationToken cancellationToken = default);
    Task<Result<LocalEmailSettingsDto>> SaveEmailAsync(LocalEmailSettingsDto email);

    Task<Result<LocalServerSettingsDto>> GetServerAsync(CancellationToken cancellationToken = default);
    Task<Result<LocalServerSettingsDto>> SavePortAsync(int port);
    Task<Result<bool>> StopAsync();
}
