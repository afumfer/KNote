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
/// api/summaries (KNote Atlas, stage 2, 2.1a): the summary of a long document of a note, by someone who edits notes and not by a
/// guest; its estimate; a run of the Server's queue of the user who asks, with its progress until the new note; an unknown model.
/// The model is a ScriptedAiChatClientProvider.
/// </summary>
[TestClass]
public class SummaryInProcessTests
{
    private const string Password = "InProcess-Summary-Password-1!";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _admin = null!;
    private static readonly ScriptedAiChatClientProvider _chatClients = new();
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _admin) = await InProcessTestHost.CreateAuthenticatedClientAsync(factory =>
        {
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

    [TestMethod]
    public async Task ADocument_IsSummarizedInANewNote_BySomeoneWhoEditsNotes()
    {
        var (noteId, documentId) = await AddDocumentAsync();
        _chatClients.Script = () => Enumerable.Range(0, 20).Select(_ => new List<ChatResponseUpdate> { new(ChatRole.Assistant, "The manual of the Sinclair QL.") }).ToList();
        var (staff, staffName) = await NewClientAsync(nameof(EnumRoles.Staff));
        using var _ = staff;
        var (guest, _) = await NewClientAsync(nameof(EnumRoles.Guest));
        using var __ = guest;
        var request = new DocumentProcessRequestDto { ResourceId = documentId, Language = "English", Length = EnumSummaryLength.Short };

        var forbidden = await guest.PostAsJsonAsync("api/summaries/documents", request);
        var estimate = await staff.PostAsJsonAsync("api/summaries/documents/estimate", request);
        var started = await staff.PostAsJsonAsync("api/summaries/documents", request);

        Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
        var estimateBody = await estimate.Content.ReadAsStringAsync();
        Assert.IsTrue(estimate.IsSuccessStatusCode, estimateBody);
        var estimated = JsonSerializer.Deserialize<Result<DocumentProcessEstimateDto>>(estimateBody, _json)!.Entity;
        Assert.AreEqual(1, estimated.Pages);
        Assert.AreEqual(2, estimated.Calls, "a batch and the final call");
        Assert.IsTrue(estimated.IsPaid);
        Assert.AreEqual(32_000, estimated.ContextTokens);
        Assert.IsTrue(started.IsSuccessStatusCode, await started.Content.ReadAsStringAsync());

        DocumentProcessProgressDto? progress = null;
        for (var i = 0; i < 200 && progress?.IsFinished != true; i++)
        {
            await Task.Delay(50);
            progress = (await staff.GetFromJsonAsync<Result<DocumentProcessProgressDto>>("api/summaries/documents/progress", _json))!.Entity;
        }
        Assert.AreEqual(EnumDocumentProcessPhase.Completed, progress!.Phase, progress.Message);
        Assert.AreEqual(staffName, progress.RequestedBy);
        Assert.AreEqual(0, _chatClients.LastTools.Count, "No tools.");
        Assert.IsNull((await _admin.GetFromJsonAsync<Result<DocumentProcessProgressDto>>("api/summaries/documents/progress", _json))!.Entity,
            "each user sees their own");

        using var service = _factory.CreateServiceForRepository(0);
        var summary = (await service.Notes.GetExtendedAsync(progress.NoteId!.Value)).Entity;
        Assert.AreEqual(KntConst.SummaryTag, summary.NoteTypeDto.Name);
        Assert.AreEqual(noteId, summary.TraceNotesTo.Single().ToId);
        StringAssert.EndsWith(summary.Description, "The manual of the Sinclair QL.");
    }

    [TestMethod]
    public async Task AModelThisServerDoesntHave_IsSaid()
    {
        var (_, documentId) = await AddDocumentAsync();
        var (staff, _) = await NewClientAsync(nameof(EnumRoles.Staff));
        using var _ = staff;

        var response = await staff.PostAsJsonAsync("api/summaries/documents",
            new DocumentProcessRequestDto { ResourceId = documentId, ProviderAlias = "Nobody's" });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "is not configured in this Server");
    }

    private static async Task<(Guid NoteId, Guid DocumentId)> AddDocumentAsync()
    {
        using var service = _factory.CreateServiceForRepository(0);
        var note = (await service.Notes.HomeNotesAsync()).Entity.First();
        var saved = await service.Notes.SaveResourceAsync(new ResourceDto
        {
            NoteId = note.NoteId, Name = $"{Guid.NewGuid()}_manual.pdf", Description = "manual.pdf", FileType = "application/pdf",
            ContentArrayBytes = TestDocuments.Pdf("The manual of the QL. It explains how to start it.")
        });
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return (note.NoteId, saved.Entity.ResourceId);
    }

    private static async Task<(HttpClient Client, string UserName)> NewClientAsync(string role)
    {
        var id = Guid.NewGuid();
        var userName = $"sum-{id:N}"[..24];
        using (var service = _factory.CreateServiceForRepository(0))
        {
            var res = await service.Users.CreateAsync(new UserRegisterDto
            {
                UserName = userName,
                EMail = $"{id:N}@knote.tests",
                FullName = $"Summary user {id:N}"[..24],
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
        return (client, userName);
    }
}
