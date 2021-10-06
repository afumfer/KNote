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
    public class FolderWebApiService : IFolderWebApiService
    {
        private readonly HttpClient _httpClient;

        public FolderWebApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<Result<List<FolderDto>>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<Result<List<FolderDto>>>("api/folders");
        }

        public async Task<Result<List<FolderDto>>> GetTreeAsync()
        {
            return await _httpClient.GetFromJsonAsync<Result<List<FolderDto>>>("api/folders/gettree");            
        }

        public Task<Result<FolderDto>> GetAsync(Guid folderId)
        {
            throw new NotImplementedException();
        }


        public Task<Result<FolderDto>> SaveAsync(FolderDto entityInfo)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<FolderDto>> DeleteAsync(Guid id)
        {
            var httpRes = await _httpClient.DeleteAsync($"api/folders/{id}");

            var res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();

            return res;
        }
    }
}
