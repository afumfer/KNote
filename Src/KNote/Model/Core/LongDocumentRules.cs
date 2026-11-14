using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// The texts of the summary of a long document (KNote Atlas, stage 2, CU-43, D21): what each call asks the model (a batch of
/// pages, a join of notes, the final summary, with the language last, D14) and the note that keeps it. In English, as KNote's
/// interface; the summary is written in the language asked for.
/// </summary>
public static class LongDocumentRules
{
    public const string NotesSeparator = "\n\n---\n\n";

    /// <summary>The notes of a batch of pieces of the document.</summary>
    public static string BatchRequest(string fileName, IReadOnlyList<SummaryPiece> pieces)
    {
        var cited = pieces.Any(p => p.Label != null);
        var text = new StringBuilder();
        text.Append($"Read this part of the document «{fileName}»{Span(pieces)}. Write notes of what it says: its subjects, facts, names, ");
        text.Append($"figures, steps and conclusions, as a list of at most {SummaryBatches.NotesWords} words.");
        if (cited)
            text.Append(" End each note with the page it comes from, as (p. 12).");
        text.Append("\n\n");
        text.Append(string.Join("\n\n", pieces.Select(p => p.Label == null ? p.Text : $"[{p.Label}]\n{p.Text}")));
        return text.ToString();
    }

    /// <summary>Notes of consecutive parts joined in one list, when they don't fit together in the final call.</summary>
    public static string JoinRequest(string fileName, IReadOnlyList<string> notes, bool paged) =>
        $"These are notes of consecutive parts of the document «{fileName}». Join them in one list of notes of at most " +
        $"{SummaryBatches.NotesWords} words, keeping what is essential{(paged ? " and the pages they cite" : "")}." +
        "\n\n" + string.Join(NotesSeparator, notes);

    /// <summary>The summary of the whole document, from the notes of all its parts.</summary>
    public static string FinalRequest(string fileName, int pages, bool paged, IReadOnlyList<string> notes, EnumSummaryLength length, string language)
    {
        var text = new StringBuilder();
        text.Append($"These are notes of the whole document «{fileName}»{(paged ? $" ({pages} pages)" : "")}. Write its summary from them: ");
        text.Append($"about {(int)length} words, in Markdown. Start with a short paragraph that says what kind of document it is and what ");
        text.Append("it is about; then its main contents, with headings if they help.");
        if (paged)
            text.Append(" Keep the page citations, as (p. 12).");
        text.Append("\n\n");
        text.Append(string.Join(NotesSeparator, notes));
        text.Append("\n\n");
        text.Append(string.IsNullOrWhiteSpace(language)
            ? "Write the summary in the language of the document."
            : $"Write the summary in {language.Trim()}.");
        return text.ToString();
    }

    /// <summary>The topic of the note with the summary.</summary>
    public static string NoteTopic(string fileName) => $"Summary of {fileName}";

    /// <summary>
    /// The text of the note with the summary: a line with where it comes from (the note of the document, cited, and its pages),
    /// the model and the date; the scanned pages left out, if any; and the summary.
    /// </summary>
    public static string NoteDescription(DocumentToSummarizeDto document, DocumentSummarySaveDto summary, DateTime date)
    {
        var header = new StringBuilder();
        header.Append($"> Summary of the document **{document.FileName}** of the note #{document.NoteNumber}");
        if (document.Paged)
            header.Append($", {document.PagesCount} pages");
        header.Append($" · {summary.Model} · {date:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(summary.Language))
            header.Append($" · {summary.Language.Trim()}");
        if (summary.MissingPages?.Count > 0)
            header.Append($"\n>\n> Scanned pages not read, without text yet: {Ranges(summary.MissingPages)}.");

        return $"{header}\n\n{summary.Summary?.Trim()}";
    }

    /// <summary>Page numbers as ranges: 3, 7–9.</summary>
    public static string Ranges(IEnumerable<int> pages)
    {
        var sorted = (pages ?? Enumerable.Empty<int>()).Distinct().OrderBy(p => p).ToList();
        var ranges = new List<string>();
        for (var i = 0; i < sorted.Count; i++)
        {
            var start = sorted[i];
            while (i + 1 < sorted.Count && sorted[i + 1] == sorted[i] + 1)
                i++;
            ranges.Add(start == sorted[i] ? $"{start}" : $"{start}–{sorted[i]}");
        }
        return string.Join(", ", ranges);
    }

    /// <summary>The name of the resource with the summary as the AI wrote it (D17), and its description.</summary>
    public static string OriginalFileName(string fileName) => $"Summary of {fileName} (original).md";

    public static string OriginalDescription(string model, DateTime date) =>
        $"The summary as the AI wrote it ({model}, {date:yyyy-MM-dd}), before anybody changed it.";

    // The pages of a batch, as « (pages 3–9)», or nothing without pages.
    private static string Span(IReadOnlyList<SummaryPiece> pieces)
    {
        var labels = pieces.Select(p => p.Label).Where(l => l != null).Distinct().ToList();
        if (labels.Count == 0)
            return "";
        return labels.Count == 1 ? $" ({labels[0]})" : $" ({labels[0]} to {labels[^1]})";
    }
}
