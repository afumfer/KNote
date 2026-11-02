using KNote.Service.Core;

namespace KNote.Tests.Helpers;

/// <summary>
/// The embedding models of a test "installation" (IKntService.EmbeddingGenerators), without any AI provider: the
/// models it has, by name. Their generators are not called yet (the semantic index comes in later deliveries).
/// </summary>
internal sealed class FakeEmbeddingGenerators : IKntEmbeddingGenerators
{
    private readonly List<string> _models;

    public FakeEmbeddingGenerators(params string[] models)
    {
        _models = models.ToList();
    }

    public IKntEmbeddingGenerator Find(string model) =>
        _models.FirstOrDefault(m => string.Equals(m, model?.Trim(), StringComparison.OrdinalIgnoreCase)) is string found
            ? new Generator(found)
            : null!;

    public IReadOnlyList<string> Models => _models;

    private sealed class Generator : IKntEmbeddingGenerator
    {
        public Generator(string model) => Model = model;

        public string Model { get; }

        public Task<IReadOnlyList<float[]>> GenerateAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The fake embedding models do not generate vectors.");
    }
}
