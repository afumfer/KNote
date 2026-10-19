using System;
using System.Collections.Generic;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KNote.Server.Helpers;

/// <summary>
/// The "RateLimits" configuration section: how many attempts each address may make.
/// </summary>
public class RateLimitsConfig
{
    public int SignInPerMinute { get; set; } = 10;

    public int PasswordResetPerHour { get; set; } = 10;
}

/// <summary>
/// Limits on the anonymous endpoints that check a password or send an email, per client address
/// ([EnableRateLimiting] on the actions): guessing passwords, or flooding someone's mailbox with links. Behind a
/// reverse proxy every client would share the proxy's address.
/// </summary>
public static class KntRateLimits
{
    public const string SignIn = "knt-sign-in";
    public const string PasswordReset = "knt-password-reset";

    private const string TooManyAttempts = "Too many attempts. Try again later.";

    public static IServiceCollection KntAddRateLimits(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitsConfig>(configuration.GetSection("RateLimits"));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Read as a Result<T> (listErrorMessage) or as the UserTokenDto of the sign in (success, error).
            options.OnRejected = async (context, cancellationToken) =>
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    error = TooManyAttempts,
                    listErrorMessage = new List<string> { TooManyAttempts }
                }, cancellationToken);

            options.AddPolicy(SignIn, http => FixedWindow(http, config => config.SignInPerMinute, TimeSpan.FromMinutes(1)));
            options.AddPolicy(PasswordReset, http => FixedWindow(http, config => config.PasswordResetPerHour, TimeSpan.FromHours(1)));
        });

        return services;
    }

    private static RateLimitPartition<string> FixedWindow(HttpContext http, Func<RateLimitsConfig, int> permits, TimeSpan window)
    {
        var config = http.RequestServices.GetRequiredService<IOptions<RateLimitsConfig>>().Value;

        return RateLimitPartition.GetFixedWindowLimiter(http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permits(config), Window = window, QueueLimit = 0 });
    }
}
