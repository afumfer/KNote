using Microsoft.JSInterop;

namespace KNote.Client.Helpers;

public static class IJSRuntimeExtensionMethods
{
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
}

