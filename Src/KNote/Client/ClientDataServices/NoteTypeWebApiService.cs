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
    public class NoteTypeWebApiService : INoteTypeWebApiService
    {
        private readonly HttpClient _httpClient;

        public NoteTypeWebApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<Result<List<NoteTypeDto>>>($"api/notetypes");
        }

        public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Result<NoteTypeDto>>($"api/notetypes/{id}");
        }

        public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto noteType)
        {
            HttpResponseMessage httpRes;

            if (noteType.NoteTypeId == Guid.Empty)
                httpRes = await _httpClient.PostAsJsonAsync<NoteTypeDto>($"api/notetypes", noteType);
            else
                httpRes = await _httpClient.PutAsJsonAsync<NoteTypeDto>($"api/notetypes", noteType);

            var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTypeDto>>();

            return res;
        }

        public async Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
        {
            var httpRes = await _httpClient.DeleteAsync($"api/notetypes/{id}");

            var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTypeDto>>();

            return res;
        }
    }
}
