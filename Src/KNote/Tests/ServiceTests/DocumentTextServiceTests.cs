using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The text of the documents in the Service (KNote Atlas, stage 4, 4.4a): «Read document» keeps it (from the folder or from
/// the database) without touching the note, a file that is not here or can't be read, replacing the file forgets it, and the
/// semantic index reads each document once (not in a quick refresh), with fragments by page. 4.4b: the upload of a resource
/// from a stream (to the folder without reading it back, or to the database) and the beginning of a text by whole pages.
/// Dapper and EF.
/// </summary>
[TestClass]
public class DocumentTextServiceTests
{
    // The reader of the app, counting how many times it reads.
    private sealed class CountingReader : IKntDocumentReader
    {
        private readonly AiDocumentReader _reader = new();
        public int Reads { get; private set; }

        public List<DocumentPageDto> Read(Stream content, EnumDocumentFormat format)
        {
            Reads++;
            return _reader.Read(content, format);
        }

        public byte[] DrawPage(Stream content, int pageNumber, int maxSide) => _reader.DrawPage(content, pageNumber, maxSide);
    }

    private SemanticIndexFixture fixture = null!;
    private CountingReader reader = null!;
    private string rootPath = null!;

    private async Task OpenAsync(string orm, bool contentInDb = false)
    {
        fixture = await SemanticIndexFixture.CreateAsync(orm);
        reader = new CountingReader();
        fixture.Service.DocumentReader = reader;
        rootPath = Path.Combine(Path.GetTempPath(), $"knote-documents-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootPath);
        var repositoryRef = fixture.Service.Repository.RepositoryRef;
        repositoryRef.ResourcesContainer = "KntResources";
        repositoryRef.ResourcesContainerRootPath = rootPath;
        repositoryRef.ResourcesContainerRootUrl = "http://intranet/KNote";
        repositoryRef.ResourceContentInDB = contentInDb;
    }

    [TestCleanup]
    public void TestCleanup()
    {
        fixture?.Dispose();
        if (rootPath != null && Directory.Exists(rootPath))
            Directory.Delete(rootPath, true);
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

    private void DeleteFiles(string name)
    {
        foreach (var file in Directory.GetFiles(rootPath, "*" + name, SearchOption.AllDirectories))
            File.Delete(file);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ReadDocument_KeepsItsText_WithThePagesToTranscribe_AndDoesntTouchTheNote(string orm)
    {
        await OpenAsync(orm);
        var note = await fixture.AddNoteAsync("The QL");
        var pdf = await AddResourceAsync(note.NoteId, "manual.pdf", TestDocuments.MixedPdf("The QL has two microdrives.", "SCANNED PAGE"));
        await fixture.RunAsync();
        var date = (await fixture.Service.Repository.Notes.GetAsync(note.NoteId)).Entity.ModificationDateTime;

        var read = await fixture.Service.ResourceContent.ReadDocumentAsync(pdf.ResourceId);

        Assert.IsTrue(read.IsValid, read.ErrorMessage);
        Assert.AreEqual((EnumExtractedState.Partial, 2, 1), (read.Entity.State, read.Entity.Pages, read.Entity.PagesToTranscribe));
        var text = (await fixture.Service.ResourceContent.GetDocumentTextAsync(pdf.ResourceId)).Entity;
        StringAssert.Contains(text.Text, "two microdrives");
        Assert.AreEqual(date, (await fixture.Service.Repository.Notes.GetAsync(note.NoteId)).Entity.ModificationDateTime);
        Assert.IsTrue(await fixture.IsPendingAsync(note.NoteId), "its fragments change");
    }

    [TestMethod]
    public async Task ADocumentInTheDatabase_IsReadFromMemory()
    {
        await OpenAsync("EntityFramework", contentInDb: true);
        var note = await fixture.AddNoteAsync("Letters");
        var docx = await AddResourceAsync(note.NoteId, "letter.docx", TestDocuments.Docx(["Dear friend, the QL arrived."]),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        DeleteFiles("letter.docx");

        var read = await fixture.Service.ResourceContent.ReadDocumentAsync(docx.ResourceId);

        Assert.IsTrue(read.IsValid, read.ErrorMessage);
        StringAssert.Contains((await fixture.Service.ResourceContent.GetDocumentTextAsync(docx.ResourceId)).Entity.Text, "the QL arrived");
    }

    [TestMethod]
    public async Task AFileNotHere_APhoto_AndADamagedDocument_AreTold()
    {
        await OpenAsync("Dapper");
        var note = await fixture.AddNoteAsync("Files");
        var away = await AddResourceAsync(note.NoteId, "away.pdf", TestDocuments.Pdf("Far away."));
        var photo = await AddResourceAsync(note.NoteId, "photo.png", [1, 2, 3], "image/png");
        var damaged = await AddResourceAsync(note.NoteId, "damaged.pdf", TestDocuments.Damaged);
        DeleteFiles("away.pdf");

        var notHere = await fixture.Service.ResourceContent.ReadDocumentAsync(away.ResourceId);
        var notADocument = await fixture.Service.ResourceContent.ReadDocumentAsync(photo.ResourceId);
        var unreadable = await fixture.Service.ResourceContent.ReadDocumentAsync(damaged.ResourceId);

        StringAssert.Contains(notHere.ErrorMessage, "not available in this installation");
        StringAssert.Contains(notADocument.ErrorMessage, "not a document");
        Assert.IsTrue(unreadable.IsValid, unreadable.ErrorMessage);
        Assert.AreEqual(EnumExtractedState.Unreadable, unreadable.Entity.State);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ReplacingTheFile_ForgetsWhatWasRead(string orm)
    {
        await OpenAsync(orm);
        var note = await fixture.AddNoteAsync("Versions");
        var pdf = await AddResourceAsync(note.NoteId, "v1.pdf", TestDocuments.Pdf("Version one."));
        Assert.IsTrue((await fixture.Service.ResourceContent.ReadDocumentAsync(pdf.ResourceId)).IsValid);

        var stored = (await fixture.Service.Notes.GetResourceAsync(pdf.ResourceId)).Entity;
        stored.Name = $"{Guid.NewGuid()}_v2.pdf";
        stored.ContentArrayBytes = TestDocuments.Pdf("Version two.");
        Assert.IsTrue((await fixture.Service.Notes.SaveResourceAsync(stored)).IsValid);

        var text = (await fixture.Service.ResourceContent.GetDocumentTextAsync(pdf.ResourceId)).Entity;
        Assert.AreEqual((EnumExtractedState.NotRead, (string?)null), (text.State, text.Text));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheIndex_ReadsADocumentOnce_AndGivesItsFragmentsByPage(string orm)
    {
        await OpenAsync(orm);
        var note = await fixture.AddNoteAsync("Sinclair QL");
        var pdf = await AddResourceAsync(note.NoteId, "manual.pdf",
            TestDocuments.Pdf("Chapter 1. Microdrives and their cartridges.", "Chapter 2. SuperBASIC procedures."));

        await fixture.RunAsync();
        note.Topic = "Sinclair QL, the manual";
        Assert.IsTrue((await fixture.Service.Notes.SaveAsync(note)).IsValid);
        await fixture.RunAsync();

        Assert.AreEqual(1, reader.Reads, "the second time, its text is kept: the file is not opened");
        var chunks = (await fixture.ChunksAsync(note.NoteId)).Where(c => c.SourceType == NoteChunkSourceType.ResourceText).ToList();
        Assert.IsTrue(chunks.All(c => c.SourceId == pdf.ResourceId));
        CollectionAssert.AreEqual(new int?[] { 1 }, chunks.Select(c => c.Page).ToList(), "two short pages: one fragment, from page 1");
        StringAssert.Contains(chunks[0].Content, "SuperBASIC");
    }

    private ResourceDto NewUpload(Guid noteId, string fileName) => new()
    {
        NoteId = noteId, Name = fileName, FileType = "application/pdf", Description = fileName, LoadedDescription = fileName, Order = 2
    };

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AnUpload_WritesItsFileToTheFolder_AndIsADocumentToRead(string orm)
    {
        await OpenAsync(orm);
        var note = await fixture.AddNoteAsync("Manuals");
        var content = TestDocuments.Pdf("The QL user guide.");

        using var stream = new MemoryStream(content);
        var uploaded = await fixture.Service.Notes.UploadResourceAsync(NewUpload(note.NoteId, "user guide.pdf"), stream, content.Length);

        Assert.IsTrue(uploaded.IsValid, uploaded.ErrorMessage);
        var resource = uploaded.Entity;
        Assert.AreEqual(note.NoteId, resource.NoteId);
        Assert.AreEqual("user_guide.pdf", ResourceInfoDto.FileNameOf(resource.Name), "the name of the file, with a prefix of its own");
        Assert.AreEqual(2, resource.Order);
        Assert.IsNull(resource.ContentBase64);
        Assert.IsFalse(string.IsNullOrEmpty(resource.FullUrl));
        var files = Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories);
        Assert.AreEqual(1, files.Length, "no .part file is left");
        CollectionAssert.AreEqual(content, File.ReadAllBytes(files[0]));

        var read = await fixture.Service.ResourceContent.ReadDocumentAsync(resource.ResourceId);
        Assert.IsTrue(read.IsValid, read.ErrorMessage);
        StringAssert.Contains((await fixture.Service.ResourceContent.GetDocumentTextAsync(resource.ResourceId)).Entity.Text, "user guide");
    }

    [TestMethod]
    public async Task AnUpload_ToARepositoryThatKeepsTheFilesInTheDatabase_KeepsItThere()
    {
        await OpenAsync("EntityFramework", contentInDb: true);
        var note = await fixture.AddNoteAsync("Manuals");
        var content = TestDocuments.Pdf("Kept in the database.");

        using var stream = new MemoryStream(content);
        var uploaded = await fixture.Service.Notes.UploadResourceAsync(NewUpload(note.NoteId, "db.pdf"), stream, content.Length);

        Assert.IsTrue(uploaded.IsValid, uploaded.ErrorMessage);
        var stored = (await fixture.Service.Repository.Notes.GetResourceAsync(uploaded.Entity.ResourceId)).Entity;
        Assert.IsTrue(stored.ContentInDB);
        CollectionAssert.AreEqual(content, stored.ContentArrayBytes);
    }

    [TestMethod]
    public async Task AnUpload_EmptyOrTooBig_IsRefused_AndLeavesNoFile()
    {
        await OpenAsync("Dapper");
        var note = await fixture.AddNoteAsync("Manuals");

        using var empty = new MemoryStream();
        var refusedEmpty = await fixture.Service.Notes.UploadResourceAsync(NewUpload(note.NoteId, "empty.pdf"), empty, 0);
        using var small = new MemoryStream([1, 2, 3]);
        var refusedBig = await fixture.Service.Notes.UploadResourceAsync(NewUpload(note.NoteId, "big.pdf"), small, KntConst.MaxLenNoteResourceFile + 1);
        // What was said of its length is not trusted: an empty stream is refused while it is written.
        using var lies = new MemoryStream();
        var refusedLie = await fixture.Service.Notes.UploadResourceAsync(NewUpload(note.NoteId, "lie.pdf"), lies, 10);

        StringAssert.Contains(refusedEmpty.ErrorMessage, "empty");
        StringAssert.Contains(refusedBig.ErrorMessage, "too big");
        StringAssert.Contains(refusedLie.ErrorMessage, "empty");
        Assert.AreEqual(0, Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories).Length);
        Assert.AreEqual(0, (await fixture.Service.Notes.GetResourcesAsync(note.NoteId)).Entity.Count);
    }

    // The list of a note's resources (what the Web shows in its tab) doesn't bring their files: with one of 200 MB it took minutes.
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task TheListOfTheResources_DoesntBringTheirFiles_ButASmallImageInTheDatabase(bool contentInDb)
    {
        await OpenAsync("EntityFramework", contentInDb);
        var note = await fixture.AddNoteAsync("Gallery");
        await AddResourceAsync(note.NoteId, "manual.pdf", TestDocuments.Pdf("A manual."));
        await AddResourceAsync(note.NoteId, "photo.png", [1, 2, 3], "image/png");

        var list = (await fixture.Service.Notes.GetResourcesInfoAsync(note.NoteId)).Entity;

        Assert.AreEqual(2, list.Count);
        Assert.IsNull(list.Single(r => r.NameOut == "manual.pdf").ContentBase64);
        Assert.AreEqual(contentInDb ? Convert.ToBase64String([1, 2, 3]) : null, list.Single(r => r.NameOut == "photo.png").ContentBase64);
        Assert.IsTrue(list.All(r => !string.IsNullOrEmpty(r.FullUrl) && r.LoadedDescription == r.Description));
    }

    [TestMethod]
    public async Task TheBeginningOfAText_HasWholePages()
    {
        await OpenAsync("Dapper");
        var note = await fixture.AddNoteAsync("Manuals");
        var pages = Enumerable.Range(1, 5).Select(i => $"Page {i}: " + new string('x', 40)).ToArray();
        var pdf = await AddResourceAsync(note.NoteId, "pages.pdf", TestDocuments.Pdf(pages));
        Assert.IsTrue((await fixture.Service.ResourceContent.ReadDocumentAsync(pdf.ResourceId)).IsValid);

        var beginning = (await fixture.Service.ResourceContent.GetDocumentTextAsync(pdf.ResourceId, 120)).Entity;
        var whole = (await fixture.Service.ResourceContent.GetDocumentTextAsync(pdf.ResourceId)).Entity;

        Assert.AreEqual((2, 5), (beginning.PagesShown, beginning.PagesCount));
        Assert.AreEqual(2, DocumentTextRules.Pages(beginning.Text).Count);
        Assert.AreEqual((5, 5), (whole.PagesShown, whole.PagesCount));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AQuickRefresh_DoesntReadDocuments_AndLeavesTheNotePendingForTheFullRun(string orm)
    {
        await OpenAsync(orm);
        var note = await fixture.AddNoteAsync("Quick");
        await AddResourceAsync(note.NoteId, "quick.pdf", TestDocuments.Pdf("Read in a full run."));

        // A search comes first: the note is indexed without its document, but it stays pending.
        await fixture.Service.SemanticSearch.RefreshPendingAsync(new SemanticIndexRefreshRequest());
        var first = await fixture.ChunksAsync(note.NoteId);
        Assert.AreEqual(0, reader.Reads);
        Assert.IsTrue(first.Count > 0, "the rest of the note is searchable");
        Assert.IsFalse(first.Any(c => c.SourceType == NoteChunkSourceType.ResourceText));
        Assert.AreEqual(1, (await fixture.StatusAsync()).PendingNotes);

        // The next searches don't write it again while it doesn't change.
        await fixture.Service.SemanticSearch.RefreshPendingAsync(new SemanticIndexRefreshRequest());
        CollectionAssert.AreEqual(first.Select(c => c.IndexedDateTime).ToList(),
            (await fixture.ChunksAsync(note.NoteId)).Select(c => c.IndexedDateTime).ToList());

        // «Update now», without touching the note: it reads the document.
        var progress = await fixture.RunAsync();

        Assert.AreEqual((EnumSemanticIndexPhase.Completed, 0, 0), (progress.Phase, progress.ErrorsCount, progress.DocumentsNotAvailable));
        Assert.AreEqual(1, reader.Reads);
        Assert.IsTrue((await fixture.ChunksAsync(note.NoteId)).Any(c => c.SourceType == NoteChunkSourceType.ResourceText));
        Assert.AreEqual(0, (await fixture.StatusAsync()).PendingNotes);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AFileNotHere_IsCounted_AndTriedAgainInEachFullRun(string orm)
    {
        await OpenAsync(orm);
        var away = await fixture.AddNoteAsync("Away");
        await AddResourceAsync(away.NoteId, "away.pdf", TestDocuments.Pdf("Far away."));
        var file = Directory.GetFiles(rootPath, "*away.pdf", SearchOption.AllDirectories).Single();
        var elsewhere = Path.Combine(rootPath, "elsewhere.bin");
        File.Move(file, elsewhere);

        var progress = await fixture.RunAsync();
        Assert.AreEqual((EnumSemanticIndexPhase.Completed, 0, 1), (progress.Phase, progress.ErrorsCount, progress.DocumentsNotAvailable));
        Assert.IsTrue((await fixture.ChunksAsync(away.NoteId)).Count > 0, "the rest of the note is indexed");
        Assert.AreEqual(1, (await fixture.StatusAsync()).PendingNotes);

        progress = await fixture.RunAsync();
        Assert.AreEqual(1, progress.DocumentsNotAvailable);

        // The file is here now: the next full run reads it.
        File.Move(elsewhere, file);
        progress = await fixture.RunAsync();
        Assert.AreEqual(0, progress.DocumentsNotAvailable);
        Assert.IsTrue((await fixture.ChunksAsync(away.NoteId)).Any(c => c.SourceType == NoteChunkSourceType.ResourceText));
        Assert.AreEqual(0, (await fixture.StatusAsync()).PendingNotes);
    }
}
