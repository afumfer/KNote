using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// The document of a period summary (KNote Atlas, stage 2, 2.6, chapter 35, D13, D15-D17): the data of the summary, the executive
/// summary (AI), the index by folders and by concepts, «What happened» by parts (AI), what is open (AI), the statistics and the notes
/// included and left out. The AI writes only its three parts; the numbers and the lists are calculated by KNote, so they are exact. The
/// citations of notes not read are left without a link (SummaryCitationRules). The headings in English, as KNote's interface (R1).
/// </summary>
public static class PeriodSummaryDocument
{
    public const string ExecutiveHeading = "## Executive summary";
    public const string IndexHeading = "## Index";
    public const string HappenedHeading = "## What happened";
    public const string PendingHeading = "## Open and at risk";
    public const string StatisticsHeading = "## Statistics";
    public const string IncludedHeading = "## Notes included";
    public const string LeftOutHeading = "## Notes left out";

    public const int MaxCitationsPerConcept = 20;
    public const string NothingWritten = "_The AI wrote nothing of this part._";

    /// <summary>A note the summary comes from (D17): its order (of its first citation; the ones not cited after them, in the order they
    /// are read) and its weight (its score of relevance).</summary>
    public sealed record Source(Guid NoteId, int NoteNumber, int Order, double Weight);

    /// <summary>The statistics of a folder (or of all of them: Folder null).</summary>
    public sealed record FolderStatistics(string Folder, int Notes, int TasksStarted, int TasksEnded, int TasksOpen, double? EstimatedTime, double? SpentTime);

    /// <summary>The text of the note of the summary.</summary>
    public static string Build(PeriodSummarySaveDto summary, string folderPath, DateTime date)
    {
        var prepared = summary.Prepared ?? new PeriodSummaryPreparedDto();
        var notes = PeriodSummaryRules.Ordered(prepared.Notes).ToList();
        var read = notes.Select(n => n.NoteNumber).ToHashSet();
        var notRecognized = new List<int>();
        string Part(string text)
        {
            var clean = PeriodSummaryRules.CleanPart(text);
            if (clean.Length == 0)
                return NothingWritten;
            var check = SummaryCitationRules.Check(clean, read);
            notRecognized.AddRange(check.NotRecognized.Where(n => !notRecognized.Contains(n)));
            return check.Text;
        }

        var executive = Part(summary.Executive);
        var sections = (summary.Sections ?? new List<PeriodSummarySectionDto>()).Select(s => (Section: s, Text: Part(s.Text))).ToList();
        var pending = Part(summary.Pending);

        var text = new StringBuilder();
        text.Append(Header(summary, folderPath, date, notRecognized)).Append("\n\n");
        text.Append(ExecutiveHeading).Append("\n\n").Append(executive).Append("\n\n");
        text.Append(IndexHeading).Append("\n\n").Append(Index(sections.Select(s => s.Section).ToList(),
            PeriodConceptRules.Tables(notes, prepared.ConceptAttributes))).Append("\n\n");
        text.Append(HappenedHeading).Append("\n\n");
        foreach (var (section, sectionText) in sections)
            text.Append($"### {section.Title}\n\n{sectionText}\n\n");
        text.Append(PendingHeading).Append("\n\n").Append(pending).Append("\n\n");
        text.Append(StatisticsHeading).Append("\n\n").Append(StatisticsTable(Statistics(notes))).Append("\n\n");
        text.Append(IncludedHeading).Append("\n\n").Append(Included(notes, prepared.Selection)).Append("\n\n");
        text.Append(LeftOutHeading).Append("\n\n").Append(LeftOut(prepared.Selection));
        return text.ToString().TrimEnd();
    }

    /// <summary>What the AI wrote, as it wrote it, with KNote's headings (D17: the original attached, to compare when it is corrected).</summary>
    public static string Original(PeriodSummarySaveDto summary)
    {
        var text = new StringBuilder();
        text.Append(ExecutiveHeading).Append("\n\n").Append(summary.Executive?.Trim()).Append("\n\n");
        text.Append(HappenedHeading).Append("\n\n");
        foreach (var section in summary.Sections ?? new List<PeriodSummarySectionDto>())
            text.Append($"### {section.Title}\n\n{section.Text?.Trim()}\n\n");
        text.Append(PendingHeading).Append("\n\n").Append(summary.Pending?.Trim());
        return text.ToString().TrimEnd() + "\n";
    }

    /// <summary>The notes the summary comes from, in their order (D17).</summary>
    public static List<Source> Sources(PeriodSummarySaveDto summary)
    {
        var notes = PeriodSummaryRules.Ordered(summary.Prepared?.Notes).ToList();
        var byNumber = notes.GroupBy(n => n.NoteNumber).ToDictionary(g => g.Key, g => g.First());
        var texts = new[] { summary.Executive }.Concat((summary.Sections ?? new List<PeriodSummarySectionDto>()).Select(s => s.Text)).Append(summary.Pending);
        var cited = SummaryCitationRules.CitedInOrder(texts).Where(byNumber.ContainsKey).Select(n => byNumber[n]).ToList();
        return cited.Concat(notes.Where(n => !cited.Contains(n)))
            .Select((n, i) => new Source(n.NoteId, n.NoteNumber, i + 1, n.Score))
            .ToList();
    }

    /// <summary>The statistics by folder, in the order they are read, and the total last (Folder null).</summary>
    public static List<FolderStatistics> Statistics(IReadOnlyList<PeriodNoteDto> notes)
    {
        static FolderStatistics Of(string folder, IReadOnlyCollection<PeriodNoteDto> notes) => new(folder, notes.Count,
            notes.Sum(n => n.TasksStarted), notes.Sum(n => n.TasksEnded), notes.Sum(n => n.TasksOpen),
            notes.Any(n => n.EstimatedTime != null) ? notes.Sum(n => n.EstimatedTime ?? 0) : null,
            notes.Any(n => n.SpentTime != null) ? notes.Sum(n => n.SpentTime ?? 0) : null);

        var folders = notes.GroupBy(n => n.FolderPath ?? "", StringComparer.CurrentCultureIgnoreCase).Select(g => Of(g.Key, g.ToList())).ToList();
        folders.Add(Of(null, notes.ToList()));
        return folders;
    }

    private static string Header(PeriodSummarySaveDto summary, string folderPath, DateTime date, IReadOnlyCollection<int> notRecognized)
    {
        var selection = summary.Request?.Selection ?? new NoteSelectionRequestDto();
        var prepared = summary.Prepared ?? new PeriodSummaryPreparedDto();
        var header = new StringBuilder();
        header.Append($"> Summary of the period **{PeriodSummaryRules.PeriodText(selection.From, selection.To)}**");
        if (!string.IsNullOrWhiteSpace(folderPath))
            header.Append($" · folder **{folderPath}** and its subfolders");
        if (!string.IsNullOrWhiteSpace(selection.Keywords))
            header.Append($" · keywords «{selection.Keywords.Trim()}»");
        if (!string.IsNullOrWhiteSpace(selection.MeaningText))
            header.Append($" · talks about «{selection.MeaningText.Trim()}»");
        if (prepared.ConceptAttributes?.Count > 0)
            header.Append($" · concepts by {string.Join(", ", prepared.ConceptAttributes.Select(a => $"**{PeriodSummaryRules.Line(a)}**"))}");
        var leftOut = prepared.Selection?.Out ?? 0;
        var kNote = prepared.Selection?.KNoteNotes ?? 0;
        header.Append($"\n>\n> {PeriodSummaryRules.Notes(prepared.Notes?.Count ?? 0)} read");
        if (leftOut > 0)
            header.Append($", {PeriodSummaryRules.Notes(leftOut)} left out");
        if (kNote > 0)
            header.Append($" (and {kNote:N0} of KNote's own)");
        header.Append($" · {summary.Model} · {date:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(summary.Language))
            header.Append($" · {summary.Language.Trim()}");
        header.Append($"\n>\n> {(summary.Calls == 1 ? "1 call" : $"{summary.Calls:N0} calls")} to the model, {Duration(summary.Seconds)}");
        if (SummaryCitationRules.NotRecognizedText(notRecognized) is string citations)
            header.Append($" · {citations}");
        return header.ToString();
    }

    private static string Index(IReadOnlyList<PeriodSummarySectionDto> sections, IReadOnlyList<PeriodConceptRules.ConceptTable> concepts)
    {
        var text = new StringBuilder("**By folder**\n\n");
        foreach (var section in sections)
            text.Append($"- {section.Title}: {PeriodSummaryRules.Notes(section.NoteNumbers?.Count ?? 0)}\n");
        text.Append("\n**By concept**\n\n");
        text.Append(concepts.Count == 0 ? PeriodConceptRules.NoneChosen : string.Join("\n\n", concepts.Select(ConceptTable)));
        return text.ToString().TrimEnd();
    }

    // The table of an attribute chosen: a row per value, with its notes and their tasks with activity in the period.
    private static string ConceptTable(PeriodConceptRules.ConceptTable table)
    {
        var name = PeriodSummaryRules.Line(table.Attribute);
        if (table.Rows.Count == 0)
            return $"No note has a value of {name}.";

        static string Cited(List<int> notes) => string.Join(", ", notes.Take(MaxCitationsPerConcept).Select(PeriodSummaryRules.Label)) +
            (notes.Count > MaxCitationsPerConcept ? "…" : "");
        static string Row(string value, PeriodConceptRules.Concept row) =>
            $"| {value} | {row.NoteNumbers.Count:N0} | {row.Tasks:N0} | {row.TasksStarted:N0} / {row.TasksEnded:N0} / {row.TasksOpen:N0} | {Cited(row.NoteNumbers)} |\n";

        var text = new StringBuilder($"| {Cell(table.Attribute)} | Notes | Tasks | Started / ended / open | Notes cited |\n|---|---:|---:|---|---|\n");
        foreach (var row in table.Rows)
            text.Append(Row(Cell(row.Value), row));
        if (table.Others is { } others)
            text.Append(Row(table.OtherValues == 1 ? "_1 other value_" : $"_{table.OtherValues:N0} other values_", others));
        if (table.NotesWithoutValue > 0)
            text.Append($"\n{PeriodSummaryRules.Notes(table.NotesWithoutValue)} without a value of {name}.");
        return text.ToString().TrimEnd();
    }

    private static string StatisticsTable(IReadOnlyList<FolderStatistics> statistics)
    {
        var text = new StringBuilder("| Folder | Notes | Tasks started | Tasks ended | Tasks open at the end | Estimated time | Spent time |\n");
        text.Append("|---|---:|---:|---:|---:|---:|---:|\n");
        foreach (var row in statistics)
        {
            var folder = row.Folder == null ? "**Total**" : Cell(row.Folder);
            text.Append($"| {folder} | {row.Notes:N0} | {row.TasksStarted:N0} | {row.TasksEnded:N0} | {row.TasksOpen:N0} | {Time(row.EstimatedTime)} | {Time(row.SpentTime)} |\n");
        }
        return text.ToString().TrimEnd();
    }

    private static string Included(IReadOnlyList<PeriodNoteDto> notes, NoteSelectionResultDto selection)
    {
        var reasons = (selection?.Notes ?? new List<NoteSelectionDto>()).GroupBy(n => n.NoteId).ToDictionary(g => g.Key, g => g.First().Reason);
        var text = new StringBuilder("| Note | Topic | Folder | Modified | Why |\n|---|---|---|---|---|\n");
        foreach (var note in notes)
            text.Append($"| {PeriodSummaryRules.Label(note.NoteNumber)} | {Cell(note.Topic)} | {Cell(note.FolderPath)} | " +
                $"{PeriodSummaryRules.Day(note.ModificationDateTime)} | {Cell(reasons.GetValueOrDefault(note.NoteId))} |\n");
        return text.ToString().TrimEnd();
    }

    private static string LeftOut(NoteSelectionResultDto selection)
    {
        var notes = (selection?.Notes ?? new List<NoteSelectionDto>()).Where(n => n.Decision == EnumSelectionDecision.Out).OrderBy(n => n.NoteNumber).ToList();
        var text = new StringBuilder();
        if (notes.Count == 0)
            text.Append("None.");
        else
        {
            text.Append("| Note | Topic | Why |\n|---|---|---|\n");
            foreach (var note in notes)
                text.Append($"| {PeriodSummaryRules.Label(note.NoteNumber)} | {Cell(note.Topic)} | {Cell(note.Reason)} |\n");
        }
        if (selection?.KNoteNotes > 0)
            text.Append($"\n\nAnd {PeriodSummaryRules.Notes(selection.KNoteNotes)} of KNote's own, which are never summarized.");
        return text.ToString().TrimEnd();
    }

    private static string Cell(string text) => PeriodSummaryRules.Line(text).Replace("|", "\\|");

    private static string Time(double? time) => time is double value ? value.ToString("0.##", CultureInfo.InvariantCulture) : "";

    private static string Duration(double seconds) =>
        seconds < 90 ? $"{Math.Round(seconds):0} s" : $"{Math.Round(seconds / 60):0} min";
}
