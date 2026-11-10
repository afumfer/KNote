using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// What the two apps say of «Process pending resources» (KNote Atlas, stage 4, D10): its status, the question before a run
/// and its progress, so that the Web and ClientWin show the same.
/// </summary>
public static class ResourceProcessingTexts
{
    public const string Title = "Process pending resources";

    public const string Description = "The AI describes the photos of the notes that have no description of their own, so that " +
        "the search by meaning finds them by what they show. What a person wrote and locked is never changed, and describing a " +
        "photo doesn't change the date of its note.";

    // The choice for the descriptions written by people that are not locked (the same as «Describe photos» in a note).
    public const string PersonChoiceSkip = "Leave them as they are (not described)";
    public const string PersonChoiceLock = "Keep them and lock them";
    public const string PersonChoiceReplace = "Replace them with the AI's, written from them (it can't be undone)";

    public static string Counts(ResourceProcessingStatusDto status) =>
        $"{Photos(status.PendingPhotos)} without a description of its own · {status.PersonPhotos} with a description written by a person, not locked";

    // Null before the first run.
    public static string LastRun(ResourceProcessingStatusDto status) =>
        status.LastRunDateTime == null
            ? null
            : $"Last run: {status.LastRunDateTime.Value:g}"
              + (status.SecondsPerPhoto is double pace ? $" · {SemanticIndexTexts.Duration(pace)} per photo" : "");

    public static string PersonPhotos(int count) =>
        $"{Photos(count)} {(count == 1 ? "has" : "have")} a description written by a person, not locked. They can't be told apart " +
        "from one written by hand:";

    /// <summary>The photos a run would describe with this choice.</summary>
    public static int PhotosToDescribe(ResourceProcessingStatusDto status, EnumPersonDescriptions choice) =>
        status.PendingPhotos + (choice == EnumPersonDescriptions.Replace ? status.PersonPhotos : 0);

    /// <summary>What a run is going to do, before it starts: how many photos, with which model, how long and, with a paid model, that they are sent.</summary>
    public static string RunConfirmation(ResourceProcessingStatusDto status, EnumPersonDescriptions choice, ResourceDescriberDto describer)
    {
        var photos = PhotosToDescribe(status, choice);
        var estimate = ResourceProcessingRules.EstimateSeconds(photos, status.SecondsPerPhoto) is double seconds
            ? $" It should take about {SemanticIndexTexts.Duration(seconds)}."
            : "";
        var locked = choice == EnumPersonDescriptions.Lock && status.PersonPhotos > 0
            ? $" {status.PersonPhotos} descriptions written by people are locked to keep them."
            : "";
        var paid = describer.IsPaid ? $" {describer.Model} is a paid model: these photos are sent to its provider." : "";
        return $"The AI describes {Photos(photos)} with the model {describer.Model}.{estimate}{locked}{paid} " +
            "You can go on working meanwhile; canceling keeps what is done.";
    }

    public static string Progress(ResourceProcessingProgressDto progress) => progress.Phase switch
    {
        EnumResourceProcessingPhase.Queued => "Queued" + (progress.QueuePosition > 0 ? $" (position {progress.QueuePosition})" : "")
            + ": one run of KNote Atlas at a time.",
        EnumResourceProcessingPhase.Preparing => "Preparing…",
        EnumResourceProcessingPhase.Processing => $"Describing photos: {progress.PhotosDone} of {progress.PhotosTotal}"
            + (progress.RemainingSeconds is double left && left > 0 ? $" · about {SemanticIndexTexts.Duration(left)} left" : ""),
        EnumResourceProcessingPhase.Completed => $"Done: {Photos(progress.PhotosDescribed)} described in {SemanticIndexTexts.Duration(progress.ElapsedSeconds)}"
            + Extra(progress) + ".",
        EnumResourceProcessingPhase.Canceled => $"Canceled after {Photos(progress.PhotosDescribed)} described{Extra(progress)}: what is done is kept.",
        _ => $"Failed: {progress.Message}"
    };

    public static string Errors(ResourceProcessingProgressDto progress) =>
        progress.ErrorsCount == 0 ? null : $"{Photos(progress.ErrorsCount)} failed (they are tried again in the next run):";

    private static string Extra(ResourceProcessingProgressDto progress) =>
        (progress.PhotosLocked > 0 ? $" · {progress.PhotosLocked} locked" : "")
        + (progress.NotAvailable > 0 ? $" · {progress.NotAvailable} not available here" : "");

    private static string Photos(int count) => count == 1 ? "1 photo" : $"{count} photos";
}
