using System.Net;
using System.Text.Json;
using KNote.ClientWin.Core;

namespace KNote.ClientWin.Tests;

[TestClass]
public class AiChatHtmlTests
{
    private static AiChatTurn Turn(string prompt, string answer) =>
        new(prompt, answer, "Claude", TimeSpan.FromSeconds(1.2)) { InputTokens = 3, OutputTokens = 2, TotalTokens = 5 };

    [TestMethod]
    public void Page_ShowsEveryTurnInOrder_WithTheChatStyleAndScript()
    {
        var html = AiChatHtml.Page(new[] { Turn("First question", "First answer"), Turn("Second question", "Second answer") }, "<style>/*md*/</style>");

        var positions = new[] { "First question", "First answer", "Second question", "Second answer" }
            .Select(text => html.IndexOf(text, StringComparison.Ordinal)).ToArray();
        CollectionAssert.DoesNotContain(positions, -1);
        CollectionAssert.AreEqual(positions.OrderBy(p => p).ToArray(), positions);
        StringAssert.Contains(html, "<style>/*md*/</style>");
        StringAssert.Contains(html, ".msg.user");          // KNoteAIChat.css, embedded
        StringAssert.Contains(html, "window.kntChat");
    }

    [TestMethod]
    public void Page_NoTurns_HasAnEmptyChat()
    {
        var html = AiChatHtml.Page(Array.Empty<AiChatTurn>(), "");

        StringAssert.Contains(html, "<main id=\"chat\"></main>");
    }

    [TestMethod]
    public void UserMessage_IsPlainText()
    {
        var html = AiChatHtml.UserMessage("Is <b>this</b> & **that** bold?");

        StringAssert.Contains(html, "Is &lt;b&gt;this&lt;/b&gt; &amp; **that** bold?");
        StringAssert.Contains(html, "class=\"msg user\"");
    }

    [TestMethod]
    public void AssistantMessage_RendersMarkdown_WithAuthorAndFooter()
    {
        var turn = Turn("q", "Some **bold** text");

        var html = AiChatHtml.AssistantMessage(turn);

        StringAssert.Contains(html, "<strong>bold</strong>");
        StringAssert.Contains(html, "Assistant · Claude");
        StringAssert.Contains(html, $"<div class=\"meta\">{WebUtility.HtmlEncode(turn.UsageSummary)}</div>");
        StringAssert.Contains(html, "class=\"msg assistant\"");
    }

    [TestMethod]
    public void MarkdownToHtml_RawHtmlOfTheModel_IsShownAsText()
    {
        var html = AiChatHtml.MarkdownToHtml("Hi <img src=x onerror=alert(1)> <script>alert(2)</script>");

        Assert.IsFalse(html.Contains("<img", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(html.Contains("<script", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(html, "&lt;script&gt;");
    }

    [TestMethod]
    public void PendingAnswer_NothingReceivedYet_ShowsTheTypingIndicator()
    {
        var html = AiChatHtml.PendingAnswer("Claude", null);

        StringAssert.Contains(html, "class=\"typing\"");
        StringAssert.Contains(html, "msg assistant pending");
    }

    [TestMethod]
    public void PendingAnswer_TextReceived_RendersIt()
    {
        var html = AiChatHtml.PendingAnswer("Claude", "- one\r\n- tw");

        StringAssert.Contains(html, "<li>tw</li>");
        Assert.IsFalse(html.Contains("class=\"typing\""));
    }

    [TestMethod]
    public void FailedAnswer_KeepsWhatArrived_MarkedAsFailed()
    {
        var html = AiChatHtml.FailedAnswer("Claude", "partial");

        StringAssert.Contains(html, "msg assistant failed");
        StringAssert.Contains(html, "partial");
        StringAssert.Contains(html, "request failed");
    }

    [TestMethod]
    public void UsageSummary_ReportedTokens_ShowsInOutAndTotal()
    {
        Assert.AreEqual("3 in · 2 out · 5 tokens · 1.2 s", Turn("q", "a").UsageSummary);
    }

    [TestMethod]
    public void UsageSummary_EstimatedTokens_SaysSo()
    {
        var turn = new AiChatTurn("q", "a", null, TimeSpan.FromSeconds(2)) { TotalTokens = 12, TokensEstimated = true };

        Assert.AreEqual("~12 tokens (estimated) · 2.0 s", turn.UsageSummary);
    }

    [TestMethod]
    public void Page_ModelInfoHidden_HidesItWithTheBodyClass()
    {
        Assert.IsFalse(AiChatHtml.Page(new[] { Turn("q", "a") }, "", showModelInfo: true).Contains("hide-model-info\">"));
        StringAssert.Contains(AiChatHtml.Page(new[] { Turn("q", "a") }, "", showModelInfo: false), "<body class=\"hide-model-info\">");
    }

    [TestMethod]
    [DataRow(Keys.Control | Keys.K, "Ctrl+K")]
    [DataRow(Keys.Control | Keys.Return, "Ctrl+Enter")]
    [DataRow(Keys.Control | Keys.Shift | Keys.F5, "Ctrl+Shift+F5")]
    [DataRow(Keys.Alt | Keys.D1, "Alt+D1")]
    public void ShortcutText_NamesTheKeysAsThePageScriptDoes(Keys keys, string expected)
    {
        Assert.AreEqual(expected, AiChatHtml.ShortcutText(keys));
    }

    [TestMethod]
    public void Page_PassesTheHostShortcutsToItsScript()
    {
        var shortcuts = new[] { "Ctrl+K", "Ctrl+Enter" };

        var html = AiChatHtml.Page(Array.Empty<AiChatTurn>(), "", hostShortcuts: shortcuts);

        // As a JSON array, a valid JavaScript literal ("+" may be escaped as +: the same string).
        StringAssert.Contains(html, $"window.kntChatShortcuts = {JsonSerializer.Serialize(shortcuts)};");
        StringAssert.Contains(html, "window.chrome.webview.postMessage(shortcut)");
    }

    [TestMethod]
    public void Transcript_WithModelInfo_HasEachTurnAndItsUsage()
    {
        var text = AiChatTranscript.Markdown(new[] { Turn("First", "One"), Turn("Second", "Two") }, includeModelInfo: true);

        Assert.AreEqual(
            "**User:** \r\nFirst\r\n\r\n**Assistant:** \r\nOne\r\n\r\n*(3 in · 2 out · 5 tokens · 1.2 s)*\r\n\r\n" +
            "**User:** \r\nSecond\r\n\r\n**Assistant:** \r\nTwo\r\n\r\n*(3 in · 2 out · 5 tokens · 1.2 s)*\r\n\r\n",
            text);
    }

    [TestMethod]
    public void AssistantMessage_TruncatedAnswer_ShowsTheNoticeApartFromTheModelInfo()
    {
        var html = AiChatHtml.AssistantMessage(Turn("q", "Once upon a") with { Truncated = true });

        StringAssert.Contains(html, $"<div class=\"notice\">{WebUtility.HtmlEncode(AiChatTurn.TruncatedNotice)}</div>");
        Assert.IsFalse(AiChatHtml.AssistantMessage(Turn("q", "The end.")).Contains("class=\"notice\""));
    }

    [TestMethod]
    public void Transcript_TruncatedAnswer_KeepsTheNoticeWithoutModelInfo()
    {
        var text = AiChatTranscript.Markdown(new[] { Turn("First", "One") with { Truncated = true } }, includeModelInfo: false);

        Assert.AreEqual($"**User:** \r\nFirst\r\n\r\n**Assistant:** \r\nOne\r\n\r\n*({AiChatTurn.TruncatedNotice})*\r\n\r\n", text);
    }

    [TestMethod]
    public void Transcript_WithoutModelInfo_HasNoUsage()
    {
        var text = AiChatTranscript.Markdown(new[] { Turn("First", "One") }, includeModelInfo: false);

        Assert.AreEqual("**User:** \r\nFirst\r\n\r\n**Assistant:** \r\nOne\r\n\r\n", text);
    }

    [TestMethod]
    public void ScriptCall_ArgumentIsAnEscapedStringLiteral()
    {
        var argument = "<div>\"quotes\" 'single' \\ \r\n</script></div>";

        var script = AiChatHtml.ScriptCall("append", argument);

        StringAssert.StartsWith(script, "window.kntChat.append(\"");
        StringAssert.EndsWith(script, "\");");
        Assert.IsFalse(script.Contains('\n'));
        Assert.IsFalse(script.Contains("</script>"));
        var literal = script["window.kntChat.append(".Length..^2];
        Assert.AreEqual(argument, JsonSerializer.Deserialize<string>(literal));
    }
}
