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
