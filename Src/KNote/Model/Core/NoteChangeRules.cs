using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// Whether saving a note modifies it (KNote Atlas, stage 2, D6): only if its own data changed against what is stored (topic,
/// text, tags, type, folder, priority, script, format and the values of its attributes), whatever the editor marked as
/// changed. Its status (InternalTags, from its tasks and alarms), resources, tasks, alarms and relations don't count. Texts
/// compare as the editors give them back: no value and empty are the same, CR LF and LF are the same, and the spaces and
/// lines at the end don't count (the Web's markdown editor does the same).
/// </summary>
public static class NoteChangeRules
{
    /// <summary>The first of the note's own data that changed, by its name, or null if none did.</summary>
    public static string ChangedField(NoteDto stored, NoteDto edited)
    {
        if (stored == null || edited == null)
            return "Note";

        if (!SameText(stored.Topic, edited.Topic))
            return nameof(NoteDto.Topic);
        if (!SameText(stored.Description, edited.Description))
            return nameof(NoteDto.Description);
        if (!SameText(stored.Tags, edited.Tags))
            return nameof(NoteDto.Tags);
        if (stored.FolderId != edited.FolderId)
            return nameof(NoteDto.FolderId);
        if (stored.NoteTypeId != edited.NoteTypeId)
            return nameof(NoteDto.NoteTypeId);
        if (stored.Priority != edited.Priority)
            return nameof(NoteDto.Priority);
        if (!SameText(stored.Script, edited.Script))
            return nameof(NoteDto.Script);
        if (!SameText(stored.ContentType, edited.ContentType))
            return nameof(NoteDto.ContentType);

        var storedValues = AttributeValues(stored.KAttributesDto);
        var editedValues = AttributeValues(edited.KAttributesDto);
        foreach (var id in storedValues.Keys.Union(editedValues.Keys))
        {
            storedValues.TryGetValue(id, out var before);
            editedValues.TryGetValue(id, out var after);
            if (!SameText(before, after))
                return nameof(NoteDto.KAttributesDto);
        }

        return null;
    }

    /// <summary>
    /// The tags of a note after changing oldTag into newTag (IKntNoteRepository.PatchChangeTagsAsync): without oldTag, newTag is
    /// added if the tags don't have it yet; with it, every oldTag becomes newTag. Shared by both repositories and the history.
    /// </summary>
    public static string ChangeTag(string tags, string oldTag, string newTag)
    {
        var changed = tags ?? "";
        newTag ??= "";
        if (string.IsNullOrEmpty(oldTag))
        {
            if (changed.IndexOf(newTag, StringComparison.Ordinal) < 0)
                changed += " " + newTag;
        }
        else
            changed = changed.Replace(oldTag, newTag);
        return changed.Trim();
    }

    /// <summary>Two texts of a note as the same text (see the class).</summary>
    public static bool SameText(string a, string b)
    {
        if (a == b)
            return true;
        a = (a ?? "").Replace("\r\n", "\n");
        b = (b ?? "").Replace("\r\n", "\n");
        return a.AsSpan().TrimEnd().SequenceEqual(b.AsSpan().TrimEnd());
    }

    // The value of each attribute of the note; an attribute it doesn't have is an empty one.
    private static Dictionary<Guid, string> AttributeValues(List<NoteKAttributeDto> attributes) =>
        (attributes ?? new List<NoteKAttributeDto>())
            .GroupBy(a => a.KAttributeId)
            .ToDictionary(g => g.Key, g => g.Last().Value);
}
