using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// "Forgot your password?" in the Server: api/users/forgot-password emails a link to the configured web address
/// (always the same answer), and api/users/reset-password sets the new password with its token, signs the user in
/// and closes the sessions opened before. See PasswordResetRestrictionsInProcessTests for what is refused.
/// </summary>
[TestClass]
public class PasswordResetInProcessTests
{
    private const string Password = "Reset-Password-1!";
    private const string NewPassword = "Reset-Password-2!";
    private const string PublicBaseUrl = "https://knote.tests/KNote/";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _adminClient = null!;
    private static readonly FakeEmailSender EmailSender = new();

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _adminClient) = await InProcessTestHost.CreateAuthenticatedClientAsync(factory =>
        {
            factory.AppConfiguration["PasswordReset:PublicBaseUrl"] = PublicBaseUrl;
            factory.AppConfiguration["Email:Host"] = "smtp.knote.tests";
            factory.AppConfiguration["Email:FromAddress"] = "knote@knote.tests";
            factory.AppConfiguration["RateLimits:PasswordResetPerHour"] = "1000";
            factory.TestServices = services => services.AddSingleton<IEmailSender>(EmailSender);
        });
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _adminClient.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task Forgot_SendsALink_ThatSetsANewPassword_Once()
    {
        var (user, client) = await RegisterAsync();

        var forgot = await ForgotAsync(user.UserName);
        var email = await EmailSender.WaitForAsync();

        Assert.IsTrue(forgot.IsValid, forgot.ErrorMessage);
        Assert.IsNotNull(email, "No email sent.");
        Assert.AreEqual(user.EMail, email.To);
        StringAssert.Contains(email.Body, $"{PublicBaseUrl}reset-password?token=");
        var token = TokenIn(email.Body);

        var httpRes = await _factory.CreateClient().PostAsJsonAsync("api/users/reset-password", new UserResetPasswordDto { Token = token, NewPassword = NewPassword });
        var reset = await httpRes.Content.ReadFromJsonAsync<Result<UserTokenDto>>();

        Assert.AreEqual(HttpStatusCode.OK, httpRes.StatusCode, reset?.ErrorMessage);
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await client.GetAsync("api/users/me")).StatusCode, "The session opened before.");
        using var signedIn = _factory.CreateClient();
        signedIn.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", reset!.Entity.token);
        Assert.AreEqual(HttpStatusCode.OK, (await signedIn.GetAsync("api/users/me")).StatusCode, "Signed in by the reset.");
        Assert.IsTrue(await CanSignInAsync(user.UserName, NewPassword));

        var again = await _factory.CreateClient().PostAsJsonAsync("api/users/reset-password", new UserResetPasswordDto { Token = token, NewPassword = "Reset-Password-3!" });
        Assert.AreEqual(HttpStatusCode.BadRequest, again.StatusCode);
        Assert.IsTrue(await CanSignInAsync(user.UserName, NewPassword));
    }

    [TestMethod]
    public async Task Forgot_ByEmail_SendsTheLinkToIt()
    {
        var (user, _) = await RegisterAsync();

        var forgot = await ForgotAsync(user.EMail);
        var email = await EmailSender.WaitForAsync();

        Assert.IsTrue(forgot.IsValid, forgot.ErrorMessage);
        Assert.AreEqual(user.EMail, email?.To);
    }

    [TestMethod]
    public async Task Forgot_UnknownUser_SameAnswer_AndNoEmail()
    {
        var (user, _) = await RegisterAsync();
        var known = await ForgotAsync(user.UserName);
        await EmailSender.WaitForAsync();

        var unknown = await ForgotAsync($"nobody-{Guid.NewGuid():N}");

        Assert.IsTrue(unknown.IsValid, unknown.ErrorMessage);
        Assert.AreEqual(known.Entity, unknown.Entity);
        Assert.IsNull(await EmailSender.WaitForAsync(TimeSpan.FromMilliseconds(500)));
    }

    [TestMethod]
    public async Task Reset_WithAnInvalidToken_IsABadRequestWithItsReason()
    {
        var httpRes = await _factory.CreateClient().PostAsJsonAsync("api/users/reset-password", new UserResetPasswordDto { Token = "not-a-token", NewPassword = NewPassword });
        var res = await httpRes.Content.ReadFromJsonAsync<Result<UserTokenDto>>();

        Assert.AreEqual(HttpStatusCode.BadRequest, httpRes.StatusCode);
        StringAssert.Contains(res!.ErrorMessage, "not valid");
    }

    private static async Task<Result<string>> ForgotAsync(string userNameOrEMail)
    {
        var httpRes = await _factory.CreateClient().PostAsJsonAsync("api/users/forgot-password", new UserForgotPasswordDto { UserNameOrEMail = userNameOrEMail });
        return (await httpRes.Content.ReadFromJsonAsync<Result<string>>())!;
    }

    private static string TokenIn(string body)
    {
        var match = Regex.Match(body, "token=([^\"&<\\s]+)");
        Assert.IsTrue(match.Success, body);
        return Uri.UnescapeDataString(WebUtility.HtmlDecode(match.Groups[1].Value));
    }

    private static async Task<(UserRegisterDto User, HttpClient Client)> RegisterAsync()
    {
        var user = new UserRegisterDto
        {
            UserName = $"rst-{Guid.NewGuid():N}"[..24],
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = "Reset in-process user",
            Password = Password
        };
        var client = _factory.CreateClient();
        var token = await (await client.PostAsJsonAsync("api/users/register", user)).Content.ReadFromJsonAsync<UserTokenDto>();
        Assert.IsTrue(token!.success, token.error);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.token);
        return (user, client);
    }

    private static async Task<bool> CanSignInAsync(string userName, string password)
    {
        var httpRes = await _factory.CreateClient().PostAsJsonAsync("api/users/login", new UserCredentialsDto { UserName = userName, Password = password });
        return (await httpRes.Content.ReadFromJsonAsync<UserTokenDto>())?.success == true;
    }
}

/// <summary>
/// What the Server refuses around passwords: password reset without its configuration, and too many attempts
/// (KntRateLimits). A host for each test, as the limits count every request of a host.
/// </summary>
[TestClass]
public class PasswordResetRestrictionsInProcessTests
{
    [TestMethod]
    public async Task Forgot_WithoutTheEmailSettings_IsNotAvailable()
    {
        await using var factory = new KNoteWebApplicationFactory();
        using var client = factory.CreateClient();

        var httpRes = await client.PostAsJsonAsync("api/users/forgot-password", new UserForgotPasswordDto { UserNameOrEMail = "adminKNote" });
        var res = await httpRes.Content.ReadFromJsonAsync<Result<string>>();

        Assert.AreEqual(HttpStatusCode.BadRequest, httpRes.StatusCode);
        StringAssert.Contains(res!.ErrorMessage, "not available");
    }

    [TestMethod]
    public async Task Forgot_TooManyTimes_IsRefused_WithItsReason()
    {
        await using var factory = new KNoteWebApplicationFactory();
        factory.AppConfiguration["RateLimits:PasswordResetPerHour"] = "2";
        using var client = factory.CreateClient();

        HttpResponseMessage httpRes = null!;
        for (var i = 0; i < 3; i++)
            httpRes = await client.PostAsJsonAsync("api/users/forgot-password", new UserForgotPasswordDto { UserNameOrEMail = "adminKNote" });
        var res = await httpRes.Content.ReadFromJsonAsync<Result<string>>();

        Assert.AreEqual(HttpStatusCode.TooManyRequests, httpRes.StatusCode);
        StringAssert.Contains(res!.ErrorMessage, "Too many attempts");
    }

    [TestMethod]
    public async Task Login_TooManyTimes_IsRefused_WithItsReason()
    {
        await using var factory = new KNoteWebApplicationFactory();
        factory.AppConfiguration["RateLimits:SignInPerMinute"] = "2";
        using var client = factory.CreateClient();

        HttpResponseMessage httpRes = null!;
        for (var i = 0; i < 3; i++)
            httpRes = await client.PostAsJsonAsync("api/users/login", new UserCredentialsDto { UserName = "adminKNote", Password = "Wrong-Password-1!" });
        var res = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();

        Assert.AreEqual(HttpStatusCode.TooManyRequests, httpRes.StatusCode);
        Assert.IsFalse(res!.success);
        StringAssert.Contains(res.error, "Too many attempts");
    }
}
