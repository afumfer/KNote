using System.Collections.Generic;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// What the two apps say of «Process pending resources» (KNote Atlas, stage 4, D10) and of «Describe with AI» in a note: its
/// status, the question before a run and its progress, so that the Web and ClientWin show the same.
/// </summary>
public static class ResourceProcessingTexts
{
    public const string Title = "Process pending resources";
    public const string DescribeWithAi = "Describe with AI";

    public const string Description = "The AI describes the photos and summarizes the documents of the notes that have no description " +
        "of their own, and KNote reads the text of the documents not read yet, so that the search finds them by what they show or " +
        "say. What a person wrote and locked is never changed, and none of this changes the date of a note.";

    public const string PhotosOption = "Photos: describe them";
    public const string DocumentsOption = "Documents: read their text and summarize them";

    // The choice for the descriptions written by people that are not locked (the same as «Describe with AI» in a note).
    public const string PersonChoiceSkip = "Leave them as they are (not described)";
    public const string PersonChoiceLock = "Keep them and lock them";
    public const string PersonChoiceReplace = "Replace them with the AI's, written from them (it can't be undone)";

    public static string Counts(ResourceProcessingStatusDto status) =>
        $"Photos: {status.PendingPhotos} without a description of their own · {status.PersonPhotos} with one written by a person, not locked. " +
        $"Documents: {status.PendingDocuments} without a description of their own · {status.PersonDocuments} with one written by a person, " +
        $"not locked · {status.DocumentsToRead} not read yet.";

    // Null before the first run.
    public static string LastRun(ResourceProcessingStatusDto status)
    {
        if (status.LastRunDateTime == null)
            return null;
        var paces = new List<string>();
        if (status.SecondsPerPhoto is double photo)
            paces.Add($"{SemanticIndexTexts.Duration(photo)} per photo");
        if (status.SecondsPerDocument is double document)
            paces.Add($"{SemanticIndexTexts.Duration(document)} per document");
        return $"Last run: {status.LastRunDateTime.Value:g}" + (paces.Count > 0 ? " · " + string.Join(" · ", paces) : "");
    }

    /// <summary>The resources with a description a person wrote and didn't lock (the question of the choice), or null if none.</summary>
    public static string PersonDescriptions(int photos, int documents)
    {
        if (photos + documents == 0)
            return null;
        var what = photos > 0 && documents > 0 ? $"{Photos(photos)} and {Documents(documents)}"
            : photos > 0 ? Photos(photos) : Documents(documents);
        return $"{what} {(photos + documents == 1 ? "has" : "have")} a description written by a person, not locked. They can't be told " +
            "apart from one written by hand:";
    }

    /// <summary>The photos and the documents a run would describe and summarize with these options.</summary>
    public static (int Photos, int Documents) ToProcess(ResourceProcessingStatusDto status, ResourceProcessingOptionsDto options)
    {
        var replace = options.PersonDescriptions == EnumPersonDescriptions.Replace;
        return (options.Photos ? status.PendingPhotos + (replace ? status.PersonPhotos : 0) : 0,
            options.Documents ? status.PendingDocuments + (replace ? status.PersonDocuments : 0) : 0);
    }

    /// <summary>Whether a run has something to do with these options (to describe, to summarize, to read or to lock).</summary>
    public static bool HasWork(ResourceProcessingStatusDto status, ResourceProcessingOptionsDto options)
    {
        var (photos, documents) = ToProcess(status, options);
        var toLock = options.PersonDescriptions == EnumPersonDescriptions.Lock
            && ((options.Photos ? status.PersonPhotos : 0) + (options.Documents ? status.PersonDocuments : 0)) > 0;
        return photos + documents > 0 || toLock || options.Documents && status.DocumentsToRead > 0;
    }

    /// <summary>
    /// What a run is going to do, before it starts: how many photos and documents, with which model, how long and, with a paid
    /// model, that they are sent.
    /// </summary>
    public static string RunConfirmation(ResourceProcessingStatusDto status, ResourceProcessingOptionsDto options, ResourceDescriberDto describer)
    {
        var (photos, documents) = ToProcess(status, options);
        var parts = new List<string>();
        if (photos > 0)
            parts.Add($"describes {Photos(photos)}");
        if (documents > 0)
            parts.Add($"summarizes {Documents(documents)}");
        var ai = parts.Count > 0 ? $"The AI {string.Join(" and ", parts)} with the model {describer.Model}." : "";
        var read = options.Documents && status.DocumentsToRead > 0 ? $" KNote reads the text of {Documents(status.DocumentsToRead)} first (without the AI)." : "";
        var estimate = parts.Count > 0
            && ResourceProcessingRules.EstimateSeconds(photos, status.SecondsPerPhoto, documents, status.SecondsPerDocument) is double seconds
            ? $" It should take about {SemanticIndexTexts.Duration(seconds)}."
            : "";
        var toLock = (options.Photos ? status.PersonPhotos : 0) + (options.Documents ? status.PersonDocuments : 0);
        var locked = options.PersonDescriptions == EnumPersonDescriptions.Lock && toLock > 0
            ? $" {toLock} descriptions written by people are locked to keep them."
            : "";
        var paid = describer.IsPaid && parts.Count > 0
            ? $" {describer.Model} is a paid model: these {(photos > 0 && documents > 0 ? "photos and documents" : photos > 0 ? "photos" : "documents")} are sent to its provider."
            : "";
        return $"{ai}{read}{estimate}{locked}{paid} You can go on working meanwhile; canceling keeps what is done.".Trim();
    }

    public static string Progress(ResourceProcessingProgressDto progress) => progress.Phase switch
    {
        EnumResourceProcessingPhase.Queued => "Queued" + (progress.QueuePosition > 0 ? $" (position {progress.QueuePosition})" : "")
            + ": one run of KNote Atlas at a time.",
        EnumResourceProcessingPhase.Preparing => "Preparing…",
        EnumResourceProcessingPhase.Processing => progress.Step switch
        {
            EnumResourceProcessingStep.ReadingDocuments => $"Reading documents: {progress.DocumentsRead} of {progress.DocumentsToReadTotal}",
            EnumResourceProcessingStep.Documents => $"Summarizing documents: {progress.DocumentsDone} of {progress.DocumentsTotal}",
            _ => $"Describing photos: {progress.PhotosDone} of {progress.PhotosTotal}"
        } + (progress.RemainingSeconds is double left && left > 0 ? $" · about {SemanticIndexTexts.Duration(left)} left" : ""),
        EnumResourceProcessingPhase.Completed => $"Done: {Done(progress)} in {SemanticIndexTexts.Duration(progress.ElapsedSeconds)}" + Extra(progress) + ".",
        EnumResourceProcessingPhase.Canceled => $"Canceled after {Done(progress)}{Extra(progress)}: what is done is kept.",
        _ => $"Failed: {progress.Message}"
    };

    /// <summary>The value and the maximum of the progress bar of the step being done.</summary>
    public static (int Value, int Max) Bar(ResourceProcessingProgressDto progress) => progress.Step switch
    {
        EnumResourceProcessingStep.ReadingDocuments => (progress.DocumentsRead, progress.DocumentsToReadTotal),
        EnumResourceProcessingStep.Documents => (progress.DocumentsDone, progress.DocumentsTotal),
        _ => (progress.PhotosDone, progress.PhotosTotal)
    };

    public static string Errors(ResourceProcessingProgressDto progress) =>
        progress.ErrorsCount == 0 ? null : $"{progress.ErrorsCount} {(progress.ErrorsCount == 1 ? "resource" : "resources")} failed (tried again in the next run):";

    private static string Done(ResourceProcessingProgressDto progress)
    {
        var parts = new List<string>();
        if (progress.PhotosTotal > 0 || progress.DocumentsTotal == 0 && progress.DocumentsToReadTotal == 0)
            parts.Add($"{Photos(progress.PhotosDescribed)} described");
        if (progress.DocumentsTotal > 0)
            parts.Add($"{Documents(progress.DocumentsSummarized)} summarized");
        if (progress.DocumentsToReadTotal > 0)
            parts.Add($"{Documents(progress.DocumentsRead)} read");
        return string.Join(", ", parts);
    }

    private static string Extra(ResourceProcessingProgressDto progress) =>
        (progress.PhotosLocked > 0 ? $" · {progress.PhotosLocked} locked" : "")
        + (progress.DocumentsWaiting > 0 ? $" · {Documents(progress.DocumentsWaiting)} scanned, waiting for transcription" : "")
        + (progress.DocumentsUnreadable > 0 ? $" · {Documents(progress.DocumentsUnreadable)} that can't be read" : "")
        + (progress.NotAvailable > 0 ? $" · {progress.NotAvailable} not available here" : "");

    public static string Photos(int count) => count == 1 ? "1 photo" : $"{count} photos";

    public static string Documents(int count) => count == 1 ? "1 document" : $"{count} documents";
}
