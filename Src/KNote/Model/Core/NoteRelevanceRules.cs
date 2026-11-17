using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// Whether a note of the period goes into a summary (KNote Atlas, stage 2, 2.3, chapter 29, D8), with its reason in words. The
/// first that decides: KNote's own notes are out and not listed (the caller counts them, NoiseRules) → a person's correction (S3:
/// above the policy and the tags, it does not expire) → the noise policy and the tags #NoKnowledge / #Knowledge → the signals of
/// the note: tasks (with activity in the period), citations from the other notes of the selection, relations, resources and its
/// size. Without any of them, a very short note created and left the same day is out, a short one is doubtful (the AI will be asked
/// from 2.4) and the rest go in. The score is the sum of the signals, whatever decides: it orders the lists and will be the weight
/// of the relation of the summary with the note (2.6). Being shown as a post-it does not count (D8).
/// </summary>
public static class NoteRelevanceRules
{
    // S6: below these words (topic and text, cleaned), without any signal, a note is out (if left the same day) or doubtful.
    public const int OutWords = 10;
    public const int DoubtfulWords = 40;

    public const string SelectionHelp = "The notes modified in the period or with task activity in it (a task that started or ended), " +
        "of the folder and its subfolders, with the keywords and about what «Talks about…» says, if given. Each one goes in, is doubtful " +
        "or stays out, with its reason; KNote's own notes are always out. «Review selection» shows them and lets you change them: " +
        "the changes are remembered for the next summaries, without touching the notes.";

    public sealed record Signals(int Words, int Tasks, int TasksWithActivity, int Resources, int Relations, int CitedBy, bool LeftTheSameDay)
    {
        public bool Any => Tasks > 0 || Resources > 0 || Relations > 0 || CitedBy > 0;
    }

    /// <summary>The signals of a note, with its cleaned text (NoteReadingCleaner) and how many notes of the selection cite it.</summary>
    public static Signals SignalsOf(NoteSelectionSourceDto note, string cleanedText, int citedBy) => new(
        NoteReadingCleaner.CountWords(note.Topic) + NoteReadingCleaner.CountWords(cleanedText),
        note.Tasks, note.TasksWithActivity, note.Resources, note.Relations, citedBy,
        note.CreationDateTime.Date == note.ModificationDateTime.Date);

    public static double Score(Signals s)
    {
        var score = s.TasksWithActivity * 3.0 + Math.Min(s.Tasks, 5) + s.CitedBy * 2.0 + Math.Min(s.Relations, 5)
            + Math.Min(s.Resources, 5) * 0.5 + Math.Min(s.Words / 100, 5);
        if (!s.Any && s.Words < OutWords && s.LeftTheSameDay)
            score -= 3;
        return Math.Round(score, 1);
    }

    /// <summary>What the signals decide, with the reason.</summary>
    public static (EnumSelectionDecision Decision, string Reason) BySignals(Signals s)
    {
        if (s.Any)
        {
            var parts = new List<string>();
            if (s.TasksWithActivity > 0)
                parts.Add($"{Count(s.TasksWithActivity, "task")} with activity in the period");
            if (s.Tasks > s.TasksWithActivity)
                parts.Add(s.TasksWithActivity > 0 ? $"{Count(s.Tasks, "task")} in all" : Count(s.Tasks, "task"));
            if (s.CitedBy > 0)
                parts.Add($"cited by {Count(s.CitedBy, "note")} of the selection");
            if (s.Relations > 0)
                parts.Add(Count(s.Relations, "relation"));
            if (s.Resources > 0)
                parts.Add(Count(s.Resources, "resource"));
            parts.Add(Count(s.Words, "word"));
            return (EnumSelectionDecision.In, "In: " + string.Join(", ", parts));
        }
        if (s.Words < OutWords && s.LeftTheSameDay)
            return (EnumSelectionDecision.Out, $"Out: a note of {Count(s.Words, "word")} created and left the same day, without tasks, resources, relations or citations");
        if (s.Words < DoubtfulWords)
            return (EnumSelectionDecision.Doubtful, $"Doubtful: a short note ({Count(s.Words, "word")}) without tasks, resources, relations or citations");
        return (EnumSelectionDecision.In, $"In: {Count(s.Words, "word")}");
    }

    /// <summary>
    /// The decision of a note that is not one of KNote's own (noise is not KNoteNote): its correction, if a person made one; else the
    /// noise policy or its tag; else its signals. ComputedDecision / ComputedReason are what KNote decides without the correction.
    /// </summary>
    public static void Decide(NoteSelectionDto note, EnumNoiseReason noise, Signals signals, RelevanceCorrectionDto correction)
    {
        ArgumentNullException.ThrowIfNull(note);
        if (noise == EnumNoiseReason.KNoteNote)
            throw new ArgumentException("KNote's own notes are not in the selection.", nameof(noise));

        var (computed, reason, source) = noise switch
        {
            EnumNoiseReason.NoKnowledgeTag => (EnumSelectionDecision.Out, $"Out: it has the tag {KntConst.NoKnowledgeTag}", EnumSelectionSource.Tag),
            EnumNoiseReason.KnowledgeTag => (EnumSelectionDecision.In, $"In: it has the tag {KntConst.KnowledgeTag}", EnumSelectionSource.Tag),
            EnumNoiseReason.ExcludedNoteType => (EnumSelectionDecision.Out, "Out: its note type is left out by the noise policy", EnumSelectionSource.Policy),
            EnumNoiseReason.ExcludedFolder => (EnumSelectionDecision.Out, "Out: its folder is left out by the noise policy", EnumSelectionSource.Policy),
            _ => Signal(signals)
        };

        note.Words = signals.Words;
        note.Score = Score(signals);
        note.ComputedDecision = computed;
        note.ComputedReason = reason;
        if (correction != null && correction.Decision != EnumSelectionDecision.Doubtful)
        {
            note.Decision = correction.Decision;
            note.Source = EnumSelectionSource.Correction;
            note.Reason = correction.Decision == EnumSelectionDecision.In
                ? $"In: included by {correction.UserName} on {correction.Date:yyyy-MM-dd}"
                : $"Out: left out by {correction.UserName} on {correction.Date:yyyy-MM-dd}";
        }
        else
        {
            note.Decision = computed;
            note.Source = source;
            note.Reason = reason;
        }
    }

    /// <summary>
    /// The change «Review selection» keeps when a note is checked (include) or unchecked: a correction In or Out, or null when it is
    /// again what KNote decides (its correction is taken away). A doubtful note is included as KNote decides it (S5).
    /// </summary>
    public static EnumSelectionDecision? CorrectionFor(NoteSelectionDto note, bool include)
    {
        ArgumentNullException.ThrowIfNull(note);
        var computedIncluded = note.ComputedDecision != EnumSelectionDecision.Out;
        if (include == computedIncluded)
            return null;
        return include ? EnumSelectionDecision.In : EnumSelectionDecision.Out;
    }

    /// <summary>The order of the lists: in, doubtful, out; the highest score first; then by number.</summary>
    public static List<NoteSelectionDto> Order(IEnumerable<NoteSelectionDto> notes) =>
        (notes ?? Enumerable.Empty<NoteSelectionDto>())
            .OrderBy(n => n.Decision).ThenByDescending(n => n.Score).ThenBy(n => n.NoteNumber).ToList();

    /// <summary>The counts of a selection in words (both apps).</summary>
    public static string CountText(NoteSelectionResultDto selection)
    {
        if (selection == null)
            return "";
        var text = selection.Notes.Count == 0 && selection.KNoteNotes == 0
            ? "No note was modified or had task activity in the period, with these filters."
            : $"{selection.In:N0} in, {selection.Doubtful:N0} doubtful, {selection.Out:N0} out.";
        if (selection.KNoteNotes > 0)
            text += $" {Count(selection.KNoteNotes, "note")} of KNote's own, always out, are not listed.";
        return text;
    }

    /// <summary>What «Review selection» would keep (both apps).</summary>
    public static string ChangesText(int included, int total, int changes) =>
        $"{included:N0} of {total:N0} notes would go into the summary" + (changes == 0 ? "." : $" · {Count(changes, "change")} to keep.");

    private static (EnumSelectionDecision, string, EnumSelectionSource) Signal(Signals signals)
    {
        var (decision, reason) = BySignals(signals);
        return (decision, reason, EnumSelectionSource.Signals);
    }

    private static string Count(int count, string noun) => count == 1 ? $"1 {noun}" : $"{count:N0} {noun}s";
}
