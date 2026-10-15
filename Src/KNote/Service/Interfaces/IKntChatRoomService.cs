using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

/// <summary>
/// Chat room threads, persisted as notes: one note of the NoteType KntConst.ChatRoomTag per thread, in the folder
/// KntConst.ChatRoomFolderName, with the conversation in its Description (ChatRoomTranscript) and a NoteTask for
/// each participant (whoever created the thread or joined it, as IKntService.UserIdentityName). The note type and
/// the folder are created the first time they are needed. Threads are public: any Staff user can read them, join
/// them and write in them. This service does not serialize writes: two messages added at the same time to the same
/// thread must be added one after the other by the caller (the Server's chat hub does).
/// </summary>
public interface IKntChatRoomService
{
    /// <summary>Every thread, most recent activity first.</summary>
    Task<Result<List<ChatRoomThreadInfoDto>>> GetThreadsAsync();

    /// <summary>A thread with its participants and messages.</summary>
    Task<Result<ChatRoomThreadDto>> GetAsync(Guid threadId);

    /// <summary>A new thread, with the current user as its first participant.</summary>
    Task<Result<ChatRoomThreadInfoDto>> CreateAsync(string topic);

    /// <summary>The general thread (KntConst.ChatRoomGeneralThreadTopic), created when missing.</summary>
    Task<Result<ChatRoomThreadInfoDto>> GetGeneralAsync();

    /// <summary>Makes the current user a participant of the thread (nothing changes if they already are).</summary>
    Task<Result<ChatRoomThreadInfoDto>> JoinAsync(Guid threadId);

    /// <summary>
    /// Adds a message of the current user (message.ThreadId and message.Text) at the end of the thread, which
    /// they join if they hadn't. Returns the message as saved, with its MessageId, UserName and SentDateTime.
    /// </summary>
    Task<Result<ChatRoomMessageDto>> AddMessageAsync(ChatRoomMessageDto message);
}
