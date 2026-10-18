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

    public Task<Result<List<KAttributeInfoDto>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<KAttributeInfoDto>>(ct => _httpClient.GetAsync("api/kattributes", ct), "Get attributes", cancellationToken: cancellationToken);

    public Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId, CancellationToken cancellationToken = default) =>
        SendAsync<List<KAttributeInfoDto>>(ct => _httpClient.GetAsync($"api/kattributes/getfornotetype/{typeId}", ct),
            "Get attributes for note type", cancellationToken: cancellationToken);

    public Task<Result<KAttributeDto>> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<KAttributeDto>(ct => _httpClient.GetAsync($"api/kattributes/{id}", ct), "Get attribute", cancellationToken: cancellationToken);

    public Task<Result<KAttributeDto>> SaveAsync(KAttributeDto kattribute, CancellationToken cancellationToken = default) =>
        SendAsync<KAttributeDto>(ct => kattribute.KAttributeId == Guid.Empty
                ? _httpClient.PostAsJsonAsync("api/kattributes", kattribute, ct)
                : _httpClient.PutAsJsonAsync("api/kattributes", kattribute, ct),
            "Save attribute", true, cancellationToken);

    public Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<KAttributeInfoDto>(ct => _httpClient.DeleteAsync($"api/kattributes/{id}", ct), "Delete attribute", true, cancellationToken);

    public Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<List<KAttributeTabulatedValueDto>>(ct => _httpClient.GetAsync($"api/kattributes/{id}/gettabulatedvalues", ct),
            "Get attributes tabulated values", cancellationToken: cancellationToken);
}
