using System;
using System.Collections.Generic;
using System.IO;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Core;

/// <summary>
/// Reads the text of a document (KNote Atlas, stage 4, D7): the pages of a PDF, with the ones that need a transcription
/// (DocumentTextRules.NeedsTranscription), or the text of a DOCX, an ODT or a TXT as one page. Service doesn't reference
/// KNote.Ai, which implements it (AiDocumentReader): each host gives one to its IKntService (IKntService.DocumentReader), as
/// the embedding models. No AI: the indexer uses it.
/// </summary>
public interface IKntDocumentReader
{
    /// <summary>
    /// The pages of the document (the stream can be read from its start; it is not closed). A document that can't be read (a
    /// protected PDF, an encrypted ODT, a damaged file) throws KntDocumentUnreadableException: it is not tried again.
    /// </summary>
    List<DocumentPageDto> Read(Stream content, EnumDocumentFormat format);

    /// <summary>
    /// A page of a PDF drawn as a JPEG on white, with its longest side of <paramref name="maxSide"/> pixels, to transcribe it
    /// (4.5). A page that can't be drawn throws KntDocumentUnreadableException; a system where PDFium can't run (KNote Local on
    /// another platform), PlatformNotSupportedException.
    /// </summary>
    byte[] DrawPage(Stream content, int pageNumber, int maxSide);
}

/// <summary>A document KNote can't read, whatever is tried: it is kept as EnumExtractedState.Unreadable.</summary>
public class KntDocumentUnreadableException : Exception
{
    public KntDocumentUnreadableException(string message, Exception innerException = null) : base(message, innerException)
    {
    }
}
