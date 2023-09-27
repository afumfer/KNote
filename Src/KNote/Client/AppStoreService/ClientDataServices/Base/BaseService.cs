using KNote.Model;
using System.Net.Http.Json;

namespace KNote.Client.AppStoreService.ClientDataServices.Base;

public class BaseService
{
    protected readonly HttpClient _httpClient;

    protected readonly AppState _appState;

    public BaseService(AppState appState, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _appState = appState;
    }

    protected async Task<Result<T>> ProcessResultFromHttpResponse<T>(HttpResponseMessage httpRes, string action, bool emitNotifySucess = false)
    {
        Result<T>? res;

        if (httpRes.IsSuccessStatusCode)
        {
            res = await httpRes.Content.ReadFromJsonAsync<Result<T>>();
            if (res == null)
            {
                res = new Result<T>();
                res.AddErrorMessage($"Error. The web server has responded with the following message: StatusCode - {httpRes.StatusCode}. Reason Phrase - {httpRes.ReasonPhrase}");
            }
        }
        else
        {
            res = new Result<T>();
            res.AddErrorMessage($"Error. The web server has responded with the following message: StatusCode - {httpRes.StatusCode}. Reason Phrase - {httpRes.ReasonPhrase}");
        }        
                    
        if (res.IsValid)
        {
            if (emitNotifySucess)
                _appState.NotifySuccess(action, $"The action '{action}' has been executed.");
        }
        else
            _appState.NotifyError(action, res.ErrorMessage);
        
        return res;
    }

    protected async Task<Result<T>?> GetResultFromHttpResponse<T>(HttpResponseMessage httpRes, bool throwsEx = true)
    {                
        if(throwsEx)
            httpRes.EnsureSuccessStatusCode();  

        return await httpRes.Content.ReadFromJsonAsync<Result<T>>();        
    }

}
