using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The search by meaning of the photos and the documents (KNote Atlas, stage 4, D14), through the Service with the fake embedding
/// model and the reader of the app: «Photos» and «Documents» give the resources of that kind by their own fragments (the text of the
/// note doesn't count), each with its note, its fragment and its page, never the ones of the system notes; «All» gives the notes, and
/// says the file and the page of a fragment of a resource. Dapper and EF.
/// </summary>
[TestClass]
public class SemanticResourceSearchServiceTests
{
    private SemanticIndexFixture fixture = null!;
    private string rootPath = null!;

    private async Task OpenAsync(string orm)
    {
        fixture = await SemanticIndexFixture.CreateAsync(orm);
        fixture.Service.DocumentReader = new AiDocumentReader();
        rootPath = Path.Combine(Path.GetTempPath(), $"knote-found-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootPath);
        var repositoryRef = fixture.Service.Repository.RepositoryRef;
        repositoryRef.ResourcesContainer = "KntResources";
        repositoryRef.ResourcesContainerRootPath = rootPath;
        repositoryRef.ResourcesContainerRootUrl = "http://intranet/KNote";
    }

    [TestCleanup]
    public void TestCleanup()
    {
        fixture?.Dispose();
        if (rootPath != null && Directory.Exists(rootPath))
            Directory.Delete(rootPath, true);
    }

    private async Task<ResourceDto> AddResourceAsync(Guid noteId, string name, string description, byte[] content, string fileType)
    {
        var saved = await fixture.Service.Notes.SaveResourceAsync(new ResourceDto
        {
            NoteId = noteId, Name = $"{Guid.NewGuid()}_{name}", Description = description, FileType = fileType, ContentArrayBytes = content
        });
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity;
    }

    private async Task<SemanticSearchResultDto> SearchAsync(string text, EnumResourceKind kind)
    {
        var res = await fixture.Service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = text, ResourceKind = kind });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    // 4.6b: of the notes not up to date, the ones the update before a search leaves for the full indexing (a document not read yet)
    // and the ones that can't be brought up to date here (a document not read whose file is not in this installation).
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ThePendingNotes_SayWhyTheyArePending(string orm)
    {
        await OpenAsync(orm);
        await fixture.AddNoteAsync("Indexed", "Spectrum keyboard membrane replaced.");
        await fixture.RunAsync();
        var withDocument = await fixture.AddNoteAsync("With a manual", "Notes of the workshop.");
        await AddResourceAsync(withDocument.NoteId, "manual.pdf", "manual.pdf", TestDocuments.Pdf("The manual of the computer."), "application/pdf");
        var withoutFile = await fixture.AddNoteAsync("With a manual not here", "More notes.");
        await AddResourceAsync(withoutFile.NoteId, "away.pdf", "away.pdf", TestDocuments.Pdf("Far away."), "application/pdf");
        foreach (var file in Directory.GetFiles(rootPath, "*away.pdf", SearchOption.AllDirectories))
            File.Delete(file);

        var found = await SearchAsync("spectrum keyboard membrane", EnumResourceKind.All);

        Assert.AreEqual((2, 1, 1), (found.PendingNotes, found.NotesForFullRun, found.NotesWithoutFiles));
        Assert.IsTrue(SemanticIndexTexts.ShowsPendingNotice(found.PendingNotes, found.NotesWithoutFiles));

        await fixture.RunAsync();
        var afterFullRun = await SearchAsync("spectrum keyboard membrane", EnumResourceKind.All);

        Assert.AreEqual((1, 0, 1), (afterFullRun.PendingNotes, afterFullRun.NotesForFullRun, afterFullRun.NotesWithoutFiles),
            "the full indexing read the manual; the one whose file is not here stays");
        Assert.IsFalse(SemanticIndexTexts.ShowsPendingNotice(afterFullRun.PendingNotes, afterFullRun.NotesWithoutFiles), "it doesn't show the notice");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task PhotosAndDocuments_AreFoundByTheirOwnFragments_WithTheirNoteAndPage(string orm)
    {
        await OpenAsync(orm);
        var note = await fixture.AddNoteAsync("Workshop", "Spectrum keyboard membrane replaced.");
        var photo = await AddResourceAsync(note.NoteId, "beach.png", "Grandparents on the beach with umbrellas and sand.", [1, 2, 3], "image/png");
        var manual = await AddResourceAsync(note.NoteId, "manual.pdf", "manual.pdf",
            TestDocuments.Pdf(string.Join("\n", Enumerable.Repeat("An introduction to the parts of the computer and how to use it.", 8)),
                "Power supply regulator voltage transformer."), "application/pdf");
        Assert.IsTrue((await fixture.Service.ResourceContent.ReadDocumentAsync(manual.ResourceId)).IsValid);
        var sessionsType = (await fixture.Service.NoteTypes.SaveAsync(new NoteTypeDto { Name = KntConst.ChatSessionsTag })).Entity;
        var session = await fixture.AddNoteAsync("A conversation", "", noteTypeId: sessionsType.NoteTypeId);
        await AddResourceAsync(session.NoteId, "attachment.png", "Grandparents on the beach with umbrellas and sand.", [1, 2, 3], "image/png");
        await fixture.RunAsync();

        var photos = await SearchAsync("grandparents beach umbrellas sand", EnumResourceKind.Photos);
        var documents = await SearchAsync("power supply regulator voltage transformer", EnumResourceKind.Documents);
        var noPhotos = await SearchAsync("spectrum keyboard membrane replaced", EnumResourceKind.Photos);
        var all = await SearchAsync("power supply regulator voltage transformer", EnumResourceKind.All);

        var foundPhoto = photos.Resources.Single();
        Assert.AreEqual(0, photos.Notes.Count);
        Assert.AreEqual((photo.ResourceId, note.NoteId, note.NoteNumber, "Workshop"),
            (foundPhoto.Resource.ResourceId, foundPhoto.NoteId, foundPhoto.NoteNumber, foundPhoto.Topic));
        Assert.IsNull(foundPhoto.Resource.ContentBase64, "without its file");
        Assert.IsFalse(string.IsNullOrEmpty(foundPhoto.Resource.FullUrl), "to open it");
        var foundPage = documents.Resources.Single();
        Assert.AreEqual((manual.ResourceId, 2, NoteChunkSourceType.ResourceText), (foundPage.Resource.ResourceId, foundPage.FragmentPage, foundPage.FragmentSource));
        StringAssert.Contains(foundPage.Fragment, "regulator");
        Assert.AreEqual(0, noPhotos.Resources.Count, "the text of the note doesn't count");
        var foundNote = all.Notes.First();
        Assert.AreEqual((note.NoteId, "manual.pdf", 2), (foundNote.NoteId, foundNote.FragmentResource, foundNote.FragmentPage));
        StringAssert.StartsWith(foundNote.FragmentShown(), "(in manual.pdf, p. 2) ");
    }
}
