using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>What can be brought back from a row of the history, as the lists show it (H28 and the «History» tab).</summary>
public enum EnumHistoryRowState
{
    /// <summary>Nothing to say: a note that exists («Repository changes») or a version that can be restored («History»).</summary>
    None = 0,
    /// <summary>A note in the deleted notes: «Recover deleted notes ...» until a day.</summary>
    Recoverable = 1,
    /// <summary>The same, for a user who can't recover it (a Guest).</summary>
    RecoverableByStaff = 2,
    /// <summary>A deleted note whose copy is no longer kept (the trash emptied it, or «Remove permanently»).</summary>
    NotRecoverable = 3,
    /// <summary>A change of the structure: noted, never restored (it has no versions, H6).</summary>
    OnlyNoted = 4,
    /// <summary>«History»: the version after this change is the note as it is now; nothing to restore.</summary>
    CurrentVersion = 5,
    /// <summary>«History»: it can be restored, but the version includes later changes (copies in between were emptied, H18).</summary>
    LaterChanges = 6,
    /// <summary>«History»: it can't be restored (a deletion, or no copy after it).</summary>
    NotRestorable = 7
}

/// <summary>How a state is drawn: the colour of its row (soft, with its version for the dark theme in each app).</summary>
public enum EnumHistoryRowTone
{
    None,
    /// <summary>Something can still be done, with a limit (soft amber).</summary>
    Amber,
    /// <summary>It can't be brought back (soft orange).</summary>
    Orange,
    /// <summary>Normal for it, only information (soft grey).</summary>
    Grey
}

/// <summary>
/// The state of each row of the lists of the history, so that a row that can't be brought back says why instead of a button that
/// doesn't work (2026-11-27): «Repository changes» (by the note as it is now: it exists, it is in the deleted notes until a day,
/// or its copy is gone; the structure is only noted) and the «History» tab of a note (the version after the row can be restored,
/// includes later changes, is the note now, or can't be restored). Its short text (the column «State»), its legend and its tone
/// are the same in both apps. Pure: the Service gives the states, the apps draw them.
/// </summary>
public static class HistoryRowStates
{
    /// <summary>The state of a row of «Repository changes»: deletedUntil, the day a note in the deleted notes stops being recoverable.</summary>
    public static EnumHistoryRowState ForChange(KLogDto row, bool noteExists, DateTime? deletedUntil, bool canRecover)
    {
        if (row == null)
            return EnumHistoryRowState.None;
        if (row.EntityName != HistoryEntities.Note)
            return EnumHistoryRowState.OnlyNoted;
        if (noteExists)
            return EnumHistoryRowState.None;
        if (deletedUntil != null)
            return canRecover ? EnumHistoryRowState.Recoverable : EnumHistoryRowState.RecoverableByStaff;
        return EnumHistoryRowState.NotRecoverable;
    }

    /// <summary>The state of a row of the «History» tab of a note that exists (rows: all the rows of the note).</summary>
    public static EnumHistoryRowState ForNoteHistory(IReadOnlyCollection<KLogDto> rows, KLogDto row, ISet<Guid> restorable)
    {
        if (row == null)
            return EnumHistoryRowState.None;
        if (row.ActionType == nameof(EnumHistoryAction.Deleted) || row.ActionType == nameof(EnumHistoryAction.Removed))
            return EnumHistoryRowState.NotRestorable;
        if (restorable != null && restorable.Contains(row.KLogId))
        {
            var source = HistoryRules.VersionAfter(rows, row.KLogId);
            return source?.Exact == false ? EnumHistoryRowState.LaterChanges : EnumHistoryRowState.None;
        }
        var after = HistoryRules.VersionAfter(rows, row.KLogId);
        return after?.CurrentNote == true ? EnumHistoryRowState.CurrentVersion : EnumHistoryRowState.NotRestorable;
    }

    public static EnumHistoryRowTone Tone(EnumHistoryRowState state) => state switch
    {
        EnumHistoryRowState.Recoverable or EnumHistoryRowState.RecoverableByStaff or EnumHistoryRowState.LaterChanges => EnumHistoryRowTone.Amber,
        EnumHistoryRowState.NotRecoverable or EnumHistoryRowState.NotRestorable => EnumHistoryRowTone.Orange,
        EnumHistoryRowState.OnlyNoted or EnumHistoryRowState.CurrentVersion => EnumHistoryRowTone.Grey,
        _ => EnumHistoryRowTone.None
    };

    /// <summary>The column «State»: short.</summary>
    public static string Text(EnumHistoryRowState state, DateTime? until = null, IFormatProvider culture = null) => state switch
    {
        EnumHistoryRowState.Recoverable => until is DateTime day
            ? $"Recoverable until {day.ToString("d", culture ?? System.Globalization.CultureInfo.CurrentCulture)}" : "Recoverable",
        EnumHistoryRowState.RecoverableByStaff => "Recoverable by Staff and up",
        EnumHistoryRowState.NotRecoverable => "Can't be recovered",
        EnumHistoryRowState.OnlyNoted => "Only noted",
        EnumHistoryRowState.CurrentVersion => "The note now",
        EnumHistoryRowState.LaterChanges => "Includes later changes",
        EnumHistoryRowState.NotRestorable => "Can't be restored",
        _ => ""
    };

    /// <summary>The legend under the list (and the tooltip of the state): why.</summary>
    public static string Legend(EnumHistoryRowState state) => state switch
    {
        EnumHistoryRowState.Recoverable => "The note is in the deleted notes: «Recover deleted notes ...» brings it back until that day.",
        EnumHistoryRowState.RecoverableByStaff => "The note is in the deleted notes: a user with the Staff role or higher can recover it.",
        EnumHistoryRowState.NotRecoverable => "The note was deleted and its copy is no longer kept: it can't be recovered.",
        EnumHistoryRowState.OnlyNoted => "Folders, note types, attributes, relation types, users and settings: their changes are only noted, they can't be restored.",
        EnumHistoryRowState.CurrentVersion => "The note as it was after this change is the note as it is now: there is nothing to restore.",
        EnumHistoryRowState.LaterChanges => "Restoring it brings back a later version: the copies in between are no longer kept.",
        EnumHistoryRowState.NotRestorable => "There is no copy of the note after this change, or it is its deletion: it can't be restored.",
        _ => ""
    };

    /// <summary>The states of a list, once each, in the order of the legend (none without a state).</summary>
    public static List<EnumHistoryRowState> InLegend(IEnumerable<EnumHistoryRowState> states) =>
        (states ?? Enumerable.Empty<EnumHistoryRowState>()).Where(s => s != EnumHistoryRowState.None).Distinct().OrderBy(s => (int)s).ToList();
}
