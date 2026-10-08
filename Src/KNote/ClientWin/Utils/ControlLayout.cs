namespace KNote.ClientWin.Utils;

public static class ControlLayout
{
    /// <summary>
    /// Width of a button placed right of the control it acts on ("...", "X"), and its gap to that control, in
    /// logical (96 DPI) pixels. The reference is the note editor's folder button; the Designer files use the
    /// same values, and code placing one of these buttons passes them through LogicalToDeviceUnits.
    /// </summary>
    public const int SideButtonWidth = 27;
    public const int SideButtonGap = 5;

    /// <summary>
    /// Gives a button placed right of the text box it acts on ("...", "X") the same height and vertical
    /// position, so both read as one field. Call it after InitializeComponent(), or after placing the text box
    /// in code: a single-line TextBox's real height comes from its font at run time, not from the Designer.
    /// The button keeps following the text box if it is moved or resized later (e.g. in dark mode, see
    /// AppTheme.AdjustControlsForDarkMode).
    /// </summary>
    public static void AlignToTextBox(this ButtonBase button, Control textBox)
    {
        void Align()
        {
            button.Top = textBox.Top;
            button.Height = textBox.Height;
        }

        Align();
        textBox.LocationChanged += (s, e) => Align();
        textBox.SizeChanged += (s, e) => Align();
    }
}
