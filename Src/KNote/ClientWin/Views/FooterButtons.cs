namespace KNote.ClientWin.Views;

/// <summary>
/// The buttons at the foot of the windows built in code (DeletedNotesForm, NoteVersionForm, NoteCompareForm), as the Designer's
/// Accept / Cancel: 75 × 29 at 96 dpi, scaled with the form (AutoScaleMode.Font). Not AutoSize: with it the button was measured at
/// the scaling of Windows and then scaled again with the form (at 200 %, 96 high instead of 62). After the form is scaled
/// (OnLoad), FitTexts only makes wider the ones whose text doesn't fit.
/// </summary>
internal static class FooterButtons
{
    private static readonly object FooterTag = new();

    public static Button Create(string text) =>
        new() { Text = text, Size = new Size(75, 29), UseVisualStyleBackColor = true, Tag = FooterTag };

    public static void FitTexts(Control container)
    {
        foreach (Control control in container.Controls)
        {
            if (control is Button button && ReferenceEquals(button.Tag, FooterTag))
            {
                // The text and, around it, as much room as the button's height.
                var needed = TextRenderer.MeasureText(button.Text, button.Font).Width + button.Height;
                if (button.Width < needed)
                    button.Width = needed;
            }
            FitTexts(control);
        }
    }
}
