using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;

using KNote.Client.AppStoreService;
using KNote.Client.Helpers;
using KNote.Client.Auth;

using Radzen;
using Microsoft.AspNetCore.Components.Web;
using KNote.Client.AppStoreService.ClientDataServices;

namespace KNote.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddOptions();

        // TODO: will be deprecated ....
        builder.Services.AddScoped<IShowMessages, ShowMessages>();
        builder.Services.AddScoped<IGenericDataService, GenericDataService>();  
        //......................

        builder.Services.AddScoped<IStore, Store>();

        builder.Services.AddAuthorizationCore();            
        builder.Services.AddScoped<AuthenticationProviderJWT>();
        builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationProviderJWT>(
            provider => provider.GetRequiredService<AuthenticationProviderJWT>()
        );
        builder.Services.AddScoped<ILoginService, AuthenticationProviderJWT>(
            provider => provider.GetRequiredService<AuthenticationProviderJWT>()
        );

        builder.Services.AddScoped<DialogService>();
        builder.Services.AddScoped<NotificationService>();
        builder.Services.AddScoped<TooltipService>();
        builder.Services.AddScoped<ContextMenuService>();

        await builder.Build().RunAsync();
    }
}