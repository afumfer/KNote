using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using Microsoft.Extensions.Logging;

namespace KNote.Service.Core;

/// <summary>
/// The single point that notes the changes of the notes in their history (KLogs, 03-Historial.md). The commands that change a
/// note's own data (save, delete, move, retag) or one of its parts (tasks, resources, alarms, relations: rows of their note, H3)
/// ask it for the row of the change (Prepare*), compared with what is stored, and give it to the repository, which writes it in
/// the same transaction as the change (H10): if it can't be noted, nothing changes.
/// Then (After*, which never fails the change) it applies the retention of the note's versions and empties the trash.
/// It notes nothing of the notes KNote writes by itself (KntConst.HistoryExcludedNoteTypeNames).
/// </summary>
public static class KntHistory
{
    private const int MaxFolderDepth = 64;

    private static readonly AsyncLocal<bool> _notNoted = new();
    private static readonly AsyncLocal<DeletingNoteScope> _deletingNote = new();
    private static readonly AsyncLocal<HistoryRestoredDto> _restoring = new();

    #region Scopes

    /// <summary>
    /// What KNote writes by itself to keep things going, not noted while the scope lasts: the alarm of an email that couldn't be
    /// sent (its comment, or the alarm as it was, to be sent again). Like KntHistoryOrigin, it flows with the async calls.
    /// </summary>
    public static IDisposable NotNoted()
    {
        var previous = _notNoted.Value;
        _notNoted.Value = true;
        return new Restore(() => _notNoted.Value = previous);
    }

    /// <summary>
    /// While a note and its parts are deleted (KntNotesDeleteExtendedAsyncCommand): its parts are not noted one by one, its row
    /// Deleted says how many it had, and each note related to it notes that it lost that relation (H14).
    /// </summary>
    internal static IDisposable DeletingNote(Guid noteId)
    {
        var previous = _deletingNote.Value;
        _deletingNote.Value = new DeletingNoteScope(noteId);
        return new Restore(() => _deletingNote.Value = previous);
    }

    /// <summary>
    /// While a version is restored (KntHistoryRestoreNoteVersionAsyncCommand, H23): saving the note leaves a row Restored of its own
    /// (never merged into a session), with its copy of how the note was before, so restoring can be undone. With Recovered, a deleted
    /// note created again (KntHistoryRecoverNoteAsyncCommand, H24): its row Restored, without a copy (there was no note).
    /// </summary>
    internal static IDisposable Restoring(HistoryRestoredDto restored)
    {
        var previous = _restoring.Value;
        _restoring.Value = restored;
        return new Restore(() => _restoring.Value = previous);
    }

    private sealed class DeletingNoteScope
    {
        public DeletingNoteScope(Guid noteId) => NoteId = noteId;

        public Guid NoteId { get; }
        private int _tasks, _resources, _alarms, _relations;

        public void Count(EnumHistoryFields part)
        {
            switch (part)
            {
                case EnumHistoryFields.Tasks: Interlocked.Increment(ref _tasks); break;
                case EnumHistoryFields.Resources: Interlocked.Increment(ref _resources); break;
                case EnumHistoryFields.Alarms: Interlocked.Increment(ref _alarms); break;
                case EnumHistoryFields.Relations: Interlocked.Increment(ref _relations); break;
            }
        }

        public void CountInto(HistoryDeletedDto deleted)
        {
            if (deleted == null)
                return;
            deleted.Tasks = _tasks > 0 ? _tasks : null;
            deleted.Resources = _resources > 0 ? _resources : null;
            deleted.Alarms = _alarms > 0 ? _alarms : null;
            deleted.Relations = _relations > 0 ? _relations : null;
        }
    }

    private sealed class Restore : IDisposable
    {
        private Action _restore;
        public Restore(Action restore) => _restore = restore;
        public void Dispose()
        {
            _restore?.Invoke();
            _restore = null;
        }
    }

    #endregion

    #region The parts of a note (H3)

    /// <summary>The row of a task added (stored null), removed (after null) or changed, or null if there is nothing to note.</summary>
    internal static async Task<KLogWriteDto> PrepareTaskAsync(IKntService service, NoteTaskDto stored, NoteTaskDto after)
    {
        var task = after ?? stored;
        if (task == null || SkipPart(task.NoteId, EnumHistoryFields.Tasks))
            return null;
        var names = new HistoryNames();
        await AddUsersAsync(service, names, stored?.UserId, after?.UserId);
        return await PreparePartAsync(service, task.NoteId, EnumHistoryFields.Tasks, NotePartChangeDescriber.Task(stored, after, names));
    }

    /// <summary>The row of a resource saved or removed by a person; newFile: it came with a file of its own.</summary>
    internal static async Task<KLogWriteDto> PrepareResourceAsync(IKntService service, ResourceInfoDto stored, ResourceInfoDto after, bool newFile)
    {
        var resource = after ?? stored;
        if (resource == null || SkipPart(resource.NoteId, EnumHistoryFields.Resources))
            return null;
        return await PreparePartAsync(service, resource.NoteId, EnumHistoryFields.Resources,
            NotePartChangeDescriber.Resource(stored, after, newFile));
    }

    /// <summary>The row of an alarm added, removed or changed by a person (not its reprogramming when due).</summary>
    internal static async Task<KLogWriteDto> PrepareAlarmAsync(IKntService service, KMessageDto stored, KMessageDto after)
    {
        if ((after ?? stored)?.NoteId is not Guid noteId || SkipPart(noteId, EnumHistoryFields.Alarms))
            return null;
        var names = new HistoryNames();
        await AddUsersAsync(service, names, stored?.UserId, after?.UserId);
        return await PreparePartAsync(service, noteId, EnumHistoryFields.Alarms, NotePartChangeDescriber.Alarm(stored, after, names));
    }

    /// <summary>
    /// The rows of a relation added, removed or changed: one in each of its notes. None for the ones KNote makes (@DerivedFrom of
    /// a summary or a translation); while a note is deleted, only the other one notes it (H14).
    /// </summary>
    internal static async Task<IReadOnlyList<KLogWriteDto>> PrepareRelationAsync(IKntService service, TraceNoteDto stored, TraceNoteDto after)
    {
        var writes = new List<KLogWriteDto>();
        var relation = after ?? stored;
        if (relation == null || _notNoted.Value)
            return writes;

        var names = new HistoryNames();
        foreach (var typeId in new[] { stored?.TraceNoteTypeId, after?.TraceNoteTypeId }.OfType<Guid>().Distinct())
        {
            var type = (await service.Repository.TraceNoteTypes.GetAsync(typeId)).Entity;
            if (type?.Name == KntConst.DerivedFromTraceNoteType)
                return writes;
            if (type != null)
                names.TraceNoteTypes[typeId] = type.Name;
        }

        var deleting = _deletingNote.Value;
        var sides = relation.FromId == relation.ToId
            ? new[] { (Note: relation.FromId, Other: relation.ToId) }
            : new[] { (Note: relation.FromId, Other: relation.ToId), (Note: relation.ToId, Other: relation.FromId) };
        foreach (var (noteId, otherId) in sides)
        {
            if (deleting?.NoteId == noteId)
            {
                deleting.Count(EnumHistoryFields.Relations);
                continue;
            }
            var other = (await service.Repository.Notes.GetAsync(otherId)).Entity;
            var otherTitle = other == null ? "(unknown note)" : HistoryRules.NoteTitle(other.NoteNumber, other.Topic);
            var change = NotePartChangeDescriber.Relation(stored, after, otherTitle, names, otherNoteDeleted: deleting?.NoteId == otherId);
            if (await PreparePartAsync(service, noteId, EnumHistoryFields.Relations, change) is { } write)
                writes.Add(write);
        }
        return writes;
    }

    // Not noted now (KNote's own upkeep), or counted in the row of the note being deleted.
    private static bool SkipPart(Guid noteId, EnumHistoryFields part)
    {
        if (_notNoted.Value)
            return true;
        if (_deletingNote.Value is { } deleting && deleting.NoteId == noteId)
        {
            deleting.Count(part);
            return true;
        }
        return false;
    }

    // The row of a change of a part of a note: a new one, or into the row of its session (H5). Never a version: a part is not
    // the note's own data (H6).
    private static async Task<KLogWriteDto> PreparePartAsync(IKntService service, Guid noteId, EnumHistoryFields part, HistoryPartChangeDto change)
    {
        if (change == null || noteId == Guid.Empty)
            return null;

        var note = (await service.Repository.Notes.GetAsync(noteId)).Entity;
        if (note == null)
            return null;
        var types = await NoteTypeNamesAsync(service);
        if (IsExcluded(types, note.NoteTypeId))
            return null;

        var details = NotePartChangeDescriber.Details(part, change);
        var title = HistoryRules.NoteTitle(note.NoteNumber, note.Topic);
        var now = HistoryRules.Timestamp(DateTime.Now);
        var standalone = NewRow(service, noteId, EnumHistoryAction.Updated, now, details, title);

        var last = (await service.Repository.History.GetLastAsync(HistoryEntities.Note, noteId)).Entity;
        if (!HistoryRules.CanGroup(last, service.UserIdentityName, KntHistoryOrigin.Current(service), now))
            return new KLogWriteDto { Row = standalone };

        var sessionDetails = NoteChangeDescriber.Merge(NoteChangeDetailsDto.FromJson(last.ChangeDetails), details);
        return new KLogWriteDto
        {
            Row = MergedRow(last, now, sessionDetails, title),
            MergeIntoKLogId = last.KLogId,
            ExpectedLastDateTime = last.LastDateTime,
            Standalone = standalone
        };
    }

    #endregion

    #region The structure of the repository (H.2b)

    /// <summary>The titles of the settings of the history (EntityTitle of their SystemValue rows).</summary>
    public const string RepositoryNameTitle = "Repository name";
    public const string AiModelsTitle = "AI models";
    public const string NoisePolicyTitle = "Noise policy";
    public const string HistoryRetentionTitle = "History retention";

    /// <summary>
    /// The row of a change of an entity of the structure (HistoryEntities), or null if nothing changed: a new one, or into the
    /// row of its session (H5); a deletion is always a row of its own. userName: who, when the service has no user (registering,
    /// «Forgot password»). Never fails the change: KntAttributes writes it in its transaction, the rest just after (WriteAsync).
    /// </summary>
    internal static async Task<KLogWriteDto> PrepareStructureAsync(IKntService service, string entityName, Guid entityId, string title,
        StructureChangeDetailsDto change, string userName = null)
    {
        if (change == null || change.IsEmpty || entityId == Guid.Empty || _notNoted.Value)
            return null;

        var who = HistoryRules.Truncate(string.IsNullOrWhiteSpace(service.UserIdentityName) ? userName : service.UserIdentityName,
            KLogDto.MaxUserNameLength);
        var origin = KntHistoryOrigin.Current(service);
        var now = HistoryRules.Timestamp(DateTime.Now);
        var action = change.Created == true ? EnumHistoryAction.Created
            : change.Deleted == true ? EnumHistoryAction.Deleted : EnumHistoryAction.Updated;
        var standalone = new KLogDto
        {
            KLogId = Guid.NewGuid(),
            EntityName = entityName,
            EntityId = entityId,
            RegistryDateTime = now,
            LastDateTime = now,
            ActionType = action.ToString(),
            UserName = who,
            Origin = origin.ToString(),
            Fields = EnumHistoryFields.None,
            ChangeDetails = change.ToJson(),
            RegistryMessage = StructureChangeDescriber.Render(entityName, change),
            EntityTitle = HistoryRules.Title(title)
        };
        if (action == EnumHistoryAction.Deleted)
            return new KLogWriteDto { Row = standalone };

        var last = (await service.Repository.History.GetLastAsync(entityName, entityId)).Entity;
        if (!HistoryRules.CanGroup(last, who, origin, now))
            return new KLogWriteDto { Row = standalone };

        var merged = StructureChangeDescriber.Merge(StructureChangeDetailsDto.FromJson(last.ChangeDetails), change);
        return new KLogWriteDto
        {
            Row = new KLogDto
            {
                KLogId = last.KLogId,
                EntityName = last.EntityName,
                EntityId = last.EntityId,
                RegistryDateTime = last.RegistryDateTime,
                ActionType = last.ActionType,
                UserName = last.UserName,
                Origin = last.Origin,
                LastDateTime = now,
                Fields = EnumHistoryFields.None,
                ChangeDetails = merged.ToJson(),
                RegistryMessage = StructureChangeDescriber.Render(entityName, merged),
                EntityTitle = HistoryRules.Title(title) ?? last.EntityTitle
            },
            MergeIntoKLogId = last.KLogId,
            ExpectedLastDateTime = last.LastDateTime,
            Standalone = standalone
        };
    }

    /// <summary>
    /// Notes a change of the structure just after it was made (H10: those changes have no transaction of their own). Never
    /// fails it: an error goes to the log.
    /// </summary>
    internal static async Task NoteStructureAsync(IKntService service, string entityName, Guid entityId, string title,
        StructureChangeDetailsDto change, string userName = null)
    {
        try
        {
            var write = await PrepareStructureAsync(service, entityName, entityId, title, change, userName);
            if (write != null)
                await service.Repository.History.WriteAsync(write);
        }
        catch (Exception ex)
        {
            service.Logger?.LogError(ex, "The change of {entityName} {title} could not be noted in the history.", entityName, title);
        }
    }

    /// <summary>The path of a folder (Home\Projects\KNote), or null.</summary>
    internal static async Task<string> FolderPathAsync(IKntService service, Guid? folderId)
    {
        if (folderId is not Guid id || id == Guid.Empty)
            return null;
        var names = new HistoryNames();
        await AddFolderAsync(service, names, id);
        return names.Folders.TryGetValue(id, out var path) ? path : null;
    }

    /// <summary>The names of the note types of the repository, for the history of the structure.</summary>
    internal static async Task<HistoryNames> NoteTypeNamesForHistoryAsync(IKntService service)
    {
        var names = new HistoryNames();
        foreach (var type in await NoteTypeNamesAsync(service))
            names.NoteTypes[type.Key] = type.Value;
        return names;
    }

    #endregion

    #region Helpers

    // The names of the users of a task or an alarm, read from the repository (the history needs no role of its own).
    private static async Task AddUsersAsync(IKntService service, HistoryNames names, params Guid?[] userIds)
    {
        foreach (var userId in userIds.OfType<Guid>().Where(id => id != Guid.Empty).Distinct())
        {
            var user = (await service.Repository.Users.GetAsync(userId)).Entity;
            if (user != null)
                names.Users[userId] = string.IsNullOrWhiteSpace(user.FullName) ? user.UserName : user.FullName;
        }
    }

    #endregion

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
        if (_deletingNote.Value is { } deleting && deleting.NoteId == stored.NoteId)
            deleting.CountInto(details.Deleted);
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
        // A version restored onto the note, or a deleted note recovered (created again, Recovered): a row Restored (H23, H24).
        var restoring = _restoring.Value;
        if (restoring != null && (before == null) != (restoring.Recovered == true))
            restoring = null;
        if (restoring != null)
        {
            if (before == null)
                details = new NoteChangeDetailsDto { Restored = restoring };
            else
                details.Restored = restoring;
            action = EnumHistoryAction.Restored;
        }

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
        // A restore is a row of its own, with its copy (H23).
        if (restoring != null)
            return write;

        var last = (await service.Repository.History.GetLastAsync(HistoryEntities.Note, noteId)).Entity;
        if (!HistoryRules.CanGroup(last, service.UserIdentityName, KntHistoryOrigin.Current(service), now))
            return write;

        // Into the row of its session (H5): described again from how the note was when it started, if the row keeps it.
        // Its own data described again: the parts of the session (tasks, resources...) stay as they were.
        var lastDetails = NoteChangeDetailsDto.FromJson(last.ChangeDetails);
        NoteChangeDetailsDto sessionDetails;
        string sessionContent = null;
        if (last.ActionType == nameof(EnumHistoryAction.Created))
            sessionDetails = NotePartChangeDescriber.KeepParts(NoteChangeDescriber.Describe(null, after, names), lastDetails);
        else
        {
            var sessionStart = last.HasPreviousContent
                ? NoteVersionDto.FromJson((await service.Repository.History.GetPreviousContentAsync(last.KLogId)).Entity)
                : null;
            if (sessionStart != null)
            {
                await AddFolderAsync(service, names, sessionStart.FolderId);
                sessionDetails = NotePartChangeDescriber.KeepParts(NoteChangeDescriber.Describe(sessionStart, after, names), lastDetails);
            }
            else
            {
                sessionDetails = NoteChangeDescriber.Merge(lastDetails, details);
                // The session had no version yet (or it was too big): this one is how the note was before it.
                if (!last.HasPreviousContent)
                    sessionContent = beforeJson;
            }
        }

        return new KLogWriteDto
        {
            Row = MergedRow(last, now, sessionDetails, title),
            PreviousContent = sessionContent,
            MergeIntoKLogId = last.KLogId,
            ExpectedLastDateTime = last.LastDateTime,
            Standalone = standalone,
            StandalonePreviousContent = beforeJson,
            TitleFromNewNote = before == null
        };
    }

    // The row of a session (last) with this change in it: its details as they are now.
    private static KLogDto MergedRow(KLogDto last, DateTime now, NoteChangeDetailsDto sessionDetails, string title) => new()
    {
        KLogId = last.KLogId,
        EntityName = last.EntityName,
        EntityId = last.EntityId,
        RegistryDateTime = last.RegistryDateTime,
        ActionType = last.ActionType,
        UserName = last.UserName,
        Origin = last.Origin,
        LastDateTime = now,
        Fields = sessionDetails.Fields,
        ChangeDetails = sessionDetails.ToJson(),
        RegistryMessage = NoteChangeDescriber.Render(sessionDetails),
        EntityTitle = title ?? last.EntityTitle
    };

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
