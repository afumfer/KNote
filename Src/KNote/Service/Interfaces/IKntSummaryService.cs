using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

/// <summary>
/// The summaries of KNote Atlas (stage 2): for now, of a long document of a note (CU-43, D21), and its translations (2.1b). The AI writes in KNote.Ai
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

    /// <summary>
    /// Keeps a part of the translation of a document (2.1b): the first one creates the note of the type @KNoteTranslation in the folder
    /// of the note of the document, related to it (@DerivedFrom), with the translation as its text (2.1b2); the next ones are added
    /// to that text, only if it goes on from where it was. Only if the document is still the one that was read. Staff.
    /// </summary>
    Task<Result<NoteDto>> SaveDocumentTranslationAsync(DocumentTranslationSaveDto translation);

    /// <summary>The translations of a document, the last one first, finished or not («Continue»). Guest.</summary>
    Task<Result<List<DocumentTranslationDto>>> GetDocumentTranslationsAsync(Guid resourceId);

    /// <summary>The noise policy of the repository (2.2, D3), without the types and folders that no longer exist. Guest.</summary>
    Task<Result<NoisePolicyDto>> GetNoisePolicyAsync();

    /// <summary>Keeps the noise policy, without the types and folders that don't exist nor KNote's own ones (always out). ProjectManager.</summary>
    Task<Result<NoisePolicyDto>> SaveNoisePolicyAsync(NoisePolicyDto policy);

    /// <summary>How many notes this policy (saved or not) leaves out of the summaries, and why. Guest.</summary>
    Task<Result<NoiseCountDto>> CountNoiseAsync(NoisePolicyDto policy);

    /// <summary>The selection of the notes of a summary (2.3, CU-15), each one with its decision and reason, without the AI; nothing is
    /// written. Staff.</summary>
    Task<Result<NoteSelectionResultDto>> PrepareSelectionAsync(NoteSelectionRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Keeps the changes of «Review selection» in «Note relevance» (created the first time), for the current user; it gives
    /// the corrections as they are kept. Staff.</summary>
    Task<Result<List<RelevanceCorrectionDto>>> SaveRelevanceCorrectionsAsync(RelevanceCorrectionsSaveDto changes);

    /// <summary>Doubtful notes as the AI reads them (2.4): their data, the beginning of their text and their fingerprint. Staff.</summary>
    Task<Result<List<NoteToClassifyDto>>> GetNotesToClassifyAsync(List<Guid> noteIds);

    /// <summary>Keeps the answers of the AI in «Note relevance» (each one replaces the note's one). Staff.</summary>
    Task<Result<List<RelevanceAiAnswerDto>>> SaveRelevanceAiAnswersAsync(RelevanceAiAnswersSaveDto answers);

    /// <summary>A period summary ready to be read (2.5): its selection and what the AI reads of each note that goes in. Staff.</summary>
    Task<Result<PeriodSummaryPreparedDto>> PreparePeriodAsync(PeriodSummaryRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Keeps a period summary in a new note @KNoteSummary of «KNote Summary» (2.6: the document of PeriodSummaryDocument, with
    /// what KNote calculates), related to each note it comes from (@DerivedFrom), with the original of the AI attached. Staff.</summary>
    Task<Result<NoteDto>> SavePeriodSummaryAsync(PeriodSummarySaveDto summary);
}
