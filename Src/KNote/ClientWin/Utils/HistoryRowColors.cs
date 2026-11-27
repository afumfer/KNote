using KNote.Model.Core;

namespace KNote.ClientWin.Utils;

/// <summary>
/// The background of a row of a list of the history by its tone (Model/Core/HistoryRowStates, the same tones as the web app): soft
/// amber, soft orange or soft grey, with a version for the dark mode (AppTheme). Empty: the list's own colour.
/// </summary>
public static class HistoryRowColors
{
    public static Color For(EnumHistoryRowTone tone) => For(tone, AppTheme.IsDark);

    public static Color For(EnumHistoryRowTone tone, bool dark) => tone switch
    {
        EnumHistoryRowTone.Amber => dark ? Color.FromArgb(74, 62, 30) : Color.FromArgb(255, 243, 205),
        EnumHistoryRowTone.Orange => dark ? Color.FromArgb(82, 46, 30) : Color.FromArgb(255, 222, 200),
        EnumHistoryRowTone.Grey => dark ? Color.FromArgb(52, 52, 52) : Color.FromArgb(238, 238, 238),
        _ => Color.Empty
    };

    public static Color For(EnumHistoryRowState state) => For(HistoryRowStates.Tone(state));

    /// <summary>The state of an item of a ListView: its tone as the background (none: the list's colour) and why in its tooltip.</summary>
    public static void Paint(ListViewItem item, EnumHistoryRowState state)
    {
        var color = For(state);
        item.UseItemStyleForSubItems = true;
        item.BackColor = color.IsEmpty ? item.ListView?.BackColor ?? SystemColors.Window : color;
        if (state != EnumHistoryRowState.None)
            item.ToolTipText = $"{item.ToolTipText}\n\n{HistoryRowStates.Text(state)}: {HistoryRowStates.Legend(state)}".TrimStart();
    }
}
