using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.Service.ServicesCommands;

/// <summary>A page of the history of a note, the newest first, without the copies (the lists never read them).</summary>
[KntAuthorize(EnumRoles.Guest)]
public class KntHistoryGetNoteHistoryAsyncCommand : KntCommandServiceBase<(Guid NoteId, int Skip, int Take), Result<NoteHistoryPageDto>>
{
    public KntHistoryGetNoteHistoryAsyncCommand(IKntService service, Guid noteId, int skip, int take) : base(service, (noteId, skip, take))
    {

    }

    public override async Task<Result<NoteHistoryPageDto>> Execute()
    {
        var result = new Result<NoteHistoryPageDto>();
        var rows = await Repository.History.GetEntityHistoryAsync(HistoryEntities.Note, Param.NoteId);
        if (!rows.IsValid)
        {
            result.AddListErrorMessage(rows.ListErrorMessage);
            return result;
        }

        var all = rows.Entity ?? new();
        var skip = Math.Max(0, Param.Skip);
        var take = Param.Take <= 0 ? NoteHistoryPageDto.PageSize : Math.Min(Param.Take, NoteHistoryPageDto.PageSize);
        var page = all.Skip(skip).Take(take).ToList();
        var restorable = HistoryRules.Restorable(all);
        result.Entity = new NoteHistoryPageDto
        {
            NoteId = Param.NoteId,
            Rows = page,
            Total = all.Count,
            Skip = skip,
            Restorable = page.Where(r => restorable.Contains(r.KLogId)).Select(r => r.KLogId).ToList()
        };
        return result;
    }
}

/// <summary>
/// The note as it was after a change of its history (H18, HistoryRules.VersionAfter): the copy of a later row or the note as it
/// is now, with the path of its folder, the names of its type and of its attributes («(deleted attribute)» for one that no longer
/// exists) and without its script (H21).
/// </summary>
[KntAuthorize(EnumRoles.Guest)]
public class KntHistoryGetNoteVersionAsyncCommand : KntCommandServiceBase<(Guid NoteId, Guid KLogId), Result<NoteVersionViewDto>>
{
    public const string NotInHistory = "This change is not in the history of the note.";
    public const string CopyNotKept = "The copy of this version is no longer kept.";

    public KntHistoryGetNoteVersionAsyncCommand(IKntService service, Guid noteId, Guid klogId) : base(service, (noteId, klogId))
    {

    }

    public override async Task<Result<NoteVersionViewDto>> Execute()
    {
        var result = new Result<NoteVersionViewDto>();
        var rows = (await Repository.History.GetEntityHistoryAsync(HistoryEntities.Note, Param.NoteId)).Entity ?? new();
        var row = rows.FirstOrDefault(r => r.KLogId == Param.KLogId);
        if (row == null)
        {
            result.AddErrorMessage(NotInHistory);
            return result;
        }

        var note = (await Repository.Notes.GetAsync(Param.NoteId)).Entity;
        var (source, version) = await KntHistoryVersions.AfterAsync(Service, rows, row.KLogId, note);
        if (version == null)
        {
            result.AddErrorMessage(CopyNotKept);
            return result;
        }

        var view = new NoteVersionViewDto
        {
            NoteId = Param.NoteId,
            KLogId = row.KLogId,
            NoteNumber = note?.NoteNumber ?? NumberOf(row.EntityTitle),
            AsOf = row.LastDateTime,
            UserName = row.UserName,
            Change = row.RegistryMessage,
            Exact = source.Exact,
            IncludesUntil = source.IncludesUntil,
            Notice = HistoryRules.VersionNotice(source.IncludesUntil),
            Topic = version.Topic,
            FolderPath = await KntHistory.FolderPathAsync(Service, version.FolderId) ?? "(unknown folder)",
            NoteTypeName = version.NoteTypeId is Guid typeId ? (await Repository.NoteTypes.GetAsync(typeId)).Entity?.Name ?? "(deleted type)" : null,
            Tags = version.Tags,
            Priority = version.Priority,
            DescriptionFormat = ContentTypeExtJsonHelper.Read(version.ContentType).ForDescription,
            Description = version.Description
        };
        foreach (var attribute in version.Attributes)
        {
            var stored = await Repository.KAttributes.GetAsync(attribute.Id);
            view.Attributes.Add(new NoteVersionAttributeViewDto
            {
                Name = stored.IsValid && stored.Entity != null ? stored.Entity.Name : "(deleted attribute)",
                Value = attribute.Value
            });
        }

        result.Entity = view;
        return result;
    }

    // The number of a note from the title of its row (#340 Topic), for one that no longer exists.
    internal static int NumberOf(string title) => HistoryRules.NoteNumberOf(title);
}

/// <summary>
/// «Compare» (H12, H22): what a change did (the note before that row and after it) or that version against the note now. If the
/// copy before the change is gone, it compares with the note now and says so. The fields with their names resolved; the script,
/// only whether it changed (H21).
/// </summary>
[KntAuthorize(EnumRoles.Guest)]
public class KntHistoryGetNoteComparisonAsyncCommand : KntCommandServiceBase<(Guid NoteId, Guid KLogId, bool WithCurrentNote), Result<NoteComparisonDto>>
{
    public const string NothingToCompare = "This change didn't touch the note's own data: there is nothing to compare.";
    public const string NoteGone = "The note no longer exists: it can't be compared with the note now.";
    public const string BeforeNotKept = "The copy of the note before this change is no longer kept: it is compared with the note now.";

    public KntHistoryGetNoteComparisonAsyncCommand(IKntService service, Guid noteId, Guid klogId, bool withCurrentNote)
        : base(service, (noteId, klogId, withCurrentNote))
    {

    }

    public override async Task<Result<NoteComparisonDto>> Execute()
    {
        var result = new Result<NoteComparisonDto>();
        var rows = (await Repository.History.GetEntityHistoryAsync(HistoryEntities.Note, Param.NoteId)).Entity ?? new();
        var row = rows.FirstOrDefault(r => r.KLogId == Param.KLogId);
        if (row == null)
        {
            result.AddErrorMessage(KntHistoryGetNoteVersionAsyncCommand.NotInHistory);
            return result;
        }
        if (!HistoryRules.CanCompare(row))
        {
            result.AddErrorMessage(NothingToCompare);
            return result;
        }

        var note = (await Repository.Notes.GetAsync(Param.NoteId)).Entity;
        var (source, after) = await KntHistoryVersions.AfterAsync(Service, rows, row.KLogId, note);
        if (after == null)
        {
            result.AddErrorMessage(KntHistoryGetNoteVersionAsyncCommand.CopyNotKept);
            return result;
        }

        var notices = new List<string>();
        var beforeSource = HistoryRules.VersionBefore(row);
        NoteVersionDto before = null;
        if (beforeSource?.CopyRow != null)
            before = NoteVersionDto.FromJson((await Repository.History.GetPreviousContentAsync(beforeSource.CopyRow.KLogId)).Entity);
        var canCompareChange = beforeSource != null && (beforeSource.EmptyNote || before != null);

        var withCurrent = Param.WithCurrentNote;
        if (!withCurrent && !canCompareChange)
        {
            withCurrent = true;
            notices.Add(BeforeNotKept);
        }
        if (withCurrent && note == null)
        {
            result.AddErrorMessage(NoteGone);
            return result;
        }
        if (HistoryRules.VersionNotice(source.IncludesUntil) is string notice)
            notices.Add(notice);

        var left = withCurrent ? after : before;
        var right = withCurrent ? NoteVersionDto.From(note) : after;
        var comparison = NoteComparer.Compare(left, right, await KntHistoryVersions.NamesAsync(Service, left, right));
        comparison.NoteId = Param.NoteId;
        comparison.KLogId = row.KLogId;
        comparison.NoteNumber = note?.NoteNumber ?? KntHistoryGetNoteVersionAsyncCommand.NumberOf(row.EntityTitle);
        comparison.WithCurrentNote = withCurrent;
        comparison.CanCompareChange = canCompareChange;
        comparison.BeforeLabel = withCurrent ? "This version" : "Before the change";
        comparison.AfterLabel = withCurrent ? "The note now" : "After the change";
        comparison.Notice = notices.Count == 0 ? null : string.Join(" ", notices);
        result.Entity = comparison;
        return result;
    }
}

/// <summary>
/// «Restore» (H23): the note as it was after a change of its history (H18), saved as always (KntNotesSaveAsyncCommand: its rules,
/// its status, the index) in a row Restored of its own, with its copy of how the note was before, so it can be undone. A folder,
/// a type or an attribute that no longer exists leaves what the note has now, and the result says so. Not a deleted note (H24).
/// </summary>
[KntAuthorize(EnumRoles.Staff)]
public class KntHistoryRestoreNoteVersionAsyncCommand : KntCommandServiceBase<(Guid NoteId, Guid KLogId), Result<NoteRestoreResultDto>>
{
    public const string NoteDeleted = "The note was deleted: it is recovered from «Deleted notes».";
    public const string NothingToRestore = "This is how the note is now: there is nothing to restore.";
    public const string AlreadySo = "The note was already as in that version.";

    public KntHistoryRestoreNoteVersionAsyncCommand(IKntService service, Guid noteId, Guid klogId) : base(service, (noteId, klogId))
    {

    }

    public override async Task<Result<NoteRestoreResultDto>> Execute()
    {
        var result = new Result<NoteRestoreResultDto>();
        var rows = (await Repository.History.GetEntityHistoryAsync(HistoryEntities.Note, Param.NoteId)).Entity ?? new();
        var row = rows.FirstOrDefault(r => r.KLogId == Param.KLogId);
        if (row == null)
        {
            result.AddErrorMessage(KntHistoryGetNoteVersionAsyncCommand.NotInHistory);
            return result;
        }

        var stored = (await Repository.Notes.GetAsync(Param.NoteId)).Entity;
        if (stored == null || row.ActionType == nameof(EnumHistoryAction.Deleted))
        {
            result.AddErrorMessage(NoteDeleted);
            return result;
        }
        if (!HistoryRules.Restorable(rows).Contains(row.KLogId))
        {
            result.AddErrorMessage(NothingToRestore);
            return result;
        }

        var (source, version) = await KntHistoryVersions.AfterAsync(Service, rows, row.KLogId, stored);
        if (version == null || source.CopyRow == null)
        {
            result.AddErrorMessage(KntHistoryGetNoteVersionAsyncCommand.CopyNotKept);
            return result;
        }

        var restore = new NoteRestoreResultDto { NoteId = Param.NoteId };
        if (HistoryRules.VersionNotice(source.IncludesUntil) is string notice)
            restore.Notices.Add(notice);

        var note = (await Repository.Notes.GetAsync(Param.NoteId)).Entity;
        note.Topic = version.Topic;
        note.Description = version.Description;
        note.ContentType = version.ContentType;
        note.Tags = version.Tags;
        note.Priority = version.Priority;
        note.Script = version.Script;

        if (version.FolderId != note.FolderId)
        {
            var folder = await Repository.Folders.GetAsync(version.FolderId);
            if (folder.IsValid && folder.Entity != null && folder.Entity.FolderId == version.FolderId)
            {
                note.FolderId = folder.Entity.FolderId;
                note.FolderDto = folder.Entity;
            }
            else
                restore.Notices.Add($"The folder of that version no longer exists: the note stays in {await KntHistory.FolderPathAsync(Service, note.FolderId) ?? "its folder"}.");
        }

        if (version.NoteTypeId != note.NoteTypeId)
        {
            NoteTypeDto type = null;
            if (version.NoteTypeId is Guid typeId)
            {
                var found = await Repository.NoteTypes.GetAsync(typeId);
                type = found.IsValid && found.Entity?.NoteTypeId == typeId ? found.Entity : null;
            }
            if (version.NoteTypeId == null || type != null)
            {
                note.NoteTypeId = version.NoteTypeId;
                note.NoteTypeDto = type;
                // The attributes of the type it had go (as the editors do); those of the new one come next.
                note.KAttributesDto.RemoveAll(a => a.KAttributeNoteTypeId != null);
            }
            else
                restore.Notices.Add("The type of that version no longer exists: the note keeps its type.");
        }

        note.KAttributesDto = await Service.Notes.UtilCompleteNoteAttributesAsync(note.KAttributesDto ?? new(), note.NoteId, note.NoteTypeId)
            ?? note.KAttributesDto ?? new();
        foreach (var attribute in note.KAttributesDto)
            attribute.Value = version.AttributeValue(attribute.KAttributeId) ?? "";
        var lost = version.Attributes.Count(v => note.KAttributesDto.All(a => a.KAttributeId != v.Id));
        if (lost > 0)
            restore.Notices.Add(lost == 1
                ? "1 attribute of that version no longer exists or isn't of the note's type: it was not restored."
                : $"{lost} attributes of that version no longer exist or aren't of the note's type: they were not restored.");

        if (NoteChangeRules.ChangedField(stored, note) == null)
        {
            restore.Notices.Insert(0, AlreadySo);
            result.Entity = restore;
            return result;
        }

        Result<NoteDto> saved;
        using (KntHistory.Restoring(new HistoryRestoredDto { KLogId = row.KLogId, VersionOf = row.LastDateTime }))
            saved = await Service.Notes.SaveAsync(note);
        if (!saved.IsValid)
        {
            result.AddListErrorMessage(saved.ListErrorMessage);
            return result;
        }

        restore.Changed = true;
        result.Entity = restore;
        return result;
    }
}

/// <summary>
/// «Recover deleted notes» (H24): the notes whose last row is their deletion and still keep their last version (the trash, which
/// the purge empties after DeletedNotesDays), the newest first; not the ones that can no longer be recovered.
/// </summary>
[KntAuthorize(EnumRoles.Staff)]
public class KntHistoryGetDeletedNotesAsyncCommand : KntCommandServiceBase<Result<List<DeletedNoteDto>>>
{
    public KntHistoryGetDeletedNotesAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<DeletedNoteDto>>> Execute()
    {
        var result = new Result<List<DeletedNoteDto>>();
        var rows = await Repository.History.GetDeletedAsync(HistoryEntities.Note);
        if (!rows.IsValid)
        {
            result.AddListErrorMessage(rows.ListErrorMessage);
            return result;
        }
        result.Entity = (rows.Entity ?? new()).Select(row => new DeletedNoteDto
        {
            NoteId = row.EntityId,
            NoteNumber = HistoryRules.NoteNumberOf(row.EntityTitle),
            Topic = HistoryRules.NoteTopicOf(row.EntityTitle),
            FolderPath = NoteChangeDetailsDto.FromJson(row.ChangeDetails).Deleted?.Folder,
            DeletedAt = row.LastDateTime,
            DeletedBy = row.UserName,
            Origin = row.Origin,
            Row = row
        }).ToList();
        return result;
    }
}

/// <summary>
/// Recovers a deleted note (H24, H26): created again by the save of always (KntNotesSaveAsyncCommand: its rules, the index, the
/// history) with its own data as it was deleted (H8), in a row Restored («Recovered from the deleted notes»). The same id, so its
/// history goes on; its number if it is free (another one if not); its folder or, if it no longer exists, Home; its type and the
/// attributes that still exist; created when its row Created says, modified now. Its tasks, resources, alarms and relations don't
/// come back: the result says so.
/// </summary>
[KntAuthorize(EnumRoles.Staff)]
public class KntHistoryRecoverNoteAsyncCommand : KntCommandServiceBase<Guid, Result<NoteRecoverResultDto>>
{
    public const string NotDeleted = "This note is not among the deleted notes that can be recovered.";
    public const string PartsNotRecovered = "Its tasks, resources, alarms and relations are not recovered.";

    public KntHistoryRecoverNoteAsyncCommand(IKntService service, Guid noteId) : base(service, noteId)
    {

    }

    public override async Task<Result<NoteRecoverResultDto>> Execute()
    {
        var result = new Result<NoteRecoverResultDto>();
        var rows = (await Repository.History.GetEntityHistoryAsync(HistoryEntities.Note, Param)).Entity ?? new();
        var deleted = rows.OrderBy(r => r.RegistryDateTime).ThenBy(r => r.LastDateTime).LastOrDefault();
        if (deleted == null || deleted.ActionType != nameof(EnumHistoryAction.Deleted) || !deleted.HasPreviousContent
            || (await Repository.Notes.GetAsync(Param)).Entity != null)
        {
            result.AddErrorMessage(NotDeleted);
            return result;
        }
        var version = NoteVersionDto.FromJson((await Repository.History.GetPreviousContentAsync(deleted.KLogId)).Entity);
        if (version == null)
        {
            result.AddErrorMessage(KntHistoryGetNoteVersionAsyncCommand.CopyNotKept);
            return result;
        }

        var notices = new List<string>();
        var number = HistoryRules.NoteNumberOf(deleted.EntityTitle);
        if (number > 0 && (await Repository.Notes.GetAsync(number)).Entity != null)
            number = 0;

        var folder = (await Repository.Folders.GetAsync(version.FolderId)).Entity;
        if (folder == null || folder.FolderId != version.FolderId)
        {
            folder = (await Repository.Folders.GetAsync(KntConst.DefaultFolderNumber)).Entity;
            if (folder == null)
            {
                result.AddErrorMessage("The folder of the note no longer exists, nor the Home folder.");
                return result;
            }
            notices.Add($"Its folder no longer exists: it is in {await KntHistory.FolderPathAsync(Service, folder.FolderId) ?? folder.Name}.");
        }

        NoteTypeDto type = null;
        if (version.NoteTypeId is Guid typeId)
        {
            var found = await Repository.NoteTypes.GetAsync(typeId);
            type = found.IsValid && found.Entity?.NoteTypeId == typeId ? found.Entity : null;
            if (type == null)
                notices.Add("Its type no longer exists: it has no type.");
        }

        var now = DateTime.Now;
        var note = new NoteDto
        {
            NoteId = Param,
            NoteNumber = number,
            Topic = version.Topic,
            Description = version.Description,
            ContentType = version.ContentType,
            Tags = version.Tags,
            Priority = version.Priority,
            Script = version.Script,
            InternalTags = "",
            FolderId = folder.FolderId,
            FolderDto = folder,
            NoteTypeId = type?.NoteTypeId,
            NoteTypeDto = type,
            CreationDateTime = rows.FirstOrDefault(r => r.ActionType == nameof(EnumHistoryAction.Created))?.RegistryDateTime ?? now,
            ModificationDateTime = now
        };
        note.KAttributesDto = await Service.Notes.UtilCompleteNoteAttributesAsync(new List<NoteKAttributeDto>(), note.NoteId, note.NoteTypeId) ?? new();
        foreach (var attribute in note.KAttributesDto)
        {
            attribute.NoteId = note.NoteId;
            attribute.Value = version.AttributeValue(attribute.KAttributeId) ?? "";
        }
        var lost = version.Attributes.Count(v => note.KAttributesDto.All(a => a.KAttributeId != v.Id));
        if (lost > 0)
            notices.Add(lost == 1
                ? "1 attribute of it no longer exists or isn't of its type: it was not recovered."
                : $"{lost} attributes of it no longer exist or aren't of its type: they were not recovered.");

        Result<NoteDto> saved;
        using (KntHistory.Restoring(new HistoryRestoredDto { Recovered = true, KLogId = deleted.KLogId, VersionOf = deleted.LastDateTime }))
            saved = await Service.Notes.SaveAsync(note);
        if (!saved.IsValid)
        {
            result.AddListErrorMessage(saved.ListErrorMessage);
            return result;
        }

        var wanted = HistoryRules.NoteNumberOf(deleted.EntityTitle);
        if (wanted > 0 && saved.Entity.NoteNumber != wanted)
            notices.Insert(0, $"Its number #{wanted} is now another note's: it is #{saved.Entity.NoteNumber}.");
        notices.Add(PartsNotRecovered);
        result.Entity = new NoteRecoverResultDto
        {
            NoteId = saved.Entity.NoteId,
            NoteNumber = saved.Entity.NoteNumber,
            Topic = saved.Entity.Topic,
            Notices = notices
        };
        return result;
    }
}

/// <summary>
/// «Remove permanently» (H27, Admin): empties every version of a deleted note, as the purge does after DeletedNotesDays; its rows
/// stay and it can no longer be recovered.
/// </summary>
[KntAuthorize(EnumRoles.Admin)]
public class KntHistoryRemoveDeletedNoteAsyncCommand : KntCommandServiceBase<Guid, Result>
{
    public KntHistoryRemoveDeletedNoteAsyncCommand(IKntService service, Guid noteId) : base(service, noteId)
    {

    }

    public override async Task<Result> Execute()
    {
        var result = new Result();
        var last = (await Repository.History.GetLastAsync(HistoryEntities.Note, Param)).Entity;
        if (last == null || last.ActionType != nameof(EnumHistoryAction.Deleted) || (await Repository.Notes.GetAsync(Param)).Entity != null)
        {
            result.AddErrorMessage(KntHistoryRecoverNoteAsyncCommand.NotDeleted);
            return result;
        }
        var cleared = await Repository.History.ClearEntityContentAsync(HistoryEntities.Note, Param);
        if (!cleared.IsValid)
            return cleared;
        Service.Logger?.LogInformation("History: the versions of the deleted note {title} were removed by {user}.", last.EntityTitle, Service.UserIdentityName);
        return result;
    }
}

/// <summary>The retention of the history (H25) and the day the history started: any user (the editor is the Admin's).</summary>
[KntAuthorize(EnumRoles.Guest)]
public class KntHistoryGetRetentionAsyncCommand : KntCommandServiceBase<Result<HistoryRetentionInfoDto>>
{
    public KntHistoryGetRetentionAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<HistoryRetentionInfoDto>> Execute()
    {
        var start = (await Repository.SystemValues.GetAsync(KntConst.SystemScope, KntConst.HistoryStartKey)).Entity?.Value;
        return new Result<HistoryRetentionInfoDto>(new HistoryRetentionInfoDto
        {
            Retention = await KntHistory.RetentionAsync(Service),
            Since = DateTime.TryParse(start, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var since)
                ? since : null
        });
    }
}

/// <summary>
/// Keeps the retention of the history (H25, Admin), within the limits of HistoryRetentionDto. It applies in the next purge (H9) and
/// goes into the history of the settings, with its values.
/// </summary>
[KntAuthorize(EnumRoles.Admin)]
public class KntHistorySaveRetentionAsyncCommand : KntCommandServiceBase<HistoryRetentionDto, Result<HistoryRetentionDto>>
{
    public KntHistorySaveRetentionAsyncCommand(IKntService service, HistoryRetentionDto retention) : base(service, retention)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null)
            result.AddErrorMessage("The retention of the history is required.");
        else if (Param.GetErrorMessage() is string error && !string.IsNullOrEmpty(error))
            result.AddErrorMessage(error);
        return result;
    }

    public override async Task<Result<HistoryRetentionDto>> Execute()
    {
        var result = new Result<HistoryRetentionDto>();
        var retention = Param.Normalized();
        var before = await KntHistory.RetentionAsync(Service);
        var stored = await Repository.SystemValues.GetAsync(KntConst.SystemScope, KntConst.HistoryRetentionKey);

        var saved = stored.IsValid && stored.Entity != null
            ? await Repository.SystemValues.UpdateAsync(new SystemValueDto
            {
                SystemValueId = stored.Entity.SystemValueId, Scope = KntConst.SystemScope, Key = KntConst.HistoryRetentionKey, Value = retention.ToJson()
            })
            : await Repository.SystemValues.AddAsync(new SystemValueDto
            {
                SystemValueId = Guid.NewGuid(), Scope = KntConst.SystemScope, Key = KntConst.HistoryRetentionKey, Value = retention.ToJson()
            });
        if (!saved.IsValid)
        {
            result.AddListErrorMessage(saved.ListErrorMessage);
            return result;
        }

        await KntHistory.NoteStructureAsync(Service, HistoryEntities.SystemValue,
            HistoryRules.SettingId(KntConst.SystemScope, KntConst.HistoryRetentionKey), KntHistory.HistoryRetentionTitle,
            StructureChangeDescriber.HistoryRetention(before, retention));
        result.Entity = retention;
        return result;
    }
}

/// <summary>What «See this version», «Compare» and «Restore» share: the version after a row (H18) and the names of two versions.</summary>
internal static class KntHistoryVersions
{
    /// <summary>The note after a change: the copy of a later row or the note now (note, null if it no longer exists).</summary>
    internal static async Task<(HistoryRules.VersionSource Source, NoteVersionDto Version)> AfterAsync(IKntService service,
        List<KLogDto> rows, Guid klogId, NoteDto note)
    {
        var source = HistoryRules.VersionAfter(rows, klogId);
        if (source == null)
            return (null, null);
        var version = source.CopyRow != null
            ? NoteVersionDto.FromJson((await service.Repository.History.GetPreviousContentAsync(source.CopyRow.KLogId)).Entity)
            : NoteVersionDto.From(note);
        return (source, version);
    }

    /// <summary>The paths of the folders, the names of the types and of the attributes of some versions.</summary>
    internal static async Task<HistoryNames> NamesAsync(IKntService service, params NoteVersionDto[] versions)
    {
        var names = new HistoryNames();
        foreach (var version in versions.Where(v => v != null))
        {
            if (!names.Folders.ContainsKey(version.FolderId) && await KntHistory.FolderPathAsync(service, version.FolderId) is string path)
                names.Folders[version.FolderId] = path;
            if (version.NoteTypeId is Guid typeId && !names.NoteTypes.ContainsKey(typeId))
            {
                var type = await service.Repository.NoteTypes.GetAsync(typeId);
                names.NoteTypes[typeId] = type.IsValid && type.Entity?.NoteTypeId == typeId ? type.Entity.Name : "(deleted type)";
            }
            foreach (var attribute in version.Attributes.Where(a => !names.Attributes.ContainsKey(a.Id)))
            {
                var stored = await service.Repository.KAttributes.GetAsync(attribute.Id);
                names.Attributes[attribute.Id] = stored.IsValid && stored.Entity?.KAttributeId == attribute.Id ? stored.Entity.Name : "(deleted attribute)";
            }
        }
        return names;
    }
}
