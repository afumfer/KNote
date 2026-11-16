using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// process_document with a real local model (KNote Atlas 2.1b2b): asked for an extended summary or a translation of a whole long
/// document of a note, does the model propose it, or does it read the document and write a summary itself? Found by the owner: both
/// qwen3.5:9b and an OpenAI model read it and answered in the chat, with no card. The way of production: the system prompt of the
/// assistant (AiAssistantContext), the tools of the turn and AiChatClientFactory. It prints, for each request, what the model did.
/// Only with KNOTE_RESOURCE_SMOKE_MODEL (an Ollama model with tools; host: OLLAMA_HOST or localhost).
/// </summary>
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class AiProcessDocumentSmokeTests
{
    private sealed class Host(AiProviderRef provider) : IKNoteAiToolsHost
    {
        public Task<AiNoteDestination> GetNewNoteDestinationAsync() => Task.FromResult<AiNoteDestination>(null!);
        public void OnNoteCreated(IKntService service, NoteExtendedDto note, bool openEditor = true) { }
        public bool CanProcessDocuments => true;
        public Task<AiProviderRef> GetDocumentModelAsync(IKntService service) => Task.FromResult(provider);
    }

    [TestMethod]
    public async Task AskedForAnExtendedSummaryOrATranslation_TheModelProposesIt()
    {
        var model = Environment.GetEnvironmentVariable("KNOTE_RESOURCE_SMOKE_MODEL");
        if (string.IsNullOrWhiteSpace(model))
        {
            Assert.Inconclusive("KNOTE_RESOURCE_SMOKE_MODEL is not set - skipping.");
            return;
        }
        var provider = new AiProviderRef
        {
            Alias = "Smoke", Provider = EnumAiProvider.Ollama, Model = model, Kind = EnumAiProviderKind.Chat,
            Host = Environment.GetEnvironmentVariable("OLLAMA_HOST") is { Length: > 0 } host ? host : "http://localhost:11434"
        };

        using var fixture = await SemanticIndexFixture.CreateAsync("EntityFramework");
        fixture.Service.UserIdentityName = (await fixture.Service.Users.GetAsync(fixture.UserId)).Entity.UserName;
        var note = await fixture.AddNoteAsync("Sinclair QL", "The user guide of the QL is attached.");
        var resource = await fixture.Service.Repository.Notes.AddResourceAsync(new ResourceDto
        {
            ResourceId = Guid.NewGuid(), NoteId = note.NoteId, Name = $"{Guid.NewGuid()}_manual.pdf", Description = "manual.pdf", ContentInDB = true,
            ContentArrayBytes = [1], FileType = "application/pdf", Container = "KntResources"
        });
        // 120 pages, too long to read here at once.
        var text = string.Join(DocumentTextRules.PageSeparator, Enumerable.Range(1, 120).Select(p =>
            $"QL User Guide, page {p}. " + string.Join(" ", Enumerable.Repeat($"The Sinclair QL has two microdrives and SuperBASIC; this is section {p}.", 25))));
        Assert.IsTrue((await fixture.Service.Repository.Notes.SaveResourceTextAsync(resource.Entity.ResourceId, text,
            DocumentTextRules.StateOf(DocumentTextRules.Pages(text)), resource.Entity.Name)).Entity);

        var requests = new[]
        {
            $"Haz un resumen extendido del manual de la nota #{note.NoteNumber}, en español.",
            $"Traduce al inglés el documento de la nota #{note.NoteNumber}.",
            $"Resume en una nota nueva el manual de la nota #{note.NoteNumber}.",
            $"Make an extended summary of the PDF of the note #{note.NoteNumber}."
        };
        var runs = int.TryParse(Environment.GetEnvironmentVariable("KNOTE_SMOKE_RUNS"), out var n) && n > 0 ? n : 2;

        var proposed = 0;
        var total = 0;
        foreach (var request in requests)
            for (var run = 0; run < runs; run++)
            {
                var tools = new KNoteAiTools(fixture.Service, new Host(provider));
                tools.BeginTurn(null, null);
                var turnTools = await tools.GetToolsAsync();
                var client = AiChatClientFactory.Create(provider, turnTools);
                var system = await AiAssistantContext.ComposeAsync(fixture.Service, null, null, request);
                var called = new List<string>();
                var response = await client.GetResponseAsync([new ChatMessage(ChatRole.System, system.Text), new ChatMessage(ChatRole.User, request)],
                    new ChatOptions { Tools = turnTools.ToList() });
                called.AddRange(response.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Select(c => c.Name));

                total++;
                var didPropose = tools.TurnActions.Any(a => a.Kind == AiActionRules.ProcessDocument);
                if (didPropose)
                    proposed++;
                var answer = response.Text.Replace('\n', ' ');
                Console.WriteLine($"[{(didPropose ? "PROPOSED" : "no card ")}] {request}\n   tools: {string.Join(", ", called)}\n   answer: {answer[..Math.Min(200, answer.Length)]}\n");
            }

        Console.WriteLine($"process_document proposed in {proposed} of {total}.");
        Assert.AreEqual(total, proposed, "the model proposed process_document every time");
    }
}
