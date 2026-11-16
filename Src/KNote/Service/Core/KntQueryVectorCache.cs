using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;

namespace KNote.Service.Core;

/// <summary>
/// The vectors of the last texts searched by meaning, per generator (the one of a provider's configuration: another host is
/// another generator, AiEmbeddingGenerators), so the same search done again does not ask the model again (with Ollama
/// without a GPU, that call alone took seconds). The vector of a text depends only on the model, the text (with the
/// instruction of the query) and the dimensions it is cut to. At most MaxTexts texts per generator: when full, it starts again.
/// </summary>
internal static class KntQueryVectorCache
{
    public const int MaxTexts = 32;

    private static readonly ConditionalWeakTable<IKntEmbeddingGenerator, ConcurrentDictionary<string, float[]>> Caches = new();

    /// <summary>The normalized vector of the text, from the cache or asked to the generator.</summary>
    public static async Task<float[]> GetAsync(IKntEmbeddingGenerator generator, string text, int? dimensions, CancellationToken cancellationToken)
    {
        var cache = Caches.GetValue(generator, _ => new ConcurrentDictionary<string, float[]>(StringComparer.Ordinal));
        var key = $"{dimensions}\n{text}";
        if (cache.TryGetValue(key, out var cached))
            return cached;

        var vectors = await generator.GenerateAsync(new[] { text }, cancellationToken);
        if (vectors == null || vectors.Count != 1)
            throw new KntServiceException($"The model gave {vectors?.Count ?? 0} vectors for 1 text.");
        var vector = EmbeddingVector.Normalize(vectors[0], dimensions);

        if (cache.Count >= MaxTexts)
            cache.Clear();
        cache[key] = vector;
        return vector;
    }
}
