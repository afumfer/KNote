using System.Collections.Concurrent;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.Ai;

// The embedding models of an installation (IKntEmbeddingGenerators, IKntService.EmbeddingGenerators), the same for both
// hosts: each one only gives its list of AI providers, read every time (ClientWin: Store.Settings.Ai.Providers, which
// the user edits while the app runs; Server: AiProvidersCatalog, reloaded in KNote Local). Only the valid embeddings
// providers count; with two of the same model, the first one. The clients are built once per provider configuration.
public class AiEmbeddingGenerators : IKntEmbeddingGenerators
{
    private readonly Func<IEnumerable<AiProviderRef>> _providers;
    private readonly ConcurrentDictionary<string, IKntEmbeddingGenerator> _generators = new();

    public AiEmbeddingGenerators(Func<IEnumerable<AiProviderRef>> providers)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
    }

    public IKntEmbeddingGenerator Find(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            return null;

        var provider = EmbeddingProviders().FirstOrDefault(p => string.Equals(p.Model?.Trim(), model.Trim(), StringComparison.OrdinalIgnoreCase));
        if (provider == null)
            return null;

        // A change of host, key... in the configuration is another client.
        var key = string.Join("\n", provider.Provider, provider.Host, provider.Model, provider.ApiKey);
        return _generators.GetOrAdd(key, _ => AiEmbeddingGeneratorFactory.CreateKntGenerator(provider));
    }

    public IReadOnlyList<string> Models =>
        EmbeddingProviders().Select(p => p.Model.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    private IEnumerable<AiProviderRef> EmbeddingProviders() =>
        (_providers() ?? Enumerable.Empty<AiProviderRef>()).Where(p => p != null && p.IsEmbeddings && p.IsValid());
}
