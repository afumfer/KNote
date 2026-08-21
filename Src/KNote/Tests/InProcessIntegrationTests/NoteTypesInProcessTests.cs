using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// In-process equivalent of WebApiIntegrationTests/NoteTypesTests.cs - see FoldersInProcessTests
/// for the general pattern (fresh per-class Sqlite database, self-registered Admin user).
/// </summary>
[TestClass]
public class NoteTypesInProcessTests
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
        string noteTypeName = "__TEST_NOTETYPE_###__";
        string noteTypeDescription = "__TEST_NOTETYPE_DESCRIPTION_###__";
        Guid noteTypeId = Guid.Empty;
        NoteTypeDto noteType = new() { NoteTypeId = noteTypeId, Name = noteTypeName, Description = noteTypeDescription };

        var httpRes = await _httpClient.PostAsJsonAsync("api/notetypes", noteType);
        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTypeDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreNotEqual(noteTypeId, res!.Entity!.NoteTypeId);
        Assert.AreEqual(noteTypeName, res.Entity.Name);
        Assert.AreEqual(noteTypeDescription, res.Entity.Description);

        noteTypeId = res.Entity.NoteTypeId;

        // Get_All - confirm the created note type shows up
        httpRes = await _httpClient.GetAsync("api/notetypes");
        var resList = await httpRes.Content.ReadFromJsonAsync<Result<List<NoteTypeDto>>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(resList?.Entity);
        Assert.IsTrue(resList!.Entity!.Any(nt => nt.NoteTypeId == noteTypeId));

        // Get
        httpRes = await _httpClient.GetAsync($"api/notetypes/{noteTypeId}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTypeDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(noteTypeId, res!.Entity!.NoteTypeId);

        // Update
        noteType = res.Entity;
        string newDescription = $"{noteType.Description} UPDATED!!";
        noteType.Description = newDescription;
        httpRes = await _httpClient.PutAsJsonAsync("api/notetypes", noteType);
        res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTypeDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(newDescription, res!.Entity!.Description);

        // Delete
        httpRes = await _httpClient.DeleteAsync($"api/notetypes/{noteTypeId}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTypeDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(noteTypeId, res!.Entity!.NoteTypeId);
    }
}
