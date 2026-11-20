namespace KNote.Model.Core;

/// <summary>
/// The line ends of the texts KNote saves for the AI (summaries, translations, the notes of the assistant): CR LF, as the notes
/// written in the editors of Windows. Only CR, LF and CR LF are line ends here: unlike string.ReplaceLineEndings, a form feed
/// (the separator of the pages of a document) and the other Unicode separators stay as they are.
/// </summary>
public static class TextLineEndings
{
    public static string ToCrLf(string text) =>
        string.IsNullOrEmpty(text) ? text ?? "" : text.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\r\n");
}
