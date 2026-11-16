using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The work the semantic index must not repeat (or skip) with a slow model (Ollama without a GPU, whose calls went past the
/// 100 s of HttpClient): a note that fails in the updates before the searches is left by them after MaxFailuresPerProcess
/// failures, but «Update now» (a full run) tries it again and shows its error instead of saying «Done: 0 notes»; a search shown
/// again (BringIndexUpToDate false) does not bring the index up to date; and the same text searched again does not ask the
/// model again (KntQueryVectorCache).
/// </summary>
[TestClass]
public class SemanticIndexRepeatedWorkTests
{
    private const string Query = "keyboard membrane spectrum";

    // KntSemanticIndexer.MaxFailuresPerProcess (internal).
    private const int MaxFailuresPerProcess = 3;

    private static async Task<SemanticSearchResultDto> SearchAsync(SemanticIndexFixture fixture, string text, bool bringIndexUpToDate = true)
    {
        var res = await fixture.Service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = text, BringIndexUpToDate = bringIndexUpToDate });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ANoteThatFailsInTheQuickUpdates_IsLeftByThem_ButAFullRunTriesItAgainAndShowsItsError(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        await fixture.AddNoteAsync("Spectrum", Query);
        await fixture.RunAsync();
        var broken = await fixture.AddNoteAsync("Broken", "A brokenword text.");
        fixture.Models.FailWhen = text => text.Contains("brokenword");

        for (var i = 0; i < MaxFailuresPerProcess; i++)
            await SearchAsync(fixture, $"query {i}");
        fixture.Models.Reset();

        var found = await SearchAsync(fixture, Query);
        Assert.AreEqual(1, fixture.Models.Calls, "only the vector of the query: the quick update leaves the note");
        Assert.AreEqual((1, 1), (found.PendingNotes, found.NotesForFullRun), "the notice says it waits for the full indexing");

        var failing = await fixture.RunAsync();
        Assert.AreEqual(EnumSemanticIndexPhase.Completed, failing.Phase);
        Assert.AreEqual(1, failing.ErrorsCount, "«Update now» tries it again, not «Done: 0 notes» without saying why");
        StringAssert.Contains(failing.Errors.Single(), "refused");

        fixture.Models.FailWhen = null;
        var done = await fixture.RunAsync();
        Assert.AreEqual(1, done.NotesDone);
        Assert.AreEqual(0, done.ErrorsCount);
        Assert.IsFalse(await fixture.IsPendingAsync(broken.NoteId));
    }

    [TestMethod]
    public async Task ASearchShownAgain_DoesNotBringTheIndexUpToDate()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await fixture.AddNoteAsync("Spectrum", Query);
        await fixture.RunAsync();
        var pending = await fixture.AddNoteAsync("Keyboard", "Another keyboard membrane of a Spectrum.");

        var shownAgain = await SearchAsync(fixture, Query, bringIndexUpToDate: false);

        Assert.IsTrue(await fixture.IsPendingAsync(pending.NoteId), "not indexed");
        Assert.AreEqual(1, shownAgain.PendingNotes, "the notice still says it");

        await SearchAsync(fixture, Query);
        Assert.IsFalse(await fixture.IsPendingAsync(pending.NoteId), "a new search brings it up to date");
    }

    [TestMethod]
    public async Task TheSameTextSearchedAgain_DoesNotAskTheModelAgain()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var spectrum = await fixture.AddNoteAsync("Spectrum", Query);
        await fixture.RunAsync();
        fixture.Models.Reset();

        var first = await SearchAsync(fixture, Query);
        var again = await SearchAsync(fixture, Query);
        var other = await SearchAsync(fixture, "garden tomatoes");

        Assert.AreEqual(2, fixture.Models.Calls, "one per text");
        CollectionAssert.AreEqual(new[] { Query, "garden tomatoes" }, fixture.Models.GeneratedTexts);
        Assert.AreEqual(spectrum.NoteId, again.Notes[0].NoteId);
        Assert.AreEqual(first.Notes[0].Similarity, again.Notes[0].Similarity);
        Assert.AreEqual(0, other.Notes.Count);
    }
}
