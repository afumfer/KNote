using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>SearchEvaluationReport: the measures of each way of searching and the report of an evaluation.</summary>
[TestClass]
public class SearchEvaluationReportTests
{
    private const string Meaning = "Meaning · fake@64";

    private static SearchEvaluationQueryResultDto Result(int row, int? wordsPosition, int wordsResults, int? meaningPosition, string meaningError = null) => new()
    {
        Query = new SearchTestQueryDto { Row = row, Query = $"query | {row}", ExpectedNotes = new() { row } },
        Runs =
        {
            new SearchEvaluationRunDto { Variant = "Words", Mode = EnumNotesSearchMode.Words, Position = wordsPosition, Results = wordsResults },
            new SearchEvaluationRunDto { Variant = Meaning, Mode = EnumNotesSearchMode.Meaning, IndexKey = "fake@64", Position = meaningPosition, Results = 100, Error = meaningError }
        }
    };

    private static readonly List<SearchEvaluationQueryResultDto> Results = new()
    {
        Result(1, wordsPosition: 1, wordsResults: 3, meaningPosition: 2),
        Result(2, wordsPosition: 15, wordsResults: 40, meaningPosition: 1),
        Result(3, wordsPosition: null, wordsResults: 0, meaningPosition: null, meaningError: "The model did not answer.")
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

        var meaning = summaries[1];
        Assert.AreEqual(2, meaning.InTop);
        Assert.AreEqual(1, meaning.Errors);
        Assert.AreEqual(1.5, meaning.MeanPosition!.Value, 1e-9);
    }

    [TestMethod]
    public void Markdown_HasTheSummary_ThePositionOfEachQuery_AndTheErrors()
    {
        var report = SearchEvaluationReport.Markdown("Public", new DateTime(2026, 11, 3, 10, 30, 0), Results, "Find the notes");

        StringAssert.StartsWith(report, "# Search evaluation · 2026-11-03 10:30");
        StringAssert.Contains(report, "Repository: Public · 3 test queries");
        StringAssert.Contains(report, "Query instruction: «Find the notes».");
        StringAssert.Contains(report, "| Words | 1 of 3 (33 %) | 2 | 8.0 | 14.3 | 0 |");
        StringAssert.Contains(report, $"| {Meaning} | 2 of 3 (67 %) | 2 | 1.5 | 100.0 | 1 |");
        StringAssert.Contains(report, "| 2 | query \\| 2 | #2 | **15** | 1 |");
        StringAssert.Contains(report, "| 3 | query \\| 3 | #3 | — | error |");
        StringAssert.Contains(report, $"- Row 3, {Meaning}: The model did not answer.");
    }
}
