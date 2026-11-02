using KNote.Model;

namespace KNote.Tests;

/// <summary>
/// AiProviderRef's kind (chat or embedding model, KNote Atlas) and context size: a provider is a chat model unless it
/// says otherwise, Anthropic has no embedding models and the context size has its limits.
/// </summary>
[TestClass]
public class AiProviderRefTests
{
    [TestMethod]
    public void ANewProvider_IsAChatModel()
    {
        var provider = new AiProviderRef();

        Assert.AreEqual(EnumAiProviderKind.Chat, provider.Kind);
        Assert.IsTrue(provider.IsChat);
        Assert.IsFalse(provider.IsEmbeddings);
        Assert.IsNull(provider.ContextWindow);
    }

    [TestMethod]
    [DataRow(EnumAiProvider.Ollama)]
    [DataRow(EnumAiProvider.OpenAI)]
    public void EmbeddingModels_OfOllamaAndOpenAI_AreValid(string providerName)
    {
        var provider = Provider(providerName, EnumAiProviderKind.Embeddings);

        Assert.IsTrue(provider.IsValid(), provider.GetErrorMessage());
        Assert.IsTrue(provider.IsEmbeddings);
    }

    [TestMethod]
    public void EmbeddingModels_OfAnthropic_AreRefused()
    {
        var provider = Provider(EnumAiProvider.Anthropic, EnumAiProviderKind.Embeddings);

        Assert.IsFalse(provider.IsValid());
        StringAssert.Contains(provider.GetErrorMessage(), "embedding");
    }

    [TestMethod]
    public void AnUnknownKind_IsRefused()
    {
        Assert.IsFalse(Provider(EnumAiProvider.Ollama, "Vision").IsValid());
    }

    [TestMethod]
    [DataRow(null, true)]
    [DataRow(AiProviderRef.MinContextWindow, true)]
    [DataRow(131072, true)]
    [DataRow(AiProviderRef.MaxContextWindow, true)]
    [DataRow(AiProviderRef.MinContextWindow - 1, false)]
    [DataRow(0, false)]
    [DataRow(AiProviderRef.MaxContextWindow + 1, false)]
    public void TheContextSize_HasItsLimits(int? contextWindow, bool valid)
    {
        var provider = Provider(EnumAiProvider.Ollama, EnumAiProviderKind.Chat);
        provider.ContextWindow = contextWindow;

        Assert.AreEqual(valid, provider.IsValid(), provider.GetErrorMessage());
    }

    [TestMethod]
    [DataRow(EnumAiProvider.Ollama, false)]
    [DataRow(EnumAiProvider.OpenAI, true)]
    [DataRow(EnumAiProvider.Anthropic, true)]
    public void OnlyOllama_IsNotPaid(string providerName, bool paid)
    {
        Assert.AreEqual(paid, Provider(providerName, EnumAiProviderKind.Chat).IsPaid);
    }

    private static AiProviderRef Provider(string providerName, string kind) => new()
    {
        Alias = "Test",
        Provider = providerName,
        Model = "some-model",
        Kind = kind,
        Host = providerName == EnumAiProvider.Ollama ? "http://localhost:11434" : null
    };
}
