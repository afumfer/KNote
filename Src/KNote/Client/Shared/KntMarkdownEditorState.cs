namespace KNote.Client.Shared;

/// <summary>
/// What a KntMarkdownEditor remembers while it isn't shown (in NoteForm, another tab of the note is): where its cursor
/// was and how far its text was scrolled, so it starts there again. The positions are those of its text as the browser
/// keeps it (line breaks as LF).
/// </summary>
public class KntMarkdownEditorState
{
    public int? SelectionStart { get; set; }
    public int? SelectionEnd { get; set; }
    public double ScrollTop { get; set; }
    /// <summary>The editor takes the focus when it starts (a block was inserted where its cursor was).</summary>
    public bool Focus { get; set; }

    /// <summary>
    /// The text with the block inserted, in its own paragraph, where the cursor was (at the end if the editor was never
    /// left: it wasn't shown, or not used), and the cursor after it.
    /// </summary>
    public string InsertBlock(string? text, string block)
    {
        var current = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
        var at = Math.Clamp(SelectionEnd ?? current.Length, 0, current.Length);
        var before = current[..at];
        var after = current[at..];
        var lead = at == 0 || before.EndsWith("\n\n") ? "" : before.EndsWith('\n') ? "\n" : "\n\n";
        var trail = after.Length == 0 || after.StartsWith("\n\n") ? "" : after.StartsWith('\n') ? "\n" : "\n\n";

        SelectionStart = SelectionEnd = at + lead.Length + block.Length;
        Focus = true;
        return before + lead + block + trail + after;
    }
}
