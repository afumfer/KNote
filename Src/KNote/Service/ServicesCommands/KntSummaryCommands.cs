using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

// KNote Atlas, stage 2 (CU-43, D21): a document of a note ready to be summarized. A document not read yet is read now and its
// text kept, as «Read document» does (the index reads it too).
[KntAuthorize(EnumRoles.Staff)]
public class KntSummaryPrepareDocumentAsyncCommand : KntCommandServiceBase<Guid, Result<DocumentToSummarizeDto>>
{
    public KntSummaryPrepareDocumentAsyncCommand(IKntService service, Guid resourceId) : base(service, resourceId)
    {

    }

    public override async Task<Result<DocumentToSummarizeDto>> Execute()
    {
        var result = new Result<DocumentToSummarizeDto>();

        var (resource, note) = await KntResourceContent.GetDescribableAsync(Service, Param, result);
        if (!result.IsValid)
            return result;
        var format = DocumentTextRules.FormatOf(resource.FileType, resource.Name);
        if (format == EnumDocumentFormat.None)
        {
            result.AddErrorMessage(KntSummaryTexts.NotADocument);
            return result;
        }

        var kept = await Repository.Notes.GetResourceTextAsync(Param);
        if (!kept.IsValid)
        {
            result.AddListErrorMessage(kept.ListErrorMessage);
            return result;
        }
        var text = kept.Entity;
        if (text == null || text.State == EnumExtractedState.NotRead)
        {
            var (outcome, read) = await KntDocuments.ReadAndSaveAsync(Service, Param);
            var error = KntSummaryTexts.ReadError(outcome);
            if (error != null)
            {
                result.AddErrorMessage(error);
                return result;
            }
            text = read;
        }
        if (text.State == EnumExtractedState.Unreadable)
        {
            result.AddErrorMessage(AiDocumentRules.UnreadableMessage);
            return result;
        }

        var paged = DocumentTextRules.IsPaged(format);
        var pages = DocumentTextRules.Pages(text.Text);
        result.Entity = new DocumentToSummarizeDto
        {
            ResourceId = resource.ResourceId,
            ResourceName = resource.Name,
            FileName = resource.NameOut,
            FileType = resource.FileType,
            Paged = paged,
            PagesCount = paged ? pages.Count : 1,
            MissingPages = SummaryPieces.FromDocument(text.Text, paged).MissingPages,
            NoteId = note.NoteId,
            NoteNumber = note.NoteNumber,
            NoteTopic = note.Topic,
            FolderId = note.FolderId,
            Text = text.Text ?? ""
        };
        if (string.IsNullOrWhiteSpace(result.Entity.Text) || pages.All(p => string.IsNullOrWhiteSpace(p.Text)))
            result.AddErrorMessage(KntSummaryTexts.NoText);
        return result;
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntSummaryGetStateAsyncCommand : KntCommandServiceBase<Result<SummaryStateDto>>
{
    public KntSummaryGetStateAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<SummaryStateDto>> Execute() =>
        new Result<SummaryStateDto>(await KntSummaryState.ReadAsync(Service) ?? new SummaryStateDto());
}

// The summary of a document in a new note (D17, D21): of the type @KNoteSummary, in the folder of the note of the document and
// related to it; the summary as the AI wrote it, attached and locked, so that the stage 4 doesn't describe it. The note of the
// document is not touched (nor its date). The type of note and the type of relation are created the first time.
[KntAuthorize(EnumRoles.Staff)]
public class KntSummarySaveDocumentSummaryAsyncCommand : KntCommandServiceBase<DocumentSummarySaveDto, Result<NoteDto>>
{
    private const string BypassReason = "KNote Atlas: the type of the summaries and of their relations (stage 2)";

    public KntSummarySaveDocumentSummaryAsyncCommand(IKntService service, DocumentSummarySaveDto summary) : base(service, summary)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null || Param.ResourceId == Guid.Empty)
            result.AddErrorMessage("The document is required.");
        else if (string.IsNullOrWhiteSpace(Param.Summary))
            result.AddErrorMessage("The AI gave no summary.");
        else if (string.IsNullOrWhiteSpace(Param.Model))
            result.AddErrorMessage("The model that wrote the summary is required.");
        return result;
    }

    public override async Task<Result<NoteDto>> Execute()
    {
        var result = new Result<NoteDto>();

        var (resource, note) = await KntResourceContent.GetDescribableAsync(Service, Param.ResourceId, result);
        if (!result.IsValid)
            return result;
        if (resource.Name != Param.ExpectedResourceName)
        {
            result.AddErrorMessage(KntSummaryTexts.DocumentChanged);
            return result;
        }

        var summaryTypeId = await KntSystemNotes.EnsureNoteTypeAsync(Service, KntConst.SummaryTag,
            "Summaries written by KNote Atlas", BypassReason, result);
        var derivedFromId = await KntSystemNotes.EnsureTraceNoteTypeAsync(Service, KntConst.DerivedFromTraceNoteType,
            "Where a summary of KNote Atlas comes from", BypassReason, result);
        if (!result.IsValid)
            return result;

        var newNote = await Service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = summaryTypeId });
        if (!newNote.IsValid)
        {
            result.AddListErrorMessage(newNote.ListErrorMessage);
            return result;
        }

        var now = DateTime.Now;
        var document = new DocumentToSummarizeDto
        {
            FileName = resource.NameOut,
            Paged = DocumentTextRules.IsPaged(DocumentTextRules.FormatOf(resource.FileType, resource.Name)),
            PagesCount = Param.PagesCount,
            NoteNumber = note.NoteNumber
        };
        var summary = newNote.Entity;
        summary.Topic = LongDocumentRules.NoteTopic(resource.NameOut);
        summary.Description = LongDocumentRules.NoteDescription(document, Param, now);
        summary.Tags = "";
        summary.NoteTypeId = summaryTypeId;
        summary.FolderId = note.FolderId;
        summary.FolderDto = note.FolderDto;
        summary.Resources.Add(new ResourceDto
        {
            Name = $"{Guid.NewGuid()}_{LongDocumentRules.OriginalFileName(resource.NameOut)}",
            Description = LongDocumentRules.OriginalDescription(Param.Model, now),
            DescriptionByUser = true,
            FileType = "text/markdown",
            ContentArrayBytes = Encoding.UTF8.GetBytes(Param.Summary)
        });
        summary.TraceNotesTo.Add(new TraceNoteDto { ToId = note.NoteId, TraceNoteTypeId = derivedFromId, Weight = 1.0 });

        var saved = await Service.Notes.SaveExtendedAsync(summary);
        if (!saved.IsValid)
        {
            result.AddListErrorMessage(saved.ListErrorMessage);
            return result;
        }

        // The pace of the model, for the next estimates (D18).
        var state = await KntSummaryState.ReadAsync(Service) ?? new SummaryStateDto();
        if (SummaryEstimate.Pace(Param.CharactersRead, Param.Seconds) is double pace)
            state.SecondsPer1000Characters[Param.Model] = pace;
        state.LastRunDateTime = now;
        await KntSummaryState.WriteAsync(Service, state);

        result.Entity = saved.Entity.GetSimpleDto<NoteDto>();
        return result;
    }
}

// The texts of the commands of the summaries, common to both apps.
public static class KntSummaryTexts
{
    public const string NotADocument = "It is not a document KNote reads (PDF, DOCX, ODT, TXT or MD).";
    public const string NoText = "The document has no text to summarize: if it is scanned, transcribe its pages first.";
    public const string DocumentChanged = "The document was replaced while it was being summarized: nothing was saved.";

    public static string ReadError(EnumDocumentReadOutcome outcome) => outcome switch
    {
        EnumDocumentReadOutcome.Read => null,
        EnumDocumentReadOutcome.Unreadable => AiDocumentRules.UnreadableMessage,
        EnumDocumentReadOutcome.NoReader => "This installation can't read documents.",
        EnumDocumentReadOutcome.NotAvailable => AiDocumentRules.FileNotAvailableMessage,
        EnumDocumentReadOutcome.Changed => "Its file was replaced while it was being read: try again.",
        _ => NotADocument
    };
}
