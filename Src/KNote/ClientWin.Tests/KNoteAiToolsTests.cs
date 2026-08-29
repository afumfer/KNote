using System.Text.Json;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
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
        var tools = new KNoteAiTools(service);

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
        var tools = new KNoteAiTools(service);

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
        var tools = new KNoteAiTools(service);

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
        var tools = new KNoteAiTools(service);

        var result = await GetSearchNotesTool(tools)("test", false);

        StringAssert.Contains(result, "boom");
    }

    [TestMethod]
    public async Task GetNoteDetails_NeitherIdNorNumberProvided_ReturnsErrorWithoutCallingTheService()
    {
        var service = new FakeKntService();
        var tools = new KNoteAiTools(service);

        var result = await GetNoteDetailsTool(tools)(null, null);

        StringAssert.Contains(result, "provide either noteId or noteNumber");
    }

    [TestMethod]
    public async Task GetNoteDetails_InvalidGuidNoteId_ReturnsErrorWithoutCallingTheService()
    {
        var service = new FakeKntService();
        var tools = new KNoteAiTools(service);

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
        var tools = new KNoteAiTools(service);

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
        var tools = new KNoteAiTools(service);

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
        var tools = new KNoteAiTools(service);

        var result = await GetNoteDetailsTool(tools)(noteId.ToString(), null);

        Assert.AreEqual("Note not found.", result);
    }

    // KNoteAiTools.SearchNotesAsync/GetNoteDetailsAsync are private (they're only meant to be
    // reached through the AITool built by AIFunctionFactory.Create in GetTools()), so tests reach
    // them via reflection instead of widening their accessibility just for testing.
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
}
