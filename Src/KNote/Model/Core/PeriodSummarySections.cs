using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>A part of «What happened» of a period summary: its title, its notes in the order they are read, and its words.</summary>
public sealed record PeriodSection(string Title, IReadOnlyList<PeriodNoteDto> Notes, int Words);

/// <summary>
/// How a period summary is written by parts (KNote Atlas, stage 2, 2.6, D13, R2, R3), always, also the short ones: the executive
/// summary takes 10 % of the words (100 to 400), what is open 15 % (100 to 800) and «What happened» the rest, in a part per group of
/// folders (each note with its folder up to the second level of its path: «Home\Projects»), at most MaxSections (the smallest groups,
/// together in «Other folders»), with the words shared by their notes; a part longer than MaxSectionWords is cut into parts by date.
/// Each part is a call of its own: a model writes well up to a few thousand words in one answer.
/// </summary>
public static class PeriodSummarySections
{
    public const int MaxSections = 8;
    public const int MaxSectionWords = 2500;
    public const int MinSectionWords = 100;
    public const string OtherFolders = "Other folders";

    /// <summary>The words of the executive summary, of what is open and of «What happened». A very short summary keeps their shares.</summary>
    public static (int Executive, int Pending, int Narrative) Words(int words)
    {
        words = PeriodSummaryRules.ClampWords(words);
        var executive = Math.Clamp((int)Math.Round(words * 0.10), Math.Min(100, words / 4), 400);
        var pending = Math.Clamp((int)Math.Round(words * 0.15), Math.Min(100, words / 4), 800);
        var narrative = Math.Max(words - executive - pending, Math.Min(MinSectionWords, words / 2));
        return (executive, pending, narrative);
    }

    /// <summary>The group of a folder: its path up to the second level («Home\Projects\KNote» → «Home\Projects»).</summary>
    public static string GroupOf(string folderPath)
    {
        var levels = (folderPath ?? "").Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
        return levels.Length == 0 ? "Notes" : string.Join("\\", levels.Take(2));
    }

    /// <summary>The parts of «What happened», in the order the notes are read (by folder and date).</summary>
    public static List<PeriodSection> Plan(IEnumerable<PeriodNoteDto> notes, int words)
    {
        var ordered = PeriodSummaryRules.Ordered(notes).ToList();
        if (ordered.Count == 0)
            return new List<PeriodSection>();

        var groups = ordered.GroupBy(n => GroupOf(n.FolderPath), StringComparer.CurrentCultureIgnoreCase)
            .Select((g, index) => (Title: g.Key, Notes: g.ToList(), Index: index))
            .ToList();
        if (groups.Count > MaxSections)
        {
            var kept = groups.OrderByDescending(g => g.Notes.Count).ThenBy(g => g.Index).Take(MaxSections - 1).Select(g => g.Index).ToHashSet();
            var others = groups.Where(g => !kept.Contains(g.Index)).SelectMany(g => g.Notes).ToList();
            groups = groups.Where(g => kept.Contains(g.Index)).Append((Title: OtherFolders, Notes: others, Index: int.MaxValue)).ToList();
        }

        var narrative = Words(words).Narrative;
        var minimum = Math.Min(MinSectionWords, narrative);
        var sections = new List<PeriodSection>();
        foreach (var group in groups)
        {
            var groupWords = Math.Max(minimum, (int)Math.Round(narrative * (double)group.Notes.Count / ordered.Count));
            var parts = Math.Min(group.Notes.Count, (int)Math.Ceiling(groupWords / (double)MaxSectionWords));
            if (parts <= 1)
            {
                sections.Add(new PeriodSection(group.Title, group.Notes, Math.Min(groupWords, MaxSectionWords)));
                continue;
            }

            var byDate = group.Notes.OrderBy(n => n.ModificationDateTime).ThenBy(n => n.NoteNumber).ToList();
            var partWords = Math.Min(MaxSectionWords, groupWords / parts);
            for (var part = 0; part < parts; part++)
            {
                var start = byDate.Count * part / parts;
                var end = byDate.Count * (part + 1) / parts;
                sections.Add(new PeriodSection($"{group.Title} (part {part + 1} of {parts})", byDate.GetRange(start, end - start), partWords));
            }
        }
        return sections;
    }
}
