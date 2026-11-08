using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// SearchEvaluationReport: the measures of each way of searching (also the similarities, the expected notes among the
/// first ones and the time), the table of higher minimum similarities and the report of an evaluation.
/// </summary>
[TestClass]
public class SearchEvaluationReportTests
{
    private const string Meaning = "Meaning · fake@64";

    // 12 results: the expected note #5 second (0.70) and the result 10 at 0.50.
    private static readonly List<float> ManyResults = new() { 0.80f, 0.70f, 0.65f, 0.62f, 0.60f, 0.58f, 0.56f, 0.54f, 0.52f, 0.50f, 0.48f, 0.46f };

    private static SearchEvaluationQueryResultDto Result(int row, List<int> expected,
        int? wordsPosition, int wordsResults, int wordsInTop, int wordsMs,
        int? meaningPosition, List<float> similarities, int meaningInTop, int meaningMs, string? meaningError = null) => new()
    {
        Query = new SearchTestQueryDto { Row = row, Query = $"query | {row}", ExpectedNotes = expected },
        Runs =
        {
            new SearchEvaluationRunDto { Variant = "Words", Mode = EnumNotesSearchMode.Words, Position = wordsPosition, Results = wordsResults,
                ExpectedInTop = wordsInTop, Milliseconds = wordsMs },
            new SearchEvaluationRunDto { Variant = Meaning, Mode = EnumNotesSearchMode.Meaning, IndexKey = "fake@64", Position = meaningPosition,
                Results = similarities.Count, Similarities = similarities, ExpectedInTop = meaningInTop, Milliseconds = meaningMs, Error = meaningError }
        }
    };

    private static readonly List<SearchEvaluationQueryResultDto> Results = new()
    {
        Result(1, new() { 1, 5 }, wordsPosition: 1, wordsResults: 3, wordsInTop: 1, wordsMs: 10,
            meaningPosition: 2, ManyResults, meaningInTop: 2, meaningMs: 30),
        Result(2, new() { 2 }, wordsPosition: 15, wordsResults: 40, wordsInTop: 0, wordsMs: 20,
            meaningPosition: 1, new() { 0.52f, 0.50f, 0.47f }, meaningInTop: 1, meaningMs: 50),
        Result(3, new() { 3 }, wordsPosition: null, wordsResults: 0, wordsInTop: 0, wordsMs: 30,
            meaningPosition: null, new(), meaningInTop: 0, meaningMs: 0, meaningError: "The model did not answer.")
    };

    [TestMethod]
    public void VariantNames()
    {
        Assert.AreEqual("Words", SearchEvaluationReport.VariantName(EnumNotesSearchMode.Words));
        Assert.AreEqual("Meaning · m@64", SearchEvaluationReport.VariantName(EnumNotesSearchMode.Meaning, "m@64"));
        Assert.AreEqual("Meaning · m (no instruction)", SearchEvaluationReport.VariantName(EnumNotesSearchMode.Meaning, "m", withInstruction: false));
    }

    [TestMethod]
    public void Summarize_CountsTheHitsInTheFirst10_TheNotesFound_AndTheErrorsApart()
    {
        var summaries = SearchEvaluationReport.Summarize(Results);

        var words = summaries[0];
        Assert.AreEqual("Words", words.Variant);
        Assert.AreEqual(3, words.Queries);
        Assert.AreEqual(1, words.InTop);
        Assert.AreEqual(2, words.Found);
        Assert.AreEqual(8.0, words.MeanPosition!.Value, 1e-9);
        Assert.AreEqual(43 / 3.0, words.MeanResults, 1e-9);
        Assert.IsNull(words.MeanFirstSimilarity, "by words there are no similarities");

        var meaning = summaries[1];
        Assert.AreEqual(2, meaning.InTop);
        Assert.AreEqual(1, meaning.Errors);
        Assert.AreEqual(1.5, meaning.MeanPosition!.Value, 1e-9);
    }

    [TestMethod]
    public void Summarize_GivesTheSimilarities_TheExpectedNotesInTheFirst10_AndTheTime()
    {
        var summaries = SearchEvaluationReport.Summarize(Results);

        var words = summaries[0];
        Assert.AreEqual(1, words.ExpectedInTop);
        Assert.AreEqual(4, words.Expected, "every expected note of the queries done");
        Assert.AreEqual(20.0, words.MeanMilliseconds, 1e-9);

        var meaning = summaries[1];
        Assert.AreEqual(3, meaning.ExpectedInTop);
        Assert.AreEqual(3, meaning.Expected, "the query with an error does not count");
        Assert.AreEqual(0.61, meaning.MeanFirstSimilarity!.Value, 1e-6, "(0.70 + 0.52) / 2");
        Assert.AreEqual(0.50, meaning.MeanTopSimilarity!.Value, 1e-6, "only the query with 10 results or more");
        Assert.AreEqual(40.0, meaning.MeanMilliseconds, 1e-9);
    }

    [TestMethod]
    public void FirstExpectedSimilarity_IsTheOneAtItsPosition()
    {
        Assert.AreEqual(0.70f, SearchEvaluationReport.FirstExpectedSimilarity(Results[0].Runs[1]));
        Assert.AreEqual(0.50f, SearchEvaluationReport.TopSimilarity(Results[0].Runs[1]));
        Assert.IsNull(SearchEvaluationReport.FirstExpectedSimilarity(Results[0].Runs[0]), "by words");
        Assert.IsNull(SearchEvaluationReport.TopSimilarity(Results[1].Runs[1]), "fewer than 10 results");
    }

    [TestMethod]
    public void SummarizeCutoffs_KeepsTheResultsThatReachEachMinimum_OnlyByMeaning()
    {
        var cutoffs = SearchEvaluationReport.SummarizeCutoffs(Results);

        Assert.IsTrue(cutoffs.All(c => c.Variant == Meaning));
        CollectionAssert.AreEqual(new[] { 0.45f, 0.50f, 0.55f, 0.60f }, cutoffs.Select(c => c.Cutoff).ToList());
        Assert.IsTrue(cutoffs.All(c => c.Queries == 2), "the query with an error does not count");
        CollectionAssert.AreEqual(new[] { 2, 2, 1, 1 }, cutoffs.Select(c => c.InTop).ToList(), "query 2 is lost from 0.55 (its note at 0.52)");
        CollectionAssert.AreEqual(new[] { 7.5, 6.0, 3.5, 2.5 }, cutoffs.Select(c => c.MeanResults).ToList());
    }

    [TestMethod]
    public void Markdown_HasTheSummary_TheCutoffs_ThePositionOfEachQuery_AndTheErrors()
    {
        var report = SearchEvaluationReport.Markdown("Public", new DateTime(2026, 11, 3, 10, 30, 0), Results, "Find the notes");

        StringAssert.StartsWith(report, "# Search evaluation · 2026-11-03 10:30");
        StringAssert.Contains(report, "Repository: Public · 3 test queries");
        StringAssert.Contains(report, "Query instruction: «Find the notes».");
        StringAssert.Contains(report, "| Words | 1 of 3 (33 %) | 1 of 4 (25 %) | 2 | 8.0 | — | — | 14.3 | 20 ms | 0 |");
        StringAssert.Contains(report, $"| {Meaning} | 2 of 3 (67 %) | 3 of 3 (100 %) | 2 | 1.5 | 0.61 | 0.50 | 7.5 | 40 ms | 1 |");
        StringAssert.Contains(report, "| Search | 0.45 | 0.50 | 0.55 | 0.60 |");
        StringAssert.Contains(report, $"| {Meaning} | 2 of 2 · 7.5 | 2 of 2 · 6.0 | 1 of 2 · 3.5 | 1 of 2 · 2.5 |");
        StringAssert.Contains(report, "| 1 | query \\| 1 | #1, #5 | 1 | 2 (0.70) |");
        StringAssert.Contains(report, "| 2 | query \\| 2 | #2 | **15** | 1 (0.52) |");
        StringAssert.Contains(report, "| 3 | query \\| 3 | #3 | — | error |");
        StringAssert.Contains(report, $"- Row 3, {Meaning}: The model did not answer.");
    }

    [TestMethod]
    public void Markdown_ByWordsOnly_HasNoTableOfCutoffs()
    {
        var wordsOnly = Results.Select(r => new SearchEvaluationQueryResultDto { Query = r.Query, Runs = { r.Runs[0] } }).ToList();

        var report = SearchEvaluationReport.Markdown("Public", DateTime.Now, wordsOnly, null);

        Assert.IsFalse(report.Contains("If the minimum similarity were"));
    }
}
