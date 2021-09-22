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

        public async Task<Result<List<UserDto>>> GetAll()
        {
            //var xx = await _httpClient.GetAsync($"api/users/getall");

            return await _httpClient.GetFromJsonAsync<Result<List<UserDto>>>($"api/users/getall");            
        }
    }
}
