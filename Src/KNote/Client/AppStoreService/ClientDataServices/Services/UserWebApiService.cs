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

    public async Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier? pagination = null)
    {
        string urlApi;
        if (pagination == null)
            urlApi = @"api/users";
        else
            urlApi = $"api/users?pageNumber={pagination.PageNumber}&pageSize={pagination.PageSize}";

        var httpRes = await httpClient.GetAsync(urlApi);
        return await ProcessResultFromHttpResponse<List<UserDto>>(httpRes, "Get users");
    }

    public async Task<Result<UserDto>> DeleteAsync(Guid userId)
    {
        var httpRes = await httpClient.DeleteAsync($"api/users/{userId}");
        return await ProcessResultFromHttpResponse<UserDto>(httpRes, "Delete user", true);
    }

    public async Task<Result<UserDto>> GetAsync(Guid userId)
    {
        var httpRes = await httpClient.GetAsync($"api/users/{userId}");
        return await ProcessResultFromHttpResponse<UserDto>(httpRes, "Get user");
    }

    public async Task<Result<UserDto>> SaveAsync(UserDto user)
    {
        HttpResponseMessage httpRes;

        if (user.UserId == Guid.Empty)
            httpRes = await httpClient.PostAsJsonAsync($"api/users", user);
        else
            httpRes = await httpClient.PutAsJsonAsync($"api/users", user);

        return await ProcessResultFromHttpResponse<UserDto>(httpRes, "Save user", true);
    }

    public async Task<UserTokenDto?> RegisterAsync(UserRegisterDto user)
    {
        var httpRes = await httpClient.PostAsJsonAsync($"api/users/register", user);
        return await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();        
    }

    public async Task<UserTokenDto?> LoginAsync(UserCredentialsDto user)
    {
        var httpRes = await httpClient.PostAsJsonAsync($"api/users/login", user);
        return await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();         
    }
}

