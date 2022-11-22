using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

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
            httpRes = await _httpClient.PostAsJsonAsync("api/kattributes", kattribute);
        else
            httpRes = await _httpClient.PutAsJsonAsync("api/kattributes", kattribute);

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
        return await _httpClient.GetFromJsonAsync<Result<List<KAttributeTabulatedValueDto>>>($"api/kattributes/{id}/gettabulatedvalues");
    }
}

