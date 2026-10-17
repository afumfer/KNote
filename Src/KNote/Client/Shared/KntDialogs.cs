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

    /// <summary>A document to read (a printable report, Shared/KntReportPreview): an A4 page wide, at most the window's.</summary>
    public static DialogOptions Document(bool showClose = true) => Create("min(1240px, 96vw)", showClose);

    /// <summary>
    /// A dialog drawn by Shared/KntDialogFrame (an entity's editor): without Radzen's title bar, since the frame has
    /// its own header (icon, title, subtitle, close button) and footer. E.g. KntDialogs.Framed(KntDialogs.Medium()).
    /// </summary>
    public static DialogOptions Framed(DialogOptions options)
    {
        options.ShowTitle = false;
        options.ShowClose = false;
        options.CssClass = "knt-framed-dialog";
        return options;
    }

    private static DialogOptions Create(string width, bool showClose) =>
        new() { Width = width, Resizable = true, Draggable = true, ShowClose = showClose };
}
