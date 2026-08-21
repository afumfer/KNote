using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// In-process equivalent of WebApiIntegrationTests/NotesTests.cs - see FoldersInProcessTests for
/// the general pattern (fresh per-class Sqlite database, self-registered Admin user).
///
/// Unlike the live-server NotesTests, this suite does not depend on any pre-existing seed content
/// (e.g. a note about "Hopper") - each test creates whatever data it needs and searches/filters for
/// that, since the per-class Sqlite database only carries KntDbContext's own seed data.
/// </summary>
[TestClass]
public class NotesInProcessTests
{
    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _httpClient = null!;
    private static FolderInfoDto _folder = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _httpClient) = await InProcessTestHost.CreateAuthenticatedClientAsync();

        var foldersRes = await _httpClient.GetAsync("api/folders");
        var folders = await foldersRes.Content.ReadFromJsonAsync<Result<List<FolderInfoDto>>>();
        _folder = folders!.Entity!.First();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task Get_HomeNotes()
    {
        var httpRes = await _httpClient.GetAsync("api/notes/homenotes");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<List<NoteInfoDto>>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        // KntDbContext seeds 2 notes ("Wellcome to KNote", "{AppName} documentation") in the Home folder.
        Assert.AreEqual(2, res!.Entity!.Count);
    }

    [TestMethod]
    public async Task Execute_Search_And_Filter()
    {
        string uniqueTopic = $"__PILOT_SEARCH_{Guid.NewGuid():N}__";
        var note = await CreateNoteAsync(uniqueTopic);

        try
        {
            var searchRes = await _httpClient.GetAsync($"api/notes/search?textSearch={uniqueTopic}&pageNumber=1&pageSize=25");
            var search = await searchRes.Content.ReadFromJsonAsync<Result<List<NoteInfoDto>>>();

            Assert.IsTrue(searchRes.IsSuccessStatusCode);
            Assert.IsNotNull(search?.Entity);
            Assert.IsTrue(search!.Entity!.Any(n => n.NoteId == note.NoteId));

            var filterDto = new NotesFilterDto { Topic = uniqueTopic, PageIdentifier = new PageIdentifier { PageNumber = 1, PageSize = 25 } };
            var filterRes = await _httpClient.PostAsJsonAsync("api/notes/filter", filterDto);
            var filter = await filterRes.Content.ReadFromJsonAsync<Result<List<NoteInfoDto>>>();

            Assert.IsTrue(filterRes.IsSuccessStatusCode);
            Assert.IsNotNull(filter?.Entity);
            Assert.IsTrue(filter!.Entity!.Any(n => n.NoteId == note.NoteId));
        }
        finally
        {
            await _httpClient.DeleteAsync($"api/notes/{note.NoteId}");
        }
    }

    [TestMethod]
    public async Task Execute_BasicCRUD()
    {
        // Create
        string topic = "__TEST NOTE ###__";
        var note = await CreateNoteAsync(topic);
        Assert.AreEqual(topic, note.Topic);

        // Get
        var httpRes = await _httpClient.GetAsync($"api/notes/{note.NoteId}");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(note.NoteId, res!.Entity!.NoteId);
        Assert.AreEqual(topic, res.Entity.Topic);

        // Update
        var updated = res.Entity;
        string newTopic = $"{topic} UPDATED!!";
        updated.Topic = newTopic;
        updated.FolderDto = _folder; // see CreateNoteAsync's comment about this nested property
        httpRes = await _httpClient.PutAsJsonAsync("api/notes", updated);
        if (!httpRes.IsSuccessStatusCode)
            Assert.Fail($"Status: {httpRes.StatusCode}, Body: {await httpRes.Content.ReadAsStringAsync()}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(newTopic, res!.Entity!.Topic);

        // Delete
        httpRes = await _httpClient.DeleteAsync($"api/notes/{note.NoteId}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreEqual(note.NoteId, res!.Entity!.NoteId);
    }

    private static async Task<NoteDto> CreateNoteAsync(string topic)
    {
        NoteDto note = new()
        {
            NoteId = Guid.Empty,
            Topic = topic,
            Description = "Created by NotesInProcessTests.",
            FolderId = _folder.FolderId,
            // MVC validates this nested complex property recursively, and NoteDto.FolderDto's getter
            // lazily defaults to an empty FolderDto if left unset - which then fails its own
            // [Required] Name validation. Provide the matching folder to avoid that.
            FolderDto = _folder,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        };

        var httpRes = await _httpClient.PostAsJsonAsync("api/notes", note);
        if (!httpRes.IsSuccessStatusCode)
            Assert.Fail($"Status: {httpRes.StatusCode}, Body: {await httpRes.Content.ReadAsStringAsync()}");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreNotEqual(Guid.Empty, res!.Entity!.NoteId);

        return res.Entity;
    }
}
