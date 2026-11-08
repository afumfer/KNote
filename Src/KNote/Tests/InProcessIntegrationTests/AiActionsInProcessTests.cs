using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Ai;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The actions of the assistant through the API (KNote Atlas, 1.4): a proposal comes in the completed turn and changes
/// nothing, what is accepted is run by POST actions with the roles of the user, and the model is told what happened.
/// The model is a ScriptedAiChatClientProvider that really calls the tools.
/// </summary>
[TestClass]
public class AiActionsInProcessTests
{
    private const string Password = "InProcess-Actions-Password-1!";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _admin = null!;
    private static readonly ScriptedAiChatClientProvider _chatClients = new();
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _admin) = await InProcessTestHost.CreateAuthenticatedClientAsync(factory =>
        {
            // Several tests need more than one user, and each one signs in.
            factory.AppConfiguration["RateLimits:SignInPerMinute"] = "1000";
            factory.AppConfiguration["ai:providers:0:alias"] = "Fake provider";
            factory.AppConfiguration["ai:providers:0:provider"] = EnumAiProvider.OpenAI;
            factory.AppConfiguration["ai:providers:0:model"] = "fake-model";
            factory.AppConfiguration["ai:providers:0:apiKey"] = "test-key";
            factory.TestServices = services => services.AddSingleton<IAiChatClientProvider>(_chatClients);
        });
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _admin.Dispose();
        await _factory.DisposeAsync();
    }

    private static async Task<NoteDto> FirstNoteAsync()
    {
        using var service = _factory.CreateServiceForRepository(0);
        return (await service.Notes.GetAsync((await service.Notes.HomeNotesAsync()).Entity.First().NoteId)).Entity;
    }

    private static async Task<NoteDto> ReadAsync(Guid noteId)
    {
        using var service = _factory.CreateServiceForRepository(0);
        return (await service.Notes.GetAsync(noteId)).Entity;
    }

    private static List<List<ChatResponseUpdate>> ScriptCalling(string tool, Dictionary<string, object?> arguments) =>
    [
        [new ChatResponseUpdate(ChatRole.Assistant, [new FunctionCallContent("1", tool, arguments)])],
        [
            new ChatResponseUpdate(ChatRole.Assistant, "I propose it."),
            new ChatResponseUpdate { Role = ChatRole.Assistant, FinishReason = ChatFinishReason.Stop }
        ]
    ];

    // The completed turn of a chat request, from its server-sent events.
    private static async Task<AiChatTurnDto> ChatAsync(HttpClient client, AiChatRequestDto request)
    {
        var response = await client.PostAsJsonAsync("api/aiassistant/chat", request);
        var body = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(response.IsSuccessStatusCode, body);
        var completed = body.Split('\n').Where(l => l.StartsWith("data:")).Select(l => JsonSerializer.Deserialize<AiChatStreamEventDto>(l[5..], _json)!)
            .Single(e => e.Type == AiChatStreamEventTypes.Completed);
        return completed.Turn;
    }

    private static async Task<List<AiActionDto>> RunAsync(HttpClient client, List<AiActionDto> actions)
    {
        var response = await client.PostAsJsonAsync("api/aiassistant/actions", actions);
        var body = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(response.IsSuccessStatusCode, body);
        return JsonSerializer.Deserialize<Result<List<AiActionDto>>>(body, _json)!.Entity;
    }

    [TestMethod]
    public async Task AProposal_ChangesNothing_AndRunsWhenAccepted_ThenTheModelIsTold()
    {
        var note = await FirstNoteAsync();
        _chatClients.Script = () => ScriptCalling(AiActionRules.SetTags, new()
        {
            ["noteNumber"] = note.NoteNumber, ["add"] = new[] { "proposed-tag" }
        });
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));

        var turn = await ChatAsync(staff, new AiChatRequestDto { Prompt = "Tag it" });

        var action = turn.Actions.Single();
        Assert.AreEqual(EnumAiActionStatus.Proposed, action.Status);
        Assert.IsFalse((await ReadAsync(note.NoteId)).Tags?.Contains("proposed-tag") == true, "Nothing done yet.");

        var ran = (await RunAsync(staff, new List<AiActionDto> { action })).Single();

        Assert.AreEqual(EnumAiActionStatus.Done, ran.Status, ran.Result);
        StringAssert.Contains((await ReadAsync(note.NoteId)).Tags, "proposed-tag");

        turn.Actions = new List<AiActionDto> { ran };
        _chatClients.Script = () => [[new ChatResponseUpdate(ChatRole.Assistant, "Done."), new ChatResponseUpdate { Role = ChatRole.Assistant, FinishReason = ChatFinishReason.Stop }]];
        await ChatAsync(staff, new AiChatRequestDto { Prompt = AiActionRules.ContinuePrompt, History = { turn } });

        // As it happened: the call to the tool and how it ended, not a text in what the model wrote, which it would imitate.
        var call = _chatClients.LastMessages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Single();
        var result = _chatClients.LastMessages.SelectMany(m => m.Contents).OfType<FunctionResultContent>().Single();
        Assert.AreEqual(AiActionRules.SetTags, call.Name);
        Assert.AreEqual(call.CallId, result.CallId);
        StringAssert.Contains(result.Result?.ToString(), "Done: Tags of");
        Assert.IsFalse(_chatClients.LastMessages.Last(m => m.Role == ChatRole.Assistant).Text.Contains("Done: Tags of"));
    }

    [TestMethod]
    public async Task AKindApproved_IsDoneInTheTurn()
    {
        var note = await FirstNoteAsync();
        _chatClients.Script = () => ScriptCalling(AiActionRules.SetTags, new()
        {
            ["noteNumber"] = note.NoteNumber, ["add"] = new[] { "approved-tag" }
        });
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));

        var turn = await ChatAsync(staff, new AiChatRequestDto { Prompt = "Tag it", AutoApprovedActionKinds = { AiActionRules.SetTags } });

        Assert.AreEqual(EnumAiActionStatus.Done, turn.Actions.Single().Status, turn.Actions.Single().Result);
        StringAssert.Contains((await ReadAsync(note.NoteId)).Tags, "approved-tag");
    }

    [TestMethod]
    public async Task AGuest_CannotRunActions()
    {
        var note = await FirstNoteAsync();
        using var guest = await NewClientAsync(nameof(EnumRoles.Guest));

        var response = await guest.PostAsJsonAsync("api/aiassistant/actions", new List<AiActionDto>
        {
            new() { Kind = AiActionRules.SetTags, NoteId = note.NoteId, AddTags = new() { "forged" }, Text = "Forged" }
        });

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ASystemNote_CannotBeChangedWithAForgedAction()
    {
        var glossary = JsonSerializer.Deserialize<Result<NoteDto>>(await (await _admin.PostAsync("api/atlas/glossary", null)).Content.ReadAsStringAsync(), _json)!.Entity;
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));

        var ran = (await RunAsync(staff, new List<AiActionDto>
        {
            new() { Kind = AiActionRules.AppendToNote, NoteId = glossary.NoteId, Content = "Forged", Text = "Forged" }
        })).Single();

        Assert.AreEqual(EnumAiActionStatus.Failed, ran.Status);
        Assert.IsFalse((await ReadAsync(glossary.NoteId)).Description.Contains("Forged"));
    }

    [TestMethod]
    public async Task CreatingAFolder_AStaffIsToldWhy_AndAForgedOneFails_AProjectManagerCan()
    {
        Guid homeId;
        using (var service = _factory.CreateServiceForRepository(0))
            homeId = (await service.Folders.GetHomeAsync()).Entity.FolderId;
        var name = $"Folder {Guid.NewGuid():N}";
        _chatClients.Script = () => ScriptCalling(AiActionRules.CreateFolder, new() { ["name"] = name });
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));
        using var manager = await NewClientAsync(nameof(EnumRoles.ProjectManager));
        AiActionDto Forged() => new() { Kind = AiActionRules.CreateFolder, FolderId = homeId, Topic = name, Text = "Forged" };

        var refused = await ChatAsync(staff, new AiChatRequestDto { Prompt = "Create it" });
        var forged = (await RunAsync(staff, new List<AiActionDto> { Forged() })).Single();
        var proposed = await ChatAsync(manager, new AiChatRequestDto { Prompt = "Create it" });

        Assert.AreEqual(0, refused.Actions.Count, "No card for what the role does not allow.");
        Assert.AreEqual(EnumAiActionStatus.Failed, forged.Status);
        Assert.AreEqual(EnumAiActionStatus.Proposed, proposed.Actions.Single().Status);

        var ran = (await RunAsync(manager, proposed.Actions)).Single();

        Assert.AreEqual(EnumAiActionStatus.Done, ran.Status, ran.Result);
        using (var service = _factory.CreateServiceForRepository(0))
            Assert.AreEqual(homeId, (await service.Folders.GetAllAsync()).Entity.Single(f => f.Name == name).ParentId);
    }

    [TestMethod]
    public async Task AnAlarm_ThroughTheApi_KeepsItsDate_AndCannotBeForTheUserItSays()
    {
        var note = await FirstNoteAsync();
        var comment = $"Alarm {Guid.NewGuid():N}";
        var when = DateTime.Now.AddDays(2).Date.AddHours(9);
        _chatClients.Script = () => ScriptCalling(AiActionRules.AddAlarm, new()
        {
            ["noteNumber"] = note.NoteNumber, ["message"] = comment, ["when"] = when.ToString("yyyy-MM-dd HH:mm")
        });
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));

        var turn = await ChatAsync(staff, new AiChatRequestDto { Prompt = "Remind me" });
        var other = Guid.NewGuid();
        turn.Actions.Single().UserId = other;
        var ran = (await RunAsync(staff, turn.Actions)).Single();

        Assert.AreEqual(EnumAiActionStatus.Done, ran.Status, ran.Result);
        using var service = _factory.CreateServiceForRepository(0);
        var alarm = (await service.Notes.GetMessagesAsync(note.NoteId)).Entity.Single(m => m.Comment == comment);
        Assert.AreEqual(when, alarm.AlarmDateTime);
        Assert.AreEqual(EnumNotificationType.AppInfo, alarm.NotificationType);
        Assert.AreNotEqual(other, alarm.UserId);
    }

    private static byte[] Png()
    {
        using var bitmap = new SkiaSharp.SKBitmap(64, 48);
        bitmap.Erase(SkiaSharp.SKColors.Blue);
        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static async Task<AiChatAttachmentDto> AttachAsync(HttpClient client, string fileName, string contentType, byte[] content)
    {
        var response = await client.PostAsJsonAsync("api/aiassistant/sessions/attachments", new AiChatAttachmentUploadDto
        {
            Provider = EnumAiProvider.OpenAI, Model = "fake-model", Topic = "Look", FileName = fileName, ContentType = contentType,
            ContentBase64 = Convert.ToBase64String(content)
        });
        var body = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(response.IsSuccessStatusCode, body);
        return JsonSerializer.Deserialize<Result<AiChatAttachmentDto>>(body, _json)!.Entity;
    }

    private static List<List<ChatResponseUpdate>> ScriptAnswer() =>
    [[
        new ChatResponseUpdate(ChatRole.Assistant, "A blue square."),
        new ChatResponseUpdate { Role = ChatRole.Assistant, FinishReason = ChatFinishReason.Stop }
    ]];

    [TestMethod]
    public async Task AnImage_ReachesTheModelOnlyInItsTurn_AndComesBackInTheTurn()
    {
        _chatClients.Script = ScriptAnswer;
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));
        var image = await AttachAsync(staff, "square.png", "image/png", Png());
        var pdf = await AttachAsync(staff, "manual.pdf", "application/pdf", new byte[] { 1, 2, 3 });

        var turn = await ChatAsync(staff, new AiChatRequestDto
        {
            Prompt = "What is it?", SessionNoteId = image.SessionNoteId, Attachments = { image, pdf }
        });

        var message = _chatClients.LastMessages.Last(m => m.Role == ChatRole.User);
        Assert.AreEqual(1, message.Contents.OfType<DataContent>().Count(), "The image, and not the PDF.");
        StringAssert.Contains(message.Text, "[Images attached, which you can see: square.png]");
        StringAssert.Contains(message.Text, "[Files attached, whose content you cannot see: manual.pdf]");
        Assert.AreEqual("What is it?", turn.Prompt);
        Assert.AreEqual(2, turn.Attachments.Count);

        _chatClients.Script = ScriptAnswer;
        await ChatAsync(staff, new AiChatRequestDto { Prompt = "And now?", History = { turn } });

        Assert.IsFalse(_chatClients.LastMessages.SelectMany(m => m.Contents).OfType<DataContent>().Any());
    }

    [TestMethod]
    public async Task ThePreviewOfAnImage_IsOnlyForTheUserOfTheSession()
    {
        using var owner = await NewClientAsync(nameof(EnumRoles.Staff));
        using var other = await NewClientAsync(nameof(EnumRoles.Staff));
        var image = await AttachAsync(owner, "square.png", "image/png", Png());
        var path = $"api/aiassistant/sessions/{image.SessionNoteId}/attachments/{image.ResourceId}/preview";

        var mine = await owner.GetAsync(path);
        var theirs = await other.GetAsync(path);

        Assert.IsTrue(mine.IsSuccessStatusCode, await mine.Content.ReadAsStringAsync());
        var preview = JsonSerializer.Deserialize<Result<AiChatAttachmentContentDto>>(await mine.Content.ReadAsStringAsync(), _json)!.Entity;
        StringAssert.StartsWith(preview.ContentType, "image/");
        Assert.AreEqual(HttpStatusCode.BadRequest, theirs.StatusCode);
    }

    [TestMethod]
    public async Task AnImageOfAnotherUser_DoesNotReachTheModel()
    {
        _chatClients.Script = ScriptAnswer;
        using var owner = await NewClientAsync(nameof(EnumRoles.Staff));
        using var other = await NewClientAsync(nameof(EnumRoles.Staff));
        var image = await AttachAsync(owner, "private.png", "image/png", Png());

        await ChatAsync(other, new AiChatRequestDto { Prompt = "Look", SessionNoteId = image.SessionNoteId, Attachments = { image } });

        var message = _chatClients.LastMessages.Last(m => m.Role == ChatRole.User);
        Assert.IsFalse(message.Contents.OfType<DataContent>().Any());
    }

    [TestMethod]
    public async Task AMessage_WithTooManyFiles_IsRefused()
    {
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));
        var request = new AiChatRequestDto { Prompt = "Look" };
        for (var i = 0; i <= AiAttachmentRules.MaxPerMessage; i++)
            request.Attachments.Add(new AiChatAttachmentDto { ResourceId = Guid.NewGuid(), FileName = "a.png", ContentType = "image/png" });

        var response = await staff.PostAsJsonAsync("api/aiassistant/chat", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Capabilities_SayIfTheModelSeesImages_AndAnUnknownProviderIsAnError()
    {
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));

        var known = await staff.GetAsync($"api/aiassistant/providers/{Uri.EscapeDataString("Fake provider")}/capabilities");
        var unknown = await staff.GetAsync("api/aiassistant/providers/Nobody/capabilities");

        var capabilities = JsonSerializer.Deserialize<Result<AiModelCapabilitiesDto>>(await known.Content.ReadAsStringAsync(), _json)!.Entity;
        Assert.IsTrue(capabilities.SupportsImages);
        Assert.AreEqual(HttpStatusCode.BadRequest, unknown.StatusCode);
    }

    [TestMethod]
    public async Task TheImagesOfANote_ReachTheModel_AfterTheResultOfTheTool()
    {
        var note = await FirstNoteAsync();
        using (var service = _factory.CreateServiceForRepository(0))
        {
            var saved = await service.Notes.SaveResourceAsync(new ResourceDto
            {
                NoteId = note.NoteId, Name = $"{Guid.NewGuid()}_board.png", Description = "board", FileType = "image/png", ContentArrayBytes = Png()
            });
            Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        }
        _chatClients.Script = () => ScriptCalling("view_note_images", new() { ["noteNumber"] = note.NoteNumber });
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));

        await ChatAsync(staff, new AiChatRequestDto { Prompt = "Describe its photos" });

        var last = _chatClients.LastMessages[^1];
        Assert.AreEqual(ChatRole.User, last.Role);
        Assert.IsTrue(last.Contents.OfType<DataContent>().Any(), "The image comes in the message after the result.");
        Assert.AreEqual(ChatRole.Tool, _chatClients.LastMessages[^2].Role);
    }

    [TestMethod]
    public async Task AFileOfTheConversation_IsAttachedToANote_OnceAccepted()
    {
        var note = await FirstNoteAsync();
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));
        var file = await AttachAsync(staff, "square.png", "image/png", Png());
        _chatClients.Script = () => ScriptCalling(AiActionRules.AttachFile, new() { ["noteNumber"] = note.NoteNumber });

        var turn = await ChatAsync(staff, new AiChatRequestDto { Prompt = "Keep it there", SessionNoteId = file.SessionNoteId, Attachments = { file } });

        var action = turn.Actions.Single();
        Assert.AreEqual(EnumAiActionStatus.Proposed, action.Status);

        var ran = (await RunAsync(staff, turn.Actions)).Single();

        Assert.AreEqual(EnumAiActionStatus.Done, ran.Status, ran.Result);
        using var service = _factory.CreateServiceForRepository(0);
        Assert.IsTrue((await service.Notes.GetResourcesInfoAsync(note.NoteId)).Entity.Any(r => r.NameOut == "square.png"));
    }

    [TestMethod]
    public async Task AFileOfAnotherUser_CannotBeAttached_WithAForgedAction()
    {
        var note = await FirstNoteAsync();
        using var owner = await NewClientAsync(nameof(EnumRoles.Staff));
        using var other = await NewClientAsync(nameof(EnumRoles.Staff));
        var file = await AttachAsync(owner, "private.png", "image/png", Png());
        var forged = new AiActionDto { Kind = AiActionRules.AttachFile, NoteId = note.NoteId, SessionNoteId = file.SessionNoteId, ResourceId = file.ResourceId, Text = "Forged" };

        var ran = (await RunAsync(other, new List<AiActionDto> { forged })).Single();

        Assert.AreEqual(EnumAiActionStatus.Failed, ran.Status);
    }

    [TestMethod]
    public async Task AskingToOpenANote_TellsTheWebClient()
    {
        var note = await FirstNoteAsync();
        _chatClients.Script = () => ScriptCalling(AiActionRules.OpenNoteEditor, new() { ["noteNumber"] = note.NoteNumber });
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));

        var response = await staff.PostAsJsonAsync("api/aiassistant/chat", new AiChatRequestDto { Prompt = "Open it" });
        var events = (await response.Content.ReadAsStringAsync()).Split('\n').Where(l => l.StartsWith("data:"))
            .Select(l => JsonSerializer.Deserialize<AiChatStreamEventDto>(l[5..], _json)!).ToList();

        var open = events.Single(e => e.Type == AiChatStreamEventTypes.OpenNote);
        Assert.AreEqual(note.NoteId, open.NoteId);
        Assert.AreEqual(note.NoteNumber, open.NoteNumber);
    }

    [TestMethod]
    public async Task ReplacingTheText_ThroughTheApi_AsksFirst_AndRunsOnceAccepted()
    {
        var note = await FirstNoteAsync();
        var content = $"Text {Guid.NewGuid():N}";
        _chatClients.Script = () => ScriptCalling(AiActionRules.ReplaceNoteText, new() { ["noteNumber"] = note.NoteNumber, ["content"] = content });
        using var staff = await NewClientAsync(nameof(EnumRoles.Staff));

        var turn = await ChatAsync(staff, new AiChatRequestDto { Prompt = "Replace it" });

        Assert.AreEqual(EnumAiActionStatus.Proposed, turn.Actions.Single().Status);
        Assert.AreNotEqual(content, (await ReadAsync(note.NoteId)).Description);

        var ran = (await RunAsync(staff, turn.Actions)).Single();

        Assert.AreEqual(EnumAiActionStatus.Done, ran.Status, ran.Result);
        Assert.AreEqual(content, (await ReadAsync(note.NoteId)).Description);
    }

    private static async Task<HttpClient> NewClientAsync(string role)
    {
        var id = Guid.NewGuid();
        var userName = $"act-{id:N}"[..24];
        using (var service = _factory.CreateServiceForRepository(0))
        {
            var res = await service.Users.CreateAsync(new UserRegisterDto
            {
                UserName = userName,
                EMail = $"{id:N}@knote.tests",
                FullName = $"Actions user {id:N}"[..24],
                RoleDefinition = role,
                Password = Password
            });
            Assert.IsTrue(res.IsValid, res.ErrorMessage);
        }

        var client = _factory.CreateClient();
        var token = await (await client.PostAsJsonAsync("api/users/login", new UserCredentialsDto { UserName = userName, Password = Password }))
            .Content.ReadFromJsonAsync<UserTokenDto>();
        Assert.IsTrue(token!.success, token.error);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.token);
        return client;
    }
}
