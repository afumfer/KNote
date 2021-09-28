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

            return await _httpClient.GetFromJsonAsync<Result<List<UserDto>>>(urlApi);
        }

        public async Task<Result<UserDto>> DeleteAsync(Guid userId)
        {                        
            var httpRes = await _httpClient.DeleteAsync($"api/users/{userId.ToString()}");
            
            var res = await httpRes.Content.ReadFromJsonAsync<Result<UserDto>>();

            return res;
        }


    }
}
