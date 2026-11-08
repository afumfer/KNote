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
/// "Search the Internet" of the Web assistant (KNote Atlas, chapter 23): which providers offer it, the tools and the rules a
/// turn gets only when the user turned it on, and the sources of the provider's citations at the end of the answer. The model
/// is a ScriptedAiChatClientProvider: nothing reaches a real provider or ollama.com.
/// </summary>
[TestClass]
public class WebSearchInProcessTests
{
    private const string PaidAlias = "Paid search";
    private const string OllamaWithKeyAlias = "Ollama with web search";
    private const string OllamaWithoutKeyAlias = "Ollama without web search";
    private const string WebSearchKey = "ollama-web-search-secret";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _httpClient = null!;
    private static readonly ScriptedAiChatClientProvider _chatClients = new();
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _httpClient) = await InProcessTestHost.CreateAuthenticatedClientAsync(factory =>
        {
            // The first three providers of appsettings.json are replaced by these.
            Provider(factory, 0, PaidAlias, EnumAiProvider.OpenAI, "fake-model");
            Provider(factory, 1, OllamaWithKeyAlias, EnumAiProvider.Ollama, "fake-local-model");
            factory.AppConfiguration["ai:providers:1:webSearchApiKey"] = WebSearchKey;
            Provider(factory, 2, OllamaWithoutKeyAlias, EnumAiProvider.Ollama, "fake-local-model");
            factory.TestServices = services => services.AddSingleton<IAiChatClientProvider>(_chatClients);
        });
    }

    private static void Provider(KNoteWebApplicationFactory factory, int index, string alias, string provider, string model)
    {
        factory.AppConfiguration[$"ai:providers:{index}:alias"] = alias;
        factory.AppConfiguration[$"ai:providers:{index}:provider"] = provider;
        factory.AppConfiguration[$"ai:providers:{index}:model"] = model;
        factory.AppConfiguration[$"ai:providers:{index}:kind"] = EnumAiProviderKind.Chat;
        factory.AppConfiguration[$"ai:providers:{index}:host"] = "http://127.0.0.1:9";
        factory.AppConfiguration[$"ai:providers:{index}:apiKey"] = "fake-key";
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task Providers_SayWhichOnesSearch_WithoutTheKey()
    {
        var httpRes = await _httpClient.GetAsync("api/aiassistant/providers");
        var body = await httpRes.Content.ReadAsStringAsync();
        var providers = JsonSerializer.Deserialize<Result<List<AiProviderInfoDto>>>(body, _json)!.Entity;

        Assert.IsTrue(providers.Single(p => p.Alias == PaidAlias).SupportsWebSearch);
        Assert.IsTrue(providers.Single(p => p.Alias == OllamaWithKeyAlias).SupportsWebSearch);
        Assert.IsFalse(providers.Single(p => p.Alias == OllamaWithoutKeyAlias).SupportsWebSearch);
        Assert.DoesNotContain(WebSearchKey, body, "The keys never leave the server.");
    }

    [TestMethod]
    public async Task Chat_WithTheSearchOn_ThePaidProviderSearchesItself_AndTheModelGetsTheRules()
    {
        await ChatAsync(PaidAlias, webSearch: true);

        Assert.AreEqual(1, _chatClients.LastTools.OfType<HostedWebSearchTool>().Count());
        Assert.IsFalse(_chatClients.LastTools.Any(t => t.Name == AiWebSearchRules.WebFetchName), "KNote's own tools are only for Ollama.");
        StringAssert.Contains(SystemMessage(), AiWebSearchRules.SystemRules);
    }

    [TestMethod]
    public async Task Chat_WithTheSearchOff_NoProviderSearches()
    {
        await ChatAsync(PaidAlias, webSearch: false);
        Assert.IsFalse(_chatClients.LastTools.Any(IsWebSearchTool));
        Assert.IsFalse(SystemMessage().Contains(AiWebSearchRules.SystemRules));
        StringAssert.Contains(SystemMessage(), AiWebSearchRules.SystemRulesOff, "It says so, instead of answering from memory as if it had searched.");

        await ChatAsync(OllamaWithKeyAlias, webSearch: false);
        Assert.IsFalse(_chatClients.LastTools.Any(IsWebSearchTool));
    }

    [TestMethod]
    public async Task Chat_WithOllama_KnoteSearches_OnlyWithItsKey()
    {
        await ChatAsync(OllamaWithKeyAlias, webSearch: true);
        CollectionAssert.IsSubsetOf(new[] { AiWebSearchRules.WebSearchName, AiWebSearchRules.WebFetchName },
            _chatClients.LastTools.OfType<AIFunction>().Select(t => t.Name).ToList());
        Assert.IsFalse(_chatClients.LastTools.OfType<HostedWebSearchTool>().Any());

        await ChatAsync(OllamaWithoutKeyAlias, webSearch: true);
        Assert.IsFalse(_chatClients.LastTools.Any(IsWebSearchTool), "Asked for, but this provider cannot search.");
        Assert.IsFalse(SystemMessage().Contains(AiWebSearchRules.SystemRules));
        StringAssert.Contains(SystemMessage(), AiWebSearchRules.SystemRulesUnavailable);
    }

    [TestMethod]
    public async Task Chat_TheCitationsOfTheProvider_EndTheAnswerAsItsSources()
    {
        var events = await ChatAsync(PaidAlias, webSearch: true,
        [
            new ChatResponseUpdate(ChatRole.Assistant, [new WebSearchToolCallContent("search-1")]),
            new ChatResponseUpdate(ChatRole.Assistant, [new TextContent("It came out in 1985.")
            {
                Annotations = [new CitationAnnotation { Url = new Uri("https://example.org/cpc6128"), Title = "Amstrad CPC 6128" }]
            }])
        ]);

        Assert.AreEqual(AiWebSearchRules.WebSearchName, events.First(e => e.Type == AiChatStreamEventTypes.Tool).Text);
        var answer = events.Last().Turn!.Answer;
        StringAssert.StartsWith(answer, "It came out in 1985.");
        StringAssert.Contains(answer, "Sources:\n- [Amstrad CPC 6128](https://example.org/cpc6128)");
        Assert.AreEqual(answer, string.Concat(events.Where(e => e.Type == AiChatStreamEventTypes.Delta).Select(e => e.Text)),
            "The sources arrive as the rest of the answer.");
    }

    private static bool IsWebSearchTool(AITool tool) =>
        tool is HostedWebSearchTool || tool.Name is AiWebSearchRules.WebSearchName or AiWebSearchRules.WebFetchName;

    private static string SystemMessage() => _chatClients.LastMessages.First(m => m.Role == ChatRole.System).Text;

    private static async Task<List<AiChatStreamEventDto>> ChatAsync(string alias, bool webSearch, List<ChatResponseUpdate>? updates = null)
    {
        _chatClients.Script = () => [updates ?? [new ChatResponseUpdate(ChatRole.Assistant, "An answer.")]];

        var httpRes = await _httpClient.PostAsJsonAsync("api/aiassistant/chat",
            new AiChatRequestDto { ProviderAlias = alias, Prompt = "When did the CPC 6128 come out?", WebSearch = webSearch });
        var body = await httpRes.Content.ReadAsStringAsync();
        Assert.IsTrue(httpRes.IsSuccessStatusCode, body);

        return body.ReplaceLineEndings("\n")
            .Split('\n')
            .Where(line => line.StartsWith("data:"))
            .Select(line => JsonSerializer.Deserialize<AiChatStreamEventDto>(line["data:".Length..].Trim(), _json)!)
            .ToList();
    }
}
