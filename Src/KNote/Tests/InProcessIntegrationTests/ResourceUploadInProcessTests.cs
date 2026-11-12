using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The resources of up to 200 MB of the Web (KNote Atlas, stage 4, 4.4b): POST api/notes/{id}/resources/upload (a form, as the
/// browser sends it from the disk; Staff+), and reading a document (POST api/resources/{id}/read, Staff+) and its text
/// (GET api/resources/{id}/text, anyone signed in, its beginning only).
/// </summary>
[TestClass]
public class ResourceUploadInProcessTests
{
    private const string Password = "InProcess-Upload-Password-1!";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _admin = null!;
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _admin) = await InProcessTestHost.CreateAuthenticatedClientAsync(factory =>
            factory.AppConfiguration["RateLimits:SignInPerMinute"] = "1000");
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _admin.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task ADocument_IsUploadedAndRead_BySomeoneWhoEditsNotes_AndItsTextIsForEveryone()
    {
        var noteId = await HomeNoteIdAsync();
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));
        using var guest = await NewClientAsync(nameof(EnumRoles.Guest));
        var content = TestDocuments.Pdf("The manual of the QL.", "Chapter 2.");

        var forbidden = await guest.PostAsync($"api/notes/{noteId}/resources/upload", Form(content, "manual.pdf"));
        var uploaded = await staff.PostAsync($"api/notes/{noteId}/resources/upload", Form(content, "manual.pdf"));

        Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
        var body = await uploaded.Content.ReadAsStringAsync();
        Assert.IsTrue(uploaded.IsSuccessStatusCode, body);
        var resource = JsonSerializer.Deserialize<Result<ResourceInfoDto>>(body, _json)!.Entity;
        Assert.AreEqual(("manual.pdf", "The QL manual", noteId), (ResourceInfoDto.FileNameOf(resource.Name), resource.Description, resource.NoteId));
        Assert.IsTrue(resource.DescriptionByUser, "the lock that came with the form");
        var file = Directory.GetFiles(_factory.ResourcesFolder, "*_manual.pdf", SearchOption.AllDirectories).Single(f => f.Contains(resource.Name));
        CollectionAssert.AreEqual(content, File.ReadAllBytes(file));

        var guestRead = await guest.PostAsync($"api/resources/{resource.ResourceId}/read", null);
        var read = await staff.PostAsync($"api/resources/{resource.ResourceId}/read", null);
        Assert.AreEqual(HttpStatusCode.Forbidden, guestRead.StatusCode);
        var readBody = await read.Content.ReadAsStringAsync();
        Assert.IsTrue(read.IsSuccessStatusCode, readBody);
        Assert.AreEqual((EnumExtractedState.Complete, 2), (JsonSerializer.Deserialize<Result<DocumentReadDto>>(readBody, _json)!.Entity.State,
            JsonSerializer.Deserialize<Result<DocumentReadDto>>(readBody, _json)!.Entity.Pages));

        var text = await guest.GetFromJsonAsync<Result<ResourceTextDto>>($"api/resources/{resource.ResourceId}/text", _json);
        Assert.IsTrue(text!.IsValid, text.ErrorMessage);
        StringAssert.Contains(text.Entity.Text, "The manual of the QL.");
        Assert.AreEqual((2, 2), (text.Entity.PagesShown, text.Entity.PagesCount));
    }

    [TestMethod]
    public async Task AnEmptyFile_IsRefused_WithItsReason()
    {
        var noteId = await HomeNoteIdAsync();

        var refused = await _admin.PostAsync($"api/notes/{noteId}/resources/upload", Form([], "empty.pdf"));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);
        var result = JsonSerializer.Deserialize<Result<ResourceInfoDto>>(await refused.Content.ReadAsStringAsync(), _json)!;
        StringAssert.Contains(result.ErrorMessage, "empty");
    }

    // The form of js/kntUpload.js.
    private static MultipartFormDataContent Form(byte[] content, string fileName)
    {
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        return new MultipartFormDataContent
        {
            { new StringContent("The QL manual"), "description" },
            { new StringContent(fileName), "loadedDescription" },
            { new StringContent("true"), "descriptionByUser" },
            { new StringContent("1"), "order" },
            { file, "file", fileName }
        };
    }

    private static async Task<Guid> HomeNoteIdAsync()
    {
        using var service = _factory.CreateServiceForRepository(0);
        return (await service.Notes.HomeNotesAsync()).Entity.First().NoteId;
    }

    private static async Task<HttpClient> NewClientAsync(string role)
    {
        var id = Guid.NewGuid();
        var userName = $"upl-{id:N}"[..24];
        using (var service = _factory.CreateServiceForRepository(0))
        {
            var res = await service.Users.CreateAsync(new UserRegisterDto
            {
                UserName = userName,
                EMail = $"{id:N}@knote.tests",
                FullName = $"Upload user {id:N}"[..24],
                RoleDefinition = role,
                Password = Password
            });
            Assert.IsTrue(res.IsValid, res.ErrorMessage);
        }

        var client = _factory.CreateClient();
        var token = await (await client.PostAsJsonAsync("api/users/login", new UserCredentialsDto { UserName = userName, Password = Password }))
            .Content.ReadFromJsonAsync<UserTokenDto>();
        Assert.IsTrue(token!.success, token.error);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.token);
        return client;
    }
}
