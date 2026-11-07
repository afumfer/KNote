using System;
using System.Text.Json.Serialization;

namespace KNote.Model.Dto;

/// <summary>
/// A file attached to a message of the AI assistant (KNote Atlas, D12): a resource of the note of the session. The conversation
/// only keeps this; the content is asked for when it is needed, by whoever may read the session.
/// </summary>
public class AiChatAttachmentDto
{
    public Guid ResourceId { get; set; }

    public string FileName { get; set; }

    public string ContentType { get; set; }

    // The note it was saved in: a new one, if the conversation had none yet.
    public Guid SessionNoteId { get; set; }

    [JsonIgnore]
    public bool IsImage => AiAttachmentRules.IsImage(ContentType);
}

/// <summary>A file to attach. Provider, Model and Topic are for the note of the session, when it does not exist yet.</summary>
public class AiChatAttachmentUploadDto
{
    public Guid SessionNoteId { get; set; }

    public string Provider { get; set; }

    public string Model { get; set; }

    public string Topic { get; set; }

    public string FileName { get; set; }

    public string ContentType { get; set; }

    public string ContentBase64 { get; set; }
}

/// <summary>The content of an attachment: the file, or a reduced image to show it.</summary>
public class AiChatAttachmentContentDto
{
    public string FileName { get; set; }

    public string ContentType { get; set; }

    public string ContentBase64 { get; set; }
}

/// <summary>What the model of a provider can do, for what the apps offer.</summary>
public class AiModelCapabilitiesDto
{
    public bool SupportsImages { get; set; }
}
