using KNote.Service.Core;
using KntWebView;
using Markdig;

namespace KNote.ClientWin.Tests;

[TestClass]
public class KntDiagramsTests
{
    // A note's description as the note views show it (Service.Notes.UtilMarkdownToHtml).
    private static string NoteHtml(string markdown) =>
        KntHtmlSanitizer.Sanitize(Markdown.ToHtml(markdown, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build()));

    [TestMethod]
    public void HasDiagrams_NoteWithAMermaidBlock_IsTrue()
    {
        var html = NoteHtml("Text\n\n```mermaid\nflowchart LR\n    A[\"x<br/>y\"] -- \"label\" --> B[(\"db\")]\n```\n");

        Assert.IsTrue(KntDiagrams.HasDiagrams(html));
    }

    [TestMethod]
    public void HasDiagrams_NoteWithoutMermaidBlocks_IsFalse()
    {
        var html = NoteHtml("A note about mermaid diagrams.\n\n```csharp\nvar mermaid = 1;\n```\n\n    flowchart LR\n");

        Assert.IsFalse(KntDiagrams.HasDiagrams(html));
        Assert.IsFalse(KntDiagrams.HasDiagrams(""));
        Assert.IsFalse(KntDiagrams.HasDiagrams(null));
    }

    [TestMethod]
    public void Page_StartsWithThePolicy_AndEndsWithTheDiagramsScripts()
    {
        var html = NoteHtml("```mermaid\nflowchart LR\n    A --> B\n```\n");

        var page = KntDiagrams.Page(html);

        Assert.IsTrue(page.StartsWith("<meta http-equiv=\"Content-Security-Policy\" content=\"script-src https://knote.diagrams/; object-src 'none'; base-uri 'none'\">", StringComparison.Ordinal));
        var note = page.IndexOf(html, StringComparison.Ordinal);
        var library = page.IndexOf("<script src=\"https://knote.diagrams/mermaid.min.js\"></script>", StringComparison.Ordinal);
        var helper = page.IndexOf("<script src=\"https://knote.diagrams/kntMermaid.js\" data-knt-render></script>", StringComparison.Ordinal);
        Assert.IsTrue(note > 0 && library > note && helper > library);
        Assert.IsTrue(page.EndsWith("</script>", StringComparison.Ordinal));
    }
}
