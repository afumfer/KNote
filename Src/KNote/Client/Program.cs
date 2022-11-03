using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;

using KNote.Client.AppStateService;
using KNote.Client.ClientDataServices;
using KNote.Client.Helpers;
using KNote.Client.Auth;

using Radzen;
using Microsoft.AspNetCore.Components.Web;

namespace KNote.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        //builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });        
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddOptions();
            
        builder.Services.AddScoped<IShowMessages, ShowMessages>();

        // TODO: deprecated ....
        builder.Services.AddScoped<IGenericDataService, GenericDataService>();  
        //......................

        builder.Services.AddScoped<IWebApiService, WebApiService>();

        builder.Services.AddSingleton<AppState>();
            
        builder.Services.AddAuthorizationCore();

        // Test ...
        //builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationProviderTest>();
            
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

        // TODO: deprecated
        //builder.Services.AddFileReaderService(options => options.InitializeOnFirstCall = true);

        await builder.Build().RunAsync();
    }
}