using System.Net.Http;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// In-process equivalent of WebApiIntegrationTests/FoldersTests.cs: boots the Server host via
/// WebApplicationFactory against a fresh, per-class Sqlite database instead of requiring an
/// externally running Server, and authenticates as its own registered Admin user instead of
/// relying on pre-existing/unknown seeded credentials.
/// </summary>
[TestClass]
public class FoldersInProcessTests
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
        var httpRes = await _httpClient.GetAsync("api/folders");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<List<FolderInfoDto>>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        // KntDbContext seeds exactly 3 folders (Home, Documentation, Temp) via ModelBuilderExtensions.Seed().
        Assert.AreEqual(3, res!.Entity!.Count);
    }

    [TestMethod]
    public async Task Get_Tree()
    {
        var httpRes = await _httpClient.GetAsync("api/folders/tree");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<List<FolderDto>>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.IsTrue(res!.Entity!.Count > 0);
    }

    [TestMethod]
    public async Task Execute_BasicCRUD()
    {
        // Create
        string folderName = "__TEST FOLDER ###__";
        Guid folderId = Guid.Empty;
        FolderDto folder = new() { FolderId = folderId, FolderNumber = 0, Name = folderName, ParentId = null };

        var httpRes = await _httpClient.PostAsJsonAsync("api/folders", folder);
        var res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreNotEqual(folderId, res!.Entity!.FolderId);
        Assert.AreEqual(folderName, res.Entity.Name);

        folderId = res.Entity.FolderId;

        // Get
        httpRes = await _httpClient.GetAsync($"api/folders/{folderId}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(folderId, res!.Entity!.FolderId);
        Assert.AreEqual(folderName, res.Entity.Name);

        // Update
        folder = res.Entity;
        string newFolderName = $"{folder.Name} UPDATED!!";
        folder.Name = newFolderName;
        httpRes = await _httpClient.PutAsJsonAsync("api/folders", folder);
        res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(newFolderName, res!.Entity!.Name);

        // Delete
        httpRes = await _httpClient.DeleteAsync($"api/folders/{folderId}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(folderId, res!.Entity!.FolderId);
    }
}
