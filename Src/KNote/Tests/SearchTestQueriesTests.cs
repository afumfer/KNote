using KNote.Model;

namespace KNote.Tests;

/// <summary>SearchTestQueries: the table of the note of the search test queries, and the problems of its rows.</summary>
[TestClass]
public class SearchTestQueriesTests
{
    [TestMethod]
    public void Parse_ReadsTheQueries_TheirExpectedNotes_AndTheWordsToSearch()
    {
        var note = SearchTestQueries.Template +
            "| problemas de arranque del servidor | #512, #377 | arranque servidor |\r\n" +
            "| ordenadores con procesador Z80 | 120 121 |  |\r\n";

        var (queries, issues) = SearchTestQueries.Parse(note);

        Assert.AreEqual(0, issues.Count, string.Join("; ", issues));
        Assert.AreEqual(2, queries.Count);
        Assert.AreEqual(1, queries[0].Row);
        Assert.AreEqual("problemas de arranque del servidor", queries[0].Query);
        CollectionAssert.AreEqual(new[] { 512, 377 }, queries[0].ExpectedNotes);
        Assert.AreEqual("arranque servidor", queries[0].QueryByWords);
        CollectionAssert.AreEqual(new[] { 120, 121 }, queries[1].ExpectedNotes);
        Assert.IsNull(queries[1].WordsQuery);
        Assert.AreEqual("ordenadores con procesador Z80", queries[1].QueryByWords);
    }

    [TestMethod]
    public void Parse_TellsTheProblemsOfTheRows_AndLeavesThemOut()
    {
        var note = "| Query | Expected notes |\n|---|---|\n" +
            "| without notes | |\n" +
            "|  |  |\n" +
            "| | #3 |\n" +
            "| a wrong number | #12, twelve |\n";

        var (queries, issues) = SearchTestQueries.Parse(note);

        Assert.AreEqual(1, queries.Count);
        CollectionAssert.AreEqual(new[] { 12 }, queries[0].ExpectedNotes);
        Assert.AreEqual(4, queries[0].Row, "the empty row counts as a row");
        Assert.AreEqual(3, issues.Count, string.Join("; ", issues));
        StringAssert.Contains(issues[0], "Row 1");
        StringAssert.Contains(issues[1], "Row 3: without a query");
        StringAssert.Contains(issues[2], "«twelve» is not a note number");
    }

    [TestMethod]
    public void Parse_OnlyTheFirstTable_AndEscapedPipesInACell()
    {
        var note = "Intro\n\n| Query | Expected |\n|:--|--:|\n| a \\| b | #1 |\n\nText\n\n| Other | table |\n|---|---|\n| c | #2 |\n";

        var (queries, _) = SearchTestQueries.Parse(note);

        Assert.AreEqual("a | b", queries.Single().Query);
    }

    [TestMethod]
    public void TheTemplate_HasNoQueries()
    {
        var (queries, issues) = SearchTestQueries.Parse(SearchTestQueries.Template);

        Assert.AreEqual(0, queries.Count);
        Assert.AreEqual(0, issues.Count);
    }
}
