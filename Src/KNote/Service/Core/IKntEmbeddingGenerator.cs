using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KNote.Service.Core;

/// <summary>
/// An embedding model of this installation (KNote Atlas): turns texts into vectors that represent their meaning.
/// Implemented in KNote.Ai over the providers' clients, so that Service needs no AI package.
/// </summary>
public interface IKntEmbeddingGenerator
{
    /// <summary>The model's name, as the repository declares it (RepositoryAiModelsDto.EmbeddingModel).</summary>
    string Model { get; }

    /// <summary>
    /// One vector per text, in the same order, as the provider gives it (with the model's dimensions, not
    /// normalized: the index cuts and normalizes them).
    /// </summary>
    Task<IReadOnlyList<float[]>> GenerateAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);
}

/// <summary>
/// The embedding models of this installation, by name: each host gives one to its IKntService
/// (IKntService.EmbeddingGenerators), from its own AI providers' configuration.
/// </summary>
public interface IKntEmbeddingGenerators
{
    /// <summary>The generator of this model (case-insensitive), or null when no embeddings provider of this installation has it.</summary>
    IKntEmbeddingGenerator Find(string model);

    /// <summary>The names of the embedding models configured in this installation.</summary>
    IReadOnlyList<string> Models { get; }
}
