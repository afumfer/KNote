using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;
using Microsoft.IdentityModel.Tokens;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The signed in user's own account (api/users/me) and the sessions closed by a password change (the password
/// fingerprint of the tokens, KntPasswordStamps): changing the password gives a new token and refuses the old ones;
/// so does an Admin setting it (api/users/{id}/password); a token without the fingerprint is refused.
/// </summary>
[TestClass]
public class UserAccountInProcessTests
{
    private const string Password = "Account-Password-1!";
    private const string NewPassword = "Account-Password-2!";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _adminClient = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _adminClient) = await InProcessTestHost.CreateAuthenticatedClientAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _adminClient.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task Profile_GetAndSave_TheSignedInUsersOwn()
    {
        var (user, client) = await RegisterAsync();

        var profile = await ReadAsync<UserProfileDto>(await client.GetAsync("api/users/me"));
        Assert.IsTrue(profile.IsValid, profile.ErrorMessage);
        Assert.AreEqual(user.UserName, profile.Entity.UserName);
        Assert.AreEqual(user.EMail, profile.Entity.EMail);

        profile.Entity.FullName = "Changed full name";
        profile.Entity.UserName = "someone-else";
        var saved = await ReadAsync<UserProfileDto>(await client.PutAsJsonAsync("api/users/me", profile.Entity));

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        Assert.AreEqual("Changed full name", saved.Entity.FullName);
        Assert.AreEqual(user.UserName, saved.Entity.UserName);
    }

    [TestMethod]
    public async Task Profile_NewEmailWithoutPassword_IsABadRequestWithItsReason()
    {
        var (user, client) = await RegisterAsync();

        var httpRes = await client.PutAsJsonAsync("api/users/me",
            new UserProfileDto { EMail = $"{Guid.NewGuid():N}@knote.tests", FullName = user.FullName });
        var res = await ReadAsync<UserProfileDto>(httpRes);

        Assert.AreEqual(HttpStatusCode.BadRequest, httpRes.StatusCode);
        StringAssert.Contains(res.ErrorMessage, "current password");
    }

    [TestMethod]
    public async Task Profile_WithoutSession_IsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var httpRes = await client.GetAsync("api/users/me");

        Assert.AreEqual(HttpStatusCode.Unauthorized, httpRes.StatusCode);
    }

    [TestMethod]
    public async Task ChangePassword_GivesANewToken_AndClosesTheOtherSessions()
    {
        var (user, client) = await RegisterAsync();
        using var otherSession = await SignInAsync(user.UserName, Password);

        var httpRes = await client.PostAsJsonAsync("api/users/me/password",
            new UserChangePasswordDto { CurrentPassword = Password, NewPassword = NewPassword });
        var res = await ReadAsync<UserTokenDto>(httpRes);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.IsTrue(res.Entity.success);
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await client.GetAsync("api/users/me")).StatusCode, "The token used to change it.");
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await otherSession.GetAsync("api/users/me")).StatusCode, "Another session.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", res.Entity.token);
        Assert.AreEqual(HttpStatusCode.OK, (await client.GetAsync("api/users/me")).StatusCode, "The new token.");
        using var newSession = await SignInAsync(user.UserName, NewPassword);
        Assert.AreEqual(HttpStatusCode.OK, (await newSession.GetAsync("api/users/me")).StatusCode);
    }

    [TestMethod]
    public async Task ChangePassword_WrongCurrentOne_KeepsTheSession()
    {
        var (_, client) = await RegisterAsync();

        var httpRes = await client.PostAsJsonAsync("api/users/me/password",
            new UserChangePasswordDto { CurrentPassword = "Wrong-Password-1!", NewPassword = NewPassword });
        var res = await ReadAsync<UserTokenDto>(httpRes);

        Assert.AreEqual(HttpStatusCode.BadRequest, httpRes.StatusCode);
        StringAssert.Contains(res.ErrorMessage, "not correct");
        Assert.AreEqual(HttpStatusCode.OK, (await client.GetAsync("api/users/me")).StatusCode);
    }

    [TestMethod]
    public async Task AdminSetPassword_ClosesTheUsersSessions()
    {
        var (user, client) = await RegisterAsync();

        var res = await ReadAsync<UserDto>(await _adminClient.PutAsJsonAsync($"api/users/{user.UserId}/password",
            new UserSetPasswordDto { NewPassword = NewPassword }));

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await client.GetAsync("api/users/me")).StatusCode);
        using var newSession = await SignInAsync(user.UserName, NewPassword);
        Assert.AreEqual(HttpStatusCode.OK, (await newSession.GetAsync("api/users/me")).StatusCode);
    }

    [TestMethod]
    public async Task AdminSetPassword_NotForANonAdmin()
    {
        var (user, client) = await RegisterAsync(); // a Guest: the class's user is the second Admin

        var httpRes = await client.PutAsJsonAsync($"api/users/{user.UserId}/password", new UserSetPasswordDto { NewPassword = NewPassword });

        Assert.AreEqual(HttpStatusCode.Forbidden, httpRes.StatusCode);
    }

    [TestMethod]
    public async Task TokenWithoutPasswordStamp_IsRefused()
    {
        var (user, client) = await RegisterAsync();
        var claims = new[] { new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.Role, "Guest") };
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("KNoteTestsOnlySigningSecret_DoNotUseInProduction_1234567890"));
        var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

        Assert.AreEqual(HttpStatusCode.Unauthorized, (await client.GetAsync("api/users/me")).StatusCode);
    }

    private static async Task<(UserDto User, HttpClient Client)> RegisterAsync()
    {
        var dto = new UserRegisterDto
        {
            UserName = $"acc-{Guid.NewGuid():N}"[..24],
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = "Account in-process user",
            Password = Password
        };
        var client = _factory.CreateClient();
        var httpRes = await client.PostAsJsonAsync("api/users/register", dto);
        var token = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();
        Assert.IsTrue(token!.success, token.error);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.token);

        var user = await ReadAsync<UserDto>(await _adminClient.GetAsync($"api/users/{token.uid}"));
        Assert.IsTrue(user.IsValid, user.ErrorMessage);
        return (user.Entity, client);
    }

    private static async Task<HttpClient> SignInAsync(string userName, string password)
    {
        var client = _factory.CreateClient();
        var httpRes = await client.PostAsJsonAsync("api/users/login", new UserCredentialsDto { UserName = userName, Password = password });
        var token = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();
        Assert.IsTrue(token!.success, token.error);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.token);
        return client;
    }

    private static async Task<Result<T>> ReadAsync<T>(HttpResponseMessage httpRes)
    {
        var res = await httpRes.Content.ReadFromJsonAsync<Result<T>>();
        Assert.IsNotNull(res, $"No Result in the answer ({httpRes.StatusCode}).");
        return res!;
    }
}
