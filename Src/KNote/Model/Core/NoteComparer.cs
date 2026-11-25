using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// Two versions of a note's own data side by side («Compare», H12, H22): the fields that differ (names resolved with HistoryNames)
/// and the text by lines (LineDiff, the same comparison as the history), each line removed next to the added one that replaces it
/// compared by words (WordDiff). The equal lines more than ContextLines away from a change are folded, and no more than
/// MaxLinesShown lines are given: a big note doesn't travel whole. The script isn't shown (H21), only whether it changed.
/// </summary>
public static class NoteComparer
{
    public const int ContextLines = 3;
    public const int MaxLinesShown = 3000;
    public const int MaxFieldValueLength = 2000;
    public const string ScriptChanged = "Changed (not shown)";

    /// <summary>before null: a new note (everything it has is added).</summary>
    public static NoteComparisonDto Compare(NoteVersionDto before, NoteVersionDto after, HistoryNames names)
    {
        names ??= new HistoryNames();
        after ??= new NoteVersionDto();
        var comparison = new NoteComparisonDto();
        var fields = comparison.Fields;

        void Add(string name, string was, string now)
        {
            if (NoteChangeRules.SameText(was, now))
                return;
            fields.Add(new NoteComparisonFieldDto { Name = name, Before = Cut(was), After = Cut(now) });
        }

        Add("Topic", before?.Topic, after.Topic);
        if (before == null || before.FolderId != after.FolderId)
            Add("Folder", before == null ? null : names.Folder(before.FolderId), names.Folder(after.FolderId));
        if (before == null ? after.NoteTypeId != null : before.NoteTypeId != after.NoteTypeId)
            Add("Type", before == null ? null : NoValueAsNull(names.NoteType(before.NoteTypeId)), NoValueAsNull(names.NoteType(after.NoteTypeId)));
        Add("Tags", before?.Tags, after.Tags);
        if (before == null ? after.Priority != 0 : before.Priority != after.Priority)
            Add("Priority", before?.Priority.ToString(), after.Priority.ToString());

        var wasType = ContentTypeExtJsonHelper.Read(before?.ContentType);
        var nowType = ContentTypeExtJsonHelper.Read(after.ContentType);
        if (before != null)
        {
            if (!SameName(wasType.ForDescription, nowType.ForDescription))
                Add("Description format", wasType.ForDescription, nowType.ForDescription);
            if (wasType.DescriptionBlocked != nowType.DescriptionBlocked)
                Add("Description blocked", wasType.DescriptionBlocked ? "Yes" : "No", nowType.DescriptionBlocked ? "Yes" : "No");
            if (!SameName(wasType.ForScript, nowType.ForScript))
                Add("Script format", wasType.ForScript, nowType.ForScript);
        }
        if (!NoteChangeRules.SameText(before?.Script, after.Script))
            fields.Add(new NoteComparisonFieldDto { Name = "Script", Before = null, After = ScriptChanged });

        var beforeAttributes = before?.Attributes ?? new List<NoteVersionAttributeDto>();
        foreach (var id in beforeAttributes.Select(a => a.Id).Union(after.Attributes.Select(a => a.Id)))
            Add(names.Attribute(id), before?.AttributeValue(id), after.AttributeValue(id));

        comparison.Text = CompareText(before?.Description, after.Description);
        return comparison;
    }

    /// <summary>The text by lines: the changes with ContextLines of equal ones around them, the rest folded.</summary>
    public static NoteTextComparisonDto CompareText(string before, string after)
    {
        var text = new NoteTextComparisonDto();
        if (NoteChangeRules.SameText(before, after))
            return text;

        var diff = LineDiff.Compare(before, after);
        text.Same = diff.Added == 0 && diff.Removed == 0;
        text.Replaced = diff.Replaced;
        text.Added = diff.Added;
        text.Removed = diff.Removed;
        if (text.Same)
            return text;

        // The lines in order: each run of changes as its removed lines and then its added ones, paired by order.
        var ordered = new List<NoteComparisonLineDto>();
        var removed = new List<NoteComparisonLineDto>();
        var added = new List<NoteComparisonLineDto>();

        void FlushChanges()
        {
            for (int i = 0; i < Math.Min(removed.Count, added.Count); i++)
            {
                var words = WordDiff.Compare(removed[i].Text, added[i].Text);
                if (words == null)
                    continue;
                removed[i].Segments = Segments(words.Before);
                added[i].Segments = Segments(words.After);
            }
            ordered.AddRange(removed);
            ordered.AddRange(added);
            removed.Clear();
            added.Clear();
        }

        foreach (var line in diff.Lines)
        {
            switch (line.Kind)
            {
                case LineDiff.Kind.Equal:
                    FlushChanges();
                    ordered.Add(new NoteComparisonLineDto
                    {
                        Kind = EnumComparisonLine.Equal,
                        BeforeNumber = line.BeforeIndex + 1,
                        AfterNumber = line.AfterIndex + 1,
                        Text = diff.Before[line.BeforeIndex]
                    });
                    break;
                case LineDiff.Kind.Removed:
                    removed.Add(new NoteComparisonLineDto { Kind = EnumComparisonLine.Removed, BeforeNumber = line.BeforeIndex + 1, Text = diff.Before[line.BeforeIndex] });
                    break;
                case LineDiff.Kind.Added:
                    added.Add(new NoteComparisonLineDto { Kind = EnumComparisonLine.Added, AfterNumber = line.AfterIndex + 1, Text = diff.After[line.AfterIndex] });
                    break;
            }
        }
        FlushChanges();

        // The equal lines near a change are shown; the others, folded.
        var shown = new bool[ordered.Count];
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].Kind == EnumComparisonLine.Equal)
                continue;
            for (int j = Math.Max(0, i - ContextLines); j <= Math.Min(ordered.Count - 1, i + ContextLines); j++)
                shown[j] = true;
        }

        int folded = 0;
        foreach (var (line, index) in ordered.Select((l, i) => (l, i)))
        {
            if (!shown[index])
            {
                folded++;
                continue;
            }
            if (folded > 0)
            {
                text.Lines.Add(new NoteComparisonLineDto { Kind = EnumComparisonLine.Folded, Skipped = folded });
                folded = 0;
            }
            if (text.Lines.Count >= MaxLinesShown)
            {
                text.Truncated = true;
                return text;
            }
            text.Lines.Add(line);
        }
        if (folded > 0)
            text.Lines.Add(new NoteComparisonLineDto { Kind = EnumComparisonLine.Folded, Skipped = folded });
        return text;
    }

    private static List<NoteComparisonSegmentDto> Segments(IReadOnlyList<WordDiff.Segment> segments) =>
        segments.Select(s => new NoteComparisonSegmentDto { Text = s.Text, Changed = s.Changed }).ToList();

    private static string NoValueAsNull(string name) => name == NoteChangeDescriber.NoValue ? null : name;

    private static bool SameName(string a, string b) => string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);

    private static string Cut(string text) => string.IsNullOrEmpty(text) ? null : HistoryRules.Truncate(text, MaxFieldValueLength);
}
