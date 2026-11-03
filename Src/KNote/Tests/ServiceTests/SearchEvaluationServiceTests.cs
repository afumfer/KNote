using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The search evaluation of KNote Atlas (0.6) through the Service layer, with the fake embedding model: the note of the
/// test queries (created once, out of the searches), the indexes to evaluate, a query by words and by meaning (also
/// without the instruction, and in an index whose model is not here) and the report saved as a note.
/// </summary>
[TestClass]
public class SearchEvaluationServiceTests
{
    private const string Keyboard = "Replaced the keyboard membrane of the Spectrum and cleaned the contacts.";

    private static async Task<SearchEvaluationSetupDto> SetupAsync(SemanticIndexFixture fixture)
    {
        var res = await fixture.Service.SemanticSearch.GetSearchEvaluationAsync();
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static async Task WriteQueriesAsync(SemanticIndexFixture fixture, Guid noteId, string rows)
    {
        var note = (await fixture.Service.Notes.GetAsync(noteId)).Entity;
        note.Description = SearchTestQueries.Template + rows;
        Assert.IsTrue((await fixture.Service.Notes.SaveAsync(note)).IsValid);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheNoteOfTheTestQueries_IsCreatedOnce_AndTheirProblemsAreTold(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        var note = await fixture.AddNoteAsync("Spectrum repair", Keyboard);
        await fixture.RunAsync();

        var first = await SetupAsync(fixture);
        await WriteQueriesAsync(fixture, first.TestQueriesNoteId, $"| keyboard membrane | #{note.NoteNumber}, #9999 | membrane |\n| nothing | |\n");
        var second = await SetupAsync(fixture);

        Assert.AreEqual(first.TestQueriesNoteId, second.TestQueriesNoteId);
        Assert.AreEqual(1, second.Queries.Count);
        Assert.AreEqual("membrane", second.Queries[0].QueryByWords);
        Assert.IsTrue(second.Issues.Any(i => i.Contains("#9999 does not exist")), string.Join("; ", second.Issues));
        Assert.IsTrue(second.Issues.Any(i => i.Contains("has no expected notes")));
        var index = second.Indexes.Single();
        Assert.AreEqual(SemanticIndexFixture.Model, index.IndexKey);
        Assert.IsTrue(index.IsCurrent);
        Assert.IsTrue(index.Available);
        var folders = (await fixture.Service.Folders.GetAllAsync()).Entity;
        Assert.AreEqual(1, folders.Count(f => f.Name == KntConst.AtlasFolderName));
    }

    [TestMethod]
    public async Task AQuery_IsEvaluatedByWords_AndByMeaning_WithAndWithoutTheInstruction()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var note = await fixture.AddNoteAsync("Spectrum repair", Keyboard);
        await fixture.AddNoteAsync("Garden", "Planted tomatoes and lettuce.");
        await fixture.RunAsync();
        Assert.IsTrue((await fixture.Service.SystemValues.SaveRepositoryAiModelsAsync(new RepositoryAiModelsDto
        {
            EmbeddingModel = SemanticIndexFixture.Model, QueryInstruction = "Find the notes"
        })).IsValid);
        var setup = await SetupAsync(fixture);
        await WriteQueriesAsync(fixture, setup.TestQueriesNoteId, $"| keyboard membrane spectrum | #{note.NoteNumber} | membrane |\n");
        var query = (await SetupAsync(fixture)).Queries.Single();

        var res = await fixture.Service.SemanticSearch.EvaluateQueryAsync(new SearchEvaluationQueryRequestDto
        {
            Query = query,
            IndexKeys = { SemanticIndexFixture.Model, "not-here@32" },
            AlsoWithoutInstruction = true
        });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var runs = res.Entity.Runs;
        CollectionAssert.AreEqual(new[]
        {
            "Words",
            $"Meaning · {SemanticIndexFixture.Model}", $"Meaning · {SemanticIndexFixture.Model} (no instruction)",
            "Meaning · not-here@32", "Meaning · not-here@32 (no instruction)"
        }, runs.Select(r => r.Variant).ToList());
        Assert.AreEqual(1, runs[0].Position, "by words, «membrane» finds it (the test queries note is left out)");
        Assert.AreEqual(1, runs[0].Results);
        Assert.AreEqual(1, runs[1].Position);
        Assert.AreEqual(1, runs[2].Position);
        StringAssert.Contains(runs[3].Error, "not-here");
        Assert.IsTrue(fixture.Models.GeneratedTexts.Contains($"Instruct: Find the notes\nQuery:{query.Query}"));
        Assert.IsTrue(fixture.Models.GeneratedTexts.Contains(query.Query));
    }

    [TestMethod]
    public async Task TheReport_IsSavedAsANote_OutOfTheSearches()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await fixture.AddNoteAsync("Spectrum repair", Keyboard);
        await fixture.RunAsync();

        var res = await fixture.Service.SemanticSearch.SaveSearchEvaluationReportAsync(new SearchEvaluationReportDto
        {
            Markdown = "# Search evaluation\n\nkeyboard membrane spectrum"
        });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        StringAssert.StartsWith(res.Entity.Topic, "Search evaluation · ");
        var found = await fixture.Service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = "keyboard membrane spectrum" });
        CollectionAssert.DoesNotContain(found.Entity.Notes.Select(n => n.NoteId).ToList(), res.Entity.NoteId);
        Assert.IsFalse((await fixture.Service.SemanticSearch.SaveSearchEvaluationReportAsync(new SearchEvaluationReportDto())).IsValid);
    }
}
