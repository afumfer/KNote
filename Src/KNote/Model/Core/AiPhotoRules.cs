using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// The rules of describing the photos of a note with the AI (KNote Atlas, stage 4, D13), common to the Server, the Web and
/// ClientWin: which resources are photos, what «Describe photos» does with each one, the request the model receives with the
/// context of the note, and how its answer is cleaned before it is saved.
/// </summary>
public static class AiPhotoRules
{
    public const int MaxNoteTextLength = 1500;
    public const int MaxPersonDescriptions = 5;
    public const int MaxPersonDescriptionLength = 300;
    public const int MaxDescriptionLength = 600;

    // A photo whose file is in another installation (its folder of resources is not here): not an error of a run, it is
    // described where the file is.
    public const string FileNotAvailableMessage = "The file of the photo is not available in this installation.";

    /// <summary>Whether the resource is a photo the AI can describe (by its type or, without one, by its name).</summary>
    public static bool IsPhoto(string fileType, string name) =>
        ContentType(fileType, name) != null;

    /// <summary>The content type of a photo (png, jpeg, gif, webp), or null if it is not one.</summary>
    public static string ContentType(string fileType, string name)
    {
        var type = fileType?.Trim().ToLowerInvariant();
        if (type != null && ChatRoomImageDto.SupportedContentTypes.Contains(type))
            return type;
        return ChatRoomImageDto.ContentTypeOf(ResourceInfoDto.FileNameOf(name ?? ""));
    }

    public static bool IsPhoto(ResourceInfoDto resource) => IsPhoto(resource.FileType, resource.Name);

    /// <summary>Whether «Describe photo» is offered for a resource: a photo whose description is not locked.</summary>
    public static bool CanDescribe(ResourceInfoDto resource) => IsPhoto(resource) && !resource.DescriptionByUser;

    /// <summary>A photo whose description a person wrote and didn't lock: describing it would replace that text.</summary>
    public static bool HasPersonDescription(ResourceInfoDto resource) =>
        IsPhoto(resource) && ResourceDescriptionRules.IsAtRisk(resource.Description, resource.DescriptionByUser, resource.DescriptionModel, resource.Name);

    /// <summary>
    /// What «Describe photos» does with a resource of the note: the photos without a description of their own are described;
    /// the ones a person described, as chosen; the locked ones, the ones the AI already described and the other files, nothing.
    /// </summary>
    public static EnumPhotoAction PlanForNote(ResourceInfoDto resource, EnumPersonDescriptions choice)
    {
        if (!IsPhoto(resource) || resource.DescriptionByUser || !string.IsNullOrEmpty(resource.DescriptionModel))
            return EnumPhotoAction.Skip;
        if (!HasPersonDescription(resource))
            return EnumPhotoAction.Describe;
        return choice switch
        {
            EnumPersonDescriptions.Replace => EnumPhotoAction.Describe,
            EnumPersonDescriptions.Lock => EnumPhotoAction.Lock,
            _ => EnumPhotoAction.Skip
        };
    }

    /// <summary>
    /// What a person wrote of this photo, as the model receives it (null: nothing): a person can say who is in it, and the
    /// model may then name them (in 4.2, with only the note, Ollama and Anthropic didn't, following the rule of the names).
    /// </summary>
    public static string PhotoDescription(ResourceInfoDto resource) =>
        HasPersonDescription(resource) ? Cut(OneLine(resource.Description), MaxPersonDescriptionLength) : null;

    /// <summary>The beginning of the text of the note the model receives: its lines joined, cut at a word.</summary>
    public static string NoteText(string text) => Cut(Regex.Replace((text ?? "").Replace("\r\n", "\n"), @"\n{3,}", "\n\n").Trim(), MaxNoteTextLength);

    /// <summary>The descriptions people wrote of the other photos of the note that the model receives.</summary>
    public static List<string> PersonDescriptions(IEnumerable<ResourceInfoDto> otherResources) =>
        (otherResources ?? Enumerable.Empty<ResourceInfoDto>())
            .Where(r => r.DescriptionByUser && string.IsNullOrEmpty(r.DescriptionModel)
                && ResourceDescriptionRules.HasOwnDescription(r.Description, r.Name))
            .OrderBy(r => r.Order).ThenBy(r => r.Name)
            .Take(MaxPersonDescriptions)
            .Select(r => Cut(OneLine(r.Description), MaxPersonDescriptionLength))
            .ToList();

    /// <summary>
    /// The request of the model with the context of the note. Without a topic or a text there is no note, and it is not
    /// mentioned (in 4.0 the model spoke of a note it hadn't got). The language is the note's: its text or, without one, its topic.
    /// </summary>
    public static string Request(string noteTopic, string noteText, IReadOnlyList<string> personDescriptions, string photoDescription = null)
    {
        var request = new StringBuilder();
        var hasNote = !string.IsNullOrWhiteSpace(noteTopic) || !string.IsNullOrWhiteSpace(noteText);
        if (hasNote)
        {
            request.Append("The note this photo belongs to:\n---\n");
            if (!string.IsNullOrWhiteSpace(noteTopic))
                request.Append($"Topic: {noteTopic.Trim()}\n");
            if (!string.IsNullOrWhiteSpace(noteText))
                request.Append($"\n{noteText.Trim()}\n");
            request.Append("---\n\n");
        }

        if (personDescriptions?.Count > 0)
        {
            request.Append("Descriptions that people wrote of other photos of this note:\n");
            foreach (var description in personDescriptions)
                request.Append($"- {description}\n");
            request.Append('\n');
        }

        if (!string.IsNullOrWhiteSpace(photoDescription))
            request.Append($"What a person wrote of this photo (it is true: keep its names and facts):\n{photoDescription.Trim()}\n\n");

        // The last line is what a small model follows best (measured in 4.2 and 4.3 with qwen3.5:9b): without "without mentioning
        // the note" it wrote «The note says…»; with it alone, it left out the names a person wrote of the photo.
        var hasPhotoDescription = !string.IsNullOrWhiteSpace(photoDescription);
        request.Append(hasPhotoDescription ? "Describe this photo with the names and facts that the person wrote of it" : "Describe this photo");
        if (hasNote)
            request.Append(", without mentioning the note. Write in the language of the note (of its text or, if it has none, of its topic).");
        else
            request.Append(hasPhotoDescription ? ". Write in the language of what the person wrote of it." : ". Write in English.");
        return request.ToString();
    }

    /// <summary>
    /// The answer of the model as it is saved: one paragraph without labels, Markdown marks or the quotes around it, cut at a
    /// sentence (or a word, with "…") to MaxDescriptionLength. Empty if it said nothing.
    /// </summary>
    public static string CleanDescription(string answer)
    {
        var text = OneLine(answer);
        text = Regex.Replace(text, @"^\W*(description|descripción|descripcion)\W*:\W*", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"(\*\*|__|^#+\s*)", "");
        text = text.Trim().Trim('"', '“', '”', '«', '»', '\'').Trim();
        if (text.Length <= MaxDescriptionLength)
            return text;

        var cut = text.Substring(0, MaxDescriptionLength);
        var end = Regex.Matches(cut, @"[.!?…](?=\s)").LastOrDefault();
        if (end != null && end.Index > MaxDescriptionLength / 2)
            return cut.Substring(0, end.Index + 1);
        return Cut(text, MaxDescriptionLength);
    }

    private static string OneLine(string text) => Regex.Replace(text ?? "", @"\s+", " ").Trim();

    private static string Cut(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;
        var cut = text.Substring(0, maxLength);
        var space = cut.LastIndexOfAny(new[] { ' ', '\n' });
        if (space > maxLength / 2)
            cut = cut.Substring(0, space);
        return cut.TrimEnd(' ', ',', ';', ':', '\n') + "…";
    }
}
