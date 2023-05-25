using KNote.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using KNote.Client.AppStoreService;
using KNote.Client.Auth;
using Radzen;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("app");
builder.RootComponents.Add<HeadOutlet>("head::after");
        
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddOptions();

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
