using System;
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
    Task<Result<ResourceTextDto>> GetDocumentTextAsync(Guid resourceId);
}
