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
// KNoteData.config. Invalid entries are left out and logged. The first valid one is the default.
public class AiProvidersCatalog
{
    private readonly List<AiProviderRef> _providers;

    public AiProvidersCatalog(IOptions<AiConfig> aiConfig, ILogger<AiProvidersCatalog> logger)
    {
        _providers = new List<AiProviderRef>();
        foreach (var provider in aiConfig.Value?.Providers ?? new List<AiProviderRef>())
        {
            if (provider.IsValid())
                _providers.Add(provider);
            else
                logger.LogWarning("AI provider '{alias}' ignored, its configuration is not valid: {errors}",
                    provider.Alias, provider.GetErrorMessage());
        }
    }

    public IReadOnlyList<AiProviderRef> Providers => _providers;

    public AiProviderRef Default => _providers.FirstOrDefault();

    // The provider with this alias; the default one when no alias is given; null when there is no such provider.
    public AiProviderRef Find(string alias) =>
        string.IsNullOrWhiteSpace(alias)
            ? Default
            : _providers.FirstOrDefault(p => string.Equals(p.Alias, alias, StringComparison.OrdinalIgnoreCase));

    public List<AiProviderInfoDto> GetInfo() =>
        _providers.Select(p => new AiProviderInfoDto
        {
            Alias = p.Alias,
            Provider = p.Provider,
            Model = p.Model,
            IsDefault = p == Default
        }).ToList();
}
