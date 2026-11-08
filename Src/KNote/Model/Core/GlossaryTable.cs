using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// The glossary of a repository (KNote Atlas, chapter 20): a note with a Markdown table «Term | Meaning | Also written as»
/// (the variants separated by commas). Parse reads it and tells the problems of its rows; Find gives the terms that appear
/// in some texts (the conversation), in the order they first appear, comparing without case nor accents and by whole
/// word, also by their variants (D7: the AI only receives those).
/// </summary>
public static class GlossaryTable
{
    public const int MaxTerms = 30;

    public const string Template =
        "<!-- The glossary of the repository (KNote Atlas): its terms, acronyms and proper names. The AI only receives " +
        "the terms that appear in what it reads. One row per term: the term, what it means and, optionally, other ways " +
        "of writing it separated by commas. Write in your own language. -->\n" +
        "\n" +
        "| Term | Meaning | Also written as |\n" +
        "|---|---|---|\n";

    /// <summary>The terms of the note's table (the first one in the text) and the problems of its rows. Row is from 1, without the header.</summary>
    public static (List<GlossaryTermDto> Terms, List<string> Issues) Parse(string markdown)
    {
        var terms = new List<GlossaryTermDto>();
        var issues = new List<string>();
        var seen = new Dictionary<string, int>();

        foreach (var (row, cells) in MarkdownTable.Rows(markdown))
        {
            var term = cells.ElementAtOrDefault(0) ?? "";
            var meaning = cells.ElementAtOrDefault(1) ?? "";
            var variants = (cells.ElementAtOrDefault(2) ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (term.Length == 0)
            {
                issues.Add($"Row {row}: without a term.");
                continue;
            }
            if (meaning.Length == 0)
            {
                issues.Add($"Row {row}: «{term}» has no meaning.");
                continue;
            }
            var key = Normalize(term);
            if (seen.TryGetValue(key, out var first))
            {
                issues.Add($"Row {row}: «{term}» is repeated (row {first}).");
                continue;
            }
            seen[key] = row;
            terms.Add(new GlossaryTermDto { Row = row, Term = term, Meaning = meaning, AlsoWrittenAs = variants });
        }

        return (terms, issues);
    }

    /// <summary>
    /// The terms that appear in the texts (by their term or a variant), in the order they first appear in them (the texts
    /// in their order: a conversation from its first prompt), at most max. A conversation that grows only adds terms at
    /// the end.
    /// </summary>
    public static List<GlossaryTermDto> Find(IEnumerable<GlossaryTermDto> terms, IEnumerable<string> texts, int max = MaxTerms)
    {
        var text = Normalize(string.Join("\n", texts ?? Enumerable.Empty<string>()));
        if (text.Length == 0)
            return new List<GlossaryTermDto>();

        return terms
            .Select(t => (Term: t, At: new[] { t.Term }.Concat(t.AlsoWrittenAs ?? new List<string>())
                .Select(form => FirstWholeWord(text, Normalize(form)))
                .Where(at => at >= 0)
                .DefaultIfEmpty(-1)
                .Min()))
            .Where(t => t.At >= 0)
            .OrderBy(t => t.At)
            .ThenBy(t => t.Term.Row)
            .Take(max)
            .Select(t => t.Term)
            .ToList();
    }

    // Lower case and without accents (á → a, ñ → n).
    public static string Normalize(string value)
    {
        var decomposed = (value ?? "").Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var text = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                text.Append(c);
        return text.ToString().Normalize(NormalizationForm.FormC);
    }

    // Where the form appears first with no letter or digit just before or after it; -1, nowhere.
    private static int FirstWholeWord(string text, string form)
    {
        if (form.Length == 0)
            return -1;
        var at = text.IndexOf(form, StringComparison.Ordinal);
        while (at >= 0)
        {
            var before = at == 0 || !char.IsLetterOrDigit(text[at - 1]);
            var end = at + form.Length;
            var after = end >= text.Length || !char.IsLetterOrDigit(text[end]);
            if (before && after)
                return at;
            at = text.IndexOf(form, at + 1, StringComparison.Ordinal);
        }
        return -1;
    }
}

/// <summary>
/// The rows of the first Markdown table of a text, as their cells (an escaped pipe, \|, is part of a cell): without the
/// header and the separator rows, nor the empty ones. Row is from 1, without the header.
/// </summary>
public static class MarkdownTable
{
    public static IEnumerable<(int Row, List<string> Cells)> Rows(string markdown)
    {
        var row = 0;
        var inTable = false;
        var headerSeen = false;

        foreach (var rawLine in (markdown ?? "").Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (!line.StartsWith('|'))
            {
                if (inTable)
                    yield break; // the first table only
                continue;
            }
            inTable = true;

            var cells = Cells(line);
            if (!headerSeen)
            {
                headerSeen = true;
                continue;
            }
            if (cells.Count > 0 && cells.All(c => c.Length > 0 && c.All(ch => ch is '-' or ':' or ' ')))
                continue; // the separator row

            row++;
            if (cells.All(c => c.Length == 0))
                continue;
            yield return (row, cells);
        }
    }

    public static List<string> Cells(string line)
    {
        var cells = new List<string>();
        var cell = new StringBuilder();
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '\\' && i + 1 < line.Length && line[i + 1] == '|')
            {
                cell.Append('|');
                i++;
            }
            else if (c == '|')
            {
                cells.Add(cell.ToString().Trim());
                cell.Clear();
            }
            else
                cell.Append(c);
        }
        cells.Add(cell.ToString().Trim());

        // A row starts (and usually ends) with a pipe: the empty cells outside them are not cells.
        if (cells.Count > 0 && cells[0].Length == 0)
            cells.RemoveAt(0);
        if (cells.Count > 0 && cells[^1].Length == 0 && line.EndsWith('|') && !line.EndsWith("\\|"))
            cells.RemoveAt(cells.Count - 1);
        return cells;
    }
}
