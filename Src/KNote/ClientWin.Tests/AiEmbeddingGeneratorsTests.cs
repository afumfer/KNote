using KNote.Ai;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;

namespace KNote.ClientWin.Tests;

/// <summary>
/// KNote Atlas' embedding models in KNote.Ai, without network: the factory builds the generators of Ollama and OpenAI
/// (never Anthropic's), the resolver finds them by model name among the valid embeddings providers only (read each
/// time, so a change in the settings counts at once), the store gives it to every repository's service, the chat
/// lists keep only the chat models, and a failed test of a provider says what to do.
/// </summary>
[TestClass]
public class AiEmbeddingGeneratorsTests
{
    private static AiProviderRef Embeddings(string provider, string model, string? alias = null) => new()
    {
        Alias = alias ?? model,
        Provider = provider,
        Model = model,
        Kind = EnumAiProviderKind.Embeddings,
        Host = provider == EnumAiProvider.Ollama ? "http://localhost:11434" : null,
        ApiKey = provider == EnumAiProvider.OpenAI ? "test-key" : null
    };

    [TestMethod]
    [DataRow(EnumAiProvider.Ollama)]
    [DataRow(EnumAiProvider.OpenAI)]
    public void Factory_BuildsTheGenerator_OfOllamaAndOpenAI(string provider)
    {
        var generator = AiEmbeddingGeneratorFactory.CreateKntGenerator(Embeddings(provider, "some-embedding-model"));

        Assert.AreEqual("some-embedding-model", generator.Model);
    }

    [TestMethod]
    public void Factory_RefusesAnthropic_AndAnUnknownProvider()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AiEmbeddingGeneratorFactory.Create(Embeddings(EnumAiProvider.Anthropic, "m")));
        Assert.ThrowsExactly<ArgumentException>(() => AiEmbeddingGeneratorFactory.Create(Embeddings("NotARealProvider", "m")));
        Assert.ThrowsExactly<ArgumentNullException>(() => AiEmbeddingGeneratorFactory.Create(null!));
    }

    [TestMethod]
    public void Resolver_FindsTheModel_AmongTheValidEmbeddingsProvidersOnly()
    {
        var invalid = Embeddings(EnumAiProvider.Ollama, "qwen3-embedding:4b");
        invalid.Host = null;
        var providers = new List<AiProviderRef>
        {
            new() { Alias = "Chat", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b", Host = "http://localhost:11434" },
            Embeddings(EnumAiProvider.Ollama, "qwen3-embedding:0.6b"),
            invalid
        };
        var resolver = new AiEmbeddingGenerators(() => providers);

        Assert.AreEqual("qwen3-embedding:0.6b", resolver.Find(" QWEN3-Embedding:0.6B ")?.Model);
        Assert.IsNull(resolver.Find("qwen3.5:9b"), "A chat model.");
        Assert.IsNull(resolver.Find("qwen3-embedding:4b"), "An invalid provider.");
        Assert.IsNull(resolver.Find(""));
        CollectionAssert.AreEqual(new[] { "qwen3-embedding:0.6b" }, resolver.Models.ToArray());
    }

    [TestMethod]
    public void Resolver_ReadsTheProvidersEachTime_AndReusesTheGenerators()
    {
        var providers = new List<AiProviderRef>();
        var resolver = new AiEmbeddingGenerators(() => providers);
        Assert.IsNull(resolver.Find("qwen3-embedding:0.6b"));

        providers.Add(Embeddings(EnumAiProvider.Ollama, "qwen3-embedding:0.6b"));
        var first = resolver.Find("qwen3-embedding:0.6b");

        Assert.IsNotNull(first);
        Assert.AreSame(first, resolver.Find("qwen3-embedding:0.6b"));
        providers[0].Host = "http://another-host:11434";
        Assert.AreNotSame(first, resolver.Find("qwen3-embedding:0.6b"), "Another configuration is another client.");
    }

    [TestMethod]
    public void TheStore_GivesItsEmbeddingModels_ToEveryRepositorysService()
    {
        var store = TestStoreFactory.CreateEmpty();
        store.Settings.Ai.Providers.Add(Embeddings(EnumAiProvider.Ollama, "qwen3-embedding:0.6b"));
        var service = new FakeKntService();

        store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));

        Assert.AreSame(store.EmbeddingGenerators, service.EmbeddingGenerators);
        Assert.IsNotNull(service.EmbeddingGenerators!.Find("qwen3-embedding:0.6b"));
    }

    [TestMethod]
    public void TheAssistant_ListsOnlyTheChatModels()
    {
        var store = TestStoreFactory.CreateEmpty();
        store.Settings.Ai.Providers.Add(Embeddings(EnumAiProvider.Ollama, "qwen3-embedding:0.6b"));
        store.Settings.Ai.Providers.Add(new AiProviderRef { Alias = "Chat", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b", Host = "http://localhost" });

        var ctrl = new KNoteAIAssistantCtrl(store);

        Assert.AreEqual("Chat", ctrl.AiProviderRefs.Single().Alias);
        Assert.AreEqual("Chat", ctrl.GetPreferredProvider().Alias);
    }

    [TestMethod]
    public void ChatFactory_WithAContextSize_BuildsTheOllamaClient()
    {
        var provider = new AiProviderRef { Alias = "Chat", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b", Host = "http://localhost:11434",
            ContextWindow = 131072 };

        Assert.IsNotNull(AiChatClientFactory.Create(provider));
    }

    [TestMethod]
    public void Tester_ExplainsAModelNotDownloaded()
    {
        var provider = Embeddings(EnumAiProvider.Ollama, "qwen3-embedding:0.6b");

        var message = AiProviderTester.Explain(provider, new InvalidOperationException("model \"qwen3-embedding:0.6b\" not found, try pulling it first"));

        StringAssert.Contains(message, "ollama pull qwen3-embedding:0.6b");
    }

    [TestMethod]
    public async Task Tester_RefusesAnInvalidProvider_WithoutCallingIt()
    {
        var result = await AiProviderTester.TestAsync(Embeddings(EnumAiProvider.Anthropic, "m"));

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.ErrorMessage, "embedding");
    }
}
