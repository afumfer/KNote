using System;
using System.Collections.Generic;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// process_document, the action of the assistant that starts the extended summary or the translation of a document of a note
/// (KNote Atlas, stage 2, 2.1b2b, D22): what the model writes (the task and the length), the line of the card and of the record,
/// and what the model is told. The run is the one of «Extended summary» / «Translate document», in the background. In English, as
/// KNote's interface.
/// </summary>
public static class AiDocumentProcessRules
{
    /// <summary>The task the model asks for: a summary or a translation.</summary>
    public static (EnumDocumentProcessKind? Kind, string Error) Task(string task) => (task ?? "").Trim().ToLowerInvariant() switch
    {
        "summary" or "summarize" or "summarise" or "extended summary" or "resumen" => (EnumDocumentProcessKind.Summarize, null),
        "translation" or "translate" or "traduccion" or "traducción" => (EnumDocumentProcessKind.Translate, null),
        _ => (null, $"Error: '{task}' is not a task: use summary or translation.")
    };

    /// <summary>The length of a summary: short, medium (by default) or long.</summary>
    public static (EnumSummaryLength? Length, string Error) Length(string length) => (length ?? "").Trim().ToLowerInvariant() switch
    {
        "" or "medium" => (EnumSummaryLength.Medium, null),
        "short" => (EnumSummaryLength.Short, null),
        "long" => (EnumSummaryLength.Long, null),
        _ => (null, $"Error: '{length}' is not a length: use short, medium or long.")
    };

    /// <summary>The line of the card and of the record of the conversation (it goes to the model in every turn: one line).</summary>
    public static string Text(EnumDocumentProcessKind kind, string fileName, int noteNumber, string language, EnumSummaryLength length,
        DocumentTranslationDto continued = null)
    {
        if (kind == EnumDocumentProcessKind.Translate)
            return continued != null
                ? $"Continue the translation of {fileName} (#{noteNumber}) into {continued.Language} in the note #{continued.NoteNumber}"
                : $"Translate {fileName} (#{noteNumber}) into {language?.Trim()}, in a new note";
        var inLanguage = string.IsNullOrWhiteSpace(language) ? "" : $" in {language.Trim()}";
        return $"Write an extended summary of {fileName} (#{noteNumber}){inLanguage}, {LengthName(length)}, in a new note";
    }

    /// <summary>What the card adds under the line: the estimate and that it runs in the background.</summary>
    public static string CardDetail(DocumentProcessEstimateDto estimate, string providerName) =>
        string.Join("\n\n", new List<string>(SummaryTexts.EstimateLines(estimate, providerName)) { RunsInBackground });

    public const string RunsInBackground =
        "It runs in the background: you can go on working, and opening «Extended summary» or «Translate document» of the document shows how it goes.";

    /// <summary>What the model is told when the user accepted it: it was started, not done.</summary>
    public const string Started = "Started in the background, NOT finished: the new note will be ready when it ends, and the user can follow it " +
        "in «Extended summary» or «Translate document» of the document. Do not say that it is done.";

    public const string NotFromAttachment =
        "is a file of this conversation, not of a note: attach it to a note first with attach_file (the user confirms it), then ask again.";

    public const string NoModel = "Error: there is no AI model here to summarize or translate documents: tell the user to configure a chat provider.";

    public const string LanguageRequired = "Error: say into which language to translate it (ask the user if they didn't say).";

    private static string LengthName(EnumSummaryLength length) => length switch
    {
        EnumSummaryLength.Short => "short",
        EnumSummaryLength.Long => "long",
        _ => "medium"
    };
}
