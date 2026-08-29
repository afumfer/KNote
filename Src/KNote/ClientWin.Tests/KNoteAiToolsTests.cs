using System.Text.Json;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

[TestClass]
public class KNoteAiToolsTests
{
    private static Result<T> Valid<T>(T entity) => new() { Entity = entity };

    private static Result<T> Invalid<T>(string message)
    {
        var result = new Result<T>();
        result.AddErrorMessage(message);
        return result;
    }

    [TestMethod]
    public async Task SearchNotes_NoMatches_ReturnsFriendlyMessage()
    {
        var service = new FakeKntService();
        service.NotesFake.GetSearchMinimalAsyncImpl = _ => Task.FromResult(Valid(new List<NoteMinimalDto>()));
        var tools = new KNoteAiTools(service, TestStoreFactory.CreateEmpty());

        var result = await GetSearchNotesTool(tools)("nothing matches this", false);

        Assert.AreEqual("No notes found matching the given search.", result);
    }

    [TestMethod]
    public async Task SearchNotes_ForwardsTextSearchAndIncludeContentToTheQuery()
    {
        NotesSearchDto capturedSearch = null;
        var service = new FakeKntService();
        service.NotesFake.GetSearchMinimalAsyncImpl = search =>
        {
            capturedSearch = search;
            return Task.FromResult(Valid(new List<NoteMinimalDto>()));
        };
        var tools = new KNoteAiTools(service, TestStoreFactory.CreateEmpty());

        await GetSearchNotesTool(tools)("invoice !draft", true);

        Assert.AreEqual("invoice !draft", capturedSearch.TextSearch);
        Assert.IsTrue(capturedSearch.SearchInDescription);
    }

    [TestMethod]
    public async Task SearchNotes_ReturnsOneJsonLinePerMatch_WithFullNoteMinimalDtoFields()
    {
        var note = new NoteMinimalDto
        {
            NoteId = Guid.NewGuid(),
            NoteNumber = 123,
            Topic = "Test topic",
            Tags = "[Personal]",
            Priority = 2
        };
        var service = new FakeKntService();
        service.NotesFake.GetSearchMinimalAsyncImpl = _ => Task.FromResult(Valid(new List<NoteMinimalDto> { note }));
        var tools = new KNoteAiTools(service, TestStoreFactory.CreateEmpty());

        var result = await GetSearchNotesTool(tools)("test", false);

        var line = result.Trim();
        var deserialized = JsonSerializer.Deserialize<NoteMinimalDto>(line);
        Assert.AreEqual(note.NoteId, deserialized.NoteId);
        Assert.AreEqual(note.NoteNumber, deserialized.NoteNumber);
        Assert.AreEqual(note.Topic, deserialized.Topic);
        Assert.AreEqual(note.Tags, deserialized.Tags);
        Assert.AreEqual(note.Priority, deserialized.Priority);
    }

    [TestMethod]
    public async Task SearchNotes_ServiceReturnsError_ReturnsErrorTextToTheModel()
    {
        var service = new FakeKntService();
        service.NotesFake.GetSearchMinimalAsyncImpl = _ => Task.FromResult(Invalid<List<NoteMinimalDto>>("boom"));
        var tools = new KNoteAiTools(service, TestStoreFactory.CreateEmpty());

        var result = await GetSearchNotesTool(tools)("test", false);

        StringAssert.Contains(result, "boom");
    }

    [TestMethod]
    public async Task GetNoteDetails_NeitherIdNorNumberProvided_ReturnsErrorWithoutCallingTheService()
    {
        var service = new FakeKntService();
        var tools = new KNoteAiTools(service, TestStoreFactory.CreateEmpty());

        var result = await GetNoteDetailsTool(tools)(null, null);

        StringAssert.Contains(result, "provide either noteId or noteNumber");
    }

    [TestMethod]
    public async Task GetNoteDetails_InvalidGuidNoteId_ReturnsErrorWithoutCallingTheService()
    {
        var service = new FakeKntService();
        var tools = new KNoteAiTools(service, TestStoreFactory.CreateEmpty());

        var result = await GetNoteDetailsTool(tools)("not-a-guid", null);

        StringAssert.Contains(result, "not a valid noteId");
    }

    [TestMethod]
    public async Task GetNoteDetails_ByNoteId_ReturnsFullNoteAsJson()
    {
        var noteId = Guid.NewGuid();
        var note = new NoteDto { NoteId = noteId, Topic = "Full note", Description = "The body" };
        var service = new FakeKntService();
        service.NotesFake.GetByIdAsyncImpl = id => id == noteId ? Task.FromResult(Valid(note)) : throw new InvalidOperationException("wrong id");
        var tools = new KNoteAiTools(service, TestStoreFactory.CreateEmpty());

        var result = await GetNoteDetailsTool(tools)(noteId.ToString(), null);

        var deserialized = JsonSerializer.Deserialize<NoteDto>(result);
        Assert.AreEqual(note.Topic, deserialized.Topic);
        Assert.AreEqual(note.Description, deserialized.Description);
    }

    [TestMethod]
    public async Task GetNoteDetails_ByNoteNumber_ReturnsFullNoteAsJson()
    {
        var note = new NoteDto { NoteNumber = 42, Topic = "By number", Description = "Body" };
        var service = new FakeKntService();
        service.NotesFake.GetByNumberAsyncImpl = number => number == 42 ? Task.FromResult(Valid(note)) : throw new InvalidOperationException("wrong number");
        var tools = new KNoteAiTools(service, TestStoreFactory.CreateEmpty());

        var result = await GetNoteDetailsTool(tools)(null, 42);

        var deserialized = JsonSerializer.Deserialize<NoteDto>(result);
        Assert.AreEqual(note.Topic, deserialized.Topic);
        Assert.AreEqual(note.Description, deserialized.Description);
    }

    [TestMethod]
    public async Task GetNoteDetails_NoteNotFound_ReturnsFriendlyMessage()
    {
        var noteId = Guid.NewGuid();
        var service = new FakeKntService();
        service.NotesFake.GetByIdAsyncImpl = _ => Task.FromResult(Valid<NoteDto>(null));
        var tools = new KNoteAiTools(service, TestStoreFactory.CreateEmpty());

        var result = await GetNoteDetailsTool(tools)(noteId.ToString(), null);

        Assert.AreEqual("Note not found.", result);
    }

    // CreateTask (create_task) persists through the Service layer (like search_notes/get_note_details),
    // then fires-and-forgets opening the saved note via NoteEditorCtrl.LoadModelById+Run() - the same
    // "open an existing note" path the rest of the app uses. The persistence logic below is fully
    // unit-testable with fakes; only that final "opens a real Form" tail isn't (no WinForms UI
    // automation available here) - covered by manual verification instead.
    [TestMethod]
    public async Task CreateTask_EmptyTopic_ReturnsErrorWithoutTouchingStore()
    {
        var tools = new KNoteAiTools(new FakeKntService(), TestStoreFactory.CreateEmpty());

        var result = await GetCreateTaskTool(tools)("   ", "some description");

        StringAssert.Contains(result, "topic is required");
    }

    [TestMethod]
    public async Task CreateTask_NoDefaultFolderConfigured_ReturnsFriendlyError()
    {
        // TestStoreFactory.CreateEmpty() leaves Store.DefaultFolderWithServiceRef unset (null) -
        // the same state a freshly-constructed Store is in before Program.cs's LoadAppStore runs.
        var tools = new KNoteAiTools(new FakeKntService(), TestStoreFactory.CreateEmpty());

        var result = await GetCreateTaskTool(tools)("Buy milk", "2% milk, one gallon");

        StringAssert.Contains(result, "no default repository/folder is configured");
    }

    [TestMethod]
    public async Task CreateTask_HappyPath_CreatesAndSavesUsingTheDefaultFoldersService()
    {
        var defaultFolderId = Guid.NewGuid();
        var savedNoteId = Guid.NewGuid();
        var defaultService = new FakeKntService();
        defaultService.NotesFake.NewExtendedAsyncImpl = _ => Task.FromResult(Valid(new NoteExtendedDto()));
        NoteExtendedDto savedNote = null;
        defaultService.NotesFake.SaveExtendedAsyncImpl = note =>
        {
            savedNote = note;
            note.NoteId = savedNoteId;
            note.NoteNumber = 7;
            return Task.FromResult(Valid(note));
        };
        var store = TestStoreFactory.CreateEmpty();
        store.DefaultFolderWithServiceRef = new FolderWithServiceRef
        {
            ServiceRef = TestServiceRefFactory.CreateWithFakeService(defaultService),
            FolderInfo = new FolderInfoDto { FolderId = defaultFolderId, Name = "Default folder" }
        };
        // The "active" service (used by search_notes/get_note_details) is a separate, untouched
        // fake - create_task must use the default folder's service, not this one.
        var activeService = new FakeKntService();
        var tools = new KNoteAiTools(activeService, store);

        var result = await GetCreateTaskTool(tools)("Buy milk", "2% milk, one gallon");

        Assert.IsNotNull(savedNote, "SaveExtendedAsync was never called.");
        Assert.AreEqual("Buy milk", savedNote.Topic);
        Assert.AreEqual("2% milk, one gallon", savedNote.Description);
        Assert.AreEqual(defaultFolderId, savedNote.FolderId);
        // Regression: NewExtendedAsync leaves Tags null by design; KntNotesSaveExtendedAsyncCommand.
        // Execute unconditionally calls Param.Tags.Contains(...), so a null Tags throws a
        // NullReferenceException on save (caught against the real database, not this fake).
        Assert.AreEqual("", savedNote.Tags);
        StringAssert.Contains(result, "Buy milk");
        StringAssert.Contains(result, "#7");
    }

    [TestMethod]
    public async Task CreateTask_NullDescription_IsSavedAsEmptyString()
    {
        var defaultService = new FakeKntService();
        defaultService.NotesFake.NewExtendedAsyncImpl = _ => Task.FromResult(Valid(new NoteExtendedDto()));
        NoteExtendedDto savedNote = null;
        defaultService.NotesFake.SaveExtendedAsyncImpl = note =>
        {
            savedNote = note;
            return Task.FromResult(Valid(note));
        };
        var store = TestStoreFactory.CreateEmpty();
        store.DefaultFolderWithServiceRef = new FolderWithServiceRef
        {
            ServiceRef = TestServiceRefFactory.CreateWithFakeService(defaultService),
            FolderInfo = new FolderInfoDto { FolderId = Guid.NewGuid(), Name = "Default folder" }
        };
        var tools = new KNoteAiTools(new FakeKntService(), store);

        await GetCreateTaskTool(tools)("Buy milk", null);

        Assert.AreEqual("", savedNote.Description);
    }

    [TestMethod]
    public async Task CreateTask_NewExtendedAsyncFails_ReturnsErrorWithoutSaving()
    {
        var defaultService = new FakeKntService();
        defaultService.NotesFake.NewExtendedAsyncImpl = _ => Task.FromResult(Invalid<NoteExtendedDto>("cannot create"));
        var saveCalled = false;
        defaultService.NotesFake.SaveExtendedAsyncImpl = note =>
        {
            saveCalled = true;
            return Task.FromResult(Valid(note));
        };
        var store = TestStoreFactory.CreateEmpty();
        store.DefaultFolderWithServiceRef = new FolderWithServiceRef
        {
            ServiceRef = TestServiceRefFactory.CreateWithFakeService(defaultService),
            FolderInfo = new FolderInfoDto { FolderId = Guid.NewGuid(), Name = "Default folder" }
        };
        var tools = new KNoteAiTools(new FakeKntService(), store);

        var result = await GetCreateTaskTool(tools)("Buy milk", "details");

        StringAssert.Contains(result, "cannot create");
        Assert.IsFalse(saveCalled, "SaveExtendedAsync should not be called when NewExtendedAsync fails.");
    }

    [TestMethod]
    public async Task CreateTask_SaveExtendedAsyncFails_ReturnsErrorToTheModel()
    {
        var defaultService = new FakeKntService();
        defaultService.NotesFake.NewExtendedAsyncImpl = _ => Task.FromResult(Valid(new NoteExtendedDto()));
        defaultService.NotesFake.SaveExtendedAsyncImpl = _ => Task.FromResult(Invalid<NoteExtendedDto>("disk full"));
        var store = TestStoreFactory.CreateEmpty();
        store.DefaultFolderWithServiceRef = new FolderWithServiceRef
        {
            ServiceRef = TestServiceRefFactory.CreateWithFakeService(defaultService),
            FolderInfo = new FolderInfoDto { FolderId = Guid.NewGuid(), Name = "Default folder" }
        };
        var tools = new KNoteAiTools(new FakeKntService(), store);

        var result = await GetCreateTaskTool(tools)("Buy milk", "details");

        StringAssert.Contains(result, "disk full");
    }

    // KNoteAiTools.SearchNotesAsync/GetNoteDetailsAsync/CreateTaskAsync are private (they're only
    // meant to be reached through the AITool built by AIFunctionFactory.Create in GetTools()), so
    // tests reach them via reflection instead of widening their accessibility just for testing.
    private static Func<string, bool, Task<string>> GetSearchNotesTool(KNoteAiTools tools)
    {
        var method = typeof(KNoteAiTools).GetMethod("SearchNotesAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (textSearch, includeContent) => (Task<string>)method.Invoke(tools, new object[] { textSearch, includeContent });
    }

    private static Func<string, int?, Task<string>> GetNoteDetailsTool(KNoteAiTools tools)
    {
        var method = typeof(KNoteAiTools).GetMethod("GetNoteDetailsAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (noteId, noteNumber) => (Task<string>)method.Invoke(tools, new object[] { noteId, noteNumber });
    }

    private static Func<string, string, Task<string>> GetCreateTaskTool(KNoteAiTools tools)
    {
        var method = typeof(KNoteAiTools).GetMethod("CreateTaskAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (topic, description) => (Task<string>)method.Invoke(tools, new object[] { topic, description });
    }
}
