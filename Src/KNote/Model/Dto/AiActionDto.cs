using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

public enum EnumAiActionStatus
{
    // Waiting for the user: the card of the conversation.
    Proposed,
    Done,
    Failed,
    Rejected,
    // Left without an answer when the conversation was closed: it is never done afterwards.
    NotConfirmed
}

/// <summary>
/// An action of the AI assistant on the repository (KNote Atlas, D2): what it does, in Text (Markdown, with its #n), for the
/// card and for the record of the conversation; its data, for whoever runs it (KntAiActionExecutor); and how it ended.
/// </summary>
public class AiActionDto
{
    public Guid ActionId { get; set; } = Guid.NewGuid();

    // One of AiActionRules' kinds.
    public string Kind { get; set; }

    public EnumAiActionStatus Status { get; set; } = EnumAiActionStatus.Proposed;

    public string Text { get; set; }

    // Failed: why.
    public string Result { get; set; }

    // The note it acts on, or the one it created.
    public Guid? NoteId { get; set; }
    public int? NoteNumber { get; set; }

    // Its data. Only what it needs is filled in; none of this is saved with the conversation.
    public Guid? FolderId { get; set; }
    public string Topic { get; set; }
    public string Content { get; set; }

    // A replacement: the text of the note it was proposed for (AiActionRules.TextStamp).
    public string TextStamp { get; set; }
    public List<string> AddTags { get; set; }
    public List<string> RemoveTags { get; set; }

    // The user of a task. An alarm has none here: it is always for the user who runs it.
    public Guid? UserId { get; set; }

    // When an alarm goes off, or when a task should end.
    public DateTime? Date { get; set; }
    public EnumAlarmType? AlarmType { get; set; }

    public Guid? AttributeId { get; set; }
    public string Value { get; set; }

    // The other note of a relation, and its type if it has one.
    public Guid? TargetNoteId { get; set; }
    public Guid? TraceNoteTypeId { get; set; }
}
