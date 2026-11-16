using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KNote.Model;

/// <summary>
/// The cleaning of a note to read it in the summaries (KNote Atlas, stage 2, chapter 28, D7): the one of the semantic index
/// (NoteTextCleaner: embedded images, HTML comments, long code blocks, encoded data) and, besides, what does not help to understand
/// the note: quoted mails (lines with «>», the line «On … wrote:» before them, and an original message forwarded below), the
/// signature after «-- », and huge tables and lists (their size and their first rows). Only to read: the note does not change, and
/// the index keeps its own cleaning (rebuilding it is not needed). Code blocks are left to NoteTextCleaner.
/// </summary>
public static class NoteReadingCleaner
{
    public const int MaxTableRows = 20;
    public const int KeptTableRows = 5;
    public const int MaxListItems = 30;
    public const int KeptListItems = 10;
    public const int MaxSignatureLines = 10;

    private static readonly Regex QuoteHeader = new(@"^\s*(On\b.*\bwrote:|El\b.*\bescribi[oó]:)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex OriginalMessage = new(@"^\s*-{2,}\s*(Original Message|Mensaje original|Forwarded message|Mensaje reenviado)\s*-{2,}\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ListItem = new(@"^\s*([-*+]|\d{1,4}[.)])\s+\S", RegexOptions.Compiled);
    private static readonly Regex Word = new(@"[\p{L}\p{N}]", RegexOptions.Compiled);

    public static string Clean(string markdown)
    {
        var lines = NoteTextCleaner.NormalizeLineEnds(markdown).Split('\n').ToList();
        lines = CutOriginalMessage(lines);
        lines = RemoveSignature(lines);
        lines = RemoveQuotes(lines);
        lines = ShortenTables(lines);
        lines = ShortenLists(lines);
        return NoteTextCleaner.Clean(string.Join("\n", lines));
    }

    /// <summary>The words of a text: its pieces between spaces with a letter or a digit.</summary>
    public static int CountWords(string text) =>
        string.IsNullOrWhiteSpace(text) ? 0 : text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Count(w => Word.IsMatch(w));

    // The lines out of code blocks, with their index (code is NoteTextCleaner's).
    private static bool[] CodeLines(List<string> lines)
    {
        var code = new bool[lines.Count];
        var inCode = false;
        for (var i = 0; i < lines.Count; i++)
        {
            if (NoteTextCleaner.IsFence(lines[i]))
            {
                code[i] = true;
                inCode = !inCode;
                continue;
            }
            code[i] = inCode;
        }
        return code;
    }

    // An original message forwarded or answered below the note: everything from its line on.
    private static List<string> CutOriginalMessage(List<string> lines)
    {
        var code = CodeLines(lines);
        for (var i = 0; i < lines.Count; i++)
            if (!code[i] && OriginalMessage.IsMatch(lines[i]))
                return lines.Take(i).Append($"[An original message of {lines.Count - i - 1} lines left out]").ToList();
        return lines;
    }

    // The signature: the line «-- » and at most MaxSignatureLines lines after it, to the end.
    private static List<string> RemoveSignature(List<string> lines)
    {
        var code = CodeLines(lines);
        for (var i = lines.Count - 1; i >= 0 && i >= lines.Count - 1 - MaxSignatureLines; i--)
            if (!code[i] && lines[i] == "-- ")
                return lines.Take(i).ToList();
        return lines;
    }

    // The quoted lines (with «>») and the line «On … wrote:» just before them.
    private static List<string> RemoveQuotes(List<string> lines)
    {
        var code = CodeLines(lines);
        var result = new List<string>();
        var i = 0;
        while (i < lines.Count)
        {
            if (code[i] || !lines[i].TrimStart().StartsWith('>'))
            {
                result.Add(lines[i]);
                i++;
                continue;
            }

            var start = i;
            while (i < lines.Count && !code[i] && lines[i].TrimStart().StartsWith('>'))
                i++;
            // The header line, and the blank one between it and the quote.
            var last = result.Count - 1;
            while (last >= 0 && result[last].Trim().Length == 0)
                last--;
            if (last >= 0 && QuoteHeader.IsMatch(result[last]))
                result.RemoveRange(last, result.Count - last);
            result.Add($"[A quoted message of {i - start} lines left out]");
        }
        return result;
    }

    // A table of more than MaxTableRows rows: its header, its separator and its first KeptTableRows rows, and its size.
    private static List<string> ShortenTables(List<string> lines)
    {
        var code = CodeLines(lines);
        var result = new List<string>();
        var i = 0;
        while (i < lines.Count)
        {
            if (code[i] || !IsTableLine(lines[i]))
            {
                result.Add(lines[i]);
                i++;
                continue;
            }

            var start = i;
            while (i < lines.Count && !code[i] && IsTableLine(lines[i]))
                i++;
            var block = lines.GetRange(start, i - start);
            var rows = block.Count - 2;
            if (rows <= MaxTableRows)
                result.AddRange(block);
            else
            {
                result.AddRange(block.Take(2 + KeptTableRows));
                result.Add("");
                result.Add($"[A table of {rows} rows: the first {KeptTableRows} shown]");
            }
        }
        return result;
    }

    // A list of more than MaxListItems items: its first KeptListItems items (with their lines), and its size.
    private static List<string> ShortenLists(List<string> lines)
    {
        var code = CodeLines(lines);
        var result = new List<string>();
        var i = 0;
        while (i < lines.Count)
        {
            if (code[i] || !ListItem.IsMatch(lines[i]))
            {
                result.Add(lines[i]);
                i++;
                continue;
            }

            // The list: its items and the indented lines that go on them.
            var start = i;
            var itemStarts = new List<int>();
            while (i < lines.Count && !code[i] && (ListItem.IsMatch(lines[i]) || (lines[i].Length > 0 && char.IsWhiteSpace(lines[i][0]) && lines[i].Trim().Length > 0)))
            {
                if (ListItem.IsMatch(lines[i]))
                    itemStarts.Add(i);
                i++;
            }
            if (itemStarts.Count <= MaxListItems)
                result.AddRange(lines.GetRange(start, i - start));
            else
            {
                result.AddRange(lines.GetRange(start, itemStarts[KeptListItems] - start));
                result.Add("");
                result.Add($"[A list of {itemStarts.Count} items: the first {KeptListItems} shown]");
            }
        }
        return result;
    }

    private static bool IsTableLine(string line)
    {
        var trimmed = line.Trim();
        return trimmed.Length > 1 && trimmed.StartsWith('|');
    }
}
