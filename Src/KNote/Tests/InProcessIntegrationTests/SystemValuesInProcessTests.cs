using System.Net.Http;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The repository's name for the header of the Web (api/systemvalues/repositoryname): read without signing
/// in, since Home is public. The rest of SystemValuesController stays for Admins only.
/// </summary>
[TestClass]
public class SystemValuesInProcessTests
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
    public async Task RepositoryName_WithoutSigningIn_IsTheNameGivenToTheDatabase()
    {
        using var anonymous = _factory.CreateClient();

        var httpRes = await anonymous.GetAsync("api/systemvalues/repositoryname");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<string>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        // The RepositoryRef:Name KNoteWebApplicationFactory configures, given to its new database.
        Assert.AreEqual("KNoteInProcessTests", res?.Entity);
    }

    [TestMethod]
    public async Task AppSettings_WithoutSigningIn_IsUnauthorized()
    {
        using var anonymous = _factory.CreateClient();

        var httpRes = await anonymous.GetAsync("api/systemvalues/getappsettings");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, httpRes.StatusCode);
    }
}
