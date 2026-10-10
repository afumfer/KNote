using KNote.Ai;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;

namespace KNote.ClientWin.Tests;

[TestClass]
public class AiChatClientFactoryTests
{
    private const string NonExistentEnvVar = "KNOTE_TEST_ENV_VAR_THAT_DOES_NOT_EXIST";

    [TestMethod]
    public void ResolveApiKey_ProviderRefHasKey_ReturnsIt()
    {
        var providerRef = new AiProviderRef { ApiKey = "from-config" };

        var result = AiChatClientFactory.ResolveApiKey(providerRef, NonExistentEnvVar);

        Assert.AreEqual("from-config", result);
    }

    [TestMethod]
    public void ResolveApiKey_ProviderRefEmpty_FallsBackToEnvironmentVariable()
    {
        const string envVar = "KNOTE_TEST_ENV_VAR_RESOLVE_API_KEY";
        Environment.SetEnvironmentVariable(envVar, "from-env");
        try
        {
            var providerRef = new AiProviderRef { ApiKey = "" };

            var result = AiChatClientFactory.ResolveApiKey(providerRef, envVar);

            Assert.AreEqual("from-env", result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [TestMethod]
    public void ResolveApiKey_NeitherConfigured_ReturnsEmptyString()
    {
        var providerRef = new AiProviderRef { ApiKey = "" };

        var result = AiChatClientFactory.ResolveApiKey(providerRef, NonExistentEnvVar);

        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void Create_NullProviderRef_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => AiChatClientFactory.Create(null));
    }

    [TestMethod]
    public void Create_UnknownProvider_ThrowsArgumentException()
    {
        var providerRef = new AiProviderRef { Provider = "NotARealProvider", Model = "x" };

        Assert.ThrowsExactly<ArgumentException>(() => AiChatClientFactory.Create(providerRef));
    }

    [TestMethod]
    public void Create_EachKnownProvider_ReturnsChatClientWithoutTouchingTheNetwork()
    {
        // Client construction (OpenAI.Responses.ResponsesClient / AnthropicClient / OllamaApiClient, plus the
        // .AsBuilder()/.UseFunctionInvocation() wrapping) is all lazy - no request is made until a
        // GetResponseAsync/GetStreamingResponseAsync call - so a placeholder key/host is enough to
        // catch build-breaking API changes from a NuGet bump without needing real credentials.
        foreach (var provider in EnumAiProvider.All)
        {
            var providerRef = new AiProviderRef
            {
                Alias = $"Test {provider}",
                Provider = provider,
                Model = "test-model",
                ApiKey = "placeholder-key",
                Host = "http://localhost:11434"
            };

            // With and without tools: ClientWin always passes KNoteAiTools' (via TestAiChatClientFactory, the
            // same wiring as KNoteAIAssistantCtrl), the bare factory call must work too.
            Assert.IsNotNull(AiChatClientFactory.Create(providerRef), $"Create({provider}) returned null.");
            Assert.IsNotNull(TestAiChatClientFactory.Create(providerRef), $"Create({provider}) with tools returned null.");
        }
    }
}
