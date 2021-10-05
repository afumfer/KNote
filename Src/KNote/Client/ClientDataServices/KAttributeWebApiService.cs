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

        public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
        {
            return await _httpClient.GetFromJsonAsync<Result<List<KAttributeInfoDto>>>($"api/kattributes/getfornotetype/{typeId}");
        }

        public async Task<Result<KAttributeDto>> GetAsync(Guid id)
        {            
            return await _httpClient.GetFromJsonAsync<Result<KAttributeDto>>($"api/kattributes/{id}");
        }

        public async Task<Result<KAttributeDto>> SaveAsync(KAttributeDto kattribute)
        {
            HttpResponseMessage httpRes;

            if (kattribute.KAttributeId == Guid.Empty)
                httpRes = await _httpClient.PostAsJsonAsync<KAttributeDto>("api/kattributes", kattribute);
            else
                httpRes = await _httpClient.PutAsJsonAsync<KAttributeDto>("api/kattributes", kattribute);

            var res = await httpRes.Content.ReadFromJsonAsync<Result<KAttributeDto>>();

            return res;
        }

        public async Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id)
        {
            var httpRes = await _httpClient.DeleteAsync($"api/kattributes/{id}");

            var res = await httpRes.Content.ReadFromJsonAsync<Result<KAttributeInfoDto>>();

            return res;
        }

        public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid id)
        {            
            return await _httpClient.GetFromJsonAsync<Result<List<KAttributeTabulatedValueDto>>>($"api/kattributes/getattributetabulatedvalues/{id}");
        }

        public async Task<Result<KAttributeTabulatedValueDto>> DeleteKAttributeTabulatedValueAsync(Guid id)
        {
            var httpRes = await _httpClient.DeleteAsync($"api/kattributes/deletetabulatedvalue/{id}");

            var res = await httpRes.Content.ReadFromJsonAsync<Result<KAttributeTabulatedValueDto>>();

            return res;
        }
    }
}
