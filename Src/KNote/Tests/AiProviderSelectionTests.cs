using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// AiProviderSelection.ForSession: a resumed AI session continues with the configured provider it was held with
/// (same provider and model, whatever its alias is now), or with the application's default when there is none.
/// </summary>
[TestClass]
public class AiProviderSelectionTests
{
    private static readonly List<AiProviderInfoDto> Providers =
    [
        new() { Alias = "Default", Provider = EnumAiProvider.OpenAI, Model = "gpt-4o-mini", IsDefault = true },
        new() { Alias = "Claude renamed", Provider = EnumAiProvider.Anthropic, Model = "claude-haiku-4-5" }
    ];

    [TestMethod]
    public void SameProviderAndModel_IsFound_WhateverItsAlias()
    {
        var provider = AiProviderSelection.ForSession(Providers, "anthropic", "CLAUDE-HAIKU-4-5");

        Assert.AreEqual("Claude renamed", provider?.Alias);
    }

    [TestMethod]
    public void AModelNoLongerConfigured_IsNotFound()
    {
        Assert.IsNull(AiProviderSelection.ForSession(Providers, EnumAiProvider.Anthropic, "claude-old"));
        Assert.IsNull(AiProviderSelection.ForSession(Providers, EnumAiProvider.Ollama, "gpt-4o-mini"));
        Assert.IsNull(AiProviderSelection.ForSession<AiProviderInfoDto>(null!, EnumAiProvider.OpenAI, "gpt-4o-mini"));
    }

    [TestMethod]
    public void WorksForClientWinProviders_Too()
    {
        var clientWinProviders = new List<AiProviderRef>
        {
            new() { Alias = "Local", Provider = EnumAiProvider.Ollama, Model = "llama3.1", Host = "http://localhost:11434" }
        };

        Assert.AreEqual("Local", AiProviderSelection.ForSession(clientWinProviders, EnumAiProvider.Ollama, "llama3.1")?.Alias);
    }

    [TestMethod]
    public void UsageSummary_TellsReportedFromEstimatedTokens()
    {
        var reported = new AiChatTurnDto { InputTokens = 3, OutputTokens = 2, TotalTokens = 5, ProcessingTime = TimeSpan.FromSeconds(1.25) };
        var estimated = new AiChatTurnDto { TotalTokens = 7, TokensEstimated = true, ProcessingTime = TimeSpan.FromSeconds(2) };

        Assert.AreEqual("3 in · 2 out · 5 tokens · 1.3 s", reported.UsageSummary());
        Assert.AreEqual("~7 tokens (estimated) · 2.0 s", estimated.UsageSummary());
    }
}
