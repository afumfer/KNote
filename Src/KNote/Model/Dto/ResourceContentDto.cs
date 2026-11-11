using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>
/// What the AI needs to describe a photo of a note (KNote Atlas, stage 4, D13): the resource with its file, and the context of
/// its note (its topic, the beginning of its text as Markdown and the descriptions people wrote of its other photos), and what a
/// person wrote of this photo, if it is not locked: describing it uses that text (who is in it, where it is) instead of losing it.
/// </summary>
public class PhotoDescriptionContextDto
{
    public ResourceDto Resource { get; set; }
    public string NoteTopic { get; set; }
    public string NoteText { get; set; }
    public List<string> PersonDescriptions { get; set; } = new();
    public string PhotoDescription { get; set; }
}

/// <summary>
/// What the AI receives to transcribe a scanned page of a PDF (KNote Atlas 4.5): the page drawn as a JPEG
/// (AiTranscriptionRules.PageImageSide) and what is checked when the transcription is saved (the file is the same).
/// </summary>
public class PageTranscriptionContextDto
{
    public Guid ResourceId { get; set; }
    public Guid NoteId { get; set; }
    public string FileName { get; set; }
    public int Page { get; set; }
    public int PagesCount { get; set; }
    public byte[] Image { get; set; }
    public string ContentType { get; set; } = "image/jpeg";
    public string ExpectedName { get; set; }
}

/// <summary>The transcription of a scanned page, saved only if the file is the same and the page is still pending.</summary>
public class PageTranscriptionSaveDto
{
    public Guid ResourceId { get; set; }
    public int Page { get; set; }
    public string Text { get; set; }
    public string ExpectedName { get; set; }

    // Without the text: the log of the commands writes the parameter.
    public override string ToString() => $"Resource {ResourceId}, page {Page}, {Text?.Length ?? 0} characters";
}

/// <summary>
/// The description the AI wrote of a resource, and what the resource was when it was read: it is written only if it is still
/// so and not locked (D12).
/// </summary>
public class AiDescriptionSaveDto
{
    public Guid ResourceId { get; set; }
    public string Description { get; set; }
    public string Model { get; set; }
    public string ExpectedDescription { get; set; }
    public string ExpectedName { get; set; }

    public override string ToString() => $"{ResourceId} ({Model})";
}

/// <summary>
/// The model that describes photos in this installation (the repository's recommended LLM if it is configured here, else the
/// default chat one): what the apps show before describing. Never its host or key. Message: why it can't, if it can't.
/// </summary>
public class ResourceDescriberDto
{
    public string Provider { get; set; }
    public string Model { get; set; }
    public bool IsPaid { get; set; }
    public bool SupportsImages { get; set; }
    public string Message { get; set; }

    public bool Available => string.IsNullOrEmpty(Message);

    // The documents only need a model, not one that sees images (4.4c).
    public bool CanSummarize => !string.IsNullOrEmpty(Model);
}

/// <summary>
/// What the AI receives to summarize a document (IKntResourceContentService.GetDocumentContextAsync, KNote Atlas 4.4c): the
/// resource (without its content), the note, what a person wrote of it, how many pages and the beginning of its text.
/// </summary>
public class DocumentSummaryContextDto
{
    public ResourceInfoDto Resource { get; set; }
    public string NoteTopic { get; set; }
    public string NoteText { get; set; }
    public string PersonDescription { get; set; }
    public int Pages { get; set; }
    public string DocumentText { get; set; }
}

/// <summary>What «Describe photos» does with the photos of a note whose description a person wrote and didn't lock.</summary>
public enum EnumPersonDescriptions
{
    Replace,
    Lock,
    Skip
}

/// <summary>What «Describe photos» does with a resource of the note.</summary>
public enum EnumPhotoAction
{
    Skip,
    Describe,
    Lock
}
