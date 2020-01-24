using KNote.Client.ClientDataServices;
using KNote.Client.Helpers;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IKntClientDataService, KntClientDataService>();
            services.AddScoped<IShowMessages, ShowMessages>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
