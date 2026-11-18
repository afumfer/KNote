using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// The noise policy (KNote Atlas, stage 2, 2.2, chapter 27, D2-D4): whether a note goes into the summaries, and why. In order:
/// KNote's own notes (their types and folders), always out; #NoKnowledge, out (also with #Knowledge); #Knowledge, in although its
/// type or its folder are left out; its note type or its folder left out, out (only its own folder: a folder is left out on its
/// own, not with its subfolders); the rest, in. The tags are read whole and without case in the tags of the note.
/// </summary>
public static class NoiseRules
{
    private static readonly char[] TagSeparators = { ' ', ',', ';' };

    /// <summary>
    /// The note types of KNote's own notes, always out: the system ones, and what KNote Atlas writes (the summaries and the
    /// translations, D4: they are not summarized again).
    /// </summary>
    public static IReadOnlyList<string> KNoteNoteTypeNames { get; } =
        KntConst.SystemNoteTypeNames.Concat(new[] { KntConst.SummaryTag, KntConst.TranslationTag }).ToList();

    /// <summary>Whether the tags of a note have this one, whole and without case.</summary>
    public static bool HasTag(string tags, string tag) =>
        Tags(tags).Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));

    /// <summary>The tags of a note, one by one, as they are written.</summary>
    public static IEnumerable<string> Tags(string tags) => (tags ?? "").Split(TagSeparators, StringSplitOptions.RemoveEmptyEntries);

    /// <summary>Why a note is in or out. kNoteNoteTypeIds / kNoteFolderIds: the ids of KNote's own types and folders.</summary>
    public static EnumNoiseReason Classify(NoteClassificationDto note, NoisePolicyDto policy, ISet<Guid> kNoteNoteTypeIds, ISet<Guid> kNoteFolderIds)
    {
        if (note.NoteTypeId is Guid typeId && kNoteNoteTypeIds.Contains(typeId) || kNoteFolderIds.Contains(note.FolderId))
            return EnumNoiseReason.KNoteNote;
        if (HasTag(note.Tags, KntConst.NoKnowledgeTag))
            return EnumNoiseReason.NoKnowledgeTag;

        var byPolicy = note.NoteTypeId is Guid type && (policy?.ExcludedNoteTypeIds?.Contains(type) ?? false) ? EnumNoiseReason.ExcludedNoteType
            : policy?.ExcludedFolderIds?.Contains(note.FolderId) ?? false ? EnumNoiseReason.ExcludedFolder
            : EnumNoiseReason.In;
        return byPolicy != EnumNoiseReason.In && HasTag(note.Tags, KntConst.KnowledgeTag) ? EnumNoiseReason.KnowledgeTag : byPolicy;
    }

    public static bool IsOut(EnumNoiseReason reason) => reason is not (EnumNoiseReason.In or EnumNoiseReason.KnowledgeTag);

    /// <summary>How many notes the policy leaves out, and why.</summary>
    public static NoiseCountDto Count(IEnumerable<NoteClassificationDto> notes, NoisePolicyDto policy, ISet<Guid> kNoteNoteTypeIds, ISet<Guid> kNoteFolderIds)
    {
        var count = new NoiseCountDto();
        foreach (var note in notes ?? Enumerable.Empty<NoteClassificationDto>())
        {
            count.Notes++;
            switch (Classify(note, policy, kNoteNoteTypeIds, kNoteFolderIds))
            {
                case EnumNoiseReason.KNoteNote: count.KNoteNotes++; break;
                case EnumNoiseReason.NoKnowledgeTag: count.NoKnowledgeTag++; break;
                case EnumNoiseReason.ExcludedNoteType: count.ExcludedNoteType++; break;
                case EnumNoiseReason.ExcludedFolder: count.ExcludedFolder++; break;
                case EnumNoiseReason.KnowledgeTag: count.KeptByKnowledgeTag++; break;
            }
        }
        return count;
    }

    /// <summary>
    /// The policy as it is kept: without the types and the folders that don't exist any more, nor KNote's own ones (always out),
    /// each id once.
    /// </summary>
    public static NoisePolicyDto Clean(NoisePolicyDto policy, ISet<Guid> noteTypeIds, ISet<Guid> folderIds, ISet<Guid> kNoteNoteTypeIds, ISet<Guid> kNoteFolderIds) => new()
    {
        ExcludedNoteTypeIds = (policy?.ExcludedNoteTypeIds ?? new()).Where(id => noteTypeIds.Contains(id) && !kNoteNoteTypeIds.Contains(id)).Distinct().ToList(),
        ExcludedFolderIds = (policy?.ExcludedFolderIds ?? new()).Where(id => folderIds.Contains(id) && !kNoteFolderIds.Contains(id)).Distinct().ToList()
    };

    /// <summary>A reason in words, for the lists of the selection (2.3) and the help.</summary>
    public static string ReasonText(EnumNoiseReason reason) => reason switch
    {
        EnumNoiseReason.KnowledgeTag => $"In by its tag {KntConst.KnowledgeTag}",
        EnumNoiseReason.KNoteNote => "One of KNote's own notes",
        EnumNoiseReason.NoKnowledgeTag => $"Out by its tag {KntConst.NoKnowledgeTag}",
        EnumNoiseReason.ExcludedNoteType => "Its note type is left out",
        EnumNoiseReason.ExcludedFolder => "Its folder is left out",
        _ => "In"
    };

    /// <summary>The recount in words (both apps).</summary>
    public static List<string> CountLines(NoiseCountDto count)
    {
        var lines = new List<string> { $"Of {Notes(count.Notes)}, {Notes(count.Out)} would be left out of the summaries and {Notes(count.In)} would go in." };
        if (count.ExcludedNoteType > 0)
            lines.Add($"By their note type: {Notes(count.ExcludedNoteType)}.");
        if (count.ExcludedFolder > 0)
            lines.Add($"By their folder: {Notes(count.ExcludedFolder)}.");
        if (count.NoKnowledgeTag > 0)
            lines.Add($"By the tag {KntConst.NoKnowledgeTag}: {Notes(count.NoKnowledgeTag)}.");
        if (count.KNoteNotes > 0)
            lines.Add($"KNote's own notes (always out): {Notes(count.KNoteNotes)}.");
        if (count.KeptByKnowledgeTag > 0)
            lines.Add($"Kept by the tag {KntConst.KnowledgeTag}, although the policy would leave them out: {Notes(count.KeptByKnowledgeTag)}.");
        return lines;
    }

    public const string Help = "The notes of the note types and of the folders left out never go into a summary. A folder is left out on its own: " +
        "its subfolders go in unless they are left out too. A note with the tag #NoKnowledge is always out, and one with #Knowledge always in, " +
        "although its type or its folder are left out. KNote's own notes are always out.";

    private static string Notes(int count) => count == 1 ? "1 note" : $"{count:N0} notes";
}
