using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// The measures and the report of a search evaluation (KNote Atlas, chapter 18; D5 of delivery 0.6, extended in 1.0): for
/// each way of searching, in how many test queries an expected note is among the first TopResults results (the criterion
/// of stage 0), how many expected notes are among them, in how many queries one is found at all, its mean position when
/// found, the mean number of results and the mean time; by meaning, also the mean similarity of the first expected note
/// and of the result TopResults (where the first ones end), and what the measures would be with a higher minimum
/// similarity (Cutoffs), to choose it. Per query, the position (and similarity) in each way. The report is Markdown, to
/// show it and to save it as a note.
/// </summary>
public static class SearchEvaluationReport
{
    public const int TopResults = 10;

    // The minimum similarities of the table «If the minimum similarity were…»: from the one of the search on (below it
    // the results were not kept).
    public static readonly IReadOnlyList<float> Cutoffs = new[] { 0.45f, 0.50f, 0.55f, 0.60f }
        .Where(c => c >= SemanticSearchRules.MinSimilarity)
        .ToList();

    public sealed record Summary(string Variant, int Queries, int InTop, int Found, double? MeanPosition, double MeanResults, int Errors,
        int ExpectedInTop, int Expected, double? MeanFirstSimilarity, double? MeanTopSimilarity, double MeanMilliseconds);

    // A higher minimum similarity: in how many queries an expected note would stay among the first TopResults, and the
    // mean number of results.
    public sealed record CutoffSummary(string Variant, float Cutoff, int Queries, int InTop, double MeanResults);

    public static string VariantName(EnumNotesSearchMode mode, string indexKey = null, bool withInstruction = true) =>
        mode == EnumNotesSearchMode.Words
            ? "Words"
            : $"Meaning · {indexKey}{(withInstruction ? "" : " (no instruction)")}";

    /// <summary>The similarity of the first expected note of a run by meaning; null, by words or not found.</summary>
    public static float? FirstExpectedSimilarity(SearchEvaluationRunDto run) =>
        run.Position is int position && run.Similarities != null && run.Similarities.Count >= position
            ? run.Similarities[position - 1]
            : null;

    /// <summary>The similarity of the result TopResults of a run by meaning; null, by words or fewer results.</summary>
    public static float? TopSimilarity(SearchEvaluationRunDto run) =>
        run.Similarities != null && run.Similarities.Count >= TopResults ? run.Similarities[TopResults - 1] : null;

    /// <summary>The measures of each way of searching, in the order they come in the results.</summary>
    public static List<Summary> Summarize(IReadOnlyList<SearchEvaluationQueryResultDto> results)
    {
        return Variants(results).Select(variant =>
        {
            var runs = RunsOf(results, variant);
            var done = runs.Where(r => r.Run.Error == null).ToList();
            var found = done.Where(r => r.Run.Position != null).ToList();
            var firstSimilarities = found.Select(r => FirstExpectedSimilarity(r.Run)).Where(s => s != null).ToList();
            var topSimilarities = done.Select(r => TopSimilarity(r.Run)).Where(s => s != null).ToList();
            return new Summary(variant,
                runs.Count,
                found.Count(r => r.Run.Position <= TopResults),
                found.Count,
                found.Count > 0 ? found.Average(r => r.Run.Position.Value) : null,
                done.Count > 0 ? done.Average(r => r.Run.Results) : 0,
                runs.Count - done.Count,
                done.Sum(r => r.Run.ExpectedInTop),
                done.Sum(r => r.Query.ExpectedNotes.Distinct().Count()),
                firstSimilarities.Count > 0 ? firstSimilarities.Average(s => (double)s.Value) : null,
                topSimilarities.Count > 0 ? topSimilarities.Average(s => (double)s.Value) : null,
                done.Count > 0 ? done.Average(r => r.Run.Milliseconds) : 0);
        }).ToList();
    }

    /// <summary>
    /// The ways by meaning with each minimum similarity of Cutoffs: the results of a query are the ones with at least that
    /// similarity, and an expected note stays among the first ones if its similarity reaches it (the order does not change).
    /// </summary>
    public static List<CutoffSummary> SummarizeCutoffs(IReadOnlyList<SearchEvaluationQueryResultDto> results)
    {
        var summaries = new List<CutoffSummary>();
        foreach (var variant in Variants(results))
        {
            var done = RunsOf(results, variant).Select(r => r.Run).Where(r => r.Error == null && r.Mode == EnumNotesSearchMode.Meaning).ToList();
            if (done.Count == 0)
                continue;
            foreach (var cutoff in Cutoffs)
                summaries.Add(new CutoffSummary(variant, cutoff, done.Count,
                    done.Count(r => r.Position <= TopResults && FirstExpectedSimilarity(r) >= cutoff),
                    done.Average(r => r.Similarities.Count(s => s >= cutoff))));
        }
        return summaries;
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

        text.Append($"| Search | In the first {TopResults} | Expected notes in the first {TopResults} | Found | Mean position | " +
            $"Similarity of the first expected | Similarity of the result {TopResults} | Mean results | Mean time | Errors |\n");
        text.Append("|---|---|---|---|---|---|---|---|---|---|\n");
        foreach (var summary in summaries)
        {
            text.Append($"| {Escape(summary.Variant)} | {summary.InTop} of {summary.Queries} ({Percent(summary.InTop, summary.Queries)}) | " +
                $"{summary.ExpectedInTop} of {summary.Expected} ({Percent(summary.ExpectedInTop, summary.Expected)}) | {summary.Found} | " +
                $"{Number(summary.MeanPosition, "0.0")} | {Number(summary.MeanFirstSimilarity, "0.00")} | {Number(summary.MeanTopSimilarity, "0.00")} | " +
                $"{summary.MeanResults.ToString("0.0", culture)} | {summary.MeanMilliseconds.ToString("0", culture)} ms | {summary.Errors} |\n");
        }
        text.Append($"\nThe similarities are means: of the first expected note, in the queries where it is found; of the result {TopResults}, " +
            $"in the queries with at least {TopResults} results. The time is of the query itself (by meaning, its vector and the ranking).\n");

        var cutoffs = SummarizeCutoffs(results);
        if (cutoffs.Count > 0)
        {
            text.Append("\n## If the minimum similarity were…\n\n");
            text.Append($"Each cell: queries with an expected note in the first {TopResults} · mean results. " +
                $"The search keeps the results from {SemanticSearchRules.MinSimilarity.ToString("0.00", culture)} on.\n\n");
            text.Append("| Search | " + string.Join(" | ", Cutoffs.Select(c => c.ToString("0.00", culture))) + " |\n");
            text.Append("|---|" + string.Concat(Cutoffs.Select(_ => "---|")) + "\n");
            foreach (var variant in cutoffs.GroupBy(c => c.Variant))
            {
                text.Append($"| {Escape(variant.Key)} | ");
                text.Append(string.Join(" | ", variant.Select(c => $"{c.InTop} of {c.Queries} · {c.MeanResults.ToString("0.0", culture)}")));
                text.Append(" |\n");
            }
        }

        text.Append("\n## Queries\n\n");
        text.Append($"Position of the first expected note in each search and, by meaning, its similarity («—» not found, **bold** beyond the first {TopResults}).\n\n");
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

    private static List<string> Variants(IReadOnlyList<SearchEvaluationQueryResultDto> results) =>
        results.SelectMany(r => r.Runs.Select(run => run.Variant)).Distinct().ToList();

    private static List<(SearchTestQueryDto Query, SearchEvaluationRunDto Run)> RunsOf(IReadOnlyList<SearchEvaluationQueryResultDto> results, string variant) =>
        results.Select(r => (r.Query, Run: r.Runs.FirstOrDefault(run => run.Variant == variant))).Where(r => r.Run != null).ToList();

    private static string Cell(SearchEvaluationRunDto run)
    {
        if (run == null)
            return "";
        if (run.Error != null)
            return "error";
        if (run.Position == null)
            return "—";
        var position = run.Position <= TopResults ? run.Position.ToString() : $"**{run.Position}**";
        var similarity = FirstExpectedSimilarity(run);
        return similarity == null ? position : $"{position} ({similarity.Value.ToString("0.00", CultureInfo.InvariantCulture)})";
    }

    private static string Percent(int part, int total) =>
        $"{(total > 0 ? 100.0 * part / total : 0).ToString("0", CultureInfo.InvariantCulture)} %";

    private static string Number(double? value, string format) =>
        value?.ToString(format, CultureInfo.InvariantCulture) ?? "—";

    // In a cell of a Markdown table: its pipes escaped, in one line.
    private static string Escape(string value) =>
        (value ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
}
