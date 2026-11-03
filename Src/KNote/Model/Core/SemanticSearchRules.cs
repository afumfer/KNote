using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// The rules of the search by meaning (KNote Atlas), common to both apps: the text sent to the embedding model, the
/// ranking of the notes by their best fragment with the minimum similarity, and the fragment shown in the results.
/// The vectors are normalized (EmbeddingVector.Normalize), so the similarity is their dot product (cosine).
/// </summary>
public static class SemanticSearchRules
{
    // D6 of stage 0: initial values, adjusted with the test queries (delivery 0.6).
    public const float MinSimilarity = 0.45f;
    public const int MaxResults = 100;
    public const int MaxFragmentLength = 300;

    public readonly record struct NoteMatch(Guid NoteId, float Similarity, Guid NoteChunkId, NoteChunkSourceType SourceType);

    /// <summary>
    /// The text of a query for the embedding model: with the repository's query instruction, in the format of the
    /// models that use one (Qwen3-Embedding: "Instruct: {instruction}\nQuery:{query}"); without it, the query alone.
    /// </summary>
    public static string QueryText(string instruction, string query)
    {
        query = query?.Trim() ?? "";
        instruction = instruction?.Trim();
        return string.IsNullOrEmpty(instruction) ? query : $"Instruct: {instruction}\nQuery:{query}";
    }

    /// <summary>
    /// The notes whose best fragment is at least minSimilarity similar to the query, the most similar first (the note's id
    /// breaks ties, so the order is always the same). Only the notes allowed, if given.
    /// Fragments of another size than the query's (an index of other dimensions) are not compared.
    /// </summary>
    public static List<NoteMatch> Rank(IEnumerable<NoteChunkVectorDto> chunks, float[] query, float minSimilarity = MinSimilarity,
        Func<Guid, bool> allowed = null)
    {
        ArgumentNullException.ThrowIfNull(query);

        var best = new Dictionary<Guid, NoteMatch>();
        foreach (var chunk in chunks ?? Enumerable.Empty<NoteChunkVectorDto>())
        {
            if (chunk?.Vector == null || chunk.Vector.Length != query.Length)
                continue;
            if (allowed != null && !allowed(chunk.NoteId))
                continue;

            var similarity = EmbeddingVector.Dot(chunk.Vector, query);
            if (similarity < minSimilarity)
                continue;
            if (!best.TryGetValue(chunk.NoteId, out var current) || similarity > current.Similarity)
                best[chunk.NoteId] = new NoteMatch(chunk.NoteId, similarity, chunk.NoteChunkId, chunk.SourceType);
        }

        return best.Values
            .OrderByDescending(m => m.Similarity)
            .ThenBy(m => m.NoteId)
            .ToList();
    }

    /// <summary>
    /// The start of a fragment to show it in a list: in one line (its line breaks and spaces as one space), at most
    /// maxLength characters, cut at the end of a word with "…".
    /// </summary>
    public static string Fragment(string content, int maxLength = MaxFragmentLength)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "";

        var text = new StringBuilder(content.Length);
        var space = false;
        foreach (var c in content.Trim())
        {
            if (char.IsWhiteSpace(c))
                space = true;
            else
            {
                if (space)
                    text.Append(' ');
                text.Append(c);
                space = false;
            }
        }

        if (text.Length <= maxLength)
            return text.ToString();

        var cut = text.ToString(0, maxLength);
        var lastSpace = cut.LastIndexOf(' ');
        if (lastSpace > maxLength / 2)
            cut = cut[..lastSpace];
        return cut.TrimEnd() + "…";
    }
}
