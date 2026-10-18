using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// IKntNoteService.UtilHtmlToMarkdown / UtilMarkdownToHtml: a note written in ClientWin's HTML editor, converted to
/// Markdown and shown again as HTML, keeps its colors and fonts, whether they were given with &lt;font&gt; (the
/// editor's color button) or with a style (text pasted from Word or a web page).
/// </summary>
[TestClass]
public class NoteHtmlMarkdownTests
{
    private static RepositoryTestDatabase db = null!;
    private static KntService service = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository("EntityFramework"), activateMessageBroker: false);
    }

    [ClassCleanup]
    public static void ClassCleanup() => db.Dispose();

    [TestMethod]
    [DataRow("<P>Normal <FONT color=#c00000>red font</FONT> text</P>", "#c00000", "red font")]
    [DataRow("<P>Normal <SPAN style=\"COLOR: #c00000\">red span</SPAN> text</P>", "color: #c00000", "red span")]
    [DataRow("<P><SPAN style=\"FONT-FAMILY: Courier New; COLOR: red\">code font</SPAN></P>", "Courier New", "color: #ff0000", "code font")]
    [DataRow("<P style=\"COLOR: #0070c0\">blue <STRONG>paragraph</STRONG></P>", "color: #0070c0", "<strong>paragraph</strong>")]
    [DataRow("<H2 style=\"COLOR: #c00000\">Red title</H2>", "<h2", "color: #c00000", "Red title")]
    [DataRow("<UL style=\"COLOR: #7030a0\"><LI>purple item</LI></UL>", "<li", "color: #7030a0", "purple item")]
    [DataRow("<TABLE><TR><TD style=\"BACKGROUND-COLOR: #ffff00\">yellow cell</TD></TR></TABLE>", "background-color: #ffff00", "yellow cell")]
    [DataRow("<P>Normal <FONT style=\"BACKGROUND-COLOR: #ffff00\">highlight</FONT></P>", "background-color: #ffff00", "highlight")]
    public void HtmlToMarkdownToHtml_KeepsColorsAndFonts(string html, params string[] kept)
    {
        var markdown = service.Notes.UtilHtmlToMarkdown(html);
        var shown = service.Notes.UtilMarkdownToHtml(markdown);

        foreach (var part in kept)
            Assert.IsTrue(shown.Contains(part, StringComparison.OrdinalIgnoreCase), $"'{part}' missing in: {shown} (Markdown: {markdown})");
    }

    [TestMethod]
    public void HtmlToMarkdownToHtml_StyledBlockWithInnerBlocks_GivesItsStyleToThem()
    {
        var html = "<DIV style=\"COLOR: #00b050\"><P>first</P><P style=\"FONT-WEIGHT: bold\">second</P></DIV>";

        var shown = service.Notes.UtilMarkdownToHtml(service.Notes.UtilHtmlToMarkdown(html));

        // Both paragraphs green; the second one keeps its own style too.
        Assert.AreEqual(2, shown.Split("#00b050").Length - 1, shown);
        Assert.IsTrue(shown.Contains("font-weight: bold"), shown);
    }

    [TestMethod]
    public void HtmlToMarkdown_WithoutStyles_IsPlainMarkdown()
    {
        var markdown = service.Notes.UtilHtmlToMarkdown("<P>Normal <STRONG>bold</STRONG> and <SPAN>plain span</SPAN></P>");

        Assert.AreEqual("Normal **bold** and plain span", markdown.Trim());
    }

    // A Mermaid diagram (```mermaid): the block both apps draw (kntMermaid.js), with its definition as text.
    private const string MermaidDiagram = "flowchart LR\n    A[\"Desktop<br/>App.sln\"] -- \"by default\" --> B[(\"Database\")]\n    A -. \"uses\" .-> C";

    [TestMethod]
    public void MarkdownToHtml_MermaidBlock_IsADiagramBlockWithItsDefinitionAsText()
    {
        var shown = service.Notes.UtilMarkdownToHtml($"Text\n\n```mermaid\n{MermaidDiagram}\n```\n");

        Assert.IsTrue(shown.Contains("<pre class=\"mermaid\">flowchart LR"), shown);
        Assert.IsTrue(shown.Contains("A[\"Desktop&lt;br/&gt;App.sln\"] -- \"by default\" --&gt; B[(\"Database\")]"), shown);
        Assert.IsTrue(shown.Contains("A -. \"uses\" .-&gt; C"), shown);
    }

    [TestMethod]
    public void MarkdownToHtml_MermaidBlockWithHtml_KeepsItAsText()
    {
        var shown = service.Notes.UtilMarkdownToHtml("```mermaid\nflowchart LR\n    A[\"<img src=x onerror=alert(1)>\"] --> B\n```\n");

        Assert.IsFalse(shown.Contains("<img", StringComparison.OrdinalIgnoreCase), shown);
        Assert.IsTrue(shown.Contains("&lt;img src=x onerror=alert(1)&gt;"), shown);
    }

    [TestMethod]
    public void MarkdownToHtmlToMarkdown_MermaidBlock_KeepsIt()
    {
        var markdown = service.Notes.UtilHtmlToMarkdown(service.Notes.UtilMarkdownToHtml($"```mermaid\n{MermaidDiagram}\n```\n"));

        Assert.AreEqual($"```mermaid\n{MermaidDiagram}\n```", markdown.Trim().ReplaceLineEndings("\n"));
    }
}
