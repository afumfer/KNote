using KNote.ClientWin.Core.Reports;

namespace KNote.ClientWin.Tests;

[TestClass]
public class NotesListReportTests
{
    private static NotesListSnapshot Snapshot(string textFilter = "", int loaded = 2)
    {
        var snapshot = new NotesListSnapshot { TextFilter = textFilter, LoadedCount = loaded };
        snapshot.Columns.Add(new NotesListColumn("NoteNumber", "Number", true, 80));
        snapshot.Columns.Add(new NotesListColumn("Topic", "Topic", false, 400));
        snapshot.Columns.Add(new NotesListColumn("ModificationDateTime", "Modification date", true, 120));
        snapshot.Rows.Add(new[] { "10", "Alpha <b>bold</b>", "01/02/2026 9:30" });
        snapshot.Rows.Add(new[] { "20", "Beta & co", "02/02/2026 9:30" });
        return snapshot;
    }

    private static readonly NotesListContext FolderContext = new()
    {
        Source = NotesListSource.Folder,
        FolderPath = @"Notes\Projects",
        FolderNumber = 3,
        RepositoryAlias = "Demo",
        RepositoryProvider = "Microsoft.Data.Sqlite"
    };

    [TestMethod]
    public void Build_IsALandscapeNotesListHeadedByTheContext()
    {
        var report = NotesListReport.Build(Snapshot(), FolderContext, new DateTime(2026, 10, 2, 10, 0, 0));

        Assert.AreEqual(ReportOrientation.Landscape, report.Orientation);
        Assert.AreEqual("Notes list", report.Title);
        Assert.AreEqual(@"Notes\Projects", report.Heading);
        Assert.AreEqual("Folder number 3", report.Subheading);
        Assert.AreEqual(@"Notes - Notes\Projects - 2026-10-02", report.FileNameBase);
        CollectionAssert.AreEqual(new[] { "Repository", "Notes", "Generated" }, report.Meta.Select(m => m.Label).ToArray());
        Assert.AreEqual("Demo (Microsoft.Data.Sqlite)", report.Meta[0].Value);
        Assert.AreEqual("2", report.Meta[1].Value);
    }

    [TestMethod]
    public void Build_WithTextFilter_CountsShownOfLoaded()
    {
        var report = NotesListReport.Build(Snapshot(textFilter: "a", loaded: 7), FolderContext, DateTime.Now);

        Assert.AreEqual("2 of 7", report.Meta.Single(m => m.Label == "Notes").Value);
    }

    [TestMethod]
    public void Build_Body_HasHeadersAndRowsInOrder_Encoded()
    {
        var body = NotesListReport.Build(Snapshot(), FolderContext, DateTime.Now).BodyHtml;

        StringAssert.Contains(body, "<th class=\"knt-right\">Number</th><th>Topic</th><th class=\"knt-right\">Modification date</th>");
        StringAssert.Contains(body, "Alpha &lt;b&gt;bold&lt;/b&gt;");
        StringAssert.Contains(body, "Beta &amp; co");
        Assert.IsTrue(body.IndexOf("Alpha", StringComparison.Ordinal) < body.IndexOf("Beta", StringComparison.Ordinal));
        StringAssert.Contains(body, "2 notes");
    }

    [TestMethod]
    public void Build_EmptyList_SaysSo()
    {
        var snapshot = new NotesListSnapshot();

        var body = NotesListReport.Build(snapshot, FolderContext, DateTime.Now).BodyHtml;

        StringAssert.Contains(body, "There are no notes in this list.");
        Assert.IsFalse(body.Contains("<table"));
    }

    [TestMethod]
    public void ColumnPercents_FollowScreenProportionsAndAddUpTo100()
    {
        var percents = NotesListReport.ColumnPercents(Snapshot().Columns);

        Assert.AreEqual(100, percents.Sum(), 0.001);
        Assert.AreEqual(100.0 * 400 / 600, percents[1], 0.001);
    }

    [TestMethod]
    public void ColumnPercents_NarrowColumnsGetTheirMinimum_TakenFromTopic()
    {
        // A very wide Topic (wide monitor) would leave Number/Priority/dates too narrow for paper.
        var columns = new List<NotesListColumn>
        {
            new("NoteNumber", "Number", true, 80),
            new("Topic", "Topic", false, 3000),
            new("Priority", "Priority", true, 70),
            new("ModificationDateTime", "Modification date", true, 130)
        };

        var percents = NotesListReport.ColumnPercents(columns);

        Assert.AreEqual(100, percents.Sum(), 0.001);
        Assert.IsTrue(percents[0] >= 5.5 - 0.001);
        Assert.IsTrue(percents[2] >= 5.5 - 0.001);
        Assert.IsTrue(percents[3] >= 10.5 - 0.001);
        Assert.IsTrue(percents[1] < 100.0 * 3000 / 3280, "Topic gives up the room the others needed");
    }

    [TestMethod]
    public void ColumnPercents_NoColumns_IsEmpty()
    {
        Assert.AreEqual(0, NotesListReport.ColumnPercents(new List<NotesListColumn>()).Length);
    }
}
