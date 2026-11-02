using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// A fragment of a note for the semantic index as NoteChunkBuilder makes it, before it has a vector (a row of
/// NoteChunks with it). Content is what is kept and shown; the model receives
/// TextForEmbedding (the heading of context, not kept, and the content). ContentHash is the hash of that text: a
/// fragment whose hash does not change keeps its vector.
/// </summary>
public class NoteChunkDraft
{
    public NoteChunkSourceType SourceType { get; init; }
    public Guid SourceId { get; init; }
    public int ChunkIndex { get; init; }
    public string Heading { get; init; }
    public string Content { get; init; }
    public string ContentHash { get; init; }

    public string TextForEmbedding => NoteChunkBuilder.TextForEmbedding(Heading, Content);
}

/// <summary>The fragments of a note and the hash of everything they were made from.</summary>
public class NoteChunkSet
{
    public string SourceHash { get; init; }
    public List<NoteChunkDraft> Chunks { get; init; } = new();
}

/// <summary>
/// The rules that split a note into the fragments of the semantic index (KNote Atlas, stage 0). The fixed labels of the
/// content are in English, as the app.
/// <list type="bullet">
/// <item>Data (one per note): "Key: value" lines with the topic, type, folder, tags, the attributes with a value and the
/// names of the resources (MaxResourceNames at most).</item>
/// <item>Text: the clean description (NoteTextCleaner) by Markdown sections (headings out of code blocks). A section of
/// less than MinSectionLength characters joins the next one while they fit in MaxChunkLength; a longer one is cut by
/// paragraphs, lines or, at last, spaces. No overlap: the contents in order are exactly the clean text.</item>
/// <item>Task (one per task): "Task:", "Status: pending | resolved" and its clean description.</item>
/// <item>Resource (one per resource with a description of its own, HasOwnDescription): "Resource:", "Type:" and its
/// description.</item>
/// </list>
/// Every fragment is cut to MaxChunkLength as the text is. SourceId is the task or the resource, Guid.Empty for the note's
/// own fragments. The heading of context is the topic and the path of headings, joined by HeadingSeparator, in the
/// text, the topic in tasks and resources and none in the data (they have the topic). SourceHash covers the whole input, not cleaned, and RulesVersion.
/// </summary>
public static class NoteChunkBuilder
{
    // Changing what the fragments are made of changes the SourceHash of every note: raise it with the rules.
    public const int RulesVersion = 1;
    public const int MaxChunkLength = 1500;
    public const int MinSectionLength = 300;
    public const int MaxResourceNames = 20;
    public const string HeadingSeparator = " \u203A ";

    private static readonly Regex MarkdownHeading = new(@"^ {0,3}(#{1,6})[ \t]+(.*?)[ \t#]*$", RegexOptions.Compiled);
    private static readonly Regex Spaces = new(@"\s+", RegexOptions.Compiled);

    public static NoteChunkSet Build(NoteChunkSource source)
    {
        var topic = OneLine(source.Topic);
        var chunks = new List<NoteChunkDraft>();

        Add(chunks, NoteChunkSourceType.Data, Guid.Empty, "", Split(DataContent(source)));

        var index = 0;
        foreach (var (heading, content) in TextPieces(topic, source.Description))
            chunks.Add(NewChunk(NoteChunkSourceType.Text, Guid.Empty, index++, heading, content));

        foreach (var task in source.Tasks ?? new())
            Add(chunks, NoteChunkSourceType.Task, task.TaskId, topic, Split(TaskContent(task)));

        foreach (var resource in (source.Resources ?? new()).Where(HasOwnDescription))
            Add(chunks, NoteChunkSourceType.Resource, resource.ResourceId, topic, Split(ResourceContent(resource)));

        return new NoteChunkSet { SourceHash = SourceHashOf(source), Chunks = chunks };
    }

    /// <summary>The text the embedding model receives for a fragment.</summary>
    public static string TextForEmbedding(string heading, string content)
    {
        var text = (content ?? "").Trim();
        return string.IsNullOrEmpty(heading) ? text : heading + "\n\n" + text;
    }

    /// <summary>
    /// Whether the description of a resource says something of it: not empty, nor its file name, nor the text the apps
    /// give an image pasted from the clipboard. The others are left out of the index.
    /// </summary>
    public static bool HasOwnDescription(NoteChunkResource resource)
    {
        var description = resource.Description?.Trim();
        if (string.IsNullOrEmpty(description))
            return false;

        var name = resource.Name?.Trim() ?? "";
        return !string.Equals(description, name, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(description, System.IO.Path.GetFileNameWithoutExtension(name), StringComparison.OrdinalIgnoreCase)
            && !string.Equals(description, KntConst.ClipboardImageResourceDescription, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>SHA-256 in lowercase hexadecimal of the text, with its line ends normalized.</summary>
    public static string Hash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(NoteTextCleaner.NormalizeLineEnds(text))));

    #region Private methods

    private static NoteChunkDraft NewChunk(NoteChunkSourceType sourceType, Guid sourceId, int index, string heading, string content) => new()
    {
        SourceType = sourceType,
        SourceId = sourceId,
        ChunkIndex = index,
        Heading = heading,
        Content = content,
        ContentHash = Hash(TextForEmbedding(heading, content))
    };

    private static void Add(List<NoteChunkDraft> chunks, NoteChunkSourceType sourceType, Guid sourceId, string heading, List<string> pieces)
    {
        for (var i = 0; i < pieces.Count; i++)
            chunks.Add(NewChunk(sourceType, sourceId, i, heading, pieces[i]));
    }

    private static string DataContent(NoteChunkSource source)
    {
        var lines = new List<string> { $"Topic: {OneLine(source.Topic)}" };
        AddLine(lines, "Type", source.NoteType);
        AddLine(lines, "Folder", source.FolderPath);
        AddLine(lines, "Tags", source.Tags);
        foreach (var attribute in source.Attributes ?? new())
            if (!string.IsNullOrWhiteSpace(attribute.Name))
                AddLine(lines, OneLine(attribute.Name), attribute.Value);

        var names = (source.Resources ?? new()).Select(r => OneLine(r.Name)).Where(n => n.Length > 0).ToList();
        if (names.Count > 0)
        {
            var listed = string.Join(", ", names.Take(MaxResourceNames));
            if (names.Count > MaxResourceNames)
                listed += $", \u2026 ({names.Count - MaxResourceNames} more)";
            lines.Add($"Resources: {listed}");
        }

        return string.Join("\n", lines);
    }

    private static string TaskContent(NoteChunkTask task)
    {
        var content = $"Task: {OneLine(task.Topic)}\nStatus: {(task.Resolved ? "resolved" : "pending")}";
        var description = NoteTextCleaner.Clean(task.Description);
        return description.Length > 0 ? content + "\n\n" + description : content;
    }

    private static string ResourceContent(NoteChunkResource resource)
    {
        var lines = new List<string> { $"Resource: {OneLine(resource.Name)}" };
        AddLine(lines, "Type", resource.FileType);
        return string.Join("\n", lines) + "\n\n" + NoteTextCleaner.Clean(resource.Description);
    }

    private static void AddLine(List<string> lines, string key, string value)
    {
        var text = OneLine(value);
        if (text.Length > 0)
            lines.Add($"{key}: {text}");
    }

    private static string OneLine(string value) => Spaces.Replace(value ?? "", " ").Trim();

    // The clean description by sections, the small ones joined to the next, the long ones cut.
    private static IEnumerable<(string Heading, string Content)> TextPieces(string topic, string description)
    {
        var text = NoteTextCleaner.Clean(description);
        if (text.Length == 0)
            yield break;

        var merged = new List<(string Heading, string Text)>();
        foreach (var section in Sections(topic, text))
        {
            if (merged.Count > 0)
            {
                var last = merged[^1];
                if (last.Text.Length < MinSectionLength && last.Text.Length + section.Text.Length <= MaxChunkLength)
                {
                    merged[^1] = (last.Heading, last.Text + section.Text);
                    continue;
                }
            }
            merged.Add(section);
        }

        foreach (var section in merged)
            foreach (var piece in Split(section.Text))
                yield return (section.Heading, piece);
    }

    // The text from each heading (out of code blocks) to the next, with its path of headings. The sections together are
    // the text.
    private static List<(string Heading, string Text)> Sections(string topic, string text)
    {
        var sections = new List<(string Heading, string Text)>();
        var path = new List<(int Level, string Title)>();
        var heading = topic;
        var start = 0;
        var position = 0;
        var inCode = false;

        while (position < text.Length)
        {
            var end = text.IndexOf('\n', position);
            var line = end < 0 ? text[position..] : text[position..end];

            if (NoteTextCleaner.IsFence(line))
                inCode = !inCode;
            else if (!inCode && MarkdownHeading.Match(line) is { Success: true } match)
            {
                if (position > start)
                    sections.Add((heading, text[start..position]));
                start = position;

                var level = match.Groups[1].Value.Length;
                while (path.Count > 0 && path[^1].Level >= level)
                    path.RemoveAt(path.Count - 1);
                path.Add((level, OneLine(match.Groups[2].Value)));
                heading = string.Join(HeadingSeparator,
                    new[] { topic }.Concat(path.Select(p => p.Title)).Where(t => t.Length > 0));
            }

            position = end < 0 ? text.Length : end + 1;
        }

        sections.Add((heading, text[start..]));
        return sections;
    }

    // Cuts a text into pieces of MaxChunkLength at most, after a blank line, a line end or a space if there is one in
    // the last two thirds of the piece. The pieces together are the text.
    private static List<string> Split(string text)
    {
        var pieces = new List<string>();
        var start = 0;
        while (text.Length - start > MaxChunkLength)
        {
            var cut = CutPosition(text, start);
            pieces.Add(text[start..cut]);
            start = cut;
        }
        if (start < text.Length)
            pieces.Add(text[start..]);
        return pieces;
    }

    private static int CutPosition(string text, int start)
    {
        var limit = start + MaxChunkLength;
        var minCut = start + MaxChunkLength / 3;
        foreach (var separator in new[] { "\n\n", "\n", " " })
        {
            // The separator ends at limit at most.
            var at = text.LastIndexOf(separator, limit - 1, limit - minCut, StringComparison.Ordinal);
            if (at >= minCut)
                return at + separator.Length;
        }
        return limit;
    }

    private static string SourceHashOf(NoteChunkSource source)
    {
        var text = new StringBuilder();
        void Add(string key, object value) =>
            text.Append(key).Append('\u001F').Append(value?.ToString() ?? "").Append('\u001E');

        Add("rules", RulesVersion);
        Add("topic", source.Topic);
        Add("type", source.NoteType);
        Add("folder", source.FolderPath);
        Add("tags", source.Tags);
        foreach (var attribute in source.Attributes ?? new())
        {
            Add("attribute", attribute.Name);
            Add("value", attribute.Value);
        }
        Add("description", source.Description);
        foreach (var task in source.Tasks ?? new())
        {
            Add("task", task.TaskId);
            Add("topic", task.Topic);
            Add("description", task.Description);
            Add("resolved", task.Resolved);
        }
        foreach (var resource in source.Resources ?? new())
        {
            Add("resource", resource.ResourceId);
            Add("name", resource.Name);
            Add("type", resource.FileType);
            Add("description", resource.Description);
        }
        return Hash(text.ToString());
    }

    #endregion
}
