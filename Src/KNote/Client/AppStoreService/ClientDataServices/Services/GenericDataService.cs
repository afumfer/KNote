using System.Text;
using System.Text.Json;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class GenericDataService : IGenericDataService
{
    private readonly HttpClient httpClient;

    public GenericDataService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<HttpResponseWrapper<T>> Get<T>(string url)
    {
        var responseHTTP = await httpClient.GetAsync(url);

        if (responseHTTP.IsSuccessStatusCode)
        {
            var response = await DeserializeResponse<T>(responseHTTP, DefaultJSONOptions);
            return new HttpResponseWrapper<T>(response, false, responseHTTP);
        }
        else
            return new HttpResponseWrapper<T>(default!, true, responseHTTP);
    }

    public async Task<HttpResponseWrapper<object>> Post<T>(string url, T enviar)
    {
        var sendJSON = JsonSerializer.Serialize(enviar);
        var sendContent = new StringContent(sendJSON, Encoding.UTF8, "application/json");
        var responseHttp = await httpClient.PostAsync(url, sendContent);
        return new HttpResponseWrapper<object>(null!, !responseHttp.IsSuccessStatusCode, responseHttp);
    }

    public async Task<HttpResponseWrapper<TResponse>> Post<T, TResponse>(string url, T enviar)
    {
        var enviarJSON = JsonSerializer.Serialize(enviar);
        var enviarContent = new StringContent(enviarJSON, Encoding.UTF8, "application/json");
        var responseHttp = await httpClient.PostAsync(url, enviarContent);
        if (responseHttp.IsSuccessStatusCode)
        {
            var response = await DeserializeResponse<TResponse>(responseHttp, DefaultJSONOptions);
            return new HttpResponseWrapper<TResponse>(response, false, responseHttp);
        }
        else
            return new HttpResponseWrapper<TResponse>(default!, true, responseHttp);
    }

    public async Task<HttpResponseWrapper<object>> Put<T>(string url, T enviar)
    {
        var sendJSON = JsonSerializer.Serialize(enviar);
        var sendContent = new StringContent(sendJSON, Encoding.UTF8, "application/json");
        var responseHttp = await httpClient.PutAsync(url, sendContent);
        return new HttpResponseWrapper<object>(null!, !responseHttp.IsSuccessStatusCode, responseHttp);
    }

    public async Task<HttpResponseWrapper<object>> Delete(string url)
    {
        var responseHTTP = await httpClient.DeleteAsync(url);
        return new HttpResponseWrapper<object>(null!, !responseHTTP.IsSuccessStatusCode, responseHTTP);
    }

    #region Utils

    private async Task<T> DeserializeResponse<T>(HttpResponseMessage httpResponse, JsonSerializerOptions jsonSerializerOptions)
    {
        var responseString = await httpResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseString, jsonSerializerOptions);
    }

    private JsonSerializerOptions DefaultJSONOptions =>
        new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

    #endregion 

}

