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

    public override string ToString() => $"{Selection} «{Subject}» {Words} words";
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
    public double Score { get; set; }
}

/// <summary>A period summary ready to be read: its selection (with the doubtful notes as they are) and the notes that go in.</summary>
public class PeriodSummaryPreparedDto
{
    public NoteSelectionResultDto Selection { get; set; } = new();
    public List<PeriodNoteDto> Notes { get; set; } = new();
}

/// <summary>A period summary to keep in a new note of «KNote Summary».</summary>
public class PeriodSummarySaveDto
{
    public PeriodSummaryRequestDto Request { get; set; } = new();
    public string Model { get; set; }
    public string Language { get; set; }
    // What the AI wrote, as it wrote it.
    public string Summary { get; set; }
    public int NotesRead { get; set; }
    public int NotesLeftOut { get; set; }
    public int KNoteNotes { get; set; }
    // To keep the pace of the model (D18, P6).
    public int CharactersRead { get; set; }
    public double Seconds { get; set; }

    // Without the summary: the log of the commands writes the parameter.
    public override string ToString() => $"«{Request?.Subject}», {Model}, {NotesRead} notes, {Summary?.Length ?? 0} characters";
}
