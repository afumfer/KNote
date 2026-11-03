using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The search by meaning through the API (POST api/semantic/search, KNote Atlas 0.4) and the similar notes of a note (GET
/// api/semantic/notes/{id}/similar, 0.5), with a fake embedding model in the Server: any user searches, also with a
/// filter, and gets the notes with their similarity and fragment, and the similar notes. A host of its own: the index is
/// created here (SemanticIndexInProcessTests starts from a repository without one).
/// </summary>
[TestClass]
public class SearchByMeaningInProcessTests
{
    private const string Model = "fake-embed";
    private const string Query = "keyboard membrane spectrum";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _admin = null!;
    private static NoteDto _note = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        var models = new FakeEmbeddingGenerators(Model);
        (_factory, _admin) = await InProcessTestHost.CreateAuthenticatedClientAsync(factory =>
            factory.TestServices = services => services.AddSingleton<IKntEmbeddingGenerators>(models));

        var service = _factory.CreateServiceForRepository(0);
        service.EmbeddingGenerators = models;
        Assert.IsTrue((await service.SystemValues.SaveRepositoryAiModelsAsync(new RepositoryAiModelsDto { EmbeddingModel = Model })).IsValid);
        var note = (await service.Notes.NewAsync()).Entity;
        note.Topic = "Spectrum";
        note.Description = "# Repair\n\nReplaced the keyboard membrane of the Spectrum.";
        note.FolderId = (await service.Folders.GetHomeAsync()).Entity.FolderId;
        _note = (await service.Notes.SaveAsync(note)).Entity;
        var run = await service.SemanticSearch.RunIndexAsync(new SemanticIndexRunRequest());
        Assert.IsTrue(run.IsValid, run.ErrorMessage);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _admin.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task AGuest_SearchesByMeaning_AlsoWithAFilter()
    {
        using var guest = await GuestClientAsync();

        var found = await ReadAsync<SemanticSearchResultDto>(await guest.PostAsJsonAsync("api/semantic/search",
            new SemanticSearchRequestDto { Text = Query }));
        var filtered = await ReadAsync<SemanticSearchResultDto>(await guest.PostAsJsonAsync("api/semantic/search",
            SemanticSearchRequestDto.ForFilter(new NotesFilterDto { Topic = "Nothing like this", MeaningText = Query })));

        var hit = found.Notes.Single(n => n.NoteId == _note.NoteId);
        Assert.AreEqual("Spectrum", hit.Topic);
        Assert.IsTrue(hit.Similarity >= SemanticSearchRules.MinSimilarity);
        StringAssert.Contains(hit.Fragment, "keyboard membrane");
        Assert.AreEqual(0, filtered.Notes.Count, "only among the notes of the filter");
    }

    [TestMethod]
    public async Task AGuest_GetsTheSimilarNotesOfANote()
    {
        // Their own words: the searches of the other tests do not find them.
        var service = _factory.CreateServiceForRepository(0);
        service.EmbeddingGenerators = new FakeEmbeddingGenerators(Model);
        var homeId = (await service.Folders.GetHomeAsync()).Entity.FolderId;
        var notes = new List<NoteDto>();
        foreach (var topic in new[] { "Amstrad cassette deck", "Amstrad cassette deck again" })
        {
            var note = (await service.Notes.NewAsync()).Entity;
            note.Topic = topic;
            note.Description = "Cleaned the cassette head and replaced the drive belt of the datacorder.";
            note.FolderId = homeId;
            notes.Add((await service.Notes.SaveAsync(note)).Entity);
        }
        Assert.IsTrue((await service.SemanticSearch.RunIndexAsync(new SemanticIndexRunRequest())).IsValid);
        using var guest = await GuestClientAsync();

        var similar = await ReadAsync<SemanticSearchResultDto>(await guest.GetAsync($"api/semantic/notes/{notes[0].NoteId}/similar"));
        var unknown = await guest.GetAsync($"api/semantic/notes/{Guid.NewGuid()}/similar");

        Assert.AreEqual(notes[1].NoteId, similar.Notes.First().NoteId);
        Assert.IsTrue(similar.Notes.All(n => n.NoteId != notes[0].NoteId));
        Assert.AreEqual(HttpStatusCode.BadRequest, unknown.StatusCode);
    }

    [TestMethod]
    public async Task TheListFoundByMeaning_IsExported_WithItsSimilarity()
    {
        var file = await ReadAsync<ReportFileDto>(await _admin.PostAsJsonAsync("api/reports/noteslistcsv", new NotesListReportRequestDto
        {
            Source = NotesListReportSource.Search,
            SearchMode = EnumNotesSearchMode.Meaning,
            TextSearch = Query
        }));
        var filtered = await ReadAsync<ReportDto>(await _admin.PostAsJsonAsync("api/reports/noteslist", new NotesListReportRequestDto
        {
            Source = NotesListReportSource.Filter,
            NotesFilter = new NotesFilterDto { MeaningText = Query }
        }));

        var lines = file.Content.Split('\n');
        StringAssert.Contains(lines[0], "Similarity");
        StringAssert.Contains(file.Content, "Spectrum");
        StringAssert.Contains(filtered.Html, "Talks about");
        Assert.AreEqual(1, filtered.NotesCount);
    }

    [TestMethod]
    public async Task ASearchWithoutText_IsABadRequest_WithItsReason()
    {
        var httpRes = await _admin.PostAsJsonAsync("api/semantic/search", new SemanticSearchRequestDto { Text = " " });
        var result = await httpRes.Content.ReadFromJsonAsync<Result<SemanticSearchResultDto>>();

        Assert.AreEqual(HttpStatusCode.BadRequest, httpRes.StatusCode);
        StringAssert.Contains(result!.ErrorMessage, "The text to search is required.");
    }

    [TestMethod]
    public async Task AProjectManager_EvaluatesTheSearch_AndSavesTheReport_AGuestCannot()
    {
        var setup = await ReadAsync<SearchEvaluationSetupDto>(await _admin.GetAsync("api/semantic/evaluation"));
        var query = new SearchTestQueryDto { Row = 1, Query = Query, ExpectedNotes = { _note.NoteNumber } };
        var evaluated = await ReadAsync<SearchEvaluationQueryResultDto>(await _admin.PostAsJsonAsync("api/semantic/evaluation/query",
            new SearchEvaluationQueryRequestDto { Query = query, IndexKeys = setup.Indexes.Select(i => i.IndexKey).ToList() }));
        var report = SearchEvaluationReport.Markdown("Tests", DateTime.Now, new[] { evaluated }, setup.QueryInstruction);
        var saved = await ReadAsync<NoteDto>(await _admin.PostAsJsonAsync("api/semantic/evaluation/report", new SearchEvaluationReportDto { Markdown = report }));
        using var guest = await GuestClientAsync();
        var forbidden = await guest.GetAsync("api/semantic/evaluation");

        Assert.AreNotEqual(Guid.Empty, setup.TestQueriesNoteId);
        Assert.AreEqual(Model, setup.Indexes.Single().IndexKey);
        Assert.AreEqual(1, evaluated.Runs.Single(r => r.Mode == EnumNotesSearchMode.Meaning).Position);
        StringAssert.StartsWith(saved.Topic, "Search evaluation · ");
        Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    // Registered after the class' Admin: a Guest (KntUsersRegisterAsyncCommand).
    private static async Task<HttpClient> GuestClientAsync()
    {
        var client = _factory.CreateClient();
        var token = await (await client.PostAsJsonAsync("api/users/register", new UserRegisterDto
        {
            UserName = $"guest-{Guid.NewGuid():N}"[..24],
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = "Atlas guest",
            Password = "InProcess-Test-Password-1!"
        })).Content.ReadFromJsonAsync<UserTokenDto>();
        Assert.IsTrue(token!.success, token.error);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.token);
        return client;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage httpRes)
    {
        var result = await httpRes.Content.ReadFromJsonAsync<Result<T>>();
        Assert.IsNotNull(result);
        Assert.IsTrue(httpRes.IsSuccessStatusCode && result.IsValid, $"{httpRes.StatusCode}: {result.ErrorMessage}");
        return result.Entity;
    }
}
