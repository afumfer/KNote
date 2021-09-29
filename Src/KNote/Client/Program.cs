using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using KNote.Client;
using KNote.Client.ClientDataServices;
using KNote.Client.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using KNote.Client.Auth;
using Tewr.Blazor.FileReader;
using System.Net.Http;
using KNote.Client.Shared;

namespace KNote.Client
{    
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            
            // old 2
            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            //builder.Services.AddScoped(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddOptions();
            
            builder.Services.AddScoped<IShowMessages, ShowMessages>();
            
            builder.Services.AddScoped<IGenericDataService, GenericDataService>();

            //... experimental ...                        
            builder.Services.AddScoped<IWebApiService, WebApiService>();

            builder.Services.AddSingleton<AppState>();
            //....................

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

            builder.Services.AddFileReaderService(options => options.InitializeOnFirstCall = true);

            await builder.Build().RunAsync();
        }
    }
}