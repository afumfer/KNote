namespace KNote.Client.AppStoreService.ClientDataServices;

public interface IGenericDataService
{
    Task<HttpResponseWrapper<object>> Delete(string url);
    Task<HttpResponseWrapper<T>> Get<T>(string url);
    Task<HttpResponseWrapper<object>> Post<T>(string url, T enviar);
    Task<HttpResponseWrapper<TResponse>> Post<T, TResponse>(string url, T enviar);
    Task<HttpResponseWrapper<object>> Put<T>(string url, T enviar);
}

