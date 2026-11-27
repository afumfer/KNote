using KNote.Model.Core;
using KNote.Model.Dto;
using Radzen;

namespace KNote.Client.Shared;

/// <summary>
/// How the web app draws the state of a row of a list of the history (Model/Core/HistoryRowStates: «Repository changes» and the
/// «History» tab): its icon and the tone of the row (the --knt-history-* variables of knt-theme.css) painted on its cells.
/// The texts and the legend are the Model's, the same as in ClientWin.
/// </summary>
public static class KntHistoryStates
{
    public static string Icon(EnumHistoryRowState state) => state switch
    {
        EnumHistoryRowState.Recoverable => "restore_from_trash",
        EnumHistoryRowState.RecoverableByStaff => "lock",
        EnumHistoryRowState.NotRecoverable or EnumHistoryRowState.NotRestorable => "block",
        EnumHistoryRowState.OnlyNoted => "info",
        EnumHistoryRowState.CurrentVersion => "check_circle",
        EnumHistoryRowState.LaterChanges => "history_toggle_off",
        _ => ""
    };

    public static string? ToneVariable(EnumHistoryRowState state) => HistoryRowStates.Tone(state) switch
    {
        EnumHistoryRowTone.Amber => "var(--knt-history-amber)",
        EnumHistoryRowTone.Orange => "var(--knt-history-orange)",
        EnumHistoryRowTone.Grey => "var(--knt-history-grey)",
        _ => null
    };

    /// <summary>
    /// The tone of the row on each of its cells (RadzenDataGrid.CellRender): a translucent inset shadow, so the colours of the theme
    /// and of the selected row still show under it (a background would hide the selection).
    /// </summary>
    public static void PaintCell<TItem>(DataGridCellRenderEventArgs<TItem> args, Func<TItem, EnumHistoryRowState> stateOf)
    {
        if (args.Data != null && ToneVariable(stateOf(args.Data)) is string tone)
            args.Attributes.Add("style", $"box-shadow: inset 0 0 0 100vmax {tone};");
    }
}
