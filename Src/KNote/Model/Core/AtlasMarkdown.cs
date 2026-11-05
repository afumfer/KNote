using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KNote.Model;

/// <summary>
/// What KNote Atlas does with the Markdown of the notes people write for the AI (profile, task instructions; chapters 19
/// and 21): the help of their templates are HTML comments, which the AI never receives (Clean), nor the sections left
/// empty; the length in words and its cut; and the lines «Fill in folders and types» adds to a section without touching
/// what is written.
/// </summary>
public static class AtlasMarkdown
{
    private static readonly Regex Comment = new(@"<!--.*?-->", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex Heading = new(@"^(#{1,6})\s+(.*?)\s*#*\s*$", RegexOptions.Compiled);
    private static readonly Regex Word = new(@"\S+", RegexOptions.Compiled);
    private static readonly Regex BlankLines = new(@"\n{3,}", RegexOptions.Compiled);

    /// <summary>Without HTML comments and without the sections that are left with no text (only headings).</summary>
    public static string Clean(string markdown)
    {
        var text = Comment.Replace((markdown ?? "").Replace("\r\n", "\n"), "");
        var lines = text.Split('\n').ToList();

        // A heading stays if some line of text comes before the next heading of its level or a higher one.
        var keep = new bool[lines.Count];
        for (var i = 0; i < lines.Count; i++)
        {
            var heading = Heading.Match(lines[i].Trim());
            if (!heading.Success)
            {
                keep[i] = true;
                continue;
            }
            var level = heading.Groups[1].Length;
            for (var j = i + 1; j < lines.Count; j++)
            {
                var next = Heading.Match(lines[j].Trim());
                if (next.Success)
                {
                    if (next.Groups[1].Length <= level)
                        break;
                    continue;
                }
                if (lines[j].Trim().Length > 0)
                {
                    keep[i] = true;
                    break;
                }
            }
        }

        var cleaned = string.Join("\n", lines.Where((_, i) => keep[i]).Select(l => l.TrimEnd()));
        return BlankLines.Replace(cleaned, "\n\n").Trim();
    }

    public static int CountWords(string text) => Word.Matches(text ?? "").Count;

    /// <summary>The first maxWords words of the text (its lines kept); true if it was cut.</summary>
    public static (string Text, bool Cut) TrimToWords(string text, int maxWords)
    {
        var matches = Word.Matches(text ?? "");
        if (matches.Count <= maxWords)
            return (text ?? "", false);
        var last = matches[maxWords - 1];
        return (text.Substring(0, last.Index + last.Length).TrimEnd() + " […]", true);
    }

    /// <summary>
    /// Adds to the section with that title (any heading level, without case) the lines it does not contain yet (compared
    /// without case), at its end; without the section, it is added at the end of the text as «## title». Nothing is
    /// removed or changed, and the text keeps its line endings (CR+LF, as the notes KNote creates).
    /// </summary>
    public static string AddMissingLines(string markdown, string sectionTitle, IEnumerable<string> lines)
    {
        var added = AddMissingLinesToLf(markdown, sectionTitle, lines);
        return added != markdown && markdown?.Contains("\r\n") == true ? added.ReplaceLineEndings("\r\n") : added;
    }

    private static string AddMissingLinesToLf(string markdown, string sectionTitle, IEnumerable<string> lines)
    {
        var text = (markdown ?? "").Replace("\r\n", "\n");
        var all = text.Split('\n').ToList();

        var start = -1;
        var level = 0;
        for (var i = 0; i < all.Count; i++)
        {
            var heading = Heading.Match(all[i].Trim());
            if (heading.Success && string.Equals(heading.Groups[2].Value.Trim(), sectionTitle, StringComparison.OrdinalIgnoreCase))
            {
                start = i;
                level = heading.Groups[1].Length;
                break;
            }
        }

        if (start < 0)
        {
            var missingAll = lines.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (missingAll.Count == 0)
                return markdown ?? "";
            var appended = new StringBuilder(text.TrimEnd());
            if (appended.Length > 0)
                appended.Append("\n\n");
            appended.Append($"## {sectionTitle}\n\n");
            appended.Append(string.Join("\n", missingAll)).Append('\n');
            return appended.ToString();
        }

        var end = all.Count;
        for (var i = start + 1; i < all.Count; i++)
        {
            var heading = Heading.Match(all[i].Trim());
            if (heading.Success && heading.Groups[1].Length <= level)
            {
                end = i;
                break;
            }
        }

        var section = string.Join("\n", all.Skip(start + 1).Take(end - start - 1));
        var missing = lines
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(l => !section.Contains(l.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (missing.Count == 0)
            return markdown ?? "";

        // After the last line with something written of the section (its comments included), before its blank lines.
        var insertAt = end;
        while (insertAt > start + 1 && all[insertAt - 1].Trim().Length == 0)
            insertAt--;
        var block = new List<string>();
        if (insertAt == start + 1 || !all[insertAt - 1].TrimStart().StartsWith("- "))
            block.Add("");
        block.AddRange(missing);
        if (insertAt == end && end < all.Count)
            block.Add("");
        all.InsertRange(insertAt, block);
        return string.Join("\n", all);
    }
}
