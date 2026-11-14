using System;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

/// <summary>
/// The summaries of KNote Atlas (stage 2): for now, of a long document of a note (CU-43, D21). The AI writes in KNote.Ai
/// (AiDocumentProcessor); here, what it reads and where it is kept. The result always goes to a new note: the note of the
/// document is never touched, nor its date. The documents of KNote's own notes are left out.
/// </summary>
public interface IKntSummaryService
{
    /// <summary>
    /// A document of a note ready to be summarized: its text (read now and kept, if it was not read) and its scanned pages without
    /// text. Staff.
    /// </summary>
    Task<Result<DocumentToSummarizeDto>> PrepareDocumentAsync(Guid resourceId);

    /// <summary>The pace of each model in the summaries, for the estimates (D18). Guest.</summary>
    Task<Result<SummaryStateDto>> GetStateAsync();

    /// <summary>
    /// Keeps the summary of a document in a new note of the type @KNoteSummary in the folder of the note of the document, related
    /// to it (@DerivedFrom), with the summary as the AI wrote it attached (D17), and the pace of the model. Only if the document
    /// is still the one that was read. Staff.
    /// </summary>
    Task<Result<NoteDto>> SaveDocumentSummaryAsync(DocumentSummarySaveDto summary);
}
