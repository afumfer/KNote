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
    // For Blazor 3.2.0
    //public class Program
    //{
    //    public static async Task Main(string[] args)
    //    {
    //        var builder = WebAssemblyHostBuilder.CreateDefault(args);
    //        builder.RootComponents.Add<App>("app");

    //        builder.Services.AddScoped<IShowMessages, ShowMessages>();
    //        builder.Services.AddScoped<IKntClientDataService, KntClientDataService>();

    //        await builder.Build().RunAsync();
    //    }
    //}

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
            BlazorWebAssemblyHost.CreateDefaultBuilder()
            .UseBlazorStartup<Startup>();

    }

}