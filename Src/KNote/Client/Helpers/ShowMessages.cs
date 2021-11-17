using Microsoft.JSInterop;

namespace KNote.Client.Helpers;

public class ShowMessages : IShowMessages
{
    // <!-- sweetalert is required  // sweetalert2.github.io  -->
    private readonly IJSRuntime js;

    public ShowMessages(IJSRuntime js)
    {
        this.js = js;
    }

    // TODO: implement messages for info  "info" type 

    public async Task ShowErrorMessage(string message)
    {
        await ShowMessage("Error", message, "error");
    }

    public async Task ShowOkMessage(string message)
    {
        await ShowMessage("Ok", message, "success");
    }

    private async ValueTask ShowMessage(string title, string message, string messageType)
    {
        // dummy
        // await Task.FromResult(0);            

        // version 1
        // await js.InvokeAsync<bool>("alert", message);

        // version 2
        // messageType is a value of Swal.fire (error, success, info, ...=            
        await js.InvokeVoidAsync("Swal.fire", title, message, messageType);
    }

}

