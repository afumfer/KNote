using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

/// <summary>
/// What the AI reads and writes of the resources of the notes (KNote Atlas, stage 4): the descriptions of the photos (4.2) and,
/// later, the text of the documents. The AI never writes over what a person wrote (D12), and writing doesn't change the date
/// of the note. The resources of KNote's own notes (sessions, chat room) are left out.
/// </summary>
public interface IKntResourceContentService
{
    /// <summary>A photo with its file and the context of its note, to describe it. Staff (who describes).</summary>
    Task<Result<PhotoDescriptionContextDto>> GetPhotoContextAsync(Guid resourceId);

    /// <summary>
    /// What the AI receives to summarize a document (KNote Atlas, stage 4, 4.4c): reads it first if it was not read. Staff.
    /// </summary>
    Task<Result<DocumentSummaryContextDto>> GetDocumentContextAsync(Guid resourceId);

    /// <summary>
    /// Writes the description the AI wrote of a resource, only if it is not locked and has not changed since it was read
    /// (Entity: the resource as it is now). Marks its note for the semantic index. Staff.
    /// </summary>
    Task<Result<ResourceInfoDto>> SaveAiDescriptionAsync(AiDescriptionSaveDto description);

    /// <summary>What «Process pending resources» would process, and the pace of its last run. Guest.</summary>
    Task<Result<ResourceProcessingStatusDto>> GetProcessingStatusAsync();

    /// <summary>
    /// Starts a run of «Process pending resources» (D10): locks the descriptions written by people that were chosen to keep,
    /// and gives the photos to describe. Admin, as the semantic index.
    /// </summary>
    Task<Result<ResourceProcessingPlanDto>> PrepareProcessingAsync(ResourceProcessingOptionsDto options);

    /// <summary>Keeps the pace of a run that ended, for the estimates. Admin.</summary>
    Task<Result> SaveProcessingRunAsync(ResourceProcessingRunDto run);

    /// <summary>
    /// Reads a document of a note now and keeps its text (D2), also in a repository without a semantic index (D10, D18): its
    /// file must be in this installation. Marks its note for the index. Staff (who edits the resources).
    /// </summary>
    Task<Result<DocumentReadDto>> ReadDocumentAsync(Guid resourceId);

    /// <summary>The text read of a document (DocumentTextRules' format). Guest.</summary>
    Task<Result<ResourceTextDto>> GetDocumentTextAsync(Guid resourceId, int maxLength = 0);

    /// <summary>
    /// The scanned documents with pages pending of a transcription (4.5): of the repository, or the one given (an empty list if it
    /// has none). Staff.
    /// </summary>
    Task<Result<List<ScannedDocumentDto>>> GetScannedDocumentsAsync(Guid? resourceId = null);

    /// <summary>
    /// A photo with its file, to draw its thumbnail (KNote Atlas, stage 4, D14: the search of photos), never one of KNote's own
    /// notes. Guest (who sees the notes).
    /// </summary>
    Task<Result<ResourceDto>> GetPhotoFileAsync(Guid resourceId);

    /// <summary>
    /// The text of a document for the assistant (4.7a, read_document): the one kept or, not read yet, read now and not kept. Guest.
    /// </summary>
    Task<Result<ResourceTextDto>> GetDocumentToReadAsync(Guid resourceId);

    /// <summary>A page of a PDF drawn, for the assistant (4.7a, view_document_pages). Guest.</summary>
    Task<Result<ResourceThumbnailDto>> DrawDocumentPageAsync(Guid resourceId, int page);

    /// <summary>What was read of each document of a note (4.7a, get_note_items): its state, pages and scanned pages to transcribe. Guest.</summary>
    Task<Result<List<DocumentReadDto>>> GetNoteDocumentsAsync(Guid noteId);

    /// <summary>A scanned page pending of a transcription, drawn, to transcribe it (4.5). Staff.</summary>
    Task<Result<PageTranscriptionContextDto>> GetPageContextAsync(Guid resourceId, int page);

    /// <summary>
    /// Keeps the transcription of a scanned page (4.5), only if its file is the same and the page is still pending (Entity: what
    /// is left of the document). Marks its note for the semantic index. Staff.
    /// </summary>
    Task<Result<DocumentReadDto>> SaveTranscriptionAsync(PageTranscriptionSaveDto transcription);
}
