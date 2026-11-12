using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The assistant reads documents (KNote Atlas, stage 4, D14, 4.7a), against a real repository and the reader of the app, called as the
/// model calls them: read_document gives the text by pages with the scanned ones marked (a document not read yet is read now and not
/// kept), view_document_pages draws pages of a PDF as images of the turn, both also on a document attached to the conversation, and
/// get_note_items says what was read of each document. The system notes are out. Dapper and EF.
/// </summary>
[TestClass]
public class AiDocumentToolsTests
{
    private sealed class NoHost : IKNoteAiToolsHost
    {
        public Task<AiNoteDestination> GetNewNoteDestinationAsync() => Task.FromResult<AiNoteDestination>(null!);
        public void OnNoteCreated(IKntService service, NoteExtendedDto note, bool openEditor = true) { }
    }

    private SemanticIndexFixture fixture = null!;
    private KNoteAiTools tools = null!;
    private string rootPath = null!;

    private async Task OpenAsync(string orm = "EntityFramework")
    {
        fixture = await SemanticIndexFixture.CreateAsync(orm);
        fixture.Service.UserIdentityName = (await fixture.Service.Users.GetAsync(fixture.UserId)).Entity.UserName;
        fixture.Service.DocumentReader = new AiDocumentReader();
        rootPath = Path.Combine(Path.GetTempPath(), $"knote-doc-tools-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootPath);
        var repositoryRef = fixture.Service.Repository.RepositoryRef;
        repositoryRef.ResourcesContainer = "KntResources";
        repositoryRef.ResourcesContainerRootPath = rootPath;
        repositoryRef.ResourcesContainerRootUrl = "http://intranet/KNote";
        tools = new KNoteAiTools(fixture.Service, new NoHost());
        tools.BeginTurn(null, null);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        fixture?.Dispose();
        if (rootPath != null && Directory.Exists(rootPath))
            Directory.Delete(rootPath, true);
    }

    private async Task<string> CallAsync(string tool, params (string Name, object Value)[] arguments)
    {
        var function = tools.GetTools().OfType<AIFunction>().Single(t => t.Name == tool);
        var result = await function.InvokeAsync(new AIFunctionArguments(arguments.ToDictionary(a => a.Name, a => (object?)a.Value)));
        return result is System.Text.Json.JsonElement json ? json.GetString()! : result?.ToString()!;
    }

    private async Task<ResourceDto> AddResourceAsync(Guid noteId, string name, byte[] content, string fileType = "application/pdf")
    {
        var saved = await fixture.Service.Notes.SaveResourceAsync(new ResourceDto
        {
            NoteId = noteId, Name = $"{Guid.NewGuid()}_{name}", Description = name, FileType = fileType, ContentArrayBytes = content
        });
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity;
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ReadDocument_GivesThePages_WithTheScannedOnesMarked_AndANotReadOneIsNotKept(string orm)
    {
        await OpenAsync(orm);
        var note = await fixture.AddNoteAsync("Manuals", "");
        var manual = await AddResourceAsync(note.NoteId, "manual.pdf", TestDocuments.Pdf("The first page.", "The second page."));
        var scan = await AddResourceAsync(note.NoteId, "scan.pdf", TestDocuments.MixedPdf("A page of text.", "SCANNED PAGE"));
        Assert.IsTrue((await fixture.Service.ResourceContent.ReadDocumentAsync(scan.ResourceId)).IsValid);
        var context = (await fixture.Service.ResourceContent.GetPageContextAsync(scan.ResourceId, 2)).Entity;
        Assert.IsTrue((await fixture.Service.ResourceContent.SaveTranscriptionAsync(new PageTranscriptionSaveDto
        {
            ResourceId = scan.ResourceId, Page = 2, Text = "The transcribed page.", ExpectedName = context.ExpectedName
        })).IsValid);

        var read = await CallAsync("read_document", ("noteNumber", note.NoteNumber), ("fileName", "MANUAL.pdf"));
        var transcribed = await CallAsync("read_document", ("noteNumber", note.NoteNumber), ("fileName", "scan"), ("fromPage", 2));
        var which = await CallAsync("read_document", ("noteNumber", note.NoteNumber));

        StringAssert.StartsWith(read, "manual.pdf: 2 pages. Pages 1–2.");
        StringAssert.Contains(read, "[Page 1]\nThe first page.");
        StringAssert.Contains(read, "[Page 2]\nThe second page.");
        Assert.AreEqual(EnumExtractedState.NotRead, (await fixture.Service.ResourceContent.GetDocumentTextAsync(manual.ResourceId)).Entity.State,
            "read now, not kept");
        StringAssert.Contains(transcribed, "[Page 2: automatic transcription of a scanned page, it may have mistakes]\nThe transcribed page.");
        Assert.IsFalse(transcribed.Contains("A page of text."), "from the page asked");
        StringAssert.Contains(which, "say which document:");
        StringAssert.Contains(which, "manual.pdf");
        StringAssert.Contains(which, "scan.pdf");
    }

    [TestMethod]
    public async Task ReadDocument_SaysWhyItCant()
    {
        await OpenAsync();
        var empty = await fixture.AddNoteAsync("Nothing", "");
        var glossary = (await fixture.Service.AtlasContext.CreateGlossaryAsync()).Entity;
        await AddResourceAsync(glossary.NoteId, "secret.pdf", TestDocuments.Pdf("Secret."));
        var note = await fixture.AddNoteAsync("Away", "");
        await AddResourceAsync(note.NoteId, "away.pdf", TestDocuments.Pdf("Far away."));
        foreach (var file in Directory.GetFiles(rootPath, "*away.pdf", SearchOption.AllDirectories))
            File.Delete(file);

        StringAssert.Contains(await CallAsync("read_document", ("noteNumber", empty.NoteNumber)), "has no documents");
        StringAssert.Contains(await CallAsync("read_document", ("noteNumber", glossary.NoteNumber)), "Note not found", "a system note");
        StringAssert.Contains(await CallAsync("read_document", ("noteNumber", note.NoteNumber)), AiDocumentRules.FileNotAvailableMessage);
        StringAssert.Contains(await CallAsync("read_document"), "this conversation has no documents");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ViewDocumentPages_DrawsThePagesAsked_AsImagesOfTheTurn(string orm)
    {
        await OpenAsync(orm);
        var note = await fixture.AddNoteAsync("Manuals", "");
        await AddResourceAsync(note.NoteId, "manual.pdf", TestDocuments.Pdf("One.", "Two.", "Three."));
        await AddResourceAsync(note.NoteId, "letter.docx", TestDocuments.Docx(["Dear friend."]), "");

        var seen = await CallAsync("view_document_pages", ("noteNumber", note.NoteNumber), ("fileName", "manual.pdf"), ("pages", new[] { 1, 3 }));
        var outside = await CallAsync("view_document_pages", ("noteNumber", note.NoteNumber), ("fileName", "manual.pdf"), ("pages", new[] { 9 }));
        var docx = await CallAsync("view_document_pages", ("noteNumber", note.NoteNumber), ("fileName", "letter.docx"));

        Assert.AreEqual("manual.pdf has 3 pages. You receive these in the next message: page 1, page 3.", seen);
        Assert.AreEqual(2, tools.TurnImages.Count);
        StringAssert.Contains(outside, "there is no page 9");
        StringAssert.Contains(docx, "is not a PDF");
    }

    [TestMethod]
    public async Task ADocumentOfTheConversation_IsReadAndLookedAt()
    {
        await OpenAsync();
        var res = await fixture.Service.AiSessions.AddAttachmentAsync(new AiChatAttachmentUploadDto
        {
            Provider = EnumAiProvider.Ollama, Model = "m", FileName = "offer.pdf", ContentType = "application/pdf",
            ContentBase64 = Convert.ToBase64String(TestDocuments.Pdf("The offer of the computer.", "The price."))
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        tools.BeginTurn(null, null, res.Entity.SessionNoteId, new[] { res.Entity });

        var read = await CallAsync("read_document");
        var seen = await CallAsync("view_document_pages", ("pages", new[] { 2 }));

        StringAssert.StartsWith(read, "offer.pdf: 2 pages.");
        StringAssert.Contains(read, "The offer of the computer.");
        StringAssert.Contains(seen, "page 2");
        Assert.AreEqual(1, tools.TurnImages.Count);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetNoteItems_SaysWhatWasReadOfEachDocument(string orm)
    {
        await OpenAsync(orm);
        var note = await fixture.AddNoteAsync("Manuals", "");
        var scan = await AddResourceAsync(note.NoteId, "scan.pdf", TestDocuments.MixedPdf("A page of text.", "SCANNED PAGE"));
        await AddResourceAsync(note.NoteId, "manual.pdf", TestDocuments.Pdf("One."));
        Assert.IsTrue((await fixture.Service.ResourceContent.ReadDocumentAsync(scan.ResourceId)).IsValid);

        var items = await CallAsync("get_note_items", ("noteNumber", note.NoteNumber), ("include", new[] { "resources" }));

        StringAssert.Contains(items, "\"Name\":\"scan.pdf\"");
        StringAssert.Contains(items, "\"Document\":\"read\",\"Pages\":2,\"ScannedNotTranscribed\":1");
        StringAssert.Contains(items, "\"Document\":\"not read yet\"");
    }
}
