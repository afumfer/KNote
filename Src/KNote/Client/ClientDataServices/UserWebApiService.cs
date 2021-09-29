using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public class UserWebApiService : IUserWebApiService
    {
        private readonly HttpClient _httpClient;

        public UserWebApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
            var httpRes = await _httpClient.DeleteAsync($"api/users/{userId.ToString()}");
            
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
            
            if(user.UserId == Guid.Empty)
                httpRes = await _httpClient.PostAsJsonAsync<UserDto>($"api/users", user);
            else             
                httpRes = await _httpClient.PutAsJsonAsync<UserDto>($"api/users", user);

            var res = await httpRes.Content.ReadFromJsonAsync<Result<UserDto>>();

            return res;
        }

        public async Task<UserTokenDto> RegisterAsync(UserRegisterDto user)
        {            
            var httpRes = await _httpClient.PostAsJsonAsync<UserRegisterDto>($"api/users/register", user);
         
            var res = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();

            return res;
        }

        public async Task<UserTokenDto> LoginAsync(UserCredentialsDto user)
        {
            var httpRes = await _httpClient.PostAsJsonAsync<UserCredentialsDto>($"api/users/login", user);

            var res = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();

            return res;
        }
    }
}
