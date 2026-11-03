using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// The note of the test queries of the search (KNote Atlas, chapter 18): a Markdown table with a row per query, the notes
/// that should be found («#123, #456») and, optionally, the words to search by words. Parse reads it and tells the
/// problems of its rows; Template is the note's first content.
/// </summary>
public static class SearchTestQueries
{
    public const string Template =
        "# Search test queries\n" +
        "\n" +
        "The searches that measure the search by meaning (Search evaluation, KNote Atlas). One row per search: the search " +
        "as you would write it, the notes it should find (their numbers: `#123, #456`) and, optionally, the words you " +
        "would search by words (empty: the search itself). Write searches you really do, with your own words, without " +
        "looking at the notes.\n" +
        "\n" +
        "| Query | Expected notes | Words (optional) |\n" +
        "|---|---|---|\n";

    private static readonly Regex NoteNumber = new(@"^#?(\d+)$", RegexOptions.Compiled);

    /// <summary>
    /// The queries of the note's table (the first one in the text) and the problems of its rows: without expected notes,
    /// or with something that is not a note number. Empty rows are left out. Row is the number of the row in the table
    /// (from 1, without the header).
    /// </summary>
    public static (List<SearchTestQueryDto> Queries, List<string> Issues) Parse(string markdown)
    {
        var queries = new List<SearchTestQueryDto>();
        var issues = new List<string>();
        var row = 0;
        var inTable = false;
        var headerSeen = false;

        foreach (var rawLine in (markdown ?? "").Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (!line.StartsWith('|'))
            {
                if (inTable)
                    break; // the first table only
                continue;
            }
            inTable = true;

            var cells = Cells(line);
            if (!headerSeen)
            {
                headerSeen = true; // the header row
                continue;
            }
            if (cells.Count > 0 && cells.All(c => c.Length > 0 && c.All(ch => ch is '-' or ':' or ' ')))
                continue; // the separator row

            row++;
            if (cells.All(c => c.Length == 0))
                continue; // an empty row
            var query = cells.ElementAtOrDefault(0) ?? "";
            var expected = cells.ElementAtOrDefault(1) ?? "";
            var words = cells.ElementAtOrDefault(2) ?? "";
            if (query.Length == 0)
            {
                issues.Add($"Row {row}: without a query.");
                continue;
            }

            var numbers = new List<int>();
            foreach (var token in expected.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var match = NoteNumber.Match(token.Trim());
                if (match.Success && int.TryParse(match.Groups[1].Value, out var number) && number > 0)
                {
                    if (!numbers.Contains(number))
                        numbers.Add(number);
                }
                else
                    issues.Add($"Row {row}: «{token.Trim()}» is not a note number (#123).");
            }
            if (numbers.Count == 0)
            {
                issues.Add($"Row {row}: «{query}» has no expected notes.");
                continue;
            }

            queries.Add(new SearchTestQueryDto
            {
                Row = row,
                Query = query,
                ExpectedNotes = numbers,
                WordsQuery = words.Length == 0 ? null : words
            });
        }

        return (queries, issues);
    }

    // The cells of a table row: between its pipes (an escaped one, \|, is part of a cell).
    private static List<string> Cells(string line)
    {
        var cells = new List<string>();
        var cell = new System.Text.StringBuilder();
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
