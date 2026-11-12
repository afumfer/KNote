using System;

namespace KNote.Model.Dto;

/// <summary>What KNote has read of a document (Resources.ExtractedState, KNote Atlas stage 4, D2).</summary>
public enum EnumExtractedState
{
    // Not read yet.
    NotRead = 0,
    // Read, with pages that need a transcription (scanned): they have the text of their OCR layer, if any, until then.
    Partial = 1,
    // Read.
    Complete = 2,
    // It can't be read (a protected PDF, an encrypted ODT, a damaged file): it is not tried again.
    Unreadable = 3
}

/// <summary>A page of a document as a reader gives it (a document without pages, DOCX, ODT or TXT, is one).</summary>
public class DocumentPageDto
{
    public int Number { get; set; }
    public string Text { get; set; }
    // A scanned page (DocumentTextRules.NeedsTranscription): its text, if any, is the one of its OCR layer.
    public bool NeedsTranscription { get; set; }
    // A scanned page transcribed by the AI (KNote Atlas 4.5): its text is the transcription.
    public bool Transcribed { get; set; }
}

/// <summary>What «Read document» did (IKntResourceContentService.ReadDocumentAsync).</summary>
public class DocumentReadDto
{
    public Guid ResourceId { get; set; }
    public EnumExtractedState State { get; set; }
    public int Pages { get; set; }
    // Scanned pages, read when they are transcribed (stage 4.5); meanwhile, with the text of their OCR layer if they have one.
    public int PagesToTranscribe { get; set; }
}

/// <summary>
/// The text read of a resource (Resources.ExtractedText, DocumentTextRules' format), out of the queries of the resources,
/// which never read it: it can be of hundreds of pages. To show it, only its beginning may come (DocumentTextRules.Cut):
/// PagesShown of PagesCount.
/// </summary>
public class ResourceTextDto
{
    public Guid ResourceId { get; set; }
    public Guid NoteId { get; set; }
    public string Name { get; set; }
    public string FileType { get; set; }
    public EnumExtractedState State { get; set; }
    public string Text { get; set; }
    public int PagesShown { get; set; }
    public int PagesCount { get; set; }
}
