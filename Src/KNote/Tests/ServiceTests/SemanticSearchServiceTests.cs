using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The search by meaning of KNote Atlas through the Service layer (IKntSemanticSearchService.SearchAsync), with the fake,
/// deterministic embedding model (a vector from the words of each text): the notes found and their order, the filter,
/// the index brought up to date before searching, the notes still pending and the searches that cannot be done.
/// </summary>
[TestClass]
public class SemanticSearchServiceTests
{
    private const string Query = "keyboard membrane spectrum";

    private static async Task<SemanticSearchResultDto> SearchAsync(SemanticIndexFixture fixture, string text, NotesFilterDto? filter = null)
    {
        var res = await fixture.Service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = text, Filter = filter });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static List<Guid> Ids(SemanticSearchResultDto found) => found.Notes.Select(n => n.NoteId).ToList();

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Search_FindsTheMostSimilarNotes_WithTheirFragment_AndNeverTheSystemOnes(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        var spectrum = await fixture.AddNoteAsync("Spectrum", "# Repair\n\nReplaced the keyboard membrane of the Spectrum.");
        var garden = await fixture.AddNoteAsync("Garden", "Planted tomatoes and lettuce.");
        var sessionsType = (await fixture.Service.NoteTypes.SaveAsync(new NoteTypeDto { Name = KntConst.ChatSessionsTag })).Entity;
        var session = await fixture.AddNoteAsync("Keyboard membrane", Query, noteTypeId: sessionsType.NoteTypeId);
        await fixture.RunAsync();

        var found = await SearchAsync(fixture, Query);

        Assert.AreEqual(spectrum.NoteId, found.Notes[0].NoteId);
        CollectionAssert.DoesNotContain(Ids(found), garden.NoteId);
        CollectionAssert.DoesNotContain(Ids(found), session.NoteId);
        var first = found.Notes[0];
        Assert.AreEqual("Spectrum", first.Topic);
        Assert.AreEqual(spectrum.NoteNumber, first.NoteNumber);
        Assert.AreEqual(NoteChunkSourceType.Text, first.FragmentSource);
        StringAssert.Contains(first.Fragment, "keyboard membrane");
        Assert.IsTrue(found.Notes.All(n => n.Similarity >= SemanticSearchRules.MinSimilarity));
        CollectionAssert.AreEqual(found.Notes.OrderByDescending(n => n.Similarity).ToList(), found.Notes);
        Assert.AreEqual(0, found.PendingNotes);
    }

    [TestMethod]
    public async Task Search_WithAFilter_FindsOnlyAmongItsNotes()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var folder = (await fixture.Service.Folders.SaveAsync(new FolderDto { Name = "Collection", ParentId = fixture.HomeId })).Entity;
        var inHome = await fixture.AddNoteAsync("Spectrum one", Query);
        var inFolder = await fixture.AddNoteAsync("Spectrum two", Query, folder.FolderId);
        await fixture.RunAsync();

        var all = await SearchAsync(fixture, Query);
        var filtered = await SearchAsync(fixture, Query, new NotesFilterDto { FolderId = folder.FolderId, MeaningText = Query });

        CollectionAssert.IsSubsetOf(new[] { inHome.NoteId, inFolder.NoteId }, Ids(all));
        CollectionAssert.AreEqual(new[] { inFolder.NoteId }, Ids(filtered));
        Assert.AreEqual(Query, SemanticSearchRequestDto.ForFilter(new NotesFilterDto { MeaningText = Query }).Text);
    }

    [TestMethod]
    public async Task Search_BringsThePendingNotesUpToDate_First()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var note = await fixture.AddNoteAsync("Mountain", "Nothing yet.");
        await fixture.RunAsync();
        Assert.AreEqual(0, (await SearchAsync(fixture, "lava eruption volcano")).Notes.Count);

        note.Description = "Lava eruption of the volcano.";
        Assert.IsTrue((await fixture.Service.Notes.SaveAsync(note)).IsValid);
        var found = await SearchAsync(fixture, "lava eruption volcano");

        CollectionAssert.AreEqual(new[] { note.NoteId }, Ids(found));
        StringAssert.Contains(found.Notes[0].Fragment, "volcano");
        Assert.AreEqual(0, found.PendingNotes);
    }

    [TestMethod]
    public async Task Search_SaysHowManyNotesAreStillPending()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await fixture.AddNoteAsync("Spectrum", Query);
        await fixture.RunAsync();
        var pendingBefore = (await fixture.StatusAsync()).PendingNotes;
        for (var i = 0; i < SemanticIndexRefreshRequest.DefaultMaxNotes + 3; i++)
            await fixture.AddNoteAsync($"Filler {i}", "Some text.");

        var found = await SearchAsync(fixture, Query);

        Assert.AreEqual(pendingBefore + 3, found.PendingNotes);
    }

    [TestMethod]
    public async Task Search_ADeletedNote_IsNotFoundAnyMore()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var note = await fixture.AddNoteAsync("Spectrum", Query);
        var other = await fixture.AddNoteAsync("Spectrum too", Query);
        await fixture.RunAsync();
        CollectionAssert.IsSubsetOf(new[] { note.NoteId, other.NoteId }, Ids(await SearchAsync(fixture, Query)));

        Assert.IsTrue((await fixture.Service.Notes.DeleteAsync(note.NoteId)).IsValid);
        var found = await SearchAsync(fixture, Query);

        CollectionAssert.DoesNotContain(Ids(found), note.NoteId);
        CollectionAssert.Contains(Ids(found), other.NoteId);
    }

    [TestMethod]
    public async Task Search_SendsTheQueryWithTheRepositorysInstruction()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await fixture.AddNoteAsync("Spectrum", Query);
        await fixture.RunAsync();
        var res = await fixture.Service.SystemValues.SaveRepositoryAiModelsAsync(new RepositoryAiModelsDto
        {
            EmbeddingModel = SemanticIndexFixture.Model, QueryInstruction = "Find the notes"
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        fixture.Models.Reset();

        await SearchAsync(fixture, Query);

        Assert.AreEqual($"Instruct: Find the notes\nQuery:{Query}", fixture.Models.GeneratedTexts.Single());
    }

    [TestMethod]
    public async Task Search_WithoutIndex_ModelOrGenerator_FailsWithoutSendingAnything()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await fixture.AddNoteAsync("Spectrum", Query);

        var withoutIndex = await fixture.Service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = Query });
        Assert.IsFalse(withoutIndex.IsValid);
        StringAssert.Contains(withoutIndex.ErrorMessage, "no semantic index");
        Assert.AreEqual(0, fixture.Models.Calls, "a search does not create the index");

        await fixture.SetModelAsync("not-here");
        var withoutGenerator = await fixture.Service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = Query });
        Assert.IsFalse(withoutGenerator.IsValid);
        StringAssert.Contains(withoutGenerator.ErrorMessage, "not-here");

        await fixture.SetModelAsync(null!);
        var withoutModel = await fixture.Service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = Query });
        Assert.IsFalse(withoutModel.IsValid);
        StringAssert.Contains(withoutModel.ErrorMessage, "no embedding model");

        var withoutText = await fixture.Service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = " " });
        Assert.IsFalse(withoutText.IsValid);
    }

    [TestMethod]
    public async Task Search_WhenTheModelFails_SaysSo()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await fixture.AddNoteAsync("Spectrum", Query);
        await fixture.RunAsync();
        fixture.Models.FailWhen = text => text == Query;

        var res = await fixture.Service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = Query });

        Assert.IsFalse(res.IsValid);
        StringAssert.Contains(res.ErrorMessage, "could not calculate the vector of the search");
    }
}
