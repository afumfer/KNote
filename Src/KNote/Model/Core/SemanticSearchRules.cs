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

    // The similar notes of a note (delivery 0.5): a short list, from a higher similarity than the search's (two whole
    // notes are compared, not a short text with a note), and the mark of a possible duplicate. Initial values.
    public const float MinSimilarNotes = 0.60f;
    public const int MaxSimilarNotes = 10;
    public const float PossibleDuplicateSimilarity = 0.95f;

    public static bool IsPossibleDuplicate(float similarity) => similarity >= PossibleDuplicateSimilarity;

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
    /// The notes most similar to a note (D1 of delivery 0.5): for each fragment of the note, the most similar fragment of
    /// the other note; the similarity is the mean of those, so what counts is what they have in common as a whole (two
    /// notes are not similar just because one fragment is, e.g. their data of the same folder and type). The fragment
    /// returned is the other note's most similar one. At least minSimilarity, the most similar first; never the note
    /// itself, and only the notes allowed, if given.
    /// </summary>
    public static List<NoteMatch> RankSimilar(IReadOnlyList<NoteChunkVectorDto> noteChunks, IEnumerable<NoteChunkVectorDto> chunks,
        float minSimilarity = MinSimilarNotes, Func<Guid, bool> allowed = null)
    {
        var own = (noteChunks ?? Array.Empty<NoteChunkVectorDto>()).Where(c => c?.Vector != null).ToList();
        if (own.Count == 0)
            return new List<NoteMatch>();
        var noteId = own[0].NoteId;
        var dimensions = own[0].Vector.Length;
        own = own.Where(c => c.Vector.Length == dimensions).ToList();

        var notes = new Dictionary<Guid, SimilarNote>();
        foreach (var chunk in chunks ?? Enumerable.Empty<NoteChunkVectorDto>())
        {
            if (chunk?.Vector == null || chunk.Vector.Length != dimensions || chunk.NoteId == noteId)
                continue;
            if (allowed != null && !allowed(chunk.NoteId))
                continue;

            if (!notes.TryGetValue(chunk.NoteId, out var note))
                notes[chunk.NoteId] = note = new SimilarNote(own.Count);
            for (var i = 0; i < own.Count; i++)
            {
                var similarity = EmbeddingVector.Dot(own[i].Vector, chunk.Vector);
                if (similarity > note.Best[i])
                    note.Best[i] = similarity;
                if (similarity > note.BestChunkSimilarity)
                {
                    note.BestChunkSimilarity = similarity;
                    note.BestChunk = chunk;
                }
            }
        }

        return notes
            .Select(n => new NoteMatch(n.Key, n.Value.Best.Average(), n.Value.BestChunk.NoteChunkId, n.Value.BestChunk.SourceType))
            .Where(m => m.Similarity >= minSimilarity)
            .OrderByDescending(m => m.Similarity)
            .ThenBy(m => m.NoteId)
            .ToList();
    }

    private sealed class SimilarNote
    {
        public SimilarNote(int ownChunks)
        {
            Best = Enumerable.Repeat(-1f, ownChunks).ToArray();
        }

        // For each fragment of the note, its best similarity with this note's fragments.
        public float[] Best { get; }
        public float BestChunkSimilarity { get; set; } = float.MinValue;
        public NoteChunkVectorDto BestChunk { get; set; }
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
