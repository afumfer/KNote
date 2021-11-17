using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using KNote.Client.ClientDataServices;
using KNote.Client.Helpers;
using KNote.Client.Auth;
using KNote.Client.Shared;

namespace KNote.Client;
    
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("app");
                        
        builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        //builder.Services.AddScoped(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddOptions();
            
        builder.Services.AddScoped<IShowMessages, ShowMessages>();

        // TODO: deprecated
        builder.Services.AddScoped<IGenericDataService, GenericDataService>();  

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

        // TODO: deprecated
        //builder.Services.AddFileReaderService(options => options.InitializeOnFirstCall = true);

        await builder.Build().RunAsync();
    }
}