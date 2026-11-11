using System;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// What the two apps say of the semantic index (KNote Atlas): its status, the progress of a run and the question before
/// starting one, so that the Web and ClientWin show the same.
/// </summary>
public static class SemanticIndexTexts
{
    public static string Status(SemanticIndexStatusDto status) => status.Status switch
    {
        EnumSemanticIndexStatus.Ready when status.Rebuilding => "Being rebuilt (the old vectors are used meanwhile)",
        EnumSemanticIndexStatus.Ready => "Ready",
        EnumSemanticIndexStatus.Building => "Being created (not finished: the next run goes on)",
        _ => "Not created"
    };

    // Whether the search by meaning can be offered: an index with notes, and this installation with its model. Without
    // it, only the search by words (CU-04).
    public static bool CanSearch(SemanticIndexStatusDto status) =>
        status != null && status.IndexedNotes > 0 && status.EmbeddingModelAvailable;

    // The notice of a search by meaning that left notes not up to date (D8), with «Update index» beside it.
    public static string PendingInSearch(int pendingNotes) =>
        $"{pendingNotes} {(pendingNotes == 1 ? "note is" : "notes are")} not up to date in the semantic index: "
        + "they may be missing from the results or found by their previous content.";

    /// <summary>
    /// Whether a search by meaning shows its notice (4.6b): notes not up to date that a full indexing can bring up to date. The ones
    /// whose documents have no file here can't be: they don't show it by themselves.
    /// </summary>
    public static bool ShowsPendingNotice(int pendingNotes, int notesWithoutFiles) => pendingNotes - notesWithoutFiles > 0;

    /// <summary>
    /// The notice (4.6b): the notes not up to date, how many wait for the full indexing, who can run it (an Admin, with «Update index»
    /// beside it) and, besides, the notes with documents whose file is not in this installation.
    /// </summary>
    public static string PendingInSearch(int pendingNotes, int notesForFullRun, int notesWithoutFiles, bool canRunIndex)
    {
        var pending = Math.Max(0, pendingNotes - notesWithoutFiles);
        var forFullRun = Math.Min(Math.Max(0, notesForFullRun), pending);
        var text = PendingInSearch(pending);
        if (forFullRun > 0)
            text += $" {(forFullRun == pending ? (pending == 1 ? "It waits" : "They wait") : $"{forFullRun} of them {(forFullRun == 1 ? "waits" : "wait")}")} for the full " +
                "indexing (documents not read yet, or too long).";
        if (!canRunIndex)
            text += " An Admin can bring the index up to date in KNote Atlas › Semantic index.";
        if (notesWithoutFiles > 0)
            text += $" Besides, {notesWithoutFiles} {(notesWithoutFiles == 1 ? "note has" : "notes have")} documents whose file is not in this installation.";
        return text;
    }

    public const string UpdateIndex = "Update index";

    public const string UpdateIndexHelp = "Brings the whole semantic index up to date, as «Update now» of KNote Atlas › Semantic index, in the " +
        "background (you can go on working), and searches again when it ends.";

    public const string UpdateIndexQuestion = "Bring the whole semantic index of the repository up to date now? It runs in the background, as " +
        "«Update now» of KNote Atlas › Semantic index: you can go on working, and the search is repeated when it ends. With a paid model, " +
        "the texts of the notes are sent to its provider.";

    public static string Counts(SemanticIndexStatusDto status) =>
        $"{status.IndexedNotes} of {status.TotalNotes} notes indexed · {status.PendingNotes} pending · {status.Chunks} fragments";

    // Null before the first complete creation.
    public static string LastCreation(SemanticIndexStatusDto status) =>
        status.CompletedDateTime == null
            ? null
            : $"Last complete creation: {status.CompletedDateTime.Value:g}"
              + (status.LastBuildSeconds != null ? $" ({Duration(status.LastBuildSeconds.Value)})" : "");

    public static string Progress(SemanticIndexProgressDto progress) => progress.Phase switch
    {
        EnumSemanticIndexPhase.Queued => "Queued" + (progress.QueuePosition > 0 ? $" (position {progress.QueuePosition})" : "")
            + ": one index is created at a time.",
        EnumSemanticIndexPhase.Preparing => "Preparing…",
        EnumSemanticIndexPhase.Indexing => $"{(progress.Rebuild ? "Rebuilding" : "Indexing")}: {progress.NotesDone} of {progress.NotesTotal} notes"
            + (progress.RemainingSeconds is double left && left > 0 ? $" · about {Duration(left)} left" : ""),
        EnumSemanticIndexPhase.Completed => $"Done: {progress.NotesDone} notes in {Duration(progress.ElapsedSeconds)}"
            + (progress.DocumentsNotAvailable > 0 ? $" · {progress.DocumentsNotAvailable} documents not available here" : "") + ".",
        EnumSemanticIndexPhase.Canceled => $"Canceled after {progress.NotesDone} notes: what is done is kept.",
        _ => $"Failed: {progress.Message}"
    };

    public static string Errors(SemanticIndexProgressDto progress) =>
        progress.ErrorsCount == 0 ? null : $"{progress.ErrorsCount} notes failed (they are tried again in the next run):";

    public static string RunTitle(SemanticIndexStatusDto status, bool rebuild) =>
        rebuild ? "Rebuild the semantic index"
        : status.Status == EnumSemanticIndexStatus.None ? "Create the semantic index"
        : "Update the semantic index";

    /// <summary>What a run is going to do, before it starts: how many notes, with which model, how long and, with a paid model, that their content is sent.</summary>
    public static string RunConfirmation(SemanticIndexStatusDto status, bool rebuild, bool paidModel)
    {
        var notes = rebuild ? status.TotalNotes : status.PendingNotes;
        var estimate = status.NotesPerMinute is double pace && pace > 0 ? $" It should take about {Duration(notes / pace * 60)}." : "";
        var paid = paidModel ? $" {status.EmbeddingModel} is a paid model: the content of these notes is sent to its provider." : "";
        var what = rebuild ? "calculates again the meaning of" : "calculates the meaning of";
        return $"KNote {what} {notes} notes with the model {status.IndexKey}.{estimate}{paid} You can go on working meanwhile; canceling keeps what is done.";
    }

    public static string Duration(double seconds)
    {
        var time = TimeSpan.FromSeconds(Math.Max(0, seconds));
        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours} h {time.Minutes:00} min";
        if (time.TotalMinutes >= 1)
            return $"{time.Minutes} min {time.Seconds:00} s";
        return $"{Math.Max(1, (int)Math.Round(time.TotalSeconds))} s";
    }
}
