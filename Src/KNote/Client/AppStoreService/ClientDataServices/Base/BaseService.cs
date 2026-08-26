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

        // Server controllers deliberately return BadRequest(resApi) - not just Ok(resApi) - with a
        // real Result<T> body (IsValid=false, ErrorMessage set to the actual business-rule reason,
        // e.g. "Can't delete this note type: N note(s) still use it.") whenever a service call is
        // rejected, not just for malformed requests. Try to read that body regardless of status
        // code, so the user sees that real reason instead of a generic "server responded with
        // BadRequest" message; only fall back to the generic message when there's truly no
        // parseable body (a raw framework/network failure, or a 401/403 with no JSON at all).
        try
        {
            res = await httpRes.Content.ReadFromJsonAsync<Result<T>>();
        }
        catch
        {
            res = null;
        }

        if (res == null)
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
