using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>What the description of a resource is when it is saved: its text, whether it is locked and the model that wrote it.</summary>
public record ResourceDescriptionState(string Description, bool DescriptionByUser, string DescriptionModel);

/// <summary>
/// The description of a note's resource (KNote Atlas, stage 4, D12): what a person writes the AI never overwrites, and the
/// lock is what the editor's checkbox says. The editors check it by themselves when a person writes a text of their own
/// (<see cref="LockWhileEditing"/>), and the person can uncheck it before saving. Every resource read from KNote carries the
/// description it was read with (ResourceInfoDto.LoadedDescription): saving it without changing the text keeps what is
/// saved by then, so loading a whole note and saving it doesn't undo what the AI wrote meanwhile. Code that builds a
/// resource without reading it changes the text to one of its own: that locks it. A description that says nothing (empty,
/// the file name, the text of an image pasted from the clipboard) is not "own": it is shown as the file name and left out of
/// the index.
/// </summary>
public static class ResourceDescriptionRules
{
    public const int TitleLength = 60;

    /// <summary>
    /// Whether the description says something of the resource: not empty, nor its file name (with or without its extension,
    /// compared only by its letters and digits without accents, because the file name is kept with them replaced:
    /// "mi foto (1).png" is saved as "mi_foto__1_.png"), nor the text the apps give an image pasted from the clipboard.
    /// </summary>
    public static bool HasOwnDescription(string description, string name)
    {
        if (string.IsNullOrWhiteSpace(description))
            return false;

        var text = AsciiLettersAndDigits(description);
        if (text.Length == 0)
            return true;  // all in another alphabet: it can't be the file name, whose letters are replaced.

        var fileName = ResourceInfoDto.FileNameOf(name ?? "");
        return text != AsciiLettersAndDigits(fileName)
            && text != AsciiLettersAndDigits(System.IO.Path.GetFileNameWithoutExtension(fileName))
            && text != AsciiLettersAndDigits(KntConst.ClipboardImageResourceDescription);
    }

    /// <summary>
    /// The lock an editor shows while a person types, until the person touches the checkbox: the one it had, or checked when
    /// the text is no longer the one loaded and says something.
    /// </summary>
    public static bool LockWhileEditing(string loadedDescription, bool loadedLock, string description, string name) =>
        loadedLock || (!SameText(description, loadedDescription) && HasOwnDescription(description, name));

    /// <summary>
    /// What is saved. <paramref name="stored"/>: the resource as it is in the database (null for a new one), whose file is
    /// <paramref name="storedName"/>. <paramref name="loadedDescription"/>: the description it was read with, null if it
    /// wasn't read from KNote. The model never comes from outside: it is kept while the text and the file are the same.
    /// </summary>
    public static ResourceDescriptionState Decide(ResourceDescriptionState stored, string storedName,
        string description, bool descriptionByUser, string loadedDescription, string name)
    {
        if (stored == null)
            return new(description, descriptionByUser, null);

        var fromEditor = loadedDescription != null;
        var changed = !SameText(description, fromEditor ? loadedDescription : stored.Description);
        if (!changed)
        {
            // The text the editor loaded may not be the one saved now (the AI wrote it meanwhile): the saved one stays.
            var fileReplaced = !string.Equals(storedName, name, StringComparison.Ordinal);
            return new(stored.Description, descriptionByUser, fileReplaced ? null : stored.DescriptionModel);
        }

        var locked = fromEditor ? descriptionByUser : descriptionByUser || HasOwnDescription(description, name);
        return new(description, locked, null);
    }

    /// <summary>
    /// The short title of the resource (as NoteTaskDto.GetTopic): the first sentence of its description, cut at a word with
    /// "…"; without a description of its own, the file name.
    /// </summary>
    public static string Title(string description, string name, int maxLength = TitleLength)
    {
        if (!HasOwnDescription(description, name))
            return ResourceInfoDto.FileNameOf(name ?? "");

        var line = description.Replace("\r\n", "\n").Split('\n')
            .Select(l => l.Trim().TrimStart('#', '>', '-', '*', '+', ' ').Trim())
            .FirstOrDefault(l => l.Length > 0) ?? "";

        // The end of the first sentence: a mark followed by the start of another one, or by nothing.
        var end = Regex.Match(line, @"[.!?…](?=\s+[\p{Lu}¿¡""«(]|\s*$)");
        var sentence = end.Success ? line.Substring(0, end.Index + 1) : line;
        if (sentence.EndsWith('.') && !sentence.EndsWith(".."))
            sentence = sentence.Substring(0, sentence.Length - 1);

        if (sentence.Length <= maxLength)
            return sentence;

        var cut = sentence.Substring(0, maxLength);
        var space = cut.LastIndexOf(' ');
        if (space > maxLength / 2)
            cut = cut.Substring(0, space);
        return cut.TrimEnd(' ', ',', ';', ':') + "…";
    }

    /// <summary>
    /// A text that can go inside the link to a resource inserted in a note (its Markdown text and title, or an HTML attribute
    /// or text) without breaking it: the quotes, brackets and angle brackets are replaced by similar ones, and the lines joined.
    /// </summary>
    public static string ForLink(string text)
    {
        var builder = new StringBuilder((text ?? "").Length);
        foreach (var c in text ?? "")
        {
            builder.Append(c switch
            {
                '\'' => '’',
                '"' => '”',
                '<' => '‹',
                '>' => '›',
                '[' => '(',
                ']' => ')',
                '\\' => '/',
                '\r' or '\n' or '\t' => ' ',
                _ => c
            });
        }
        return Regex.Replace(builder.ToString(), " {2,}", " ").Trim();
    }

    /// <summary>Who wrote the description, as the editors and viewers show it under it. Empty when there is nothing to say.</summary>
    public static string Author(string description, bool descriptionByUser, string descriptionModel, string name)
    {
        if (!string.IsNullOrEmpty(descriptionModel))
            return $"Written by the AI ({descriptionModel}).";
        if (!HasOwnDescription(description, name))
            return descriptionByUser ? "Locked without a description: the AI won't describe this resource." : "";
        return descriptionByUser ? "Written by a person." : "Written by a person, not locked: the AI may rewrite it and this text would be lost.";
    }

    /// <summary>Whether a person's text is not protected: the editors show <see cref="Author"/> as a warning.</summary>
    public static bool IsAtRisk(string description, bool descriptionByUser, string descriptionModel, string name) =>
        !descriptionByUser && string.IsNullOrEmpty(descriptionModel) && HasOwnDescription(description, name);

    /// <summary>The model that will keep the description an editor is changing: the one it loaded while its text is the same.</summary>
    public static string ModelWhileEditing(string loadedDescription, string loadedModel, string description) =>
        SameText(description, loadedDescription) ? loadedModel : null;

    /// <summary>The same text, but for its line ends and the spaces around it.</summary>
    public static bool SameText(string a, string b) =>
        string.Equals((a ?? "").Replace("\r\n", "\n").Trim(), (b ?? "").Replace("\r\n", "\n").Trim(), StringComparison.Ordinal);

    private static string AsciiLettersAndDigits(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var c in text.Normalize(NormalizationForm.FormD))
            if (c < 128 && char.IsLetterOrDigit(c))
                builder.Append(char.ToLowerInvariant(c));
        return builder.ToString();
    }
}
