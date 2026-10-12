using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace KNote.Service.Core;

/// <summary>
/// Prepares the HTML of a note to be converted to Markdown (IKntNoteService.UtilHtmlToMarkdown, ReverseMarkdown)
/// without losing its colors and fonts. The converter keeps &lt;font&gt; as it is (an unknown tag for Markdown) but
/// turns &lt;span&gt;, &lt;p&gt;, &lt;div&gt;... into Markdown, dropping their style (e.g. text pasted from Word or a web
/// page in ClientWin's HTML editor). So the style goes to a &lt;font style="..."&gt;: a styled span becomes one, and
/// a styled block keeps it around its content (or passes it to its inner blocks). What the style does to the
/// block itself (alignment, margins...) is still lost: Markdown can't say it.
/// </summary>
internal static class KntHtmlStylesForMarkdown
{
    // Blocks whose content can go inside a <font>.
    private static readonly HashSet<string> TextBlocks = new(StringComparer.OrdinalIgnoreCase)
    {
        "P", "DIV", "LI", "TD", "TH", "H1", "H2", "H3", "H4", "H5", "H6", "BLOCKQUOTE", "DD", "DT"
    };

    // Blocks that hold other blocks: their style goes to the blocks inside.
    private static readonly HashSet<string> Containers = new(StringComparer.OrdinalIgnoreCase)
    {
        "UL", "OL", "DL", "TABLE", "THEAD", "TBODY", "TFOOT", "TR", "SECTION", "ARTICLE", "CENTER"
    };

    public static string Prepare(string html)
    {
        if (string.IsNullOrEmpty(html) || html.IndexOf("style", StringComparison.OrdinalIgnoreCase) < 0)
            return html;

        var document = new HtmlParser().ParseDocument(html);
        var body = document.Body;
        if (body == null)
            return html;

        var changed = false;

        // All the elements, in document order: a block's style reaches its inner blocks before they are seen (they
        // may have had no style of their own).
        foreach (var element in body.QuerySelectorAll("*").ToList())
        {
            var style = element.GetAttribute("style");
            if (string.IsNullOrWhiteSpace(style))
                continue;

            if (element.NodeName == "SPAN")
            {
                Replace(document, element, "font");
                changed = true;
            }
            else if (TextBlocks.Contains(element.NodeName) || Containers.Contains(element.NodeName))
            {
                var innerBlocks = element.Children.Where(IsBlock).ToList();
                if (innerBlocks.Count > 0)
                {
                    // Their own style goes after, so it wins.
                    foreach (var inner in innerBlocks)
                        inner.SetAttribute("style", $"{style.TrimEnd(';', ' ')}; {inner.GetAttribute("style")}".TrimEnd(';', ' '));
                    changed = true;
                }
                else if (TextBlocks.Contains(element.NodeName) && element.HasChildNodes)
                {
                    var font = document.CreateElement("font");
                    font.SetAttribute("style", style);
                    foreach (var child in element.ChildNodes.ToList())
                        font.AppendChild(child);
                    element.AppendChild(font);
                    changed = true;
                }
            }
        }

        return changed ? body.InnerHtml : html;
    }

    private static bool IsBlock(IElement element) =>
        TextBlocks.Contains(element.NodeName) || Containers.Contains(element.NodeName);

    // The element as another tag, with its attributes and content.
    private static void Replace(IDocument document, IElement element, string tagName)
    {
        var replacement = document.CreateElement(tagName);
        foreach (var attribute in element.Attributes)
            replacement.SetAttribute(attribute.Name, attribute.Value);
        foreach (var child in element.ChildNodes.ToList())
            replacement.AppendChild(child);
        element.Replace(replacement);
    }
}
