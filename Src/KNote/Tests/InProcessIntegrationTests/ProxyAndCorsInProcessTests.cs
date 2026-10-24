using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Tests.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The "ForwardedHeaders" section (a Server behind a reverse proxy: the sign-in limits count each client, not the proxy)
/// and the "Cors" section (no other origin by default). One host per test: the limits count every request of a host.
/// </summary>
[TestClass]
public class ProxyAndCorsInProcessTests
{
    private const string OtherOrigin = "https://other.example";

    [TestMethod]
    public async Task ForwardedHeaders_Enabled_EachClientBehindTheProxyHasItsOwnSignInLimit()
    {
        await using var factory = NewFactoryBehindAProxy(enabled: true);
        using var client = factory.CreateClient();

        var first = await LoginFromAsync(client, "10.0.0.1");
        var second = await LoginFromAsync(client, "10.0.0.1");
        var otherClient = await LoginFromAsync(client, "10.0.0.2");

        Assert.AreNotEqual(HttpStatusCode.TooManyRequests, first);
        Assert.AreEqual(HttpStatusCode.TooManyRequests, second);
        Assert.AreNotEqual(HttpStatusCode.TooManyRequests, otherClient);
    }

    [TestMethod]
    public async Task ForwardedHeaders_Disabled_TheHeadersAreIgnored()
    {
        await using var factory = NewFactoryBehindAProxy(enabled: false);
        using var client = factory.CreateClient();

        await LoginFromAsync(client, "10.0.0.1");
        var otherClient = await LoginFromAsync(client, "10.0.0.2");

        Assert.AreEqual(HttpStatusCode.TooManyRequests, otherClient);
    }

    [TestMethod]
    public void ForwardedHeaders_AProxyThatIsNotAnAddress_IsRefused()
    {
        var config = new ForwardedHeadersConfig { KnownProxies = ["not-an-address"] };

        Assert.ThrowsExactly<InvalidOperationException>(() => KntForwardedHeaders.Apply(config, new ForwardedHeadersOptions()));
    }

    [TestMethod]
    public void ForwardedHeaders_KnownProxiesAndNetworks_AreTrusted()
    {
        var config = new ForwardedHeadersConfig { KnownProxies = ["192.168.1.10"], KnownNetworks = ["10.0.0.0/8"], ForwardLimit = 2 };
        var options = new ForwardedHeadersOptions();

        KntForwardedHeaders.Apply(config, options);

        Assert.Contains(IPAddress.Parse("192.168.1.10"), options.KnownProxies);
        Assert.Contains(System.Net.IPNetwork.Parse("10.0.0.0/8"), options.KnownIPNetworks);
        Assert.AreEqual(2, options.ForwardLimit);
    }

    [TestMethod]
    public async Task Cors_ByDefault_NoOtherOriginIsAllowed()
    {
        await using var factory = new KNoteWebApplicationFactory();
        using var client = factory.CreateClient();

        var httpRes = await GetFromOriginAsync(client);

        Assert.IsFalse(httpRes.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [TestMethod]
    public async Task Cors_AConfiguredOrigin_IsAllowed()
    {
        await using var factory = new KNoteWebApplicationFactory();
        factory.AppConfiguration["Cors:AllowedOrigins:0"] = OtherOrigin;
        using var client = factory.CreateClient();

        var httpRes = await GetFromOriginAsync(client);

        Assert.AreEqual(OtherOrigin, httpRes.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    // The test server gives no client address: as if every request came from a proxy on the same machine.
    private static KNoteWebApplicationFactory NewFactoryBehindAProxy(bool enabled)
    {
        var factory = new KNoteWebApplicationFactory();
        factory.AppConfiguration["RateLimits:SignInPerMinute"] = "1";
        factory.AppConfiguration["ForwardedHeaders:Enabled"] = enabled.ToString();
        factory.TestServices = services => services.AddTransient<IStartupFilter, LoopbackClientStartupFilter>();
        return factory;
    }

    private static async Task<HttpStatusCode> LoginFromAsync(HttpClient client, string clientAddress)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/users/login")
        {
            Content = JsonContent.Create(new UserCredentialsDto { UserName = "adminKNote", Password = "Wrong-Password-1!" })
        };
        request.Headers.Add("X-Forwarded-For", clientAddress);
        return (await client.SendAsync(request)).StatusCode;
    }

    private static async Task<HttpResponseMessage> GetFromOriginAsync(HttpClient client)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/systemvalues/repositoryname");
        request.Headers.Add("Origin", OtherOrigin);
        return await client.SendAsync(request);
    }

    private sealed class LoopbackClientStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use((context, nextMiddleware) =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Loopback;
                return nextMiddleware(context);
            });
            next(app);
        };
    }
}
