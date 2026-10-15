using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KNote.Model.Dto;

// The Description of a chat room thread note: the conversation as Markdown, readable like any other note, with
// an HTML comment before each message that Markdown viewers don't show and that lets Parse get the messages back
// exactly (the same idea as AiChatSessionTranscript):
//
//   <!-- knt-chat:message {"id":"…","user":"armando","at":"2026-10-15T10:31:02"} -->
//   **armando** · 2026-10-15 10:31
//   (the message)
//
// A message line that is itself exactly one of these markers would split the message there; text before the
// first marker is ignored.
public static class ChatRoomTranscript
{
    private const string MarkerStart = "<!-- knt-chat:message";
    private const string MarkerEnd = "-->";
    private const string Separator = "\r\n\r\n";

    private static readonly JsonSerializerOptions MarkerJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Write(IEnumerable<ChatRoomMessageDto> messages)
    {
        var text = new StringBuilder();
        foreach (var message in messages)
            text.Append(WriteMessage(message));
        return text.ToString();
    }

    // The transcript with one more message at its end.
    public static string Append(string transcript, ChatRoomMessageDto message)
    {
        if (string.IsNullOrEmpty(transcript))
            return WriteMessage(message);

        var text = transcript.TrimEnd('\r', '\n');
        return text + Separator + WriteMessage(message);
    }

    public static List<ChatRoomMessageDto> Parse(string transcript, Guid threadId = default)
    {
        var messages = new List<ChatRoomMessageDto>();
        if (string.IsNullOrEmpty(transcript))
            return messages;

        ChatRoomMessageDto message = null;
        List<string> section = null;

        void CloseSection()
        {
            if (message != null && section != null)
                message.Text = SectionContent(section, Header(message));
        }

        foreach (var line in transcript.ReplaceLineEndings("\n").Split('\n'))
        {
            if (line.StartsWith(MarkerStart) && line.EndsWith(MarkerEnd))
            {
                CloseSection();
                message = ReadMarker(line.Substring(MarkerStart.Length, line.Length - MarkerStart.Length - MarkerEnd.Length));
                message.ThreadId = threadId;
                messages.Add(message);
                section = new List<string>();
            }
            else
                section?.Add(line);
        }
        CloseSection();

        return messages;
    }

    private static string WriteMessage(ChatRoomMessageDto message)
    {
        var marker = JsonSerializer.Serialize(new MessageMarker
        {
            Id = message.MessageId,
            User = message.UserName,
            At = message.SentDateTime
        }, MarkerJsonOptions);

        return $"{MarkerStart} {marker} {MarkerEnd}\r\n{Header(message)}  \r\n{Content(message.Text)}{Separator}";
    }

    private static string Header(ChatRoomMessageDto message) =>
        $"**{message.UserName}** · {message.SentDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}";

    private static string Content(string text) =>
        (text ?? "").ReplaceLineEndings("\r\n").TrimEnd('\r', '\n');

    // The lines of a message without its header and the blank lines that separate it from the next one.
    private static string SectionContent(List<string> lines, string header)
    {
        var start = lines.Count > 0 && lines[0].Trim() == header ? 1 : 0;
        var end = lines.Count;
        while (end > start && string.IsNullOrWhiteSpace(lines[end - 1]))
            end--;
        return string.Join("\r\n", lines.Skip(start).Take(end - start));
    }

    private static ChatRoomMessageDto ReadMarker(string json)
    {
        try
        {
            var marker = JsonSerializer.Deserialize<MessageMarker>(json.Trim(), MarkerJsonOptions);
            if (marker != null)
                return new ChatRoomMessageDto { MessageId = marker.Id, UserName = marker.User, SentDateTime = marker.At };
        }
        catch (JsonException)
        {
            // A hand-edited marker must not lose the conversation: the message is kept, without its data.
        }
        return new ChatRoomMessageDto();
    }

    private class MessageMarker
    {
        public Guid Id { get; set; }
        public string User { get; set; }
        public DateTime At { get; set; }
    }
}
