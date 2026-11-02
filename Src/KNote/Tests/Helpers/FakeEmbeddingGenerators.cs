using System.Text.RegularExpressions;
using KNote.Service.Core;

namespace KNote.Tests.Helpers;

/// <summary>
/// The embedding models of a test "installation" (IKntService.EmbeddingGenerators), without any AI provider: the
/// models it has, by name, whose generators give a deterministic vector derived from the words of each text (Vector),
/// not normalized. It keeps the texts it was asked for, can fail with some of them (FailWhen) and can wait before
/// answering (BeforeGenerate), for the tests of the semantic index.
/// </summary>
internal sealed class FakeEmbeddingGenerators : IKntEmbeddingGenerators
{
    public const int Dimensions = 64;

    private readonly List<string> _models;

    public FakeEmbeddingGenerators(params string[] models)
    {
        _models = models.ToList();
    }

    public List<string> GeneratedTexts { get; } = new();

    public int Calls { get; private set; }

    public Func<string, bool>? FailWhen { get; set; }

    public Func<IReadOnlyList<string>, CancellationToken, Task>? BeforeGenerate { get; set; }

    public IKntEmbeddingGenerator Find(string model) =>
        _models.FirstOrDefault(m => string.Equals(m, model?.Trim(), StringComparison.OrdinalIgnoreCase)) is string found
            ? new Generator(this, found)
            : null!;

    public IReadOnlyList<string> Models => _models;

    public void Reset()
    {
        GeneratedTexts.Clear();
        Calls = 0;
    }

    // One dimension per word (by a stable hash of it), counted; the first one never zero.
    public static float[] Vector(string text)
    {
        var vector = new float[Dimensions];
        vector[0] = 0.01f;
        foreach (var word in Regex.Split(text.ToLowerInvariant(), @"\W+").Where(w => w.Length > 0))
        {
            uint hash = 2166136261;
            foreach (var c in word)
                hash = (hash ^ c) * 16777619;
            vector[hash % Dimensions] += 1;
        }
        return vector;
    }

    private sealed class Generator : IKntEmbeddingGenerator
    {
        private readonly FakeEmbeddingGenerators _owner;

        public Generator(FakeEmbeddingGenerators owner, string model)
        {
            _owner = owner;
            Model = model;
        }

        public string Model { get; }

        public async Task<IReadOnlyList<float[]>> GenerateAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
        {
            if (_owner.BeforeGenerate != null)
                await _owner.BeforeGenerate(texts, cancellationToken);

            lock (_owner)
            {
                _owner.Calls++;
                if (texts.FirstOrDefault(t => _owner.FailWhen?.Invoke(t) == true) is string failing)
                    throw new InvalidOperationException($"The fake model refused: {failing.Split('\n')[0]}");
                _owner.GeneratedTexts.AddRange(texts);
            }
            return texts.Select(Vector).ToList();
        }
    }
}
