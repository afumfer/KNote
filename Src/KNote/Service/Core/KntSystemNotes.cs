using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Core;

// What the features that keep their data as notes of a NoteType of the system (AI assistant sessions, chat room
// threads) share: the current user, and the note type, its attributes and the folder, created when missing (found
// by name, so a folder moved elsewhere in the tree is still found). Creating them needs Admin/ProjectManager on
// their own, so only their creation runs in a KntAuthorizationBypass scope (with the reason given). Errors are
// added to the command's result instead of thrown, as every command reports them.
//
// Their creation is serialized per database (FindOrCreateAsync): several requests of the Server (or windows of
// ClientWin) using a feature for the first time at once would otherwise all find them missing and all create them -
// the note type and the attributes then failed on their unique indexes, and the folder was duplicated. The lock is
// per process: when another process using the same database creates one first, the failed creation finds it.
internal static class KntSystemNotes
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> CreationLocks = new();

    public static async Task<UserDto> GetCurrentUserAsync(IKntService service, ResultBase result)
    {
        var user = await service.GetCurrentUserAsync();
        if (user == null)
            result.AddErrorMessage($"The user '{service.UserIdentityName}' is not registered in the repository '{service.RepositoryRef?.Name}'.");
        return user;
    }

    public static Task<Guid> EnsureNoteTypeAsync(IKntService service, string name, string description,
        string bypassReason, ResultBase result) =>
        FindOrCreateAsync(service, result,
            find: async () =>
            {
                var resTypes = await service.NoteTypes.GetAllAsync();
                if (!resTypes.IsValid)
                {
                    result.AddListErrorMessage(resTypes.ListErrorMessage);
                    return Guid.Empty;
                }
                return resTypes.Entity?.FirstOrDefault(t => t.Name == name)?.NoteTypeId ?? Guid.Empty;
            },
            create: async () =>
            {
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
            });

    public static Task<Guid> EnsureAttributeAsync(IKntService service, Guid noteTypeId, string name,
        string description, int order, string bypassReason, ResultBase result) =>
        FindOrCreateAsync(service, result,
            find: async () =>
            {
                var resAttributes = await service.KAttributes.GetAllAsync(noteTypeId);
                if (!resAttributes.IsValid)
                {
                    result.AddListErrorMessage(resAttributes.ListErrorMessage);
                    return Guid.Empty;
                }
                return resAttributes.Entity?.FirstOrDefault(a => a.NoteTypeId == noteTypeId && a.Name == name)?.KAttributeId
                    ?? Guid.Empty;
            },
            create: async () =>
            {
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
            });

    // With several folders of that name (the old race created copies), always the oldest one, so new notes don't
    // spread over the copies.
    public static Task<FolderInfoDto> EnsureFolderAsync(IKntService service, string name, string bypassReason,
        ResultBase result) =>
        FindOrCreateAsync(service, result,
            find: async () =>
            {
                var resFolders = await service.Folders.GetAllAsync();
                if (!resFolders.IsValid)
                {
                    result.AddListErrorMessage(resFolders.ListErrorMessage);
                    return null;
                }
                return resFolders.Entity?.Where(f => f.Name == name).OrderBy(f => f.FolderNumber).FirstOrDefault();
            },
            create: async () =>
            {
                Result<FolderDto> resSave;
                using (KntAuthorizationBypass.Begin(service, bypassReason))
                {
                    resSave = await service.Folders.SaveAsync(new FolderDto { Name = name });
                }
                if (!resSave.IsValid)
                    result.AddListErrorMessage(resSave.ListErrorMessage);
                return (FolderInfoDto)resSave.Entity;
            });

    // What find finds or, when it is missing, what create makes. Only one caller at a time creates, per database; the
    // ones that waited find what it created. find and create add their errors to result (and give default).
    private static async Task<T> FindOrCreateAsync<T>(IKntService service, ResultBase result, Func<Task<T>> find,
        Func<Task<T>> create)
    {
        var found = await find();
        if (!result.IsValid || !IsMissing(found))
            return found;

        var creationLock = CreationLocks.GetOrAdd(service.RepositoryRef?.ConnectionString ?? "", _ => new SemaphoreSlim(1, 1));
        await creationLock.WaitAsync();
        try
        {
            // Created meanwhile by the caller that held the lock.
            found = await find();
            if (!result.IsValid || !IsMissing(found))
                return found;

            try
            {
                return await create();
            }
            catch (Exception)
            {
                // Created first by another process using the same database (the lock is per process): it is there now.
                found = await find();
                if (result.IsValid && !IsMissing(found))
                    return found;
                throw;
            }
        }
        finally
        {
            creationLock.Release();
        }
    }

    private static bool IsMissing<T>(T value) => EqualityComparer<T>.Default.Equals(value, default);

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
