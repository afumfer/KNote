using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// The texts of the summary (2.1a2) and of the translation (2.1b2) of a long document in both apps (KNote Atlas, stage 2): the window or
/// dialog «Extended summary» / «Translate document», its estimate and its progress. In English, as KNote's interface.
/// </summary>
public static class SummaryTexts
{
    public const string Title = "Extended summary";
    public const string MenuItem = "Extended summary of the document...";
    public const string Estimate = "Estimate";
    public const string Generate = "Generate";
    public const string Cancel = "Cancel";
    public const string OpenNote = "Open the note";
    public const string TranscribeFirst = "Transcribe them first";
    public const string LanguageHelp = "Empty: the language of the document.";
    public const string Intro =
        "The AI reads the whole document by parts and writes its extended summary in a new note, next to the note of the document and related " +
        "to it. The note of the document doesn't change.";
    public const string Running = "An extended summary is being written: it goes on if you close this window.";
    public const string OtherRunning = "Another task of KNote Atlas is running: wait until it ends.";
    public const string IndexRunning = "Updating the search index: wait until it ends.";
    public const string YoursRunning = "You have another extended summary or translation going on: wait until it ends.";

    // The translation (2.1b2).
    public const string TranslateTitle = "Translate document";
    public const string TranslateMenuItem = "Translate the whole document...";
    public const string Continue = "Continue";
    public const string TranslateLanguageHelp = "The language to translate it into.";
    public const string TranslateIntro =
        "The AI translates the whole document page by page into the text of a new note, next to the note of the document and related " +
        "to it. The note of the document doesn't change. If it stops, «Continue» goes on with it.";
    public const string TranslateRunning = "A translation is being written: it goes on if you close this window.";

    // The summary of a period (2.5b): the section «Period summary» of KNote Atlas.
    public const string PeriodTitle = "Period summary";
    public const string PeriodRunning = "A summary is being written: it goes on if you leave this page or close the window.";
    public const string SubjectHelp = "Empty: the period and the subject of the filters.";
    public const string PeriodLanguageHelp = "Empty: the language of the notes.";
    public static string WordsHelp => $"From {PeriodSummaryRules.MinWords} to {PeriodSummaryRules.MaxWords:N0}.";

    public static string TitleOf(EnumDocumentProcessKind kind) => kind switch
    {
        EnumDocumentProcessKind.Translate => TranslateTitle,
        EnumDocumentProcessKind.PeriodSummary => PeriodTitle,
        _ => Title
    };

    public static string IntroOf(EnumDocumentProcessKind kind) => kind == EnumDocumentProcessKind.Translate ? TranslateIntro : Intro;

    public static string RunningOf(EnumDocumentProcessKind kind) => kind switch
    {
        EnumDocumentProcessKind.Translate => TranslateRunning,
        EnumDocumentProcessKind.PeriodSummary => PeriodRunning,
        _ => Running
    };

    public static string LanguageHelpOf(EnumDocumentProcessKind kind) => kind == EnumDocumentProcessKind.Translate ? TranslateLanguageHelp : LanguageHelp;

    /// <summary>A translation not finished of the document, in a line: its language, how far it went and its note.</summary>
    public static string Unfinished(DocumentTranslationDto translation) =>
        $"Not finished: {translation.Language}, {Done(translation)} (note #{translation.NoteNumber}).";

    /// <summary>The question before starting a translation into the language of one not finished.</summary>
    public static string ContinueInstead(DocumentTranslationDto translation) =>
        $"There is a translation into {translation.Language} not finished (note #{translation.NoteNumber}, {Done(translation)}).\n\n" +
        "Continue it instead of starting another one? (No: start another one.)";

    /// <summary>Whether «Open the note» has a note to open: the summary or the translation done, or what was translated of one stopped.</summary>
    public static bool HasNote(DocumentProcessProgressDto progress) =>
        progress?.NoteId != null && (progress.Phase == EnumDocumentProcessPhase.Completed
            || (progress.Kind == EnumDocumentProcessKind.Translate && progress.Phase is EnumDocumentProcessPhase.Canceled or EnumDocumentProcessPhase.Failed));

    private static string Done(DocumentTranslationDto translation) =>
        translation.Paged
            ? (translation.NextPage ?? 1) > 1 ? $"pages 1–{translation.NextPage - 1} of {translation.PagesCount}" : $"part of the page 1 of {translation.PagesCount}"
            : $"{translation.NextPart} of {translation.Parts} parts";

    // The languages offered (any other can be written).
    public static IReadOnlyList<string> Languages { get; } = new[] { "English", "Spanish", "French", "German", "Italian", "Portuguese" };

    public static IReadOnlyList<EnumSummaryLength> Lengths { get; } = new[] { EnumSummaryLength.Short, EnumSummaryLength.Medium, EnumSummaryLength.Long };

    public static string LengthText(EnumSummaryLength length) => length switch
    {
        EnumSummaryLength.Short => $"Short (about {(int)length} words)",
        EnumSummaryLength.Long => $"Long (about {(int)length:N0} words)",
        _ => $"Medium (about {(int)length} words)"
    };

    /// <summary>What the estimate says, line by line.</summary>
    public static List<string> EstimateLines(DocumentProcessEstimateDto estimate, string providerName)
    {
        if (estimate.Kind == EnumDocumentProcessKind.PeriodSummary)
            return PeriodEstimateLines(estimate, providerName);
        var lines = new List<string>
        {
            estimate.Paged
                ? $"{estimate.Pages} pages, {estimate.Characters:N0} characters: {Calls(estimate.Calls)} to the model."
                : $"{estimate.Characters:N0} characters: {Calls(estimate.Calls)} to the model.",
            estimate.Seconds is double seconds ? $"About {Duration(seconds)} with {estimate.Model}."
                : $"{estimate.Model}: no time yet, its first {(estimate.Kind == EnumDocumentProcessKind.Translate ? "translation" : "extended summary")} will say its pace."
        };
        if (estimate.FromPage is int fromPage)
            lines.Insert(0, $"It goes on from the page {fromPage}.");
        if (estimate.MissingPages?.Count > 0)
            lines.Add(MissingPages(estimate.MissingPages));
        if (estimate.IsPaid)
            lines.Add($"The whole document is sent to {providerName ?? "the provider"}, which charges for it.");
        return lines;
    }

    // A period summary: its notes, the doubtful ones asked first, the calls, the time and, of a paid model, that the notes are sent.
    private static List<string> PeriodEstimateLines(DocumentProcessEstimateDto estimate, string providerName)
    {
        var lines = new List<string>
        {
            $"{(estimate.Notes == 1 ? "1 note" : $"{estimate.Notes:N0} notes")}, {estimate.Characters:N0} characters, " +
                $"{(estimate.Sections == 1 ? "1 part" : $"{estimate.Sections} parts")} in «What happened»: {Calls(estimate.Calls)} to the model.",
            estimate.Seconds is double seconds ? $"About {Duration(seconds)} with {estimate.Model}."
                : $"{estimate.Model}: no time yet, its first period summary will say its pace."
        };
        if (estimate.DoubtfulToClassify > 0)
            lines.Insert(1, $"First, the AI is asked about {(estimate.DoubtfulToClassify == 1 ? "1 doubtful note" : $"{estimate.DoubtfulToClassify} doubtful notes")} (in those calls).");
        if (estimate.IsPaid)
            lines.Add($"The notes are sent to {providerName ?? "the provider"}, which charges for it.");
        return lines;
    }

    public static string MissingPages(IReadOnlyCollection<int> pages) =>
        $"Scanned pages without text are left out ({pages.Count}: {LongDocumentRules.Ranges(pages)}). Transcribe them first to include them too.";

    /// <summary>How the summary goes, in a line.</summary>
    public static string Progress(DocumentProcessProgressDto progress)
    {
        if (progress == null)
            return "";
        var what = progress.Phase switch
        {
            EnumDocumentProcessPhase.Queued => progress.QueuePosition > 0 ? $"Waiting in the queue ({progress.QueuePosition})" : "Waiting in the queue",
            EnumDocumentProcessPhase.Preparing => progress.Kind == EnumDocumentProcessKind.PeriodSummary ? "Preparing the notes" : "Preparing the document",
            EnumDocumentProcessPhase.Classifying => $"Asking the AI about the doubtful notes: {progress.CallsDone} of {progress.CallsTotal}",
            EnumDocumentProcessPhase.Reading => progress.Kind == EnumDocumentProcessKind.PeriodSummary
                ? $"Reading the notes: {progress.CallsDone} of {progress.CallsTotal}"
                : $"Reading the document: {progress.CallsDone} of {progress.CallsTotal}",
            EnumDocumentProcessPhase.Joining => $"Joining its notes: {progress.CallsDone} of {progress.CallsTotal}",
            EnumDocumentProcessPhase.Writing => progress.Kind == EnumDocumentProcessKind.PeriodSummary
                ? $"Writing the summary: {progress.CallsDone} of {progress.CallsTotal}" : "Writing the extended summary",
            EnumDocumentProcessPhase.Saving => "Saving the note",
            EnumDocumentProcessPhase.Translating => progress.Page is int page
                ? $"Translating: page {page} of {progress.PagesCount}"
                : $"Translating: part {Math.Min(progress.CallsDone + 1, progress.CallsTotal)} of {progress.CallsTotal}",
            EnumDocumentProcessPhase.Completed => progress.NoteNumber is int number
                ? $"Done: the {What(progress)} is in the note #{number}." : "Done.",
            EnumDocumentProcessPhase.Canceled => progress.NoteNumber is int kept
                ? $"Canceled: what was translated is in the note #{kept}; «Continue» goes on with it." : "Canceled: nothing was kept.",
            EnumDocumentProcessPhase.Failed => progress.NoteNumber is int partial
                ? $"It failed: {progress.Message} What was translated is in the note #{partial}."
                : $"It failed: {progress.Message}",
            _ => progress.Phase.ToString()
        };
        if (!progress.IsFinished && progress.RemainingSeconds is double remaining && remaining >= 1)
            what += $" · about {Duration(remaining)} left";
        return what;
    }

    /// <summary>The share done (0 to 1) of a summary going on.</summary>
    public static double Share(DocumentProcessProgressDto progress) =>
        progress == null || progress.CallsTotal <= 0 ? 0 : Math.Min(1, (double)progress.CallsDone / progress.CallsTotal);

    public static string Duration(double seconds) =>
        seconds < 90 ? $"{Math.Max(1, Math.Round(seconds)):0} s"
        : seconds < 5400 ? $"{Math.Round(seconds / 60):0} min"
        : $"{seconds / 3600:0.#} h";

    private static string Calls(int calls) => calls == 1 ? "1 call" : $"{calls} calls";

    private static string What(DocumentProcessProgressDto progress) => progress.Kind switch
    {
        EnumDocumentProcessKind.Translate => "translation",
        EnumDocumentProcessKind.PeriodSummary => "summary",
        _ => "extended summary"
    };
}
