using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The similar notes of a note (KNote Atlas 0.5, IKntSemanticSearchService.GetSimilarNotesAsync), with the fake,
/// deterministic embedding model: the notes that talk about the same, never the note itself, the related ones nor the
/// system ones, the possible duplicates, the note brought up to date first and the cases where they can't be calculated.
/// </summary>
[TestClass]
public class SimilarNotesServiceTests
{
    private const string Keyboard = "Replaced the keyboard membrane of the Spectrum and cleaned the keyboard contacts with alcohol.";
    private const string Garden = "Planted tomatoes, lettuce and peppers in the garden beds this spring.";

    private static async Task<SemanticSearchResultDto> SimilarAsync(SemanticIndexFixture fixture, Guid noteId)
    {
        var res = await fixture.Service.SemanticSearch.GetSimilarNotesAsync(noteId);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static List<Guid> Ids(SemanticSearchResultDto found) => found.Notes.Select(n => n.NoteId).ToList();

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Similar_AreTheNotesThatTalkAboutTheSame_NeverItselfNorTheSystemOnes(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        var note = await fixture.AddNoteAsync("Spectrum repair", Keyboard);
        var similar = await fixture.AddNoteAsync("Spectrum keyboard", "Replaced the keyboard membrane of the Spectrum and cleaned the contacts.");
        var garden = await fixture.AddNoteAsync("Garden", Garden);
        var sessionsType = (await fixture.Service.NoteTypes.SaveAsync(new NoteTypeDto { Name = KntConst.ChatSessionsTag })).Entity;
        var session = await fixture.AddNoteAsync("Spectrum repair", Keyboard, noteTypeId: sessionsType.NoteTypeId);
        await fixture.RunAsync();

        var found = await SimilarAsync(fixture, note.NoteId);

        CollectionAssert.Contains(Ids(found), similar.NoteId);
        CollectionAssert.DoesNotContain(Ids(found), note.NoteId);
        CollectionAssert.DoesNotContain(Ids(found), garden.NoteId);
        CollectionAssert.DoesNotContain(Ids(found), session.NoteId);
        Assert.IsTrue(found.Notes.All(n => n.Similarity >= SemanticSearchRules.MinSimilarNotes));
        Assert.IsTrue(found.Notes.Count <= SemanticSearchRules.MaxSimilarNotes);
        Assert.IsFalse(string.IsNullOrEmpty(found.Notes.Single(n => n.NoteId == similar.NoteId).Fragment));
    }

    [TestMethod]
    public async Task ACopyOfTheNote_IsAPossibleDuplicate()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var note = await fixture.AddNoteAsync("Spectrum repair", Keyboard);
        var copy = await fixture.AddNoteAsync("Spectrum repair", Keyboard);
        await fixture.RunAsync();

        var found = await SimilarAsync(fixture, note.NoteId);

        var hit = found.Notes.First();
        Assert.AreEqual(copy.NoteId, hit.NoteId);
        Assert.IsTrue(SemanticSearchRules.IsPossibleDuplicate(hit.Similarity), $"{hit.Similarity}");
    }

    [TestMethod]
    public async Task TheNotesAlreadyRelated_DoNotComeOut_EitherWay()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var note = await fixture.AddNoteAsync("Spectrum repair", Keyboard);
        var copy = await fixture.AddNoteAsync("Spectrum repair", Keyboard);
        await fixture.RunAsync();
        Assert.IsTrue((await fixture.Service.Notes.SaveTraceNoteAsync(new TraceNoteDto { FromId = note.NoteId, ToId = copy.NoteId, Weight = 1.0 })).IsValid);

        CollectionAssert.DoesNotContain(Ids(await SimilarAsync(fixture, note.NoteId)), copy.NoteId);
        CollectionAssert.DoesNotContain(Ids(await SimilarAsync(fixture, copy.NoteId)), note.NoteId);
    }

    [TestMethod]
    public async Task ANoteChanged_IsIndexedAgainFirst()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var note = await fixture.AddNoteAsync("Spectrum repair", Keyboard);
        var keyboard = await fixture.AddNoteAsync("Spectrum repair", Keyboard);
        var garden = await fixture.AddNoteAsync("Garden", Garden);
        await fixture.RunAsync();

        note.Topic = "Garden";
        note.Description = Garden;
        Assert.IsTrue((await fixture.Service.Notes.SaveAsync(note)).IsValid);
        var found = await SimilarAsync(fixture, note.NoteId);

        CollectionAssert.Contains(Ids(found), garden.NoteId);
        CollectionAssert.DoesNotContain(Ids(found), keyboard.NoteId);
        Assert.IsFalse(await fixture.IsPendingAsync(note.NoteId));
    }

    [TestMethod]
    public async Task WithoutIndex_ForANoteThatDoesNotExist_OrIsNotIndexed_SaysWhy()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var note = await fixture.AddNoteAsync("Spectrum repair", Keyboard);

        var withoutIndex = await fixture.Service.SemanticSearch.GetSimilarNotesAsync(note.NoteId);
        StringAssert.Contains(withoutIndex.ErrorMessage, "no semantic index");

        await fixture.RunAsync();
        var unknown = await fixture.Service.SemanticSearch.GetSimilarNotesAsync(Guid.NewGuid());
        StringAssert.Contains(unknown.ErrorMessage, "does not exist");
        Assert.IsFalse((await fixture.Service.SemanticSearch.GetSimilarNotesAsync(Guid.Empty)).IsValid);

        fixture.Models.FailWhen = text => text.Contains("Unindexable");
        var notIndexed = await fixture.AddNoteAsync("Unindexable", "Its vectors can't be calculated.");
        var res = await fixture.Service.SemanticSearch.GetSimilarNotesAsync(notIndexed.NoteId);
        StringAssert.Contains(res.ErrorMessage, "not in the semantic index yet");
    }
}
