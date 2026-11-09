using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>
/// What the AI needs to describe a photo of a note (KNote Atlas, stage 4, D13): the resource with its file, and the context of
/// its note (its topic, the beginning of its text as Markdown and the descriptions people wrote of its other photos), and what a
/// person wrote of this photo, if it is not locked: describing it uses that text (who is in it, where it is) instead of losing it.
/// </summary>
public class PhotoDescriptionContextDto
{
    public ResourceDto Resource { get; set; }
    public string NoteTopic { get; set; }
    public string NoteText { get; set; }
    public List<string> PersonDescriptions { get; set; } = new();
    public string PhotoDescription { get; set; }
}

/// <summary>
/// The description the AI wrote of a resource, and what the resource was when it was read: it is written only if it is still
/// so and not locked (D12).
/// </summary>
public class AiDescriptionSaveDto
{
    public Guid ResourceId { get; set; }
    public string Description { get; set; }
    public string Model { get; set; }
    public string ExpectedDescription { get; set; }
    public string ExpectedName { get; set; }

    public override string ToString() => $"{ResourceId} ({Model})";
}

/// <summary>
/// The model that describes photos in this installation (the repository's recommended LLM if it is configured here, else the
/// default chat one): what the apps show before describing. Never its host or key. Message: why it can't, if it can't.
/// </summary>
public class ResourceDescriberDto
{
    public string Provider { get; set; }
    public string Model { get; set; }
    public bool IsPaid { get; set; }
    public bool SupportsImages { get; set; }
    public string Message { get; set; }

    public bool Available => string.IsNullOrEmpty(Message);
}

/// <summary>What «Describe photos» does with the photos of a note whose description a person wrote and didn't lock.</summary>
public enum EnumPersonDescriptions
{
    Replace,
    Lock,
    Skip
}

/// <summary>What «Describe photos» does with a resource of the note.</summary>
public enum EnumPhotoAction
{
    Skip,
    Describe,
    Lock
}
