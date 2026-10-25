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
using KNote.Server.Local;
using KNote.MessageBroker.RabbitMQ;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

// KNote Local (a data folder: Local__DataPath, or the Local environment): its logs go to the data folder.
var localData = KntLocalData.FromEnvironment();
if (localData != null)
    GlobalDiagnosticsContext.Set("logDir", localData.LogsFolder);

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Trace("Init KNote WebServer main.");

try
{
    ////////////////////////////////////////////////////////////////
    // Create HostBuilder

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, ContentRootPath = KntLocal.ContentRootPath() });

    // KNote Local: the user's configuration of the data folder over the rest (see Server/CLAUDE.md, "KNote Local").
    var localProtector = localData != null ? builder.KntAddLocalConfiguration(localData, logger) : null;

    ////////////////////////////////////////////////////////////////
    // Configure the application and add services to the container.

    var appSettingsSection = builder.Configuration.GetSection("AppSettings");
    builder.Services.Configure<AppSettings>(appSettingsSection);
    var appSettings = appSettingsSection.Get<AppSettings>();

    // The repositories ("Repositories", or the single "RepositoryRef"), opened now: their databases are created or
    // updated, and each one's RepositoryId is read.
    using var startupLoggerFactory = new NLog.Extensions.Logging.NLogLoggerFactory();
    var repositories = await KntRepositories.OpenAsync(KntRepositories.ReadConfiguration(builder.Configuration),
        builder.Environment.WebRootPath, startupLoggerFactory.CreateLogger<KntRepositories>());

    builder.Services.KntAddServices(repositories);
    builder.Services.KntAddAiAssistant(builder.Configuration);
    builder.Services.KntAddPasswordReset(builder.Configuration);
    builder.Services.KntAddAlarms(builder.Configuration);
    builder.Services.KntAddRateLimits(builder.Configuration);
    builder.Services.KntAddForwardedHeaders(builder.Configuration);
    builder.Services.KntAddCors();
    if (localData != null)
        builder.Services.KntAddLocal(builder.Configuration, localData, localProtector);

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
            // A session is in one of this Server's repositories (KntRepositories), and a token issued before the user's
            // password changed in it is no longer valid (see KntPasswordStamps). Checked with a service of the token's
            // repository: the request's IKntService can't be created yet (HttpContext.User isn't set until this ends).
            OnTokenValidated = async context =>
            {
                var services = context.HttpContext.RequestServices;
                var repository = repositories.FindForUser(context.Principal);
                if (repository == null)
                {
                    context.Fail("The session is not in a repository of this server.");
                    return;
                }
                var stamps = services.GetRequiredService<KntPasswordStamps>();
                if (!await stamps.IsValidAsync(context.Principal, repository, repositories))
                    context.Fail("The session is closed: the password of the user has changed.");
            }
        };
    });
    builder.Services.AddAuthorization();
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<KntPasswordStamps>();

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

    app.UseKntForwardedHeaders();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        if (appSettings.UseHttpsRedirection)
            app.UseHsts();
    }

    app.UsePathBase("/KNote");

    if (appSettings.UseHttpsRedirection)
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
    app.KntAddResourcesStaticFiles(appSettings, repositories);

    app.UseRouting();
    app.UseRateLimiter();
    app.UseKntCors(app.Configuration);
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();
    app.MapHub<ChatHub>(ChatRoomHubContract.Path);
    app.MapKntLocal();
    app.MapFallbackToFile("index.html", clientStaticFileOptions);

    await app.KntConfigureMessageBroker(appSettings, repositories.Default.RepositoryRef);

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

