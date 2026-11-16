using System.ComponentModel;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;

namespace KNote.Ai;

// KNote Atlas, stage 2 (2.1b2b, D22): process_document, the extended summary or the translation of a whole document of a note in a
// new note. Only proposed, always with its card (no «Don't ask again»): it takes long and, with a paid model, it costs. The card
// says the estimate with the model the application uses for it (the repository's recommended LLM, IKNoteAiToolsHost); accepted, the
// host starts it in the background (the jobs of the Server, or Store.DocumentProcessing in ClientWin). Only documents of notes: a
// file of the conversation is attached to a note first (attach_file). A translation not finished into that language goes on.
public partial class KNoteAiTools
{
    private AITool GetProcessDocumentTool() => AIFunctionFactory.Create(ProcessDocumentAsync, new AIFunctionFactoryOptions
    {
        Name = AiActionRules.ProcessDocument,
        Description = "Starts, in the background, an extended summary or a translation of a whole long document attached to a note " +
            "(PDF, DOCX, ODT, TXT or MD), written in a new note next to it. Use it when the user asks for an extended summary, for a " +
            "summary or a translation in a new note, or to summarize or translate a whole document, or one too long to read here with " +
            "read_document: do not read it to write that summary or translation yourself. It takes minutes or hours, so it is proposed " +
            "to the user, who has to confirm it: until then it is not done, and when it is accepted it only starts."
    });

    [Description("Proposes the extended summary or the translation of a document of a note.")]
    private async Task<string> ProcessDocumentAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123).")]
        int noteNumber,
        [Description("What to do: summary (an extended summary of the whole document) or translation (of the whole document).")]
        string task,
        [Description("The name of the document. Without it, the only document of the note.")]
        string fileName = null,
        [Description("The language, in English (e.g. Spanish). Required for a translation; for a summary, by default the language of the document.")]
        string language = null,
        [Description("The length of a summary: short (about 300 words), medium (about 800) or long (about 2,000). By default, medium.")]
        string length = null)
    {
        var (kind, taskError) = AiDocumentProcessRules.Task(task);
        if (taskError != null)
            return taskError;
        var (summaryLength, lengthError) = AiDocumentProcessRules.Length(length);
        if (lengthError != null)
            return lengthError;
        if (kind == EnumDocumentProcessKind.Translate && string.IsNullOrWhiteSpace(language))
            return AiDocumentProcessRules.LanguageRequired;

        var (note, noteError) = await FindCitedNoteAsync(noteNumber);
        if (noteError != null)
            return noteError;
        var resources = await _service.Notes.GetResourcesInfoAsync(note.NoteId);
        if (!resources.IsValid)
            return $"Error reading the resources of the note: {resources.ErrorMessage}";
        var documents = (resources.Entity ?? new()).Where(DocumentTextRules.IsDocument).ToList();
        var (document, findError) = AiDocumentToolRules.Find(documents, r => r.NameOut, fileName, $"The note #{noteNumber}");
        if (findError != null)
        {
            var attachedDocuments = _attachments.Where(a => DocumentTextRules.IsDocument(a.ContentType, a.FileName)).ToList();
            var attached = string.IsNullOrWhiteSpace(fileName)
                ? (documents.Count == 0 ? attachedDocuments.LastOrDefault() : null)
                : attachedDocuments.FirstOrDefault(a => string.Equals(a.FileName, fileName.Trim(), StringComparison.OrdinalIgnoreCase));
            return attached != null ? $"Error: {attached.FileName} {AiDocumentProcessRules.NotFromAttachment}" : findError;
        }

        var provider = await _host.GetDocumentModelAsync(_service);
        if (provider == null)
            return AiDocumentProcessRules.NoModel;

        // A translation into that language not finished goes on, as «Generate» proposes in the window.
        DocumentTranslationDto continued = null;
        if (kind == EnumDocumentProcessKind.Translate)
        {
            var translations = await _service.Summaries.GetDocumentTranslationsAsync(document.ResourceId);
            continued = (translations.IsValid ? translations.Entity : null)?.FirstOrDefault(t => !t.Finished
                && string.Equals(t.Language?.Trim(), language.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        var request = new DocumentProcessRequestDto
        {
            ResourceId = document.ResourceId,
            Kind = kind.Value,
            ProviderAlias = provider.Alias,
            Language = (continued?.Language ?? language)?.Trim(),
            Length = summaryLength.Value,
            ContinueNoteId = continued?.NoteId
        };
        var estimate = await AiDocumentProcessor.EstimateAsync(_service, provider, request);
        if (!estimate.IsValid)
            return $"Error: {estimate.ErrorMessage}";

        return await ProposeAsync(new AiActionDto
        {
            Kind = AiActionRules.ProcessDocument,
            Text = AiDocumentProcessRules.Text(kind.Value, document.NameOut, noteNumber, request.Language, summaryLength.Value, continued),
            Content = AiDocumentProcessRules.CardDetail(estimate.Entity, provider.Provider),
            NoteId = note.NoteId,
            NoteNumber = noteNumber,
            ResourceId = document.ResourceId,
            ProcessKind = kind,
            Language = request.Language,
            Length = summaryLength,
            ContinueNoteId = continued?.NoteId
        });
    }

    // Accepted in the card: the host starts it with the model of now (the one of the card may have changed meanwhile).
    private async Task StartDocumentProcessAsync(IKntService service, AiActionDto action)
    {
        try
        {
            var provider = await _host.GetDocumentModelAsync(service);
            if (provider == null || action.ResourceId is not Guid resourceId || action.ProcessKind is not EnumDocumentProcessKind kind)
            {
                action.Status = EnumAiActionStatus.Failed;
                action.Result = provider == null ? AiDocumentProcessRules.NoModel["Error: ".Length..] : "The document is not known.";
                return;
            }

            var error = await _host.StartDocumentProcessAsync(service, provider, new DocumentProcessRequestDto
            {
                ResourceId = resourceId,
                Kind = kind,
                ProviderAlias = provider.Alias,
                Language = action.Language,
                Length = action.Length ?? EnumSummaryLength.Medium,
                ContinueNoteId = action.ContinueNoteId
            });
            action.Status = error == null ? EnumAiActionStatus.Done : EnumAiActionStatus.Failed;
            action.Result = error;
        }
        catch (Exception ex)
        {
            action.Status = EnumAiActionStatus.Failed;
            action.Result = ex.Message;
        }
    }
}
