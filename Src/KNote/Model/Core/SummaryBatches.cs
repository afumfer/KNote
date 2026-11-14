using System;
using System.Collections.Generic;
using System.Linq;

namespace KNote.Model.Core;

/// <summary>
/// How what is summarized is read in calls that fit in the model's context (KNote Atlas, stage 2, D11): the pieces in order, in
/// batches; the notes of the batches joined in batches again while they don't fit together in the final call («summaries of
/// summaries»). Sizes in characters, about four per token.
/// </summary>
public static class SummaryBatches
{
    public const int CharactersPerToken = 4;

    // Around the pieces of a call: the request's own text and the labels.
    public const int RequestOverheadTokens = 300;

    // The words of the notes of a batch (LongDocumentRules.BatchRequest) and the most tokens the model may write for them.
    public const int NotesWords = 250;
    public const int NotesOutputTokens = 800;

    // The smallest part of a call left for what is read: with less, the context is too small to summarize.
    public const int MinInputCharacters = 4_000;

    /// <summary>The characters of the pieces that fit in a call, with its system prompt and the tokens kept for the answer.</summary>
    public static int InputCharacters(int contextTokens, int systemPromptCharacters, int outputTokens) =>
        (contextTokens - outputTokens - RequestOverheadTokens) * CharactersPerToken - systemPromptCharacters;

    /// <summary>The tokens a final text of these words may take (about 1.5 per word, and some more for its Markdown).</summary>
    public static int OutputTokens(int words) => words * 2 + 200;

    /// <summary>
    /// The pieces in batches of at most maxCharacters, in order. A piece is never cut (SummaryPieces already cuts them): a
    /// longer one goes alone.
    /// </summary>
    public static List<List<T>> Group<T>(IReadOnlyList<T> pieces, Func<T, int> length, int maxCharacters)
    {
        var batches = new List<List<T>>();
        var batch = new List<T>();
        var size = 0;
        foreach (var piece in pieces ?? Array.Empty<T>())
        {
            var pieceLength = length(piece);
            if (batch.Count > 0 && size + pieceLength > maxCharacters)
            {
                batches.Add(batch);
                batch = new List<T>();
                size = 0;
            }
            batch.Add(piece);
            size += pieceLength;
        }
        if (batch.Count > 0)
            batches.Add(batch);
        return batches;
    }

    /// <summary>What a piece takes in a batch: its text, its label ("[p. 12]" and a line) and the line between pieces.</summary>
    public static int PieceLength(SummaryPiece piece) => piece.Text.Length + (piece.Label == null ? 0 : piece.Label.Length + 3) + 2;

    /// <summary>Whether these notes fit together in one call.</summary>
    public static bool Fit(IEnumerable<string> notes, int maxCharacters) => (notes ?? Enumerable.Empty<string>()).Sum(n => n?.Length ?? 0) <= maxCharacters;
}
