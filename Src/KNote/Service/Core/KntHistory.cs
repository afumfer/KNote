using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using Microsoft.Extensions.Logging;

namespace KNote.Service.Core;

/// <summary>
/// The single point that notes the changes of the notes in their history (KLogs, 03-Historial.md). The commands that change a
/// note's own data (save, delete, move, retag) ask it for the row of the change (Prepare*), compared with what is stored, and give
/// it to the repository, which writes it in the same transaction as the change (H10): if it can't be noted, nothing changes.
/// Then (After*, which never fails the change) it applies the retention of the note's versions and empties the trash.
/// It notes nothing of the notes KNote writes by itself (KntConst.HistoryExcludedNoteTypeNames).
/// </summary>
public static class KntHistory
{
    private const int MaxFolderDepth = 64;

    /// <summary>The row of saving edited (stored: how it is now, null if it is new), or null if there is nothing to note.</summary>
    internal static Task<KLogWriteDto> PrepareSaveAsync(IKntService service, NoteDto stored, NoteDto edited) =>
        PrepareChangeAsync(service, edited.NoteId, stored, NoteVersionDto.From(edited),
            stored == null ? null : HistoryRules.NoteTitle(stored.NoteNumber, edited.Topic),
            (stored?.KAttributesDto ?? new()).Concat(edited.KAttributesDto ?? new()));

    /// <summary>The row of a change of a stored note to after (moved, retagged), or null if there is nothing to note.</summary>
    internal static Task<KLogWriteDto> PreparePatchAsync(IKntService service, NoteDto stored, NoteVersionDto after) =>
        stored == null ? Task.FromResult<KLogWriteDto>(null)
        : PrepareChangeAsync(service, stored.NoteId, stored, after, HistoryRules.NoteTitle(stored.NoteNumber, after.Topic),
            stored.KAttributesDto ?? new());

    /// <summary>The row of deleting a note, with its last version (the trash, H8), or null if it is not noted.</summary>
    internal static async Task<KLogWriteDto> PrepareDeleteAsync(IKntService service, NoteDto stored)
    {
        if (stored == null)
            return null;

        var types = await NoteTypeNamesAsync(service);
        if (IsExcluded(types, stored.NoteTypeId))
            return null;

        var last = NoteVersionDto.From(stored);
        var names = new HistoryNames();
        await AddFolderAsync(service, names, last.FolderId);
        var details = NoteChangeDescriber.DescribeDeleted(last, names);
        var json = last.ToJson();
        var retention = await RetentionAsync(service);
        var now = HistoryRules.Timestamp(DateTime.Now);

        return new KLogWriteDto
        {
            Row = NewRow(service, stored.NoteId, EnumHistoryAction.Deleted, now, details, HistoryRules.NoteTitle(stored.NoteNumber, stored.Topic)),
            PreviousContent = HistoryRules.FitsCopy(HistoryRules.ContentSize(json), retention) ? json : null
        };
    }

    /// <summary>After a change of a note was written: the retention of its versions, if one was kept. Never fails.</summary>
    internal static async Task AfterChangeAsync(IKntService service, Guid noteId, KLogWriteDto write)
    {
        if (write == null || (write.PreviousContent == null && write.StandalonePreviousContent == null))
            return;
        try
        {
            var retention = await RetentionAsync(service);
            var rows = (await service.Repository.History.GetEntityHistoryAsync(HistoryEntities.Note, noteId)).Entity;
            var empty = HistoryRules.CopiesToEmpty(rows, retention, DateTime.Now);
            if (empty.Count > 0)
                await service.Repository.History.ClearPreviousContentAsync(empty);
        }
        catch (Exception ex)
        {
            service.Logger?.LogError(ex, "The retention of the versions of the note {noteId} could not be applied.", noteId);
        }
    }

    /// <summary>After a note was deleted: the trash (the versions of the notes deleted DeletedNotesDays ago). Never fails.</summary>
    internal static Task AfterDeleteAsync(IKntService service, KLogWriteDto write) =>
        write == null ? Task.CompletedTask : PurgeDeletedNotesAsync(service);

    /// <summary>
    /// Empties every version of the notes deleted more than DeletedNotesDays ago (H8, H9); their rows stay. At every delete and
    /// when a host opens the repository (ServiceRef, the Server's KntRepositories). Never fails: an error goes to the log.
    /// Returns how many notes lost their versions.
    /// </summary>
    public static async Task<int> PurgeDeletedNotesAsync(IKntService service)
    {
        try
        {
            var retention = await RetentionAsync(service);
            var limit = HistoryRules.DeletedNotesLimit(retention, DateTime.Now);
            var noteIds = (await service.Repository.History.GetDeletedWithContentAsync(HistoryEntities.Note, limit)).Entity ?? new();
            foreach (var noteId in noteIds)
                await service.Repository.History.ClearEntityContentAsync(HistoryEntities.Note, noteId);
            if (noteIds.Count > 0)
                service.Logger?.LogInformation("History: the versions of {count} notes deleted before {limit} were emptied.", noteIds.Count, limit);
            return noteIds.Count;
        }
        catch (Exception ex)
        {
            service.Logger?.LogError(ex, "The versions of the deleted notes could not be emptied.");
            return 0;
        }
    }

    /// <summary>The retention of the repository (SYSTEM/HISTORY_RETENTION), or the default one.</summary>
    internal static async Task<HistoryRetentionDto> RetentionAsync(IKntService service)
    {
        var value = await service.Repository.SystemValues.GetAsync(KntConst.SystemScope, KntConst.HistoryRetentionKey);
        return HistoryRetentionDto.FromJson(value?.Entity?.Value);
    }

    private static async Task<KLogWriteDto> PrepareChangeAsync(IKntService service, Guid noteId, NoteDto stored, NoteVersionDto after,
        string title, IEnumerable<NoteKAttributeDto> attributes)
    {
        var types = await NoteTypeNamesAsync(service);
        if (IsExcluded(types, stored?.NoteTypeId) || IsExcluded(types, after.NoteTypeId))
            return null;

        var before = NoteVersionDto.From(stored);
        var names = new HistoryNames();
        foreach (var type in types)
            names.NoteTypes[type.Key] = type.Value;
        foreach (var attribute in attributes.Where(a => !string.IsNullOrEmpty(a.Name)))
            names.Attributes[attribute.KAttributeId] = attribute.Name;
        if (before != null)
            await AddFolderAsync(service, names, before.FolderId);
        await AddFolderAsync(service, names, after.FolderId);

        var details = NoteChangeDescriber.Describe(before, after, names);
        var action = before == null ? EnumHistoryAction.Created : EnumHistoryAction.Updated;
        if (action == EnumHistoryAction.Updated && details.Fields == EnumHistoryFields.None)
            return null;

        var retention = await RetentionAsync(service);
        string beforeJson = null;
        if (before != null)
        {
            var json = before.ToJson();
            if (HistoryRules.FitsCopy(HistoryRules.ContentSize(json), retention))
                beforeJson = json;
        }

        var now = HistoryRules.Timestamp(DateTime.Now);
        var standalone = NewRow(service, noteId, action, now, details, title);
        var write = new KLogWriteDto { Row = standalone, PreviousContent = beforeJson, TitleFromNewNote = before == null };

        var last = (await service.Repository.History.GetLastAsync(HistoryEntities.Note, noteId)).Entity;
        if (!HistoryRules.CanGroup(last, service.UserIdentityName, KntHistoryOrigin.Current(service), now))
            return write;

        // Into the row of its session (H5): described again from how the note was when it started, if the row keeps it.
        NoteChangeDetailsDto sessionDetails;
        string sessionContent = null;
        if (last.ActionType == nameof(EnumHistoryAction.Created))
            sessionDetails = NoteChangeDescriber.Describe(null, after, names);
        else
        {
            var sessionStart = last.HasPreviousContent
                ? NoteVersionDto.FromJson((await service.Repository.History.GetPreviousContentAsync(last.KLogId)).Entity)
                : null;
            if (sessionStart != null)
            {
                await AddFolderAsync(service, names, sessionStart.FolderId);
                sessionDetails = NoteChangeDescriber.Describe(sessionStart, after, names);
            }
            else
            {
                sessionDetails = NoteChangeDescriber.Merge(NoteChangeDetailsDto.FromJson(last.ChangeDetails), details);
                // The session had no version yet (or it was too big): this one is how the note was before it.
                if (!last.HasPreviousContent)
                    sessionContent = beforeJson;
            }
        }

        var merged = new KLogDto
        {
            KLogId = last.KLogId,
            EntityName = last.EntityName,
            EntityId = last.EntityId,
            RegistryDateTime = last.RegistryDateTime,
            ActionType = last.ActionType,
            UserName = last.UserName,
            Origin = last.Origin,
            LastDateTime = now,
            Fields = (last.Fields & ~EnumHistoryFields.NoteData) | sessionDetails.Fields,
            ChangeDetails = sessionDetails.ToJson(),
            RegistryMessage = NoteChangeDescriber.Render(sessionDetails),
            EntityTitle = title ?? last.EntityTitle
        };

        return new KLogWriteDto
        {
            Row = merged,
            PreviousContent = sessionContent,
            MergeIntoKLogId = last.KLogId,
            ExpectedLastDateTime = last.LastDateTime,
            Standalone = standalone,
            StandalonePreviousContent = beforeJson,
            TitleFromNewNote = before == null
        };
    }

    private static KLogDto NewRow(IKntService service, Guid noteId, EnumHistoryAction action, DateTime now, NoteChangeDetailsDto details,
        string title) => new()
        {
            KLogId = Guid.NewGuid(),
            EntityName = HistoryEntities.Note,
            EntityId = noteId,
            RegistryDateTime = now,
            LastDateTime = now,
            ActionType = action.ToString(),
            UserName = HistoryRules.Truncate(service.UserIdentityName, KLogDto.MaxUserNameLength),
            Origin = KntHistoryOrigin.Current(service).ToString(),
            Fields = details.Fields,
            ChangeDetails = details.ToJson(),
            RegistryMessage = NoteChangeDescriber.Render(details),
            EntityTitle = title
        };

    private static bool IsExcluded(Dictionary<Guid, string> noteTypes, Guid? noteTypeId) =>
        noteTypeId is Guid id && noteTypes.TryGetValue(id, out var name) && KntConst.HistoryExcludedNoteTypeNames.Contains(name);

    private static async Task<Dictionary<Guid, string>> NoteTypeNamesAsync(IKntService service)
    {
        var types = (await service.Repository.NoteTypes.GetAllAsync()).Entity ?? new();
        return types.GroupBy(t => t.NoteTypeId).ToDictionary(g => g.Key, g => g.First().Name);
    }

    // The path of a folder (Home\Projects\KNote), read from the repository: the history needs no role of its own.
    private static async Task AddFolderAsync(IKntService service, HistoryNames names, Guid folderId)
    {
        if (names.Folders.ContainsKey(folderId))
            return;

        var path = new List<string>();
        Guid? id = folderId;
        while (id is Guid current && path.Count < MaxFolderDepth)
        {
            var folder = (await service.Repository.Folders.GetAsync(current)).Entity;
            if (folder == null)
                break;
            path.Insert(0, folder.Name);
            id = folder.ParentId;
        }
        if (path.Count > 0)
            names.Folders[folderId] = string.Join("\\", path);
    }
}
