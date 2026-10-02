using KNote.ClientWin.Core.Reports;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ReportHtmlTests
{
    [TestMethod]
    public void Render_IncludesCommonStyleAndPageSetupForTheOrientation()
    {
        var landscape = ReportHtml.Render(new ReportDocument { Title = "Notes list", Orientation = ReportOrientation.Landscape });
        var portrait = ReportHtml.Render(new ReportDocument { Title = "Note details", Orientation = ReportOrientation.Portrait });

        StringAssert.Contains(landscape, "table.knt-grid");              // embedded stylesheet loaded
        StringAssert.Contains(landscape, "size: A4 landscape");
        StringAssert.Contains(landscape, "<body class=\"knt-landscape\">");
        StringAssert.Contains(portrait, "size: A4 portrait");
        StringAssert.Contains(portrait, "counter(pages)");
    }

    [TestMethod]
    public void Render_HeaderTexts_AreHtmlEncoded()
    {
        var html = ReportHtml.Render(new ReportDocument
        {
            Title = "Notes list",
            Heading = "<script>alert('x')</script>",
            Subheading = "a & b",
            Meta = { new ReportMetaItem("Repo <1>", "\"quoted\"") }
        });

        Assert.IsFalse(html.Contains("<script>"), "Heading must not be emitted as markup");
        StringAssert.Contains(html, "&lt;script&gt;");
        StringAssert.Contains(html, "a &amp; b");
        StringAssert.Contains(html, "Repo &lt;1&gt;");
    }

    [TestMethod]
    public void Render_BodyHtml_IsEmittedAsIs()
    {
        var html = ReportHtml.Render(new ReportDocument { Title = "T", BodyHtml = "<table class=\"knt-grid\"><tr><td>1</td></tr></table>" });

        StringAssert.Contains(html, "<table class=\"knt-grid\"><tr><td>1</td></tr></table>");
    }

    [TestMethod]
    public void Render_RunningHeader_IsAValidCssStringEvenWithQuotesBackslashesAndNewLines()
    {
        var html = ReportHtml.Render(new ReportDocument { Title = "Notes list", Heading = "\\Root\\\"Quoted\"\r\nfolder" });

        StringAssert.Contains(html, "content: \"Notes list · \\\\Root\\\\\\\"Quoted\\\"  folder\";");
    }

    [TestMethod]
    public void Render_RunningHeader_CannotCloseTheStyleElement()
    {
        var html = ReportHtml.Render(new ReportDocument { Title = "Notes list", Heading = "</style><b>x</b>" });

        Assert.AreEqual(1, html.Split("</style>").Length - 1, "Only the report's own </style> may appear");
    }

    [TestMethod]
    public void CssString_EscapesQuotesBackslashesAndAngleBrackets()
    {
        Assert.AreEqual("\"a\\\"b\\\\c d\"", ReportHtml.CssString("a\"b\\c\nd"));
        Assert.AreEqual("\"\\3C /style\\3E \"", ReportHtml.CssString("</style>"));
        Assert.AreEqual("\"\"", ReportHtml.CssString(null));
    }
}

[TestClass]
public class ReportFileNameTests
{
    [TestMethod]
    public void Sanitize_FolderPath_UsesDashesForSeparators()
    {
        Assert.AreEqual("Mis notas-Proyectos-KaNote", ReportFileName.Sanitize(@"\Mis notas\Proyectos\KaNote"));
        Assert.AreEqual("a-b", ReportFileName.Sanitize("a / b".Replace(" ", "")));
    }

    [TestMethod]
    public void Sanitize_InvalidCharacters_AreReplaced()
    {
        Assert.AreEqual("Topic=a_b_ Tags=x_y", ReportFileName.Sanitize("Topic=a:b? Tags=x*y"));
    }

    [TestMethod]
    public void Sanitize_CollapsesWhitespaceAndTrimsTrailingDots()
    {
        Assert.AreEqual("Notes list", ReportFileName.Sanitize("  Notes \t  list ... "));
    }

    [TestMethod]
    public void Sanitize_EmptyOrOnlyInvalid_ReturnsFallback()
    {
        Assert.AreEqual("KNote report", ReportFileName.Sanitize(null));
        Assert.AreEqual("KNote report", ReportFileName.Sanitize("   "));
        Assert.AreEqual("fallback", ReportFileName.Sanitize("\\/", "fallback"));
    }

    [TestMethod]
    public void Sanitize_LongText_IsTruncated()
    {
        var name = ReportFileName.Sanitize(new string('x', 300));

        Assert.AreEqual(ReportFileName.MaxLength, name.Length);
    }
}
