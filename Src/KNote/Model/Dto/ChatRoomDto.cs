using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

// Chat room threads as the Server keeps them (one note per thread, see IKntChatRoomService). Shared by the Server's
// chat hub, ClientWin and the Web client.

// One message of a thread. MessageId, UserName and SentDateTime are given by the Server when the message arrives.
public class ChatRoomMessageDto
{
    public Guid MessageId { get; set; }

    public Guid ThreadId { get; set; }

    public string UserName { get; set; }

    public DateTime SentDateTime { get; set; }

    // Markdown.
    public string Text { get; set; } = "";

    public const int MaxTextLength = 8000;
}

// A thread as listed in the threads panel.
public class ChatRoomThreadInfoDto
{
    // The NoteId of the thread's note.
    public Guid ThreadId { get; set; }

    public int NoteNumber { get; set; }

    public string Topic { get; set; }

    public DateTime CreationDateTime { get; set; }

    // The last message (or the creation, before the first one).
    public DateTime ModificationDateTime { get; set; }

    // The current user has created the thread or joined it.
    public bool IsParticipant { get; set; }

    public const int MaxTopicLength = 120;
}

public class ChatRoomThreadDto : ChatRoomThreadInfoDto
{
    // User names of everybody who has created the thread or joined it.
    public List<string> Participants { get; set; } = new();

    public List<ChatRoomMessageDto> Messages { get; set; } = new();

    // Who is subscribed to the thread now (filled by the Server's chat hub, see ChatRoomHubContract).
    public List<string> OnlineUserNames { get; set; } = new();
}

// An image sent to a thread: kept as a resource of the thread's note (as the images of any note), and shown in a message
// with its Markdown. The client sends ThreadId, FileName, ContentType and ContentBase64 (POST ChatRoomHubContract
// .ImagesPath); the answer has the resource's ResourceId, its Url and the Markdown to send, without the content.
public class ChatRoomImageDto
{
    public Guid ThreadId { get; set; }

    public string FileName { get; set; }

    // One of SupportedContentTypes.
    public string ContentType { get; set; }

    public string ContentBase64 { get; set; }

    public Guid ResourceId { get; set; }

    // Relative to the Server's resources (as note descriptions keep them) in the Service; absolute in the Server's answer.
    public string Url { get; set; }

    public string Markdown { get; set; }

    public static readonly string[] SupportedContentTypes = { "image/png", "image/jpeg", "image/gif", "image/webp" };

    // The content type of an image file by its extension, or null when it is not one of SupportedContentTypes.
    public static string ContentTypeOf(string fileName) =>
        System.IO.Path.GetExtension(fileName ?? "").ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => null
        };
}
