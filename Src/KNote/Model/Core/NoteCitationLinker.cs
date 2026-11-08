using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KNote.Model;

/// <summary>
/// Turns the citations of notes (#123) of an HTML already made safe into links (KNote Atlas, D14), without looking at
/// whether the note exists: it runs each time an answer of the AI is drawn, and in the browser in the web. Only in text: not
/// inside a tag, a link or code; not in #urgent, C#123, #0 or the numeric entities (&amp;#123;). What a click does is the
/// app's: the web opens the note's viewer (url), ClientWin stops the navigation to DesktopUrl.
/// </summary>
public static class NoteCitationLinker
{
    public const string LinkClass = "knt-note-link";

    private const string DesktopScheme = "knt-note:";

    private static readonly Regex Citation = new(@"(?<![\w&#/])#([1-9][0-9]{0,8})(?![\w-])", RegexOptions.Compiled);

    // Their content is left as it is.
    private static readonly HashSet<string> SkippedElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "code", "pre", "kbd", "samp", "script", "style", "textarea", "button"
    };

    public static string Link(string html, Func<int, string> url)
    {
        if (string.IsNullOrEmpty(html) || !html.Contains('#'))
            return html;

        var result = new StringBuilder(html.Length + 64);
        var skipped = 0;
        var position = 0;
        while (position < html.Length)
        {
            var tag = html.IndexOf('<', position);
            var textEnd = tag < 0 ? html.Length : tag;
            var text = html.Substring(position, textEnd - position);
            result.Append(skipped > 0 ? text : LinkText(text, url));
            if (tag < 0)
                break;

            var tagEnd = FindTagEnd(html, tag);
            UpdateSkipped(html, tag, tagEnd, ref skipped);
            result.Append(html, tag, tagEnd - tag);
            position = tagEnd;
        }
        return result.ToString();
    }

    /// <summary>The link of a citation in ClientWin's views, which stop the navigation to it (TryGetDesktopNumber).</summary>
    public static string DesktopUrl(int noteNumber) => DesktopScheme + noteNumber;

    public static string LinkForDesktop(string html) => Link(html, DesktopUrl);

    public static bool TryGetDesktopNumber(string url, out int noteNumber)
    {
        noteNumber = 0;
        return url != null && url.StartsWith(DesktopScheme, StringComparison.OrdinalIgnoreCase)
            && int.TryParse(url.Substring(DesktopScheme.Length), out noteNumber);
    }

    private static string LinkText(string text, Func<int, string> url) =>
        Citation.Replace(text, match =>
        {
            var number = int.Parse(match.Groups[1].Value);
            return $"<a class=\"{LinkClass}\" href=\"{url(number)}\" data-note-number=\"{number}\">{match.Value}</a>";
        });

    // Where what starts at '<' ends: a comment, or a tag, where a '>' inside an attribute's quotes does not end it.
    private static int FindTagEnd(string html, int start)
    {
        if (string.CompareOrdinal(html, start, "<!--", 0, 4) == 0)
        {
            var commentEnd = html.IndexOf("-->", start, StringComparison.Ordinal);
            return commentEnd < 0 ? html.Length : commentEnd + 3;
        }

        char? quote = null;
        for (var i = start + 1; i < html.Length; i++)
        {
            var c = html[i];
            if (quote != null)
            {
                if (c == quote)
                    quote = null;
            }
            else if (c == '"' || c == '\'')
                quote = c;
            else if (c == '>')
                return i + 1;
        }
        return html.Length;
    }

    private static void UpdateSkipped(string html, int start, int end, ref int skipped)
    {
        var closing = start + 1 < html.Length && html[start + 1] == '/';
        var nameStart = start + (closing ? 2 : 1);
        var nameEnd = nameStart;
        while (nameEnd < end && char.IsLetterOrDigit(html[nameEnd]))
            nameEnd++;
        if (!SkippedElements.Contains(html.Substring(nameStart, nameEnd - nameStart)))
            return;

        if (closing)
            skipped = Math.Max(0, skipped - 1);
        else if (html[end - 1] != '>' || html[end - 2] != '/')
            skipped++;
    }
}
