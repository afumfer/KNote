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

    // 2.2 (D3): the noise policy of the session's repository: anyone reads it and its recount; only ProjectManager and Admin keep it.
    [TestMethod]
    public async Task TheNoisePolicy_IsReadByAnyone_AndKeptByWhoIsResponsible()
    {
        var (staff, _) = await NewClientAsync(nameof(EnumRoles.Staff));
        using var _ = staff;
        using var service = _factory.CreateServiceForRepository(0);
        var type = await service.NoteTypes.SaveAsync(new NoteTypeDto { Name = $"Noise {Guid.NewGuid():N}"[..20] });
        Assert.IsTrue(type.IsValid, type.ErrorMessage);
        var policy = new NoisePolicyDto { ExcludedNoteTypeIds = { type.Entity.NoteTypeId } };

        var refused = await staff.PostAsJsonAsync("api/summaries/noise-policy", policy);
        var saved = await _admin.PostAsJsonAsync("api/summaries/noise-policy", policy);
        var read = await staff.GetFromJsonAsync<Result<NoisePolicyDto>>("api/summaries/noise-policy", _json);
        var count = await staff.PostAsJsonAsync("api/summaries/noise-policy/count", policy);

        Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode);
        Assert.IsTrue(saved.IsSuccessStatusCode, await saved.Content.ReadAsStringAsync());
        CollectionAssert.Contains(read!.Entity.ExcludedNoteTypeIds, type.Entity.NoteTypeId);
        var countBody = await count.Content.ReadAsStringAsync();
        Assert.IsTrue(count.IsSuccessStatusCode, countBody);
        var counted = JsonSerializer.Deserialize<Result<NoiseCountDto>>(countBody, _json)!.Entity;
        Assert.IsGreaterThan(0, counted.Notes);
    }

    // 2.3 (CU-15): the selection of a summary and the corrections of «Review selection», for a Staff; not for a Guest.
    [TestMethod]
    public async Task TheSelection_AndItsCorrections_AreForAStaff()
    {
        var (staff, _) = await NewClientAsync(nameof(EnumRoles.Staff));
        using var _ = staff;
        var (guest, _) = await NewClientAsync(nameof(EnumRoles.Guest));
        using var __ = guest;
        using var service = _factory.CreateServiceForRepository(0);
        var note = (await service.Notes.NewAsync()).Entity;
        note.Topic = $"Selection {Guid.NewGuid():N}";
        note.Description = string.Join(" ", Enumerable.Range(1, 60).Select(i => $"word{i}"));
        note.FolderId = (await service.Folders.GetHomeAsync()).Entity.FolderId;
        var saved = await service.Notes.SaveAsync(note);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        note = saved.Entity;
        var request = new NoteSelectionRequestDto { From = DateTime.Today, To = DateTime.Today };

        var refused = await guest.PostAsJsonAsync("api/summaries/selection", request);
        var answer = await staff.PostAsJsonAsync("api/summaries/selection", request);
        var corrected = await staff.PostAsJsonAsync("api/summaries/relevance/corrections", new RelevanceCorrectionsSaveDto
        {
            Changes = { new RelevanceCorrectionChangeDto { NoteId = note.NoteId, Decision = EnumSelectionDecision.Out } }
        });
        var again = await staff.PostAsJsonAsync("api/summaries/selection", request);

        Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode);
        var body = await answer.Content.ReadAsStringAsync();
        Assert.IsTrue(answer.IsSuccessStatusCode, body);
        Assert.AreEqual(EnumSelectionDecision.In, JsonSerializer.Deserialize<Result<NoteSelectionResultDto>>(body, _json)!.Entity.Notes.Single(n => n.NoteId == note.NoteId).Decision);
        Assert.IsTrue(corrected.IsSuccessStatusCode, await corrected.Content.ReadAsStringAsync());
        var corrections = JsonSerializer.Deserialize<Result<NoteSelectionResultDto>>(await again.Content.ReadAsStringAsync(), _json)!.Entity;
        Assert.AreEqual(EnumSelectionSource.Correction, corrections.Notes.Single(n => n.NoteId == note.NoteId).Source);
    }

    // 2.5 (CU-16, CU-17): a period summary in the queue of the Server, as the summary of a document: estimated, made and kept in
    // a new note of «KNote Summary».
    [TestMethod]
    public async Task APeriodSummary_IsMadeInTheQueue_AndKeptInANewNote()
    {
        var (staff, _) = await NewClientAsync(nameof(EnumRoles.Staff));
        using var _ = staff;
        using var service = _factory.CreateServiceForRepository(0);
        var note = (await service.Notes.NewAsync()).Entity;
        note.Topic = $"Period {Guid.NewGuid():N}";
        note.Description = string.Join(" ", Enumerable.Range(1, 60).Select(i => $"word{i}"));
        note.FolderId = (await service.Folders.GetHomeAsync()).Entity.FolderId;
        Assert.IsTrue((await service.Notes.SaveAsync(note)).IsValid);
        _chatClients.Script = () => Enumerable.Range(0, 40).Select(_ => new List<ChatResponseUpdate> { new(ChatRole.Assistant, "The week in short.") }).ToList();
        var request = new DocumentProcessRequestDto
        {
            Kind = EnumDocumentProcessKind.PeriodSummary, Language = "English",
            Period = new PeriodSummaryRequestDto { Subject = "Week in short", Words = 200, Selection = new NoteSelectionRequestDto { From = DateTime.Today, To = DateTime.Today } }
        };

        var estimate = await staff.PostAsJsonAsync("api/summaries/documents/estimate", request);
        var started = await staff.PostAsJsonAsync("api/summaries/documents", request);

        var estimateBody = await estimate.Content.ReadAsStringAsync();
        Assert.IsTrue(estimate.IsSuccessStatusCode, estimateBody);
        Assert.IsGreaterThan(0, JsonSerializer.Deserialize<Result<DocumentProcessEstimateDto>>(estimateBody, _json)!.Entity.Notes);
        Assert.IsTrue(started.IsSuccessStatusCode, await started.Content.ReadAsStringAsync());
        DocumentProcessProgressDto? progress = null;
        for (var i = 0; i < 200 && progress?.IsFinished != true; i++)
        {
            await Task.Delay(50);
            progress = (await staff.GetFromJsonAsync<Result<DocumentProcessProgressDto>>("api/summaries/documents/progress", _json))!.Entity;
        }
        Assert.AreEqual(EnumDocumentProcessPhase.Completed, progress!.Phase, progress.Message);
        Assert.AreEqual(EnumDocumentProcessKind.PeriodSummary, progress.Kind);
        Assert.AreEqual("Week in short", progress.FileName);

        var summary = (await service.Notes.GetExtendedAsync(progress.NoteId!.Value)).Entity;
        Assert.AreEqual(KntConst.SummaryTag, summary.NoteTypeDto.Name);
        Assert.AreEqual("Week in short", summary.Topic);
        StringAssert.Contains(summary.Description, "## Executive summary\n\nThe week in short.");
        StringAssert.Contains(summary.Description, "## What happened\n\n### Home\n\nThe week in short.");
        CollectionAssert.Contains(summary.TraceNotesTo.Select(r => r.ToId).ToList(), note.NoteId, "related with the note it comes from (D17)");
    }

    // 2.4 (D9, C2): a batch of doubtful notes asked to the AI, by a Staff, and its answers used by the selection.
    [TestMethod]
    public async Task TheDoubtfulNotes_AreAskedToTheAi_ByAStaff()
    {
        var (staff, _) = await NewClientAsync(nameof(EnumRoles.Staff));
        using var _ = staff;
        var (guest, _) = await NewClientAsync(nameof(EnumRoles.Guest));
        using var __ = guest;
        using var service = _factory.CreateServiceForRepository(0);
        var note = (await service.Notes.NewAsync()).Entity;
        note.Topic = $"Doubtful {Guid.NewGuid():N}";
        note.Description = string.Join(" ", Enumerable.Range(1, 15).Select(i => $"word{i}"));
        note.FolderId = (await service.Folders.GetHomeAsync()).Entity.FolderId;
        var saved = await service.Notes.SaveAsync(note);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        note = saved.Entity;
        _chatClients.Script = () => [[new(ChatRole.Assistant, $"#{note.NoteNumber} | Out | a list of words")]];
        var request = new NoteClassificationRequestDto { NoteIds = { note.NoteId } };

        var refused = await guest.PostAsJsonAsync("api/summaries/selection/classify", request);
        var answer = await staff.PostAsJsonAsync("api/summaries/selection/classify", request);
        var selection = await staff.PostAsJsonAsync("api/summaries/selection", new NoteSelectionRequestDto { From = DateTime.Today, To = DateTime.Today });

        Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode);
        var body = await answer.Content.ReadAsStringAsync();
        Assert.IsTrue(answer.IsSuccessStatusCode, body);
        Assert.AreEqual(EnumSelectionDecision.Out, JsonSerializer.Deserialize<Result<List<RelevanceAiAnswerDto>>>(body, _json)!.Entity.Single().Decision);
        var decided = JsonSerializer.Deserialize<Result<NoteSelectionResultDto>>(await selection.Content.ReadAsStringAsync(), _json)!.Entity
            .Notes.Single(n => n.NoteId == note.NoteId);
        Assert.AreEqual((EnumSelectionDecision.Out, EnumSelectionSource.Ai), (decided.Decision, decided.Source));
        StringAssert.Contains(decided.Reason, "fake-model");
    }

    // 2.1b2b: process_document of the assistant, accepted in its card, queues the same run for the user of the conversation.
    [TestMethod]
    public async Task TheAssistant_StartsTheSummaryOfADocument_WhenItsCardIsAccepted()
    {
        var (_, documentId) = await AddDocumentAsync();
        _chatClients.Script = () => Enumerable.Range(0, 20).Select(_ => new List<ChatResponseUpdate> { new(ChatRole.Assistant, "The manual of the Sinclair QL.") }).ToList();
        var (staff, staffName) = await NewClientAsync(nameof(EnumRoles.Staff));
        using var _ = staff;
        var action = new AiActionDto
        {
            Kind = AiActionRules.ProcessDocument, Text = "Write an extended summary of manual.pdf", ResourceId = documentId,
            ProcessKind = EnumDocumentProcessKind.Summarize, Length = EnumSummaryLength.Short
        };

        var answer = await staff.PostAsJsonAsync("api/aiassistant/actions", new List<AiActionDto> { action });

        var body = await answer.Content.ReadAsStringAsync();
        Assert.IsTrue(answer.IsSuccessStatusCode, body);
        var ran = JsonSerializer.Deserialize<Result<List<AiActionDto>>>(body, _json)!.Entity.Single();
        Assert.AreEqual(EnumAiActionStatus.Done, ran.Status, ran.Result);
        DocumentProcessProgressDto? progress = null;
        for (var i = 0; i < 200 && progress?.IsFinished != true; i++)
        {
            await Task.Delay(50);
            progress = (await staff.GetFromJsonAsync<Result<DocumentProcessProgressDto>>("api/summaries/documents/progress", _json))!.Entity;
        }
        Assert.AreEqual(EnumDocumentProcessPhase.Completed, progress!.Phase, progress.Message);
        Assert.AreEqual(staffName, progress.RequestedBy);
        Assert.AreEqual(documentId, progress.ResourceId);
    }

    // 2.1b: the translation, a run of the same queue, and the translations of the document («Continue»).
    [TestMethod]
    public async Task ADocument_IsTranslatedInANewNote_AndItsTranslationsAreListed()
    {
        var (noteId, documentId) = await AddDocumentAsync();
        _chatClients.Script = () => Enumerable.Range(0, 20).Select(_ => new List<ChatResponseUpdate> { new(ChatRole.Assistant, "El manual del QL. Explica cómo arrancarlo.") }).ToList();
        var (staff, _) = await NewClientAsync(nameof(EnumRoles.Staff));
        using var _ = staff;
        var (guest, _) = await NewClientAsync(nameof(EnumRoles.Guest));
        using var __ = guest;
        var request = new DocumentProcessRequestDto { ResourceId = documentId, Kind = EnumDocumentProcessKind.Translate, Language = "Spanish" };

        var noLanguage = await staff.PostAsJsonAsync("api/summaries/documents/estimate", new DocumentProcessRequestDto { ResourceId = documentId, Kind = EnumDocumentProcessKind.Translate });
        var started = await staff.PostAsJsonAsync("api/summaries/documents", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, noLanguage.StatusCode);
        Assert.IsTrue(started.IsSuccessStatusCode, await started.Content.ReadAsStringAsync());
        DocumentProcessProgressDto? progress = null;
        for (var i = 0; i < 200 && progress?.IsFinished != true; i++)
        {
            await Task.Delay(50);
            progress = (await staff.GetFromJsonAsync<Result<DocumentProcessProgressDto>>("api/summaries/documents/progress", _json))!.Entity;
        }
        Assert.AreEqual(EnumDocumentProcessPhase.Completed, progress!.Phase, progress.Message);
        Assert.AreEqual(EnumDocumentProcessKind.Translate, progress.Kind);

        var forbidden = await guest.GetAsync($"api/summaries/documents/{documentId}/translations");
        var translations = (await staff.GetFromJsonAsync<Result<List<DocumentTranslationDto>>>($"api/summaries/documents/{documentId}/translations", _json))!.Entity;

        Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
        var translation = translations.Single();
        Assert.AreEqual(progress.NoteId, translation.NoteId);
        Assert.IsTrue(translation.Finished);
        Assert.AreEqual("Spanish", translation.Language);
        using var service = _factory.CreateServiceForRepository(0);
        var note = (await service.Notes.GetExtendedAsync(translation.NoteId)).Entity;
        Assert.AreEqual(KntConst.TranslationTag, note.NoteTypeDto.Name);
        Assert.AreEqual(noteId, note.TraceNotesTo.Single().ToId);
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
