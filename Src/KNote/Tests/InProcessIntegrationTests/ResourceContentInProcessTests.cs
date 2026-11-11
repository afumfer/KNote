using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Ai;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// api/resources (KNote Atlas, 4.2): the model that would describe the photos, and describing one with the roles of the user.
/// The model is a ScriptedAiChatClientProvider.
/// </summary>
[TestClass]
public class ResourceContentInProcessTests
{
    private const string Password = "InProcess-Photos-Password-1!";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _admin = null!;
    private static readonly ScriptedAiChatClientProvider _chatClients = new();
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _admin) = await InProcessTestHost.CreateAuthenticatedClientAsync(factory =>
        {
            factory.AppConfiguration["RateLimits:SignInPerMinute"] = "1000";
            factory.AppConfiguration["ai:providers:0:alias"] = "Fake provider";
            factory.AppConfiguration["ai:providers:0:provider"] = EnumAiProvider.OpenAI;
            factory.AppConfiguration["ai:providers:0:model"] = "fake-model";
            factory.AppConfiguration["ai:providers:0:apiKey"] = "test-key";
            factory.TestServices = services => services.AddSingleton<IAiChatClientProvider>(_chatClients);
        });
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _admin.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task TheDescriber_IsTheModelOfThisServer()
    {
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));

        var response = await staff.GetAsync("api/resources/describer");

        var body = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(response.IsSuccessStatusCode, body);
        var describer = JsonSerializer.Deserialize<Result<ResourceDescriberDto>>(body, _json)!.Entity;
        Assert.AreEqual("fake-model", describer.Model);
        Assert.IsTrue(describer.IsPaid);
        Assert.IsTrue(describer.Available);
    }

    [TestMethod]
    public async Task APhoto_IsDescribedBySomeoneWhoEditsNotes_NotByAGuest()
    {
        var photoId = await AddPhotoAsync();
        _chatClients.Script = () => [[new ChatResponseUpdate(ChatRole.Assistant, "A blue square.")]];
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));
        using var guest = await NewClientAsync(nameof(EnumRoles.Guest));

        var forbidden = await guest.PostAsync($"api/resources/{photoId}/describe", null);
        var described = await staff.PostAsync($"api/resources/{photoId}/describe", null);

        Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
        var body = await described.Content.ReadAsStringAsync();
        Assert.IsTrue(described.IsSuccessStatusCode, body);
        Assert.AreEqual("A blue square.", JsonSerializer.Deserialize<Result<ResourceInfoDto>>(body, _json)!.Entity.Description);
        Assert.AreEqual(0, _chatClients.LastTools.Count, "No tools.");
        using var service = _factory.CreateServiceForRepository(0);
        Assert.AreEqual("fake-model", (await service.Notes.GetResourceAsync(photoId)).Entity.DescriptionModel);
    }

    // 4.4c: a document is read (by the Server's reader) and summarized with the model, by someone who edits notes.
    [TestMethod]
    public async Task ADocument_IsSummarizedBySomeoneWhoEditsNotes_NotByAGuest()
    {
        Guid documentId;
        using (var service = _factory.CreateServiceForRepository(0))
        {
            var note = (await service.Notes.HomeNotesAsync()).Entity.First();
            var saved = await service.Notes.SaveResourceAsync(new ResourceDto
            {
                NoteId = note.NoteId, Name = $"{Guid.NewGuid()}_manual.pdf", Description = "manual.pdf", FileType = "application/pdf",
                ContentArrayBytes = TestDocuments.Pdf("The manual of the QL.")
            });
            Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
            documentId = saved.Entity.ResourceId;
        }
        _chatClients.Script = () => [[new ChatResponseUpdate(ChatRole.Assistant, "The manual of the Sinclair QL.")]];
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));
        using var guest = await NewClientAsync(nameof(EnumRoles.Guest));

        var forbidden = await guest.PostAsync($"api/resources/{documentId}/summarize", null);
        var summarized = await staff.PostAsync($"api/resources/{documentId}/summarize", null);

        Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
        var body = await summarized.Content.ReadAsStringAsync();
        Assert.IsTrue(summarized.IsSuccessStatusCode, body);
        Assert.AreEqual("The manual of the Sinclair QL.", JsonSerializer.Deserialize<Result<ResourceInfoDto>>(body, _json)!.Entity.Description);
        Assert.AreEqual(0, _chatClients.LastTools.Count, "No tools.");
        using var check = _factory.CreateServiceForRepository(0);
        Assert.AreEqual(EnumExtractedState.Complete, (await check.ResourceContent.GetDocumentTextAsync(documentId)).Entity.State, "read first");
    }

    // «Process pending resources» (4.3): only an Admin runs it, in the Server's queue; its progress until it ends, and what is
    // pending afterwards.
    [TestMethod]
    public async Task ProcessPendingResources_IsAnAdminRun_InTheServersQueue()
    {
        var photoId = await AddPhotoAsync();
        _chatClients.Script = () => Enumerable.Range(0, 100).Select(_ => new List<ChatResponseUpdate> { new(ChatRole.Assistant, "A blue square.") }).ToList();
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));
        var options = new ResourceProcessingOptionsDto { Photos = true, PersonDescriptions = EnumPersonDescriptions.Skip };

        var status = await staff.GetAsync("api/resources/status");
        var forbidden = await staff.PostAsJsonAsync("api/resources/process", options);
        var started = await _admin.PostAsJsonAsync("api/resources/process", options);

        Assert.IsTrue(status.IsSuccessStatusCode, await status.Content.ReadAsStringAsync());
        Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
        var body = await started.Content.ReadAsStringAsync();
        Assert.IsTrue(started.IsSuccessStatusCode, body);

        ResourceProcessingProgressDto? progress = null;
        for (var i = 0; i < 200 && progress?.IsFinished != true; i++)
        {
            await Task.Delay(50);
            var res = await _admin.GetFromJsonAsync<Result<ResourceProcessingProgressDto>>("api/resources/process/progress", _json);
            progress = res!.Entity;
        }
        Assert.AreEqual(EnumResourceProcessingPhase.Completed, progress!.Phase, progress.Message);
        Assert.IsTrue(progress.PhotosDescribed >= 1);
        Assert.AreEqual("fake-model", progress.Model);
        using var service = _factory.CreateServiceForRepository(0);
        Assert.AreEqual("A blue square.", (await service.Notes.GetResourceAsync(photoId)).Entity.Description);
        Assert.AreEqual(0, (await service.ResourceContent.GetProcessingStatusAsync()).Entity.PendingPhotos);
    }

    private static async Task<Guid> AddPhotoAsync()
    {
        using var service = _factory.CreateServiceForRepository(0);
        var note = (await service.Notes.HomeNotesAsync()).Entity.First();
        var saved = await service.Notes.SaveResourceAsync(new ResourceDto
        {
            NoteId = note.NoteId, Name = $"{Guid.NewGuid()}_square.png", Description = "square.png", FileType = "image/png", ContentArrayBytes = Png()
        });
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity.ResourceId;
    }

    private static byte[] Png()
    {
        using var bitmap = new SkiaSharp.SKBitmap(64, 48);
        bitmap.Erase(SkiaSharp.SKColors.Blue);
        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static async Task<HttpClient> NewClientAsync(string role)
    {
        var id = Guid.NewGuid();
        var userName = $"pho-{id:N}"[..24];
        using (var service = _factory.CreateServiceForRepository(0))
        {
            var res = await service.Users.CreateAsync(new UserRegisterDto
            {
                UserName = userName,
                EMail = $"{id:N}@knote.tests",
                FullName = $"Photos user {id:N}"[..24],
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
