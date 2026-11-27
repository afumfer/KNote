using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// api/history (the «History» tab of the web note editor, H.3): a page of the rows of a note and the version after one of them,
/// with its description read as the web app reads a note's. See FoldersInProcessTests for the general pattern.
/// </summary>
[TestClass]
public class HistoryInProcessTests
{
    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _httpClient = null!;
    private static FolderInfoDto _folder = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _httpClient) = await InProcessTestHost.CreateAuthenticatedClientAsync();
        var folders = await _httpClient.GetFromJsonAsync<Result<List<FolderInfoDto>>>("api/folders");
        _folder = folders!.Entity!.First();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task TheHistoryOfANote_AndAVersion()
    {
        var note = await PostAsync(new NoteDto
        {
            NoteId = Guid.Empty, Topic = "__TEST_HISTORY__", Description = "First text", FolderId = _folder.FolderId, FolderDto = _folder,
            CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
        });

        var page = (await _httpClient.GetFromJsonAsync<Result<NoteHistoryPageDto>>($"api/history/notes/{note.NoteId}"))!.Entity!;
        var created = page.Rows.Single();
        Assert.AreEqual("Created", created.ActionType);
        Assert.AreEqual("Web", created.Origin);

        var version = await _httpClient.GetFromJsonAsync<Result<NoteVersionViewDto>>($"api/history/notes/{note.NoteId}/versions/{created.KLogId}");
        Assert.IsTrue(version!.IsValid, version.ErrorMessage);
        Assert.AreEqual("__TEST_HISTORY__", version.Entity!.Topic);
        StringAssert.Contains(version.Entity.Description, "First text");

        var other = await _httpClient.GetAsync($"api/history/notes/{note.NoteId}/versions/{Guid.NewGuid()}");
        Assert.IsFalse(other.IsSuccessStatusCode);
        Assert.IsFalse((await other.Content.ReadFromJsonAsync<Result<NoteVersionViewDto>>())!.IsValid);
    }

    [TestMethod]
    public async Task CompareAndRestore()
    {
        var note = await PostAsync(new NoteDto
        {
            NoteId = Guid.Empty, Topic = "__TEST_HISTORY_RESTORE__", Description = "First text", FolderId = _folder.FolderId, FolderDto = _folder,
            CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
        });
        note.Description = "Second text";
        note = await PutAsync(note);

        var page = (await _httpClient.GetFromJsonAsync<Result<NoteHistoryPageDto>>($"api/history/notes/{note.NoteId}"))!.Entity!;
        // The same user and origin within ten minutes: one row, the creation and the edit.
        var created = page.Rows.Single();

        var comparison = (await _httpClient.GetFromJsonAsync<Result<NoteComparisonDto>>(
            $"api/history/notes/{note.NoteId}/compare/{created.KLogId}?withCurrentNote=false"))!;
        Assert.IsTrue(comparison.IsValid, comparison.ErrorMessage);
        Assert.AreEqual(EnumComparisonLine.Added, comparison.Entity!.Text.Lines.Single().Kind);
        Assert.AreEqual("Second text", comparison.Entity.Text.Lines.Single().Text);

        // Nothing to restore: the only row is the note now.
        var restore = await _httpClient.PostAsync($"api/history/notes/{note.NoteId}/versions/{created.KLogId}/restore", null);
        Assert.IsFalse(restore.IsSuccessStatusCode);
        StringAssert.Contains((await restore.Content.ReadFromJsonAsync<Result<NoteRestoreResultDto>>())!.ErrorMessage, "nothing to restore");
    }

    [TestMethod]
    public async Task ADeletedNote_IsRecovered()
    {
        var note = await PostAsync(new NoteDto
        {
            NoteId = Guid.Empty, Topic = "__TEST_HISTORY_RECOVER__", Description = "To recover", FolderId = _folder.FolderId, FolderDto = _folder,
            CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue((await _httpClient.DeleteAsync($"api/notes/{note.NoteId}")).IsSuccessStatusCode);

        var deleted = (await _httpClient.GetFromJsonAsync<Result<List<DeletedNoteDto>>>("api/history/deleted"))!.Entity!;
        var mine = deleted.Single(d => d.NoteId == note.NoteId);
        Assert.AreEqual("__TEST_HISTORY_RECOVER__", mine.Topic);

        var recover = await _httpClient.PostAsync($"api/history/deleted/{note.NoteId}/recover", null);
        Assert.IsTrue(recover.IsSuccessStatusCode, await recover.Content.ReadAsStringAsync());
        var result = (await recover.Content.ReadFromJsonAsync<Result<NoteRecoverResultDto>>())!.Entity!;
        Assert.AreEqual(note.NoteNumber, result.NoteNumber);
        var recovered = await _httpClient.GetFromJsonAsync<Result<NoteDto>>($"api/notes/{note.NoteId}");
        Assert.AreEqual("To recover", recovered!.Entity!.Description);

        Assert.IsTrue((await _httpClient.DeleteAsync($"api/notes/{note.NoteId}")).IsSuccessStatusCode);
        Assert.IsTrue((await _httpClient.DeleteAsync($"api/history/deleted/{note.NoteId}")).IsSuccessStatusCode, "remove permanently (Admin)");
        deleted = (await _httpClient.GetFromJsonAsync<Result<List<DeletedNoteDto>>>("api/history/deleted"))!.Entity!;
        Assert.IsFalse(deleted.Any(d => d.NoteId == note.NoteId));
    }

    [TestMethod]
    public async Task TheRetention_IsReadAndValidated()
    {
        var info = (await _httpClient.GetFromJsonAsync<Result<HistoryRetentionInfoDto>>("api/history/retention"))!.Entity!;
        Assert.IsNotNull(info.Retention);

        var wrong = await _httpClient.PutAsJsonAsync("api/history/retention", new HistoryRetentionDto { KeepAllMonths = 6, DailyUntilMonths = 2 });
        Assert.IsFalse(wrong.IsSuccessStatusCode);

        var saved = await _httpClient.PutAsJsonAsync("api/history/retention", info.Retention);
        Assert.IsTrue(saved.IsSuccessStatusCode, await saved.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task TheChangesOfTheRepository()
    {
        var note = await PostAsync(new NoteDto
        {
            NoteId = Guid.Empty, Topic = "__TEST_HISTORY_CHANGES__", Description = "A change", FolderId = _folder.FolderId, FolderDto = _folder,
            CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
        });

        var response = await _httpClient.PostAsJsonAsync("api/history/changes", new RepositoryChangesQueryDto { EntityName = HistoryEntities.Note });
        Assert.IsTrue(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        var page = (await response.Content.ReadFromJsonAsync<Result<RepositoryChangesPageDto>>())!.Entity!;
        var row = page.Rows.First(r => r.EntityId == note.NoteId);
        Assert.AreEqual("Created", row.ActionType);
        Assert.IsTrue(page.NoteExists(row));

        var users = await _httpClient.PostAsJsonAsync("api/history/changes/users", new RepositoryChangesQueryDto());
        Assert.IsTrue((await users.Content.ReadFromJsonAsync<Result<List<string>>>())!.Entity!.Contains(row.UserName));
    }

    private static async Task<NoteDto> PutAsync(NoteDto note)
    {
        var httpRes = await _httpClient.PutAsJsonAsync("api/notes", note);
        if (!httpRes.IsSuccessStatusCode)
            Assert.Fail($"Status: {httpRes.StatusCode}, Body: {await httpRes.Content.ReadAsStringAsync()}");
        return (await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>())!.Entity!;
    }

    private static async Task<NoteDto> PostAsync(NoteDto note)
    {
        var httpRes = await _httpClient.PostAsJsonAsync("api/notes", note);
        if (!httpRes.IsSuccessStatusCode)
            Assert.Fail($"Status: {httpRes.StatusCode}, Body: {await httpRes.Content.ReadAsStringAsync()}");
        return (await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>())!.Entity!;
    }
}
