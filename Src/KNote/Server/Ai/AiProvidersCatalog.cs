using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KNote.Server.Ai;

// The AI providers configured for the Web assistant: section "ai" of the configuration (appsettings.json, with
// the API keys in user-secrets or environment variables), the same AiProviderRef fields ClientWin keeps in
// KNoteData.config. Invalid entries are left out and logged. The assistant only sees the chat ones (Kind = Chat): the
// first valid one is the default. The embeddings ones are the Server's embedding models (EmbeddingGenerators, KNote
// Atlas). Read when it is created and, in KNote Local, again when the computer's settings change (Reload,
// KntLocalSettings).
public class AiProvidersCatalog
{
    private readonly IOptions<AiConfig> _aiConfig;
    private readonly ILogger<AiProvidersCatalog> _logger;
    private volatile List<AiProviderRef> _providers;

    public AiProvidersCatalog(IOptions<AiConfig> aiConfig, ILogger<AiProvidersCatalog> logger)
    {
        _aiConfig = aiConfig;
        _logger = logger;
        Reload();
    }

    public void Reload()
    {
        var providers = new List<AiProviderRef>();
        foreach (var provider in _aiConfig.Value?.Providers ?? new List<AiProviderRef>())
        {
            if (provider.IsValid())
                providers.Add(provider);
            else
                _logger.LogWarning("AI provider '{alias}' ignored, its configuration is not valid: {errors}",
                    provider.Alias, provider.GetErrorMessage());
        }
        _providers = providers;
    }

    public IReadOnlyList<AiProviderRef> Providers => _providers;

    public IEnumerable<AiProviderRef> ChatProviders => _providers.Where(p => p.IsChat);

    public IEnumerable<AiProviderRef> EmbeddingProviders => _providers.Where(p => p.IsEmbeddings);

    public AiProviderRef Default => ChatProviders.FirstOrDefault();

    // The chat provider with this alias; the default one when no alias is given; null when there is no such provider.
    public AiProviderRef Find(string alias) =>
        string.IsNullOrWhiteSpace(alias)
            ? Default
            : ChatProviders.FirstOrDefault(p => string.Equals(p.Alias, alias, StringComparison.OrdinalIgnoreCase));

    // The chat and embedding models of this Server (to choose the repository's models), without hosts or keys.
    public AiInstallationModelsDto GetModels() => new()
    {
        EmbeddingModels = ModelsOf(EmbeddingProviders),
        ChatModels = ModelsOf(ChatProviders)
    };

    private static List<AiModelInfoDto> ModelsOf(IEnumerable<AiProviderRef> providers) =>
        providers
            .GroupBy(p => p.Model.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new AiModelInfoDto { Provider = g.First().Provider, Model = g.Key, IsPaid = g.First().IsPaid })
            .ToList();

    public List<AiProviderInfoDto> GetInfo() =>
        ChatProviders.Select(p => new AiProviderInfoDto
        {
            Alias = p.Alias,
            Provider = p.Provider,
            Model = p.Model,
            IsDefault = p == Default
        }).ToList();
}
