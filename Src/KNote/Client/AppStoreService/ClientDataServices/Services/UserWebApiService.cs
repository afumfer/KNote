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

    public async Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier pagination = null)
    {
        string urlApi;
        if (pagination == null)
            urlApi = @"api/users";
        else
            urlApi = $"api/users?pageNumber={pagination.PageNumber}&pageSize={pagination.PageSize}";

        // option 1
        //return await httpClient.GetFromJsonAsync<Result<List<UserDto>>>(urlApi);

        // option 2
        var httpRes = await httpClient.GetAsync(urlApi);
        //var res = await httpRes.Content.ReadFromJsonAsync<Result<List<UserDto>>>();
        //return res;
        return await ProcessResultFromHttpResponse<List<UserDto>>(httpRes, "Get users");
    }

    public async Task<Result<UserDto>> DeleteAsync(Guid userId)
    {
        var httpRes = await httpClient.DeleteAsync($"api/users/{userId}");

        //var res = await httpRes.Content.ReadFromJsonAsync<Result<UserDto>>();
        //return res;
        return await ProcessResultFromHttpResponse<UserDto>(httpRes, "Delete user", true);
    }

    public async Task<Result<UserDto>> GetAsync(Guid userId)
    {
        //return await httpClient.GetFromJsonAsync<Result<UserDto>>($"api/users/{userId}");
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

        //var res = await httpRes.Content.ReadFromJsonAsync<Result<UserDto>>();
        //return res;
        return await ProcessResultFromHttpResponse<UserDto>(httpRes, "Save user", true);
    }

    public async Task<UserTokenDto> RegisterAsync(UserRegisterDto user)
    {
        var httpRes = await httpClient.PostAsJsonAsync($"api/users/register", user);

        var res = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();

        return res;
    }

    public async Task<UserTokenDto> LoginAsync(UserCredentialsDto user)
    {
        var httpRes = await httpClient.PostAsJsonAsync($"api/users/login", user);

        var res = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();

        return res;
    }
}

