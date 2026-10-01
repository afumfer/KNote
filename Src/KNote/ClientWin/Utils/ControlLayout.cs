namespace KNote.ClientWin.Utils;

public static class ControlLayout
{
    /// <summary>
    /// Gives a button placed right of the text box it acts on ("...", "X") the same height and vertical
    /// position, so both read as one field. Call it after InitializeComponent(), or after placing the text box
    /// in code: a single-line TextBox's real height comes from its font at run time, not from the Designer.
    /// </summary>
    public static void AlignToTextBox(this ButtonBase button, Control textBox)
    {
        button.Top = textBox.Top;
        button.Height = textBox.Height;
    }
}
