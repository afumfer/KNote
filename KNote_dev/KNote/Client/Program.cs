using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
//using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using KNote.Client;
using KNote.Client.ClientDataServices;
using KNote.Client.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using KNote.Client.Auth;
using Blazor.FileReader;
using System.Net.Http;

namespace KNote.Client
{    
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            // builder.Services.AddBaseAddressHttpClient();  // old 1
            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });  // old 2
            //builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });    // rc 1 OJO ESTO NO FUNCIONA

            builder.Services.AddOptions();
            builder.Services.AddScoped<IShowMessages, ShowMessages>();
            builder.Services.AddScoped<IKntClientDataService, KntClientDataService>();
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