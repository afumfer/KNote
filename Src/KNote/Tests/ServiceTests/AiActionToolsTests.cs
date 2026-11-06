using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The tools that act on the repository (KNote Atlas, 1.4) against a real one, called as the model calls them. What needs
/// the user's confirmation changes nothing until it is accepted; creating is done at once, and so is what was approved for
/// the conversation or a change to a note created in it. What runs goes through the commands, with the roles of the user.
/// </summary>
[TestClass]
public class AiActionToolsTests
{
    private sealed class Host : IKNoteAiToolsHost
    {
        public AiNoteDestination Destination { get; set; } = null!;
        public List<Guid> Created { get; } = new();
        public List<Guid> Changed { get; } = new();
        public HashSet<Guid> InEditor { get; } = new();

        public Task<AiNoteDestination> GetNewNoteDestinationAsync() => Task.FromResult(Destination);
        public void OnNoteCreated(IKntService service, NoteExtendedDto note) => Created.Add(note.NoteId);
        public bool CanChangeNote(Guid noteId) => !InEditor.Contains(noteId);
        public void OnNoteChanged(IKntService service, Guid noteId) => Changed.Add(noteId);
    }

    private sealed record Setup(SemanticIndexFixture Fixture, KNoteAiTools Tools, Host Host) : IDisposable
    {
        public KntService Service => Fixture.Service;
        public void Dispose() => Fixture.Dispose();

        public async Task<string> CallAsync(string tool, params (string Name, object Value)[] arguments)
        {
            var function = Tools.GetTools().OfType<AIFunction>().Single(t => t.Name == tool);
            var result = await function.InvokeAsync(new AIFunctionArguments(arguments.ToDictionary(a => a.Name, a => (object?)a.Value)));
            return result is System.Text.Json.JsonElement json ? json.GetString()! : result?.ToString()!;
        }

        public async Task<NoteDto> ReadAsync(Guid noteId) => (await Service.Notes.GetAsync(noteId)).Entity;
    }

    private static async Task<Setup> SetupAsync(string orm = "Dapper")
    {
        var fixture = await SemanticIndexFixture.CreateAsync(orm);
        fixture.Service.UserIdentityName = (await fixture.Service.Users.GetAsync(fixture.UserId)).Entity.UserName;
        var home = (await fixture.Service.Folders.GetHomeAsync()).Entity;
        var host = new Host { Destination = new AiNoteDestination(fixture.Service, home) };
        var tools = new KNoteAiTools(fixture.Service, host);
        tools.BeginTurn(null, null);
        return new Setup(fixture, tools, host);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task CreateNote_IsDoneAtOnce_InTheFolderAsked_AndShown(string orm)
    {
        using var setup = await SetupAsync(orm);
        var folder = (await setup.Service.Folders.SaveAsync(new FolderDto { Name = "Sinclair", ParentId = setup.Fixture.HomeId })).Entity;

        var result = await setup.CallAsync(AiActionRules.CreateNote, ("topic", "Gold Card"), ("content", "An accelerator."),
            ("folder", "Sinclair"), ("tags", new[] { "ql", "hardware" }));

        var action = setup.Tools.TurnActions.Single();
        Assert.AreEqual(EnumAiActionStatus.Done, action.Status, action.Result);
        StringAssert.Contains(result, $"Done: created the note #{action.NoteNumber}");
        var note = await setup.ReadAsync(action.NoteId!.Value);
        Assert.AreEqual("Gold Card", note.Topic);
        Assert.AreEqual("An accelerator.", note.Description);
        Assert.AreEqual(folder.FolderId, note.FolderId);
        Assert.AreEqual("ql hardware", note.Tags);
        CollectionAssert.Contains(setup.Host.Created, note.NoteId);
    }

    [TestMethod]
    public async Task CreateNote_WithoutAFolder_GoesToTheDefaultOne_AndAWrongOneGivesTheError()
    {
        using var setup = await SetupAsync();

        await setup.CallAsync(AiActionRules.CreateNote, ("topic", "Note"), ("content", "Text"));
        var wrong = await setup.CallAsync(AiActionRules.CreateNote, ("topic", "Note"), ("content", "Text"), ("folder", "Nowhere"));

        var note = await setup.ReadAsync(setup.Tools.TurnActions.Single().NoteId!.Value);
        Assert.AreEqual(setup.Fixture.HomeId, note.FolderId);
        StringAssert.Contains(wrong, "there is no folder 'Nowhere'");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Append_ChangesNothingUntilItIsAccepted(string orm)
    {
        using var setup = await SetupAsync(orm);
        var note = await setup.Fixture.AddNoteAsync("QL", "First text.");

        var result = await setup.CallAsync(AiActionRules.AppendToNote, ("noteNumber", note.NoteNumber), ("content", "Second text."));

        StringAssert.Contains(result, "NOT done");
        var action = setup.Tools.TurnActions.Single();
        Assert.AreEqual(EnumAiActionStatus.Proposed, action.Status);
        StringAssert.Contains(action.Text, $"#{note.NoteNumber}");
        StringAssert.Contains(action.Text, "Second text.");
        Assert.AreEqual("First text.", (await setup.ReadAsync(note.NoteId)).Description);

        await setup.Tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual(EnumAiActionStatus.Done, action.Status, action.Result);
        Assert.AreEqual("First text.\r\n\r\nSecond text.", (await setup.ReadAsync(note.NoteId)).Description);
        CollectionAssert.Contains(setup.Host.Changed, note.NoteId);
    }

    [TestMethod]
    public async Task Append_InAnHtmlNote_AddsHtml_AndALockedOneIsRefused()
    {
        using var setup = await SetupAsync();
        var html = await setup.Fixture.AddNoteAsync("Html", "<p>Old</p>", contentType: "html");
        var locked = await setup.Fixture.AddNoteAsync("Locked", "Text");
        var read = await setup.ReadAsync(locked.NoteId);
        var type = read.GetContentTypeExt();
        type.DescriptionBlocked = true;
        read.SetContentTypeExt(type);
        Assert.IsTrue((await setup.Service.Notes.SaveAsync(read)).IsValid);

        await setup.CallAsync(AiActionRules.AppendToNote, ("noteNumber", html.NoteNumber), ("content", "**New**"));
        await setup.Tools.ExecuteActionsAsync(setup.Tools.TurnActions);
        var refused = await setup.CallAsync(AiActionRules.AppendToNote, ("noteNumber", locked.NoteNumber), ("content", "More"));

        StringAssert.Contains((await setup.ReadAsync(html.NoteId)).Description, "<strong>New</strong>");
        StringAssert.Contains(refused, KntAiActionExecutor.NoteLocked);
        Assert.AreEqual(1, setup.Tools.TurnActions.Count, "Nothing proposed for the locked note.");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SetTags_AddsAndRemoves_WhenAccepted(string orm)
    {
        using var setup = await SetupAsync(orm);
        var note = await setup.Fixture.AddNoteAsync("MSX", "Text");
        var read = await setup.ReadAsync(note.NoteId);
        read.Tags = "pending hardware";
        Assert.IsTrue((await setup.Service.Notes.SaveAsync(read)).IsValid);

        await setup.CallAsync(AiActionRules.SetTags, ("noteNumber", note.NoteNumber), ("add", new[] { "restored" }), ("remove", new[] { "pending" }));
        var action = setup.Tools.TurnActions.Single();
        Assert.AreEqual("pending hardware", (await setup.ReadAsync(note.NoteId)).Tags);
        await setup.Tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual("hardware restored", (await setup.ReadAsync(note.NoteId)).Tags);
        StringAssert.Contains(await setup.CallAsync(AiActionRules.SetTags, ("noteNumber", note.NoteNumber), ("add", new[] { "hardware" })), "does not change");
    }

    [TestMethod]
    public async Task SystemNotes_AndNotesThatDoNotExist_CannotBeChanged()
    {
        using var setup = await SetupAsync();
        var glossary = (await setup.Service.AtlasContext.CreateGlossaryAsync()).Entity;

        var system = await setup.CallAsync(AiActionRules.AppendToNote, ("noteNumber", glossary.NoteNumber), ("content", "Text"));
        var missing = await setup.CallAsync(AiActionRules.SetTags, ("noteNumber", 999999), ("add", new[] { "x" }));

        StringAssert.Contains(system, KntAiActionExecutor.NoteNotFound);
        StringAssert.Contains(missing, KntAiActionExecutor.NoteNotFound);
        Assert.AreEqual(0, setup.Tools.TurnActions.Count);
    }

    [TestMethod]
    public async Task Execute_LooksAtTheNoteAgain_AndDoesNotRunUnknownKinds()
    {
        using var setup = await SetupAsync();
        var glossary = (await setup.Service.AtlasContext.CreateGlossaryAsync()).Entity;
        var forged = new AiActionDto { Kind = AiActionRules.AppendToNote, NoteId = glossary.NoteId, Content = "Forged", Text = "Forged" };
        var unknown = new AiActionDto { Kind = "delete_note", NoteId = glossary.NoteId, Text = "Delete" };

        await setup.Tools.ExecuteActionsAsync(new[] { forged, unknown });

        Assert.AreEqual(EnumAiActionStatus.Failed, forged.Status);
        Assert.AreEqual(EnumAiActionStatus.Failed, unknown.Status);
        Assert.IsFalse((await setup.ReadAsync(glossary.NoteId)).Description.Contains("Forged"));
    }

    [TestMethod]
    public async Task OneThatFails_DoesNotStopTheOthers()
    {
        using var setup = await SetupAsync();
        var first = await setup.Fixture.AddNoteAsync("First", "Text");
        var second = await setup.Fixture.AddNoteAsync("Second", "Text");
        await setup.CallAsync(AiActionRules.AppendToNote, ("noteNumber", first.NoteNumber), ("content", "More"));
        await setup.CallAsync(AiActionRules.AppendToNote, ("noteNumber", second.NoteNumber), ("content", "More"));
        var actions = setup.Tools.TurnActions;
        Assert.IsTrue((await setup.Service.Notes.DeleteAsync(first.NoteId)).IsValid);

        await setup.Tools.ExecuteActionsAsync(actions);

        Assert.AreEqual(EnumAiActionStatus.Failed, actions[0].Status);
        Assert.AreEqual(EnumAiActionStatus.Done, actions[1].Status);
    }

    [TestMethod]
    public async Task ANoteOpenInAnEditor_IsNotChanged()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        setup.Host.InEditor.Add(note.NoteId);
        await setup.CallAsync(AiActionRules.AppendToNote, ("noteNumber", note.NoteNumber), ("content", "More"));

        await setup.Tools.ExecuteActionsAsync(setup.Tools.TurnActions);

        var action = setup.Tools.TurnActions.Single();
        Assert.AreEqual(EnumAiActionStatus.Failed, action.Status);
        StringAssert.Contains(action.Result, "open in an editor");
        Assert.AreEqual("Text", (await setup.ReadAsync(note.NoteId)).Description);
    }

    [TestMethod]
    public async Task AKindApprovedForTheConversation_IsDoneAtOnce_AndOnlyThatKind()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        setup.Tools.BeginTurn(null, new[] { AiActionRules.SetTags });

        var tags = await setup.CallAsync(AiActionRules.SetTags, ("noteNumber", note.NoteNumber), ("add", new[] { "x" }));
        var append = await setup.CallAsync(AiActionRules.AppendToNote, ("noteNumber", note.NoteNumber), ("content", "More"));

        StringAssert.StartsWith(tags, "Done:");
        StringAssert.Contains(append, "NOT done");
        Assert.AreEqual("x", (await setup.ReadAsync(note.NoteId)).Tags);
    }

    [TestMethod]
    public async Task ANoteCreatedInTheConversation_IsCompletedWithoutAsking_AlsoInLaterTurns()
    {
        using var setup = await SetupAsync();
        await setup.CallAsync(AiActionRules.CreateNote, ("topic", "New"), ("content", "Text"));
        var created = setup.Tools.TurnActions.Single();
        var other = await setup.Fixture.AddNoteAsync("Old", "Text");

        setup.Tools.BeginTurn(new[] { new AiChatTurnDto { Actions = { created } } }, null);
        var onNew = await setup.CallAsync(AiActionRules.AppendToNote, ("noteNumber", created.NoteNumber!.Value), ("content", "More"));
        var onOld = await setup.CallAsync(AiActionRules.AppendToNote, ("noteNumber", other.NoteNumber), ("content", "More"));

        StringAssert.StartsWith(onNew, "Done:");
        StringAssert.Contains(onOld, "NOT done");
    }

    [TestMethod]
    public async Task Roles_AGuestIsNotOfferedTheActions_NorCanRunThem()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var guest = (await setup.Service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(), UserName = "action-guest", EMail = "action-guest@knote.tests", FullName = "Guest", RoleDefinition = "Guest"
        })).Entity;
        var service = new KntService(setup.Fixture.Db.CreateRepository("Dapper")) { UserIdentityName = guest.UserName, EnforceAuthorization = true };
        var tools = new KNoteAiTools(service, setup.Host);

        var names = (await tools.GetToolsAsync()).Select(t => t.Name).ToList();
        var forged = new AiActionDto { Kind = AiActionRules.AppendToNote, NoteId = note.NoteId, Content = "Forged", Text = "Forged" };
        await tools.ExecuteActionsAsync(new[] { forged });

        CollectionAssert.DoesNotContain(names, AiActionRules.CreateNote);
        CollectionAssert.DoesNotContain(names, AiActionRules.AppendToNote);
        Assert.AreEqual(EnumAiActionStatus.Failed, forged.Status);
        Assert.AreEqual("Text", (await setup.ReadAsync(note.NoteId)).Description);
    }

    [TestMethod]
    public async Task CreateTask_IsRecorded_AndItsNoteCountsAsNew()
    {
        using var setup = await SetupAsync();

        var result = await setup.CallAsync("create_task", ("topic", "Reminder"), ("description", "Text"));

        var action = setup.Tools.TurnActions.Single();
        Assert.AreEqual(AiActionRules.CreateTask, action.Kind);
        Assert.AreEqual(EnumAiActionStatus.Done, action.Status);
        StringAssert.Contains(result, $"note #{action.NoteNumber}");
        StringAssert.StartsWith(await setup.CallAsync(AiActionRules.AppendToNote, ("noteNumber", action.NoteNumber!.Value), ("content", "More")), "Done:");
    }

    [TestMethod]
    public void History_ATurnThatEndsInTheCard_SendsNoEmptyMessage()
    {
        var action = new AiActionDto { Kind = AiActionRules.SetTags, Status = EnumAiActionStatus.Done, Text = "Tags of #1", NoteNumber = 1 };
        var history = new[] { new AiChatTurnDto { Prompt = "Q", Answer = "", Actions = new List<AiActionDto> { action } } };

        var roles = AiChatTurnStreamer.BuildMessages(null, history, "Next").Select(m => m.Role).ToList();

        CollectionAssert.AreEqual(new List<ChatRole> { ChatRole.User, ChatRole.Assistant, ChatRole.Tool, ChatRole.User }, roles);
    }

    [TestMethod]
    public async Task AChangeRepeatedByTheModel_IsNotProposedTwice_NorDoneAgainWithTheKindApproved()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        (string, object) number = ("noteNumber", note.NoteNumber), content = ("content", "More");

        await setup.CallAsync(AiActionRules.AppendToNote, number, content);
        var again = await setup.CallAsync(AiActionRules.AppendToNote, number, content);

        StringAssert.StartsWith(again, "Nothing to do");
        var action = setup.Tools.TurnActions.Single();
        await setup.Tools.ExecuteActionsAsync(new[] { action });

        // The turn that goes on after the card, with the kind approved: it would be done at once.
        setup.Tools.BeginTurn(new[] { new AiChatTurnDto { Actions = { action } } }, new[] { AiActionRules.AppendToNote });
        var afterTheCard = await setup.CallAsync(AiActionRules.AppendToNote, number, content);
        var other = await setup.CallAsync(AiActionRules.AppendToNote, number, ("content", "Other"));

        StringAssert.StartsWith(afterTheCard, "Nothing to do");
        StringAssert.StartsWith(other, "Done:");
        var description = (await setup.ReadAsync(note.NoteId)).Description;
        Assert.AreEqual(1, description.Split("More").Length - 1, description);
    }
}
