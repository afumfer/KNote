using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KNote.Server.Local;

/// <summary>The "Local" section: the id of this installation (written the first time, see KntLocalData).</summary>
public class LocalConfig
{
    public string InstanceId { get; set; }
}

/// <summary>
/// KNote Local in the Server: everything here is added only when there is a data folder (KntLocalData.FromEnvironment),
/// so the server mode doesn't change. Program calls KntAddLocalConfiguration before the configuration is read,
/// KntAddLocal with the other services and MapKntLocal with the other endpoints.
/// </summary>
public static class KntLocal
{
    public const string ControlHeader = "X-KNote-Control";

    /// <summary>
    /// Where the application's files are (appsettings, nlog.config, wwwroot): the application's folder, except in
    /// Development (the project's folder, as dotnet run and Visual Studio set it). Started by the system or by the
    /// launcher, the working folder can be any other, and the app would come up blank.
    /// </summary>
    public static string ContentRootPath() =>
        string.Equals(KntLocalData.EnvironmentName, Environments.Development, StringComparison.OrdinalIgnoreCase)
            ? null
            : AppContext.BaseDirectory;

    /// <summary>
    /// The data folder (created with its configuration the first time) and its configuration file over the rest of the
    /// configuration, its secrets decrypted (reloaded when it changes). The secrets that couldn't be decrypted are logged.
    /// </summary>
    public static KntSecretProtector KntAddLocalConfiguration(this WebApplicationBuilder builder, KntLocalData localData, NLog.Logger logger)
    {
        var protector = new KntSecretProtector(localData.KeysFolder);
        if (localData.EnsureCreated(protector))
            logger.Info("KNote Local: data folder {path} created.", localData.Path);

        var source = new KntProtectedJsonConfigurationSource
        {
            Path = localData.ConfigFile,
            Optional = false,
            ReloadOnChange = true,
            Protector = protector
        };
        source.ResolveFileProvider();
        ((IConfigurationBuilder)builder.Configuration).Add(source);

        var provider = ((IConfigurationRoot)builder.Configuration).Providers.OfType<KntProtectedJsonConfigurationProvider>().LastOrDefault();
        foreach (var key in provider?.UnreadableKeys ?? [])
            logger.Warn("KNote Local: the secret {key} of {file} could not be decrypted (keys of another user or computer): enter it again.", key, localData.ConfigFile);

        // The Client's files of a build (not of a publication) come from its project: served in Local as in Development.
        builder.WebHost.UseStaticWebAssets();
        return protector;
    }

    public static IServiceCollection KntAddLocal(this IServiceCollection services, IConfiguration configuration, KntLocalData localData, KntSecretProtector protector)
    {
        services.AddSingleton(localData);
        services.AddSingleton(protector);
        services.Configure<LocalConfig>(configuration.GetSection("Local"));
        services.AddHealthChecks();
        services.AddSingleton<KntLocalInstance>();
        services.AddHostedService(provider => provider.GetRequiredService<KntLocalInstance>());
        return services;
    }

    /// <summary>
    /// /health (under the path base: /KNote/health) says it is this KNote (its version and its installation's id), for the
    /// launcher to tell it from another program, or another user's KNote, in the same port. POST /local/stop, with the
    /// control token of the data folder (only the user can read it) in X-KNote-Control, stops the server cleanly (the
    /// launcher's --stop, the installer and the updates).
    /// </summary>
    public static WebApplication MapKntLocal(this WebApplication app)
    {
        if (app.Services.GetService<KntLocalData>() == null)
            return app;

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                var local = context.RequestServices.GetRequiredService<IOptions<LocalConfig>>().Value;
                await context.Response.WriteAsJsonAsync(new
                {
                    app = KNote.Model.KntConst.AppName,
                    version = typeof(KntLocal).Assembly.GetName().Version?.ToString(),
                    instanceId = local.InstanceId,
                    status = report.Status.ToString()
                });
            }
        });

        app.MapPost("/local/stop", (HttpContext context, KntLocalInstance instance, IHostApplicationLifetime lifetime) =>
        {
            if (!IsLocalRequest(context) || !instance.IsControlToken(context.Request.Headers[ControlHeader]))
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            lifetime.StopApplication();
            return Results.Accepted();
        });

        return app;
    }

    // A request from this computer. Without a remote address (the in-process tests' TestServer) it is too.
    public static bool IsLocalRequest(HttpContext context) =>
        context.Connection.RemoteIpAddress is not { } address || IPAddress.IsLoopback(address);
}

/// <summary>
/// While the server runs, its PID and a control token (random, new each time) are in the data folder, where only the
/// user can read them: the launcher finds the server it started and asks it to stop with the token (KntLocal.MapKntLocal).
/// They are deleted when it stops.
/// </summary>
public class KntLocalInstance : IHostedService
{
    private readonly KntLocalData _localData;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<KntLocalInstance> _logger;
    private readonly string _controlToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    public KntLocalInstance(KntLocalData localData, IHostApplicationLifetime lifetime, ILogger<KntLocalInstance> logger)
    {
        _localData = localData;
        _lifetime = lifetime;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(() =>
        {
            File.WriteAllText(_localData.PidFile, Environment.ProcessId.ToString());
            File.WriteAllText(_localData.ControlTokenFile, _controlToken);
            _logger.LogInformation("KNote Local started (data folder {path}).", _localData.Path);
        });
        _lifetime.ApplicationStopping.Register(DeleteFiles);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public bool IsControlToken(string token) =>
        !string.IsNullOrEmpty(token)
        && CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(token), Encoding.ASCII.GetBytes(_controlToken));

    // Only if they are still this server's (another one may have started since).
    private void DeleteFiles()
    {
        try
        {
            if (File.Exists(_localData.PidFile) && File.ReadAllText(_localData.PidFile).Trim() == Environment.ProcessId.ToString())
            {
                File.Delete(_localData.PidFile);
                File.Delete(_localData.ControlTokenFile);
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "KNote Local: the PID and control token files could not be deleted.");
        }
    }
}
