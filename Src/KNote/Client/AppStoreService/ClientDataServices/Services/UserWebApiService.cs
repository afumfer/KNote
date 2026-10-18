using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class UserWebApiService : BaseService, IUserWebApiService
{
    public UserWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier? pagination = null, CancellationToken cancellationToken = default)
    {
        string urlApi;
        if (pagination == null)
            urlApi = @"api/users";
        else
            urlApi = $"api/users?pageNumber={pagination.PageNumber}&pageSize={pagination.PageSize}";

        return SendAsync<List<UserDto>>(ct => _httpClient.GetAsync(urlApi, ct), "Get users", cancellationToken: cancellationToken);
    }

    public Task<Result<UserDto>> DeleteAsync(Guid userId, CancellationToken cancellationToken = default) =>
        SendAsync<UserDto>(ct => _httpClient.DeleteAsync($"api/users/{userId}", ct), "Delete user", true, cancellationToken);

    public Task<Result<UserDto>> GetAsync(Guid userId, CancellationToken cancellationToken = default) =>
        SendAsync<UserDto>(ct => _httpClient.GetAsync($"api/users/{userId}", ct), "Get user", cancellationToken: cancellationToken);

    public Task<Result<UserDto>> SaveAsync(UserDto user, CancellationToken cancellationToken = default) =>
        SendAsync<UserDto>(ct => user.UserId == Guid.Empty
                ? _httpClient.PostAsJsonAsync($"api/users", user, ct)
                : _httpClient.PutAsJsonAsync($"api/users", user, ct),
            "Save user", true, cancellationToken);

    public Task<UserTokenDto?> RegisterAsync(UserRegisterDto user, CancellationToken cancellationToken = default) =>
        SendForTokenAsync(ct => _httpClient.PostAsJsonAsync($"api/users/register", user, ct), cancellationToken);

    public Task<UserTokenDto?> LoginAsync(UserCredentialsDto user, CancellationToken cancellationToken = default) =>
        SendForTokenAsync(ct => _httpClient.PostAsJsonAsync($"api/users/login", user, ct), cancellationToken);

    // Login and register answer with a UserTokenDto, not a Result: a failure without that answer (no network, the
    // server down, an answer that isn't one) is a UserTokenDto not successful with its reason, as the server's own.
    private static async Task<UserTokenDto?> SendForTokenAsync(Func<CancellationToken, Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        try
        {
            using var httpRes = await send(cancellationToken);
            return await httpRes.Content.ReadFromJsonAsync<UserTokenDto>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new UserTokenDto { success = false, error = "The request was cancelled." };
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
        {
            return new UserTokenDto { success = false, error = $"Error. The web server could not be reached: {ex.Message}" };
        }
        catch (System.Text.Json.JsonException ex)
        {
            return new UserTokenDto { success = false, error = $"Error. The web server's answer could not be read: {ex.Message}" };
        }
    }
}
