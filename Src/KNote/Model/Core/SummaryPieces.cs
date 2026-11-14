using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// A piece of what is summarized (KNote Atlas, stage 2, D11): a page of a document, a part of a document without pages or,
/// later (2.5), a note. Label is how the summary cites it ("p. 12", "#123"); null, it is not cited.
/// </summary>
public sealed record SummaryPiece(string Label, string Text);

/// <summary>The pieces of a document (D21): its pages, or parts of it cut by paragraphs when it has no pages.</summary>
public static class SummaryPieces
{
    // The longest piece: a page or a part longer is cut, so that any piece fits in a batch (SummaryBatches) with others.
    public const int MaxPieceLength = 6_000;

    /// <summary>
    /// The pieces of a document's text (DocumentTextRules' format), and the scanned pages left out because they have no text yet
    /// (a scanned page with the text of an OCR layer is read with it). Empty pages are left out.
    /// </summary>
    public static (List<SummaryPiece> Pieces, List<int> MissingPages) FromDocument(string extractedText, bool paged)
    {
        var pieces = new List<SummaryPiece>();
        var missing = new List<int>();

        if (!paged)
        {
            pieces.AddRange(Cut(string.Join("\n\n", DocumentTextRules.Pages(extractedText).Select(p => p.Text))).Select(t => new SummaryPiece(null, t)));
            return (pieces, missing);
        }

        foreach (var page in DocumentTextRules.Pages(extractedText))
        {
            if (string.IsNullOrWhiteSpace(page.Text))
            {
                if (page.NeedsTranscription)
                    missing.Add(page.Number);
                continue;
            }
            pieces.AddRange(Cut(page.Text).Select(t => new SummaryPiece(PageLabel(page.Number), t)));
        }
        return (pieces, missing);
    }

    public static string PageLabel(int page) => $"p. {page}";

    /// <summary>A text in parts of at most MaxPieceLength characters, cut between paragraphs, lines or words.</summary>
    public static List<string> Cut(string text, int maxLength = MaxPieceLength)
    {
        var parts = new List<string>();
        var rest = (text ?? "").Replace("\r\n", "\n").Trim();
        while (rest.Length > maxLength)
        {
            var cut = LastBreak(rest, maxLength, "\n\n");
            if (cut <= 0) cut = LastBreak(rest, maxLength, "\n");
            if (cut <= 0) cut = LastBreak(rest, maxLength, " ");
            if (cut <= 0) cut = maxLength;
            parts.Add(rest[..cut].Trim());
            rest = rest[cut..].Trim();
        }
        if (rest.Length > 0)
            parts.Add(rest);
        return parts;
    }

    // Where the text before a break of this kind is the longest within maxLength (the break goes to the next part).
    private static int LastBreak(string text, int maxLength, string separator) =>
        text.LastIndexOf(separator, Math.Min(maxLength, text.Length - 1), StringComparison.Ordinal);
}
