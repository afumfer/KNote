using KNote.ClientWin.Core;
using KNote.ClientWin.Core.Reports;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

[TestClass]
public class NoteDetailReportTests
{
    private static NoteExtendedDto Note()
    {
        var note = new NoteExtendedDto
        {
            NoteId = Guid.NewGuid(),
            NoteNumber = 6056,
            Topic = "Escritorio <remoto>",
            Tags = "#EX",
            InternalTags = "Alarms Pending",
            Priority = 3,
            Description = "Body",
            Script = "SECRET_NOTE_SCRIPT",
            FolderId = Guid.NewGuid(),
            FolderDto = new FolderDto { FolderNumber = 88, Name = "Accesos" },
            NoteTypeDto = new NoteTypeDto { Name = "Meeting" },
            CreationDateTime = new DateTime(2016, 5, 26, 9, 48, 0),
            ModificationDateTime = new DateTime(2025, 2, 5, 13, 3, 0),
            KAttributesDto = new List<NoteKAttributeDto>
            {
                new() { Name = "Owner", Value = "afumfer", Order = 2, Script = "SECRET_ATTRIBUTE_SCRIPT" },
                new() { Name = "Area", Value = "", Order = 1 }
            }
        };
        note.Messages.Add(new KMessageDto { Comment = "SECRET_ALARM_COMMENT" });
        return note;
    }

    private static NoteDetailReportData Data(NoteExtendedDto note = null) => new()
    {
        Note = note ?? Note(),
        FolderPath = @"- Accesos rápidos\Web",
        Repository = "Personal (Microsoft.Data.Sqlite)",
        DescriptionHtml = "<p>Rendered <b>body</b></p>"
    };

    [TestMethod]
    public void Build_IsAPortraitReportHeadedByTheTopicAndFolder()
    {
        var report = NoteDetailReport.Build(Data(), new DateTime(2026, 10, 2));

        Assert.AreEqual(ReportOrientation.Portrait, report.Orientation);
        Assert.AreEqual("Note details", report.Title);
        Assert.AreEqual("Escritorio <remoto>", report.Heading);              // encoded later by ReportHtml.Render
        Assert.AreEqual(@"- Accesos rápidos\Web", report.Subheading);
        Assert.AreEqual("Note 6056 - Escritorio <remoto>", report.FileNameBase);
        CollectionAssert.AreEqual(new[] { "#6056", "#88", "Personal (Microsoft.Data.Sqlite)" },
            report.Meta.Take(3).Select(m => m.Value).ToArray());
    }

    [TestMethod]
    public void Build_NeverIncludesScriptsOrAlarms()
    {
        var html = ReportHtml.Render(NoteDetailReport.Build(Data(), DateTime.Now));

        Assert.IsFalse(html.Contains("SECRET_NOTE_SCRIPT"));
        Assert.IsFalse(html.Contains("SECRET_ATTRIBUTE_SCRIPT"));
        Assert.IsFalse(html.Contains("SECRET_ALARM_COMMENT"));
    }

    [TestMethod]
    public void Build_HasEverySectionWithItsContent()
    {
        var body = NoteDetailReport.Build(Data(), DateTime.Now).BodyHtml;

        foreach (var section in new[] { ">Properties<", ">Description<", ">Attributes<", ">Resources<", ">Tasks<", ">Trace notes<" })
            StringAssert.Contains(body, section);

        StringAssert.Contains(body, "<p>Rendered <b>body</b></p>");              // description HTML as is
        StringAssert.Contains(body, "Meeting");
        StringAssert.Contains(body, "Alarms Pending");
        Assert.IsTrue(body.IndexOf("Area", StringComparison.Ordinal) < body.IndexOf("Owner", StringComparison.Ordinal),
            "Attributes follow their defined Order");
        StringAssert.Contains(body, "No resources.");
        StringAssert.Contains(body, "No tasks.");
    }

    [TestMethod]
    public void Build_UnsavedChangesAndNewNotes_AreFlagged()
    {
        var unsaved = Data();
        unsaved.UnsavedChanges = true;
        StringAssert.Contains(NoteDetailReport.Build(unsaved, DateTime.Now).BodyHtml, "Unsaved changes");
        Assert.IsFalse(NoteDetailReport.Build(Data(), DateTime.Now).BodyHtml.Contains("Unsaved changes"));

        var newNote = Note();
        newNote.NoteId = Guid.Empty;
        var report = NoteDetailReport.Build(Data(newNote), DateTime.Now);
        StringAssert.Contains(report.BodyHtml, "New note");
        Assert.AreEqual("(new)", report.Meta[0].Value);
    }

    [TestMethod]
    public void Build_WebPageDescription_IsALink()
    {
        var data = Data();
        data.DescriptionHtml = null;
        data.DescriptionUrl = "https://example.org/a?b=1&c=2";

        var body = NoteDetailReport.Build(data, DateTime.Now).BodyHtml;

        StringAssert.Contains(body, "<a href=\"https://example.org/a?b=1&amp;c=2\">");
    }

    [TestMethod]
    public void Build_Resources_ImagesGetAThumbnailOthersTheirFileKind()
    {
        var data = Data();
        data.Resources.Add(new NoteDetailResource(1, "photo.png", "image/png", "A photo", "https://knote.resources/R/photo.png"));
        data.Resources.Add(new NoteDetailResource(2, "manual.pdf", "application/pdf", null, null));

        var body = NoteDetailReport.Build(data, DateTime.Now).BodyHtml;

        StringAssert.Contains(body, "<img src=\"https://knote.resources/R/photo.png\" alt=\"photo.png\">");
        StringAssert.Contains(body, "<span>PDF</span>");
    }

    [TestMethod]
    public void Build_Tasks_ShowStatusResponsibleDatesAndEffort()
    {
        var data = Data();
        data.Tasks.Add(new NoteDetailTask(new NoteTaskDto { Resolved = true, Priority = 2, Tags = "Deploy", UserFullName = "Ana", StartDate = new DateTime(2026, 1, 2, 8, 0, 0), EstimatedTime = 1.5 }, "<p>steps</p>"));
        data.Tasks.Add(new NoteDetailTask(new NoteTaskDto { Resolved = false }, null));

        var body = NoteDetailReport.Build(data, DateTime.Now).BodyHtml;

        StringAssert.Contains(body, "Resolved</span>");
        StringAssert.Contains(body, "Pending</span>");
        StringAssert.Contains(body, "<p>steps</p>");
        StringAssert.Contains(body, "Ana");
        StringAssert.Contains(body, new DateTime(2026, 1, 2, 8, 0, 0).ToString("g"));
        StringAssert.Contains(body, 1.5.ToString("0.##"));
    }

    [TestMethod]
    public void Build_TraceNotes_ListBothSides()
    {
        var data = Data();
        data.TraceNotesFrom.Add(new TraceNoteRow(Guid.NewGuid(), "#10", "Origin", "a", "Depends on", 1, 0.5));
        data.TraceNotesTo.Add(new TraceNoteRow(Guid.NewGuid(), "#20", "Target", "b", "", 2, 1));

        var body = NoteDetailReport.Build(data, DateTime.Now).BodyHtml;

        StringAssert.Contains(body, "Trace notes from");
        StringAssert.Contains(body, "Trace notes to");
        StringAssert.Contains(body, "Origin");
        StringAssert.Contains(body, "Depends on");
        StringAssert.Contains(body, "Target");
    }
}

/// <summary>
/// NoteDetailReportData.CreateAsync against the hand-made service fakes: folder path, description and
/// task rendering, resource thumbnails and trace notes resolution.
/// </summary>
[TestClass]
public class NoteDetailReportDataTests
{
    private static (Store store, FakeKntService service, KNote.Service.Core.ServiceRef serviceRef) Setup()
    {
        var store = new Store(new TestFactoryViews()) { AppUserName = "jdoe" };
        var service = new FakeKntService();
        var serviceRef = TestServiceRefFactory.CreateWithFakeService(service);

        var rootId = Guid.NewGuid();
        service.FoldersFake.GetByIdAsyncImpl = id => Task.FromResult(new Result<FolderDto>
        {
            Entity = id == rootId ? new FolderDto { Name = "Root" } : new FolderDto { Name = "Web", ParentId = rootId }
        });
        service.NotesFake.UtilMarkdownToHtmlImpl = md => $"<p>{md}</p>";
        service.NotesFake.UtilGetResourceFilePathImpl = r => "";
        service.TraceNoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<TraceNoteTypeDto>> { Entity = new() });

        return (store, service, serviceRef);
    }

    private static NoteExtendedDto Note(string description, string contentType = "markdown")
    {
        var note = new NoteExtendedDto { NoteId = Guid.NewGuid(), Topic = "T", Description = description, FolderId = Guid.NewGuid() };
        note.SetContentTypeExt(new ContentTypeExt { ForDescription = contentType });
        return note;
    }

    [TestMethod]
    public async Task CreateAsync_ResolvesFolderPathAndRendersMarkdown()
    {
        var (store, _, serviceRef) = Setup();

        var data = await NoteDetailReportData.CreateAsync(store, serviceRef, Note("Body"), unsavedChanges: true);

        Assert.AreEqual(@"Root\Web", data.FolderPath);
        Assert.AreEqual("<p>Body</p>", data.DescriptionHtml);
        Assert.IsTrue(data.UnsavedChanges);
    }

    [TestMethod]
    public async Task CreateAsync_HtmlIsKeptAndWebPagesBecomeALink()
    {
        var (store, _, serviceRef) = Setup();

        var html = await NoteDetailReportData.CreateAsync(store, serviceRef, Note("<b>x</b>", "html"), false);
        var web = await NoteDetailReportData.CreateAsync(store, serviceRef, Note("https://example.org", "navigation"), false);
        var empty = await NoteDetailReportData.CreateAsync(store, serviceRef, Note("  "), false);

        Assert.AreEqual("<b>x</b>", html.DescriptionHtml);
        Assert.AreEqual("https://example.org", web.DescriptionUrl);
        Assert.IsNull(web.DescriptionHtml);
        Assert.IsNull(empty.DescriptionHtml);
    }

    [TestMethod]
    public async Task CreateAsync_ResourcesInTheRepositoryFolder_AreServedThroughTheVirtualHost()
    {
        var (store, service, serviceRef) = Setup();
        var root = Path.Combine(Path.GetTempPath(), "knt-report-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "R", "2026"));
        var file = Path.Combine(root, "R", "2026", "my photo.png");
        File.WriteAllBytes(file, new byte[] { 1, 2, 3 });
        try
        {
            serviceRef.RepositoryRef.ResourcesContainerRootPath = root;
            serviceRef.RepositoryRef.ResourcesContainer = "R";
            service.NotesFake.UtilGetResourceFilePathImpl = r => r.Name == "my photo.png" ? file : "";

            var note = Note("![img](R/2026/a.png)");
            note.Resources.Add(new ResourceDto { Name = "my photo.png", FileType = "image/png", Order = 2 });
            note.Resources.Add(new ResourceDto { Name = "mem.png", FileType = "image/png", Order = 1, ContentArrayBytes = new byte[] { 9 } });
            note.Resources.Add(new ResourceDto { Name = "doc.pdf", FileType = "application/pdf", Order = 3 });

            var data = await NoteDetailReportData.CreateAsync(store, serviceRef, note, false);

            CollectionAssert.AreEqual(new[] { "mem.png", "my photo.png", "doc.pdf" }, data.Resources.Select(r => r.Name).ToArray());
            Assert.AreEqual("data:image/png;base64,CQ==", data.Resources[0].ThumbnailSrc);
            Assert.AreEqual("https://knote.resources/R/2026/my%20photo.png", data.Resources[1].ThumbnailSrc);
            Assert.IsNull(data.Resources[2].ThumbnailSrc);
            Assert.AreEqual("<p>![img](https://knote.resources/R/2026/a.png)</p>", data.DescriptionHtml);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task CreateAsync_TasksAndTraceNotes_AreResolved()
    {
        var (store, service, serviceRef) = Setup();
        var typeId = Guid.NewGuid();
        var related = Guid.NewGuid();
        service.TraceNoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<TraceNoteTypeDto>>
        {
            Entity = new() { new TraceNoteTypeDto { TraceNoteTypeId = typeId, Name = "Depends on" } }
        });
        service.NotesFake.GetByIdAsyncImpl = id => Task.FromResult(new Result<NoteDto>
        {
            Entity = id == related ? new NoteDto { NoteNumber = 42, Topic = "Related" } : null
        });

        var note = Note("");
        note.Tasks.Add(new NoteTaskDto { Description = "do it" });
        note.TraceNotesFrom = new() { new TraceNoteDto { FromId = related, ToId = note.NoteId, TraceNoteTypeId = typeId } };
        note.TraceNotesTo = new() { new TraceNoteDto { FromId = note.NoteId, ToId = Guid.NewGuid() } };

        var data = await NoteDetailReportData.CreateAsync(store, serviceRef, note, false);

        Assert.AreEqual("<p>do it</p>", data.Tasks.Single().DescriptionHtml);
        Assert.AreEqual("#42", data.TraceNotesFrom.Single().Number);
        Assert.AreEqual("Related", data.TraceNotesFrom.Single().Topic);
        Assert.AreEqual("Depends on", data.TraceNotesFrom.Single().Type);
        Assert.AreEqual("?", data.TraceNotesTo.Single().Number, "A related note that no longer exists");
        Assert.AreEqual("", data.TraceNotesTo.Single().Type);
    }
}
