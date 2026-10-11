using Radzen;

namespace KNote.Client.Shared;

/// <summary>
/// Options for the app's dialogs (DialogService.OpenAsync), so they all share the same sizes and behavior.
/// No height is set: a dialog fits its content (no empty space above the buttons). A dialog with a list or
/// tree that can grow limits that area itself (max-height + scroll), not the dialog.
/// </summary>
public static class KntDialogs
{
    /// <summary>A few fields (login, note type, attribute value...).</summary>
    public static DialogOptions Small(bool showClose = true) => Create("480px", showClose);

    /// <summary>A form with several fields, or a selector.</summary>
    public static DialogOptions Medium(bool showClose = true) => Create("640px", showClose);

    /// <summary>Fields side by side, or a grid.</summary>
    public static DialogOptions Large(bool showClose = true) => Create("860px", showClose);

    private static DialogOptions Create(string width, bool showClose) =>
        new() { Width = width, Resizable = true, Draggable = true, ShowClose = showClose };
}
