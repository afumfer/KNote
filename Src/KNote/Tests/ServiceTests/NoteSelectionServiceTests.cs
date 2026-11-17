using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The selection of the notes of a summary and «Note relevance» (KNote Atlas, stage 2, 2.3, CU-15, D6, D8, D10, S3-S7), with a
/// repository: the notes of the period, of a folder and its subfolders, with some words and about something; each one decided with
/// the noise policy, the tags, the citations between them and their size; KNote's own notes only counted; the corrections kept in
/// «Note relevance» by a Staff (not by hand), above the policy, taken away when they are again KNote's, and the rows of the notes
/// deleted taken away. Nothing of this changes the people's notes.
/// </summary>
[TestClass]
public class NoteSelectionServiceTests
{
    private static readonly DateTime From = new(2020, 3, 9);
    private static readonly DateTime To = new(2020, 3, 15);
    private static readonly DateTime InPeriod = new(2020, 3, 10, 12, 0, 0);

    private static string Words(int count, string word = "word") => string.Join(" ", Enumerable.Range(1, count).Select(i => $"{word}{i}"));

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheSelection_OfThePeriod_ByFolderAndWords(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        var projects = await AddFolderAsync(fixture, "Projects");
        var ql = await AddFolderAsync(fixture, "QL", projects);
        var review = await AddNoteAsync(fixture, "Sinclair QL review", Words(45), ql, InPeriod, InPeriod.AddDays(-30));
        var tiny = await AddNoteAsync(fixture, "QL", "buy", fixture.HomeId, InPeriod, InPeriod);
        var budget = await AddNoteAsync(fixture, "Budget", Words(20), projects, InPeriod, InPeriod.AddDays(-2));
        var old = await AddNoteAsync(fixture, "Old", Words(50), projects, new DateTime(2020, 1, 1));

        var all = await SelectAsync(fixture, new NoteSelectionRequestDto { From = From, To = To });
        var inProjects = await SelectAsync(fixture, new NoteSelectionRequestDto { From = From, To = To, FolderId = projects });
        var aboutQl = await SelectAsync(fixture, new NoteSelectionRequestDto { From = From, To = To, Keywords = "QL" });

        CollectionAssert.AreEquivalent(new[] { review, tiny, budget }, Ids(all));
        Assert.AreEqual(EnumSelectionDecision.In, Note(all, review).Decision);
        Assert.AreEqual("In: 48 words", Note(all, review).Reason);
        Assert.AreEqual(EnumSelectionDecision.Out, Note(all, tiny).Decision);
        Assert.AreEqual(EnumSelectionDecision.Doubtful, Note(all, budget).Decision);
        Assert.AreEqual("QL", Note(all, review).FolderName);
        CollectionAssert.AreEquivalent(new[] { review, budget }, Ids(inProjects), "the folder and its subfolders");
        CollectionAssert.AreEquivalent(new[] { review, tiny }, Ids(aboutQl));
        Assert.IsFalse(Ids(all).Contains(old));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ThePolicy_TheTags_TheCitations_AndKNotesOwnNotes(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        var archive = await AddFolderAsync(fixture, "Archive");
        var cited = await AddNoteAsync(fixture, "Decision", "short text", fixture.HomeId, InPeriod, InPeriod);
        var citedNumber = (await fixture.Service.Notes.GetAsync(cited)).Entity.NoteNumber;
        var citing = await AddNoteAsync(fixture, "Minutes", $"{Words(50)} as decided in #{citedNumber}.", fixture.HomeId, InPeriod);
        var tagged = await AddNoteAsync(fixture, "Draft", Words(60), fixture.HomeId, InPeriod, tags: "draft #NoKnowledge");
        var archived = await AddNoteAsync(fixture, "Archived", Words(60), archive, InPeriod);
        var sessions = (await fixture.Service.NoteTypes.SaveAsync(new NoteTypeDto { Name = KntConst.ChatSessionsTag })).Entity.NoteTypeId;
        await AddNoteAsync(fixture, "A conversation", Words(60), fixture.HomeId, InPeriod, noteTypeId: sessions);
        Assert.IsTrue((await fixture.Service.Summaries.SaveNoisePolicyAsync(new NoisePolicyDto { ExcludedFolderIds = { archive } })).IsValid);

        var selection = await SelectAsync(fixture, new NoteSelectionRequestDto { From = From, To = To });

        CollectionAssert.AreEquivalent(new[] { cited, citing, tagged, archived }, Ids(selection));
        Assert.AreEqual(1, selection.KNoteNotes, "counted, not listed");
        Assert.AreEqual(EnumSelectionDecision.In, Note(selection, cited).Decision);
        StringAssert.Contains(Note(selection, cited).Reason, "cited by 1 note of the selection");
        Assert.AreEqual((EnumSelectionDecision.Out, EnumSelectionSource.Tag), (Note(selection, tagged).Decision, Note(selection, tagged).Source));
        Assert.AreEqual((EnumSelectionDecision.Out, EnumSelectionSource.Policy), (Note(selection, archived).Decision, Note(selection, archived).Source));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ACorrection_IsKeptInNoteRelevance_AboveThePolicy_AndTakenAwayWhenItIsKNotesAgain(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        await SignInAsync(fixture);
        var archive = await AddFolderAsync(fixture, "Archive");
        var archived = await AddNoteAsync(fixture, "Archived", Words(60), archive, InPeriod);
        var kept = await AddNoteAsync(fixture, "Kept", Words(60), fixture.HomeId, InPeriod);
        Assert.IsTrue((await fixture.Service.Summaries.SaveNoisePolicyAsync(new NoisePolicyDto { ExcludedFolderIds = { archive } })).IsValid);
        var before = (await fixture.Service.Notes.GetAsync(archived)).Entity.ModificationDateTime;

        var saved = await fixture.Service.Summaries.SaveRelevanceCorrectionsAsync(new RelevanceCorrectionsSaveDto
        {
            Changes = { new() { NoteId = archived, Decision = EnumSelectionDecision.In }, new() { NoteId = kept, Decision = EnumSelectionDecision.Out } }
        });

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        Assert.HasCount(2, saved.Entity);
        var selection = await SelectAsync(fixture, new NoteSelectionRequestDto { From = From, To = To });
        var note = Note(selection, archived);
        Assert.AreEqual((EnumSelectionDecision.In, EnumSelectionSource.Correction, EnumSelectionDecision.Out),
            (note.Decision, note.Source, note.ComputedDecision));
        StringAssert.StartsWith(note.Reason, "In: included by ");
        Assert.AreEqual(EnumSelectionDecision.Out, Note(selection, kept).Decision);
        Assert.AreEqual(before, (await fixture.Service.Notes.GetAsync(archived)).Entity.ModificationDateTime, "the person's note does not change");

        // The note «Note relevance»: of KNote, in «KNote Atlas», with its table.
        var relevance = await RelevanceNoteAsync(fixture);
        StringAssert.Contains(relevance.Description, "| In |");
        Assert.AreEqual(KntConst.AtlasFolderName, (await fixture.Service.Folders.GetAsync(relevance.FolderId)).Entity.Name);

        // Back to what KNote decides: the correction goes away.
        Assert.IsTrue((await fixture.Service.Summaries.SaveRelevanceCorrectionsAsync(new RelevanceCorrectionsSaveDto
        {
            Changes = { new() { NoteId = archived, Decision = null } }
        })).IsValid);
        selection = await SelectAsync(fixture, new NoteSelectionRequestDto { From = From, To = To });
        Assert.AreEqual(EnumSelectionSource.Policy, Note(selection, archived).Source);
        Assert.AreEqual(EnumSelectionSource.Correction, Note(selection, kept).Source);
    }

    [TestMethod]
    public async Task TheRowsOfTheNotesDeleted_GoAway_AndTwoPeopleAtOnce_KeepBoth()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await SignInAsync(fixture);
        var first = await AddNoteAsync(fixture, "First", Words(60), fixture.HomeId, InPeriod);
        var second = await AddNoteAsync(fixture, "Second", Words(60), fixture.HomeId, InPeriod);
        var deleted = await AddNoteAsync(fixture, "Deleted", Words(60), fixture.HomeId, InPeriod);
        Assert.IsTrue((await fixture.Service.Summaries.SaveRelevanceCorrectionsAsync(Change(deleted, EnumSelectionDecision.Out))).IsValid);
        Assert.IsTrue((await fixture.Service.Notes.DeleteAsync(deleted)).IsValid);

        // Two services of the same database at once: the writes wait their turn and read the note again.
        var other = new KntService(fixture.Db.CreateRepository("Dapper")) { UserIdentityName = fixture.Service.UserIdentityName };
        var results = await Task.WhenAll(
            fixture.Service.Summaries.SaveRelevanceCorrectionsAsync(Change(first, EnumSelectionDecision.Out)),
            other.Summaries.SaveRelevanceCorrectionsAsync(Change(second, EnumSelectionDecision.Out)));

        Assert.IsTrue(results.All(r => r.IsValid), string.Join(" ", results.Select(r => r.ErrorMessage)));
        var (corrections, _) = KNote.Model.Core.RelevanceNote.Parse((await RelevanceNoteAsync(fixture)).Description);
        CollectionAssert.AreEquivalent(new[] { first, second }, corrections.Select(c => c.NoteId).ToList());
    }

    [TestMethod]
    public async Task AStaff_CorrectsThroughTheCommand_ButNotByHand_AndAGuestDoesNotSelect()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var note = await AddNoteAsync(fixture, "Note", Words(60), fixture.HomeId, InPeriod);
        var staff = await ServiceOfAsync(fixture, "selection-staff", nameof(EnumRoles.Staff));
        var guest = await ServiceOfAsync(fixture, "selection-guest", nameof(EnumRoles.Guest));

        var corrected = await staff.Summaries.SaveRelevanceCorrectionsAsync(Change(note, EnumSelectionDecision.Out));
        var relevance = await RelevanceNoteAsync(fixture);
        relevance.Description += "\nBy hand.";
        var byHand = await staff.Notes.SaveAsync(relevance);
        var guestSelection = await guest.Summaries.PrepareSelectionAsync(new NoteSelectionRequestDto { From = From, To = To });

        Assert.IsTrue(corrected.IsValid, corrected.ErrorMessage);
        StringAssert.Contains((await RelevanceNoteAsync(fixture)).Description, "selection-staff");
        Assert.IsFalse(byHand.IsValid, "a protected note of KNote Atlas");
        Assert.IsTrue(guestSelection.NotAuthorized);
    }

    [TestMethod]
    public async Task TalksAbout_IsAmongTheNotesOfThePeriod_AndNeedsTheIndex()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var volcano = await AddNoteAsync(fixture, "Mountain", "Lava eruption of the volcano.", fixture.HomeId, InPeriod);
        await AddNoteAsync(fixture, "Garden", "Tomatoes and lettuce in the garden.", fixture.HomeId, InPeriod);
        await AddNoteAsync(fixture, "Old mountain", "Lava eruption of the volcano.", fixture.HomeId, new DateTime(2019, 1, 1));
        var request = new NoteSelectionRequestDto { From = From, To = To, MeaningText = "lava eruption volcano" };

        var withoutIndex = await fixture.Service.Summaries.PrepareSelectionAsync(request);
        await fixture.RunAsync();
        var about = await SelectAsync(fixture, request);

        Assert.IsFalse(withoutIndex.IsValid, "a search does not create the index");
        CollectionAssert.AreEqual(new[] { volcano }, Ids(about));
        Assert.IsTrue(about.MeaningUsed);
    }

    [TestMethod]
    public async Task APeriodThatEndsBeforeItStarts_IsRefused()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();

        var result = await fixture.Service.Summaries.PrepareSelectionAsync(new NoteSelectionRequestDto { From = To, To = From });

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.ErrorMessage, "The end of the period is before its start.");
    }

    [TestMethod]
    public async Task TheSelection_GivesNoteRelevance_OnceItExists()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await SignInAsync(fixture);
        var note = await AddNoteAsync(fixture, "Note", Words(60), fixture.HomeId, InPeriod);
        Assert.IsNull((await SelectAsync(fixture, new NoteSelectionRequestDto { From = From, To = To })).RelevanceNote, "not created by reading");

        Assert.IsTrue((await fixture.Service.Summaries.SaveRelevanceCorrectionsAsync(Change(note, EnumSelectionDecision.Out))).IsValid);
        var relevance = (await SelectAsync(fixture, new NoteSelectionRequestDto { From = From, To = To })).RelevanceNote;

        Assert.IsNotNull(relevance);
        Assert.AreEqual(KntConst.RelevanceTopic, relevance.Topic);
        Assert.AreEqual((await RelevanceNoteAsync(fixture)).NoteId, relevance.NoteId);
    }

    private static RelevanceCorrectionsSaveDto Change(Guid noteId, EnumSelectionDecision? decision) =>
        new() { Changes = { new RelevanceCorrectionChangeDto { NoteId = noteId, Decision = decision } } };

    private static async Task<NoteSelectionResultDto> SelectAsync(SemanticIndexFixture fixture, NoteSelectionRequestDto request)
    {
        var result = await fixture.Service.Summaries.PrepareSelectionAsync(request);
        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        return result.Entity;
    }

    private static List<Guid> Ids(NoteSelectionResultDto selection) => selection.Notes.Select(n => n.NoteId).ToList();

    private static NoteSelectionDto Note(NoteSelectionResultDto selection, Guid noteId) => selection.Notes.Single(n => n.NoteId == noteId);

    private static async Task SignInAsync(SemanticIndexFixture fixture) =>
        fixture.Service.UserIdentityName = (await fixture.Service.Users.GetAsync(fixture.UserId)).Entity.UserName;

    private static async Task<KntService> ServiceOfAsync(SemanticIndexFixture fixture, string userName, string role)
    {
        var user = await fixture.Service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(), UserName = userName, EMail = $"{userName}@knote.tests", FullName = userName, RoleDefinition = role
        });
        Assert.IsTrue(user.IsValid, user.ErrorMessage);
        return new KntService(fixture.Db.CreateRepository("Dapper")) { UserIdentityName = userName, EnforceAuthorization = true };
    }

    private static async Task<NoteDto> RelevanceNoteAsync(SemanticIndexFixture fixture)
    {
        var type = (await fixture.Service.NoteTypes.GetAllAsync()).Entity.Single(t => t.Name == KntConst.RelevanceTag);
        var notes = (await fixture.Service.Notes.GetFilterMinimalAsync(new NotesFilterDto { NoteTypeId = type.NoteTypeId })).Entity;
        return (await fixture.Service.Notes.GetAsync(notes.Single().NoteId)).Entity;
    }

    private static async Task<Guid> AddFolderAsync(SemanticIndexFixture fixture, string name, Guid? parentId = null)
    {
        var saved = await fixture.Service.Folders.SaveAsync(new FolderDto { Name = name, ParentId = parentId ?? fixture.HomeId });
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity.FolderId;
    }

    // A note with its dates (the repository keeps the dates it receives); created the day it was modified unless said.
    private static async Task<Guid> AddNoteAsync(SemanticIndexFixture fixture, string topic, string description, Guid folderId,
        DateTime modified, DateTime? created = null, string? tags = null, Guid? noteTypeId = null)
    {
        var noteId = Guid.NewGuid();
        var note = await fixture.Service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId, Topic = topic, Description = description, FolderId = folderId, Tags = tags, NoteTypeId = noteTypeId,
            CreationDateTime = created ?? modified.AddDays(-1), ModificationDateTime = modified
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }
}
