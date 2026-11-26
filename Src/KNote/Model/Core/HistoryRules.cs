using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// The rules of the history of the notes (KLogs): when a change goes into the row of its session (H5), how a note is titled,
/// which versions the retention empties (H7), and when the versions of a deleted note go (H8). Pure: the Service applies them.
/// </summary>
public static class HistoryRules
{
    /// <summary>The same user, entity and origin within this time since the last change update the same row (H5).</summary>
    public static readonly TimeSpan GroupingWindow = TimeSpan.FromMinutes(10);

    /// <summary>
    /// The moment of a change, to the millisecond: what both engines keep exactly (the repositories write it as datetime2 in SQL
    /// Server), so the row of a session is found again by its LastDateTime, and two changes of the same second keep their order.
    /// </summary>
    public static DateTime Timestamp(DateTime moment) => new(moment.Ticks - moment.Ticks % TimeSpan.TicksPerMillisecond, moment.Kind);

    /// <summary>Whether a change of userName from origin at now goes into last, the latest row of its entity.</summary>
    public static bool CanGroup(KLogDto last, string userName, EnumHistoryOrigin origin, DateTime now)
    {
        if (last == null)
            return false;
        if (last.ActionType != nameof(EnumHistoryAction.Created) && last.ActionType != nameof(EnumHistoryAction.Updated))
            return false;
        if (!string.Equals(last.UserName ?? "", userName ?? "", StringComparison.OrdinalIgnoreCase))
            return false;
        if (last.Origin != origin.ToString())
            return false;
        var elapsed = now - last.LastDateTime;
        return elapsed >= TimeSpan.Zero && elapsed < GroupingWindow;
    }

    public static string NoteTitle(int noteNumber, string topic) =>
        Truncate($"#{noteNumber} {topic}".Trim(), KLogDto.MaxEntityTitleLength);

    /// <summary>
    /// The EntityId of a setting of the history (SystemValue): always the same for the same scope and key (without case), so that
    /// its changes are one history, also those of a setting kept in several rows (the AI models). FNV-1a, no cryptography (the web
    /// app reads Model too, where MD5 doesn't exist).
    /// </summary>
    public static Guid SettingId(string scope, string key)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes($"{scope}/{key}".ToUpperInvariant());
        ulong first = 14695981039346656037UL, second = 1099511628211UL ^ 0x9E3779B97F4A7C15UL;
        foreach (var b in bytes)
        {
            first = (first ^ b) * 1099511628211UL;
            second = (second ^ b) * 1099511628211UL + 0x632BE59BD9B4E019UL;
        }
        var guid = new byte[16];
        BitConverter.GetBytes(first).CopyTo(guid, 0);
        BitConverter.GetBytes(second).CopyTo(guid, 8);
        return new Guid(guid);
    }

    public static string Title(string name) => Truncate(name?.Trim(), KLogDto.MaxEntityTitleLength);

    /// <summary>The number of a note from the title of its row (#340 Topic): for one that no longer exists. 0 if it has none.</summary>
    public static int NoteNumberOf(string title)
    {
        if (string.IsNullOrEmpty(title) || title[0] != '#')
            return 0;
        int end = 1;
        while (end < title.Length && char.IsDigit(title[end]))
            end++;
        return int.TryParse(title[1..end], out var number) ? number : 0;
    }

    /// <summary>The topic of a note from the title of its row (#340 Topic).</summary>
    public static string NoteTopicOf(string title)
    {
        if (string.IsNullOrEmpty(title) || NoteNumberOf(title) == 0)
            return title ?? "";
        int end = 1;
        while (end < title.Length && char.IsDigit(title[end]))
            end++;
        return title[end..].Trim();
    }

    /// <summary>When the retention empties versions (H9): said under the warning of a shorter retention.</summary>
    public const string RetentionApplies = "The versions are emptied in each note the next time it is saved, and those of the deleted notes "
        + "when another note is deleted or the repository is opened. An emptied version can't be brought back.";

    /// <summary>
    /// What a new retention shortens (H25), said before it is saved: one line for each value lower than before. Nothing if it keeps
    /// as much as before or more.
    /// </summary>
    public static List<string> RetentionReductions(HistoryRetentionDto before, HistoryRetentionDto after)
    {
        before = (before ?? new HistoryRetentionDto()).Normalized();
        after = (after ?? new HistoryRetentionDto()).Normalized();
        var lines = new List<string>();
        if (after.KeepAllMonths < before.KeepAllMonths)
            lines.Add($"All the versions: {before.KeepAllMonths} → {after.KeepAllMonths} months. Older ones keep one a day.");
        if (after.DailyUntilMonths < before.DailyUntilMonths)
            lines.Add($"One version a day: {before.DailyUntilMonths} → {after.DailyUntilMonths} months. Older ones keep one a month.");
        if (after.MaxCopiesPerNote < before.MaxCopiesPerNote)
            lines.Add($"Versions per note: {before.MaxCopiesPerNote} → {after.MaxCopiesPerNote}. The oldest ones above it are emptied.");
        if (after.DeletedNotesDays < before.DeletedNotesDays)
            lines.Add($"Days in the deleted notes: {before.DeletedNotesDays} → {after.DeletedNotesDays}. Notes deleted before can no longer be recovered.");
        if (after.MaxContentSizeMB < before.MaxContentSizeMB)
            lines.Add($"Largest note copied: {before.MaxContentSizeMB} → {after.MaxContentSizeMB} MB. Bigger notes will be noted without their copy.");
        return lines;
    }

    /// <summary>Where the note as it was after a change of its history comes from (VersionAfter).</summary>
    public sealed record VersionSource(KLogDto CopyRow, bool CurrentNote, bool Exact, DateTime? IncludesUntil);

    /// <summary>
    /// The note as it was after a change (H18, «See this version»): the copy of the first later row that has one (it is how the
    /// note was before that row) or, without any, the note as it is now. A deleted note: the copy of its row Deleted (how it was
    /// when deleted). Exact unless some row in between changed its own data and has no copy (the retention emptied it, or it was
    /// too big): then it includes later changes, up to the last of those. The rows of its parts never have a copy and don't count.
    /// </summary>
    public static VersionSource VersionAfter(IEnumerable<KLogDto> rows, Guid klogId)
    {
        var ordered = (rows ?? Enumerable.Empty<KLogDto>()).OrderBy(r => r.RegistryDateTime).ThenBy(r => r.LastDateTime).ToList();
        var index = ordered.FindIndex(r => r.KLogId == klogId);
        if (index < 0)
            return null;

        var row = ordered[index];
        if (row.ActionType == nameof(EnumHistoryAction.Deleted))
            return new VersionSource(row.HasPreviousContent ? row : null, !row.HasPreviousContent, row.HasPreviousContent, null);

        DateTime? includesUntil = null;
        foreach (var later in ordered.Skip(index + 1))
        {
            if (later.HasPreviousContent)
                return new VersionSource(later, false, includesUntil == null, includesUntil);
            if ((later.Fields & EnumHistoryFields.NoteData) != 0)
                includesUntil = later.LastDateTime;
        }
        return new VersionSource(null, true, includesUntil == null, includesUntil);
    }

    /// <summary>
    /// Whether «Compare» can show what a change did (H22): the first one (created) or one that changed the note's own data. Not one
    /// of its parts only, nor its deletion (there is no note after it).
    /// </summary>
    public static bool CanCompare(KLogDto row) =>
        row != null && row.ActionType != nameof(EnumHistoryAction.Deleted)
        && (row.ActionType == nameof(EnumHistoryAction.Created) || (row.Fields & EnumHistoryFields.NoteData) != 0);

    /// <summary>Where the note before a change comes from (H22): its own copy, or an empty note for the first one.</summary>
    public sealed record BeforeSource(KLogDto CopyRow, bool EmptyNote);

    /// <summary>The note before a change: the copy its row keeps, an empty note if it created it, or null if the copy is gone.</summary>
    public static BeforeSource VersionBefore(KLogDto row)
    {
        if (row == null)
            return null;
        if (row.ActionType == nameof(EnumHistoryAction.Created))
            return new BeforeSource(null, true);
        return row.HasPreviousContent ? new BeforeSource(row, false) : null;
    }

    /// <summary>
    /// The rows whose version can be restored (H23): those of a note that exists (its last row isn't Deleted) with a later row that
    /// keeps a copy (the version after them, H18); not the last change (that is the note now) nor a deletion (a deleted note is
    /// recovered from «Deleted notes», H24).
    /// </summary>
    public static HashSet<Guid> Restorable(IEnumerable<KLogDto> rowsOfANote)
    {
        var ordered = (rowsOfANote ?? Enumerable.Empty<KLogDto>()).OrderBy(r => r.RegistryDateTime).ThenBy(r => r.LastDateTime).ToList();
        var restorable = new HashSet<Guid>();
        if (ordered.Count == 0 || ordered[^1].ActionType == nameof(EnumHistoryAction.Deleted))
            return restorable;

        bool laterCopy = false;
        for (int i = ordered.Count - 1; i >= 0; i--)
        {
            var row = ordered[i];
            if (laterCopy && row.ActionType != nameof(EnumHistoryAction.Deleted))
                restorable.Add(row.KLogId);
            laterCopy |= row.HasPreviousContent;
        }
        return restorable;
    }

    /// <summary>The moment of a version in the lines of the history (English, as RegistryMessage).</summary>
    public static string MomentText(DateTime moment) => moment.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// When a row happened, as the «History» tab writes it in the culture of the app: one moment, or a session from its first
    /// change to its last (25/11/2026 10:05–10:31; across days, both dates).
    /// </summary>
    public static string WhenText(KLogDto row, IFormatProvider culture = null)
    {
        if (row == null)
            return "";
        culture ??= System.Globalization.CultureInfo.CurrentCulture;
        string Full(DateTime d) => d.ToString("g", culture);
        if (row.LastDateTime.Date != row.RegistryDateTime.Date)
            return $"{Full(row.RegistryDateTime)} – {Full(row.LastDateTime)}";
        if (row.LastDateTime - row.RegistryDateTime < TimeSpan.FromMinutes(1))
            return Full(row.LastDateTime);
        return $"{Full(row.RegistryDateTime)}–{row.LastDateTime.ToString("t", culture)}";
    }

    /// <summary>What the window of a version says when it is not exact (H18).</summary>
    public static string VersionNotice(DateTime? includesUntil) => includesUntil is not DateTime until ? null
        : $"Includes later changes, up to {MomentText(until)}: the copies in between were not kept.";

    /// <summary>The size of a version in bytes (UTF-8), as kept in PreviousContentSize.</summary>
    public static int ContentSize(string json) => json == null ? 0 : Encoding.UTF8.GetByteCount(json);

    /// <summary>Whether a version of this size is kept (a bigger note is noted without its copy).</summary>
    public static bool FitsCopy(int size, HistoryRetentionDto retention) => size <= (retention ?? new HistoryRetentionDto()).MaxContentSizeBytes;

    /// <summary>
    /// The rows of an entity whose version the retention empties at now: all of them are kept for KeepAllMonths; then the first
    /// one of each day (how the note was when that day began, that is, when the day before with changes ended) until
    /// DailyUntilMonths; then the first one of each month. And then, the oldest ones above MaxCopiesPerNote.
    /// </summary>
    public static List<Guid> CopiesToEmpty(IEnumerable<KLogDto> rowsOfAnEntity, HistoryRetentionDto retention, DateTime now)
    {
        retention = (retention ?? new HistoryRetentionDto()).Normalized();
        var keepAllFrom = now.AddMonths(-retention.KeepAllMonths);
        var dailyFrom = now.AddMonths(-retention.DailyUntilMonths);

        var copies = (rowsOfAnEntity ?? Enumerable.Empty<KLogDto>())
            .Where(r => r.HasPreviousContent)
            .OrderBy(r => r.RegistryDateTime).ThenBy(r => r.LastDateTime)
            .ToList();

        var empty = new List<Guid>();
        var kept = new List<KLogDto>();
        string lastPeriod = null;

        foreach (var copy in copies)
        {
            if (copy.RegistryDateTime >= keepAllFrom)
            {
                kept.Add(copy);
                continue;
            }

            var period = copy.RegistryDateTime >= dailyFrom
                ? "d" + copy.RegistryDateTime.ToString("yyyyMMdd")
                : "m" + copy.RegistryDateTime.ToString("yyyyMM");
            if (period == lastPeriod)
                empty.Add(copy.KLogId);
            else
            {
                kept.Add(copy);
                lastPeriod = period;
            }
        }

        var over = kept.Count - retention.MaxCopiesPerNote;
        if (over > 0)
            empty.AddRange(kept.Take(over).Select(r => r.KLogId));

        return empty;
    }

    /// <summary>The notes deleted before this moment lose all their versions (H8): the trash keeps them DeletedNotesDays.</summary>
    public static DateTime DeletedNotesLimit(HistoryRetentionDto retention, DateTime now) =>
        now.AddDays(-(retention ?? new HistoryRetentionDto()).Normalized().DeletedNotesDays);

    public static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
        return maxLength <= 1 ? text[..maxLength] : text[..(maxLength - 1)] + "…";
    }
}
