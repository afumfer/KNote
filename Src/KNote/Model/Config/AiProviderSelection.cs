using System;
using System.Collections.Generic;
using System.Linq;

namespace KNote.Model;

// What identifies a configured AI provider, whether it is ClientWin's AiProviderRef or the Web's AiProviderInfoDto.
public interface IAiProviderIdentity
{
    string Alias { get; }

    string Provider { get; }

    string Model { get; }
}

public static class AiProviderSelection
{
    // The configured provider a saved session was held with (same provider and model, whatever its alias is
    // now), or null when it is no longer configured: then each application continues with its default one.
    public static T ForSession<T>(IEnumerable<T> providers, string provider, string model) where T : class, IAiProviderIdentity =>
        providers?.FirstOrDefault(p =>
            string.Equals(p.Provider, provider, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.Model, model, StringComparison.OrdinalIgnoreCase));

    // The provider of a task of KNote Atlas (D13): one with the repository's recommended LLM if this installation has it,
    // else the fallback (the default chat provider); null if there is none.
    public static T ForRepository<T>(IEnumerable<T> providers, string recommendedModel, T fallback) where T : class, IAiProviderIdentity =>
        (string.IsNullOrWhiteSpace(recommendedModel) ? null : providers?.FirstOrDefault(p =>
            string.Equals(p.Model?.Trim(), recommendedModel.Trim(), StringComparison.OrdinalIgnoreCase)))
        ?? fallback;
}
