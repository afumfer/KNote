using System;
using System.Collections.Generic;
using System.Linq;

namespace KNote.Model.Dto;

// KNote Atlas, stage 2 (2.3, CU-15, chapters 29, 30 and 33): the selection of the notes of a summary, without the AI.

/// <summary>What is selected: the notes of a period (whole days, From and To included) and, optionally, of a folder and its
/// subfolders, with some words (the search by words of always, in the topic and the text) and about something (by meaning, among
/// the notes of the period). All of them combined with «and».</summary>
public class NoteSelectionRequestDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public Guid? FolderId { get; set; }
    public string Keywords { get; set; }
    public string MeaningText { get; set; }

    public override string ToString() =>
        $"{From:yyyy-MM-dd}..{To:yyyy-MM-dd} folder={FolderId} keywords={Keywords} meaning={MeaningText}";
}

/// <summary>A note of the period as the repository gives it (IKntNoteRepository.GetSelectionSourcesAsync): its data, its text and
/// how many tasks (and with activity in the period), resources and relations (both ways) it has.</summary>
public class NoteSelectionSourceDto
{
    public Guid NoteId { get; set; }
    public int NoteNumber { get; set; }
    public string Topic { get; set; }
    public string Description { get; set; }
    public string ContentType { get; set; }
    public string Tags { get; set; }
    public Guid FolderId { get; set; }
    public Guid? NoteTypeId { get; set; }
    public DateTime CreationDateTime { get; set; }
    public DateTime ModificationDateTime { get; set; }
    public int Tasks { get; set; }
    public int TasksWithActivity { get; set; }
    public int Resources { get; set; }
    public int Relations { get; set; }
}

public enum EnumSelectionDecision
{
    In,
    Doubtful,
    Out
}

/// <summary>Where a decision comes from (NoteRelevanceRules): the signals of the note, the noise policy or a tag, a person's
/// correction or, from 2.4, the AI.</summary>
public enum EnumSelectionSource
{
    Signals,
    Policy,
    Tag,
    Correction,
    Ai
}

/// <summary>A note of the selection: its decision with its reason in words, where it comes from and its score (the sum of its
/// signals: it orders the lists and will be the weight of its relation with the summary). ComputedDecision is what KNote decides
/// without the person's correction, to know whether a change in «Review selection» is a correction or takes one away.</summary>
public class NoteSelectionDto
{
    public Guid NoteId { get; set; }
    public int NoteNumber { get; set; }
    public string Topic { get; set; }
    public Guid FolderId { get; set; }
    public string FolderName { get; set; }
    public DateTime ModificationDateTime { get; set; }
    public int Words { get; set; }
    public EnumSelectionDecision Decision { get; set; }
    public EnumSelectionSource Source { get; set; }
    public string Reason { get; set; }
    public double Score { get; set; }
    public EnumSelectionDecision ComputedDecision { get; set; }
    public string ComputedReason { get; set; }

    public bool IsIncluded => Decision != EnumSelectionDecision.Out;
}

/// <summary>The selection: its notes (KNote's own ones are not listed, only counted) and the counts.</summary>
public class NoteSelectionResultDto
{
    public List<NoteSelectionDto> Notes { get; set; } = new();
    public int KNoteNotes { get; set; }
    public bool MeaningUsed { get; set; }

    public int In => Notes.Count(n => n.Decision == EnumSelectionDecision.In);
    public int Doubtful => Notes.Count(n => n.Decision == EnumSelectionDecision.Doubtful);
    public int Out => Notes.Count(n => n.Decision == EnumSelectionDecision.Out);
}

/// <summary>A correction of a person kept in «Note relevance»: the note goes in or out, whatever KNote decides (but KNote's own).</summary>
public class RelevanceCorrectionDto
{
    public Guid NoteId { get; set; }
    public int NoteNumber { get; set; }
    public string Topic { get; set; }
    public EnumSelectionDecision Decision { get; set; }
    public string UserName { get; set; }
    public DateTime Date { get; set; }
}

/// <summary>The answer of the AI for a doubtful note (2.4), valid while the note does not change (ContentHash).</summary>
public class RelevanceAiAnswerDto
{
    public Guid NoteId { get; set; }
    public int NoteNumber { get; set; }
    public string Topic { get; set; }
    public EnumSelectionDecision Decision { get; set; }
    public string Reason { get; set; }
    public string ContentHash { get; set; }
    public string Model { get; set; }
    public DateTime Date { get; set; }
}

/// <summary>A change of «Review selection»: the note goes in or out (Decision In or Out) or its correction is taken away (null: it is
/// again as KNote decides). NoteSelectionRules.CorrectionFor gives it.</summary>
public class RelevanceCorrectionChangeDto
{
    public Guid NoteId { get; set; }
    public EnumSelectionDecision? Decision { get; set; }

    public override string ToString() => $"{NoteId}={Decision?.ToString() ?? "remove"}";
}

/// <summary>The changes of «Review selection», saved at once.</summary>
public class RelevanceCorrectionsSaveDto
{
    public List<RelevanceCorrectionChangeDto> Changes { get; set; } = new();

    public override string ToString() => string.Join(", ", Changes);
}
