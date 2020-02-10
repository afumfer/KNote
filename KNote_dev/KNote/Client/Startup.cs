using KNote.Client.ClientDataServices;
using KNote.Client.Helpers;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using KNote.Client.Auth;

namespace KNote.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IKntClientDataService, KntClientDataService>();
            services.AddScoped<IShowMessages, ShowMessages>();
            services.AddAuthorizationCore();

            services.AddScoped<AuthenticationStateProvider, AuthenticationProviderTest>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
