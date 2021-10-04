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
    public class KAttributeWebApiService : IKAttributeWebApiService
    {
        private readonly HttpClient _httpClient;

        public KAttributeWebApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<Result<List<KAttributeInfoDto>>>("api/kattributes");
        }

        public Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeDto>> GetAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeDto>> SaveAsync(KAttributeDto kattribute)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id)
        {
            var httpRes = await _httpClient.DeleteAsync($"api/kattributes/{id}");

            var res = await httpRes.Content.ReadFromJsonAsync<Result<KAttributeInfoDto>>();

            return res;
        }

        public Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeTabulatedValueDto>> DeleteKAttributeTabulatedValueAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
