using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The tools of the assistant that only read (KNote Atlas, delivery 1.2) against a real repository, called as the model
/// calls them: by name, with its arguments. Folders, types, users and attributes by the names people use, the options when
/// a name is missing or could be several, and the system notes out of all of them.
/// </summary>
[TestClass]
public class AiQueryToolsTests
{
    private const string Query = "keyboard membrane of the Spectrum";

    private sealed class NoHost : IKNoteAiToolsHost
    {
        public Task<AiNoteDestination> GetNewNoteDestinationAsync() => Task.FromResult<AiNoteDestination>(null!);
        public void OnNoteCreated(IKntService service, NoteExtendedDto note, bool openEditor = true) { }
    }

    private static async Task<SemanticIndexFixture> FixtureAsync(string orm = "Dapper")
    {
        var fixture = await SemanticIndexFixture.CreateAsync(orm);
        fixture.Service.UserIdentityName = (await fixture.Service.Users.GetAsync(fixture.UserId)).Entity.UserName;
        return fixture;
    }

    private static async Task<string> CallAsync(SemanticIndexFixture fixture, string tool, params (string Name, object Value)[] arguments)
    {
        var function = new KNoteAiTools(fixture.Service, new NoHost()).GetTools().OfType<AIFunction>().Single(t => t.Name == tool);
        var result = await function.InvokeAsync(new AIFunctionArguments(arguments.ToDictionary(a => a.Name, a => (object?)a.Value)));
        return result is System.Text.Json.JsonElement json ? json.GetString()! : result?.ToString()!;
    }

    private static async Task<FolderDto> AddFolderAsync(SemanticIndexFixture fixture, string name, Guid parentId)
    {
        var res = await fixture.Service.Folders.SaveAsync(new FolderDto { Name = name, ParentId = parentId });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static async Task<UserDto> AddUserAsync(SemanticIndexFixture fixture, string userName, string fullName)
    {
        var res = await fixture.Service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(), UserName = userName, EMail = $"{userName}@knote.tests", FullName = fullName, RoleDefinition = "Staff"
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static async Task<NoteDto> AddGlossaryAsync(SemanticIndexFixture fixture, string rows)
    {
        var glossary = (await fixture.Service.AtlasContext.CreateGlossaryAsync()).Entity;
        Assert.IsNotNull(glossary);
        glossary.Description = GlossaryTable.Template + rows;
        var saved = await fixture.Service.Notes.SaveAsync(glossary);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity;
    }

    private static async Task<KAttributeDto> AddTabulatedAttributeAsync(SemanticIndexFixture fixture, string name, Guid noteTypeId, params string[] values)
    {
        var id = Guid.NewGuid();
        var attribute = new KAttributeDto
        {
            KAttributeId = id, Name = name, NoteTypeId = noteTypeId, KAttributeDataType = EnumKAttributeDataType.TabulatedValue,
            KAttributeValues = values.Select((v, i) => new KAttributeTabulatedValueDto
            {
                KAttributeTabulatedValueId = Guid.NewGuid(), KAttributeId = id, Value = v, Order = i
            }).ToList()
        };
        var res = await fixture.Service.Repository.KAttributes.AddAsync(attribute);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    [TestMethod]
    public async Task GetToolsAsync_OffersTheSearchByMeaningAndTheGlossaryOnlyWhenTheyCanBeUsed()
    {
        using var fixture = await FixtureAsync();
        var tools = new KNoteAiTools(fixture.Service, new NoHost());

        var before = (await tools.GetToolsAsync()).Select(t => t.Name).ToList();
        await fixture.AddNoteAsync("Spectrum", Query);
        await fixture.RunAsync();
        await AddGlossaryAsync(fixture, "| CPC | Amstrad Colour Personal Computer | |\n");
        var after = (await tools.GetToolsAsync()).Select(t => t.Name).ToList();

        CollectionAssert.DoesNotContain(before, "find_notes_about");
        CollectionAssert.DoesNotContain(before, "lookup_glossary");
        CollectionAssert.IsSubsetOf(new[] { "search_notes", "get_note_details", "create_task", "filter_notes", "get_folder_tree", "get_note_items", "get_tasks" }, before);
        CollectionAssert.Contains(after, "find_notes_about");
        CollectionAssert.Contains(after, "lookup_glossary");
        Assert.AreEqual(24, after.Count, "view_note_images, read_document and view_document_pages come in, and attach_file only with files in the conversation.");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task FindNotesAbout_GivesTheNotesWithTheirFolderAndFragment_AmongTheFilter(string orm)
    {
        using var fixture = await FixtureAsync(orm);
        var folder = await AddFolderAsync(fixture, "Collection", fixture.HomeId);
        var inFolder = await fixture.AddNoteAsync("Spectrum repair", Query, folder.FolderId);
        var inHome = await fixture.AddNoteAsync("Spectrum elsewhere", Query);
        await fixture.RunAsync();

        var all = await CallAsync(fixture, "find_notes_about", ("text", Query));
        var filtered = await CallAsync(fixture, "find_notes_about", ("text", Query), ("folder", "Collection"));

        StringAssert.Contains(all, $"\"NoteNumber\":{inHome.NoteNumber}");
        StringAssert.Contains(all, "\"Similarity\"");
        StringAssert.Contains(all, "\"Fragment\":\"");
        StringAssert.Contains(filtered, $"\"NoteNumber\":{inFolder.NoteNumber}");
        StringAssert.Contains(filtered, "Home\\\\Collection".Replace("Home", (await fixture.Service.Folders.GetHomeAsync()).Entity.Name));
        Assert.IsFalse(filtered.Contains($"\"NoteNumber\":{inHome.NoteNumber},"), filtered);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task FilterNotes_ByFolderAndDate_SaysHowManyAndGivesTheLastModified(string orm)
    {
        using var fixture = await FixtureAsync(orm);
        var parent = await AddFolderAsync(fixture, "Facturación", fixture.HomeId);
        var child = await AddFolderAsync(fixture, "2026", parent.FolderId);
        var note = await fixture.AddNoteAsync("Invoice", "Text", child.FolderId);
        await fixture.AddNoteAsync("Other", "Text");

        var inFolder = await CallAsync(fixture, "filter_notes", ("folder", "facturacion"));
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var modifiedToday = await CallAsync(fixture, "filter_notes", ("folder", "Facturación/2026"), ("modifiedFrom", today), ("modifiedTo", today));
        var longAgo = await CallAsync(fixture, "filter_notes", ("folder", "Facturación"), ("modifiedTo", "2001-01-01"));
        var badDate = await CallAsync(fixture, "filter_notes", ("modifiedFrom", "september"));

        StringAssert.Contains(inFolder, "1 notes meet those criteria");
        StringAssert.Contains(inFolder, $"\"NoteNumber\":{note.NoteNumber}");
        StringAssert.Contains(inFolder, "Facturación\\\\2026");
        StringAssert.Contains(modifiedToday, "Invoice");
        Assert.AreEqual("No notes meet those criteria.", longAgo);
        StringAssert.Contains(badDate, "use yyyy-MM-dd");
    }

    [TestMethod]
    public async Task FilterNotes_WithMoreNotesThanShown_SaysHowManyThereAre()
    {
        using var fixture = await FixtureAsync();
        var folder = await AddFolderAsync(fixture, "Many", fixture.HomeId);
        for (var i = 0; i < 23; i++)
            await fixture.AddNoteAsync($"Note {i}", "Text", folder.FolderId);

        var found = await CallAsync(fixture, "filter_notes", ("folder", "Many"));

        StringAssert.Contains(found, "23 notes meet those criteria. These are the 20 modified last:");
        Assert.AreEqual(21, found.Trim().Split('\n').Length);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task FilterNotes_LeavesOutTheSystemNotes(string orm)
    {
        using var fixture = await FixtureAsync(orm);
        var glossary = await AddGlossaryAsync(fixture, "| CPC | Computer | |\n");

        var all = await CallAsync(fixture, "filter_notes");

        Assert.IsFalse(all.Contains($"\"NoteNumber\":{glossary.NoteNumber},"), all);
    }

    [TestMethod]
    public async Task FilterNotes_ByATabulatedAttribute_CorrectsTheValueOrGivesTheValidOnes()
    {
        using var fixture = await FixtureAsync();
        var type = (await fixture.Service.NoteTypes.SaveAsync(new NoteTypeDto { Name = "Computer" })).Entity;
        await AddTabulatedAttributeAsync(fixture, "State", type.NoteTypeId, "Pending restoration", "Restored");
        var note = await fixture.AddNoteAsync("Spectrum", "Text", noteTypeId: type.NoteTypeId);
        var read = (await fixture.Service.Notes.GetAsync(note.NoteId)).Entity;
        read.KAttributesDto.Single(a => a.Name == "State").Value = "Pending restoration";
        Assert.IsTrue((await fixture.Service.Notes.SaveAsync(read)).IsValid);
        await fixture.AddNoteAsync("Amstrad", "Text", noteTypeId: type.NoteTypeId);

        var found = await CallAsync(fixture, "filter_notes", ("attribute", "state"), ("attributeValue", "pending RESTORATION"));
        var wrong = await CallAsync(fixture, "filter_notes", ("attribute", "State"), ("attributeValue", "pending"));
        var unknown = await CallAsync(fixture, "filter_notes", ("attribute", "Colour"), ("attributeValue", "red"));
        var halfGiven = await CallAsync(fixture, "filter_notes", ("attribute", "State"));

        StringAssert.Contains(found, "1 notes meet those criteria");
        StringAssert.Contains(found, "Spectrum");
        StringAssert.Contains(wrong, "Pending restoration, Restored");
        StringAssert.Contains(unknown, "there is no attribute 'Colour'");
        StringAssert.Contains(halfGiven, "give both attribute and attributeValue");
    }

    [TestMethod]
    public async Task Names_MissingOrThatCouldBeSeveral_GiveTheOptions()
    {
        using var fixture = await FixtureAsync();
        var a = await AddFolderAsync(fixture, "Projects", fixture.HomeId);
        var b = await AddFolderAsync(fixture, "Archive", fixture.HomeId);
        await AddFolderAsync(fixture, "KNote", a.FolderId);
        var archived = await AddFolderAsync(fixture, "KNote", b.FolderId);
        await fixture.Service.NoteTypes.SaveAsync(new NoteTypeDto { Name = "Project" });
        await AddUserAsync(fixture, "mlopez", "Marta López");
        await AddUserAsync(fixture, "mruiz", "Marta Ruiz");

        var twoFolders = await CallAsync(fixture, "filter_notes", ("folder", "KNote"));
        var byPath = await CallAsync(fixture, "get_folder_tree", ("folder", "Archive\\KNote"));
        var byNumber = await CallAsync(fixture, "get_folder_tree", ("folder", $"#{archived.FolderNumber}"));
        var noFolder = await CallAsync(fixture, "filter_notes", ("folder", "Nowhere"));
        var noType = await CallAsync(fixture, "filter_notes", ("noteType", "Unknown"));
        var twoUsers = await CallAsync(fixture, "filter_notes", ("taskUser", "Marta"));
        var noUser = await CallAsync(fixture, "get_tasks", ("user", "Nobody"));
        var oneUser = await CallAsync(fixture, "filter_notes", ("taskUser", "marta lopez"));

        StringAssert.Contains(twoFolders, "could be several folders");
        StringAssert.Contains(twoFolders, "Projects\\KNote");
        StringAssert.Contains(twoFolders, "Archive\\KNote");
        StringAssert.StartsWith(byPath, "KNote (#");
        Assert.AreEqual(byPath, byNumber);
        StringAssert.Contains(noFolder, "there is no folder 'Nowhere'");
        StringAssert.Contains(noType, "Project");
        StringAssert.Contains(twoUsers, "Marta López (mlopez), Marta Ruiz (mruiz)");
        StringAssert.Contains(noUser, "there is no user 'Nobody'");
        Assert.AreEqual("No notes meet those criteria.", oneUser);
    }

    [TestMethod]
    public async Task GetFolderTree_ShowsTheFoldersWithoutTheSystemOnes()
    {
        using var fixture = await FixtureAsync();
        var parent = await AddFolderAsync(fixture, "Projects", fixture.HomeId);
        await AddFolderAsync(fixture, "KNote", parent.FolderId);
        await AddGlossaryAsync(fixture, "| CPC | Computer | |\n");

        var tree = await CallAsync(fixture, "get_folder_tree");
        var homeName = (await fixture.Service.Folders.GetHomeAsync()).Entity.Name;

        StringAssert.Contains(tree, homeName);
        StringAssert.Contains(tree, "  Projects (#");
        StringAssert.Contains(tree, "    KNote (#");
        Assert.IsFalse(tree.Contains(KntConst.AtlasFolderName), tree);
        StringAssert.Contains(await CallAsync(fixture, "filter_notes", ("folder", KntConst.AtlasFolderName)), "there is no folder");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetNoteItems_GivesWhatTheNoteHas_WithoutScriptsOrSystemNotes(string orm)
    {
        using var fixture = await FixtureAsync(orm);
        var note = await fixture.AddNoteAsync("Spectrum", "Text");
        var related = await fixture.AddNoteAsync("Keyboard", "Text");
        var glossary = await AddGlossaryAsync(fixture, "| CPC | Computer | |\n");
        Assert.IsTrue((await fixture.Service.Notes.SaveNoteTaskAsync(new NoteTaskDto
        {
            NoteTaskId = Guid.NewGuid(), NoteId = note.NoteId, UserId = fixture.UserId, Tags = "Replace the membrane", Description = "Buy it"
        }, true)).IsValid);
        foreach (var (type, comment) in new[] { (EnumNotificationType.AppInfo, "Call the shop"), (EnumNotificationType.ExecuteKntScript, "secret script") })
            Assert.IsTrue((await fixture.Service.Repository.Notes.AddMessageAsync(new KMessageDto
            {
                KMessageId = Guid.NewGuid(), NoteId = note.NoteId, UserId = fixture.UserId, NotificationType = type,
                AlarmType = EnumAlarmType.Standard, AlarmActivated = true, AlarmDateTime = new DateTime(2026, 12, 1, 9, 0, 0), Comment = comment
            })).IsValid);
        foreach (var other in new[] { related, glossary })
            Assert.IsTrue((await fixture.Service.Notes.SaveTraceNoteAsync(new TraceNoteDto
            {
                TraceNoteId = Guid.NewGuid(), FromId = note.NoteId, ToId = other.NoteId
            }, true)).IsValid);

        var items = await CallAsync(fixture, "get_note_items", ("noteNumber", note.NoteNumber));
        var onlyTasks = await CallAsync(fixture, "get_note_items", ("noteNumber", note.NoteNumber), ("include", new[] { "tasks" }));
        var wrongKind = await CallAsync(fixture, "get_note_items", ("noteNumber", note.NoteNumber), ("include", new[] { "photos" }));
        var ofSystemNote = await CallAsync(fixture, "get_note_items", ("noteNumber", glossary.NoteNumber));
        var missing = await CallAsync(fixture, "get_note_items", ("noteNumber", 999999));

        StringAssert.Contains(items, "Replace the membrane");
        StringAssert.Contains(items, "Call the shop");
        StringAssert.Contains(items, "2026-12-01 09:00");
        Assert.IsFalse(items.Contains("secret script"), items);
        StringAssert.Contains(items, "Keyboard");
        Assert.IsFalse(items.Contains(KntConst.GlossaryTopic), items);
        Assert.IsFalse(onlyTasks.Contains("alarms:"), onlyTasks);
        StringAssert.Contains(wrongKind, "'photos' cannot be included");
        Assert.AreEqual("Note not found.", ofSystemNote);
        Assert.AreEqual("Note not found.", missing);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetTasks_OfAUserInAPeriod(string orm)
    {
        using var fixture = await FixtureAsync(orm);
        var marta = await AddUserAsync(fixture, "mlopez", "Marta López");
        var note = await fixture.AddNoteAsync("Portal", "Text");
        var today = DateTime.Today;
        foreach (var (user, topic, start, end) in new[]
        {
            (marta.UserId, "Open task", (DateTime?)today.AddDays(-1), (DateTime?)null),
            (marta.UserId, "Done task", today.AddDays(-2), today.AddDays(-1)),
            (marta.UserId, "Old task", today.AddYears(-3), today.AddYears(-3).AddDays(1)),
            (fixture.UserId, "Task of another user", today.AddDays(-1), (DateTime?)null)
        })
            Assert.IsTrue((await fixture.Service.Repository.Notes.AddNoteTaskAsync(new NoteTaskDto
            {
                NoteTaskId = Guid.NewGuid(), NoteId = note.NoteId, UserId = user, Tags = topic, Description = topic, StartDate = start, EndDate = end, Resolved = end != null
            })).IsValid);

        var from = today.AddDays(-5).ToString("yyyy-MM-dd");
        var to = today.AddDays(5).ToString("yyyy-MM-dd");
        var all = await CallAsync(fixture, "get_tasks", ("user", "Marta"), ("from", from), ("to", to));
        var open = await CallAsync(fixture, "get_tasks", ("user", "Marta"), ("from", from), ("to", to), ("status", "open"));
        var wrongStatus = await CallAsync(fixture, "get_tasks", ("status", "maybe"));
        var backwards = await CallAsync(fixture, "get_tasks", ("from", to), ("to", from));
        var tooLong = await CallAsync(fixture, "get_tasks", ("from", "2020-01-01"), ("to", "2026-01-01"));

        StringAssert.Contains(all, "Open task");
        StringAssert.Contains(all, "Done task");
        Assert.IsFalse(all.Contains("Old task"), all);
        Assert.IsFalse(all.Contains("Task of another user"), all);
        StringAssert.Contains(all, $"Tasks from {from} to {to}: resolved 1, in_progress 1, pending 0.");
        StringAssert.Contains(open, "Open task");
        Assert.IsFalse(open.Contains("Done task"), open);
        StringAssert.Contains(wrongStatus, "'maybe' is not a status");
        StringAssert.Contains(backwards, "before the first one");
        StringAssert.Contains(tooLong, "too long");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SearchAndDetails_LeaveOutEverySystemNote(string orm)
    {
        using var fixture = await FixtureAsync(orm);
        var glossary = await AddGlossaryAsync(fixture, "| CPC | Computer | |\n");

        var search = await CallAsync(fixture, "search_notes", ("textSearch", $"#{glossary.NoteNumber}"));
        var byNumber = await CallAsync(fixture, "get_note_details", ("noteNumber", glossary.NoteNumber));
        var byId = await CallAsync(fixture, "get_note_details", ("noteId", glossary.NoteId.ToString()));

        Assert.AreEqual("No notes found matching the given search.", search);
        Assert.AreEqual("Note not found.", byNumber);
        Assert.AreEqual("Note not found.", byId);
    }

    [TestMethod]
    public async Task GetNoteDetails_AddsTheTermsOfTheGlossary_AndCutsAnEnormousNote()
    {
        using var fixture = await FixtureAsync();
        await AddGlossaryAsync(fixture, "| CPC | Amstrad Colour Personal Computer | Amstrad CPC |\n");
        var usual = await fixture.AddNoteAsync("Restoring a CPC", "A short note.");
        var plain = await fixture.AddNoteAsync("Garden", "Tomatoes.");
        var enormous = await fixture.AddNoteAsync("Enormous", new string('x', KNoteAiTools.MaxNoteTextLength + 10));

        var withTerms = await CallAsync(fixture, "get_note_details", ("noteNumber", usual.NoteNumber));
        var withoutTerms = await CallAsync(fixture, "get_note_details", ("noteNumber", plain.NoteNumber));
        var cut = await CallAsync(fixture, "get_note_details", ("noteNumber", enormous.NoteNumber));

        StringAssert.Contains(withTerms, "Terms of the glossary in this note:");
        StringAssert.Contains(withTerms, "- CPC (also written as Amstrad CPC): Amstrad Colour Personal Computer");
        Assert.IsFalse(withoutTerms.Contains("Terms of the glossary"), withoutTerms);
        Assert.IsFalse(withoutTerms.Contains("was cut"), withoutTerms);
        var notice = $"only the first {KNoteAiTools.MaxNoteTextLength} of its {KNoteAiTools.MaxNoteTextLength + 10} characters";
        StringAssert.StartsWith(cut, "[IMPORTANT: this note was cut: " + notice);
        StringAssert.Contains(cut.Trim().Split('\n').Last(), notice, "also after the text");
        StringAssert.Contains(cut, "Tell the user");
        Assert.IsFalse(cut.Contains(new string('x', KNoteAiTools.MaxNoteTextLength + 1)), "the text was not cut");
    }

    [TestMethod]
    public async Task LookupGlossary_GivesTheTermsAskedFor()
    {
        using var fixture = await FixtureAsync();
        await AddGlossaryAsync(fixture, "| CPC | Amstrad Colour Personal Computer | Amstrad CPC |\n| QL | Quantum Leap | |\n");

        var found = await CallAsync(fixture, "lookup_glossary", ("terms", new[] { "amstrad cpc" }));
        var notFound = await CallAsync(fixture, "lookup_glossary", ("terms", new[] { "MSX" }));

        StringAssert.Contains(found, "Amstrad Colour Personal Computer");
        Assert.IsFalse(found.Contains("Quantum"), found);
        Assert.AreEqual("None of these terms is in the glossary.", notFound);
    }
}
