using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;
using KNote.Client;
using KNote.Client.ClientDataServices;
using KNote.Client.Helpers;

namespace KNote.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped<IShowMessages, ShowMessages>();
            builder.Services.AddScoped<IKntClientDataService, KntClientDataService>();

            await builder.Build().RunAsync();
        }
    }
}