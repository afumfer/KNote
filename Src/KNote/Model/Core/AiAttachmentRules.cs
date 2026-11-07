using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// What the attachments of the assistant have in common in both apps (KNote Atlas, D12): which ones are images, their limits,
/// their names and what the model is told of them.
/// </summary>
public static class AiAttachmentRules
{
    public const int MaxPerMessage = 5;

    private const string DefaultContentType = "application/octet-stream";
    private const int MaxFileNameLength = 120;

    public static bool IsImage(string contentType) => ChatRoomImageDto.SupportedContentTypes.Contains(contentType);

    /// <summary>By its extension: the only thing a file dropped in the desktop app says.</summary>
    public static string ContentTypeOf(string fileName) => ChatRoomImageDto.ContentTypeOf(fileName) ?? DefaultContentType;

    /// <summary>Why a file cannot be attached, or null.</summary>
    public static string SizeError(string fileName, long length) => length == 0
        ? $"{fileName} is empty."
        : length > KntConst.MaxLenResourceFile
            ? $"{fileName} is too big: {KntConst.MaxLenResourceFile / 1_000_000.0:0.#} MB at most."
            : null;

    /// <summary>The name without folders, with only letters, digits, spaces, '.', '_' and '-': anything else could break a file name.</summary>
    public static string SafeFileName(string fileName, string defaultName = "file")
    {
        var name = new string(Path.GetFileName((fileName ?? "").Replace('\\', '/'))
            .Select(c => char.IsLetterOrDigit(c) || c is ' ' or '.' or '_' or '-' ? c : '_')
            .ToArray()).Trim();
        if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(name)))
            name = defaultName + Path.GetExtension(name);
        return name.Length <= MaxFileNameLength ? name : name[^MaxFileNameLength..];
    }

    /// <summary>
    /// The text the model receives for a message with attachments. seen: the ones it receives with this message; without it, a
    /// message of the history, where it no longer has any.
    /// </summary>
    public static string PromptWithAttachments(string prompt, IReadOnlyCollection<AiChatAttachmentDto> attachments, IReadOnlyCollection<Guid> seen = null)
    {
        if (attachments == null || attachments.Count == 0)
            return prompt;

        static string Names(IEnumerable<AiChatAttachmentDto> files) => string.Join(", ", files.Select(a => a.FileName));

        if (seen == null)
            return $"{prompt}\n\n[Attached to this message: {Names(attachments)}]";

        var notes = new List<string>();
        var shown = attachments.Where(a => seen.Contains(a.ResourceId)).ToList();
        var others = attachments.Where(a => !seen.Contains(a.ResourceId)).ToList();
        if (shown.Count > 0)
            notes.Add($"[Images attached, which you can see: {Names(shown)}]");
        if (others.Count > 0)
            notes.Add($"[Files attached, whose content you cannot see: {Names(others)}]");
        return $"{prompt}\n\n{string.Join("\n", notes)}";
    }
}
