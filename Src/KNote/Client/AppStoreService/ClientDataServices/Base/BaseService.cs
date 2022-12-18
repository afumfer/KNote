using KNote.Model;
using System.Net.Http.Json;

namespace KNote.Client.AppStoreService.ClientDataServices.Base;

public class BaseService
{
    protected readonly HttpClient httpClient;

    protected readonly AppState appState;

    public BaseService(AppState appState, HttpClient httpClient)
    {
        this.httpClient = httpClient;
        this.appState = appState;
    }

    protected async Task<Result<T>> ProcessResultFromHttpResponse<T>(HttpResponseMessage httpRes, string action, bool emitNotifySucess = false)
    {
        Result<T> res;

        if (httpRes.IsSuccessStatusCode)
            res = await httpRes.Content.ReadFromJsonAsync<Result<T>>();
        else 
        {
            res = new Result<T>();
            res.AddErrorMessage($"Error. The web server has responded with the following message: StatusCode - {httpRes.StatusCode}. Reason Phrase - {httpRes.ReasonPhrase}");
        }        
                    
        if (res.IsValid)
        {
            if (emitNotifySucess)
                appState.NotifySuccess(action, $"The action '{action}' has been executed.");
        }
        else
            appState.NotifyError(action, res.ErrorMessage);
        
        return res;
    }
}
