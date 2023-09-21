using System;
using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class KAttributeWebApiService : BaseService, IKAttributeWebApiService
{
    public KAttributeWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync()
    {        
        var httpRes = await _httpClient.GetAsync("api/kattributes");
        return await ProcessResultFromHttpResponse<List<KAttributeInfoDto>>(httpRes, "Get attributes");
    }

    public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
    {        
        var httpRes = await _httpClient.GetAsync($"api/kattributes/getfornotetype/{typeId}");
        return await ProcessResultFromHttpResponse<List<KAttributeInfoDto>>(httpRes, "Get attributes for note type");
    }

    public async Task<Result<KAttributeDto>> GetAsync(Guid id)
    {     
        var httpRes = await _httpClient.GetAsync($"api/kattributes/{id}");
        return await ProcessResultFromHttpResponse<KAttributeDto>(httpRes, "Get attribute");
    }

    public async Task<Result<KAttributeDto>> SaveAsync(KAttributeDto kattribute)
    {
        HttpResponseMessage httpRes;

        if (kattribute.KAttributeId == Guid.Empty)
            httpRes = await _httpClient.PostAsJsonAsync("api/kattributes", kattribute);
        else
            httpRes = await _httpClient.PutAsJsonAsync("api/kattributes", kattribute);

        return await ProcessResultFromHttpResponse<KAttributeDto>(httpRes, "Save attribute", true);
    }

    public async Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id)
    {
        var httpRes = await _httpClient.DeleteAsync($"api/kattributes/{id}");
        return await ProcessResultFromHttpResponse<KAttributeInfoDto>(httpRes, "Delete attribute", true);
    }

    public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid id)
    {
        var httpRes = await _httpClient.GetAsync($"api/kattributes/{id}/gettabulatedvalues");
        return await ProcessResultFromHttpResponse<List<KAttributeTabulatedValueDto>>(httpRes, "Get attributes tabulated values");
    }
}

