using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        var paged = DocumentTextRules.IsPaged(format, text.Text);
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
            Paged = DocumentTextRules.IsPaged(DocumentTextRules.FormatOf(resource.FileType, resource.Name), Param.PagesCount),
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

// KNote Atlas, stage 2 (CU-43, D21, 2.1b): a part of the translation of a document. The first one creates the note of the translation
// (@KNoteTranslation, in the folder of the note of the document and related to it, with the translation as its text, 2.1b2); the next
// ones are added to that text, only if it goes on from where it was: its header is written again and what somebody changed in the
// translation is kept. The note keeps how far it went in a hidden comment (DocumentTranslationRules.Marker). The note of the document
// is not touched (nor its date).
[KntAuthorize(EnumRoles.Staff)]
public class KntSummarySaveDocumentTranslationAsyncCommand : KntCommandServiceBase<DocumentTranslationSaveDto, Result<NoteDto>>
{
    private const string BypassReason = "KNote Atlas: the type of the translations and of their relations (stage 2)";

    public KntSummarySaveDocumentTranslationAsyncCommand(IKntService service, DocumentTranslationSaveDto translation) : base(service, translation)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null || Param.ResourceId == Guid.Empty)
            result.AddErrorMessage("The document is required.");
        else if (string.IsNullOrWhiteSpace(Param.Language))
            result.AddErrorMessage(KntSummaryTexts.LanguageRequired);
        else if (string.IsNullOrWhiteSpace(Param.Model))
            result.AddErrorMessage("The model that translated is required.");
        else if (Param.Text == null || Param.Parts <= 0 || Param.FromPart < 0 || Param.NextPart <= Param.FromPart || Param.NextPart > Param.Parts)
            result.AddErrorMessage("The part of the translation is not valid.");
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

        var saved = Param.NoteId is Guid noteId
            ? await AddAsync(noteId, resource, note, result)
            : await CreateAsync(resource, note, result);
        if (!result.IsValid)
            return result;

        // The pace of the model, for the next estimates (D18).
        if (SummaryEstimate.Pace(Param.CharactersTranslated, Param.Seconds) is double pace)
        {
            var state = await KntSummaryState.ReadAsync(Service) ?? new SummaryStateDto();
            state.TranslationSecondsPer1000Characters[Param.Model] = pace;
            state.LastRunDateTime = DateTime.Now;
            await KntSummaryState.WriteAsync(Service, state);
        }

        result.Entity = saved;
        return result;
    }

    private DocumentTranslationDto State(ResourceDto resource) => new()
    {
        Format = DocumentTranslationDto.CurrentFormat,
        ResourceId = resource.ResourceId,
        ResourceName = resource.Name,
        Language = Param.Language.Trim(),
        Model = Param.Model,
        Paged = Param.Paged,
        PagesCount = Param.PagesCount,
        Parts = Param.Parts,
        NextPart = Param.NextPart,
        NextPage = Param.NextPage
    };

    private async Task<NoteDto> CreateAsync(ResourceDto resource, NoteDto note, Result<NoteDto> result)
    {
        if (Param.FromPart != 0)
        {
            result.AddErrorMessage(KntSummaryTexts.TranslationChanged);
            return null;
        }

        var typeId = await KntSystemNotes.EnsureNoteTypeAsync(Service, KntConst.TranslationTag,
            "Translations written by KNote Atlas", BypassReason, result);
        var derivedFromId = await KntSystemNotes.EnsureTraceNoteTypeAsync(Service, KntConst.DerivedFromTraceNoteType,
            "Where a summary of KNote Atlas comes from", BypassReason, result);
        if (!result.IsValid)
            return null;

        var newNote = await Service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = typeId });
        if (!newNote.IsValid)
        {
            result.AddListErrorMessage(newNote.ListErrorMessage);
            return null;
        }

        var translation = newNote.Entity;
        translation.Topic = DocumentTranslationRules.NoteTopic(resource.NameOut, Param.Language);
        translation.Description = Description(State(resource), resource, note, Param.Text);
        translation.Tags = "";
        translation.NoteTypeId = typeId;
        translation.FolderId = note.FolderId;
        translation.FolderDto = note.FolderDto;
        translation.TraceNotesTo.Add(new TraceNoteDto { ToId = note.NoteId, TraceNoteTypeId = derivedFromId, Weight = 1.0 });

        var saved = await Service.Notes.SaveExtendedAsync(translation);
        if (!saved.IsValid)
        {
            result.AddListErrorMessage(saved.ListErrorMessage);
            return null;
        }
        return saved.Entity.GetSimpleDto<NoteDto>();
    }

    private async Task<NoteDto> AddAsync(Guid noteId, ResourceDto resource, NoteDto note, Result<NoteDto> result)
    {
        var resTranslation = await Service.Notes.GetAsync(noteId);
        var translation = resTranslation.IsValid ? resTranslation.Entity : null;
        var stored = translation?.NoteTypeDto?.Name == KntConst.TranslationTag ? DocumentTranslationRules.ReadMarker(translation.Description) : null;
        if (stored == null || stored.ResourceId != resource.ResourceId || stored.ResourceName != resource.Name
            || stored.NextPart != Param.FromPart || stored.Parts != Param.Parts
            || !string.Equals(stored.Language, Param.Language.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            result.AddErrorMessage(KntSummaryTexts.TranslationChanged);
            return null;
        }

        // What was translated (with what somebody changed in it) and the new part, under the header of now.
        translation.Description = Description(State(resource), resource, note, DocumentTranslationRules.Body(translation.Description) + Param.Text);
        var saved = await Service.Notes.SaveAsync(translation);
        if (!saved.IsValid)
        {
            result.AddListErrorMessage(saved.ListErrorMessage);
            return null;
        }
        return saved.Entity;
    }

    private string Description(DocumentTranslationDto state, ResourceDto resource, NoteDto note, string body) =>
        DocumentTranslationRules.NoteDescription(state, resource.NameOut, note.NoteNumber, Param.MissingPages, DateTime.Now, body);
}

// KNote Atlas, stage 2 (2.1b): the translations of a document (the notes @KNoteTranslation related to its note whose hidden comment
// says that document), the last one first, to go on with one that is not finished («Continue»).
[KntAuthorize(EnumRoles.Guest)]
public class KntSummaryGetDocumentTranslationsAsyncCommand : KntCommandServiceBase<Guid, Result<List<DocumentTranslationDto>>>
{
    public KntSummaryGetDocumentTranslationsAsyncCommand(IKntService service, Guid resourceId) : base(service, resourceId)
    {

    }

    public override async Task<Result<List<DocumentTranslationDto>>> Execute()
    {
        var result = new Result<List<DocumentTranslationDto>>();

        var (resource, note) = await KntResourceContent.GetDescribableAsync(Service, Param, result);
        if (!result.IsValid)
            return result;

        var relations = await Service.Notes.GetTraceNotesFromAsync(note.NoteId);
        if (!relations.IsValid)
        {
            result.AddListErrorMessage(relations.ListErrorMessage);
            return result;
        }

        var translations = new List<DocumentTranslationDto>();
        foreach (var fromId in (relations.Entity ?? new()).Select(r => r.FromId).Distinct())
        {
            var from = await Service.Notes.GetAsync(fromId);
            if (!from.IsValid || from.Entity?.NoteTypeDto?.Name != KntConst.TranslationTag)
                continue;
            var translation = DocumentTranslationRules.ReadMarker(from.Entity.Description);
            if (translation == null || translation.ResourceId != resource.ResourceId)
                continue;
            translation.NoteId = from.Entity.NoteId;
            translation.NoteNumber = from.Entity.NoteNumber;
            translations.Add(translation);
        }

        result.Entity = translations.OrderByDescending(t => t.NoteNumber).ToList();
        return result;
    }
}

// KNote Atlas, stage 2 (2.2, D3): the noise policy of the repository, without the types and folders that no longer exist.
[KntAuthorize(EnumRoles.Guest)]
public class KntSummaryGetNoisePolicyAsyncCommand : KntCommandServiceBase<Result<NoisePolicyDto>>
{
    public KntSummaryGetNoisePolicyAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<NoisePolicyDto>> Execute()
    {
        var context = await KntNoisePolicy.LoadContextAsync(Service);
        return new Result<NoisePolicyDto>(context.Clean(await KntNoisePolicy.ReadAsync(Service)));
    }
}

// The noise policy kept: without the types and folders that don't exist, nor KNote's own ones (always out, D4). It gives what was kept.
[KntAuthorize(EnumRoles.ProjectManager)]
public class KntSummarySaveNoisePolicyAsyncCommand : KntCommandServiceBase<NoisePolicyDto, Result<NoisePolicyDto>>
{
    public KntSummarySaveNoisePolicyAsyncCommand(IKntService service, NoisePolicyDto policy) : base(service, policy)
    {

    }

    public override async Task<Result<NoisePolicyDto>> Execute()
    {
        var result = new Result<NoisePolicyDto>();
        var context = await KntNoisePolicy.LoadContextAsync(Service);
        var policy = context.Clean(Param);
        var written = await KntNoisePolicy.WriteAsync(Service, policy);
        if (!written.IsValid)
        {
            result.AddListErrorMessage(written.ListErrorMessage);
            return result;
        }
        result.Entity = policy;
        return result;
    }
}

// How many notes a noise policy (the one being edited, saved or not) leaves out of the summaries, and why (NoiseRules).
[KntAuthorize(EnumRoles.Guest)]
public class KntSummaryCountNoiseAsyncCommand : KntCommandServiceBase<NoisePolicyDto, Result<NoiseCountDto>>
{
    public KntSummaryCountNoiseAsyncCommand(IKntService service, NoisePolicyDto policy) : base(service, policy)
    {

    }

    public override async Task<Result<NoiseCountDto>> Execute()
    {
        var result = new Result<NoiseCountDto>();
        var notes = await Repository.Notes.GetClassificationsAsync();
        if (!notes.IsValid)
        {
            result.AddListErrorMessage(notes.ListErrorMessage);
            return result;
        }
        var context = await KntNoisePolicy.LoadContextAsync(Service);
        result.Entity = NoiseRules.Count(notes.Entity, context.Clean(Param), context.KNoteNoteTypeIds, context.KNoteFolderIds);
        return result;
    }
}

// KNote Atlas, stage 2 (2.3, CU-15): the selection of the notes of a summary, without the AI (KntNoteSelection); nothing is written.
[KntAuthorize(EnumRoles.Staff)]
public class KntSummaryPrepareSelectionAsyncCommand : KntCommandServiceBase<NoteSelectionRequestDto, Result<NoteSelectionResultDto>>
{
    private readonly CancellationToken _cancellationToken;

    public KntSummaryPrepareSelectionAsyncCommand(IKntService service, NoteSelectionRequestDto request, CancellationToken cancellationToken)
        : base(service, request)
    {
        _cancellationToken = cancellationToken;
    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null)
            result.AddErrorMessage("The period of the selection is required.");
        else if (Param.To.Date < Param.From.Date)
            result.AddErrorMessage(KntNoteSelection.PeriodError);
        return result;
    }

    public override async Task<Result<NoteSelectionResultDto>> Execute()
    {
        var result = new Result<NoteSelectionResultDto>();
        result.Entity = await KntNoteSelection.BuildAsync(Service, Param, _cancellationToken, result);
        return result;
    }
}

// The changes of «Review selection» kept in «Note relevance» (KntRelevanceNotes, D10, S3, S4): a Staff writes that protected
// system note through this command (inside KntAuthorizationBypass); a change of a note of KNote's own is not kept.
[KntAuthorize(EnumRoles.Staff)]
public class KntSummarySaveRelevanceCorrectionsAsyncCommand : KntCommandServiceBase<RelevanceCorrectionsSaveDto, Result<List<RelevanceCorrectionDto>>>
{
    public KntSummarySaveRelevanceCorrectionsAsyncCommand(IKntService service, RelevanceCorrectionsSaveDto changes) : base(service, changes)
    {

    }

    public override async Task<Result<List<RelevanceCorrectionDto>>> Execute()
    {
        var result = new Result<List<RelevanceCorrectionDto>>();
        var changes = (Param?.Changes ?? new List<RelevanceCorrectionChangeDto>())
            .GroupBy(c => c.NoteId).Select(g => g.Last()).ToList();

        // KNote's own notes are always out: they are not corrected.
        var context = await KntNoisePolicy.LoadContextAsync(Service);
        var notes = await Repository.Notes.GetClassificationsAsync();
        if (!notes.IsValid)
        {
            result.AddListErrorMessage(notes.ListErrorMessage);
            return result;
        }
        var kNoteNotes = notes.Entity
            .Where(n => (n.NoteTypeId is Guid t && context.KNoteNoteTypeIds.Contains(t)) || context.KNoteFolderIds.Contains(n.FolderId))
            .Select(n => n.NoteId).ToHashSet();
        changes = changes.Where(c => !kNoteNotes.Contains(c.NoteId)).ToList();

        result.Entity = await KntRelevanceNotes.SaveCorrectionsAsync(Service, changes, result);
        return result;
    }
}

// KNote Atlas, stage 2 (2.4, D9): doubtful notes as the AI reads them, with the same fingerprint as the selection.
[KntAuthorize(EnumRoles.Staff)]
public class KntSummaryGetNotesToClassifyAsyncCommand : KntCommandServiceBase<List<Guid>, Result<List<NoteToClassifyDto>>>
{
    public KntSummaryGetNotesToClassifyAsyncCommand(IKntService service, List<Guid> noteIds) : base(service, noteIds)
    {

    }

    public override async Task<Result<List<NoteToClassifyDto>>> Execute()
    {
        var result = new Result<List<NoteToClassifyDto>>();
        result.Entity = await KntNoteSelection.ToClassifyAsync(Service, Param, result);
        return result;
    }
}

// The answers of the AI kept in «Note relevance» (KntRelevanceNotes, D10), for a Staff (inside KntAuthorizationBypass). The answers
// of notes of KNote's own are not kept.
[KntAuthorize(EnumRoles.Staff)]
public class KntSummarySaveRelevanceAiAnswersAsyncCommand : KntCommandServiceBase<RelevanceAiAnswersSaveDto, Result<List<RelevanceAiAnswerDto>>>
{
    public KntSummarySaveRelevanceAiAnswersAsyncCommand(IKntService service, RelevanceAiAnswersSaveDto answers) : base(service, answers)
    {

    }

    public override async Task<Result<List<RelevanceAiAnswerDto>>> Execute()
    {
        var result = new Result<List<RelevanceAiAnswerDto>>();
        var answers = (Param?.Answers ?? new List<RelevanceAiAnswerDto>()).Where(a => a.NoteId != Guid.Empty)
            .GroupBy(a => a.NoteId).Select(g => g.Last()).ToList();

        var context = await KntNoisePolicy.LoadContextAsync(Service);
        var notes = await Repository.Notes.GetClassificationsAsync();
        if (!notes.IsValid)
        {
            result.AddListErrorMessage(notes.ListErrorMessage);
            return result;
        }
        var kNoteNotes = notes.Entity
            .Where(n => (n.NoteTypeId is Guid t && context.KNoteNoteTypeIds.Contains(t)) || context.KNoteFolderIds.Contains(n.FolderId))
            .Select(n => n.NoteId).ToHashSet();
        answers = answers.Where(a => !kNoteNotes.Contains(a.NoteId)).ToList();

        await KntRelevanceNotes.SaveAiAnswersAsync(Service, answers, result);
        if (result.IsValid)
            result.Entity = answers;
        return result;
    }
}

// KNote Atlas, stage 2 (2.5, CU-16): a period summary ready to be read (KntNoteSelection.PreparePeriodAsync); nothing is written.
[KntAuthorize(EnumRoles.Staff)]
public class KntSummaryPreparePeriodAsyncCommand : KntCommandServiceBase<PeriodSummaryRequestDto, Result<PeriodSummaryPreparedDto>>
{
    private readonly CancellationToken _cancellationToken;

    public KntSummaryPreparePeriodAsyncCommand(IKntService service, PeriodSummaryRequestDto request, CancellationToken cancellationToken)
        : base(service, request)
    {
        _cancellationToken = cancellationToken;
    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param?.Selection == null)
            result.AddErrorMessage("The period of the summary is required.");
        else if (Param.Selection.To.Date < Param.Selection.From.Date)
            result.AddErrorMessage(KntNoteSelection.PeriodError);
        return result;
    }

    public override async Task<Result<PeriodSummaryPreparedDto>> Execute()
    {
        var result = new Result<PeriodSummaryPreparedDto>();
        result.Entity = await KntNoteSelection.PreparePeriodAsync(Service, Param, _cancellationToken, result);
        return result;
    }
}

// A period summary in a new note (2.5, D5, D17, P5): of the type @KNoteSummary, in the folder «KNote Summary» (created the first time,
// not a system folder: the summaries are notes as any other), with the summary as the AI wrote it attached and locked, so that the
// stage 4 doesn't describe it. No note of the period is touched (nor its date). Its relations with each note come in 2.6.
[KntAuthorize(EnumRoles.Staff)]
public class KntSummarySavePeriodSummaryAsyncCommand : KntCommandServiceBase<PeriodSummarySaveDto, Result<NoteDto>>
{
    private const string BypassReason = "KNote Atlas: the type and the folder of the summaries (stage 2)";

    public KntSummarySavePeriodSummaryAsyncCommand(IKntService service, PeriodSummarySaveDto summary) : base(service, summary)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param?.Request?.Selection == null)
            result.AddErrorMessage("The period of the summary is required.");
        else if (string.IsNullOrWhiteSpace(Param.Executive) && (Param.Sections?.All(s => string.IsNullOrWhiteSpace(s.Text)) ?? true))
            result.AddErrorMessage("The AI gave no summary.");
        else if (Param.Prepared?.Notes == null || Param.Prepared.Notes.Count == 0)
            result.AddErrorMessage(PeriodSummaryRules.NoNotes);
        else if (string.IsNullOrWhiteSpace(Param.Model))
            result.AddErrorMessage("The model that wrote the summary is required.");
        return result;
    }

    public override async Task<Result<NoteDto>> Execute()
    {
        var result = new Result<NoteDto>();

        var summaryTypeId = await KntSystemNotes.EnsureNoteTypeAsync(Service, KntConst.SummaryTag,
            "Summaries written by KNote Atlas", BypassReason, result);
        var folder = await KntSystemNotes.EnsureFolderAsync(Service, KntConst.SummaryFolderName, BypassReason, result);
        var derivedFromId = await KntSystemNotes.EnsureTraceNoteTypeAsync(Service, KntConst.DerivedFromTraceNoteType,
            "Where a summary of KNote Atlas comes from", BypassReason, result);
        if (!result.IsValid)
            return result;

        // The notes it comes from that still exist and are not KNote's own: the caller is not trusted (D17).
        var classifications = await Service.Repository.Notes.GetClassificationsAsync();
        if (!classifications.IsValid)
        {
            result.AddListErrorMessage(classifications.ListErrorMessage);
            return result;
        }
        var context = await KntNoisePolicy.LoadContextAsync(Service);
        var sources = classifications.Entity
            .Where(n => !context.KNoteFolderIds.Contains(n.FolderId) && !(n.NoteTypeId is Guid t && context.KNoteNoteTypeIds.Contains(t)))
            .Select(n => n.NoteId).ToHashSet();

        var newNote = await Service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = summaryTypeId });
        if (!newNote.IsValid)
        {
            result.AddListErrorMessage(newNote.ListErrorMessage);
            return result;
        }

        string folderPath = null;
        if (Param.Request.Selection.FolderId is Guid folderId)
        {
            var path = await Service.Folders.GetPathAsync(folderId);
            folderPath = path.IsValid ? path.Entity : null;
        }

        var now = DateTime.Now;
        var subject = string.IsNullOrWhiteSpace(Param.Request.Subject) ? PeriodSummaryRules.DefaultSubject(Param.Request.Selection) : Param.Request.Subject.Trim();
        var summary = newNote.Entity;
        summary.Topic = subject;
        summary.Description = PeriodSummaryDocument.Build(Param, folderPath, now);
        summary.Tags = "";
        summary.NoteTypeId = summaryTypeId;
        var folderDto = await Service.Folders.GetAsync(folder.FolderId);
        if (!folderDto.IsValid)
        {
            result.AddListErrorMessage(folderDto.ListErrorMessage);
            return result;
        }
        summary.FolderId = folder.FolderId;
        summary.FolderDto = folderDto.Entity;
        summary.Resources.Add(new ResourceDto
        {
            Name = $"{Guid.NewGuid()}_{PeriodSummaryRules.OriginalFileName(subject)}",
            Description = PeriodSummaryRules.OriginalDescription(Param.Model, now),
            DescriptionByUser = true,
            FileType = "text/markdown",
            ContentArrayBytes = Encoding.UTF8.GetBytes(PeriodSummaryDocument.Original(Param))
        });
        // A relation with each note it comes from, in the order they are cited and with their score (D17).
        foreach (var source in PeriodSummaryDocument.Sources(Param).Where(s => sources.Contains(s.NoteId)))
            summary.TraceNotesTo.Add(new TraceNoteDto { ToId = source.NoteId, TraceNoteTypeId = derivedFromId, Order = source.Order, Weight = source.Weight });

        var saved = await Service.Notes.SaveExtendedAsync(summary);
        if (!saved.IsValid)
        {
            result.AddListErrorMessage(saved.ListErrorMessage);
            return result;
        }

        // The pace of the model in a period summary, for the next estimates (D18, P6).
        var state = await KntSummaryState.ReadAsync(Service) ?? new SummaryStateDto();
        if (SummaryEstimate.Pace(Param.CharactersRead, Param.Seconds) is double pace)
            state.PeriodSecondsPer1000Characters[Param.Model] = pace;
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
    public const string DocumentChanged = "The document was replaced while it was being processed: nothing was saved.";
    public const string LanguageRequired = "The language of the translation is required.";
    public const string TranslationChanged = "The translation is not where it was left (its note or its document changed): start it again.";

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
