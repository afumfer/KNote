using KNote.Service.Core;

namespace KNote.Tests;

/// <summary>
/// Tests for KntHtmlSanitizer: what could run script is removed from a note's HTML, and the formatting written in
/// ClientWin's HTML editor or in Markdown is kept.
/// </summary>
[TestClass]
public class KntHtmlSanitizerTests
{
    [TestMethod]
    [DataRow("<script>alert(1)</script><p>text</p>", "<script")]
    [DataRow("<img src=\"x\" onerror=\"alert(1)\">", "onerror")]
    [DataRow("<p onclick=\"alert(1)\">text</p>", "onclick")]
    [DataRow("<a href=\"javascript:alert(1)\">link</a>", "javascript:")]
    [DataRow("<a href=\"JaVaScRiPt:alert(1)\">link</a>", "alert")]
    [DataRow("<a href=\"java&#09;script:alert(1)\">link</a>", "alert")]
    [DataRow("<a href=\"vbscript:msgbox(1)\">link</a>", "vbscript")]
    [DataRow("<a href=\"data:text/html;base64,PHNjcmlwdD5hbGVydCgxKTwvc2NyaXB0Pg==\">link</a>", "data:")]
    [DataRow("<iframe src=\"https://example.com\"></iframe>", "<iframe")]
    [DataRow("<object data=\"x.swf\"></object>", "<object")]
    [DataRow("<embed src=\"x.swf\">", "<embed")]
    [DataRow("<form action=\"https://example.com\"><input type=\"text\" name=\"pwd\"></form>", "<form")]
    [DataRow("<form action=\"https://example.com\"><input type=\"text\" name=\"pwd\"></form>", "type=\"text\"")]
    [DataRow("<svg><script>alert(1)</script></svg>", "alert")]
    [DataRow("<math><mtext><table><mglyph><style><img src=x onerror=alert(1)>", "onerror")]
    [DataRow("<meta http-equiv=\"refresh\" content=\"0;url=https://example.com\">", "<meta")]
    [DataRow("<base href=\"https://example.com/\">", "<base")]
    [DataRow("<style>body { display: none }</style><p>text</p>", "display")]
    [DataRow("<div style=\"background-image: url(javascript:alert(1))\">text</div>", "javascript")]
    [DataRow("<img src=\"data:text/html,<script>alert(1)</script>\">", "data:text")]
    [DataRow("<a href=\"https://example.com\" data-x=\"1\" srcdoc=\"<script>\">link</a>", "srcdoc")]
    public void Sanitize_RemovesWhatCouldRunScript(string html, string removed)
    {
        var safe = KntHtmlSanitizer.Sanitize(html);

        Assert.IsFalse(safe.Contains(removed, StringComparison.OrdinalIgnoreCase), safe);
    }

    [TestMethod]
    [DataRow("<p><font color=\"#c00000\" face=\"Arial\" size=\"4\">red</font></p>", "<font", "color=\"#c00000\"", "face=\"Arial\"", "red")]
    [DataRow("<p style=\"color: red; font-weight: bold\">styled</p>", "style=", "color", "bold")]
    [DataRow("<table border=\"1\" bgcolor=\"#ffffcc\"><tr><td align=\"center\">cell</td></tr></table>", "<table", "bgcolor=\"#ffffcc\"", "<td", "cell")]
    [DataRow("<img src=\"https://knote.resources/folder/image.png\" alt=\"image\">", "src=\"https://knote.resources/folder/image.png\"")]
    [DataRow("<img src=\"data:image/png;base64,iVBORw0KGgo=\">", "src=\"data:image/png;base64,iVBORw0KGgo=\"")]
    [DataRow("<a href=\"https://example.com/page\" target=\"_blank\">web</a>", "href=\"https://example.com/page\"", "web")]
    [DataRow("<a href=\"mailto:someone@example.com\">mail</a>", "href=\"mailto:someone@example.com\"")]
    [DataRow("<a href=\"file:///C:/Docs/report.pdf\">file</a>", "href=\"file:///C:/Docs/report.pdf\"")]
    [DataRow("<a href=\"Resources/image.png\">relative</a>", "href=\"Resources/image.png\"")]
    [DataRow("<h2 id=\"intro\">Intro</h2><a href=\"#intro\">go</a>", "id=\"intro\"", "href=\"#intro\"")]
    [DataRow("<pre><code class=\"language-csharp\">var x = 1;</code></pre>", "class=\"language-csharp\"", "var x = 1;")]
    [DataRow("<ul><li><input disabled=\"disabled\" type=\"checkbox\" checked=\"checked\"> done</li></ul>", "type=\"checkbox\"", "checked", "done")]
    [DataRow("<p>text</p><!-- knt-ai:{\"role\":\"user\"} -->", "<!-- knt-ai:")]
    [DataRow("<p><b>bold</b> <i>italic</i> <u>underline</u> <strike>strike</strike></p><hr><blockquote>quote</blockquote>", "<b>", "<i>", "<u>", "<strike>", "<hr>", "<blockquote>")]
    public void Sanitize_KeepsTheFormattingOfNotes(string html, params string[] kept)
    {
        var safe = KntHtmlSanitizer.Sanitize(html);

        foreach (var part in kept)
            Assert.IsTrue(safe.Contains(part, StringComparison.OrdinalIgnoreCase), $"'{part}' missing in: {safe}");
    }

    [TestMethod]
    [DataRow("<p>one <o:p>two</o:p> <blink>three</blink></p>", "one", "two", "three")]
    [DataRow("<form action=\"https://example.com\"><p>inside a form</p></form>", "inside a form")]
    public void Sanitize_KeepsTheTextOfTheElementsRemoved(string html, params string[] kept)
    {
        var safe = KntHtmlSanitizer.Sanitize(html);

        foreach (var part in kept)
            Assert.IsTrue(safe.Contains(part), $"'{part}' missing in: {safe}");
    }

    [TestMethod]
    [DataRow("<script>alert(1)</script>")]
    [DataRow("<style>body { display: none }</style>")]
    [DataRow("<iframe>alert(1)</iframe>")]
    [DataRow("<textarea>alert(1)</textarea>")]
    [DataRow("<noscript><img src=x onerror=alert(1)></noscript>")]
    [DataRow("<svg><script>alert(1)</script></svg>")]
    public void Sanitize_RemovesTheContentOfCodeElements(string html)
    {
        var safe = KntHtmlSanitizer.Sanitize("<p>before</p>" + html + "<p>after</p>");

        Assert.IsFalse(safe.Contains("alert") || safe.Contains("display"), safe);
        Assert.IsTrue(safe.Contains("before") && safe.Contains("after"), safe);
    }

    [TestMethod]
    [DataRow("<p style=\"color: #c00000; background-color: yellow\">x</p>", "color: #c00000", "background-color: #ffff00")]
    [DataRow("<span style=\"COLOR: rgb(255,0,0)\">x</span>", "color: #ff0000")]
    [DataRow("<span style=\"color: rgba(255, 0, 0, 0.5)\">x</span>", "rgba(255, 0, 0, 0.5)")]
    public void Sanitize_WritesOpaqueColorsAsHex(string html, params string[] kept)
    {
        // ClientWin's HTML editor (Internet Explorer's engine, IE7 mode) doesn't know rgba().
        var safe = KntHtmlSanitizer.Sanitize(html);

        foreach (var part in kept)
            Assert.IsTrue(safe.Contains(part), $"'{part}' missing in: {safe}");
    }

    [TestMethod]
    public void Sanitize_OnlyKeepsReadOnlyCheckboxes()
    {
        var safe = KntHtmlSanitizer.Sanitize("<input type=\"checkbox\"><input type=\"text\" value=\"x\"><input>");

        Assert.AreEqual(1, safe.Split("<input").Length - 1, safe);
        Assert.IsTrue(safe.Contains("disabled"), safe);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void Sanitize_WithoutHtml_ReturnsEmpty(string? html)
    {
        Assert.AreEqual("", KntHtmlSanitizer.Sanitize(html!));
    }

    [TestMethod]
    public void Sanitize_KeepsPlainText()
    {
        Assert.AreEqual("Some text &amp; more", KntHtmlSanitizer.Sanitize("Some text &amp; more"));
    }
}
