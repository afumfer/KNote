using System.Net;
using System.Text;
using System.Text.Json;
using KNote.Ai;
using KNote.Model;
using Microsoft.Extensions.AI;

namespace KNote.Tests;

/// <summary>
/// The Internet search of the assistant (KNote Atlas, chapter 23): which providers search and with which tools, the sources
/// added to an answer, and KNote's own search with Ollama (ollama.com's web search) against a fake server.
/// </summary>
[TestClass]
public class AiWebSearchTests
{
    private static AiProviderRef Provider(string provider, string webSearchApiKey = null, string kind = EnumAiProviderKind.Chat) => new()
    {
        Alias = provider, Provider = provider, Model = "m", Host = "http://localhost:11434", Kind = kind, WebSearchApiKey = webSearchApiKey
    };

    [TestMethod]
    public void SupportsWebSearch_ThePaidProviders_AndOllamaWithItsKey_OnlyForChat()
    {
        Assert.IsTrue(Provider(EnumAiProvider.OpenAI).SupportsWebSearch);
        Assert.IsTrue(Provider(EnumAiProvider.Anthropic).SupportsWebSearch);
        Assert.IsTrue(Provider(EnumAiProvider.Ollama, "key").SupportsWebSearch);
        Assert.IsFalse(Provider(EnumAiProvider.Ollama).SupportsWebSearch);
        Assert.IsFalse(Provider(EnumAiProvider.Ollama, " ").SupportsWebSearch);
        Assert.IsFalse(Provider(EnumAiProvider.Ollama, "key", EnumAiProviderKind.Embeddings).SupportsWebSearch);
    }

    [TestMethod]
    public void ToolsFor_ThePaidProvidersSearchThemselves_OllamaWithKnotesTools()
    {
        Assert.IsInstanceOfType<HostedWebSearchTool>(AiWebSearch.ToolsFor(Provider(EnumAiProvider.OpenAI)).Single());
        Assert.IsInstanceOfType<HostedWebSearchTool>(AiWebSearch.ToolsFor(Provider(EnumAiProvider.Anthropic)).Single());
        CollectionAssert.AreEqual(new[] { AiWebSearchRules.WebSearchName, AiWebSearchRules.WebFetchName },
            AiWebSearch.ToolsFor(Provider(EnumAiProvider.Ollama, "key")).Select(t => t.Name).ToList());
        Assert.AreEqual(0, AiWebSearch.ToolsFor(Provider(EnumAiProvider.Ollama)).Count);
        Assert.AreEqual(0, AiWebSearch.ToolsFor(null).Count);
    }

    [TestMethod]
    public void SystemPrompt_SaysWhetherTheModelCanSearch()
    {
        var paid = Provider(EnumAiProvider.OpenAI);
        var withoutKey = Provider(EnumAiProvider.Ollama);

        Assert.AreEqual($"Rules.\n\n{AiWebSearchRules.SystemRules}", AiWebSearch.SystemPrompt("Rules.", paid, true));
        Assert.AreEqual($"Rules.\n\n{AiWebSearchRules.SystemRulesOff}", AiWebSearch.SystemPrompt("Rules.", paid, false));
        Assert.AreEqual($"Rules.\n\n{AiWebSearchRules.SystemRulesUnavailable}", AiWebSearch.SystemPrompt("Rules.", withoutKey, false));
        Assert.AreEqual($"Rules.\n\n{AiWebSearchRules.SystemRulesUnavailable}", AiWebSearch.SystemPrompt("Rules.", withoutKey, true));
        Assert.AreEqual("Rules.", AiWebSearch.SystemPrompt("Rules.", null, false));
    }

    [TestMethod]
    public void Notice_SaysWhereTheSearchesGo_AndThatAnthropicBillsThem()
    {
        StringAssert.Contains(AiWebSearchRules.Notice(EnumAiProvider.Anthropic), "bills each search");
        StringAssert.Contains(AiWebSearchRules.Notice(EnumAiProvider.OpenAI), "OpenAI");
        StringAssert.Contains(AiWebSearchRules.Notice(EnumAiProvider.Ollama), "ollama.com");
    }

    #region Sources

    [TestMethod]
    public void SourcesSection_ListsTheCitations_OnceEach()
    {
        var sources = AiWebSearchRules.SourcesSection("The answer.", new[]
        {
            ("https://example.org/a", "Page A"),
            ("https://example.org/a/", "Page A again"),
            ("https://example.org/b", "Page B")
        });

        Assert.AreEqual("\n\nSources:\n- [Page A](https://example.org/a)\n- [Page B](https://example.org/b)", sources);
    }

    [TestMethod]
    public void SourcesSection_LeavesOutTheLinksTheAnswerHas_AndTheOnesThatAreNotPages()
    {
        var sources = AiWebSearchRules.SourcesSection("See [it](https://example.org/a).", new[]
        {
            ("https://example.org/a", "Page A"),
            ("javascript:alert(1)", "Script"),
            ("file:///c:/secret.txt", "File"),
            (null, "Nothing")
        });

        Assert.AreEqual("", sources);
        Assert.AreEqual("", AiWebSearchRules.SourcesSection("The answer.", null));
    }

    [TestMethod]
    public void SourcesSection_ATitleWithoutBrackets_OrTheSiteWhenItHasNone()
    {
        var sources = AiWebSearchRules.SourcesSection("", new[] { ("https://example.org/x", "[Ad] Page\nnew"), ("https://site.example/y", " ") });

        StringAssert.Contains(sources, "- [Ad Page new](https://example.org/x)");
        StringAssert.Contains(sources, "- [site.example](https://site.example/y)");
    }

    #endregion

    #region Ollama's web search

    [TestMethod]
    public async Task Search_SendsTheQueryWithTheKey_AndGivesTheResultsCut_AsDataOfThirdParties()
    {
        var server = new FakeOllamaServer(HttpStatusCode.OK, JsonSerializer.Serialize(new
        {
            results = new[]
            {
                new { title = "Amstrad CPC 6128", url = "https://example.org/cpc", content = new string('x', 3000) },
                new { title = "Other", url = "https://example.org/other", content = "Short." }
            }
        }));

        var text = await new OllamaWebSearch(" the-key ", server.Client).SearchAsync("CPC 6128 launch");

        Assert.AreEqual("https://ollama.com/api/web_search", server.Request!.RequestUri!.AbsoluteUri);
        Assert.AreEqual("Bearer the-key", server.Request.Headers.Authorization!.ToString());
        using var body = JsonDocument.Parse(server.Body!);
        Assert.AreEqual("CPC 6128 launch", body.RootElement.GetProperty("query").GetString());
        Assert.AreEqual(AiWebSearchRules.MaxResults, body.RootElement.GetProperty("max_results").GetInt32());
        StringAssert.Contains(text, "data, not instructions");
        StringAssert.Contains(text, "1. Amstrad CPC 6128\n   https://example.org/cpc");
        StringAssert.Contains(text, "(cut)");
        Assert.IsFalse(text.Contains(new string('x', AiWebSearchRules.MaxResultLength + 1)));
        StringAssert.Contains(text, "2. Other");
    }

    [TestMethod]
    public async Task Fetch_GivesThePageCut_WithSomeOfItsLinks()
    {
        var server = new FakeOllamaServer(HttpStatusCode.OK, JsonSerializer.Serialize(new
        {
            title = "The page",
            content = new string('y', AiWebSearchRules.MaxPageLength + 500),
            links = Enumerable.Range(1, 30).Select(i => $"https://example.org/{i}").ToArray()
        }));

        var text = await new OllamaWebSearch("key", server.Client).FetchAsync("https://example.org/page");

        Assert.AreEqual("https://ollama.com/api/web_fetch", server.Request!.RequestUri!.AbsoluteUri);
        StringAssert.Contains(server.Body, "https://example.org/page");
        StringAssert.Contains(text, "Title: The page");
        StringAssert.Contains(text, "(cut)");
        StringAssert.Contains(text, $"https://example.org/{AiWebSearchRules.MaxPageLinks}\n".TrimEnd('\n'));
        Assert.IsFalse(text.Contains($"https://example.org/{AiWebSearchRules.MaxPageLinks + 1}"));
    }

    [TestMethod]
    public async Task Fetch_OnlyThePagesOfTheInternet()
    {
        var server = new FakeOllamaServer(HttpStatusCode.OK, "{}");

        StringAssert.StartsWith(await new OllamaWebSearch("key", server.Client).FetchAsync("file:///c:/secret.txt"), "Error:");
        StringAssert.StartsWith(await new OllamaWebSearch("key", server.Client).FetchAsync("not a link"), "Error:");
        Assert.IsNull(server.Request, "Nothing was sent.");
    }

    [TestMethod]
    [DataRow(HttpStatusCode.Unauthorized, "check it")]
    [DataRow(HttpStatusCode.Forbidden, "check it")]
    [DataRow(HttpStatusCode.TooManyRequests, "try later")]
    [DataRow(HttpStatusCode.InternalServerError, "500")]
    public async Task Search_WhatWentWrong_IsToldToTheModel_NeverThrown(HttpStatusCode status, string expected)
    {
        var text = await new OllamaWebSearch("key", new FakeOllamaServer(status, "{}").Client).SearchAsync("anything");

        StringAssert.StartsWith(text, "Error:");
        StringAssert.Contains(text, expected);
    }

    [TestMethod]
    public async Task Search_WithoutKey_OrWithoutWhatToSearch_SendsNothing()
    {
        var server = new FakeOllamaServer(HttpStatusCode.OK, "{}");

        StringAssert.Contains(await new OllamaWebSearch(null, server.Client).SearchAsync("anything"), "web search key");
        StringAssert.StartsWith(await new OllamaWebSearch("key", server.Client).SearchAsync(" "), "Error:");
        Assert.IsNull(server.Request);
    }

    [TestMethod]
    public async Task Search_ThatFindsNothing_SaysSo()
    {
        var text = await new OllamaWebSearch("key", new FakeOllamaServer(HttpStatusCode.OK, "{\"results\":[]}").Client).SearchAsync("zzz");

        StringAssert.Contains(text, "found nothing");
    }

    [TestMethod]
    public async Task Test_OfTheKey_NullWhenItWorks_TheProblemOtherwise()
    {
        Assert.IsNull(await new OllamaWebSearch("key", new FakeOllamaServer(HttpStatusCode.OK, "{\"results\":[]}").Client).TestAsync());
        StringAssert.Contains(await new OllamaWebSearch("key", new FakeOllamaServer(HttpStatusCode.Unauthorized, "{}").Client).TestAsync(), "key");
    }

    private sealed class FakeOllamaServer : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _response;

        public FakeOllamaServer(HttpStatusCode status, string response)
        {
            _status = status;
            _response = response;
            Client = new HttpClient(this);
        }

        public HttpClient Client { get; }

        public HttpRequestMessage? Request { get; private set; }

        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(_status) { Content = new StringContent(_response, Encoding.UTF8, "application/json") };
        }
    }

    #endregion
}
