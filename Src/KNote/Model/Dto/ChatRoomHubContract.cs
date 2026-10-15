using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

// The Server's chat room hub (SignalR, Server/Hubs/ChatHub), as its clients (ClientWin and the Web client) use it.
// The hub requires a JWT of the Server (Staff, ProjectManager or Admin), sent as the access_token of the connection.
//
// A client subscribes to the threads it wants to follow in its connection (Subscribe, which returns the thread with
// its messages); the hub sends each new message only to the connections subscribed to its thread. Subscriptions
// belong to the connection: they are lost when it closes, so a client subscribes again after reconnecting.
// Writing in a thread doesn't need a subscription, and makes the user a participant of the thread.
public static class ChatRoomHubContract
{
    // Relative to the application's base (/KNote).
    public const string Path = "/chathub";

    // Client -> Server. Each one returns a Result (Result<T>), invalid with the reason when it fails.

    /// <summary>() -> Result&lt;List&lt;ChatRoomThreadInfoDto&gt;&gt;: every thread, most recent activity first.</summary>
    public const string GetThreads = "GetThreads";

    /// <summary>(string topic) -> Result&lt;ChatRoomThreadInfoDto&gt;. Announced to everybody with ThreadCreated.</summary>
    public const string CreateThread = "CreateThread";

    /// <summary>() -> Result&lt;ChatRoomThreadInfoDto&gt;: the general thread, created when missing.</summary>
    public const string GetGeneralThread = "GetGeneralThread";

    /// <summary>(Guid threadId) -> Result&lt;ChatRoomThreadDto&gt;: the thread with its messages and who is online in it.</summary>
    public const string Subscribe = "Subscribe";

    /// <summary>(Guid threadId) -> Result.</summary>
    public const string Unsubscribe = "Unsubscribe";

    /// <summary>
    /// (Guid threadId, string text) -> Result&lt;ChatRoomMessageDto&gt;: the message as saved. Sent to the thread's
    /// subscribers with MessageReceived (the sender too, if subscribed) and announced to everybody with ThreadActivity.
    /// </summary>
    public const string SendMessage = "SendMessage";

    // Server -> Client.

    /// <summary>(ChatRoomMessageDto): a new message in a thread the connection is subscribed to.</summary>
    public const string MessageReceived = "MessageReceived";

    /// <summary>(ChatRoomThreadInfoDto): a new thread (IsParticipant is false: it is the same for everybody).</summary>
    public const string ThreadCreated = "ThreadCreated";

    /// <summary>(ChatRoomThreadActivityDto): a new message in any thread, to keep the threads lists up to date.</summary>
    public const string ThreadActivity = "ThreadActivity";

    /// <summary>(ChatRoomOnlineUsersDto): who is subscribed to a thread now, to that thread's subscribers.</summary>
    public const string OnlineUsersChanged = "OnlineUsersChanged";
}

public class ChatRoomThreadActivityDto
{
    public Guid ThreadId { get; set; }

    public DateTime ModificationDateTime { get; set; }

    public string UserName { get; set; }
}

public class ChatRoomOnlineUsersDto
{
    public Guid ThreadId { get; set; }

    public List<string> UserNames { get; set; } = new();
}
