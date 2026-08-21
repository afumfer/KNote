using System.Net.Http;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// In-process equivalent of WebApiIntegrationTests/UsersTests.cs - see FoldersInProcessTests for
/// the general pattern (fresh per-class Sqlite database, self-registered Admin user).
/// </summary>
[TestClass]
public class UsersInProcessTests
{
    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _httpClient = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _httpClient) = await InProcessTestHost.CreateAuthenticatedClientAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task Get_All()
    {
        var httpRes = await _httpClient.GetAsync("api/users");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<List<UserDto>>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        // KntDbContext seeds 3 users (owner, adminKNote, user1), plus the Admin user this test
        // class registered for itself.
        Assert.AreEqual(4, res!.Entity!.Count);
    }

    [TestMethod]
    public async Task Get_WithPagination()
    {
        var httpRes = await _httpClient.GetAsync("api/users?pageNumber=1&pageSize=3");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<List<UserDto>>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(3, res!.Entity!.Count);
    }

    [TestMethod]
    public async Task Execute_RegisterAndDelete()
    {
        // Register new user
        string userName = $"itest-reg-{Guid.NewGuid():N}"[..24];
        string userEmail = $"{Guid.NewGuid():N}@knote.tests";
        UserRegisterDto user = new()
        {
            UserId = Guid.Empty,
            UserName = userName,
            EMail = userEmail,
            FullName = "__TEST_REGISTERUSER_FULLNAME_###__",
            RoleDefinition = "Public",
            Password = "pass12345abcd!!"
        };

        var httpRes = await _httpClient.PostAsJsonAsync("api/users/register", user);
        var res = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res);
        Assert.IsTrue(res!.success);
        Assert.IsFalse(string.IsNullOrEmpty(res.uid));
        Assert.IsFalse(string.IsNullOrEmpty(res.token));
        Assert.IsTrue(string.IsNullOrEmpty(res.error));

        var userId = Guid.Parse(res.uid);

        // Delete (as the class' Admin user, not the newly registered one)
        httpRes = await _httpClient.DeleteAsync($"api/users/{userId}");
        var resDel = await httpRes.Content.ReadFromJsonAsync<Result<UserDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(resDel?.Entity);
        Assert.AreEqual(userId, resDel!.Entity!.UserId);
    }

    [TestMethod]
    public async Task Execute_BasicCRUD()
    {
        // Create
        string userName = $"itest-crud-{Guid.NewGuid():N}"[..24];
        string userEmail = $"{Guid.NewGuid():N}@knote.tests";
        Guid userId = Guid.Empty;
        UserDto user = new() { UserId = userId, UserName = userName, EMail = userEmail, FullName = "__TEST_CREATEUSER_FULLNAME_###__", RoleDefinition = "Public" };

        var httpRes = await _httpClient.PostAsJsonAsync("api/users", user);
        var res = await httpRes.Content.ReadFromJsonAsync<Result<UserDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreNotEqual(userId, res!.Entity!.UserId);
        Assert.AreEqual(userName, res.Entity.UserName);
        Assert.AreEqual(userEmail, res.Entity.EMail);

        userId = res.Entity.UserId;

        // Get
        httpRes = await _httpClient.GetAsync($"api/users/{userId}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<UserDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(userId, res!.Entity!.UserId);

        // Update
        user = res.Entity;
        string newUserFullName = $"{user.FullName} UPDATED!!";
        user.FullName = newUserFullName;
        httpRes = await _httpClient.PutAsJsonAsync("api/users", user);
        res = await httpRes.Content.ReadFromJsonAsync<Result<UserDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(newUserFullName, res!.Entity!.FullName);

        // Delete
        httpRes = await _httpClient.DeleteAsync($"api/users/{userId}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<UserDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(userId, res!.Entity!.UserId);
    }
}
