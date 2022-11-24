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
        return await _httpClient.GetFromJsonAsync<Result<List<UserDto>>>(urlApi);

        // option 2
        //var httpRes = await _httpClient.GetAsync(urlApi);
        //var res = await  httpRes.Content.ReadFromJsonAsync<Result<List<UserDto>>>();
        //return res;
    }

    public async Task<Result<UserDto>> DeleteAsync(Guid userId)
    {
        var httpRes = await _httpClient.DeleteAsync($"api/users/{userId}");

        var res = await httpRes.Content.ReadFromJsonAsync<Result<UserDto>>();

        return res;
    }

    public async Task<Result<UserDto>> GetAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<Result<UserDto>>($"api/users/{userId}");
    }

    public async Task<Result<UserDto>> SaveAsync(UserDto user)
    {
        HttpResponseMessage httpRes;

        if (user.UserId == Guid.Empty)
            httpRes = await _httpClient.PostAsJsonAsync($"api/users", user);
        else
            httpRes = await _httpClient.PutAsJsonAsync($"api/users", user);

        var res = await httpRes.Content.ReadFromJsonAsync<Result<UserDto>>();

        return res;
    }

    public async Task<UserTokenDto> RegisterAsync(UserRegisterDto user)
    {
        var httpRes = await _httpClient.PostAsJsonAsync($"api/users/register", user);

        var res = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();

        return res;
    }

    public async Task<UserTokenDto> LoginAsync(UserCredentialsDto user)
    {
        var httpRes = await _httpClient.PostAsJsonAsync($"api/users/login", user);

        var res = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();

        return res;
    }
}

