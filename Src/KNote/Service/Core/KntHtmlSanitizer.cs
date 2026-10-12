using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Ganss.Xss;

namespace KNote.Service.Core;

/// <summary>
/// Makes the HTML of a note (its description, after Markdown) safe to show: it keeps the formatting written in
/// ClientWin's HTML editor or in Markdown (fonts, colors, styles, tables, images, links...) and removes what could
/// run script with the session of whoever reads it (script and other active elements, on* attributes,
/// javascript: and other unknown URLs). Used through IKntNoteService (UtilMarkdownToHtml, UtilSanitizeHtml) by
/// ClientWin (note views and HTML editor) and by Server for the web app (api/notes/html, HomeNotes), so both apps
/// show a note the same way. Thread-safe.
/// </summary>
public static class KntHtmlSanitizer
{
    private static readonly HtmlSanitizer sanitizer = Create();

    /// <summary>The safe version of html (empty for null).</summary>
    public static string Sanitize(string html) =>
        string.IsNullOrEmpty(html) ? "" : sanitizer.Sanitize(html);

    // Removed elements whose content is code or data, not text to keep.
    private static readonly HashSet<string> RemovedWithContent = new(StringComparer.OrdinalIgnoreCase)
    {
        "SCRIPT", "STYLE", "NOSCRIPT", "TEMPLATE", "IFRAME", "FRAME", "FRAMESET", "OBJECT", "EMBED", "APPLET",
        "TEXTAREA", "SELECT", "TITLE", "XMP", "NOEMBED", "NOFRAMES"
    };

    // rgba(r, g, b, 1), as AngleSharp writes an opaque color.
    private static readonly Regex OpaqueColor = new(@"rgba\((\d{1,3}), (\d{1,3}), (\d{1,3}), 1\)", RegexOptions.Compiled);

    private static HtmlSanitizer Create()
    {
        // The library's defaults (formatting tags, attributes and CSS properties, http/https links) plus what
        // notes use.
        var htmlSanitizer = new HtmlSanitizer();

        // An element removed keeps its text (e.g. tags pasted from Word in ClientWin's editor), except the ones whose
        // content is not text to read (see RemovingTag).
        htmlSanitizer.KeepChildNodes = true;
        htmlSanitizer.RemovingTag += (_, e) =>
        {
            if (RemovedWithContent.Contains(e.Tag.NodeName))
                e.Tag.InnerHtml = "";
        };

        // No forms: a note could ask for data and send it anywhere.
        foreach (var tag in new[] { "form", "button", "textarea", "select", "option", "optgroup", "fieldset", "legend" })
            htmlSanitizer.AllowedTags.Remove(tag);

        // ClientWin's HTML editor writes the font with <font face="...">.
        htmlSanitizer.AllowedAttributes.Add("face");

        // Markdown: heading ids (anchors), code classes (language-...), task lists (checkboxes, see below).
        htmlSanitizer.AllowedAttributes.Add("id");
        htmlSanitizer.AllowedAttributes.Add("class");
        htmlSanitizer.AllowedTags.Add("input");
        htmlSanitizer.AllowedAttributes.Add("type");
        htmlSanitizer.AllowedAttributes.Add("checked");
        htmlSanitizer.AllowedAttributes.Add("disabled");

        // Links to mail and to local files (ClientWin); resources are https (https://knote.resources in ClientWin).
        htmlSanitizer.AllowedSchemes.Add("mailto");
        htmlSanitizer.AllowedSchemes.Add("file");

        // Images embedded in the note (data:image/...), only as images.
        htmlSanitizer.FilterUrl += (_, e) =>
        {
            if (e.Tag.NodeName == "IMG" && e.OriginalUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                e.SanitizedUrl = e.OriginalUrl;
        };

        htmlSanitizer.PostProcessNode += (_, e) =>
        {
            // The styles come back with their colors as rgba(...), which ClientWin's HTML editor (Internet Explorer's
            // engine, IE7 mode) doesn't know: the opaque ones go back to #rrggbb.
            if (e.Node is IElement styled && styled.GetAttribute("style") is { } style && style.Contains("rgba("))
                styled.SetAttribute("style", OpaqueColor.Replace(style, m =>
                    $"#{byte.Parse(m.Groups[1].Value):x2}{byte.Parse(m.Groups[2].Value):x2}{byte.Parse(m.Groups[3].Value):x2}"));

            // Only the read-only checkboxes of Markdown's task lists.
            if (e.Node is IElement { NodeName: "INPUT" } input)
            {
                if (!string.Equals(input.GetAttribute("type"), "checkbox", StringComparison.OrdinalIgnoreCase))
                    input.Remove();
                else
                    input.SetAttribute("disabled", "disabled");
            }
        };

        // Comments are kept: they don't run, and some notes keep data in them (AI assistant sessions).
        htmlSanitizer.RemovingComment += (_, e) => e.Cancel = true;

        return htmlSanitizer;
    }
}
