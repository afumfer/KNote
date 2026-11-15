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

/// <summary>What is done with a long document (KNote Atlas, stage 2, CU-43): its summary or its translation.</summary>
public enum EnumDocumentProcessKind
{
    Summarize,
    Translate
}

/// <summary>
/// A summary or a translation of a long document of a note (KNote Atlas, stage 2, CU-43, D21): which document, with which model
/// (the alias of a chat provider of the installation) and in which language and length (of a summary). An empty language is the
/// one of the document (a summary; a translation needs one). ContinueNoteId: the translation that goes on (2.1b, «Continue»), in
/// the language it was started with.
/// </summary>
public class DocumentProcessRequestDto
{
    public Guid ResourceId { get; set; }
    public EnumDocumentProcessKind Kind { get; set; } = EnumDocumentProcessKind.Summarize;
    public string ProviderAlias { get; set; }
    public string Language { get; set; }
    public EnumSummaryLength Length { get; set; } = EnumSummaryLength.Medium;
    public Guid? ContinueNoteId { get; set; }

    public override string ToString() => $"{Kind} resource {ResourceId}, {ProviderAlias}, {Length}, {Language}, continue {ContinueNoteId}";
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

/// <summary>What a summary or a translation of a document would take, before it starts (D18).</summary>
public class DocumentProcessEstimateDto
{
    public EnumDocumentProcessKind Kind { get; set; }
    public string FileName { get; set; }
    public int NoteNumber { get; set; }
    public int Pages { get; set; }
    public bool Paged { get; set; }
    public int Characters { get; set; }
    // The calls to the model: one per batch of pages, the ones that join notes that don't fit together, and the final one. Of a
    // translation: one per part (a page, or the parts of a text without pages that fit together) of what is left to translate.
    public int Batches { get; set; }
    public int Calls { get; set; }
    public int ContextTokens { get; set; }
    public List<int> MissingPages { get; set; } = new();
    public string Model { get; set; }
    public bool IsPaid { get; set; }
    // With the pace of the last summary (or translation) with this model; none before the first one.
    public double? Seconds { get; set; }
    // A translation that goes on: the page it goes on from (a document with pages).
    public int? FromPage { get; set; }
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
    Failed,
    // A translation (2.1b): its parts, one after another.
    Translating
}

/// <summary>How a summary or a translation of a document goes (the job of each app, D18).</summary>
public class DocumentProcessProgressDto
{
    public EnumDocumentProcessKind Kind { get; set; }
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
    // The note with the summary (Completed) or the translation (also Canceled or Failed: what was translated is kept).
    public Guid? NoteId { get; set; }
    public int? NoteNumber { get; set; }
    // Of a translation of a document with pages: the page it is at, of how many.
    public int? Page { get; set; }
    public int PagesCount { get; set; }
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

/// <summary>
/// A part of the translation of a document to keep (IKntSummaryService.SaveDocumentTranslationAsync, 2.1b): the first one creates the
/// note of the translation (NoteId empty); the next ones are added to it, only if it goes on from where it was (FromPart).
/// </summary>
public class DocumentTranslationSaveDto
{
    public Guid ResourceId { get; set; }
    // The resource's file when it was read: if it changed meanwhile, nothing is saved.
    public string ExpectedResourceName { get; set; }
    public Guid? NoteId { get; set; }
    public string Model { get; set; }
    public string Language { get; set; }
    public bool Paged { get; set; }
    public int PagesCount { get; set; }
    public List<int> MissingPages { get; set; } = new();
    // The parts of the document (DocumentTranslationRules.Parts): the first one of this text and the one that comes after it.
    public int Parts { get; set; }
    public int FromPart { get; set; }
    public int NextPart { get; set; }
    // The page of the next part (a document with pages).
    public int? NextPage { get; set; }
    // What is added to the translation, as it goes in its file (with its page breaks).
    public string Text { get; set; }
    // To keep the pace of the model, with the last part (D18).
    public int CharactersTranslated { get; set; }
    public double Seconds { get; set; }

    public bool Finished => NextPart >= Parts;

    // Without the text: the log of the commands writes the parameter.
    public override string ToString() => $"Resource {ResourceId}, note {NoteId}, {Language}, parts {FromPart}-{NextPart} of {Parts}, {Text?.Length ?? 0} characters";
}

/// <summary>
/// A translation of a document (2.1b): where it is (its note and the document attached to it) and how far it went. It is what the
/// note of the translation keeps in a hidden comment (DocumentTranslationRules.Marker): without it, it can't go on.
/// </summary>
public class DocumentTranslationDto
{
    public Guid ResourceId { get; set; }
    public string ResourceName { get; set; }
    public Guid NoteId { get; set; }
    public int NoteNumber { get; set; }
    public Guid TranslationResourceId { get; set; }
    public string Language { get; set; }
    public string Model { get; set; }
    public bool Paged { get; set; }
    public int PagesCount { get; set; }
    public int Parts { get; set; }
    public int NextPart { get; set; }
    public int? NextPage { get; set; }

    public bool Finished => NextPart >= Parts;

    public override string ToString() => $"Translation into {Language} in the note #{NoteNumber}, part {NextPart} of {Parts}";
}

/// <summary>
/// What KNote keeps of the summaries (AI_SUMMARY_STATE): the pace of each model, in seconds per 1,000 characters read, and of the
/// translations, per 1,000 characters translated (2.1b: a translation depends on what the model writes).
/// </summary>
public class SummaryStateDto
{
    public Dictionary<string, double> SecondsPer1000Characters { get; set; } = new();
    public Dictionary<string, double> TranslationSecondsPer1000Characters { get; set; } = new();
    public DateTime? LastRunDateTime { get; set; }
}
