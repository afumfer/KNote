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

        foreach (var (row, cells) in MarkdownTable.Rows(markdown))
        {
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
}
