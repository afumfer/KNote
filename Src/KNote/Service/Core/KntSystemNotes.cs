using System;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Core;

// What the features that keep their data as notes of a NoteType of the system (AI assistant sessions, chat room
// threads) share: the current user, and the note type, its attributes and the folder, created when missing (found
// by name, so a folder moved elsewhere in the tree is still found). Creating them needs Admin/ProjectManager on
// their own, so only their creation runs in a KntAuthorizationBypass scope (with the reason given). Errors are
// added to the command's result instead of thrown, as every command reports them.
internal static class KntSystemNotes
{
    public static async Task<UserDto> GetCurrentUserAsync(IKntService service, ResultBase result)
    {
        var user = await service.GetCurrentUserAsync();
        if (user == null)
            result.AddErrorMessage($"The user '{service.UserIdentityName}' is not registered in the repository '{service.RepositoryRef?.Name}'.");
        return user;
    }

    public static async Task<Guid> EnsureNoteTypeAsync(IKntService service, string name, string description,
        string bypassReason, ResultBase result)
    {
        var resTypes = await service.NoteTypes.GetAllAsync();
        if (!resTypes.IsValid)
        {
            result.AddListErrorMessage(resTypes.ListErrorMessage);
            return Guid.Empty;
        }

        var noteType = resTypes.Entity?.FirstOrDefault(t => t.Name == name);
        if (noteType != null)
            return noteType.NoteTypeId;

        Result<NoteTypeDto> resSave;
        using (KntAuthorizationBypass.Begin(service, bypassReason))
        {
            resSave = await service.NoteTypes.SaveAsync(new NoteTypeDto
            {
                Name = name,
                Description = description
            });
        }
        if (!resSave.IsValid)
            result.AddListErrorMessage(resSave.ListErrorMessage);
        return resSave.Entity?.NoteTypeId ?? Guid.Empty;
    }

    public static async Task<Guid> EnsureAttributeAsync(IKntService service, Guid noteTypeId, string name,
        string description, int order, string bypassReason, ResultBase result)
    {
        var resAttributes = await service.KAttributes.GetAllAsync(noteTypeId);
        if (!resAttributes.IsValid)
        {
            result.AddListErrorMessage(resAttributes.ListErrorMessage);
            return Guid.Empty;
        }

        var attribute = resAttributes.Entity?.FirstOrDefault(a => a.NoteTypeId == noteTypeId && a.Name == name);
        if (attribute != null)
            return attribute.KAttributeId;

        Result<KAttributeDto> resSave;
        using (KntAuthorizationBypass.Begin(service, bypassReason))
        {
            resSave = await service.KAttributes.SaveAsync(new KAttributeDto
            {
                Name = name,
                Description = description,
                KAttributeDataType = EnumKAttributeDataType.String,
                NoteTypeId = noteTypeId,
                Order = order
            });
        }
        if (!resSave.IsValid)
            result.AddListErrorMessage(resSave.ListErrorMessage);
        return resSave.Entity?.KAttributeId ?? Guid.Empty;
    }

    public static async Task<FolderInfoDto> EnsureFolderAsync(IKntService service, string name, string bypassReason,
        ResultBase result)
    {
        var resFolders = await service.Folders.GetAllAsync();
        if (!resFolders.IsValid)
        {
            result.AddListErrorMessage(resFolders.ListErrorMessage);
            return null;
        }

        var folder = resFolders.Entity?.FirstOrDefault(f => f.Name == name);
        if (folder != null)
            return folder;

        Result<FolderDto> resSave;
        using (KntAuthorizationBypass.Begin(service, bypassReason))
        {
            resSave = await service.Folders.SaveAsync(new FolderDto { Name = name });
        }
        if (!resSave.IsValid)
            result.AddListErrorMessage(resSave.ListErrorMessage);
        return resSave.Entity;
    }

    // The note, only if it is one of the given note type.
    public static async Task<NoteDto> GetNoteOfTypeAsync(IKntService service, Guid noteId, Guid noteTypeId,
        string notFoundMessage, ResultBase result)
    {
        var resNote = await service.Notes.GetAsync(noteId);
        if (!resNote.IsValid)
        {
            result.AddListErrorMessage(resNote.ListErrorMessage);
            return null;
        }

        var note = resNote.Entity;
        if (note == null || note.NoteTypeId != noteTypeId)
        {
            result.AddErrorMessage(notFoundMessage);
            return null;
        }
        return note;
    }
}
