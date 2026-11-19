using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

// KNote Atlas, stage 2 (2.5, CU-16, CU-17): the summary of a period or of a subject. It is one more kind of the jobs of the summaries
// (EnumDocumentProcessKind.PeriodSummary, DocumentProcessRequestDto.Period): the same queue, progress and cancellation as the extended
// summary of a document.

/// <summary>What a period summary asks for: its selection, its subject and its length (the model and the language go in
/// DocumentProcessRequestDto).</summary>
public class PeriodSummaryRequestDto
{
    public NoteSelectionRequestDto Selection { get; set; } = new();
    public string Subject { get; set; }
    public int Words { get; set; } = 2000;
    // The attributes of the index «By concept» (PeriodConceptRules.Choices): its rows are the combinations of their values.
    public List<Guid> ConceptAttributeIds { get; set; } = new();

    public override string ToString() =>
        $"{Selection} «{Subject}» {Words} words" + (ConceptAttributeIds?.Count > 0 ? $", {ConceptAttributeIds.Count} concept attributes" : "");
}

/// <summary>The fields of «Period summary» as the user left them, kept by each app for the user and the repository at each change
/// (ClientWin, AppUserState.Session.PeriodSummaries; the web, the browser) and given back the next time. Null dates: the last 7 days.</summary>
public class PeriodSummaryParametersDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool FromTheBeginning { get; set; }
    public Guid? FolderId { get; set; }
    public string Keywords { get; set; }
    public string MeaningText { get; set; }
    public string Subject { get; set; }
    public int Words { get; set; } = 2000;
    public string Language { get; set; }
    // The alias of the chat provider; empty: the repository's recommended model.
    public string ProviderAlias { get; set; }
    public List<Guid> ConceptAttributeIds { get; set; } = new();
}

/// <summary>A note of the summary as the AI reads it: its data, its cleaned text, its tasks with activity in the period and the
/// descriptions of its resources.</summary>
public class PeriodNoteDto
{
    public Guid NoteId { get; set; }
    public int NoteNumber { get; set; }
    public string Topic { get; set; }
    public string FolderPath { get; set; }
    public DateTime ModificationDateTime { get; set; }
    public string Tags { get; set; }
    public string Text { get; set; }
    public List<string> Tasks { get; set; } = new();
    public List<string> Resources { get; set; } = new();
    // The values of the attributes chosen for «By concept» that the note has, by its column, the attribute's name (PeriodConceptRules.Value).
    public Dictionary<string, string> Attributes { get; set; } = new();
    public double Score { get; set; }
    // Its tasks with activity in the period, for the statistics of the summary (2.6, D15): started and ended in it, open at its end,
    // and the sum of their estimated and spent time (none if no task has it).
    public int TasksStarted { get; set; }
    public int TasksEnded { get; set; }
    public int TasksOpen { get; set; }
    public double? EstimatedTime { get; set; }
    public double? SpentTime { get; set; }
}

/// <summary>A value of an attribute of a note as the repository gives it (IKntNoteRepository.GetAttributeValuesAsync).</summary>
public class NoteAttributeValueDto
{
    public Guid NoteId { get; set; }
    public Guid KAttributeId { get; set; }
    public string Value { get; set; }
}

/// <summary>A part of «What happened» (2.6, D13): its title (a folder, «Other folders», or a part of one), what the AI wrote of it and
/// its notes.</summary>
public class PeriodSummarySectionDto
{
    public string Title { get; set; }
    public string Text { get; set; }
    public List<int> NoteNumbers { get; set; } = new();
}

/// <summary>A period summary ready to be read: its selection (with the doubtful notes as they are) and the notes that go in.</summary>
public class PeriodSummaryPreparedDto
{
    public NoteSelectionResultDto Selection { get; set; } = new();
    public List<PeriodNoteDto> Notes { get; set; } = new();
    // The columns of «By concept» (PeriodConceptRules.Columns): the names of the attributes asked for that exist and group.
    public List<string> ConceptAttributes { get; set; } = new();
}

/// <summary>A period summary to keep in a new note of «KNote Summary»: what the AI wrote, by parts and as it wrote it (2.6, D13), and
/// the period as it was read, from which KNote calculates the rest of the document (D15) and the relations with its notes (D17).</summary>
public class PeriodSummarySaveDto
{
    public PeriodSummaryRequestDto Request { get; set; } = new();
    public string Model { get; set; }
    public string Language { get; set; }
    public string Executive { get; set; }
    public List<PeriodSummarySectionDto> Sections { get; set; } = new();
    public string Pending { get; set; }
    public PeriodSummaryPreparedDto Prepared { get; set; } = new();
    public int Calls { get; set; }
    // To keep the pace of the model (D18, P6).
    public int CharactersRead { get; set; }
    public double Seconds { get; set; }

    // Without the texts: the log of the commands writes the parameter.
    public override string ToString() =>
        $"«{Request?.Subject}», {Model}, {Prepared?.Notes?.Count ?? 0} notes, {Sections?.Count ?? 0} sections, {Calls} calls";
}
