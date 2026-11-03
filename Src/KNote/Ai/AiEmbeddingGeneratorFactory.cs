using KNote.Model;
using KNote.Service.Core;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace KNote.Ai;

// Builds the Microsoft.Extensions.AI embedding generator of an embeddings provider (AiProviderRef.Kind = Embeddings) of
// the providers that offer them (EnumAiProvider.WithEmbeddings: Ollama and OpenAI; Anthropic has no embedding models).
// Shared by ClientWin and Server, like AiChatClientFactory.
public static class AiEmbeddingGeneratorFactory
{
    public static IEmbeddingGenerator<string, Embedding<float>> Create(AiProviderRef providerRef)
    {
        if (providerRef is null)
            throw new ArgumentNullException(nameof(providerRef));

        return providerRef.Provider switch
        {
            EnumAiProvider.Ollama => new OllamaApiClient(providerRef.Host, providerRef.Model),

            EnumAiProvider.OpenAI => new OpenAI.Embeddings.EmbeddingClient(providerRef.Model,
                AiChatClientFactory.ResolveApiKey(providerRef, "OPENAI_API_KEY")).AsIEmbeddingGenerator(),

            EnumAiProvider.Anthropic => throw new ArgumentException("Anthropic does not offer embedding models.", nameof(providerRef)),

            _ => throw new ArgumentException($"Unknown AI provider: {providerRef.Provider}", nameof(providerRef))
        };
    }

    // The same generator, as the Service layer sees it (IKntEmbeddingGenerator).
    public static IKntEmbeddingGenerator CreateKntGenerator(AiProviderRef providerRef) =>
        new KntEmbeddingGenerator(providerRef.Model, Create(providerRef));
}

// IKntEmbeddingGenerator over a Microsoft.Extensions.AI embedding generator.
internal sealed class KntEmbeddingGenerator : IKntEmbeddingGenerator
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;

    public KntEmbeddingGenerator(string model, IEmbeddingGenerator<string, Embedding<float>> generator)
    {
        Model = model;
        _generator = generator;
    }

    public string Model { get; }

    public async Task<IReadOnlyList<float[]>> GenerateAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        if (texts is null || texts.Count == 0)
            return Array.Empty<float[]>();

        var embeddings = await _generator.GenerateAsync(texts, cancellationToken: cancellationToken);
        if (embeddings.Count != texts.Count)
            throw new InvalidOperationException($"The model {Model} returned {embeddings.Count} vectors for {texts.Count} texts.");

        return embeddings.Select(e => e.Vector.ToArray()).ToList();
    }
}
