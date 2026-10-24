using KNote.Client.AppStoreService;
using Microsoft.JSInterop;

namespace KNote.Client.Helpers;

public static class IJSRuntimeExtensionMethods
{
    /// <summary>
    /// Loads a script module of the app (path: "./js/..."). The browser asks the server for it the first time it is
    /// used, so with the server not reachable it can't be loaded: that is notified (AppState.NotifyError) instead of
    /// breaking the page, and null is returned; the caller goes on without what the module does, and asks for it
    /// again the next time (keep it with ??=, as it is only kept once loaded).
    /// </summary>
    public static async Task<IJSObjectReference?> ImportModuleAsync(this IJSRuntime js, string path, AppState appState)
    {
        try
        {
            return await js.InvokeAsync<IJSObjectReference>("import", path);
        }
        catch (JSException)
        {
            appState.NotifyError("Load page", $"Error. Part of the page ({path.TrimStart('.', '/')}) could not be loaded: the web server may not be reachable.");
            return null;
        }
    }

    public static async ValueTask<bool> Confirm(this IJSRuntime js, string mensaje)
    {        
        return await js.InvokeAsync<bool>("confirm", mensaje);
    }

    public static ValueTask<object> LocalStorageSetItem(this IJSRuntime js, string key, string content)
        => js.InvokeAsync<object>(
            "localStorage.setItem",
            key, content
            );

    public static ValueTask<string> LocalStorageGetItem(this IJSRuntime js, string key)
        => js.InvokeAsync<string>(
            "localStorage.getItem",
            key
            );

    public static ValueTask<object> LocalStorageRemoveItem(this IJSRuntime js, string key)
        => js.InvokeAsync<object>(
            "localStorage.removeItem",
            key);

    // Of this window (tab) only; kept when it is reloaded.
    public static ValueTask<object> SessionStorageSetItem(this IJSRuntime js, string key, string content)
        => js.InvokeAsync<object>("sessionStorage.setItem", key, content);

    public static ValueTask<string> SessionStorageGetItem(this IJSRuntime js, string key)
        => js.InvokeAsync<string>("sessionStorage.getItem", key);

    public static ValueTask<object> SessionStorageRemoveItem(this IJSRuntime js, string key)
        => js.InvokeAsync<object>("sessionStorage.removeItem", key);
}

