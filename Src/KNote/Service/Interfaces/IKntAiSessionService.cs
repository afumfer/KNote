using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

/// <summary>
/// AI assistant sessions of the current user (IKntService.UserIdentityName), persisted as notes: one note of
/// the NoteType KntConst.ChatSessionsTag per session, in the folder KntConst.AiSessionsFolderName, with the
/// provider and model in two KAttributes of that type and a NoteTask that links it to its user. The note type,
/// its attributes and the folder are created the first time they are needed. A session can only be read and
/// saved by its user.
/// </summary>
public interface IKntAiSessionService
{
    /// <summary>The user's sessions, most recently modified first.</summary>
    Task<Result<List<AiChatSessionInfoDto>>> GetUserSessionsAsync();

    Task<Result<AiChatSessionDto>> GetAsync(Guid noteId);

    /// <summary>
    /// Creates the session's note (NoteId == Guid.Empty) or updates it. Returns the session as saved, with its
    /// NoteId, NoteNumber, Topic and dates.
    /// </summary>
    Task<Result<AiChatSessionDto>> SaveAsync(AiChatSessionDto session);

    /// <summary>
    /// Saves a file of a message as a resource of the session's note, created if the conversation has none yet: the file has to
    /// be there before the message is sent, to reach the model. Returns it with the note it is in.
    /// </summary>
    Task<Result<AiChatAttachmentDto>> AddAttachmentAsync(AiChatAttachmentUploadDto attachment);

    /// <summary>
    /// The content of an attachment, only for the user of the session. It does not go by the url of the resource: those are
    /// served to anybody who has them, and a conversation is private.
    /// </summary>
    Task<Result<AiChatAttachmentContentDto>> GetAttachmentAsync(Guid sessionNoteId, Guid resourceId);
}
