using KNote.ClientWin.Core.Reports;

namespace KNote.ClientWin.Tests;

[TestClass]
public class NotesListCsvTests
{
    private static NotesListSnapshot Snapshot(params string[][] rows)
    {
        var snapshot = new NotesListSnapshot();
        snapshot.Columns.Add(new NotesListColumn("NoteNumber", "Number", true, 80));
        snapshot.Columns.Add(new NotesListColumn("Topic", "Topic", false, 400));
        snapshot.Columns.Add(new NotesListColumn("ModificationDateTime", "Modification date", true, 120));
        snapshot.Rows.AddRange(rows);
        return snapshot;
    }

    [TestMethod]
    public void Build_HeaderThenRowsInOrder_WithTheGivenSeparatorAndCrLf()
    {
        var csv = NotesListCsv.Build(Snapshot(
            new[] { "10", "Alpha", "01/02/2026 9:30" },
            new[] { "20", "Beta", "02/02/2026 9:30" }), ";");

        Assert.AreEqual(
            "Number;Topic;Modification date\r\n" +
            "10;Alpha;01/02/2026 9:30\r\n" +
            "20;Beta;02/02/2026 9:30\r\n", csv);
    }

    [TestMethod]
    public void Build_WithoutSeparator_UsesTheRegionalListSeparator()
    {
        var csv = NotesListCsv.Build(Snapshot(new[] { "1", "A", "d" }));

        StringAssert.StartsWith(csv, $"Number{NotesListCsv.DefaultSeparator}Topic");
    }

    [TestMethod]
    [DataRow("a;b", "\"a;b\"")]
    [DataRow("say \"hi\"", "\"say \"\"hi\"\"\"")]
    [DataRow("line1\r\nline2", "\"line1\r\nline2\"")]
    [DataRow(" padded ", "\" padded \"")]
    [DataRow("a,b", "a,b")]                // not the separator in use: no quotes needed
    [DataRow("", "")]
    public void Field_QuotesOnlyWhenNeeded(string value, string expected)
    {
        Assert.AreEqual(expected, NotesListCsv.Field(value, ";"));
    }

    [TestMethod]
    [DataRow("- Accesos rápidos", "'- Accesos rápidos")]
    [DataRow("=1+1", "'=1+1")]
    [DataRow("+34 600", "'+34 600")]
    [DataRow("@SUM(A1)", "'@SUM(A1)")]
    [DataRow("-5", "-5")]                  // a plain number stays a number
    [DataRow("Alpha - beta", "Alpha - beta")]
    public void Field_TextThatSpreadsheetsWouldRunAsAFormula_IsNeutralized(string value, string expected)
    {
        Assert.AreEqual(expected, NotesListCsv.Field(value, ";"));
    }

    [TestMethod]
    public void FileEncoding_WritesUtf8Bom()
    {
        CollectionAssert.AreEqual(new byte[] { 0xEF, 0xBB, 0xBF }, NotesListCsv.FileEncoding.GetPreamble());
    }

    [TestMethod]
    public void FileName_FromContext_IsSanitized()
    {
        var context = new NotesListContext { Source = NotesListSource.Filter, FilterCriteria = { new ReportMetaItem("Tags", "a/b:c") } };

        var name = ReportFileName.Sanitize(context.FileNameBase(new DateTime(2026, 10, 2))) + ".csv";

        Assert.AreEqual("Notes - Filter Tags = a-b_c - 2026-10-02.csv", name);
    }

    [TestMethod]
    public void InitialFolder_MissingFolder_FallsBackToDocuments()
    {
        Assert.AreEqual(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ReportFileName.InitialFolder(null));
        Assert.AreEqual(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ReportFileName.InitialFolder(@"Z:\does\not\exist"));
        Assert.AreEqual(Path.GetTempPath(), ReportFileName.InitialFolder(Path.GetTempPath()));
    }
}
