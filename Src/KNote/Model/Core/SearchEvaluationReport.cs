using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// The measures and the report of a search evaluation (KNote Atlas, chapter 18; D5 of delivery 0.6): for each way of
/// searching, in how many test queries an expected note is among the first TopResults results (the criterion of stage
/// 0), in how many it is found at all, its mean position when found and the mean number of results; and, per query,
/// the position in each way. The report is Markdown, to show it and to save it as a note.
/// </summary>
public static class SearchEvaluationReport
{
    public const int TopResults = 10;

    public sealed record Summary(string Variant, int Queries, int InTop, int Found, double? MeanPosition, double MeanResults, int Errors);

    public static string VariantName(EnumNotesSearchMode mode, string indexKey = null, bool withInstruction = true) =>
        mode == EnumNotesSearchMode.Words
            ? "Words"
            : $"Meaning · {indexKey}{(withInstruction ? "" : " (no instruction)")}";

    /// <summary>The measures of each way of searching, in the order they come in the results.</summary>
    public static List<Summary> Summarize(IReadOnlyList<SearchEvaluationQueryResultDto> results)
    {
        var variants = results.SelectMany(r => r.Runs.Select(run => run.Variant)).Distinct().ToList();
        return variants.Select(variant =>
        {
            var runs = results.Select(r => r.Runs.FirstOrDefault(run => run.Variant == variant)).Where(run => run != null).ToList();
            var done = runs.Where(run => run.Error == null).ToList();
            var found = done.Where(run => run.Position != null).ToList();
            return new Summary(variant,
                runs.Count,
                found.Count(run => run.Position <= TopResults),
                found.Count,
                found.Count > 0 ? found.Average(run => run.Position.Value) : null,
                done.Count > 0 ? done.Average(run => run.Results) : 0,
                runs.Count - done.Count);
        }).ToList();
    }

    public static string Markdown(string repositoryName, DateTime date, IReadOnlyList<SearchEvaluationQueryResultDto> results,
        string queryInstruction, bool canceled = false)
    {
        var culture = CultureInfo.InvariantCulture;
        var summaries = Summarize(results);
        var text = new StringBuilder();

        text.Append($"# Search evaluation · {date.ToString("yyyy-MM-dd HH:mm", culture)}\n\n");
        text.Append($"Repository: {repositoryName} · {results.Count} test queries");
        if (canceled)
            text.Append(" (canceled: only these)");
        text.Append($" · a hit is an expected note among the first {TopResults} results.\n\n");
        text.Append(string.IsNullOrWhiteSpace(queryInstruction)
            ? "Query instruction: none.\n\n"
            : $"Query instruction: «{Escape(queryInstruction.Trim())}».\n\n");

        text.Append($"| Search | In the first {TopResults} | Found | Mean position | Mean results | Errors |\n");
        text.Append("|---|---|---|---|---|---|\n");
        foreach (var summary in summaries)
        {
            var percent = summary.Queries > 0 ? 100.0 * summary.InTop / summary.Queries : 0;
            text.Append($"| {Escape(summary.Variant)} | {summary.InTop} of {summary.Queries} ({percent.ToString("0", culture)} %) | {summary.Found} | " +
                $"{(summary.MeanPosition?.ToString("0.0", culture) ?? "—")} | {summary.MeanResults.ToString("0.0", culture)} | {summary.Errors} |\n");
        }

        text.Append("\n## Queries\n\n");
        text.Append($"Position of the first expected note in each search («—» not found, **bold** beyond the first {TopResults}).\n\n");
        text.Append("| Row | Query | Expected | " + string.Join(" | ", summaries.Select(s => Escape(s.Variant))) + " |\n");
        text.Append("|---|---|---|" + string.Concat(summaries.Select(_ => "---|")) + "\n");
        foreach (var result in results)
        {
            var query = result.Query;
            var queryText = Escape(query.Query) + (string.IsNullOrWhiteSpace(query.WordsQuery) ? "" : $" (words: {Escape(query.WordsQuery)})");
            text.Append($"| {query.Row} | {queryText} | {string.Join(", ", query.ExpectedNotes.Select(n => $"#{n}"))} | ");
            text.Append(string.Join(" | ", summaries.Select(s => Cell(result.Runs.FirstOrDefault(run => run.Variant == s.Variant)))));
            text.Append(" |\n");
        }

        var errors = results.SelectMany(r => r.Runs.Where(run => run.Error != null).Select(run => (r.Query.Row, run))).ToList();
        if (errors.Count > 0)
        {
            text.Append("\n## Errors\n\n");
            foreach (var (row, run) in errors)
                text.Append($"- Row {row}, {Escape(run.Variant)}: {Escape(run.Error)}\n");
        }

        return text.ToString();
    }

    private static string Cell(SearchEvaluationRunDto run) =>
        run == null ? ""
        : run.Error != null ? "error"
        : run.Position == null ? "—"
        : run.Position <= TopResults ? run.Position.ToString()
        : $"**{run.Position}**";

    // In a cell of a Markdown table: its pipes escaped, in one line.
    private static string Escape(string value) =>
        (value ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
}
