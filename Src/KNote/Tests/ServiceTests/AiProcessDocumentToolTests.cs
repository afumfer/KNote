using KNote.Ai;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// process_document (KNote Atlas, stage 2, 2.1b2b, D22) against a real repository, called as the model calls it, with a host that
/// records what it is asked to start: always a card with the estimate, never approved for the conversation, only documents of
/// notes, a translation needs its language and goes on with one not finished, and accepted, the host starts it.
/// </summary>
[TestClass]
public class AiProcessDocumentToolTests
{
    private const char Page = DocumentTextRules.PageSeparator;
    private static readonly AiProviderRef Model = new() { Alias = "Local", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b", Host = "http://localhost:11434", Kind = EnumAiProviderKind.Chat };

    private sealed class Host : IKNoteAiToolsHost
    {
        public bool CanProcess { get; set; } = true;
        public AiProviderRef Provider { get; set; } = Model;
        public string? StartError { get; set; }
        public List<(AiProviderRef Provider, DocumentProcessRequestDto Request)> Started { get; } = new();

        public Task<AiNoteDestination> GetNewNoteDestinationAsync() => Task.FromResult<AiNoteDestination>(null!);
        public void OnNoteCreated(IKntService service, NoteExtendedDto note, bool openEditor = true) { }
        public bool CanProcessDocuments => CanProcess;
        public Task<AiProviderRef> GetDocumentModelAsync(IKntService service) => Task.FromResult(Provider);

        public Task<string> StartDocumentProcessAsync(IKntService service, AiProviderRef provider, DocumentProcessRequestDto request)
        {
            if (StartError == null)
                Started.Add((provider, request));
            return Task.FromResult(StartError);
        }
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
    }

    private static async Task<Setup> SetupAsync()
    {
        var fixture = await SemanticIndexFixture.CreateAsync();
        fixture.Service.UserIdentityName = (await fixture.Service.Users.GetAsync(fixture.UserId)).Entity.UserName;
        var host = new Host();
        var tools = new KNoteAiTools(fixture.Service, host);
        tools.BeginTurn(null, null);
        return new Setup(fixture, tools, host);
    }

    // A note with a PDF of three pages already read.
    private static async Task<(NoteDto Note, Guid ResourceId)> AddDocumentAsync(Setup setup, string fileName = "manual.pdf")
    {
        var note = await setup.Fixture.AddNoteAsync("QL manual");
        var resource = await setup.Service.Repository.Notes.AddResourceAsync(new ResourceDto
        {
            ResourceId = Guid.NewGuid(), NoteId = note.NoteId, Name = $"{Guid.NewGuid()}_{fileName}", Description = fileName, ContentInDB = true,
            ContentArrayBytes = [1], FileType = "application/pdf", Container = "KntResources"
        });
        Assert.IsTrue(resource.IsValid, resource.ErrorMessage);
        var text = $"The QL boots.{Page}It has two microdrives.{Page}The end.";
        Assert.IsTrue((await setup.Service.Repository.Notes.SaveResourceTextAsync(resource.Entity.ResourceId, text,
            DocumentTextRules.StateOf(DocumentTextRules.Pages(text)), resource.Entity.Name)).Entity);
        return (note, resource.Entity.ResourceId);
    }

    [TestMethod]
    public async Task ASummary_IsProposedWithItsEstimate_AndAcceptedTheHostStartsIt()
    {
        using var setup = await SetupAsync();
        var (note, resourceId) = await AddDocumentAsync(setup);

        var result = await setup.CallAsync(AiActionRules.ProcessDocument, ("noteNumber", note.NoteNumber), ("task", "summary"),
            ("language", "Spanish"), ("length", "long"));

        StringAssert.Contains(result, "NOT done");
        var action = setup.Tools.TurnActions.Single();
        Assert.AreEqual(EnumAiActionStatus.Proposed, action.Status);
        Assert.AreEqual($"Write an extended summary of manual.pdf (#{note.NoteNumber}) in Spanish, long, in a new note", action.Text);
        var card = AiActionRules.CardText(action);
        StringAssert.Contains(card, "3 pages", "the estimate");
        StringAssert.Contains(card, AiDocumentProcessRules.RunsInBackground);
        Assert.AreEqual(card, AiActionRules.PlainCardText(action), "the question of ClientWin's Markdown view says the same");
        Assert.IsEmpty(setup.Host.Started, "nothing starts before the card is accepted");

        await setup.Tools.ExecuteActionsAsync([action]);

        Assert.AreEqual(EnumAiActionStatus.Done, action.Status, action.Result);
        var (provider, request) = setup.Host.Started.Single();
        Assert.AreSame(Model, provider);
        Assert.AreEqual(resourceId, request.ResourceId);
        Assert.AreEqual(EnumDocumentProcessKind.Summarize, request.Kind);
        Assert.AreEqual("Spanish", request.Language);
        Assert.AreEqual(EnumSummaryLength.Long, request.Length);
        StringAssert.StartsWith(AiActionRules.Outcome(action), AiDocumentProcessRules.Started, "the model is told it only started");
        StringAssert.StartsWith(AiActionRules.Line(action), "▶ Started:");
    }

    [TestMethod]
    public async Task ItAlwaysAsks_EvenWithItsKindApproved_AndTheCardHasNoDontAskAgain()
    {
        using var setup = await SetupAsync();
        var (note, _) = await AddDocumentAsync(setup);
        setup.Tools.BeginTurn(null, new[] { AiActionRules.ProcessDocument });

        await setup.CallAsync(AiActionRules.ProcessDocument, ("noteNumber", note.NoteNumber), ("task", "summary"));

        var action = setup.Tools.TurnActions.Single();
        Assert.AreEqual(EnumAiActionStatus.Proposed, action.Status);
        Assert.IsEmpty(setup.Host.Started);
        Assert.IsFalse(AiActionRules.CanDontAskAgain([action]));
        Assert.IsEmpty(AiActionRules.ApprovedKinds([action]));
    }

    [TestMethod]
    public async Task ATranslation_NeedsItsLanguage_AndGoesOnWithOneNotFinished()
    {
        using var setup = await SetupAsync();
        var (note, resourceId) = await AddDocumentAsync(setup);

        var noLanguage = await setup.CallAsync(AiActionRules.ProcessDocument, ("noteNumber", note.NoteNumber), ("task", "translation"));
        Assert.AreEqual(AiDocumentProcessRules.LanguageRequired, noLanguage);

        // A translation into Spanish stopped after its first page.
        var saved = await setup.Service.Summaries.SaveDocumentTranslationAsync(new DocumentTranslationSaveDto
        {
            ResourceId = resourceId, ExpectedResourceName = (await setup.Service.Notes.GetResourceAsync(resourceId)).Entity.Name,
            Model = "qwen3.5:9b", Language = "Spanish", Paged = true, PagesCount = 3, Parts = 3, FromPart = 0, NextPart = 1, NextPage = 2,
            Text = "#### p. 1\n\nEl QL arranca."
        });
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);

        await setup.CallAsync(AiActionRules.ProcessDocument, ("noteNumber", note.NoteNumber), ("task", "translation"), ("language", "spanish"));

        var action = setup.Tools.TurnActions.Single();
        Assert.AreEqual(saved.Entity.NoteId, action.ContinueNoteId);
        StringAssert.StartsWith(action.Text, "Continue the translation of manual.pdf");
        StringAssert.Contains(AiActionRules.CardText(action), "It goes on from the page 2.");

        await setup.Tools.ExecuteActionsAsync([action]);
        Assert.AreEqual(saved.Entity.NoteId, setup.Host.Started.Single().Request.ContinueNoteId);
    }

    [TestMethod]
    public async Task OnlyDocumentsOfNotes_AFileOfTheConversationIsAttachedFirst()
    {
        using var setup = await SetupAsync();
        var note = await setup.Fixture.AddNoteAsync("Without documents");
        setup.Tools.BeginTurn(null, null, Guid.NewGuid(),
            [new AiChatAttachmentDto { ResourceId = Guid.NewGuid(), FileName = "report.pdf", ContentType = "application/pdf" }]);

        var result = await setup.CallAsync(AiActionRules.ProcessDocument, ("noteNumber", note.NoteNumber), ("task", "summary"));

        StringAssert.Contains(result, "report.pdf " + AiDocumentProcessRules.NotFromAttachment);
        Assert.IsEmpty(setup.Tools.TurnActions);
    }

    [TestMethod]
    public async Task WhatTheHostCannotStart_Fails_AndWithoutAModelNothingIsProposed()
    {
        using var setup = await SetupAsync();
        var (note, _) = await AddDocumentAsync(setup);
        await setup.CallAsync(AiActionRules.ProcessDocument, ("noteNumber", note.NoteNumber), ("task", "summary"));
        var action = setup.Tools.TurnActions.Single();
        setup.Host.StartError = SummaryTexts.OtherRunning;

        await setup.Tools.ExecuteActionsAsync([action]);

        Assert.AreEqual(EnumAiActionStatus.Failed, action.Status);
        Assert.AreEqual(SummaryTexts.OtherRunning, action.Result);

        setup.Tools.BeginTurn(null, null);
        setup.Host.Provider = null!;
        Assert.AreEqual(AiDocumentProcessRules.NoModel,
            await setup.CallAsync(AiActionRules.ProcessDocument, ("noteNumber", note.NoteNumber), ("task", "summary")));
    }

    [TestMethod]
    public async Task ItIsOnlyOffered_WhenTheApplicationCanRunIt()
    {
        using var setup = await SetupAsync();

        var offered = (await setup.Tools.GetToolsAsync()).Select(t => t.Name).ToList();
        setup.Host.CanProcess = false;
        var notOffered = (await setup.Tools.GetToolsAsync()).Select(t => t.Name).ToList();

        CollectionAssert.Contains(offered, AiActionRules.ProcessDocument);
        CollectionAssert.DoesNotContain(notOffered, AiActionRules.ProcessDocument);
    }

    [TestMethod]
    public void TheTaskAndTheLength_AreReadAsTheModelWritesThem()
    {
        Assert.AreEqual(EnumDocumentProcessKind.Summarize, AiDocumentProcessRules.Task(" Summary ").Kind);
        Assert.AreEqual(EnumDocumentProcessKind.Translate, AiDocumentProcessRules.Task("translate").Kind);
        Assert.IsNotNull(AiDocumentProcessRules.Task("rewrite").Error);
        Assert.AreEqual(EnumSummaryLength.Medium, AiDocumentProcessRules.Length(null).Length);
        Assert.AreEqual(EnumSummaryLength.Short, AiDocumentProcessRules.Length("short").Length);
        Assert.IsNotNull(AiDocumentProcessRules.Length("huge").Error);
        Assert.AreEqual("Translate manual.pdf (#7) into French, in a new note",
            AiDocumentProcessRules.Text(EnumDocumentProcessKind.Translate, "manual.pdf", 7, "French", EnumSummaryLength.Medium));
    }
}
