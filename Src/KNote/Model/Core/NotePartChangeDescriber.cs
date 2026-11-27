using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>The fields of the parts of a note that the history tells (HistoryFieldChangeDto.Field).</summary>
public static class HistoryPartFields
{
    // Tasks
    public const string Resolved = "Resolved";
    public const string Description = "Description";
    public const string User = "User";
    public const string Tags = "Tags";
    public const string Priority = "Priority";
    public const string EstimatedTime = "EstimatedTime";
    public const string SpentTime = "SpentTime";
    public const string Difficulty = "Difficulty";
    public const string ExpectedStart = "ExpectedStart";
    public const string ExpectedEnd = "ExpectedEnd";
    public const string Start = "Start";
    public const string End = "End";

    // Resources
    public const string Name = "Name";
    public const string File = "File";
    public const string Locked = "Locked";

    // Alarms
    public const string Date = "Date";
    public const string Activated = "Activated";
    public const string AlarmType = "AlarmType";
    public const string Minutes = "Minutes";
    public const string Notification = "Notification";
    public const string Comment = "Comment";

    // Relations
    public const string Type = "Type";
    public const string Order = "Order";
    public const string Weight = "Weight";
}

/// <summary>
/// What changed in a part of a note (H3): a task, a resource, an alarm or a relation, as a HistoryPartChangeDto of the row of
/// its note; how two changes of the same session add up (MergeList) and their lines in the history (Parts). A part changed only
/// in what the history doesn't tell (the order of a resource, the state the note calculates) is no change. The previous
/// description of a task is kept, cut to MaxTaskDescriptionLength (H4); the other values, to a few hundred characters.
/// </summary>
public static class NotePartChangeDescriber
{
    public const int MaxNameLength = 80;
    public const int MaxTaskDescriptionLength = 4096;

    private const string Yes = "Yes";
    private const string No = "No";

    #region Describe

    /// <summary>A task added (before null), removed (after null) or changed; null if nothing the history tells changed.</summary>
    public static HistoryPartChangeDto Task(NoteTaskDto before, NoteTaskDto after, HistoryNames names)
    {
        names ??= new HistoryNames();
        if (before == null && after == null)
            return null;
        if (before == null)
            return new HistoryPartChangeDto { Id = after.NoteTaskId, Action = HistoryPartActions.Added, Name = TaskName(after.Description) };
        if (after == null)
            return new HistoryPartChangeDto { Id = before.NoteTaskId, Action = HistoryPartActions.Removed, Name = TaskName(before.Description) };

        var changes = new List<HistoryFieldChangeDto>();
        // Resolved exactly when it has an end date (TaskPanelRules).
        bool wasResolved = TaskPanelRules.IsResolved(before), isResolved = TaskPanelRules.IsResolved(after);
        if (wasResolved != isResolved)
            changes.Add(Field(HistoryPartFields.Resolved, YesNo(wasResolved), YesNo(isResolved)));
        else if (!SameDate(before.EndDate, after.EndDate))
            changes.Add(Field(HistoryPartFields.End, DateText(before.EndDate), DateText(after.EndDate)));

        if (!NoteChangeRules.SameText(before.Description, after.Description))
            changes.Add(new HistoryFieldChangeDto
            {
                Field = HistoryPartFields.Description,
                Before = string.IsNullOrEmpty(before.Description) ? null : HistoryRules.Truncate(before.Description, MaxTaskDescriptionLength)
            });
        if (before.UserId != after.UserId)
            changes.Add(Field(HistoryPartFields.User, names.User(before.UserId), names.User(after.UserId)));
        if (!NoteChangeRules.SameText(before.Tags, after.Tags))
            changes.Add(Field(HistoryPartFields.Tags, before.Tags, after.Tags));
        if (before.Priority != after.Priority)
            changes.Add(Field(HistoryPartFields.Priority, Number(before.Priority), Number(after.Priority)));
        AddNumber(changes, HistoryPartFields.EstimatedTime, before.EstimatedTime, after.EstimatedTime);
        AddNumber(changes, HistoryPartFields.SpentTime, before.SpentTime, after.SpentTime);
        AddNumber(changes, HistoryPartFields.Difficulty, before.DifficultyLevel, after.DifficultyLevel);
        AddDate(changes, HistoryPartFields.ExpectedStart, before.ExpectedStartDate, after.ExpectedStartDate);
        AddDate(changes, HistoryPartFields.ExpectedEnd, before.ExpectedEndDate, after.ExpectedEndDate);
        AddDate(changes, HistoryPartFields.Start, before.StartDate, after.StartDate);

        return Changed(after.NoteTaskId, TaskName(after.Description), changes);
    }

    /// <summary>
    /// A resource added, removed or changed by a person (what the AI writes goes another way and is not noted): renamed, its
    /// file replaced (newFile: it came with a content other than the stored one), its description, its lock.
    /// </summary>
    public static HistoryPartChangeDto Resource(ResourceInfoDto before, ResourceInfoDto after, bool newFile)
    {
        if (before == null && after == null)
            return null;
        if (before == null)
            return new HistoryPartChangeDto { Id = after.ResourceId, Action = HistoryPartActions.Added, Name = Cut(after.NameOut) };
        if (after == null)
            return new HistoryPartChangeDto { Id = before.ResourceId, Action = HistoryPartActions.Removed, Name = Cut(before.NameOut) };

        var changes = new List<HistoryFieldChangeDto>();
        if (newFile)
            changes.Add(Field(HistoryPartFields.File, before.NameOut, after.NameOut));
        else if (before.NameOut != after.NameOut)
            changes.Add(Field(HistoryPartFields.Name, before.NameOut, after.NameOut));
        if (!NoteChangeRules.SameText(before.Description, after.Description))
            changes.Add(new HistoryFieldChangeDto { Field = HistoryPartFields.Description });
        if (before.DescriptionByUser != after.DescriptionByUser)
            changes.Add(Field(HistoryPartFields.Locked, YesNo(before.DescriptionByUser), YesNo(after.DescriptionByUser)));

        return Changed(after.ResourceId, Cut(after.NameOut), changes);
    }

    /// <summary>An alarm added, removed or changed. Never its reprogramming when it is due (that goes another way).</summary>
    public static HistoryPartChangeDto Alarm(KMessageDto before, KMessageDto after, HistoryNames names)
    {
        names ??= new HistoryNames();
        if (before == null && after == null)
            return null;
        if (before == null)
            return new HistoryPartChangeDto { Id = after.KMessageId, Action = HistoryPartActions.Added, Name = AlarmName(after) };
        if (after == null)
            return new HistoryPartChangeDto { Id = before.KMessageId, Action = HistoryPartActions.Removed, Name = AlarmName(before) };

        var changes = new List<HistoryFieldChangeDto>();
        AddDate(changes, HistoryPartFields.Date, before.AlarmDateTime, after.AlarmDateTime);
        if ((before.AlarmActivated ?? false) != (after.AlarmActivated ?? false))
            changes.Add(Field(HistoryPartFields.Activated, YesNo(before.AlarmActivated ?? false), YesNo(after.AlarmActivated ?? false)));
        if (before.AlarmType != after.AlarmType)
            changes.Add(Field(HistoryPartFields.AlarmType, before.AlarmType.ToString(), after.AlarmType.ToString()));
        if (before.AlarmMinutes != after.AlarmMinutes)
            changes.Add(Field(HistoryPartFields.Minutes, Number(before.AlarmMinutes), Number(after.AlarmMinutes)));
        if (before.NotificationType != after.NotificationType)
            changes.Add(Field(HistoryPartFields.Notification, before.NotificationType.ToString(), after.NotificationType.ToString()));
        if (before.UserId != after.UserId)
            changes.Add(Field(HistoryPartFields.User, names.User(before.UserId), names.User(after.UserId)));
        if (!NoteChangeRules.SameText(before.Comment, after.Comment))
            changes.Add(Field(HistoryPartFields.Comment, before.Comment, after.Comment));

        return Changed(after.KMessageId, AlarmName(after), changes);
    }

    /// <summary>
    /// A relation, seen from one of its notes: otherNote is how the other one is called (#12 Topic). Added with its type, removed
    /// (also because the other note was deleted, H14) or changed (its type, order or weight).
    /// </summary>
    public static HistoryPartChangeDto Relation(TraceNoteDto before, TraceNoteDto after, string otherNote, HistoryNames names,
        bool otherNoteDeleted = false)
    {
        names ??= new HistoryNames();
        if (before == null && after == null)
            return null;
        var name = Cut(otherNote);
        if (before == null)
        {
            var type = names.TraceNoteType(after.TraceNoteTypeId);
            return new HistoryPartChangeDto
            {
                Id = after.TraceNoteId,
                Action = HistoryPartActions.Added,
                Name = name,
                Changes = type == null ? null : new List<HistoryFieldChangeDto> { Field(HistoryPartFields.Type, null, type) }
            };
        }
        if (after == null)
            return new HistoryPartChangeDto
            {
                Id = before.TraceNoteId,
                Action = HistoryPartActions.Removed,
                Name = name,
                OtherNoteDeleted = otherNoteDeleted ? true : null
            };

        var changes = new List<HistoryFieldChangeDto>();
        if (before.TraceNoteTypeId != after.TraceNoteTypeId)
            changes.Add(Field(HistoryPartFields.Type, names.TraceNoteType(before.TraceNoteTypeId) ?? NoteChangeDescriber.NoValue,
                names.TraceNoteType(after.TraceNoteTypeId) ?? NoteChangeDescriber.NoValue));
        if (before.Order != after.Order)
            changes.Add(Field(HistoryPartFields.Order, Number(before.Order), Number(after.Order)));
        if (Math.Abs(before.Weight - after.Weight) > 1e-9)
            changes.Add(Field(HistoryPartFields.Weight, Number(before.Weight), Number(after.Weight)));

        return Changed(after.TraceNoteId, name, changes);
    }

    /// <summary>The details of a change of one part: a list of one in the member of its kind.</summary>
    public static NoteChangeDetailsDto Details(EnumHistoryFields part, HistoryPartChangeDto change)
    {
        var details = new NoteChangeDetailsDto();
        if (change == null)
            return details;
        var list = new List<HistoryPartChangeDto> { change };
        switch (part)
        {
            case EnumHistoryFields.Tasks: details.Tasks = list; break;
            case EnumHistoryFields.Resources: details.Resources = list; break;
            case EnumHistoryFields.Alarms: details.Alarms = list; break;
            case EnumHistoryFields.Relations: details.Relations = list; break;
            default: throw new ArgumentException($"{part} is not a part of a note.", nameof(part));
        }
        return details;
    }

    /// <summary>Gives target the parts of source (target was described again from its note's data, which has none).</summary>
    public static NoteChangeDetailsDto KeepParts(NoteChangeDetailsDto target, NoteChangeDetailsDto source)
    {
        if (target == null || source == null)
            return target;
        target.Tasks = source.Tasks;
        target.Resources = source.Resources;
        target.Alarms = source.Alarms;
        target.Relations = source.Relations;
        return target;
    }

    #endregion

    #region Merge

    /// <summary>
    /// Two changes of parts in the same session as one: added and then removed is nothing, added and then changed is added,
    /// changed twice is each field from its first value to its last, and changed back to how it was is nothing.
    /// </summary>
    public static List<HistoryPartChangeDto> MergeList(List<HistoryPartChangeDto> earlier, List<HistoryPartChangeDto> later)
    {
        var merged = (earlier ?? new()).Select(Copy).ToList();
        foreach (var change in later ?? new())
        {
            var existing = merged.FirstOrDefault(p => p.Id == change.Id);
            if (existing == null)
            {
                merged.Add(Copy(change));
                continue;
            }

            if (existing.Action == HistoryPartActions.Added)
            {
                if (change.Action == HistoryPartActions.Removed)
                    merged.Remove(existing);
                else
                {
                    existing.Name = change.Name ?? existing.Name;
                    if (change.Action == HistoryPartActions.Changed)
                        existing.Changes = AddedChanges(existing.Changes, change.Changes);
                }
            }
            else if (existing.Action == HistoryPartActions.Changed && change.Action == HistoryPartActions.Changed)
            {
                existing.Name = change.Name ?? existing.Name;
                existing.Changes = MergeChanges(existing.Changes, change.Changes);
                if (existing.Changes == null)
                    merged.Remove(existing);
            }
            else
                merged[merged.IndexOf(existing)] = Copy(change);
        }
        return merged.Count > 0 ? merged : null;
    }

    // Added and then changed: still added, with what tells how it was added (the type of a relation) as it ended up.
    private static List<HistoryFieldChangeDto> AddedChanges(List<HistoryFieldChangeDto> added, List<HistoryFieldChangeDto> changes)
    {
        if (added == null)
            return null;
        var result = added.Select(c => new HistoryFieldChangeDto { Field = c.Field, Before = c.Before, After = c.After }).ToList();
        foreach (var change in changes ?? new())
            foreach (var item in result.Where(r => r.Field == change.Field))
                item.After = change.After;
        return result;
    }

    private static List<HistoryFieldChangeDto> MergeChanges(List<HistoryFieldChangeDto> earlier, List<HistoryFieldChangeDto> later)
    {
        var merged = (earlier ?? new()).Select(c => new HistoryFieldChangeDto { Field = c.Field, Before = c.Before, After = c.After }).ToList();
        foreach (var change in later ?? new())
        {
            var existing = merged.FirstOrDefault(c => c.Field == change.Field);
            if (existing == null)
                merged.Add(new HistoryFieldChangeDto { Field = change.Field, Before = change.Before, After = change.After });
            else
                existing.After = change.After;
        }
        // Back to how it was (the descriptions keep only the one before: they can't tell).
        merged.RemoveAll(c => c.Field != HistoryPartFields.Description && NoteChangeRules.SameText(c.Before, c.After));
        return merged.Count > 0 ? merged : null;
    }

    private static HistoryPartChangeDto Copy(HistoryPartChangeDto part) => new()
    {
        Id = part.Id,
        Action = part.Action,
        Name = part.Name,
        OtherNoteDeleted = part.OtherNoteDeleted,
        Changes = part.Changes?.Select(c => new HistoryFieldChangeDto { Field = c.Field, Before = c.Before, After = c.After }).ToList()
    };

    #endregion

    #region Render

    /// <summary>The lines of the parts of a change, in order: tasks, resources, alarms, relations.</summary>
    public static List<string> Parts(NoteChangeDetailsDto details)
    {
        var parts = new List<string>();
        if (details == null)
            return parts;
        foreach (var task in details.Tasks ?? new())
            parts.Add(TaskLine(task));
        foreach (var resource in details.Resources ?? new())
            parts.Add(ResourceLine(resource));
        foreach (var alarm in details.Alarms ?? new())
            parts.Add(Line("Alarm", alarm));
        foreach (var relation in details.Relations ?? new())
            parts.Add(RelationLine(relation));
        return parts;
    }

    /// <summary>What a deleted note had (H14): «with 2 tasks, 1 resource», or null if it had none.</summary>
    public static string DeletedParts(HistoryDeletedDto deleted)
    {
        if (deleted == null)
            return null;
        var counts = new List<string>();
        AddCount(counts, deleted.Tasks, "task");
        AddCount(counts, deleted.Resources, "resource");
        AddCount(counts, deleted.Alarms, "alarm");
        AddCount(counts, deleted.Relations, "relation");
        return counts.Count == 0 ? null : "With " + string.Join(", ", counts);
    }

    private static string TaskLine(HistoryPartChangeDto task)
    {
        if (task.Action != HistoryPartActions.Changed)
            return Line("Task", task);

        var resolved = task.Changes?.FirstOrDefault(c => c.Field == HistoryPartFields.Resolved);
        var others = (task.Changes ?? new()).Where(c => c != resolved).ToList();
        var verb = resolved == null ? "changed" : resolved.After == Yes ? "resolved" : "reopened";
        return $"Task {verb}: «{task.Name}»" + (others.Count > 0 ? $" ({FieldNames(others)})" : "");
    }

    private static string ResourceLine(HistoryPartChangeDto resource)
    {
        if (resource.Action != HistoryPartActions.Changed)
            return Line("Resource", resource);

        var changes = resource.Changes ?? new();
        if (changes.Count == 1)
        {
            var change = changes[0];
            switch (change.Field)
            {
                case HistoryPartFields.Name: return $"Resource renamed: {change.Before} → {change.After}";
                case HistoryPartFields.File:
                    return change.Before == change.After ? $"Resource replaced: {resource.Name}" : $"Resource replaced: {change.Before} → {change.After}";
                case HistoryPartFields.Description: return $"Resource description changed: {resource.Name}";
                case HistoryPartFields.Locked: return $"Resource {(change.After == Yes ? "locked" : "unlocked")}: {resource.Name}";
            }
        }
        return $"Resource changed: {resource.Name} ({FieldNames(changes)})";
    }

    private static string RelationLine(HistoryPartChangeDto relation)
    {
        switch (relation.Action)
        {
            case HistoryPartActions.Added:
                var type = relation.Changes?.FirstOrDefault(c => c.Field == HistoryPartFields.Type)?.After;
                return $"Related to {relation.Name}" + (type != null ? $" ({type})" : "");
            case HistoryPartActions.Removed:
                return $"Relation removed: {relation.Name}" + (relation.OtherNoteDeleted == true ? " (note deleted)" : "");
            default:
                return $"Relation changed: {relation.Name} ({FieldNames(relation.Changes ?? new())})";
        }
    }

    private static string Line(string kind, HistoryPartChangeDto part) => part.Action switch
    {
        HistoryPartActions.Added => $"{kind} added: {Quoted(kind, part.Name)}",
        HistoryPartActions.Removed => $"{kind} removed: {Quoted(kind, part.Name)}",
        _ => $"{kind} changed: {Quoted(kind, part.Name)}" + (part.Changes?.Count > 0 ? $" ({FieldNames(part.Changes)})" : "")
    };

    // A task is known by its text, so it goes in quotes; a file, a date or a note, as they are.
    private static string Quoted(string kind, string name) => kind == "Task" ? $"«{name}»" : name;

    private static string FieldNames(IEnumerable<HistoryFieldChangeDto> changes) =>
        string.Join(", ", changes.Select(c => FieldName(c.Field)).Distinct());

    private static string FieldName(string field) => field switch
    {
        HistoryPartFields.EstimatedTime => "estimated time",
        HistoryPartFields.SpentTime => "spent time",
        HistoryPartFields.ExpectedStart => "expected start",
        HistoryPartFields.ExpectedEnd => "expected end",
        HistoryPartFields.Start => "start date",
        HistoryPartFields.End => "end date",
        HistoryPartFields.AlarmType => "type",
        HistoryPartFields.Notification => "notification",
        HistoryPartFields.File => "file replaced",
        HistoryPartFields.Name => "renamed",
        _ => field.ToLowerInvariant()
    };

    private static void AddCount(List<string> counts, int? count, string what)
    {
        if (count > 0)
            counts.Add(count == 1 ? $"1 {what}" : $"{count} {what}s");
    }

    #endregion

    #region Values

    /// <summary>A task is known by the first line of its text.</summary>
    public static string TaskName(string description)
    {
        var first = LineDiff.SplitLines(description).FirstOrDefault(l => l.Trim().Length > 0)?.Trim();
        return string.IsNullOrEmpty(first) ? "(no text)" : Cut(first);
    }

    /// <summary>An alarm is known by when it goes off.</summary>
    public static string AlarmName(KMessageDto alarm) => alarm.AlarmDateTime == null ? "(no date)" : DateText(alarm.AlarmDateTime);

    /// <summary>A date as the history writes it: to the minute, or the day alone if it has no time.</summary>
    public static string DateText(DateTime? date) =>
        date is not DateTime value ? NoteChangeDescriber.NoValue
        : value.TimeOfDay == TimeSpan.Zero ? value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        : value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

    private static HistoryPartChangeDto Changed(Guid id, string name, List<HistoryFieldChangeDto> changes) =>
        changes.Count == 0 ? null : new HistoryPartChangeDto { Id = id, Action = HistoryPartActions.Changed, Name = name, Changes = changes };

    private static HistoryFieldChangeDto Field(string field, string before, string after) => new()
    {
        Field = field,
        Before = string.IsNullOrEmpty(before) ? null : HistoryRules.Truncate(before, NoteChangeDescriber.MaxValueLength),
        After = string.IsNullOrEmpty(after) ? null : HistoryRules.Truncate(after, NoteChangeDescriber.MaxValueLength)
    };

    private static void AddNumber(List<HistoryFieldChangeDto> changes, string field, double? before, double? after)
    {
        if (before == null && after == null || before != null && after != null && Math.Abs(before.Value - after.Value) < 1e-9)
            return;
        changes.Add(Field(field, Number(before), Number(after)));
    }

    private static void AddDate(List<HistoryFieldChangeDto> changes, string field, DateTime? before, DateTime? after)
    {
        if (!SameDate(before, after))
            changes.Add(Field(field, DateText(before), DateText(after)));
    }

    // To the second: what the databases give back may differ from what was saved in the fractions of a second.
    private static bool SameDate(DateTime? a, DateTime? b) =>
        a == null || b == null ? a == b : Math.Abs((a.Value - b.Value).TotalSeconds) < 1;

    private static string Number(double? value) => value?.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Number(int? value) => value?.ToString(CultureInfo.InvariantCulture);

    private static string YesNo(bool value) => value ? Yes : No;

    private static string Cut(string text) => string.IsNullOrEmpty(text) ? text : HistoryRules.Truncate(text, MaxNameLength);

    #endregion
}
