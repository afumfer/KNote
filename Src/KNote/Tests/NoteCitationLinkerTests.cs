using KNote.Model;

namespace KNote.Tests;

/// <summary>The citations of notes as links (NoteCitationLinker, D14): only in text, on the HTML already made safe.</summary>
[TestClass]
public class NoteCitationLinkerTests
{
    private static string Url(int number) => $"notes/view/{number}";

    private static string Link(string html) => NoteCitationLinker.Link(html, Url);

    private static string Linked(int number) =>
        $"<a class=\"knt-note-link\" href=\"notes/view/{number}\" data-note-number=\"{number}\">#{number}</a>";

    [TestMethod]
    [DataRow("<p>See #123.</p>", "<p>See {0}.</p>")]
    [DataRow("#123", "{0}")]
    [DataRow("<p>#123</p>", "<p>{0}</p>")]
    [DataRow("(#123)", "({0})")]
    [DataRow("#123, and more", "{0}, and more")]
    [DataRow("<li>at the end #123</li>", "<li>at the end {0}</li>")]
    public void Link_ACitationInTheText_BecomesALink(string html, string expected) =>
        Assert.AreEqual(string.Format(expected, Linked(123)), Link(html));

    [TestMethod]
    public void Link_SeveralCitations_AreAllLinked() =>
        Assert.AreEqual($"<p>{Linked(1)} and {Linked(22)}</p>", Link("<p>#1 and #22</p>"));

    [TestMethod]
    [DataRow("<p>#urgent #123abc #12-3 C#123 #0 #012 ##123</p>")]
    [DataRow("<p>x/#123 and &#123; and &#x23;</p>")]
    [DataRow("<code>#123</code>")]
    [DataRow("<pre><code>a #123</code></pre>")]
    [DataRow("<pre class=\"mermaid\">graph TD; A[#123]</pre>")]
    [DataRow("<a href=\"https://example.com\">ticket #123</a>")]
    [DataRow("<a href=\"https://example.com/#123\">a link</a>")]
    [DataRow("<h2 id=\"#123\" title=\"> #123\">Title</h2>")]
    [DataRow("<!-- #123 -->")]
    [DataRow("")]
    public void Link_WhatIsNotACitation_StaysAsItIs(string html) =>
        Assert.AreEqual(html, Link(html));

    [TestMethod]
    public void Link_AfterAnElementSkipped_GoesOn() =>
        Assert.AreEqual($"<code>#1</code> {Linked(2)}", Link("<code>#1</code> #2"));

    [TestMethod]
    public void Link_InsideElementsThatAreNotSkipped_Works() =>
        Assert.AreEqual($"<p><strong>{Linked(5)}</strong></p>", Link("<p><strong>#5</strong></p>"));

    [TestMethod]
    public void Link_Null_StaysNull() => Assert.IsNull(Link(null!));

    [TestMethod]
    public void DesktopUrl_IsReadBack()
    {
        Assert.IsTrue(NoteCitationLinker.TryGetDesktopNumber(NoteCitationLinker.DesktopUrl(123), out var number));
        Assert.AreEqual(123, number);
        Assert.IsFalse(NoteCitationLinker.TryGetDesktopNumber("https://example.com", out _));
        Assert.IsFalse(NoteCitationLinker.TryGetDesktopNumber("knt-note:abc", out _));
        Assert.IsFalse(NoteCitationLinker.TryGetDesktopNumber(null!, out _));
        StringAssert.Contains(NoteCitationLinker.LinkForDesktop("#7"), "href=\"knt-note:7\"");
    }
}
