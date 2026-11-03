using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KNote.Model;

/// <summary>
/// The cleaning of a note's Markdown before it goes to the semantic index (NoteChunkBuilder), the minimal one of KNote
/// Atlas stage 0: what says nothing of the meaning, or would flood it, is taken out. Embedded images and HTML comments
/// go away (of a Markdown image, its alternative text stays), long code blocks keep their first lines and very long
/// lines of code (logs) are cut, runs of characters without spaces (encoded data) are cut, and there is at most one
/// blank line in a row. A paragraph is one line in Markdown: out of code, lines are not cut. The full cleaning of
/// stage 2 extends this one.
/// </summary>
public static class NoteTextCleaner
{
    // A code block of more lines than this keeps KeptCodeLines of them.
    public const int MaxCodeLines = 30;
    public const int KeptCodeLines = 15;
    public const int MaxCodeLineLength = 400;
    public const int MaxWordLength = 200;
    public const int KeptWordLength = 50;

    private static readonly Regex HtmlComment = new(@"<!--.*?-->", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex HtmlImage = new(@"<img\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex MarkdownImage = new(@"!\[([^\]]*)\]\([^)]*\)", RegexOptions.Compiled);
    private static readonly Regex DataUri = new(@"data:[\w/+.-]+;base64,[A-Za-z0-9+/=]+", RegexOptions.Compiled);
    private static readonly Regex LongWord = new($@"\S{{{MaxWordLength + 1},}}", RegexOptions.Compiled);

    /// <summary>Line breaks as "\n" (the hashes of the index do not change with the editor's line ends).</summary>
    public static string NormalizeLineEnds(string text) =>
        (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');

    public static string Clean(string markdown)
    {
        var text = NormalizeLineEnds(markdown);
        text = HtmlComment.Replace(text, "");
        text = HtmlImage.Replace(text, "");
        text = MarkdownImage.Replace(text, m => m.Groups[1].Value.Trim());
        text = DataUri.Replace(text, "");

        var lines = ShortenCodeBlocks(text.Split('\n'));

        var result = new StringBuilder();
        var blankLines = 0;
        foreach (var (rawLine, isCode) in lines)
        {
            var line = LongWord.Replace(rawLine.TrimEnd(), m => m.Value.Substring(0, KeptWordLength) + "\u2026");
            if (line.Length == 0)
            {
                blankLines++;
                continue;
            }

            if (result.Length > 0)
                result.Append(blankLines > 0 ? "\n\n" : "\n");
            blankLines = 0;

            result.Append(isCode && line.Length > MaxCodeLineLength ? line.Substring(0, MaxCodeLineLength) + "\u2026" : line);
        }

        return result.ToString();
    }

    /// <summary>Whether the line opens or closes a fenced code block (``` or ~~~).</summary>
    public static bool IsFence(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("```") || trimmed.StartsWith("~~~");
    }

    // The lines, the code ones marked (fences included), with the long code blocks shortened.
    private static List<(string Line, bool IsCode)> ShortenCodeBlocks(string[] lines)
    {
        var result = new List<(string Line, bool IsCode)>();
        var i = 0;
        while (i < lines.Length)
        {
            if (!IsFence(lines[i]))
            {
                result.Add((lines[i++], false));
                continue;
            }

            // The block: from its fence to the closing one (or the end of the text, if it is not closed).
            var start = i++;
            while (i < lines.Length && !IsFence(lines[i]))
                i++;
            var codeLines = i - start - 1;
            var closed = i < lines.Length;

            result.Add((lines[start], true));
            var kept = codeLines > MaxCodeLines ? KeptCodeLines : codeLines;
            for (var k = 1; k <= kept; k++)
                result.Add((lines[start + k], true));
            if (codeLines > MaxCodeLines)
                result.Add(($"\u2026 ({codeLines - KeptCodeLines} more lines)", true));
            if (closed)
                result.Add((lines[i++], true));
        }
        return result;
    }
}
