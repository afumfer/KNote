using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The Web's public Home: the folders directly inside Home (api/folders/homechildren) and their notes
/// (api/notes/homenotes?folderId=...), both read without signing in; any other folder is refused.
/// </summary>
[TestClass]
public class HomeFoldersInProcessTests
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
    public async Task HomeFolders_AndTheirNotes_AreReadWithoutSigningIn()
    {
        var home = await GetHomeAsync();
        var child = await PostFolderAsync("__TEST HOME CHILD__", home.FolderId);
        var grandchild = await PostFolderAsync("__TEST HOME GRANDCHILD__", child.FolderId);
        try
        {
            using var anonymous = _factory.CreateClient();

            var foldersRes = await anonymous.GetAsync("api/folders/homechildren");
            var folders = await foldersRes.Content.ReadFromJsonAsync<Result<List<FolderInfoDto>>>();
            Assert.IsTrue(foldersRes.IsSuccessStatusCode);
            Assert.IsTrue(folders!.Entity!.Any(f => f.FolderId == child.FolderId));
            Assert.IsFalse(folders.Entity!.Any(f => f.FolderId == grandchild.FolderId));

            var notesRes = await anonymous.GetAsync($"api/notes/homenotes?folderId={child.FolderId}");
            var notes = await notesRes.Content.ReadFromJsonAsync<Result<List<NoteInfoDto>>>();
            Assert.IsTrue(notesRes.IsSuccessStatusCode);
            Assert.IsNotNull(notes?.Entity);

            var refusedRes = await anonymous.GetAsync($"api/notes/homenotes?folderId={grandchild.FolderId}");
            var refused = await refusedRes.Content.ReadFromJsonAsync<Result<List<NoteInfoDto>>>();
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, refusedRes.StatusCode);
            Assert.IsFalse(refused!.IsValid);
        }
        finally
        {
            await _httpClient.DeleteAsync($"api/folders/{grandchild.FolderId}");
            await _httpClient.DeleteAsync($"api/folders/{child.FolderId}");
        }
    }

    private static async Task<FolderInfoDto> GetHomeAsync()
    {
        var folders = await (await _httpClient.GetAsync("api/folders")).Content.ReadFromJsonAsync<Result<List<FolderInfoDto>>>();
        return folders!.Entity!.First(f => f.FolderNumber == 1);
    }

    private static async Task<FolderDto> PostFolderAsync(string name, Guid parentId)
    {
        var httpRes = await _httpClient.PostAsJsonAsync("api/folders", new FolderDto { FolderId = Guid.Empty, Name = name, ParentId = parentId });
        var res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();
        Assert.IsTrue(res!.IsValid, res.ErrorMessage);
        return res.Entity!;
    }
}
