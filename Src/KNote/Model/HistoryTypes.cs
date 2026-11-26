using System;

namespace KNote.Model;

// The history of the notes and of the structure of a repository (KLogs): who changed what, when and from where, and how each
// note was before (its versions). See HistoryRules and NoteChangeDescriber.

/// <summary>Where a change comes from (H11). The host sets its own (IKntService.HistoryOrigin); a scope changes it (KntHistoryOrigin).</summary>
public enum EnumHistoryOrigin
{
    /// <summary>What KNote does by itself, also the background work a user asked for (summaries, translations).</summary>
    KNote = 0,
    Desktop = 1,
    Web = 2,
    /// <summary>The AI assistant, with the user's permission.</summary>
    Assistant = 3,
    Script = 4,
    Broker = 5
}

/// <summary>What happened to the entity (KLogs.ActionType, by its name).</summary>
public enum EnumHistoryAction
{
    Created,
    Updated,
    Deleted,
    Restored,
    /// <summary>A deleted note removed for good from the deleted notes (H27): its versions were emptied.</summary>
    Removed
}

/// <summary>The parts of an entity a change touched (KLogs.Fields).</summary>
[Flags]
public enum EnumHistoryFields
{
    None = 0,
    Topic = 1,
    Description = 2,
    ContentType = 4,
    Tags = 8,
    NoteType = 16,
    Folder = 32,
    Attributes = 64,
    Priority = 128,
    Script = 256,
    Tasks = 512,
    Resources = 1024,
    Relations = 2048,
    Alarms = 4096,

    /// <summary>The note's own data: what decides its modification date and what its versions keep (H6).</summary>
    NoteData = Topic | Description | ContentType | Tags | NoteType | Folder | Attributes | Priority | Script
}

/// <summary>The entities of the history (KLogs.EntityName). A change of a part of a note is a row of its note (H3).</summary>
public static class HistoryEntities
{
    public const string Note = "Note";
    public const string Folder = "Folder";
    public const string NoteType = "NoteType";
    public const string KAttribute = "KAttribute";
    public const string TraceNoteType = "TraceNoteType";
    public const string User = "User";
    public const string SystemValue = "SystemValue";
}
