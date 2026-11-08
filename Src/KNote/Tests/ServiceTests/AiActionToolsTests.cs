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
        public List<Guid> Opened { get; } = new();

        public Task<AiNoteDestination> GetNewNoteDestinationAsync() => Task.FromResult(Destination);
        public void OnNoteCreated(IKntService service, NoteExtendedDto note, bool openEditor = true)
        {
            Created.Add(note.NoteId);
            if (openEditor)
                Opened.Add(note.NoteId);
        }

        public bool CanChangeNote(Guid noteId) => !InEditor.Contains(noteId);
        public void OnNoteChanged(IKntService service, Guid noteId) => Changed.Add(noteId);
        public void OpenNoteEditor(IKntService service, NoteMinimalDto note) => Opened.Add(note.NoteId);
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
        Assert.AreEqual(0, setup.Host.Opened.Count, "A note open in an editor could not be completed: it opens when it is asked for.");
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

    // A 1x1 PNG.
    private static readonly byte[] Png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    private static void UseResources(Setup setup)
    {
        var root = Path.Combine(Path.GetTempPath(), $"knote-ai-tools-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        setup.Service.Repository.RepositoryRef.ResourcesContainer = "AiToolsTestResources";
        setup.Service.Repository.RepositoryRef.ResourcesContainerRootPath = root;
    }

    // A file attached to the conversation, and the turn begun with it.
    private static async Task<AiChatAttachmentDto> AttachToConversationAsync(Setup setup, string name = "photo.png", byte[]? content = null)
    {
        var res = await setup.Service.AiSessions.AddAttachmentAsync(new AiChatAttachmentUploadDto
        {
            Provider = EnumAiProvider.Ollama, Model = "m", FileName = name, ContentType = AiAttachmentRules.ContentTypeOf(name),
            ContentBase64 = Convert.ToBase64String(content ?? Png)
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        setup.Tools.BeginTurn(null, null, res.Entity.SessionNoteId, new[] { res.Entity });
        return res.Entity;
    }

    private static async Task AddImageToNoteAsync(Setup setup, Guid noteId, string name)
    {
        var res = await setup.Service.Notes.SaveResourceAsync(new ResourceDto
        {
            NoteId = noteId, Name = $"{Guid.NewGuid()}_{name}", Description = name, FileType = AiAttachmentRules.ContentTypeOf(name), ContentArrayBytes = Png
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AttachFile_AddsNothingUntilItIsAccepted_AndThenTheFileIsInTheNote(string orm)
    {
        using var setup = await SetupAsync(orm);
        UseResources(setup);
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        await AttachToConversationAsync(setup);

        var result = await setup.CallAsync(AiActionRules.AttachFile, ("noteNumber", note.NoteNumber));

        StringAssert.Contains(result, "NOT done");
        var action = setup.Tools.TurnActions.Single();
        Assert.AreEqual($"Attach photo.png to #{note.NoteNumber} (QL)", action.Text);
        Assert.AreEqual(0, (await setup.Service.Notes.GetResourcesInfoAsync(note.NoteId)).Entity.Count);

        await setup.Tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual(EnumAiActionStatus.Done, action.Status, action.Result);
        var resource = (await setup.Service.Notes.GetResourcesInfoAsync(note.NoteId)).Entity.Single();
        Assert.AreEqual("photo.png", resource.NameOut);
        Assert.AreEqual("image/png", resource.FileType);
        CollectionAssert.AreEqual(Png, (await setup.Service.Notes.GetResourceAsync(resource.ResourceId)).Entity.ContentArrayBytes);
        CollectionAssert.Contains(setup.Host.Changed, note.NoteId);
    }

    [TestMethod]
    public async Task AttachFile_InANoteOfTheConversation_IsDoneAtOnce()
    {
        using var setup = await SetupAsync();
        UseResources(setup);
        var file = await AttachToConversationAsync(setup);
        await setup.CallAsync(AiActionRules.CreateNote, ("topic", "New one"), ("content", "Text"));
        var created = setup.Tools.TurnActions.Single();

        var result = await setup.CallAsync(AiActionRules.AttachFile, ("noteNumber", created.NoteNumber!.Value));

        StringAssert.StartsWith(result, "Done:");
        Assert.AreEqual(1, (await setup.Service.Notes.GetResourcesInfoAsync(created.NoteId!.Value)).Entity.Count);
        Assert.AreEqual(file.SessionNoteId, setup.Tools.TurnActions.Last().SessionNoteId);
    }

    [TestMethod]
    public async Task AttachFile_IsOnlyOffered_WithFilesInTheConversation()
    {
        using var setup = await SetupAsync();
        UseResources(setup);

        var before = (await setup.Tools.GetToolsAsync()).Select(t => t.Name).ToList();
        await AttachToConversationAsync(setup);
        var after = (await setup.Tools.GetToolsAsync()).Select(t => t.Name).ToList();

        CollectionAssert.DoesNotContain(before, AiActionRules.AttachFile);
        CollectionAssert.Contains(after, AiActionRules.AttachFile);
    }

    [TestMethod]
    public async Task AttachFile_ANameThatDoesNotExist_GivesTheFiles_AndALockedNoteIsRefused()
    {
        using var setup = await SetupAsync();
        UseResources(setup);
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var locked = await setup.Fixture.AddNoteAsync("Locked", "Text");
        var read = await setup.ReadAsync(locked.NoteId);
        var type = read.GetContentTypeExt();
        type.DescriptionBlocked = true;
        read.SetContentTypeExt(type);
        await setup.Service.Notes.SaveAsync(read);
        await AttachToConversationAsync(setup);

        var wrong = await setup.CallAsync(AiActionRules.AttachFile, ("noteNumber", note.NoteNumber), ("fileName", "other.png"));
        var refused = await setup.CallAsync(AiActionRules.AttachFile, ("noteNumber", locked.NoteNumber));

        StringAssert.Contains(wrong, "photo.png");
        StringAssert.Contains(refused, KntAiActionExecutor.NoteLocked);
        Assert.AreEqual(0, setup.Tools.TurnActions.Count);
    }

    [TestMethod]
    public async Task AttachFile_ForgedWithAFileThatIsNotOfTheSession_AddsNothing()
    {
        using var setup = await SetupAsync();
        UseResources(setup);
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var file = await AttachToConversationAsync(setup);
        var forged = new AiActionDto { Kind = AiActionRules.AttachFile, NoteId = note.NoteId, SessionNoteId = file.SessionNoteId, ResourceId = Guid.NewGuid(), Text = "Forged" };

        await setup.Tools.ExecuteActionsAsync(new[] { forged });

        Assert.AreEqual(EnumAiActionStatus.Failed, forged.Status);
        Assert.AreEqual(0, (await setup.Service.Notes.GetResourcesInfoAsync(note.NoteId)).Entity.Count);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ViewNoteImages_LooksAtTheFirstOnes_AndSaysHowManyThereAre(string orm)
    {
        using var setup = await SetupAsync(orm);
        UseResources(setup);
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        for (var i = 1; i <= 5; i++)
            await AddImageToNoteAsync(setup, note.NoteId, $"{i}.png");
        await AddImageToNoteAsync(setup, note.NoteId, "manual.pdf");

        var result = await setup.CallAsync("view_note_images", ("noteNumber", note.NoteNumber));

        Assert.AreEqual(AiAttachmentRules.MaxImagesPerLook, setup.Tools.TurnImages.Count);
        StringAssert.Contains(result, $"The note #{note.NoteNumber} has 5 images");
        Assert.IsFalse(result.Contains("manual.pdf"), result);
    }

    [TestMethod]
    public async Task ViewNoteImages_TheOnesAskedFor_AndOnesThatDoNotExist()
    {
        using var setup = await SetupAsync();
        UseResources(setup);
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        await AddImageToNoteAsync(setup, note.NoteId, "a.png");
        await AddImageToNoteAsync(setup, note.NoteId, "b.png");

        var one = await setup.CallAsync("view_note_images", ("noteNumber", note.NoteNumber), ("which", new[] { 2 }));
        var wrong = await setup.CallAsync("view_note_images", ("noteNumber", note.NoteNumber), ("which", new[] { 9 }));

        Assert.AreEqual(1, setup.Tools.TurnImages.Count);
        StringAssert.Contains(one, "2. ");
        StringAssert.Contains(wrong, "there are 2");
    }

    [TestMethod]
    public async Task ViewNoteImages_ANoteWithoutImages_AndANoteOfTheSystem_GiveNothing()
    {
        using var setup = await SetupAsync();
        UseResources(setup);
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var glossary = (await setup.Service.AtlasContext.CreateGlossaryAsync()).Entity;

        var none = await setup.CallAsync("view_note_images", ("noteNumber", note.NoteNumber));
        var system = await setup.CallAsync("view_note_images", ("noteNumber", glossary.NoteNumber));

        StringAssert.Contains(none, "has no images");
        StringAssert.Contains(system, "not found");
        Assert.AreEqual(0, setup.Tools.TurnImages.Count);
    }

    [TestMethod]
    public async Task ViewNoteImages_WithoutANote_LooksAtTheOnesOfTheConversation()
    {
        using var setup = await SetupAsync();
        UseResources(setup);
        var file = await AttachToConversationAsync(setup);

        setup.Tools.BeginTurn(new[] { new AiChatTurnDto { Prompt = "Look", Attachments = { file } } }, null, file.SessionNoteId);
        var result = await setup.CallAsync("view_note_images");

        Assert.AreEqual(1, setup.Tools.TurnImages.Count);
        StringAssert.Contains(result, "This conversation has 1 images");
    }

    [TestMethod]
    public async Task ViewNoteImages_StopsAtTheImagesOfATurn()
    {
        using var setup = await SetupAsync();
        UseResources(setup);
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        for (var i = 1; i <= AiToolImages.MaxPerTurn; i++)
            await AddImageToNoteAsync(setup, note.NoteId, $"{i}.png");

        for (var from = 1; from <= AiToolImages.MaxPerTurn; from += AiAttachmentRules.MaxImagesPerLook)
            await setup.CallAsync("view_note_images", ("noteNumber", note.NoteNumber), ("which", new[] { from, from + 1, from + 2 }));
        var more = await setup.CallAsync("view_note_images", ("noteNumber", note.NoteNumber));

        Assert.AreEqual(AiToolImages.MaxPerTurn, setup.Tools.TurnImages.Count);
        StringAssert.StartsWith(more, "Error:");
    }

    [TestMethod]
    public async Task ViewNoteImages_IsNotOffered_ToAModelThatDoesNotSeeThem()
    {
        using var setup = await SetupAsync();

        setup.Tools.ModelSeesImages = false;

        CollectionAssert.DoesNotContain((await setup.Tools.GetToolsAsync()).Select(t => t.Name).ToList(), "view_note_images");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Replace_ChangesNothingUntilItIsAccepted_AndThenOnlyTheText(string orm)
    {
        using var setup = await SetupAsync(orm);
        var note = await setup.Fixture.AddNoteAsync("QL", "First text.");
        var read = await setup.ReadAsync(note.NoteId);
        read.Tags = "ql";
        await setup.Service.Notes.SaveAsync(read);

        var result = await setup.CallAsync(AiActionRules.ReplaceNoteText, ("noteNumber", note.NoteNumber), ("content", "All new."));

        StringAssert.Contains(result, "NOT done");
        var action = setup.Tools.TurnActions.Single();
        Assert.AreEqual(EnumAiActionStatus.Proposed, action.Status);
        Assert.AreEqual($"Replace the whole text of #{note.NoteNumber} (QL), 11 → 8 characters", action.Text);
        Assert.AreEqual("First text.", (await setup.ReadAsync(note.NoteId)).Description);

        await setup.Tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual(EnumAiActionStatus.Done, action.Status, action.Result);
        var replaced = await setup.ReadAsync(note.NoteId);
        Assert.AreEqual("All new.", replaced.Description);
        Assert.AreEqual("QL", replaced.Topic);
        Assert.AreEqual("ql", replaced.Tags);
        CollectionAssert.Contains(setup.Host.Changed, note.NoteId);
    }

    [TestMethod]
    public async Task Replace_InAnHtmlNote_WritesHtml_AndALockedOneIsRefused()
    {
        using var setup = await SetupAsync();
        var html = await setup.Fixture.AddNoteAsync("Html", "<p>Old</p>", contentType: "html");
        var locked = await setup.Fixture.AddNoteAsync("Locked", "Text");
        var read = await setup.ReadAsync(locked.NoteId);
        var type = read.GetContentTypeExt();
        type.DescriptionBlocked = true;
        read.SetContentTypeExt(type);
        await setup.Service.Notes.SaveAsync(read);

        await setup.CallAsync(AiActionRules.ReplaceNoteText, ("noteNumber", html.NoteNumber), ("content", "**New**"));
        var refused = await setup.CallAsync(AiActionRules.ReplaceNoteText, ("noteNumber", locked.NoteNumber), ("content", "More"));

        await setup.Tools.ExecuteActionsAsync(setup.Tools.TurnActions);

        var description = (await setup.ReadAsync(html.NoteId)).Description;
        StringAssert.Contains(description, "<strong>New</strong>");
        Assert.IsFalse(description.Contains("Old"), description);
        StringAssert.Contains(refused, KntAiActionExecutor.NoteLocked);
        Assert.AreEqual(1, setup.Tools.TurnActions.Count);
    }

    [TestMethod]
    public async Task Replace_AsksInANoteOfTheConversation_AndNotWithItsKindApproved()
    {
        using var setup = await SetupAsync();
        await setup.CallAsync(AiActionRules.CreateNote, ("topic", "New one"), ("content", "Text"));
        var created = setup.Tools.TurnActions.Single();
        var old = await setup.Fixture.AddNoteAsync("Old one", "Text");

        setup.Tools.BeginTurn(new[] { new AiChatTurnDto { Actions = { created } } }, null);
        var onNew = await setup.CallAsync(AiActionRules.ReplaceNoteText, ("noteNumber", created.NoteNumber!.Value), ("content", "Other"));

        setup.Tools.BeginTurn(null, new[] { AiActionRules.ReplaceNoteText });
        var approved = await setup.CallAsync(AiActionRules.ReplaceNoteText, ("noteNumber", old.NoteNumber), ("content", "Other"));

        StringAssert.Contains(onNew, "NOT done");
        Assert.AreEqual("Text", (await setup.ReadAsync(created.NoteId!.Value)).Description);
        StringAssert.StartsWith(approved, "Done:");
        Assert.AreEqual("Other", (await setup.ReadAsync(old.NoteId)).Description);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Move_ChangesNothingUntilItIsAccepted_AndThenOnlyTheFolder(string orm)
    {
        using var setup = await SetupAsync(orm);
        var folder = (await setup.Service.Folders.SaveAsync(new FolderDto { Name = "Sinclair", ParentId = setup.Fixture.HomeId })).Entity;
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var from = (await setup.ReadAsync(note.NoteId)).FolderId;

        var result = await setup.CallAsync(AiActionRules.MoveNote, ("noteNumber", note.NoteNumber), ("folder", "Sinclair"));

        StringAssert.Contains(result, "NOT done");
        var action = setup.Tools.TurnActions.Single();
        StringAssert.Contains(action.Text, $"Move #{note.NoteNumber} (QL)");
        StringAssert.Contains(action.Text, "Sinclair");
        Assert.AreEqual(from, (await setup.ReadAsync(note.NoteId)).FolderId);

        await setup.Tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual(EnumAiActionStatus.Done, action.Status, action.Result);
        var moved = await setup.ReadAsync(note.NoteId);
        Assert.AreEqual(folder.FolderId, moved.FolderId);
        Assert.AreEqual("Text", moved.Description);
        CollectionAssert.Contains(setup.Host.Changed, note.NoteId);
    }

    [TestMethod]
    public async Task Move_ToTheFolderItIsIn_OrToOneThatDoesNotExist_IsNotProposed()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var folderName = (await setup.Service.Folders.GetAsync((await setup.ReadAsync(note.NoteId)).FolderId)).Entity.Name;

        var same = await setup.CallAsync(AiActionRules.MoveNote, ("noteNumber", note.NoteNumber), ("folder", folderName));
        var wrong = await setup.CallAsync(AiActionRules.MoveNote, ("noteNumber", note.NoteNumber), ("folder", "Nowhere"));

        StringAssert.Contains(same, "is already in");
        StringAssert.Contains(wrong, "there is no folder 'Nowhere'");
        Assert.AreEqual(0, setup.Tools.TurnActions.Count);
    }

    [TestMethod]
    public async Task Move_ForgedToAFolderOfTheSystem_ChangesNothing()
    {
        using var setup = await SetupAsync();
        var system = (await setup.Service.Folders.SaveAsync(new FolderDto { Name = KntConst.SystemFolderNames.First(), ParentId = setup.Fixture.HomeId })).Entity;
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var from = (await setup.ReadAsync(note.NoteId)).FolderId;
        var forged = new AiActionDto { Kind = AiActionRules.MoveNote, NoteId = note.NoteId, FolderId = system.FolderId, Text = "Forged" };

        await setup.Tools.ExecuteActionsAsync(new[] { forged });

        Assert.AreEqual(EnumAiActionStatus.Failed, forged.Status);
        Assert.AreEqual(from, (await setup.ReadAsync(note.NoteId)).FolderId);
    }

    [TestMethod]
    public async Task OpenNoteEditor_ShowsTheNote_WithoutACardOrARecord()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");

        var result = await setup.CallAsync(AiActionRules.OpenNoteEditor, ("noteNumber", note.NoteNumber));

        StringAssert.StartsWith(result, "Done:");
        CollectionAssert.AreEqual(new[] { note.NoteId }, setup.Host.Opened);
        Assert.AreEqual(0, setup.Tools.TurnActions.Count);
    }

    [TestMethod]
    public async Task OpenNoteEditor_ANoteOfTheSystem_IsNotFound()
    {
        using var setup = await SetupAsync();
        var glossary = (await setup.Service.AtlasContext.CreateGlossaryAsync()).Entity;

        var result = await setup.CallAsync(AiActionRules.OpenNoteEditor, ("noteNumber", glossary.NoteNumber));

        StringAssert.Contains(result, KntAiActionExecutor.NoteNotFound);
        Assert.AreEqual(0, setup.Host.Opened.Count);
    }

    [TestMethod]
    public async Task AnActionDoneWithoutACard_TellsTheModelThatNothingIsWaiting()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        setup.Tools.BeginTurn(null, new[] { AiActionRules.SetTags });

        var result = await setup.CallAsync(AiActionRules.SetTags, ("noteNumber", note.NoteNumber), ("add", new[] { "ql" }));

        Assert.AreEqual(EnumAiActionStatus.Done, setup.Tools.TurnActions.Single().Status);
        StringAssert.StartsWith(result, "Done:");
        StringAssert.Contains(result, "nothing is waiting for the user");
    }

    [TestMethod]
    public async Task SetTags_WithoutTags_SaysWhatIsMissing_AndNotThatTheyAreThere()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");

        var result = await setup.CallAsync(AiActionRules.SetTags, ("noteNumber", note.NoteNumber));

        StringAssert.Contains(result, "this call has no tags");
        Assert.AreEqual(0, setup.Tools.TurnActions.Count);
    }

    [TestMethod]
    public void TheHistory_ShowsTheTagsOfACall_WhenTheTurnHasThem()
    {
        var withTags = new AiActionDto { Kind = AiActionRules.SetTags, Status = EnumAiActionStatus.Done, Text = "Tags", NoteNumber = 7, AddTags = new() { "ql" }, RemoveTags = new() { "old" } };
        var saved = new AiActionDto { Kind = AiActionRules.SetTags, Status = EnumAiActionStatus.Done, Text = "Tags", NoteNumber = 7 };
        var history = new[] { new AiChatTurnDto { Prompt = "Tag it", Answer = "Done", Actions = { withTags, saved } } };

        var calls = AiChatTurnStreamer.BuildMessages(null, history, "Next").SelectMany(m => m.Contents).OfType<FunctionCallContent>().ToList();

        CollectionAssert.AreEqual(new[] { "ql" }, (List<string>)calls[0].Arguments!["add"]!);
        CollectionAssert.AreEqual(new[] { "old" }, (List<string>)calls[0].Arguments!["remove"]!);
        Assert.AreEqual(7, calls[1].Arguments!["noteNumber"]);
        Assert.IsFalse(calls[1].Arguments!.ContainsKey("add"));
    }

    [TestMethod]
    public async Task Replace_WithNoTextOrTheSameOne_IsNotProposed()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Same text.\r\n");

        var empty = await setup.CallAsync(AiActionRules.ReplaceNoteText, ("noteNumber", note.NoteNumber), ("content", "  "));
        var same = await setup.CallAsync(AiActionRules.ReplaceNoteText, ("noteNumber", note.NoteNumber), ("content", "Same text."));

        StringAssert.StartsWith(empty, "Error:");
        StringAssert.Contains(same, "already has that text");
        Assert.AreEqual(0, setup.Tools.TurnActions.Count);
    }

    [TestMethod]
    public async Task Replace_IfTheNoteChangedWhileTheCardWaited_LeavesWhatWasWritten()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "First text.");
        await setup.CallAsync(AiActionRules.ReplaceNoteText, ("noteNumber", note.NoteNumber), ("content", "All new."));
        var action = setup.Tools.TurnActions.Single();

        var read = await setup.ReadAsync(note.NoteId);
        read.Description = "Written meanwhile.";
        await setup.Service.Notes.SaveAsync(read);
        await setup.Tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual(EnumAiActionStatus.Failed, action.Status);
        Assert.AreEqual(KntAiActionExecutor.NoteChanged, action.Result);
        Assert.AreEqual("Written meanwhile.", (await setup.ReadAsync(note.NoteId)).Description);
    }

    [TestMethod]
    public async Task Replace_ThatArrivesWithoutItsData_ChangesNothing()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "First text.");
        var noStamp = new AiActionDto { Kind = AiActionRules.ReplaceNoteText, NoteId = note.NoteId, Content = "Forged", Text = "Forged" };
        var noText = new AiActionDto { Kind = AiActionRules.ReplaceNoteText, NoteId = note.NoteId, TextStamp = AiActionRules.TextStamp("First text."), Text = "Forged" };

        await setup.Tools.ExecuteActionsAsync(new[] { noStamp, noText });

        Assert.AreEqual(EnumAiActionStatus.Failed, noStamp.Status);
        Assert.AreEqual(EnumAiActionStatus.Failed, noText.Status);
        Assert.AreEqual("First text.", (await setup.ReadAsync(note.NoteId)).Description);
    }

    [TestMethod]
    public async Task Replace_ANoteOpenInAnEditor_IsNotDone()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "First text.");
        await setup.CallAsync(AiActionRules.ReplaceNoteText, ("noteNumber", note.NoteNumber), ("content", "All new."));
        var action = setup.Tools.TurnActions.Single();
        setup.Host.InEditor.Add(note.NoteId);

        await setup.Tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual(EnumAiActionStatus.Failed, action.Status);
        Assert.AreEqual("First text.", (await setup.ReadAsync(note.NoteId)).Description);
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
    public void History_NamesTheFilesOfAMessage_AndSendsNoImages()
    {
        var history = new[]
        {
            new AiChatTurnDto { Prompt = "What is it?", Answer = "A QL", Attachments = { new AiChatAttachmentDto { FileName = "photo.jpg", ContentType = "image/jpeg" } } }
        };

        var messages = AiChatTurnStreamer.BuildMessages(null, history, "Next");

        StringAssert.Contains(messages[0].Text, "[Attached to this message: photo.jpg]");
        Assert.IsFalse(messages.SelectMany(m => m.Contents).OfType<DataContent>().Any(), "An image only goes in its own turn.");
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

    // The tools of a user of that role, with the commands asking for it as in the Server's chat.
    private static async Task<(KNoteAiTools Tools, KntService Service)> ToolsOfAsync(Setup setup, EnumRoles role)
    {
        var name = $"action-{role}".ToLowerInvariant();
        var user = (await setup.Service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(), UserName = name, EMail = $"{name}@knote.tests", FullName = $"Action {role}", RoleDefinition = role.ToString()
        })).Entity;
        var service = new KntService(setup.Fixture.Db.CreateRepository("Dapper")) { UserIdentityName = user.UserName, EnforceAuthorization = true };
        var tools = new KNoteAiTools(service, setup.Host);
        tools.BeginTurn(null, null);
        return (tools, service);
    }

    private static async Task<string> CallAsync(KNoteAiTools tools, string tool, params (string Name, object Value)[] arguments)
    {
        var function = tools.GetTools().OfType<AIFunction>().Single(t => t.Name == tool);
        var result = await function.InvokeAsync(new AIFunctionArguments(arguments.ToDictionary(a => a.Name, a => (object?)a.Value)));
        return result is System.Text.Json.JsonElement json ? json.GetString()! : result?.ToString()!;
    }

    private static async Task<UserDto> CurrentUserAsync(Setup setup) => await setup.Service.GetCurrentUserAsync();

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AddTask_IsDoneWhenAccepted_WithItsUserAndDate_AndTheStatusOfTheNote(string orm)
    {
        using var setup = await SetupAsync(orm);
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var before = (await setup.ReadAsync(note.NoteId)).InternalTags;

        var result = await setup.CallAsync(AiActionRules.AddTask, ("noteNumber", note.NoteNumber), ("description", "Check the PSU"), ("dueDate", "2026-11-13"));

        StringAssert.Contains(result, "NOT done");
        var action = setup.Tools.TurnActions.Single();
        StringAssert.Contains(action.Text, "Friday, 2026-11-13");
        Assert.AreEqual(0, (await setup.Service.Notes.GetNoteTasksAsync(note.NoteId)).Entity.Count);

        await setup.Tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual(EnumAiActionStatus.Done, action.Status, action.Result);
        var task = (await setup.Service.Notes.GetNoteTasksAsync(note.NoteId)).Entity.Single();
        Assert.AreEqual("Check the PSU", task.Description);
        Assert.AreEqual((await CurrentUserAsync(setup)).UserId, task.UserId);
        Assert.AreEqual(new DateTime(2026, 11, 13), task.ExpectedEndDate);
        Assert.AreEqual(1, task.Priority);
        Assert.IsNotNull(task.StartDate);
        Assert.AreNotEqual(before, (await setup.ReadAsync(note.NoteId)).InternalTags, "The status of the note says it has a task to do.");
    }

    [TestMethod]
    public async Task AddTask_ForSomebodyElse_ByName_AndOneWhoIsNotThereGivesTheUsers()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var luis = (await setup.Service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(), UserName = "luis", EMail = "luis@knote.tests", FullName = "Luis Pérez", RoleDefinition = "Staff"
        })).Entity;

        await setup.CallAsync(AiActionRules.AddTask, ("noteNumber", note.NoteNumber), ("description", "Review"), ("user", "luis perez"));
        var missing = await setup.CallAsync(AiActionRules.AddTask, ("noteNumber", note.NoteNumber), ("description", "Review"), ("user", "Marta"));

        var action = setup.Tools.TurnActions.Single();
        Assert.AreEqual(luis.UserId, action.UserId);
        StringAssert.Contains(action.Text, "Luis Pérez");
        StringAssert.Contains(missing, "there is no user 'Marta'");
        StringAssert.Contains(missing, "Luis Pérez");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AddAlarm_IsAnAppInfoNotice_ForTheUserWhoRunsIt(string orm)
    {
        using var setup = await SetupAsync(orm);
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var when = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd") + " 09:00";

        await setup.CallAsync(AiActionRules.AddAlarm, ("noteNumber", note.NoteNumber), ("message", "Review topic 3"), ("when", when), ("repeat", "weekly"));
        var action = setup.Tools.TurnActions.Single();
        StringAssert.Contains(action.Text, "(every week)");
        Assert.AreEqual(0, (await setup.Service.Notes.GetMessagesAsync(note.NoteId)).Entity.Count);

        // What comes back from the Web cannot choose who gets it.
        action.UserId = Guid.NewGuid();
        await setup.Tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual(EnumAiActionStatus.Done, action.Status, action.Result);
        var alarm = (await setup.Service.Notes.GetMessagesAsync(note.NoteId)).Entity.Single();
        Assert.AreEqual(EnumNotificationType.AppInfo, alarm.NotificationType);
        Assert.AreEqual(EnumAlarmType.Weekly, alarm.AlarmType);
        Assert.AreEqual("Review topic 3", alarm.Comment);
        Assert.AreEqual(true, alarm.AlarmActivated);
        Assert.AreEqual((await CurrentUserAsync(setup)).UserId, alarm.UserId);
    }

    [TestMethod]
    public async Task AddEmailAlarm_IsAnotherKind_AndAsksEvenWithAlarmsApproved_AndOnANewNote()
    {
        using var setup = await SetupAsync();
        await setup.CallAsync(AiActionRules.CreateNote, ("topic", "New"), ("content", "Text"));
        var created = setup.Tools.TurnActions.Single();
        var number = created.NoteNumber!.Value;

        setup.Tools.BeginTurn(new[] { new AiChatTurnDto { Actions = { created } } }, new[] { AiActionRules.AddAlarm });
        var notice = await setup.CallAsync(AiActionRules.AddAlarm, ("noteNumber", number), ("message", "Notice"), ("minutesFromNow", 60));
        var email = await setup.CallAsync(AiActionRules.AddEmailAlarm, ("noteNumber", number), ("message", "Mail"), ("minutesFromNow", 60));

        StringAssert.StartsWith(notice, "Done:");
        StringAssert.Contains(email, "NOT done");
        var action = setup.Tools.TurnActions.Single(a => a.Kind == AiActionRules.AddEmailAlarm);
        StringAssert.Contains(action.Text, "email alarms are on");

        await setup.Tools.ExecuteActionsAsync(new[] { action });

        var alarms = (await setup.Service.Notes.GetMessagesAsync(created.NoteId!.Value)).Entity;
        Assert.AreEqual(EnumNotificationType.Email, alarms.Single(a => a.Comment == "Mail").NotificationType);
    }

    [TestMethod]
    public async Task AddAlarm_WithAWrongDate_ProposesNothing()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");

        var past = await setup.CallAsync(AiActionRules.AddAlarm, ("noteNumber", note.NoteNumber), ("message", "M"), ("when", "2020-01-01 09:00"));

        StringAssert.Contains(past, "has passed");
        Assert.AreEqual(0, setup.Tools.TurnActions.Count);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SetAttribute_ShowsTheChange_AndIsDoneWhenAccepted(string orm)
    {
        using var setup = await SetupAsync(orm);
        var type = (await setup.Service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = "Machine" })).Entity;
        var id = Guid.NewGuid();
        var attribute = new KAttributeDto
        {
            KAttributeId = id, Name = "Estado", NoteTypeId = type.NoteTypeId, KAttributeDataType = EnumKAttributeDataType.TabulatedValue,
            KAttributeValues = new[] { "Funciona", "En reparación" }.Select((v, i) => new KAttributeTabulatedValueDto
            {
                KAttributeTabulatedValueId = Guid.NewGuid(), KAttributeId = id, Value = v, Order = i
            }).ToList()
        };
        Assert.IsTrue((await setup.Service.Repository.KAttributes.AddAsync(attribute)).IsValid);
        var note = await setup.Fixture.AddNoteAsync("MSX", "Text", noteTypeId: type.NoteTypeId);

        var wrong = await setup.CallAsync(AiActionRules.SetAttribute, ("noteNumber", note.NoteNumber), ("attribute", "Estado"), ("value", "Roto"));
        var noName = await setup.CallAsync(AiActionRules.SetAttribute, ("noteNumber", note.NoteNumber), ("attribute", "Color"), ("value", "Rojo"));
        await setup.CallAsync(AiActionRules.SetAttribute, ("noteNumber", note.NoteNumber), ("attribute", "estado"), ("value", "en reparacion"));

        StringAssert.Contains(wrong, "Funciona, En reparación");
        StringAssert.Contains(noName, "Estado");
        var action = setup.Tools.TurnActions.Single();
        StringAssert.Contains(action.Text, "(empty) → En reparación");
        Assert.IsTrue(string.IsNullOrEmpty((await setup.ReadAsync(note.NoteId)).KAttributesDto.Single().Value));

        await setup.Tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual(EnumAiActionStatus.Done, action.Status, action.Result);
        Assert.AreEqual("En reparación", (await setup.ReadAsync(note.NoteId)).KAttributesDto.Single().Value);
        StringAssert.Contains(await setup.CallAsync(AiActionRules.SetAttribute, ("noteNumber", note.NoteNumber), ("attribute", "Estado"), ("value", "En reparación")), "already has that value");
    }

    [TestMethod]
    public async Task SetAttribute_InANoteWithoutThem_SaysSo()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");

        StringAssert.Contains(await setup.CallAsync(AiActionRules.SetAttribute, ("noteNumber", note.NoteNumber), ("attribute", "Estado"), ("value", "X")), "has no attributes");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task RelateNotes_IsDoneWhenAccepted_InItsDirection_WithItsType(string orm)
    {
        using var setup = await SetupAsync(orm);
        var ticket = await setup.Fixture.AddNoteAsync("Ticket", "Text");
        var decision = await setup.Fixture.AddNoteAsync("Decision", "Text");
        var type = (await setup.Service.TraceNoteTypes.SaveAsync(new TraceNoteTypeDto { Name = "Depende de" })).Entity;

        var wrongType = await setup.CallAsync(AiActionRules.RelateNotes, ("noteNumber", ticket.NoteNumber), ("relatedNoteNumber", decision.NoteNumber), ("relationType", "Otro"));
        await setup.CallAsync(AiActionRules.RelateNotes, ("noteNumber", ticket.NoteNumber), ("relatedNoteNumber", decision.NoteNumber), ("relationType", "depende de"));

        StringAssert.Contains(wrongType, "Depende de");
        var action = setup.Tools.TurnActions.Single();
        StringAssert.Contains(action.Text, $"Relate #{ticket.NoteNumber} (Ticket) to #{decision.NoteNumber} (Decision), as Depende de");
        Assert.AreEqual(0, (await setup.Service.Notes.GetTraceNotesToAsync(ticket.NoteId)).Entity.Count);

        await setup.Tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual(EnumAiActionStatus.Done, action.Status, action.Result);
        var trace = (await setup.Service.Notes.GetTraceNotesToAsync(ticket.NoteId)).Entity.Single();
        Assert.AreEqual(decision.NoteId, trace.ToId);
        Assert.AreEqual(type.TraceNoteTypeId, trace.TraceNoteTypeId);
    }

    [TestMethod]
    public async Task RelateNotes_NotWithItself_NorWithASystemNote_ButALockedOneCanBeTheDestination()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var locked = await setup.Fixture.AddNoteAsync("Locked", "Text");
        var read = await setup.ReadAsync(locked.NoteId);
        var content = read.GetContentTypeExt();
        content.DescriptionBlocked = true;
        read.SetContentTypeExt(content);
        Assert.IsTrue((await setup.Service.Notes.SaveAsync(read)).IsValid);
        var glossary = (await setup.Service.AtlasContext.CreateGlossaryAsync()).Entity;

        var itself = await setup.CallAsync(AiActionRules.RelateNotes, ("noteNumber", note.NoteNumber), ("relatedNoteNumber", note.NoteNumber));
        var system = await setup.CallAsync(AiActionRules.RelateNotes, ("noteNumber", note.NoteNumber), ("relatedNoteNumber", glossary.NoteNumber));
        var fromLocked = await setup.CallAsync(AiActionRules.RelateNotes, ("noteNumber", locked.NoteNumber), ("relatedNoteNumber", note.NoteNumber));
        var toLocked = await setup.CallAsync(AiActionRules.RelateNotes, ("noteNumber", note.NoteNumber), ("relatedNoteNumber", locked.NoteNumber));

        StringAssert.Contains(itself, "itself");
        StringAssert.Contains(system, KntAiActionExecutor.NoteNotFound);
        StringAssert.Contains(fromLocked, KntAiActionExecutor.NoteLocked);
        StringAssert.Contains(toLocked, "NOT done");
    }

    [TestMethod]
    public async Task CreateFolder_AStaffIsToldWhy_WithoutACard()
    {
        using var setup = await SetupAsync();
        var (tools, _) = await ToolsOfAsync(setup, EnumRoles.Staff);

        var names = (await tools.GetToolsAsync()).Select(t => t.Name).ToList();
        var result = await CallAsync(tools, AiActionRules.CreateFolder, ("name", "Tema 5"));

        CollectionAssert.Contains(names, AiActionRules.CreateFolder);
        StringAssert.Contains(result, "does not allow it");
        StringAssert.Contains(result, nameof(EnumRoles.ProjectManager));
        Assert.AreEqual(0, tools.TurnActions.Count);
    }

    [TestMethod]
    public async Task CreateFolder_AForgedOneOfAStaff_IsStoppedByTheCommand()
    {
        using var setup = await SetupAsync();
        var (tools, _) = await ToolsOfAsync(setup, EnumRoles.Staff);
        var forged = new AiActionDto { Kind = AiActionRules.CreateFolder, Topic = "Forged", FolderId = setup.Fixture.HomeId, Text = "Forged" };

        await tools.ExecuteActionsAsync(new[] { forged });

        Assert.AreEqual(EnumAiActionStatus.Failed, forged.Status);
        Assert.IsFalse((await setup.Service.Folders.GetAllAsync()).Entity.Any(f => f.Name == "Forged"));
    }

    [TestMethod]
    [DataRow("Dapper")]
    public async Task CreateFolder_AProjectManager_InTheParentAsked_WhenAccepted(string orm)
    {
        using var setup = await SetupAsync(orm);
        var history = (await setup.Service.Folders.SaveAsync(new FolderDto { Name = "Historia", ParentId = setup.Fixture.HomeId })).Entity;
        var (tools, service) = await ToolsOfAsync(setup, EnumRoles.ProjectManager);

        var result = await CallAsync(tools, AiActionRules.CreateFolder, ("name", "Tema 5"), ("parent", "Historia"));
        var again = await CallAsync(tools, AiActionRules.CreateFolder, ("name", "Tema 5"), ("parent", "Historia"));

        StringAssert.Contains(result, "NOT done");
        StringAssert.StartsWith(again, "Nothing to do");
        var action = tools.TurnActions.Single();
        StringAssert.EndsWith(action.Text, @"in Home\Historia");
        Assert.IsFalse((await service.Folders.GetAllAsync()).Entity.Any(f => f.Name == "Tema 5"));

        await tools.ExecuteActionsAsync(new[] { action });

        Assert.AreEqual(EnumAiActionStatus.Done, action.Status, action.Result);
        Assert.AreEqual(history.FolderId, (await service.Folders.GetAllAsync()).Entity.Single(f => f.Name == "Tema 5").ParentId);

        tools.BeginTurn(null, null);
        StringAssert.Contains(await CallAsync(tools, AiActionRules.CreateFolder, ("name", "tema 5"), ("parent", "Historia")), "already a folder");
    }

    [TestMethod]
    public async Task CreateFolder_NotInTheFoldersOfTheSystem()
    {
        using var setup = await SetupAsync();
        var system = (await setup.Service.Folders.SaveAsync(new FolderDto { Name = KntConst.AtlasFolderName, ParentId = setup.Fixture.HomeId })).Entity;
        var (tools, _) = await ToolsOfAsync(setup, EnumRoles.ProjectManager);
        var forged = new AiActionDto { Kind = AiActionRules.CreateFolder, Topic = "Inside", FolderId = system.FolderId, Text = "Forged" };

        var result = await CallAsync(tools, AiActionRules.CreateFolder, ("name", "Inside"), ("parent", KntConst.AtlasFolderName));
        await tools.ExecuteActionsAsync(new[] { forged });

        StringAssert.Contains(result, "there is no folder");
        Assert.AreEqual(EnumAiActionStatus.Failed, forged.Status);
        Assert.IsFalse((await setup.Service.Folders.GetAllAsync()).Entity.Any(f => f.Name == "Inside"));
    }

    [TestMethod]
    public async Task AStaff_CanRunTheActionsOnNotes_WithTheCommandsAskingForItsRole()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        var other = await setup.Fixture.AddNoteAsync("Other", "Text");
        var (tools, service) = await ToolsOfAsync(setup, EnumRoles.Staff);

        await CallAsync(tools, AiActionRules.AddTask, ("noteNumber", note.NoteNumber), ("description", "Do it"));
        await CallAsync(tools, AiActionRules.AddAlarm, ("noteNumber", note.NoteNumber), ("message", "Notice"), ("minutesFromNow", 30));
        await CallAsync(tools, AiActionRules.AddEmailAlarm, ("noteNumber", note.NoteNumber), ("message", "Mail"), ("minutesFromNow", 30));
        await CallAsync(tools, AiActionRules.RelateNotes, ("noteNumber", note.NoteNumber), ("relatedNoteNumber", other.NoteNumber));

        Assert.AreEqual(4, tools.TurnActions.Count);
        await tools.ExecuteActionsAsync(tools.TurnActions);

        foreach (var action in tools.TurnActions)
            Assert.AreEqual(EnumAiActionStatus.Done, action.Status, $"{action.Kind}: {action.Result}");
        Assert.AreEqual(2, (await service.Notes.GetMessagesAsync(note.NoteId)).Entity.Count);
    }

    [TestMethod]
    public async Task ANewNote_IsCompletedWithoutACard_ButARepeatedTaskIsNotAddedAgain()
    {
        using var setup = await SetupAsync();
        await setup.CallAsync(AiActionRules.CreateNote, ("topic", "New"), ("content", "Text"));
        var created = setup.Tools.TurnActions.Single();
        var number = created.NoteNumber!.Value;

        var again = await setup.CallAsync(AiActionRules.CreateNote, ("topic", "New"), ("content", "Text"));
        var task = await setup.CallAsync(AiActionRules.AddTask, ("noteNumber", number), ("description", "Do it"));
        var repeated = await setup.CallAsync(AiActionRules.AddTask, ("noteNumber", number), ("description", "Do it"));

        StringAssert.StartsWith(again, "Nothing to do");
        StringAssert.StartsWith(repeated, "Nothing to do");
        StringAssert.StartsWith(task, "Done:", "A note created in this conversation needs no card.");
        Assert.AreEqual(1, (await setup.Service.Notes.GetNoteTasksAsync(created.NoteId!.Value)).Entity.Count);
    }

    [TestMethod]
    public async Task ANoteOpenInAnEditor_GetsNoTask()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("QL", "Text");
        await setup.CallAsync(AiActionRules.AddTask, ("noteNumber", note.NoteNumber), ("description", "Do it"));
        setup.Host.InEditor.Add(note.NoteId);

        await setup.Tools.ExecuteActionsAsync(setup.Tools.TurnActions);

        Assert.AreEqual(EnumAiActionStatus.Failed, setup.Tools.TurnActions.Single().Status);
        Assert.AreEqual(0, (await setup.Service.Notes.GetNoteTasksAsync(note.NoteId)).Entity.Count);
    }

    [TestMethod]
    public void History_AnActionWithoutANote_SendsNoNumber()
    {
        var action = new AiActionDto { Kind = AiActionRules.CreateFolder, Status = EnumAiActionStatus.Done, Text = "Create the folder X in Home" };
        var history = new[] { new AiChatTurnDto { Prompt = "Q", Answer = "A", Actions = new List<AiActionDto> { action } } };

        var call = AiChatTurnStreamer.BuildMessages(null, history, "Next").SelectMany(m => m.Contents).OfType<FunctionCallContent>().Single();

        Assert.AreEqual(0, call.Arguments!.Count);
    }
}
