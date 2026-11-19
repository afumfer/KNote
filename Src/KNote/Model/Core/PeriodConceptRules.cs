using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// The index «By concept» of a period summary: a table per attribute the user chooses (the ones that group: texts, tabulated values,
/// tags, yes/no and integers; never dates, decimals or long texts, where each note would be a row of its own), with a row per value and
/// its notes and tasks with activity in the period. In each table, each note is in one row (a TagsValue counts as the set of its
/// values), so its rows add up to the notes with a value of it. Calculated by KNote, never by the AI. The attributes are chosen one by one (shown with
/// their note type); two chosen with the same name (of different note types) are one column.
/// </summary>
public static class PeriodConceptRules
{
    public const int MaxRows = 30;
    public const string NoneChosen = "No attributes chosen.";

    /// <summary>A row of a table: a value of its attribute and its notes and tasks. Value null: the other values, beyond MaxRows.</summary>
    public sealed record Concept(string Value, List<int> NoteNumbers, int Tasks, int TasksStarted, int TasksEnded, int TasksOpen);

    /// <summary>The table of an attribute: its rows, the other values together (null, none) and the notes without a value of it.</summary>
    public sealed record ConceptTable(string Attribute, List<Concept> Rows, Concept Others, int OtherValues, int NotesWithoutValue);

    /// <summary>Whether an attribute of this type can group the notes.</summary>
    public static bool IsGrouper(EnumKAttributeDataType type) => type is EnumKAttributeDataType.String or EnumKAttributeDataType.TabulatedValue
        or EnumKAttributeDataType.TagsValue or EnumKAttributeDataType.Bool or EnumKAttributeDataType.Int;

    /// <summary>An attribute that can be chosen: its note type (null, of every note) and its name, shown as «Note type · Name» («- · Name»
    /// without a type).</summary>
    public sealed record Choice(Guid KAttributeId, string NoteType, string Name)
    {
        public string Label => $"{(string.IsNullOrWhiteSpace(NoteType) ? NoType : NoteType.Trim())} · {Name}";

        public override string ToString() => Label;
    }

    public const string NoType = "-";

    /// <summary>The attributes that can be chosen: the enabled ones that group, the ones of every note first and then by their note
    /// type, each one by its order and name.</summary>
    public static List<Choice> Choices(IEnumerable<KAttributeInfoDto> attributes) =>
        Grouping(attributes)
            .OrderBy(a => a.NoteTypeId == null ? 0 : 1)
            .ThenBy(a => a.NoteTypeDto?.Name ?? "", StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(a => a.Order).ThenBy(a => a.Name.Trim(), StringComparer.CurrentCultureIgnoreCase)
            .Select(a => new Choice(a.KAttributeId, a.NoteTypeId == null ? null : a.NoteTypeDto?.Name, a.Name.Trim()))
            .ToList();

    /// <summary>Of the attributes asked for, the ones that can be chosen, in the order of the choices.</summary>
    public static List<Guid> Chosen(IEnumerable<Guid> asked, IReadOnlyList<Choice> choices)
    {
        var ids = (asked ?? Enumerable.Empty<Guid>()).ToHashSet();
        return (choices ?? new List<Choice>()).Where(c => ids.Contains(c.KAttributeId)).Select(c => c.KAttributeId).ToList();
    }

    /// <summary>The columns of the index of the attributes chosen: their names, once each (two attributes of the same name, of different
    /// note types, are one column), in the order of the choices.</summary>
    public static List<string> Columns(IEnumerable<KAttributeInfoDto> attributes, IEnumerable<Guid> chosen)
    {
        var choices = Choices(attributes);
        var ids = Chosen(chosen, choices).ToHashSet();
        return choices.Where(c => ids.Contains(c.KAttributeId))
            .GroupBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase).Select(g => g.First().Name).ToList();
    }

    /// <summary>A value as it is shown and grouped: yes/no as Yes or No, the values of a TagsValue sorted, the rest in one line; empty, null.</summary>
    public static string Value(EnumKAttributeDataType type, string value)
    {
        var text = PeriodSummaryRules.Line(value);
        if (text.Length == 0)
            return null;
        switch (type)
        {
            case EnumKAttributeDataType.Bool:
                return bool.TryParse(text, out var yes) ? (yes ? "Yes" : "No") : text;
            case EnumKAttributeDataType.TagsValue:
                var values = text.Split(',').Select(v => v.Trim()).Where(v => v.Length > 0)
                    .Distinct(StringComparer.CurrentCultureIgnoreCase).OrderBy(v => v, StringComparer.CurrentCultureIgnoreCase).ToList();
                return values.Count == 0 ? null : string.Join(", ", values);
            default:
                return text;
        }
    }

    /// <summary>The values of the attributes chosen of each note, by the column (the name of the attribute) and in the order of the
    /// columns (with two attributes of the same name, the first one by its order).</summary>
    public static Dictionary<Guid, Dictionary<string, string>> ValuesByNote(IEnumerable<NoteAttributeValueDto> values,
        IEnumerable<KAttributeInfoDto> attributes, IEnumerable<Guid> chosen)
    {
        var all = attributes?.ToList() ?? new List<KAttributeInfoDto>();
        var columns = Columns(all, chosen);
        var ids = chosen?.ToHashSet() ?? new HashSet<Guid>();
        var behind = Grouping(all).Where(a => ids.Contains(a.KAttributeId))
            .Select((a, order) => (a.KAttributeId, Column: columns.First(c => string.Equals(c, a.Name.Trim(), StringComparison.CurrentCultureIgnoreCase)),
                a.KAttributeDataType, Order: order))
            .ToDictionary(a => a.KAttributeId);
        var result = new Dictionary<Guid, Dictionary<string, string>>();
        foreach (var note in (values ?? Enumerable.Empty<NoteAttributeValueDto>()).Where(v => behind.ContainsKey(v.KAttributeId)).GroupBy(v => v.NoteId))
        {
            var found = note
                .Select(v => (Attribute: behind[v.KAttributeId], Value: Value(behind[v.KAttributeId].KAttributeDataType, v.Value)))
                .Where(v => v.Value != null)
                .OrderBy(v => v.Attribute.Order)
                .ToList();
            var noteValues = new Dictionary<string, string>();
            foreach (var column in columns)
            {
                var first = found.FirstOrDefault(v => v.Attribute.Column == column).Value;
                if (first != null)
                    noteValues[column] = first;
            }
            if (noteValues.Count > 0)
                result[note.Key] = noteValues;
        }
        return result;
    }

    /// <summary>The index: a table per attribute (column), in their order.</summary>
    public static List<ConceptTable> Tables(IReadOnlyList<PeriodNoteDto> notes, IReadOnlyList<string> attributes) =>
        (attributes ?? new List<string>()).Select(a => Table(notes, a)).ToList();

    /// <summary>The table of an attribute: a row per value, from the one with more notes, the other values together beyond MaxRows,
    /// and the notes without a value apart.</summary>
    public static ConceptTable Table(IReadOnlyList<PeriodNoteDto> notes, string attribute)
    {
        var without = 0;
        var groups = new Dictionary<string, List<(string Value, PeriodNoteDto Note)>>(StringComparer.CurrentCultureIgnoreCase);
        foreach (var note in notes ?? new List<PeriodNoteDto>())
        {
            var value = note.Attributes?.GetValueOrDefault(attribute);
            if (value == null)
            {
                without++;
                continue;
            }
            if (!groups.TryGetValue(value, out var group))
                groups[value] = group = new();
            group.Add((value, note));
        }

        var rows = groups.Values
            .Select(g => Of(g[0].Value, g.Select(x => x.Note).ToList()))
            .OrderByDescending(r => r.NoteNumbers.Count)
            .ThenBy(r => r.Value, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        var others = rows.Skip(MaxRows).ToList();
        var other = others.Count == 0 ? null : new Concept(null, others.SelectMany(r => r.NoteNumbers).OrderBy(n => n).ToList(),
            others.Sum(r => r.Tasks), others.Sum(r => r.TasksStarted), others.Sum(r => r.TasksEnded), others.Sum(r => r.TasksOpen));
        return new ConceptTable(attribute, rows.Take(MaxRows).ToList(), other, others.Count, without);
    }

    private static Concept Of(string value, IReadOnlyCollection<PeriodNoteDto> notes) => new(value,
        notes.Select(n => n.NoteNumber).Distinct().OrderBy(n => n).ToList(), notes.Sum(n => n.Tasks?.Count ?? 0),
        notes.Sum(n => n.TasksStarted), notes.Sum(n => n.TasksEnded), notes.Sum(n => n.TasksOpen));

    private static IEnumerable<KAttributeInfoDto> Grouping(IEnumerable<KAttributeInfoDto> attributes) =>
        (attributes ?? Enumerable.Empty<KAttributeInfoDto>())
            .Where(a => !a.Disabled && IsGrouper(a.KAttributeDataType) && !string.IsNullOrWhiteSpace(a.Name))
            .OrderBy(a => a.Order).ThenBy(a => a.Name.Trim(), StringComparer.CurrentCultureIgnoreCase);
}
