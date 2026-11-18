using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>
/// The noise policy of a repository (KNote Atlas, stage 2, 2.2, D3): the note types and the folders whose notes never go into a
/// summary, by their ids (renaming them doesn't break it). Each folder on its own: its subfolders are not left out with it. KNote's
/// own notes are always out and are not kept here. In SystemValues, scope AI, key AI_NOISE_POLICY.
/// </summary>
public class NoisePolicyDto
{
    public List<Guid> ExcludedNoteTypeIds { get; set; } = new();
    public List<Guid> ExcludedFolderIds { get; set; } = new();

    public override string ToString() => $"Note types left out: {ExcludedNoteTypeIds?.Count ?? 0}, folders left out: {ExcludedFolderIds?.Count ?? 0}";
}

/// <summary>What the noise policy needs of a note: no texts (KNote Atlas 2.2; the selection of 2.3 too).</summary>
public class NoteClassificationDto
{
    public Guid NoteId { get; set; }
    public int NoteNumber { get; set; }
    public Guid FolderId { get; set; }
    public Guid? NoteTypeId { get; set; }
    public string Tags { get; set; }
}

/// <summary>Why a note is in or out of the summaries by the noise policy (NoiseRules), in order.</summary>
public enum EnumNoiseReason
{
    // In: nothing puts it out.
    In,
    // In by its tag #Knowledge, although its type or its folder are left out.
    KnowledgeTag,
    // Out: one of KNote's own notes (its type or its folder), always.
    KNoteNote,
    // Out by its tag #NoKnowledge (also with #Knowledge).
    NoKnowledgeTag,
    // Out by its note type.
    ExcludedNoteType,
    // Out by its folder (only the folder itself, not its parents).
    ExcludedFolder
}

/// <summary>How many notes a noise policy leaves out, and why (the section «Noise policy», KNote Atlas 2.2).</summary>
public class NoiseCountDto
{
    public int Notes { get; set; }
    public int Out => KNoteNotes + NoKnowledgeTag + ExcludedNoteType + ExcludedFolder;
    public int In => Notes - Out;
    public int KNoteNotes { get; set; }
    public int NoKnowledgeTag { get; set; }
    public int ExcludedNoteType { get; set; }
    public int ExcludedFolder { get; set; }
    // In by #Knowledge, although the policy would leave them out.
    public int KeptByKnowledgeTag { get; set; }
}
