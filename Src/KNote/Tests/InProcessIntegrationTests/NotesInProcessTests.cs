using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

    // A description with what could run script, and formatting.
    private const string UnsafeDescription = "**bold** <img src=\"x\" onerror=\"alert(1)\"> [link](javascript:alert(2)) <script>alert(3)</script>";

    private static void AssertSafeHtml(string? html)
    {
        Assert.IsNotNull(html);
        Assert.IsTrue(html.Contains("<strong>bold</strong>"), html);
        Assert.IsFalse(html.Contains("onerror") || html.Contains("javascript:") || html.Contains("<script"), html);
    }

    [TestMethod]
    public async Task Html_ReturnsTheDescriptionAsSafeHtml()
    {
        var httpRes = await _httpClient.PostAsJsonAsync("api/notes/html", UnsafeDescription);
        var res = await httpRes.Content.ReadFromJsonAsync<Result<string>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        AssertSafeHtml(res?.Entity);
    }

    // The web app may send a description's line breaks as LF (browser, Markdown editor), CR LF (as loaded) or, after
    // a former bug, CR CR LF: they are saved as ClientWin writes them, a single CR LF each.
    [TestMethod]
    [DataRow("one\ntwo\n\nthree")]
    [DataRow("one\r\ntwo\r\n\r\nthree")]
    [DataRow("one\r\r\ntwo\r\r\n\r\r\nthree")]
    [DataRow("one\r\ntwo\n\r\r\nthree")]
    public async Task Post_SavesTheDescriptionLineBreaksAsCrLf(string description)
    {
        var note = await CreateNoteAsync("__TEST LINE BREAKS__", description);
        try
        {
            var res = await (await _httpClient.GetAsync($"api/notes/{note.NoteId}")).Content.ReadFromJsonAsync<Result<NoteDto>>();

            Assert.AreEqual("one\r\ntwo\r\n\r\nthree", res!.Entity!.Description);
        }
        finally
        {
            await _httpClient.DeleteAsync($"api/notes/{note.NoteId}");
        }
    }

    // The description's view in ClientWin (ContentType): a Markdown note saved from the web keeps "navigation"
    // (rendered) or "markdown" (text); an HTML one is "html"; an "html" note whose description is no longer HTML
    // goes back to "markdown".
    [TestMethod]
    [DataRow("navigation", "Markdown text", "navigation")]
    [DataRow("markdown", "Markdown text", "markdown")]
    [DataRow("html", "Markdown text", "markdown")]
    [DataRow("markdown", "<BODY><P>html</P></BODY>", "html")]
    public async Task Post_KeepsTheDescriptionViewOfClientWin(string view, string description, string expected)
    {
        var note = await CreateNoteAsync("__TEST CONTENT TYPE__");
        try
        {
            var loaded = (await (await _httpClient.GetAsync($"api/notes/{note.NoteId}")).Content.ReadFromJsonAsync<Result<NoteDto>>())!.Entity!;
            loaded.SetContentTypeExt(new ContentTypeExt { ForDescription = view, ForScript = "knt" });
            loaded.Description = description;
            loaded.FolderDto = _folder;
            var httpRes = await _httpClient.PutAsJsonAsync("api/notes", loaded);
            Assert.IsTrue(httpRes.IsSuccessStatusCode, await httpRes.Content.ReadAsStringAsync());

            var saved = (await (await _httpClient.GetAsync($"api/notes/{note.NoteId}")).Content.ReadFromJsonAsync<Result<NoteDto>>())!.Entity!;
            Assert.AreEqual(expected, saved.GetContentTypeExt().ForDescription);
        }
        finally
        {
            await _httpClient.DeleteAsync($"api/notes/{note.NoteId}");
        }
    }

    // ContentType saved from the web is always ClientWin's JSON (a note in the old plain format is turned into it),
    // and keeps the script's language and the description's lock.
    [TestMethod]
    [DataRow("navigation#", "navigation", "knt", true)]
    [DataRow("markdown", "markdown", "knt", false)]
    [DataRow("{\"des\":\"markdown\",\"scr\":\"cs\",\"blk\":true}", "markdown", "cs", true)]
    [DataRow("{\"des\":\"navigation\",\"scr\":\"py\",\"blk\":false}", "navigation", "py", false)]
    public async Task Post_SavesContentTypeAsJsonKeepingScriptAndLock(string contentType, string view, string script, bool locked)
    {
        var note = await CreateNoteAsync("__TEST CONTENT TYPE JSON__");
        try
        {
            var loaded = (await (await _httpClient.GetAsync($"api/notes/{note.NoteId}")).Content.ReadFromJsonAsync<Result<NoteDto>>())!.Entity!;
            loaded.ContentType = contentType;
            loaded.FolderDto = _folder;
            var httpRes = await _httpClient.PutAsJsonAsync("api/notes", loaded);
            Assert.IsTrue(httpRes.IsSuccessStatusCode, await httpRes.Content.ReadAsStringAsync());

            var saved = (await (await _httpClient.GetAsync($"api/notes/{note.NoteId}")).Content.ReadFromJsonAsync<Result<NoteDto>>())!.Entity!;
            Assert.IsTrue(ContentTypeExtJsonHelper.TryDeserialize(saved.ContentType, out var ct), saved.ContentType);
            Assert.AreEqual(view, ct.ForDescription);
            Assert.AreEqual(script, ct.ForScript);
            Assert.AreEqual(locked, ct.DescriptionBlocked);
        }
        finally
        {
            await _httpClient.DeleteAsync($"api/notes/{note.NoteId}");
        }
    }

    [TestMethod]
    public async Task Html_WithoutSigningIn_IsUnauthorized()
    {
        using var anonymous = _factory.CreateClient();

        var httpRes = await anonymous.PostAsJsonAsync("api/notes/html", "**bold**");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, httpRes.StatusCode);
    }

    [TestMethod]
    public async Task Get_HomeNotes_GivesTheDescriptionsAsSafeHtml()
    {
        var folders = await (await _httpClient.GetAsync("api/folders")).Content.ReadFromJsonAsync<Result<List<FolderInfoDto>>>();
        var home = folders!.Entity!.First(f => f.Name == "Home");
        var note = await CreateNoteAsync("__TEST HOME HTML__", UnsafeDescription, home);
        try
        {
            using var anonymous = _factory.CreateClient();
            var httpRes = await anonymous.GetAsync("api/notes/homenotes");
            var res = await httpRes.Content.ReadFromJsonAsync<Result<List<NoteInfoDto>>>();

            Assert.IsTrue(httpRes.IsSuccessStatusCode);
            AssertSafeHtml(res!.Entity!.Single(n => n.NoteId == note.NoteId).Description);
        }
        finally
        {
            await _httpClient.DeleteAsync($"api/notes/{note.NoteId}");
        }
    }

    // The notes lists of the web app (tree, filter, search) are for every signed in user, Guest included, as the
    // service's commands (KntNotesGetMinimalFilterAsyncCommand, KntNotesGetMinimalSearchAsyncCommand).
    [TestMethod]
    public async Task AGuest_CanFilterAndSearch()
    {
        // Registered after the suite's Admin, so the service makes this user a Guest.
        using var guestClient = _factory.CreateClient();
        var register = await guestClient.PostAsJsonAsync("api/users/register", new UserRegisterDto
        {
            UserName = $"guest-{Guid.NewGuid():N}"[..24],
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = "Guest user",
            Password = "InProcess-Test-Password-1!"
        });
        var token = await register.Content.ReadFromJsonAsync<UserTokenDto>();
        guestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token!.token);

        var filterDto = new NotesFilterDto { FolderId = _folder.FolderId, PageIdentifier = new PageIdentifier { PageNumber = 1, PageSize = 25 } };
        var filterRes = await guestClient.PostAsJsonAsync("api/notes/filter", filterDto);
        var filter = await filterRes.Content.ReadFromJsonAsync<Result<List<NoteMinimalDto>>>();
        var searchRes = await guestClient.GetAsync("api/notes/search?textSearch=KNote&pageNumber=1&pageSize=25");

        Assert.IsTrue(filterRes.IsSuccessStatusCode, filterRes.StatusCode.ToString());
        Assert.IsTrue(filter?.IsValid == true);
        Assert.IsTrue(searchRes.IsSuccessStatusCode, searchRes.StatusCode.ToString());
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
    public async Task Post_WithNonExistentFolder_ReturnsRootCauseNotServiceWrapper()
    {
        NoteDto note = new()
        {
            NoteId = Guid.Empty,
            Topic = "__TEST NOTE BAD FOLDER__",
            Description = "Created by NotesInProcessTests.",
            FolderId = Guid.NewGuid(),
            FolderDto = _folder, // see CreateNoteAsync's comment about this nested property
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        };

        var httpRes = await _httpClient.PostAsJsonAsync("api/notes", note);
        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>();

        Assert.IsFalse(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res);
        Assert.IsTrue(res!.ErrorMessage.StartsWith("Generic error:"), res.ErrorMessage);
        Assert.IsFalse(res.ErrorMessage.Contains("KNote service error"), res.ErrorMessage);
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

    // The web note editor's "Trace notes" tab: api/notes/{id}/tracenotes (both directions, resolved), and
    // api/notes/tracenotes to save (at once) and delete.
    [TestMethod]
    public async Task Execute_TraceNotesCRUD()
    {
        var noteA = await CreateNoteAsync("__TEST TRACE NOTE A__");
        var noteB = await CreateNoteAsync("__TEST TRACE NOTE B__");
        var typeRes = await (await _httpClient.PostAsJsonAsync("api/tracenotetypes", new TraceNoteTypeDto { Name = "__TEST_TRACE_TYPE__" }))
            .Content.ReadFromJsonAsync<Result<TraceNoteTypeDto>>();
        var type = typeRes!.Entity!;

        try
        {
            // Create A -> B
            var httpRes = await _httpClient.PostAsJsonAsync("api/notes/tracenotes",
                new TraceNoteDto { FromId = noteA.NoteId, ToId = noteB.NoteId, TraceNoteTypeId = type.TraceNoteTypeId, Order = 2, Weight = 1.5 });
            var res = await httpRes.Content.ReadFromJsonAsync<Result<TraceNoteDto>>();
            Assert.IsTrue(httpRes.IsSuccessStatusCode, res?.ErrorMessage);
            Assert.AreNotEqual(Guid.Empty, res!.Entity!.TraceNoteId);
            var traceNoteId = res.Entity.TraceNoteId;

            // Outgoing from A, resolved with B and the type
            var fromA = await GetTraceNotesAsync(noteA.NoteId);
            Assert.IsEmpty(fromA.TraceNotesFrom);
            Assert.HasCount(1, fromA.TraceNotesTo);
            var outgoing = fromA.TraceNotesTo[0];
            Assert.AreEqual(traceNoteId, outgoing.TraceNoteId);
            Assert.AreEqual(noteB.NoteId, outgoing.RelatedNoteId);
            Assert.AreEqual($"#{noteB.NoteNumber}", outgoing.RelatedNoteNumber);
            Assert.AreEqual(noteB.Topic, outgoing.RelatedNoteTopic);
            Assert.AreEqual(type.Name, outgoing.TraceNoteTypeName);
            Assert.AreEqual(2, outgoing.Order);
            Assert.AreEqual(1.5, outgoing.Weight);

            // Incoming to B, resolved with A
            var fromB = await GetTraceNotesAsync(noteB.NoteId);
            Assert.IsEmpty(fromB.TraceNotesTo);
            Assert.HasCount(1, fromB.TraceNotesFrom);
            Assert.AreEqual(noteA.NoteId, fromB.TraceNotesFrom[0].RelatedNoteId);
            Assert.AreEqual(noteA.Topic, fromB.TraceNotesFrom[0].RelatedNoteTopic);

            // Update (sent back as the TraceNoteInfoDto it was read as)
            outgoing.Order = 7;
            outgoing.TraceNoteTypeId = null;
            httpRes = await _httpClient.PutAsJsonAsync("api/notes/tracenotes", outgoing);
            Assert.IsTrue(httpRes.IsSuccessStatusCode, await httpRes.Content.ReadAsStringAsync());
            fromA = await GetTraceNotesAsync(noteA.NoteId);
            Assert.AreEqual(7, fromA.TraceNotesTo[0].Order);
            Assert.AreEqual("", fromA.TraceNotesTo[0].TraceNoteTypeName);

            // A duplicate (untyped, as it is now) and a note traced to itself are rejected
            httpRes = await _httpClient.PostAsJsonAsync("api/notes/tracenotes", new TraceNoteDto { FromId = noteA.NoteId, ToId = noteB.NoteId, Weight = 1 });
            res = await httpRes.Content.ReadFromJsonAsync<Result<TraceNoteDto>>();
            Assert.IsFalse(httpRes.IsSuccessStatusCode);
            StringAssert.Contains(res!.ErrorMessage, "already traced");

            httpRes = await _httpClient.PostAsJsonAsync("api/notes/tracenotes", new TraceNoteDto { FromId = noteA.NoteId, ToId = noteA.NoteId, Weight = 1 });
            Assert.IsFalse(httpRes.IsSuccessStatusCode);

            // A type in use is not deleted
            outgoing.TraceNoteTypeId = type.TraceNoteTypeId;
            httpRes = await _httpClient.PutAsJsonAsync("api/notes/tracenotes", outgoing);
            Assert.IsTrue(httpRes.IsSuccessStatusCode, await httpRes.Content.ReadAsStringAsync());
            httpRes = await _httpClient.DeleteAsync($"api/tracenotetypes/{type.TraceNoteTypeId}");
            var typeDelete = await httpRes.Content.ReadFromJsonAsync<Result<TraceNoteTypeDto>>();
            Assert.IsFalse(httpRes.IsSuccessStatusCode);
            Assert.IsFalse(typeDelete!.IsValid);

            // Delete
            httpRes = await _httpClient.DeleteAsync($"api/notes/tracenotes/{traceNoteId}");
            Assert.IsTrue(httpRes.IsSuccessStatusCode, await httpRes.Content.ReadAsStringAsync());
            fromA = await GetTraceNotesAsync(noteA.NoteId);
            Assert.IsEmpty(fromA.TraceNotesTo);

            // Deleting a note deletes its trace notes
            httpRes = await _httpClient.PostAsJsonAsync("api/notes/tracenotes", new TraceNoteDto { FromId = noteB.NoteId, ToId = noteA.NoteId, Weight = 1 });
            Assert.IsTrue(httpRes.IsSuccessStatusCode, await httpRes.Content.ReadAsStringAsync());
            httpRes = await _httpClient.DeleteAsync($"api/notes/{noteA.NoteId}");
            Assert.IsTrue(httpRes.IsSuccessStatusCode, await httpRes.Content.ReadAsStringAsync());
            fromB = await GetTraceNotesAsync(noteB.NoteId);
            Assert.IsEmpty(fromB.TraceNotesTo);
        }
        finally
        {
            await _httpClient.DeleteAsync($"api/notes/{noteA.NoteId}");
            await _httpClient.DeleteAsync($"api/notes/{noteB.NoteId}");
            await _httpClient.DeleteAsync($"api/tracenotetypes/{type.TraceNoteTypeId}");
        }
    }

    private static async Task<NoteTraceNotesDto> GetTraceNotesAsync(Guid noteId)
    {
        var httpRes = await _httpClient.GetAsync($"api/notes/{noteId}/tracenotes");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTraceNotesDto>>();
        Assert.IsTrue(httpRes.IsSuccessStatusCode, res?.ErrorMessage);
        return res!.Entity!;
    }

    private static async Task<NoteDto> CreateNoteAsync(string topic, string description = "Created by NotesInProcessTests.", FolderInfoDto? folder = null)
    {
        NoteDto note = new()
        {
            NoteId = Guid.Empty,
            Topic = topic,
            Description = description,
            FolderId = (folder ?? _folder).FolderId,
            // MVC validates this nested complex property recursively, and NoteDto.FolderDto's getter
            // lazily defaults to an empty FolderDto if left unset - which then fails its own
            // [Required] Name validation. Provide the matching folder to avoid that.
            FolderDto = folder ?? _folder,
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
