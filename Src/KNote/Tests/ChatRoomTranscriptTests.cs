using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// ChatRoomTranscript: the Description of a chat room thread note, Markdown readable as any note and parsed back
/// into the same messages (the threads are loaded from it by the Server's chat hub).
/// </summary>
[TestClass]
public class ChatRoomTranscriptTests
{
    [TestMethod]
    public void Write_ThenParse_ReturnsTheSameMessages()
    {
        var threadId = Guid.NewGuid();
        var messages = new List<ChatRoomMessageDto>
        {
            NewMessage("armando", "First message\r\n\r\nwith a blank line", new DateTime(2026, 10, 15, 10, 31, 2)),
            NewMessage("maria", "```csharp\r\nvar x = 1;\r\n```\r\n\r\n- item", new DateTime(2026, 10, 15, 10, 32, 45))
        };

        var parsed = ChatRoomTranscript.Parse(ChatRoomTranscript.Write(messages), threadId);

        AssertSameMessages(messages, parsed, threadId);
    }

    [TestMethod]
    public void Append_AddsTheMessageAtTheEnd_AlsoToAnEmptyTranscript()
    {
        var first = NewMessage("armando", "Hello", new DateTime(2026, 10, 15, 9, 0, 0));
        var second = NewMessage("maria", "Hi\nthere", new DateTime(2026, 10, 15, 9, 1, 0));

        var transcript = ChatRoomTranscript.Append(null, first);
        transcript = ChatRoomTranscript.Append(transcript, second);

        var parsed = ChatRoomTranscript.Parse(transcript);
        Assert.AreEqual(2, parsed.Count);
        Assert.AreEqual("Hello", parsed[0].Text);
        Assert.AreEqual("Hi\r\nthere", parsed[1].Text, "Line endings are kept as CR LF, as the notes keep them.");
        Assert.AreEqual(ChatRoomTranscript.Write(new[] { first, second }), transcript);
    }

    [TestMethod]
    public void Append_ToAHandEditedTranscript_KeepsTheMessagesApart()
    {
        var first = NewMessage("armando", "Hello", new DateTime(2026, 10, 15, 9, 0, 0));
        var edited = ChatRoomTranscript.Write(new[] { first }).TrimEnd() + " (edited)";

        var parsed = ChatRoomTranscript.Parse(ChatRoomTranscript.Append(edited, NewMessage("maria", "Hi", DateTime.Now)));

        Assert.AreEqual(2, parsed.Count);
        Assert.AreEqual("Hello (edited)", parsed[0].Text);
        Assert.AreEqual("Hi", parsed[1].Text);
    }

    [TestMethod]
    public void Write_IsReadableMarkdown_WithTheMarkerHiddenInAComment()
    {
        var message = NewMessage("armando", "Hello", new DateTime(2026, 10, 15, 10, 31, 2));

        var transcript = ChatRoomTranscript.Write(new[] { message });

        StringAssert.StartsWith(transcript, "<!-- knt-chat:message {");
        StringAssert.Contains(transcript, "**armando** · 2026-10-15 10:31");
        StringAssert.Contains(transcript, "\r\nHello\r\n");
    }

    [TestMethod]
    public void Parse_IgnoresTextBeforeTheFirstMarker_AndKeepsAMessageWithABrokenMarker()
    {
        var message = NewMessage("armando", "Hello", new DateTime(2026, 10, 15, 10, 31, 2));
        var transcript = "Some notes written by hand\r\n\r\n" +
            "<!-- knt-chat:message {broken -->\r\nLost data\r\n\r\n" +
            ChatRoomTranscript.Write(new[] { message });

        var parsed = ChatRoomTranscript.Parse(transcript);

        Assert.AreEqual(2, parsed.Count);
        Assert.AreEqual("Lost data", parsed[0].Text);
        Assert.AreEqual(Guid.Empty, parsed[0].MessageId);
        Assert.AreEqual(message.MessageId, parsed[1].MessageId);
    }

    [TestMethod]
    public void Parse_EmptyOrNull_ReturnsNoMessages()
    {
        Assert.AreEqual(0, ChatRoomTranscript.Parse(null).Count);
        Assert.AreEqual(0, ChatRoomTranscript.Parse("").Count);
    }

    private static ChatRoomMessageDto NewMessage(string user, string text, DateTime sent) => new()
    {
        MessageId = Guid.NewGuid(),
        UserName = user,
        Text = text,
        SentDateTime = sent
    };

    private static void AssertSameMessages(List<ChatRoomMessageDto> expected, List<ChatRoomMessageDto> actual, Guid threadId)
    {
        Assert.AreEqual(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.AreEqual(expected[i].MessageId, actual[i].MessageId);
            Assert.AreEqual(expected[i].UserName, actual[i].UserName);
            Assert.AreEqual(expected[i].SentDateTime, actual[i].SentDateTime);
            Assert.AreEqual(expected[i].Text, actual[i].Text);
            Assert.AreEqual(threadId, actual[i].ThreadId);
        }
    }
}
