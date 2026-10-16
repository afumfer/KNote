using KNote.ClientWin.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// ChatRoomHtml: the conversation of a chat room thread in the chat room window. The user's own messages and the
/// others' (with their name), a separator when the day changes, Markdown shown with its HTML as text (other users
/// write it, and the page runs a script), and the script call that adds a message.
/// </summary>
[TestClass]
public class ChatRoomHtmlTests
{
    private const string Style = "<style>/* markdown */</style>";

    private static ChatRoomMessageDto Message(string user, string text, DateTime sent) =>
        new() { MessageId = Guid.NewGuid(), UserName = user, Text = text, SentDateTime = sent };

    [TestMethod]
    public void Page_ShowsTheUsersOwnMessagesAndTheOthers_WithTheOthersName()
    {
        var day = new DateTime(2026, 10, 15, 10, 0, 0);
        var page = ChatRoomHtml.Page(new[] { Message("jdoe", "Mine", day), Message("maria", "Hers", day.AddMinutes(1)) },
            "JDoe", Style, "Empty");

        StringAssert.Contains(page, "<div class=\"msg own\">");
        StringAssert.Contains(page, "<div class=\"msg other\">");
        StringAssert.Contains(page, "<span class=\"name\">maria</span>");
        Assert.IsFalse(page.Contains("<span class=\"name\">jdoe</span>"), "The user's own messages don't repeat their name.");
        StringAssert.Contains(page, "<span class=\"time\">10:01</span>");
        StringAssert.Contains(page, Style);
    }

    [TestMethod]
    public void Page_ResolvesRelativeUrls_AgainstTheServer_AndShowsImages()
    {
        var page = ChatRoomHtml.Page(new[] { Message("maria", "![shot.png](/KNoteResources/2026/a_shot.png)", DateTime.Now) },
            "jdoe", Style, "Empty", serverBaseUrl: new Uri("https://server/KNote/"));

        StringAssert.Contains(page, "<base href=\"https://server/KNote/\">");
        StringAssert.Contains(page, "<img src=\"/KNoteResources/2026/a_shot.png\" alt=\"shot.png\"");
    }

    [TestMethod]
    public void Message_HtmlOfTheUser_IsShownAsText_AndMarkdownIsRendered()
    {
        var html = ChatRoomHtml.Message(Message("<b>maria</b>", "**bold** <script>alert(1)</script>", DateTime.Now), "jdoe", null);

        StringAssert.Contains(html, "<strong>bold</strong>");
        Assert.IsFalse(html.Contains("<script>"));
        StringAssert.Contains(html, "&lt;script&gt;");
        Assert.IsFalse(html.Contains("<b>maria</b>"));
    }

    [TestMethod]
    public void Message_FirstOfItsDay_HasADaySeparator()
    {
        var day = new DateTime(2026, 10, 15, 23, 59, 0);

        var sameDay = ChatRoomHtml.Message(Message("maria", "x", day), "jdoe", day.AddMinutes(-5));
        var nextDay = ChatRoomHtml.Message(Message("maria", "x", day.AddMinutes(2)), "jdoe", day);
        var first = ChatRoomHtml.Message(Message("maria", "x", day), "jdoe", null);

        Assert.IsFalse(sameDay.Contains("class=\"day\""));
        StringAssert.Contains(nextDay, "class=\"day\"");
        StringAssert.Contains(first, "class=\"day\"");
    }

    [TestMethod]
    public void Page_WithoutMessages_SaysWhatToDo()
    {
        var page = ChatRoomHtml.Page(Array.Empty<ChatRoomMessageDto>(), "jdoe", Style, "Choose a \"thread\"");

        StringAssert.Contains(page, "data-empty=\"Choose a &quot;thread&quot;\"");
    }

    [TestMethod]
    public void ScriptCall_EscapesTheHtmlAsAJavaScriptString()
    {
        var call = ChatRoomHtml.ScriptCall("append", "<div>\"quoted\" </script></div>");

        StringAssert.StartsWith(call, "window.kntChatRoom.append(\"");
        Assert.IsFalse(call.Contains("</script>"), "Must not close the page's script element.");
    }

    [TestMethod]
    [DataRow("maria", "MA")]
    [DataRow("ana lopez", "AL")]
    [DataRow("j.doe", "JD")]
    [DataRow("x", "X")]
    [DataRow("", "?")]
    public void Initials_OfAUserName(string userName, string expected)
    {
        Assert.AreEqual(expected, ChatRoomHtml.Initials(userName));
    }
}
