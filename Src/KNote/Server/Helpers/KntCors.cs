using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Server.Helpers;

/// <summary>
/// The "Cors" configuration section: the web origins (other than this Server's own) whose pages may call the API from
/// a browser. Empty by default: the Client is served by this Server (same origin), and ClientWin and the tests are not
/// browsers (CORS doesn't apply to them). "*" allows any origin.
/// </summary>
public class CorsConfig
{
    public string[] AllowedOrigins { get; set; } = [];
}

public static class KntCors
{
    public const string Policy = "KntPolicy";

    // The policy is read from the configuration when used, like UseKntCors (the in-process tests can override it).
    public static IServiceCollection KntAddCors(this IServiceCollection services)
    {
        services.AddCors();
        services.AddOptions<CorsOptions>().Configure<IConfiguration>((options, configuration) =>
        {
            var origins = AllowedOrigins(configuration);
            if (origins.Length == 0)
                return;

            options.AddPolicy(Policy, policy =>
            {
                if (origins.Contains("*"))
                    policy.AllowAnyOrigin();
                else
                    policy.WithOrigins(origins);
                policy.AllowAnyMethod().AllowAnyHeader();
            });
        });

        return services;
    }

    // Between UseRouting and UseAuthentication.
    public static IApplicationBuilder UseKntCors(this IApplicationBuilder app, IConfiguration configuration) =>
        AllowedOrigins(configuration).Length == 0 ? app : app.UseCors(Policy);

    private static string[] AllowedOrigins(IConfiguration configuration) =>
        (configuration.GetSection("Cors").Get<CorsConfig>()?.AllowedOrigins ?? [])
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(o => o.Trim())
            .ToArray();
}
