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

        public async Task<Result<FolderDto>> GetAsync(Guid folderId)
        {                        
            return await _httpClient.GetFromJsonAsync<Result<FolderDto>>($"api/folders/{folderId}");
        }

        public async Task<Result<FolderDto>> SaveAsync(FolderDto folder)
        {
            HttpResponseMessage httpRes;

            if (folder.FolderId == Guid.Empty)
                httpRes = await _httpClient.PostAsJsonAsync<FolderDto>($"api/folders", folder);
            else
                httpRes = await _httpClient.PutAsJsonAsync<FolderDto>($"api/folders", folder);

            var res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();

            return res;
        }

        public async Task<Result<FolderDto>> DeleteAsync(Guid id)
        {
            var httpRes = await _httpClient.DeleteAsync($"api/folders/{id}");

            var res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();

            return res;
        }

        public async Task<Result<List<NoteInfoDto>>> GetNotes(Guid folderId)
        {
            return await _httpClient.GetFromJsonAsync<Result<List<NoteInfoDto>>>($"api/folders/{folderId}/getnotes");
        }
    }
}
