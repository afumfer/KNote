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
