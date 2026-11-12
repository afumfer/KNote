using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// The rules of summarizing the documents of a note with the AI (KNote Atlas, stage 4, D13, 4.4c), as the photos
/// (AiPhotoRules): which documents, what «Describe with AI» does with each one, the request the model receives (the note, the
/// file and the beginning of its text) and its answer, cleaned as a photo's description. The summary goes in the description of
/// the resource: what a person writes is never overwritten by the AI.
/// </summary>
public static class AiDocumentRules
{
    // The beginning of the document the model receives (decided by the owner in 4.4c: some 5 pages).
    public const int MaxDocumentTextLength = 16_000;

    // A document whose text can't be summarized yet: scanned, its pages wait for a transcription (stage 4.5).
    public const string NeedsTranscriptionMessage = "The document is scanned: its summary waits for the transcription of its pages.";
    public const string UnreadableMessage = "The document can't be read: it is protected, encrypted or damaged.";
    public const string FileNotAvailableMessage = "The file of the document is not available in this installation.";

    public static bool IsDocument(ResourceInfoDto resource) => DocumentTextRules.IsDocument(resource);

    /// <summary>Whether «Summarize document» is offered for a resource: a document whose description is not locked.</summary>
    public static bool CanSummarize(ResourceInfoDto resource) => IsDocument(resource) && !resource.DescriptionByUser;

    /// <summary>A document whose description a person wrote and didn't lock: summarizing it would replace that text.</summary>
    public static bool HasPersonDescription(ResourceInfoDto resource) =>
        IsDocument(resource) && ResourceDescriptionRules.IsAtRisk(resource.Description, resource.DescriptionByUser, resource.DescriptionModel, resource.Name);

    /// <summary>
    /// What «Describe with AI» does with a document of the note, as with a photo (AiPhotoRules.PlanForNote): the ones without a
    /// description of their own are summarized; the ones a person described, as chosen; the locked ones, the ones the AI already
    /// summarized and the other files, nothing.
    /// </summary>
    public static EnumPhotoAction PlanForNote(ResourceInfoDto resource, EnumPersonDescriptions choice)
    {
        if (!IsDocument(resource) || resource.DescriptionByUser || !string.IsNullOrEmpty(resource.DescriptionModel))
            return EnumPhotoAction.Skip;
        if (!HasPersonDescription(resource))
            return EnumPhotoAction.Describe;
        return choice switch
        {
            EnumPersonDescriptions.Replace => EnumPhotoAction.Describe,
            EnumPersonDescriptions.Lock => EnumPhotoAction.Lock,
            _ => EnumPhotoAction.Skip
        };
    }

    /// <summary>What «Describe with AI» does with any resource of the note: a photo or a document.</summary>
    public static EnumPhotoAction PlanResourceForNote(ResourceInfoDto resource, EnumPersonDescriptions choice) =>
        AiPhotoRules.IsPhoto(resource) ? AiPhotoRules.PlanForNote(resource, choice) : PlanForNote(resource, choice);

    /// <summary>
    /// Whether a document is summarized when its scanned pages are transcribed (4.5b, decided by the owner): it has no
    /// description of its own (not written by a person nor by the AI, not locked), so its summary was waiting for them.
    /// </summary>
    public static bool WaitsForSummary(ResourceInfoDto resource) =>
        PlanForNote(resource, EnumPersonDescriptions.Skip) == EnumPhotoAction.Describe;

    /// <summary>What a person wrote of this document, as the model receives it (null: nothing).</summary>
    public static string PersonDescription(ResourceInfoDto resource) =>
        HasPersonDescription(resource) ? Cut(OneLine(resource.Description), AiPhotoRules.MaxPersonDescriptionLength) : null;

    /// <summary>
    /// The beginning of a kept text (DocumentTextRules' format) as the model reads it: the pages of a PDF with their number,
    /// the scanned ones with the text of their OCR layer, if any; up to maxLength, cut at a word. Empty: nothing to summarize.
    /// </summary>
    public static string DocumentText(string extractedText, bool paged, int maxLength = MaxDocumentTextLength)
    {
        var pages = DocumentTextRules.Pages(extractedText).Where(p => !string.IsNullOrWhiteSpace(p.Text)).ToList();
        if (pages.Count == 0)
            return "";

        var text = new StringBuilder();
        foreach (var page in pages)
        {
            if (paged)
                text.Append($"[Page {page.Number}]\n");
            text.Append(page.Text.Trim()).Append("\n\n");
            if (text.Length >= maxLength)
                break;
        }
        var result = text.ToString().Trim();
        return result.Length <= maxLength ? result : Cut(result, maxLength);
    }

    /// <summary>
    /// The request of the model: the note (if it has a topic or a text), the file, its pages, what a person wrote of it and the
    /// beginning of its text. The last line is what a small model follows best (measured with the photos in 4.2 and 4.3): the
    /// summary, without mentioning the note, in the language of the note or, without one, of the document.
    /// </summary>
    public static string Request(string noteTopic, string noteText, string fileName, int pages, string documentText,
        string personDescription = null)
    {
        var request = new StringBuilder();
        var hasNote = !string.IsNullOrWhiteSpace(noteTopic) || !string.IsNullOrWhiteSpace(noteText);
        if (hasNote)
        {
            request.Append("The note this document belongs to:\n---\n");
            if (!string.IsNullOrWhiteSpace(noteTopic))
                request.Append($"Topic: {noteTopic.Trim()}\n");
            if (!string.IsNullOrWhiteSpace(noteText))
                request.Append($"\n{noteText.Trim()}\n");
            request.Append("---\n\n");
        }

        if (!string.IsNullOrWhiteSpace(personDescription))
            request.Append($"What a person wrote of this document (it is true: keep its names and facts):\n{personDescription.Trim()}\n\n");

        request.Append($"The document: {fileName}");
        if (pages > 1)
            request.Append($", {pages} pages");
        request.Append(". Its beginning:\n---\n").Append(documentText?.Trim()).Append("\n---\n\n");

        // The language: with only "the language of the note", qwen3.5:9b wrote in the document's (0 of 5 in Spanish with a
        // Spanish topic and English manuals); naming the topic, 5 of 5 (measured in 4.4c).
        request.Append("Summarize this document in two or three sentences");
        if (!string.IsNullOrWhiteSpace(noteTopic))
            request.Append($", without mentioning the note. Write in the language of the topic of the note («{noteTopic.Trim()}»).");
        else if (hasNote)
            request.Append(", without mentioning the note. Write in the language of the text of the note.");
        else
            request.Append(". Write in the language of the document.");
        return request.ToString();
    }

    private static string OneLine(string text) => System.Text.RegularExpressions.Regex.Replace(text ?? "", @"\s+", " ").Trim();

    private static string Cut(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;
        var cut = text.Substring(0, maxLength);
        var space = cut.LastIndexOfAny(new[] { ' ', '\n' });
        if (space > maxLength / 2)
            cut = cut.Substring(0, space);
        return cut.TrimEnd(' ', ',', ';', ':', '\n') + "…";
    }
}
