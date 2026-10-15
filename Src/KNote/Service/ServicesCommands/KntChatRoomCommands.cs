using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

// Chat room threads (see IKntChatRoomService). Staff, like the chat room itself: a thread is a note, and its
// participants are tasks of that note, both created by the users who write in it. Everything goes through the
// other domains' services (notes, tasks, note types, folders, users), so their rules and command events apply.
// Creating the note type and the folder needs Admin/ProjectManager on its own: once this command is authorized,
// that is done inside a KntAuthorizationBypass scope (KntSystemNotes), so the chat room works for any Staff user.

[KntAuthorize(EnumRoles.Staff)]
public class KntChatRoomsGetThreadsAsyncCommand : KntCommandServiceBase<Result<List<ChatRoomThreadInfoDto>>>
{
    public KntChatRoomsGetThreadsAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<ChatRoomThreadInfoDto>>> Execute()
    {
        var result = new Result<List<ChatRoomThreadInfoDto>>();

        var user = await KntSystemNotes.GetCurrentUserAsync(Service, result);
        var infrastructure = await ChatRooms.EnsureInfrastructureAsync(Service, result);
        if (!result.IsValid)
            return result;

        var resNotes = await Service.Notes.GetFilterMinimalAsync(new NotesFilterDto { NoteTypeId = infrastructure.NoteTypeId });
        if (!resNotes.IsValid)
        {
            result.AddListErrorMessage(resNotes.ListErrorMessage);
            return result;
        }

        var resUserNotes = await Service.Notes.GetFilterMinimalAsync(new NotesFilterDto
        {
            NoteTypeId = infrastructure.NoteTypeId,
            TaskUserId = user.UserId
        });
        if (!resUserNotes.IsValid)
        {
            result.AddListErrorMessage(resUserNotes.ListErrorMessage);
            return result;
        }
        var userThreads = resUserNotes.Entity.Select(n => n.NoteId).ToHashSet();

        result.Entity = resNotes.Entity
            .OrderByDescending(n => n.ModificationDateTime)
            .Select(n => new ChatRoomThreadInfoDto
            {
                ThreadId = n.NoteId,
                NoteNumber = n.NoteNumber,
                Topic = n.Topic,
                CreationDateTime = n.CreationDateTime,
                ModificationDateTime = n.ModificationDateTime,
                IsParticipant = userThreads.Contains(n.NoteId)
            })
            .ToList();

        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntChatRoomsGetAsyncCommand : KntCommandServiceBase<Guid, Result<ChatRoomThreadDto>>
{
    public KntChatRoomsGetAsyncCommand(IKntService service, Guid threadId) : base(service, threadId)
    {

    }

    public override async Task<Result<ChatRoomThreadDto>> Execute()
    {
        var result = new Result<ChatRoomThreadDto>();

        var user = await KntSystemNotes.GetCurrentUserAsync(Service, result);
        var infrastructure = await ChatRooms.EnsureInfrastructureAsync(Service, result);
        if (!result.IsValid)
            return result;

        var note = await ChatRooms.GetThreadNoteAsync(Service, Param, infrastructure, result);
        if (!result.IsValid)
            return result;

        var participants = await ChatRooms.GetParticipantsAsync(Service, Param, result);
        if (!result.IsValid)
            return result;

        var thread = new ChatRoomThreadDto
        {
            Participants = participants.Select(p => p.UserName).OrderBy(n => n).ToList(),
            Messages = ChatRoomTranscript.Parse(note.Description, note.NoteId)
        };
        ChatRooms.FillInfo(thread, note, participants.Any(p => p.UserId == user.UserId));
        result.Entity = thread;

        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntChatRoomsCreateAsyncCommand : KntCommandServiceBase<string, Result<ChatRoomThreadInfoDto>>
{
    public KntChatRoomsCreateAsyncCommand(IKntService service, string topic) : base(service, topic)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (string.IsNullOrWhiteSpace(Param))
            result.AddErrorMessage("The topic of the thread is required.");
        else if (Param.Trim().Length > ChatRoomThreadInfoDto.MaxTopicLength)
            result.AddErrorMessage($"The topic of the thread can't be longer than {ChatRoomThreadInfoDto.MaxTopicLength} characters.");
        return result;
    }

    public override async Task<Result<ChatRoomThreadInfoDto>> Execute()
    {
        var result = new Result<ChatRoomThreadInfoDto>();

        var user = await KntSystemNotes.GetCurrentUserAsync(Service, result);
        var infrastructure = await ChatRooms.EnsureInfrastructureAsync(Service, result);
        if (!result.IsValid)
            return result;

        result.Entity = await ChatRooms.CreateThreadAsync(Service, Param.Trim(), infrastructure, user, result);
        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntChatRoomsGetGeneralAsyncCommand : KntCommandServiceBase<Result<ChatRoomThreadInfoDto>>
{
    public KntChatRoomsGetGeneralAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<ChatRoomThreadInfoDto>> Execute()
    {
        var result = new Result<ChatRoomThreadInfoDto>();

        var user = await KntSystemNotes.GetCurrentUserAsync(Service, result);
        var infrastructure = await ChatRooms.EnsureInfrastructureAsync(Service, result);
        if (!result.IsValid)
            return result;

        var resNotes = await Service.Notes.GetFilterMinimalAsync(new NotesFilterDto { NoteTypeId = infrastructure.NoteTypeId });
        if (!resNotes.IsValid)
        {
            result.AddListErrorMessage(resNotes.ListErrorMessage);
            return result;
        }

        // The oldest one, if there were more than one with that topic.
        var general = resNotes.Entity
            .Where(n => n.Topic == KntConst.ChatRoomGeneralThreadTopic)
            .OrderBy(n => n.CreationDateTime)
            .ThenBy(n => n.NoteNumber)
            .FirstOrDefault();

        if (general == null)
        {
            result.Entity = await ChatRooms.CreateThreadAsync(Service, KntConst.ChatRoomGeneralThreadTopic, infrastructure, user, result);
            return result;
        }

        var resTasks = await Service.Notes.GetNoteTasksAsync(general.NoteId);
        if (!resTasks.IsValid)
        {
            result.AddListErrorMessage(resTasks.ListErrorMessage);
            return result;
        }

        result.Entity = new ChatRoomThreadInfoDto
        {
            ThreadId = general.NoteId,
            NoteNumber = general.NoteNumber,
            Topic = general.Topic,
            CreationDateTime = general.CreationDateTime,
            ModificationDateTime = general.ModificationDateTime,
            IsParticipant = resTasks.Entity?.Any(t => t.UserId == user.UserId) ?? false
        };
        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntChatRoomsJoinAsyncCommand : KntCommandServiceBase<Guid, Result<ChatRoomThreadInfoDto>>
{
    public KntChatRoomsJoinAsyncCommand(IKntService service, Guid threadId) : base(service, threadId)
    {

    }

    public override async Task<Result<ChatRoomThreadInfoDto>> Execute()
    {
        var result = new Result<ChatRoomThreadInfoDto>();

        var user = await KntSystemNotes.GetCurrentUserAsync(Service, result);
        var infrastructure = await ChatRooms.EnsureInfrastructureAsync(Service, result);
        if (!result.IsValid)
            return result;

        var note = await ChatRooms.GetThreadNoteAsync(Service, Param, infrastructure, result);
        if (!result.IsValid)
            return result;

        await ChatRooms.EnsureParticipantAsync(Service, Param, user, result);
        if (!result.IsValid)
            return result;

        var thread = new ChatRoomThreadInfoDto();
        ChatRooms.FillInfo(thread, note, true);
        result.Entity = thread;
        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntChatRoomsAddMessageAsyncCommand : KntCommandServiceBase<ChatRoomMessageDto, Result<ChatRoomMessageDto>>
{
    public KntChatRoomsAddMessageAsyncCommand(IKntService service, ChatRoomMessageDto message) : base(service, message)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null)
            result.AddErrorMessage("The message is required.");
        else
        {
            if (Param.ThreadId == Guid.Empty)
                result.AddErrorMessage("The thread of the message is required.");
            if (string.IsNullOrWhiteSpace(Param.Text))
                result.AddErrorMessage("The message is empty.");
            else if (Param.Text.Length > ChatRoomMessageDto.MaxTextLength)
                result.AddErrorMessage($"The message can't be longer than {ChatRoomMessageDto.MaxTextLength} characters.");
        }
        return result;
    }

    public override async Task<Result<ChatRoomMessageDto>> Execute()
    {
        var result = new Result<ChatRoomMessageDto>();

        var user = await KntSystemNotes.GetCurrentUserAsync(Service, result);
        var infrastructure = await ChatRooms.EnsureInfrastructureAsync(Service, result);
        if (!result.IsValid)
            return result;

        var note = await ChatRooms.GetThreadNoteAsync(Service, Param.ThreadId, infrastructure, result);
        if (!result.IsValid)
            return result;

        await ChatRooms.EnsureParticipantAsync(Service, note.NoteId, user, result);
        if (!result.IsValid)
            return result;

        var message = new ChatRoomMessageDto
        {
            MessageId = Guid.NewGuid(),
            ThreadId = note.NoteId,
            UserName = user.UserName,
            SentDateTime = DateTime.Now,
            Text = Param.Text.ReplaceLineEndings("\r\n").TrimEnd('\r', '\n')
        };

        note.Description = ChatRoomTranscript.Append(note.Description, message);
        var resSave = await Service.Notes.SaveAsync(note);
        if (!resSave.IsValid)
        {
            result.AddListErrorMessage(resSave.ListErrorMessage);
            return result;
        }

        result.Entity = message;
        return result;
    }
}

internal sealed record ChatRoomsInfrastructure(Guid NoteTypeId, FolderInfoDto Folder);

// What the chat room commands share. Errors are added to the command's result instead of thrown, as every command
// reports them.
internal static class ChatRooms
{
    private const string InfrastructureBypassReason = "Chat room: note type and folder";
    private const string ParticipantTaskDescription = "Chat room participant";

    // The note type and the folder of the threads, created when missing.
    public static async Task<ChatRoomsInfrastructure> EnsureInfrastructureAsync(IKntService service, ResultBase result)
    {
        var noteTypeId = await KntSystemNotes.EnsureNoteTypeAsync(service, KntConst.ChatRoomTag, "KNote chat room threads",
            InfrastructureBypassReason, result);
        if (!result.IsValid)
            return null;

        var folder = await KntSystemNotes.EnsureFolderAsync(service, KntConst.ChatRoomFolderName, InfrastructureBypassReason, result);
        if (!result.IsValid)
            return null;

        return new ChatRoomsInfrastructure(noteTypeId, folder);
    }

    public static Task<NoteDto> GetThreadNoteAsync(IKntService service, Guid threadId, ChatRoomsInfrastructure infrastructure,
        ResultBase result) =>
        KntSystemNotes.GetNoteOfTypeAsync(service, threadId, infrastructure.NoteTypeId, "The chat room thread does not exist.", result);

    public static async Task<ChatRoomThreadInfoDto> CreateThreadAsync(IKntService service, string topic,
        ChatRoomsInfrastructure infrastructure, UserDto user, ResultBase result)
    {
        // With the NoteType, KAttributesDto comes completed with the type's attributes, as for any new note.
        var resNew = await service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = infrastructure.NoteTypeId });
        if (!resNew.IsValid)
        {
            result.AddListErrorMessage(resNew.ListErrorMessage);
            return null;
        }

        var note = resNew.Entity;
        note.Topic = topic;
        note.Description = "";
        // NewExtendedAsync leaves Tags null, and KntNotesSaveExtendedAsyncCommand needs it.
        note.Tags = "";
        // FolderDto too: it is validated with the note.
        note.FolderId = infrastructure.Folder.FolderId;
        note.FolderDto = infrastructure.Folder.GetSimpleDto<FolderDto>();
        note.NoteTypeId = infrastructure.NoteTypeId;
        note.Tasks.Add(NewParticipantTask(Guid.Empty, user));

        var resSave = await service.Notes.SaveExtendedAsync(note);
        if (!resSave.IsValid)
        {
            result.AddListErrorMessage(resSave.ListErrorMessage);
            return null;
        }

        var thread = new ChatRoomThreadInfoDto();
        FillInfo(thread, resSave.Entity, true);
        return thread;
    }

    // Who has created the thread or joined it: the users of its tasks.
    public static async Task<List<UserDto>> GetParticipantsAsync(IKntService service, Guid threadId, ResultBase result)
    {
        var resTasks = await service.Notes.GetNoteTasksAsync(threadId);
        if (!resTasks.IsValid)
        {
            result.AddListErrorMessage(resTasks.ListErrorMessage);
            return null;
        }

        var userIds = (resTasks.Entity ?? new List<NoteTaskDto>()).Select(t => t.UserId).ToHashSet();
        if (userIds.Count == 0)
            return new List<UserDto>();

        var resUsers = await service.Users.GetAllAsync();
        if (!resUsers.IsValid)
        {
            result.AddListErrorMessage(resUsers.ListErrorMessage);
            return null;
        }

        return resUsers.Entity
            .Where(u => userIds.Contains(u.UserId))
            .Select(u => new UserDto { UserId = u.UserId, UserName = u.UserName })
            .ToList();
    }

    public static async Task EnsureParticipantAsync(IKntService service, Guid threadId, UserDto user, ResultBase result)
    {
        var resTasks = await service.Notes.GetNoteTasksAsync(threadId);
        if (!resTasks.IsValid)
        {
            result.AddListErrorMessage(resTasks.ListErrorMessage);
            return;
        }
        if (resTasks.Entity != null && resTasks.Entity.Any(t => t.UserId == user.UserId))
            return;

        var resSave = await service.Notes.SaveNoteTaskAsync(NewParticipantTask(threadId, user));
        if (!resSave.IsValid)
            result.AddListErrorMessage(resSave.ListErrorMessage);
    }

    public static void FillInfo(ChatRoomThreadInfoDto thread, NoteInfoDto note, bool isParticipant)
    {
        thread.ThreadId = note.NoteId;
        thread.NoteNumber = note.NoteNumber;
        thread.Topic = note.Topic;
        thread.CreationDateTime = note.CreationDateTime;
        thread.ModificationDateTime = note.ModificationDateTime;
        thread.IsParticipant = isParticipant;
    }

    // The task that links a participant to the thread, starting when they joined.
    private static NoteTaskDto NewParticipantTask(Guid threadId, UserDto user)
    {
        var now = DateTime.Now;
        return new NoteTaskDto
        {
            NoteId = threadId,
            UserId = user.UserId,
            Description = ParticipantTaskDescription,
            Tags = "",
            CreationDateTime = now,
            ModificationDateTime = now,
            StartDate = now
        };
    }
}
