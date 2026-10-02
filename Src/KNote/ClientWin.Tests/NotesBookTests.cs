using System.Text;
using KNote.ClientWin.Core;
using KNote.ClientWin.Core.Reports;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

[TestClass]
public class NotesBookTests
{
    private static readonly NotesListContext FolderContext = new()
    {
        Source = NotesListSource.Folder,
        FolderPath = @"Notas personales\Libro KaNote",
        FolderNumber = 12,
        RepositoryAlias = "Personal",
        RepositoryProvider = "Microsoft.Data.Sqlite"
    };

    private static List<BookChapter> Chapters() => new()
    {
        new BookChapter(1, "Intro <one>", "<p>First text</p>", null),
        new BookChapter(2, "Web", null, "https://example.org/?a=1&b=2"),
        new BookChapter(3, "", null, null)
    };

    [TestMethod]
    public void Build_IsABookWithPageReferencesToResolve()
    {
        var report = NotesBook.Build(FolderContext, Chapters(), new DateTime(2026, 10, 2), "es");

        Assert.AreEqual(ReportLayout.Book, report.Layout);
        Assert.AreEqual(ReportOrientation.Portrait, report.Orientation);
        Assert.AreEqual("es", report.Language);
        Assert.IsTrue(report.ResolvePageReferences);
        Assert.AreEqual("Libro KaNote", report.Heading);
        Assert.AreEqual(@"Book - Notas personales\Libro KaNote - 2026-10-02", report.FileNameBase);
    }

    [TestMethod]
    public void Build_CoverContentsAndChaptersInOrder()
    {
        var body = NotesBook.Build(FolderContext, Chapters(), DateTime.Now).BodyHtml;

        var cover = body.IndexOf("class=\"knt-cover\"", StringComparison.Ordinal);
        var contents = body.IndexOf("class=\"knt-contents\"", StringComparison.Ordinal);
        var chapter1 = body.IndexOf("id=\"knt-ch-1\"", StringComparison.Ordinal);
        var chapter3 = body.IndexOf("id=\"knt-ch-3\"", StringComparison.Ordinal);
        Assert.IsTrue(cover >= 0 && cover < contents && contents < chapter1 && chapter1 < chapter3);

        // Contents: one linked entry per chapter, each with a page reference to fill in.
        StringAssert.Contains(body, "<a class=\"knt-toc-entry\" href=\"#knt-ch-1\">");
        StringAssert.Contains(body, ReportHtml.PageReference("knt-ch-3"));
        StringAssert.Contains(body, "3 chapters");
        StringAssert.Contains(body, @"Notas personales\Libro KaNote");          // cover subtitle: full path
    }

    [TestMethod]
    public void Build_ChapterTexts()
    {
        var body = NotesBook.Build(FolderContext, Chapters(), DateTime.Now).BodyHtml;

        StringAssert.Contains(body, "Intro &lt;one&gt;");
        StringAssert.Contains(body, "<div class=\"knt-text\"><p>First text</p></div>");
        StringAssert.Contains(body, "<a href=\"https://example.org/?a=1&amp;b=2\">");
        StringAssert.Contains(body, "This note has no text.");
        StringAssert.Contains(body, "(No topic)");
    }

    [TestMethod]
    public void CoverTitles_SearchOrFilter_UseTheContextTexts()
    {
        var search = new NotesListContext { Source = NotesListSource.Search, SearchText = "kanote", SearchInDescription = true };

        Assert.AreEqual(("Search: kanote", "Searched in: topic, description"), NotesBook.CoverTitles(search));
    }

    [TestMethod]
    public void CoverTitles_RootFolderWithTextFilter()
    {
        var context = new NotesListContext { Source = NotesListSource.Folder, FolderPath = "Notas", TextFilter = "abc" };

        Assert.AreEqual(("Notas", "List text filter: abc"), NotesBook.CoverTitles(context));
    }

    [TestMethod]
    public void Render_Book_UsesTheBookStyleAndLanguage()
    {
        var html = ReportHtml.Render(NotesBook.Build(FolderContext, Chapters(), DateTime.Now, "es"));

        StringAssert.Contains(html, "<html lang=\"es\">");
        StringAssert.Contains(html, ".knt-toc-entry");                          // KNoteBook.css embedded
        StringAssert.Contains(html, "@page front");
        StringAssert.Contains(html, "<title>Libro KaNote</title>");
        Assert.IsFalse(html.Contains("knt-header"), "No report header band in a book");
    }

    [TestMethod]
    public void FillPageReferences_FillsKnownTargetsOnly()
    {
        var html = $"a {ReportHtml.PageReference("knt-ch-1")} b {ReportHtml.PageReference("knt-ch-2")}";

        var filled = ReportHtml.FillPageReferences(html, new Dictionary<string, int> { ["knt-ch-1"] = 3 });

        StringAssert.Contains(filled, "<span class=\"knt-pageref\" data-ref=\"knt-ch-1\">3</span>");
        StringAssert.Contains(filled, ReportHtml.PageReference("knt-ch-2"));
        Assert.AreEqual(html, ReportHtml.FillPageReferences(html, new Dictionary<string, int>()));
    }

    [TestMethod]
    public async Task LoadChaptersAsync_KeepsTheListOrderAndSkipsMissingNotes()
    {
        var store = new Store(new TestFactoryViews()) { AppUserName = "jdoe" };
        var service = new FakeKntService();
        var serviceRef = TestServiceRefFactory.CreateWithFakeService(service);
        service.NotesFake.UtilMarkdownToHtmlImpl = md => $"<p>{md}</p>";

        var a = Guid.NewGuid();
        var deleted = Guid.NewGuid();
        var b = Guid.NewGuid();
        var notes = new Dictionary<Guid, NoteDto>
        {
            [a] = new() { NoteId = a, Topic = "A", Description = "text a" },
            [b] = new() { NoteId = b, Topic = "B", Description = "" }
        };
        service.NotesFake.GetByIdAsyncImpl = id => Task.FromResult(new Result<NoteDto> { Entity = notes.GetValueOrDefault(id) });

        var chapters = await NotesBook.LoadChaptersAsync(store, serviceRef, new[] { b, deleted, a });

        CollectionAssert.AreEqual(new[] { "B", "A" }, chapters.Select(c => c.Title).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2 }, chapters.Select(c => c.Number).ToArray());
        Assert.IsNull(chapters[0].ContentHtml);
        Assert.AreEqual("<p>text a</p>", chapters[1].ContentHtml);
    }
}

[TestClass]
public class PdfNamedDestinationsTests
{
    // Same shape as the PDFs Chromium/Skia writes: catalog -> /Dests and a nested /Pages tree.
    private const string SamplePdf = @"%PDF-1.4
1 0 obj
<</Type /Page
/Parent 10 0 R
/Contents 2 0 R>>
endobj
2 0 obj
<</Filter /FlateDecode
/Length 5>> stream
x endobj 99 0 obj garbage
endstream
endobj
3 0 obj
<</Type /Page
/Parent 10 0 R>>
endobj
4 0 obj
<</Type /Page
/Parent 11 0 R>>
endobj
5 0 obj
<</Type /Page
/Parent 11 0 R>>
endobj
10 0 obj
<</Type /Pages
/Count 2
/Kids [1 0 R 3 0 R]
/Parent 12 0 R>>
endobj
11 0 obj
<</Type /Pages
/Count 2
/Kids [4 0 R 5 0 R]
/Parent 12 0 R>>
endobj
12 0 obj
<</Type /Pages
/Count 4
/Kids [10 0 R 11 0 R]>>
endobj
13 0 obj
<</knt-ch-1 [3 0 R /XYZ 0 831.42 0]
/knt-ch-2 [5 0 R /XYZ 0 831.42 0]
/knt#2Dch#2D3 [4 0 R /XYZ 0 831.42 0]>>
endobj
14 0 obj
<</Type /Catalog
/Pages 12 0 R
/Dests 13 0 R>>
endobj
trailer
<</Root 14 0 R>>
%%EOF";

    [TestMethod]
    public void Parse_MapsEachDestinationToItsPageNumber()
    {
        var pages = PdfNamedDestinations.Parse(Encoding.Latin1.GetBytes(SamplePdf));

        Assert.AreEqual(3, pages.Count);
        Assert.AreEqual(2, pages["knt-ch-1"]);
        Assert.AreEqual(4, pages["knt-ch-2"]);
        Assert.AreEqual(3, pages["knt-ch-3"]);                                  // "#2D" escapes decoded
    }

    [TestMethod]
    public void Parse_NotAChromiumPdf_IsEmpty()
    {
        Assert.AreEqual(0, PdfNamedDestinations.Parse(null).Count);
        Assert.AreEqual(0, PdfNamedDestinations.Parse(Array.Empty<byte>()).Count);
        Assert.AreEqual(0, PdfNamedDestinations.Parse(Encoding.ASCII.GetBytes("not a pdf")).Count);
        Assert.AreEqual(0, PdfNamedDestinations.Parse(Encoding.Latin1.GetBytes(SamplePdf.Replace("/Dests 13 0 R", ""))).Count);
    }
}
