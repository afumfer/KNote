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
/// AiAssistantController (api/aiassistant): configured providers, the streamed chat turn (server-sent events,
/// with the KNote tools) and the user's sessions. The model is a ScriptedAiChatClientProvider, never a real one.
/// </summary>
[TestClass]
public class AiAssistantInProcessTests
{
    private const string FakeAlias = "Fake provider";
    private const string FakeApiKey = "test-secret-api-key";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _httpClient = null!;
    private static readonly ScriptedAiChatClientProvider _chatClients = new();

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _httpClient) = await InProcessTestHost.CreateAuthenticatedClientAsync(factory =>
        {
            // The first provider of appsettings.json is replaced by this one, which becomes the default.
            factory.AppConfiguration["ai:providers:0:alias"] = FakeAlias;
            factory.AppConfiguration["ai:providers:0:provider"] = EnumAiProvider.OpenAI;
            factory.AppConfiguration["ai:providers:0:model"] = "fake-model";
            factory.AppConfiguration["ai:providers:0:apiKey"] = FakeApiKey;
            factory.TestServices = services => services.AddSingleton<IAiChatClientProvider>(_chatClients);
        });
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task GetProviders_ReturnsTheConfiguredOnes_WithoutTheirKeys()
    {
        var httpRes = await _httpClient.GetAsync("api/aiassistant/providers");
        var body = await httpRes.Content.ReadAsStringAsync();
        var res = JsonSerializer.Deserialize<Result<List<AiProviderInfoDto>>>(body, _json);

        Assert.IsTrue(httpRes.IsSuccessStatusCode, body);
        var fake = res!.Entity.Single(p => p.Alias == FakeAlias);
        Assert.IsTrue(fake.IsDefault);
        Assert.AreEqual("fake-model", fake.Model);
        Assert.IsFalse(body.Contains(FakeApiKey), "The API keys never leave the server.");
    }

    [TestMethod]
    public async Task Chat_StreamsTheAnswer_AndCompletesTheTurnWithItsUsage()
    {
        _chatClients.Script = () =>
        [
            [
                new ChatResponseUpdate(ChatRole.Assistant, "Hello"),
                new ChatResponseUpdate(ChatRole.Assistant, ", world"),
                new ChatResponseUpdate { Role = ChatRole.Assistant, FinishReason = ChatFinishReason.Stop,
                    Contents = [new UsageContent(new UsageDetails { InputTokenCount = 3, OutputTokenCount = 2, TotalTokenCount = 5 })] }
            ]
        ];

        var events = await ChatAsync(new AiChatRequestDto
        {
            Prompt = "Say hello",
            History = [new AiChatTurnDto { Prompt = "Earlier question", Answer = "Earlier answer" }]
        });

        var deltas = string.Concat(events.Where(e => e.Type == AiChatStreamEventTypes.Delta).Select(e => e.Text));
        Assert.AreEqual("Hello, world", deltas);
        var completed = events.Last();
        Assert.AreEqual(AiChatStreamEventTypes.Completed, completed.Type);
        Assert.AreEqual("Say hello", completed.Turn!.Prompt);
        Assert.AreEqual("Hello, world", completed.Turn.Answer);
        Assert.AreEqual(5, completed.Turn.TotalTokens);
        Assert.IsFalse(completed.Turn.TokensEstimated);
        Assert.IsFalse(completed.Turn.Truncated);
    }

    [TestMethod]
    public async Task Chat_CreateTaskTool_SavesTheNote_AndTellsTheClient()
    {
        _chatClients.Script = () =>
        [
            [
                new ChatResponseUpdate(ChatRole.Assistant,
                    [new FunctionCallContent("call-1", "create_task", new Dictionary<string, object?>
                    {
                        ["topic"] = "Buy milk",
                        ["description"] = "Two litres"
                    })])
            ],
            [new ChatResponseUpdate(ChatRole.Assistant, "Done, I noted it down.")]
        ];

        var events = await ChatAsync(new AiChatRequestDto { ProviderAlias = FakeAlias, Prompt = "Remind me to buy milk" });

        CollectionAssert.Contains(events.Select(e => e.Type).ToList(), AiChatStreamEventTypes.Tool);
        var created = events.Single(e => e.Type == AiChatStreamEventTypes.NoteCreated);
        Assert.AreEqual("Buy milk", created.Text);
        Assert.AreEqual("Done, I noted it down.", events.Last().Turn!.Answer);

        var note = await _httpClient.GetFromJsonAsync<Result<NoteDto>>($"api/notes/{created.NoteId}");
        Assert.AreEqual("Buy milk", note!.Entity.Topic);
        Assert.AreEqual(created.NoteNumber, note.Entity.NoteNumber);
    }

    [TestMethod]
    public async Task Chat_ProviderFailsMidStream_EndsWithAnErrorEvent()
    {
        _chatClients.Script = () => throw new InvalidOperationException("The provider is down.");

        var events = await ChatAsync(new AiChatRequestDto { Prompt = "Anything" });

        Assert.AreEqual(AiChatStreamEventTypes.Error, events.Last().Type);
        StringAssert.Contains(events.Last().Text, "The provider is down.");
    }

    [TestMethod]
    public async Task Chat_UnknownProviderOrEmptyPrompt_IsABadRequestWithAResult()
    {
        var unknown = await _httpClient.PostAsJsonAsync("api/aiassistant/chat", new AiChatRequestDto { ProviderAlias = "Nope", Prompt = "Hi" });
        var empty = await _httpClient.PostAsJsonAsync("api/aiassistant/chat", new AiChatRequestDto { Prompt = " " });

        Assert.AreEqual(HttpStatusCode.BadRequest, unknown.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, empty.StatusCode);
        StringAssert.Contains((await unknown.Content.ReadFromJsonAsync<Result<AiChatTurnDto>>())!.ErrorMessage, "Nope");
        Assert.IsFalse((await empty.Content.ReadFromJsonAsync<Result<AiChatTurnDto>>())!.IsValid);
    }

    [TestMethod]
    public async Task Sessions_SaveListAndGet()
    {
        var session = new AiChatSessionDto
        {
            Provider = EnumAiProvider.OpenAI,
            Model = "fake-model",
            Turns = [new AiChatTurnDto { Prompt = "What is KNote?", Answer = "A notes manager.", TotalTokens = 9 }]
        };

        var saveRes = await (await _httpClient.PostAsJsonAsync("api/aiassistant/sessions", session))
            .Content.ReadFromJsonAsync<Result<AiChatSessionDto>>();
        var listRes = await _httpClient.GetFromJsonAsync<Result<List<AiChatSessionInfoDto>>>("api/aiassistant/sessions");
        var getRes = await _httpClient.GetFromJsonAsync<Result<AiChatSessionDto>>($"api/aiassistant/sessions/{saveRes!.Entity.NoteId}");

        Assert.IsTrue(saveRes.IsValid, saveRes.ErrorMessage);
        Assert.IsTrue(listRes!.Entity.Any(s => s.NoteId == saveRes.Entity.NoteId && s.Topic == "What is KNote?"));
        Assert.AreEqual("fake-model", getRes!.Entity.Model);
        Assert.AreEqual("A notes manager.", getRes.Entity.Turns.Single().Answer);
    }

    [TestMethod]
    public async Task AGuest_IsForbidden()
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

        var res = await guestClient.GetAsync("api/aiassistant/sessions");

        Assert.AreEqual(HttpStatusCode.Forbidden, res.StatusCode);
    }

    private static async Task<List<AiChatStreamEventDto>> ChatAsync(AiChatRequestDto request)
    {
        var httpRes = await _httpClient.PostAsJsonAsync("api/aiassistant/chat", request);
        var body = await httpRes.Content.ReadAsStringAsync();

        Assert.IsTrue(httpRes.IsSuccessStatusCode, body);
        Assert.AreEqual("text/event-stream", httpRes.Content.Headers.ContentType?.MediaType);
        return ParseServerSentEvents(body);
    }

    // The "data:" line of each event, as AiChatStreamEventDto (its Type is also the SSE event type).
    private static List<AiChatStreamEventDto> ParseServerSentEvents(string body) =>
        body.ReplaceLineEndings("\n")
            .Split('\n')
            .Where(line => line.StartsWith("data:"))
            .Select(line => JsonSerializer.Deserialize<AiChatStreamEventDto>(line["data:".Length..].Trim(), _json)!)
            .ToList();
}
