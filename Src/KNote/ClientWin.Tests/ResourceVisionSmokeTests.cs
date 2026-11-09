using KNote.Ai;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;

namespace KNote.ClientWin.Tests;

// KNote Atlas 4.0: a real model describes a photo and transcribes a scanned page, with its reasoning and without it. Only Ollama:
// run these with --filter "FullyQualifiedName~ResourceVision" (see ClientWin.Tests/CLAUDE.md). A model that does not see images
// says nothing about the wiring.
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class ResourceVisionSmokeTests
{
    private static async Task<AiProviderRef?> GetProviderOrInconclusiveAsync()
    {
        var provider = AiTestConfig.TryGetOllamaProviderRef();
        if (provider == null)
        {
            Assert.Inconclusive("No Ollama host configured - skipping.");
            return null;
        }

        // The model of the stage may not be the one the other smoke tests use.
        var model = Environment.GetEnvironmentVariable("KNOTE_RESOURCE_SMOKE_MODEL");
        if (!string.IsNullOrWhiteSpace(model))
            provider.Model = model;

        if (!await AiTestConfig.IsOllamaReachableAsync(provider.Host))
        {
            Assert.Inconclusive($"Ollama host '{provider.Host}' is not reachable - skipping.");
            return null;
        }
        if (!await AiModelCapabilities.SupportsImagesAsync(provider))
        {
            Assert.Inconclusive($"{provider.Model} does not see images: try with a model that does.");
            return null;
        }
        return provider;
    }

    // Off is what the stage counts on to save time. A model that does not reason says nothing about the wiring when it is on.
    private static void AssertReasoning(AiProviderRef provider, ResourceVisionRun.Outcome outcome, bool reasoning)
    {
        if (!reasoning)
            Assert.AreEqual(0, outcome.ReasoningChars, $"{provider.Model} reasoned with its reasoning off.");
        else if (outcome.ReasoningChars == 0)
            Assert.Inconclusive($"{provider.Model} did not reason: try with a model that does.");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task APhoto_IsDescribed(bool reasoning)
    {
        var provider = await GetProviderOrInconclusiveAsync();
        if (provider == null)
            return;

        var client = AiChatClientFactory.Create(provider);
        var outcome = await ResourceVisionRun.DescribePhotoAsync(client, provider.Provider, ResourceProbe.PhotoWithText("SERIAL 7351"), "image/jpeg", null, "English", reasoning);

        StringAssert.Contains(outcome.Text, "7351");
        Assert.IsFalse(outcome.Cut, "The description reached the limit of tokens.");
        AssertReasoning(provider, outcome, reasoning);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task AScannedPage_IsDrawnAndTranscribed(bool reasoning)
    {
        var provider = await GetProviderOrInconclusiveAsync();
        if (provider == null)
            return;

        var pdf = ResourceProbe.MixedPdf("A page of text.", "SERVICE MANUAL", "PART 4821");
        Assert.IsTrue(ResourceProbe.InspectPdf(pdf)[1].NeedsTranscription);

        var client = AiChatClientFactory.Create(provider);
        var outcome = await ResourceVisionRun.TranscribePageAsync(client, provider.Provider, ResourceProbe.DrawPage(pdf, 2, 1600), reasoning);

        StringAssert.Contains(outcome.Text, "SERVICE MANUAL");
        StringAssert.Contains(outcome.Text, "4821");
        AssertReasoning(provider, outcome, reasoning);
    }
}
