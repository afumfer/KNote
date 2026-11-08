using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Ai;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// AtlasController (api/atlas) and the context of the AI assistant of the Web (KNote Atlas, delivery 1.1): the windows,
/// the creation of their notes (ProjectManager and Admin), what the AI receives, that a Staff user cannot save them with
/// the API of notes (a rule of the Service, the Server has the authorization of commands off), and that the chat turn
/// sends the system message put together. The model is a ScriptedAiChatClientProvider.
/// </summary>
[TestClass]
public class AtlasInProcessTests
{
    private const string Password = "InProcess-Atlas-Password-1!";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _admin = null!;
    private static readonly ScriptedAiChatClientProvider _chatClients = new();
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _admin) = await InProcessTestHost.CreateAuthenticatedClientAsync(factory =>
        {
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
    public async Task TheProfile_IsCreatedOnce_AndAStaffUserCannotSaveItWithTheApiOfNotes()
    {
        var created = await ReadAsync<NoteDto>(await _admin.PostAsync("api/atlas/profile", null));
        var again = await ReadAsync<NoteDto>(await _admin.PostAsync("api/atlas/profile", null));
        var info = await ReadAsync<AtlasProfileInfoDto>(await _admin.GetAsync("api/atlas/profile"));

        Assert.AreEqual(created.NoteId, again.NoteId);
        Assert.AreEqual(created.NoteNumber, info.Note.NoteNumber);

        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));
        Assert.AreEqual(HttpStatusCode.Forbidden, (await staff.PostAsync("api/atlas/profile", null)).StatusCode);
        var note = await ReadAsync<NoteDto>(await staff.GetAsync($"api/notes/{created.NoteId}"));
        note.Description = "Changed by a Staff user.";
        var put = await staff.PutAsJsonAsync("api/notes", note);
        var body = await put.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.BadRequest, put.StatusCode, body);
        StringAssert.Contains(body, "Only a ProjectManager or an Admin");
        Assert.IsTrue((await staff.GetAsync("api/atlas/profile")).IsSuccessStatusCode, "the window is for any user");
    }

    [TestMethod]
    public async Task TheNoteOfACitation_IsNotFoundForTheSystemNotesAndThoseThatDoNotExist()
    {
        var glossary = await ReadAsync<NoteDto>(await _admin.PostAsync("api/atlas/glossary", null));
        NoteDto note;
        using (var service = _factory.CreateServiceForRepository(0))
        {
            note = (await service.Notes.GetAsync((await service.Notes.HomeNotesAsync()).Entity.First().NoteId)).Entity;
        }

        using var guest = await NewClientAsync(nameof(EnumRoles.Guest));
        var cited = await ReadAsync<NoteMinimalDto>(await guest.GetAsync($"api/notes/cited/{note.NoteNumber}"));
        var system = await guest.GetAsync($"api/notes/cited/{glossary.NoteNumber}");
        var missing = await guest.GetAsync("api/notes/cited/999999");

        Assert.AreEqual(note.NoteId, cited.NoteId);
        Assert.IsTrue(system.IsSuccessStatusCode);
        Assert.IsNull((await system.Content.ReadFromJsonAsync<Result<NoteMinimalDto>>(_json))!.Entity);
        Assert.IsTrue(missing.IsSuccessStatusCode);
        Assert.IsNull((await missing.Content.ReadFromJsonAsync<Result<NoteMinimalDto>>(_json))!.Entity);
    }

    [TestMethod]
    public async Task Compose_AndTheChat_HaveTheProfileAndTheGlossaryTerms()
    {
        var profile = await ReadAsync<NoteDto>(await _admin.PostAsync("api/atlas/profile", null));
        var glossary = await ReadAsync<NoteDto>(await _admin.PostAsync("api/atlas/glossary", null));
        using (var service = _factory.CreateServiceForRepository(0))
        {
            await SaveDescriptionAsync(service, profile.NoteId, "## Purpose\n\nA collection of retro computers.");
            await SaveDescriptionAsync(service, glossary.NoteId, GlossaryTable.Template + "| CPC | Amstrad Colour Personal Computer | |\n");
        }

        var composed = await ReadAsync<AiComposedPromptDto>(await _admin.PostAsJsonAsync("api/atlas/compose",
            new AiComposeRequestDto { Texts = { "Is my CPC working?" } }));
        Assert.AreEqual("CPC", composed.GlossaryTerms.Single().Term);
        StringAssert.Contains(composed.Text, "A collection of retro computers.");

        _chatClients.Script = () =>
        [
            [
                new ChatResponseUpdate(ChatRole.Assistant, "It is."),
                new ChatResponseUpdate { Role = ChatRole.Assistant, FinishReason = ChatFinishReason.Stop }
            ]
        ];
        var chat = await _admin.PostAsJsonAsync("api/aiassistant/chat", new AiChatRequestDto { Prompt = "Is my CPC working?" });
        await chat.Content.ReadAsStringAsync();

        var system = _chatClients.LastMessages.First();
        Assert.AreEqual(ChatRole.System, system.Role);
        StringAssert.StartsWith(system.Text, "# KNote rules");
        StringAssert.Contains(system.Text, "with the role Admin");
        StringAssert.Contains(system.Text, "# Repository profile\n\n## Purpose\n\nA collection of retro computers.");
        StringAssert.Contains(system.Text, "- **CPC**: Amstrad Colour Personal Computer");
    }

    [TestMethod]
    public async Task TaskInstructions_AreCustomizedByAProjectManager_AndListedForAnyUser()
    {
        using var manager = await NewClientAsync(nameof(EnumRoles.ProjectManager));
        var note = await ReadAsync<NoteDto>(await manager.PostAsync("api/atlas/tasks/Assistant/customize", null));
        using var guest = await NewClientAsync(nameof(EnumRoles.Guest));

        var tasks = await ReadAsync<List<AtlasTaskPromptStatusDto>>(await guest.GetAsync("api/atlas/tasks"));

        Assert.AreEqual(EnumAtlasTaskPromptState.Added, tasks.Single().State);
        Assert.AreEqual(note.NoteId, tasks.Single().Note.NoteId);
        Assert.AreEqual(HttpStatusCode.Forbidden, (await guest.PostAsJsonAsync("api/atlas/compose", new AiComposeRequestDto())).StatusCode);
    }

    private static async Task SaveDescriptionAsync(KntService service, Guid noteId, string description)
    {
        var note = (await service.Notes.GetAsync(noteId)).Entity;
        note.Description = description;
        Assert.IsTrue((await service.Notes.SaveAsync(note)).IsValid);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(response.IsSuccessStatusCode, body);
        return JsonSerializer.Deserialize<Result<T>>(body, _json)!.Entity;
    }

    private static async Task<HttpClient> NewClientAsync(string role)
    {
        var id = Guid.NewGuid();
        var userName = $"atl-{id:N}"[..24];
        using (var service = _factory.CreateServiceForRepository(0))
        {
            var res = await service.Users.CreateAsync(new UserRegisterDto
            {
                UserName = userName,
                EMail = $"{id:N}@knote.tests",
                FullName = $"Atlas user {id:N}"[..24],
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
