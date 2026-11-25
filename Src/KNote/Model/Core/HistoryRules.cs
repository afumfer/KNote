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
