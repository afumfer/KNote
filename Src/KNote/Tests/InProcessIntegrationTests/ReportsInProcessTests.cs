using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The web app's printable reports and CSV export (api/reports): a folder, a quick search or a filter printed as a
/// list or a book or exported as CSV, in the order and with the columns requested, and a note's details.
/// </summary>
[TestClass]
public class ReportsInProcessTests
{
    // A word no seeded note has, so that a search only finds this suite's notes.
    private const string Word = "Zqxreport";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _httpClient = null!;
    private static FolderDto _folder = null!;
    private static readonly List<NoteDto> _notes = new();

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _httpClient) = await InProcessTestHost.CreateAuthenticatedClientAsync();

        var folders = await (await _httpClient.GetAsync("api/folders")).Content.ReadFromJsonAsync<Result<List<FolderInfoDto>>>();
        var home = folders!.Entity!.First(f => f.FolderNumber == 1);
        var folderRes = await (await _httpClient.PostAsJsonAsync("api/folders",
            new FolderDto { FolderId = Guid.Empty, Name = "__TEST REPORTS__", ParentId = home.FolderId })).Content.ReadFromJsonAsync<Result<FolderDto>>();
        Assert.IsTrue(folderRes!.IsValid, folderRes.ErrorMessage);
        _folder = folderRes.Entity!;

        _notes.Add(await CreateNoteAsync($"{Word} B", "Second", priority: 2));
        _notes.Add(await CreateNoteAsync($"{Word} A", "Hello **world**", priority: 3));
        _notes.Add(await CreateNoteAsync($"{Word} C", "- Third", priority: 1));
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task NotesList_OfAFolder_PrintsItsNotesInTheRequestedOrderAndColumns()
    {
        var report = await PostReportAsync("noteslist", FolderRequest("Topic", ascending: true, compact: false));

        Assert.AreEqual(3, report.NotesCount);
        Assert.AreEqual("Notes list", report.Title);
        StringAssert.Contains(report.Html, "__TEST REPORTS__");
        StringAssert.Contains(report.Html, "<th>Status</th>", "Extended view");
        AssertInOrder(report.Html, $"{Word} A", $"{Word} B", $"{Word} C");

        var byPriority = await PostReportAsync("noteslist", FolderRequest("Priority", ascending: true, compact: true));
        Assert.IsFalse(byPriority.Html.Contains("<th>Status</th>"), "Compact view");
        AssertInOrder(byPriority.Html, $"{Word} C", $"{Word} B", $"{Word} A");
    }

    [TestMethod]
    public async Task NotesListCsv_UsesTheUsersCultureSeparatorAndStartsWithABom()
    {
        var spanish = await PostCsvAsync(FolderRequest("Topic", ascending: true, compact: true, culture: "es-ES"));
        var english = await PostCsvAsync(FolderRequest("Topic", ascending: true, compact: true, culture: "en-US"));

        Assert.AreEqual('﻿', spanish.Content![0]);
        StringAssert.StartsWith(spanish.Content[1..], "#;Topic;Priority;Tags\r\n");
        StringAssert.StartsWith(english.Content![1..], "#,Topic,Priority,Tags\r\n");
        AssertInOrder(spanish.Content, $"{Word} A", $"{Word} B", $"{Word} C");
        StringAssert.EndsWith(spanish.FileName, ".csv");
        StringAssert.Contains(spanish.FileName, "__TEST REPORTS__");
        Assert.AreEqual("text/csv", spanish.ContentType);
    }

    [TestMethod]
    public async Task NotesBook_OfASearch_HasAChapterPerNoteWithItsDescription()
    {
        var report = await PostReportAsync("notesbook", new NotesListReportRequestDto
        {
            Source = NotesListReportSource.Search,
            TextSearch = Word,
            OrderColumn = "Topic",
            OrderAscending = false
        });

        Assert.AreEqual(3, report.NotesCount);
        Assert.AreEqual("Notes book", report.Title);
        StringAssert.Contains(report.Html, $"Search: {Word}");
        StringAssert.Contains(report.Html, "<strong>world</strong>", "The description as HTML");
        StringAssert.Contains(report.Html, "href=\"#knt-ch-1\"", "Contents with links");
        AssertInOrder(report.Html, "Chapter 1", $"{Word} C", "Chapter 2", $"{Word} B", "Chapter 3", $"{Word} A");
    }

    [TestMethod]
    public async Task NotesList_OfAFilter_PrintsTheMatchingNotesAndTheCriteria()
    {
        var report = await PostReportAsync("noteslist", new NotesListReportRequestDto
        {
            Source = NotesListReportSource.Filter,
            NotesFilter = new NotesFilterDto { FolderId = _folder.FolderId, Topic = $"{Word} A" }
        });

        Assert.AreEqual(1, report.NotesCount);
        StringAssert.Contains(report.Html, "Filter: Topic = ");
        StringAssert.Contains(report.Html, "Folder = ");
    }

    [TestMethod]
    public async Task NoteDetail_PrintsTheNote_AndANoteThatDoesNotExistIsAnError()
    {
        var note = _notes[1];

        var httpRes = await _httpClient.GetAsync($"api/reports/notes/{note.NoteId}?culture=en-US");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<ReportDto>>();
        Assert.IsTrue(httpRes.IsSuccessStatusCode, res?.ErrorMessage);
        Assert.AreEqual("Note details", res!.Entity!.Title);
        StringAssert.Contains(res.Entity.Html, note.Topic);
        StringAssert.Contains(res.Entity.Html, "<strong>world</strong>");

        var missingRes = await _httpClient.GetAsync($"api/reports/notes/{Guid.NewGuid()}");
        var missing = await missingRes.Content.ReadFromJsonAsync<Result<ReportDto>>();
        Assert.AreEqual(HttpStatusCode.BadRequest, missingRes.StatusCode);
        Assert.IsFalse(missing!.IsValid);
    }

    [TestMethod]
    public async Task AGuest_IsForbidden()
    {
        // Registered after the suite's Admin, so the service makes this user a Guest.
        using var guestClient = _factory.CreateClient();
        var register = await guestClient.PostAsJsonAsync("api/users/register", new UserRegisterDto
        {
            UserName = $"guest-{Guid.NewGuid():N}"[..24],
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = "Guest user",
            Password = "InProcess-Test-Password-1!"
        });
        var token = await register.Content.ReadFromJsonAsync<UserTokenDto>();
        guestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token!.token);

        var list = await guestClient.PostAsJsonAsync("api/reports/noteslist", FolderRequest("Topic", true, true));
        var detail = await guestClient.GetAsync($"api/reports/notes/{_notes[0].NoteId}");

        Assert.AreEqual(HttpStatusCode.Forbidden, list.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, detail.StatusCode);
    }

    private static NotesListReportRequestDto FolderRequest(string orderColumn, bool ascending, bool compact, string? culture = null) => new()
    {
        Source = NotesListReportSource.Folder,
        FolderId = _folder.FolderId,
        OrderColumn = orderColumn,
        OrderAscending = ascending,
        Compact = compact,
        Culture = culture
    };

    private static async Task<ReportDto> PostReportAsync(string report, NotesListReportRequestDto request)
    {
        var httpRes = await _httpClient.PostAsJsonAsync($"api/reports/{report}", request);
        var res = await httpRes.Content.ReadFromJsonAsync<Result<ReportDto>>();
        Assert.IsTrue(httpRes.IsSuccessStatusCode, res?.ErrorMessage);
        return res!.Entity!;
    }

    private static async Task<ReportFileDto> PostCsvAsync(NotesListReportRequestDto request)
    {
        var httpRes = await _httpClient.PostAsJsonAsync("api/reports/noteslistcsv", request);
        var res = await httpRes.Content.ReadFromJsonAsync<Result<ReportFileDto>>();
        Assert.IsTrue(httpRes.IsSuccessStatusCode, res?.ErrorMessage);
        return res!.Entity!;
    }

    private static void AssertInOrder(string text, params string[] parts)
    {
        var position = -1;
        foreach (var part in parts)
        {
            var next = text.IndexOf(part, position + 1, StringComparison.Ordinal);
            Assert.IsTrue(next > position, $"\"{part}\" is not after the previous part");
            position = next;
        }
    }

    private static async Task<NoteDto> CreateNoteAsync(string topic, string description, int priority)
    {
        var note = new NoteDto
        {
            NoteId = Guid.Empty,
            Topic = topic,
            Description = description,
            Priority = priority,
            FolderId = _folder.FolderId,
            // See NotesInProcessTests.CreateNoteAsync: MVC validates the nested folder too.
            FolderDto = _folder,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        };

        var httpRes = await _httpClient.PostAsJsonAsync("api/notes", note);
        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>();
        Assert.IsTrue(httpRes.IsSuccessStatusCode, res?.ErrorMessage);
        return res!.Entity!;
    }
}
