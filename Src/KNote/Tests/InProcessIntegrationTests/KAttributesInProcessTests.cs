using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// In-process equivalent of WebApiIntegrationTests/KAttributesTests.cs - see FoldersInProcessTests
/// for the general pattern (fresh per-class Sqlite database, self-registered Admin user).
/// </summary>
[TestClass]
public class KAttributesInProcessTests
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
    public async Task Execute_BasicCRUD()
    {
        // Create
        string kattributeName = "__TEST_KATTRIBUTE_###__";
        string kattributeDescription = "__TEST_KATTRIBUTE_DESCRIPTION_###__";
        Guid kattributeId = Guid.Empty;
        KAttributeInfoDto kAttribute = new() { KAttributeId = kattributeId, Name = kattributeName, Description = kattributeDescription };

        var httpRes = await _httpClient.PostAsJsonAsync("api/kattributes", kAttribute);
        var res = await httpRes.Content.ReadFromJsonAsync<Result<KAttributeInfoDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreNotEqual(kattributeId, res!.Entity!.KAttributeId);
        Assert.AreEqual(kattributeName, res.Entity.Name);
        Assert.AreEqual(kattributeDescription, res.Entity.Description);

        kattributeId = res.Entity.KAttributeId;

        // Get_All - confirm the created attribute shows up
        httpRes = await _httpClient.GetAsync("api/kattributes");
        var resList = await httpRes.Content.ReadFromJsonAsync<Result<List<KAttributeInfoDto>>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(resList?.Entity);
        Assert.IsTrue(resList!.Entity!.Any(a => a.KAttributeId == kattributeId));

        // Get
        httpRes = await _httpClient.GetAsync($"api/kattributes/{kattributeId}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<KAttributeInfoDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(kattributeId, res!.Entity!.KAttributeId);

        // Update
        kAttribute = res.Entity;
        string newDescription = $"{kAttribute.Description} UPDATED!!";
        kAttribute.Description = newDescription;
        httpRes = await _httpClient.PutAsJsonAsync("api/kattributes", kAttribute);
        res = await httpRes.Content.ReadFromJsonAsync<Result<KAttributeInfoDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(newDescription, res!.Entity!.Description);

        // Delete
        httpRes = await _httpClient.DeleteAsync($"api/kattributes/{kattributeId}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<KAttributeInfoDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(kattributeId, res!.Entity!.KAttributeId);
    }
}
