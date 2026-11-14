using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>How long a summary is (KNote Atlas, stage 2): about these many words.</summary>
public enum EnumSummaryLength
{
    Short = 300,
    Medium = 800,
    Long = 2000
}

/// <summary>
/// A summary of a long document of a note (KNote Atlas, stage 2, CU-43, D21): which document, with which model (the alias of a
/// chat provider of the installation) and in which language and length. An empty language is the one of the document.
/// </summary>
public class DocumentProcessRequestDto
{
    public Guid ResourceId { get; set; }
    public string ProviderAlias { get; set; }
    public string Language { get; set; }
    public EnumSummaryLength Length { get; set; } = EnumSummaryLength.Medium;

    public override string ToString() => $"Resource {ResourceId}, {ProviderAlias}, {Length}, {Language}";
}

/// <summary>
/// A document of a note ready to be summarized (IKntSummaryService.PrepareDocumentAsync): its note, its file and its text
/// (DocumentTextRules' format), with the scanned pages that have no text yet (left out, D21).
/// </summary>
public class DocumentToSummarizeDto
{
    public Guid ResourceId { get; set; }
    // The resource's file as it was read: the summary is only saved if it is still the same.
    public string ResourceName { get; set; }
    public string FileName { get; set; }
    public string FileType { get; set; }
    public bool Paged { get; set; }
    public int PagesCount { get; set; }
    public List<int> MissingPages { get; set; } = new();
    public Guid NoteId { get; set; }
    public int NoteNumber { get; set; }
    public string NoteTopic { get; set; }
    public Guid FolderId { get; set; }
    public string Text { get; set; }

    public override string ToString() => $"{FileName} of the note #{NoteNumber}, {PagesCount} pages";
}

/// <summary>What a summary of a document would take, before it starts (D18).</summary>
public class DocumentProcessEstimateDto
{
    public string FileName { get; set; }
    public int NoteNumber { get; set; }
    public int Pages { get; set; }
    public bool Paged { get; set; }
    public int Characters { get; set; }
    // The calls to the model: one per batch of pages, the ones that join notes that don't fit together, and the final one.
    public int Batches { get; set; }
    public int Calls { get; set; }
    public int ContextTokens { get; set; }
    public List<int> MissingPages { get; set; } = new();
    public string Model { get; set; }
    public bool IsPaid { get; set; }
    // With the pace of the last summary with this model; none before the first one.
    public double? Seconds { get; set; }
}

public enum EnumDocumentProcessPhase
{
    Queued,
    Preparing,
    Reading,
    Joining,
    Writing,
    Saving,
    Completed,
    Canceled,
    Failed
}

/// <summary>How a summary of a document goes (the job of each app, D18).</summary>
public class DocumentProcessProgressDto
{
    public Guid RepositoryId { get; set; }
    public string RepositoryName { get; set; }
    public string RequestedBy { get; set; }
    public Guid ResourceId { get; set; }
    public string FileName { get; set; }
    public string Model { get; set; }
    public EnumDocumentProcessPhase Phase { get; set; }
    public int QueuePosition { get; set; }
    public DateTime? StartedDateTime { get; set; }
    public double ElapsedSeconds { get; set; }
    public double? RemainingSeconds { get; set; }
    // The calls to the model done, of those planned (the joins are known as they come).
    public int CallsDone { get; set; }
    public int CallsTotal { get; set; }
    // The note with the summary (Completed).
    public Guid? NoteId { get; set; }
    public int? NoteNumber { get; set; }
    // Why it failed (Failed).
    public string Message { get; set; }

    public bool IsFinished => Phase is EnumDocumentProcessPhase.Completed or EnumDocumentProcessPhase.Canceled or EnumDocumentProcessPhase.Failed;

    public DocumentProcessProgressDto Copy() => (DocumentProcessProgressDto)MemberwiseClone();
}

/// <summary>The summary of a document written by the AI, to keep in a new note (IKntSummaryService.SaveDocumentSummaryAsync).</summary>
public class DocumentSummarySaveDto
{
    public Guid ResourceId { get; set; }
    // The resource's file when it was read: if it changed meanwhile, nothing is saved.
    public string ExpectedResourceName { get; set; }
    public string Model { get; set; }
    public string Language { get; set; }
    public EnumSummaryLength Length { get; set; }
    // What the AI wrote, as it wrote it.
    public string Summary { get; set; }
    public int PagesCount { get; set; }
    public List<int> MissingPages { get; set; } = new();
    // To keep the pace of the model (D18).
    public int CharactersRead { get; set; }
    public double Seconds { get; set; }

    // Without the summary: the log of the commands writes the parameter.
    public override string ToString() => $"Resource {ResourceId}, {Model}, {Summary?.Length ?? 0} characters";
}

/// <summary>What KNote keeps of the summaries (AI_SUMMARY_STATE): the pace of each model, in seconds per 1,000 characters read.</summary>
public class SummaryStateDto
{
    public Dictionary<string, double> SecondsPer1000Characters { get; set; } = new();
    public DateTime? LastRunDateTime { get; set; }
}
