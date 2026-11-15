using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// The texts of the summary of a long document in both apps (KNote Atlas, stage 2, 2.1a2): the window or dialog «Summarize document»,
/// its estimate and its progress. In English, as KNote's interface.
/// </summary>
public static class SummaryTexts
{
    public const string Title = "Summarize document";
    public const string MenuItem = "Summarize the whole document...";
    public const string Estimate = "Estimate";
    public const string Generate = "Generate";
    public const string Cancel = "Cancel";
    public const string OpenNote = "Open the note";
    public const string TranscribeFirst = "Transcribe them first";
    public const string LanguageHelp = "Empty: the language of the document.";
    public const string Intro =
        "The AI reads the whole document by parts and writes its summary in a new note, next to the note of the document and related " +
        "to it. The note of the document doesn't change.";
    public const string Running = "A summary is being written: it goes on if you close this window.";
    public const string OtherRunning = "Another task of KNote Atlas is running: wait until it ends.";

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
        var lines = new List<string>
        {
            estimate.Paged
                ? $"{estimate.Pages} pages, {estimate.Characters:N0} characters: {Calls(estimate.Calls)} to the model."
                : $"{estimate.Characters:N0} characters: {Calls(estimate.Calls)} to the model.",
            estimate.Seconds is double seconds ? $"About {Duration(seconds)} with {estimate.Model}." : $"{estimate.Model}: no time yet, its first summary will say its pace."
        };
        if (estimate.MissingPages?.Count > 0)
            lines.Add(MissingPages(estimate.MissingPages));
        if (estimate.IsPaid)
            lines.Add($"The whole document is sent to {providerName ?? "the provider"}, which charges for it.");
        return lines;
    }

    public static string MissingPages(IReadOnlyCollection<int> pages) =>
        $"Scanned pages without text are left out ({pages.Count}: {LongDocumentRules.Ranges(pages)}). Transcribe them first to summarize them too.";

    /// <summary>How the summary goes, in a line.</summary>
    public static string Progress(DocumentProcessProgressDto progress)
    {
        if (progress == null)
            return "";
        var what = progress.Phase switch
        {
            EnumDocumentProcessPhase.Queued => progress.QueuePosition > 0 ? $"Waiting in the queue ({progress.QueuePosition})" : "Waiting in the queue",
            EnumDocumentProcessPhase.Preparing => "Preparing the document",
            EnumDocumentProcessPhase.Reading => $"Reading the document: {progress.CallsDone} of {progress.CallsTotal}",
            EnumDocumentProcessPhase.Joining => $"Joining its notes: {progress.CallsDone} of {progress.CallsTotal}",
            EnumDocumentProcessPhase.Writing => "Writing the summary",
            EnumDocumentProcessPhase.Saving => "Saving the note",
            EnumDocumentProcessPhase.Completed => progress.NoteNumber is int number ? $"Done: the summary is in the note #{number}." : "Done.",
            EnumDocumentProcessPhase.Canceled => "Canceled: nothing was kept.",
            EnumDocumentProcessPhase.Failed => $"It failed: {progress.Message}",
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
}
