using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>The names the history shows of what a note refers to by id, given by the Service (it reads them).</summary>
public class HistoryNames
{
    /// <summary>The path of each folder (Home\Projects\KNote).</summary>
    public Dictionary<Guid, string> Folders { get; } = new();
    public Dictionary<Guid, string> NoteTypes { get; } = new();
    public Dictionary<Guid, string> Attributes { get; } = new();

    public string Folder(Guid folderId) => Folders.TryGetValue(folderId, out var name) && !string.IsNullOrEmpty(name) ? name : "(unknown folder)";

    public string NoteType(Guid? noteTypeId) =>
        noteTypeId == null ? NoteChangeDescriber.NoValue
        : NoteTypes.TryGetValue(noteTypeId.Value, out var name) && !string.IsNullOrEmpty(name) ? name : "(unknown type)";

    public string Attribute(Guid kattributeId) =>
        Attributes.TryGetValue(kattributeId, out var name) && !string.IsNullOrEmpty(name) ? name : "(unknown attribute)";

    /// <summary>The name of each user (the tasks and alarms of a note have one).</summary>
    public Dictionary<Guid, string> Users { get; } = new();
    public Dictionary<Guid, string> TraceNoteTypes { get; } = new();

    public string User(Guid? userId) =>
        userId is not Guid id || id == Guid.Empty ? NoteChangeDescriber.NoValue
        : Users.TryGetValue(id, out var name) && !string.IsNullOrEmpty(name) ? name : "(unknown user)";

    public string TraceNoteType(Guid? traceNoteTypeId) =>
        traceNoteTypeId is not Guid id ? null
        : TraceNoteTypes.TryGetValue(id, out var name) && !string.IsNullOrEmpty(name) ? name : "(unknown type)";
}

/// <summary>
/// What changed between two versions of a note's own data (NoteChangeDetailsDto) and its line in the history (RegistryMessage):
/// «Status: Restoring → Works · Tags: +restored · Changes in «Restoration» (+18 −11 lines)», at most MaxMessageLength characters
/// and «… and N more changes». A part changed exactly when NoteChangeRules says so (the same comparison of texts), so a save that
/// keeps the note's date notes nothing of its own data. The text is compared by lines (LineDiff) and told by Markdown sections.
/// </summary>
public static class NoteChangeDescriber
{
    public const int MaxMessageLength = 300;
    public const int MaxValueLength = 200;
    public const int MaxSectionsListed = 20;
    public const string NoValue = "(none)";
    public const string EmptyValue = "(empty)";

    private const string Separator = " · ";
    private static readonly char[] TagSeparators = { ' ', ',', ';', '\t', '\r', '\n' };

    /// <summary>The change from before to after; before null: a new note.</summary>
    public static NoteChangeDetailsDto Describe(NoteVersionDto before, NoteVersionDto after, HistoryNames names)
    {
        names ??= new HistoryNames();
        var details = new NoteChangeDetailsDto();
        if (after == null)
            return details;

        if (before == null)
        {
            details.Created = new HistoryCreatedDto
            {
                Folder = Cut(names.Folder(after.FolderId)),
                Sections = Headings(LineDiff.SplitLines(after.Description))
                    .Where(h => h.Title != null).Select(h => Cut(h.Title)).Distinct().Take(MaxSectionsListed).ToList()
            };
            if (details.Created.Sections.Count == 0)
                details.Created.Sections = null;
            return details;
        }

        if (!NoteChangeRules.SameText(before.Topic, after.Topic))
            details.Topic = Value(before.Topic, after.Topic);

        if (!NoteChangeRules.SameText(before.Description, after.Description))
            details.Text = TextChange(before.Description, after.Description);

        var attributeIds = before.Attributes.Select(a => a.Id).Union(after.Attributes.Select(a => a.Id)).ToList();
        foreach (var id in attributeIds)
        {
            var was = before.AttributeValue(id);
            var now = after.AttributeValue(id);
            if (NoteChangeRules.SameText(was, now))
                continue;
            details.Attributes ??= new List<HistoryAttributeChangeDto>();
            details.Attributes.Add(new HistoryAttributeChangeDto
            {
                Id = id,
                Name = Cut(names.Attribute(id)),
                Before = CutOrNull(was),
                After = CutOrNull(now)
            });
        }

        if (!NoteChangeRules.SameText(before.Tags, after.Tags))
        {
            var was = Tags(before.Tags);
            var now = Tags(after.Tags);
            details.Tags = new HistoryTagsChangeDto
            {
                Added = now.Where(t => !was.Contains(t, StringComparer.OrdinalIgnoreCase)).ToList(),
                Removed = was.Where(t => !now.Contains(t, StringComparer.OrdinalIgnoreCase)).ToList()
            };
        }

        if (before.FolderId != after.FolderId)
            details.Folder = Value(names.Folder(before.FolderId), names.Folder(after.FolderId));

        if (before.NoteTypeId != after.NoteTypeId)
            details.NoteType = Value(names.NoteType(before.NoteTypeId), names.NoteType(after.NoteTypeId));

        if (before.Priority != after.Priority)
            details.Priority = Value(before.Priority.ToString(), after.Priority.ToString());

        if (!NoteChangeRules.SameText(before.ContentType, after.ContentType))
        {
            // By its parts, as read (JSON or the old format). The same values written in another form (an old note saved
            // in JSON) are not a change of the person's.
            var was = ContentTypeExtJsonHelper.Read(before.ContentType);
            var now = ContentTypeExtJsonHelper.Read(after.ContentType);
            if (!SameName(was.ForDescription, now.ForDescription))
                details.DescriptionFormat = Value(Format(was.ForDescription), Format(now.ForDescription));
            if (!SameName(was.ForScript, now.ForScript))
                details.ScriptFormat = Value(Format(was.ForScript), Format(now.ForScript));
            if (was.DescriptionBlocked != now.DescriptionBlocked)
                details.DescriptionBlocked = now.DescriptionBlocked;
        }

        if (!NoteChangeRules.SameText(before.Script, after.Script))
            details.Script = true;

        return details;
    }

    /// <summary>A note deleted, as it was (its folder).</summary>
    public static NoteChangeDetailsDto DescribeDeleted(NoteVersionDto last, HistoryNames names) => new()
    {
        Deleted = new HistoryDeletedDto { Folder = last == null ? null : Cut((names ?? new HistoryNames()).Folder(last.FolderId)) }
    };

    /// <summary>
    /// Two changes in a row as one (a session without the version it started from, so it can't be described again from it):
    /// each value from the first before to the last after, the tags and attributes as they ended up, the lines added up.
    /// </summary>
    public static NoteChangeDetailsDto Merge(NoteChangeDetailsDto earlier, NoteChangeDetailsDto later)
    {
        if (earlier == null)
            return later ?? new NoteChangeDetailsDto();
        if (later == null)
            return earlier;

        var merged = new NoteChangeDetailsDto
        {
            Created = earlier.Created ?? later.Created,
            Deleted = later.Deleted ?? earlier.Deleted,
            Restored = earlier.Restored ?? later.Restored,
            Topic = MergeValue(earlier.Topic, later.Topic),
            Folder = MergeValue(earlier.Folder, later.Folder),
            NoteType = MergeValue(earlier.NoteType, later.NoteType),
            Priority = MergeValue(earlier.Priority, later.Priority),
            DescriptionFormat = MergeValue(earlier.DescriptionFormat, later.DescriptionFormat),
            ScriptFormat = MergeValue(earlier.ScriptFormat, later.ScriptFormat),
            // Each one is a change: blocked and then unblocked is as it was.
            DescriptionBlocked = earlier.DescriptionBlocked != null && later.DescriptionBlocked != null
                ? null : earlier.DescriptionBlocked ?? later.DescriptionBlocked,
            Script = earlier.Script == true || later.Script == true ? true : null,
            Tags = MergeTags(earlier.Tags, later.Tags),
            Text = MergeText(earlier.Text, later.Text),
            Tasks = NotePartChangeDescriber.MergeList(earlier.Tasks, later.Tasks),
            Resources = NotePartChangeDescriber.MergeList(earlier.Resources, later.Resources),
            Alarms = NotePartChangeDescriber.MergeList(earlier.Alarms, later.Alarms),
            Relations = NotePartChangeDescriber.MergeList(earlier.Relations, later.Relations)
        };

        var attributes = new List<HistoryAttributeChangeDto>();
        foreach (var change in (earlier.Attributes ?? new()).Concat(later.Attributes ?? new()))
        {
            var existing = attributes.FirstOrDefault(a => a.Id == change.Id);
            if (existing == null)
                attributes.Add(new HistoryAttributeChangeDto { Id = change.Id, Name = change.Name, Before = change.Before, After = change.After });
            else
            {
                existing.After = change.After;
                existing.Name = change.Name ?? existing.Name;
            }
        }
        attributes.RemoveAll(a => NoteChangeRules.SameText(a.Before, a.After));
        merged.Attributes = attributes.Count > 0 ? attributes : null;

        return merged;
    }

    /// <summary>The line of the history: the changes in order, as many as fit in maxLength.</summary>
    public static string Render(NoteChangeDetailsDto details, int maxLength = MaxMessageLength) =>
        Join(Parts(details ?? new NoteChangeDetailsDto()), maxLength);

    /// <summary>The parts of a line of the history, in order, as many as fit in maxLength, and «… and N more changes».</summary>
    public static string Join(IReadOnlyList<string> parts, int maxLength = MaxMessageLength)
    {
        if (parts == null || parts.Count == 0)
            return "No changes";

        var line = new StringBuilder();
        for (int i = 0; i < parts.Count; i++)
        {
            var remaining = parts.Count - i - 1;
            var more = remaining > 0 ? Separator + More(remaining) : "";
            var candidate = (line.Length > 0 ? Separator : "") + parts[i];

            if (line.Length + candidate.Length + more.Length <= maxLength)
            {
                line.Append(candidate);
                continue;
            }

            if (line.Length == 0)
            {
                // Not even the first one fits: cut it.
                var room = Math.Max(1, maxLength - more.Length - 1);
                line.Append(parts[i].Length > room ? parts[i][..room] + "…" : parts[i]);
                if (remaining > 0)
                    line.Append(Separator).Append(More(remaining));
            }
            else
                line.Append(Separator).Append(More(parts.Count - i));
            break;
        }
        return line.ToString();
    }

    private static List<string> Parts(NoteChangeDetailsDto d)
    {
        var parts = new List<string>();

        if (d.Restored != null)
            parts.Add(d.Restored.VersionOf is DateTime versionOf
                ? $"Restored the version of {HistoryRules.MomentText(versionOf)}"
                : "Restored a version");
        if (d.Created != null)
        {
            parts.Add(d.Created.Folder != null ? $"New note in {d.Created.Folder}" : "New note");
            if (d.Created.Sections?.Count > 0)
                parts.Add("Sections: " + Quoted(d.Created.Sections, 5));
        }
        if (d.Deleted != null)
        {
            parts.Add(d.Deleted.Folder != null ? $"Note deleted from {d.Deleted.Folder}" : "Note deleted");
            if (NotePartChangeDescriber.DeletedParts(d.Deleted) is string had)
                parts.Add(had);
        }

        if (d.Topic != null)
            parts.Add($"Topic: «{d.Topic.Before}» → «{d.Topic.After}»");
        if (d.Text != null)
            parts.Add(TextPart(d.Text));
        foreach (var attribute in d.Attributes ?? new())
            parts.Add($"{attribute.Name}: {attribute.Before ?? EmptyValue} → {attribute.After ?? EmptyValue}");
        if (d.Tags != null)
        {
            var tags = d.Tags.Added.Select(t => "+" + t).Concat(d.Tags.Removed.Select(t => "−" + t)).ToList();
            parts.Add(tags.Count > 0 ? "Tags: " + string.Join(" ", tags) : "Tags changed");
        }
        if (d.Folder != null)
            parts.Add($"Moved from {d.Folder.Before} to {d.Folder.After}");
        if (d.NoteType != null)
            parts.Add($"Type: {d.NoteType.Before} → {d.NoteType.After}");
        if (d.Priority != null)
            parts.Add($"Priority: {d.Priority.Before} → {d.Priority.After}");
        if (d.DescriptionFormat != null)
            parts.Add($"Description format: {d.DescriptionFormat.Before} → {d.DescriptionFormat.After}");
        if (d.DescriptionBlocked != null)
            parts.Add(d.DescriptionBlocked.Value ? "Description blocked" : "Description unblocked");
        if (d.ScriptFormat != null)
            parts.Add($"Script format: {d.ScriptFormat.Before} → {d.ScriptFormat.After}");
        if (d.Script == true)
            parts.Add("Script changed");

        parts.AddRange(NotePartChangeDescriber.Parts(d));
        return parts;
    }

    private static string TextPart(HistoryTextChangeDto text)
    {
        var counts = Counts(text.Added, text.Removed);
        if (text.Replaced)
            return counts == null ? "Text replaced" : $"Text replaced ({counts})";

        var named = text.Sections.Where(s => s.Title != null).Select(s => s.Title).ToList();
        if (named.Count == 0)
            return counts == null ? "Text changed" : $"Text changed ({counts})";

        // The text before the first heading has no name: said, so the counts aren't read as all of them in the named ones.
        var where = (text.Sections.Any(s => s.Title == null) ? "Changes at the start and in " : "Changes in ") + Quoted(named, 3);
        return counts == null ? where : $"{where} ({counts})";
    }

    private static string Counts(int added, int removed)
    {
        if (added == 0 && removed == 0)
            return null;
        var counts = new List<string>();
        if (added > 0) counts.Add("+" + added);
        if (removed > 0) counts.Add("−" + removed);
        return string.Join(" ", counts) + (added + removed == 1 ? " line" : " lines");
    }

    private static string Quoted(List<string> items, int max)
    {
        var shown = string.Join(", ", items.Take(max).Select(i => "«" + i + "»"));
        return items.Count > max ? shown + ", …" : shown;
    }

    private static string More(int count) => count == 1 ? "… and 1 more change" : $"… and {count} more changes";

    private static HistoryTextChangeDto TextChange(string before, string after)
    {
        var comparison = LineDiff.Compare(before, after);
        var beforeSections = SectionOfEachLine(comparison.Before);
        var afterSections = SectionOfEachLine(comparison.After);

        var change = new HistoryTextChangeDto { Added = comparison.Added, Removed = comparison.Removed, Replaced = comparison.Replaced };
        foreach (var line in comparison.Lines)
        {
            if (line.Kind == LineDiff.Kind.Equal)
                continue;
            var title = line.Kind == LineDiff.Kind.Added ? afterSections[line.AfterIndex] : beforeSections[line.BeforeIndex];
            var section = change.Sections.FirstOrDefault(s => s.Title == title);
            if (section == null)
            {
                section = new HistorySectionChangeDto { Title = title };
                change.Sections.Add(section);
            }
            if (line.Kind == LineDiff.Kind.Added) section.Added++;
            else section.Removed++;
        }
        if (change.Sections.Count > MaxSectionsListed)
            change.Sections = change.Sections.Take(MaxSectionsListed).ToList();
        return change;
    }

    // The title of the section each line is in (the heading itself is in its section), null before the first heading.
    private static string[] SectionOfEachLine(IReadOnlyList<string> lines)
    {
        var sections = new string[lines.Count];
        string current = null;
        var headings = Headings(lines).ToDictionary(h => h.Index, h => h.Title);
        for (int i = 0; i < lines.Count; i++)
        {
            if (headings.TryGetValue(i, out var title))
                current = Cut(title);
            sections[i] = current;
        }
        return sections;
    }

    // The Markdown headings (# to ######, outside code blocks) of the lines.
    private static List<(int Index, string Title)> Headings(IReadOnlyList<string> lines)
    {
        var headings = new List<(int, string)>();
        bool inCode = false;
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i].TrimStart();
            if (line.StartsWith("```") || line.StartsWith("~~~"))
            {
                inCode = !inCode;
                continue;
            }
            if (inCode || !line.StartsWith('#'))
                continue;

            int level = 0;
            while (level < line.Length && line[level] == '#')
                level++;
            if (level > 6 || level >= line.Length || line[level] != ' ')
                continue;

            var title = line[level..].Trim().TrimEnd('#').Trim();
            if (title.Length > 0)
                headings.Add((i, title));
        }
        return headings;
    }

    private static List<string> Tags(string tags) =>
        (tags ?? "").Split(TagSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    private static string Format(string name) => string.IsNullOrWhiteSpace(name) ? NoValue : name.Trim();

    private static bool SameName(string a, string b) => string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);

    private static HistoryValueChangeDto Value(string before, string after) => new() { Before = Cut(before ?? ""), After = Cut(after ?? "") };

    private static HistoryValueChangeDto MergeValue(HistoryValueChangeDto earlier, HistoryValueChangeDto later)
    {
        if (earlier == null || later == null)
            return earlier ?? later;
        return NoteChangeRules.SameText(earlier.Before, later.After) ? null : new HistoryValueChangeDto { Before = earlier.Before, After = later.After };
    }

    private static HistoryTagsChangeDto MergeTags(HistoryTagsChangeDto earlier, HistoryTagsChangeDto later)
    {
        if (earlier == null || later == null)
            return earlier ?? later;

        var comparer = StringComparer.OrdinalIgnoreCase;
        var added = earlier.Added.Where(t => !later.Removed.Contains(t, comparer))
            .Concat(later.Added.Where(t => !earlier.Removed.Contains(t, comparer))).Distinct(comparer).ToList();
        var removed = earlier.Removed.Where(t => !later.Added.Contains(t, comparer))
            .Concat(later.Removed.Where(t => !earlier.Added.Contains(t, comparer))).Distinct(comparer).ToList();
        return new HistoryTagsChangeDto { Added = added, Removed = removed };
    }

    private static HistoryTextChangeDto MergeText(HistoryTextChangeDto earlier, HistoryTextChangeDto later)
    {
        if (earlier == null || later == null)
            return earlier ?? later;

        var merged = new HistoryTextChangeDto
        {
            Added = earlier.Added + later.Added,
            Removed = earlier.Removed + later.Removed,
            Replaced = earlier.Replaced || later.Replaced
        };
        foreach (var section in earlier.Sections.Concat(later.Sections))
        {
            var existing = merged.Sections.FirstOrDefault(s => s.Title == section.Title);
            if (existing == null)
                merged.Sections.Add(new HistorySectionChangeDto { Title = section.Title, Added = section.Added, Removed = section.Removed });
            else
            {
                existing.Added += section.Added;
                existing.Removed += section.Removed;
            }
        }
        return merged;
    }

    private static string Cut(string text) => HistoryRules.Truncate(text, MaxValueLength);

    private static string CutOrNull(string text) => string.IsNullOrEmpty(text) ? null : Cut(text);
}
