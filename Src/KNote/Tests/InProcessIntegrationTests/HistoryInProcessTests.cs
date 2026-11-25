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
