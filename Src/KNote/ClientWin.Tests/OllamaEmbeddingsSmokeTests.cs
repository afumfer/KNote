using KNote.Ai;
using KNote.ClientWin.Tests.Helpers;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Real-call smoke tests of KNote Atlas' embedding models against a reachable Ollama server (the host of
/// OllamaProviderSmokeTests; model AiProviderSmokeTests:Ollama:EmbeddingModel, qwen3-embedding:0.6b by default, which
/// must be downloaded). Excluded from routine runs; run them after bumping OllamaSharp/Microsoft.Extensions.AI. An
/// unreachable host is Inconclusive, as in OllamaProviderSmokeTests.
/// </summary>
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class OllamaEmbeddingsSmokeTests
{
    private static async Task<KNote.Model.AiProviderRef> GetReachableProviderRefOrInconclusiveAsync()
    {
        var providerRef = AiTestConfig.TryGetOllamaEmbeddingsProviderRef();
        if (providerRef == null)
        {
            Assert.Inconclusive("No Ollama host configured (OLLAMA_HOST env var or ClientWin.Tests user-secrets/appsettings.json) - skipping Ollama embeddings smoke test.");
            return null;
        }

        if (!await AiTestConfig.IsOllamaReachableAsync(providerRef.Host))
        {
            Assert.Inconclusive($"Ollama host '{providerRef.Host}' is not reachable - skipping Ollama embeddings smoke test.");
            return null;
        }

        return providerRef;
    }

    [TestMethod]
    public async Task Generate_GivesOneVectorPerText_OfTheModelsDimensions()
    {
        var providerRef = await GetReachableProviderRefOrInconclusiveAsync();
        if (providerRef == null)
            return;

        var generator = AiEmbeddingGeneratorFactory.CreateKntGenerator(providerRef);
        var vectors = await generator.GenerateAsync(new[] { "The server does not start after a reboot", "Amstrad CPC 464 restoration" });

        Assert.HasCount(2, vectors);
        Assert.IsGreaterThan(0, vectors[0].Length);
        Assert.AreEqual(vectors[0].Length, vectors[1].Length);
        if (providerRef.Model == "qwen3-embedding:0.6b")
            Assert.AreEqual(1024, vectors[0].Length);
    }

    [TestMethod]
    public async Task TheTestOfTheProvider_SaysItsDimensions()
    {
        var providerRef = await GetReachableProviderRefOrInconclusiveAsync();
        if (providerRef == null)
            return;

        var result = await AiProviderTester.TestAsync(providerRef);

        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        StringAssert.Contains(result.Entity, "dimensions");
    }
}
