using System.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using NLog;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Server.Hubs;
using KNote.MessageBroker.RabbitMQ;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Trace("Init KNote WebServer main.");

try
{
    ////////////////////////////////////////////////////////////////
    // Create HostBuilder

    var builder = WebApplication.CreateBuilder(args);


    ////////////////////////////////////////////////////////////////
    // Configure the application and add services to the container.

    var appSettingsSection = builder.Configuration.GetSection("AppSettings");
    var repositoryRefSection = builder.Configuration.GetSection("RepositoryRef");

    builder.Services.Configure<AppSettings>(appSettingsSection);
    builder.Services.Configure<RepositoryRef>(repositoryRefSection);

    var appSettings = appSettingsSection.Get<AppSettings>();
    var repositoryRef = repositoryRefSection.Get<RepositoryRef>();
    // LEGACY(repository-alias): remove once every Server configuration uses "Name".
    // Before the name was kept in the database, this setting was called "Alias" (KntSchemaUpdater gives it
    // to the database when it has no name yet).
    if (string.IsNullOrWhiteSpace(repositoryRef.Name))
        repositoryRef.Name = repositoryRefSection["Alias"];

    builder.Services.KntAddServices(appSettings, repositoryRef);
    builder.Services.KntAddAiAssistant(builder.Configuration);
    builder.Services.KntAddPasswordReset(builder.Configuration);
    builder.Services.KntAddRateLimits(builder.Configuration);

    builder.Services.AddAuthentication().AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.Secret)),
            ClockSkew = TimeSpan.Zero
        };
        // The chat room hub's connections can't send the Authorization header (WebSockets, server-sent events):
        // SignalR clients send the token in the access_token query string instead.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments(ChatRoomHubContract.Path))
                    context.Token = accessToken;
                return Task.CompletedTask;
            },
            // A token issued before the user's password changed is no longer valid (see KntPasswordStamps).
            OnTokenValidated = async context =>
            {
                var services = context.HttpContext.RequestServices;
                var stamps = services.GetRequiredService<KntPasswordStamps>();
                if (!await stamps.IsValidAsync(context.Principal, services.GetRequiredService<IKntService>()))
                    context.Fail("The session is closed: the password of the user has changed.");
            }
        };
    });
    builder.Services.AddAuthorization();
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<KntPasswordStamps>();

    builder.Services.AddCors(p => p.AddPolicy("KntPolicy", builder =>
    {
        builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
    }));

    builder.Services.AddScoped<IFileStore, LocalFileStore>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllersWithViews()
        .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    builder.Services.AddRazorPages();
    builder.Services.AddSignalR();
    builder.Services.AddSingleton<ChatRoomManager>();
    builder.Services.AddResponseCompression(opts =>
    {
        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/octet-stream" });
    });

    builder.Services.AddSingleton<KntMessageBroker>();

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();


    /////////////////////////////////////////////////////////////////
    /// Build App and configure the HTTP request pipeline.

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UsePathBase("/KNote");

    app.UseHttpsRedirection();
    app.UseBlazorFrameworkFiles();

    // index.html and the Client's css/js keep their URLs from one version to the next, so browsers must
    // revalidate them (ETag, 304 when unchanged) or they keep using the old ones until a hard refresh.
    var clientStaticFileOptions = new StaticFileOptions
    {
        OnPrepareResponse = context =>
        {
            if (!context.Context.Request.Path.StartsWithSegments("/_framework"))
                context.Context.Response.Headers.CacheControl = "no-cache";
        }
    };

    app.UseStaticFiles(clientStaticFileOptions);
    app.KntAddResourcesStaticFiles(appSettings, repositoryRef);

    app.UseRouting();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCors("KntPolicy");

    app.MapRazorPages();
    app.MapControllers();
    app.MapHub<ChatHub>(ChatRoomHubContract.Path);
    app.MapFallbackToFile("index.html", clientStaticFileOptions);

    await app.KntConfigureMessageBroker(appSettings, repositoryRef);

    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "KNote Server has stopped because there was an exception.");
    throw;
}
finally
{
    LogManager.Shutdown();
}

// Exposes the top-level statements' generated Program class as public so
// Microsoft.AspNetCore.Mvc.Testing's WebApplicationFactory<Program> can reference it from Tests.
public partial class Program { }

