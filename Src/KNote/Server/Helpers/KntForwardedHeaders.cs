using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KNote.Server.Helpers;

/// <summary>
/// The "ForwardedHeaders" configuration section: whether the Server is behind a reverse proxy (nginx, Apache...) whose
/// X-Forwarded-For/Proto/Host headers it must trust. Off by default: IIS (ANCM) already gives the client's address
/// and scheme. KnownProxies (addresses) and KnownNetworks (CIDR, e.g. 10.0.0.0/8) are added to the loopback
/// addresses, which are always trusted (a proxy on the same machine).
/// </summary>
public class ForwardedHeadersConfig
{
    public bool Enabled { get; set; }

    public List<string> KnownProxies { get; set; } = new();

    public List<string> KnownNetworks { get; set; } = new();

    public int ForwardLimit { get; set; } = 1;
}

/// <summary>
/// Behind a reverse proxy, without these headers every request would come from the proxy: the sign-in limits
/// (KntRateLimits, per client address) would be shared by everyone, and the urls built from the request
/// (LocalFileStore, ChatHub) would have the proxy's scheme and host.
/// </summary>
public static class KntForwardedHeaders
{
    public static IServiceCollection KntAddForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        // Bound when used (the in-process tests can override it).
        services.Configure<ForwardedHeadersConfig>(configuration.GetSection("ForwardedHeaders"));
        services.AddOptions<ForwardedHeadersOptions>()
            .Configure<IOptions<ForwardedHeadersConfig>>((options, config) => Apply(config.Value, options));

        return services;
    }

    // The first middleware, so everything after it sees the client's address, scheme and host.
    public static IApplicationBuilder UseKntForwardedHeaders(this IApplicationBuilder app)
    {
        var config = app.ApplicationServices.GetRequiredService<IOptions<ForwardedHeadersConfig>>().Value;
        return config.Enabled ? app.UseForwardedHeaders() : app;
    }

    public static void Apply(ForwardedHeadersConfig config, ForwardedHeadersOptions options)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        options.ForwardLimit = Math.Max(1, config.ForwardLimit);

        foreach (var proxy in config.KnownProxies)
        {
            if (!IPAddress.TryParse(proxy?.Trim(), out var address))
                throw new InvalidOperationException($"ForwardedHeaders:KnownProxies: '{proxy}' is not an IP address.");
            options.KnownProxies.Add(address);
        }

        foreach (var network in config.KnownNetworks)
        {
            if (!System.Net.IPNetwork.TryParse(network?.Trim(), out var ipNetwork))
                throw new InvalidOperationException($"ForwardedHeaders:KnownNetworks: '{network}' is not a network in CIDR notation (e.g. 10.0.0.0/8).");
            options.KnownIPNetworks.Add(ipNetwork);
        }
    }
}
