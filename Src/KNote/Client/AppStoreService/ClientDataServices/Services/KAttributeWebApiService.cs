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
        return await httpClient.GetFromJsonAsync<Result<List<KAttributeInfoDto>>>("api/kattributes");
    }

    public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
    {
        return await httpClient.GetFromJsonAsync<Result<List<KAttributeInfoDto>>>($"api/kattributes/getfornotetype/{typeId}");
    }

    public async Task<Result<KAttributeDto>> GetAsync(Guid id)
    {
        return await httpClient.GetFromJsonAsync<Result<KAttributeDto>>($"api/kattributes/{id}");
    }

    public async Task<Result<KAttributeDto>> SaveAsync(KAttributeDto kattribute)
    {
        HttpResponseMessage httpRes;

        if (kattribute.KAttributeId == Guid.Empty)
            httpRes = await httpClient.PostAsJsonAsync("api/kattributes", kattribute);
        else
            httpRes = await httpClient.PutAsJsonAsync("api/kattributes", kattribute);

        var res = await httpRes.Content.ReadFromJsonAsync<Result<KAttributeDto>>();

        return res;
    }

    public async Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id)
    {
        var httpRes = await httpClient.DeleteAsync($"api/kattributes/{id}");

        var res = await httpRes.Content.ReadFromJsonAsync<Result<KAttributeInfoDto>>();

        return res;
    }

    public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid id)
    {
        return await httpClient.GetFromJsonAsync<Result<List<KAttributeTabulatedValueDto>>>($"api/kattributes/{id}/gettabulatedvalues");
    }
}

