using KNote.Model;
using System.Net.Http.Json;

namespace KNote.Client.AppStoreService.ClientDataServices.Base;

public class BaseService
{
    protected readonly HttpClient _httpClient;

    protected readonly AppState _appState;

    /// <summary>
    /// How long the reads of the notes lists and the folders tree wait for the server (the HttpClient's own timeout
    /// is 100 s): a list that doesn't come shows its error and can be asked again, instead of loading for so long.
    /// </summary>
    public static readonly TimeSpan ListReadTimeout = TimeSpan.FromSeconds(60);

    public BaseService(AppState appState, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _appState = appState;
    }

    /// <summary>
    /// Sends a request (send) and reads its Result&lt;T&gt; (see ProcessResultFromHttpResponse). Never throws for the
    /// request itself:
    /// - no answer from the server (no network, the server down, its timeout): a Result with the error, notified as
    ///   the others;
    /// - cancelled with cancellationToken (by its caller, who no longer wants it: another one replaced it, the
    ///   component was left): a Result not valid, but not notified, as it isn't an error for the user. The caller
    ///   tells it apart with its own token (KntLatestRequest does it);
    /// - with a timeout (e.g. ListReadTimeout), no answer in that time: a Result with the error, notified.
    /// </summary>
    protected async Task<Result<T>> SendAsync<T>(Func<CancellationToken, Task<HttpResponseMessage>> send, string action,
        bool emitNotifySucess = false, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (timeout != null)
            timeoutSource.CancelAfter(timeout.Value);

        try
        {
            using var httpRes = await send(timeoutSource.Token);
            return await ProcessResultFromHttpResponse<T>(httpRes, action, emitNotifySucess, timeoutSource.Token);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CancelledResult<T>();
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            var res = new Result<T>();
            res.AddErrorMessage($"Error. The web server did not respond in {timeout!.Value.TotalSeconds:0} seconds.");
            _appState.NotifyError(action, res.ErrorMessage);
            return res;
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
        {
            // OperationCanceledException not asked for: HttpClient's timeout.
            var res = new Result<T>();
            res.AddErrorMessage($"Error. The web server could not be reached: {ex.Message}");
            _appState.NotifyError(action, res.ErrorMessage);
            return res;
        }
    }

    protected async Task<Result<T>> ProcessResultFromHttpResponse<T>(HttpResponseMessage httpRes, string action, bool emitNotifySucess = false,
        CancellationToken cancellationToken = default)
    {
        Result<T>? res;

        // The session of a signed in user is no longer valid (the token expired, or the password changed since it was
        // issued, see the Server's KntPasswordStamps): the user is signed out (AppState.OnSessionEnded), which tells them.
        if (httpRes.StatusCode == System.Net.HttpStatusCode.Unauthorized && _appState.UserName != null)
        {
            res = new Result<T>();
            res.AddErrorMessage("The session has ended. Sign in again.");
            _appState.NotifySessionEnded();
            return res;
        }

        // Server controllers deliberately return BadRequest(resApi) - not just Ok(resApi) - with a
        // real Result<T> body (IsValid=false, ErrorMessage set to the actual business-rule reason,
        // e.g. "Can't delete this note type: N note(s) still use it.") whenever a service call is
        // rejected, not just for malformed requests. Try to read that body regardless of status
        // code, so the user sees that real reason instead of a generic "server responded with
        // BadRequest" message; only fall back to the generic message when there's truly no
        // parseable body (a raw framework/network failure, or a 401/403 with no JSON at all).
        try
        {
            res = await httpRes.Content.ReadFromJsonAsync<Result<T>>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
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

    // Not valid (its data must not be used), with no notification.
    private static Result<T> CancelledResult<T>()
    {
        var res = new Result<T>();
        res.AddErrorMessage("The request was cancelled.");
        return res;
    }

}
