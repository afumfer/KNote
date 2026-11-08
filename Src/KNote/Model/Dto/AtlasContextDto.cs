using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>A term of the repository's glossary (KNote Atlas, chapter 20). Row is its row in the note's table.</summary>
public class GlossaryTermDto
{
    public int Row { get; set; }
    public string Term { get; set; }
    public string Meaning { get; set; }
    public List<string> AlsoWrittenAs { get; set; } = new();
}

/// <summary>A note of the context of KNote Atlas (profile, glossary, task instructions), as its windows show it.</summary>
public class AtlasNoteRefDto
{
    public Guid NoteId { get; set; }
    public int NoteNumber { get; set; }
    public string Topic { get; set; }
    public DateTime ModificationDateTime { get; set; }
}

/// <summary>
/// The window of the repository's profile (chapter 19): its note (null, not created yet), its length in words (of what
/// the AI receives) and the notices (too long, several profiles).
/// </summary>
public class AtlasProfileInfoDto
{
    public AtlasNoteRefDto Note { get; set; }
    public int Words { get; set; }
    public int MaxWords { get; set; }
    public List<string> Notices { get; set; } = new();
}

/// <summary>The window of the glossary (chapter 20): its note (null, not created yet), its terms and the problems of its rows.</summary>
public class AtlasGlossaryInfoDto
{
    public AtlasNoteRefDto Note { get; set; }
    public int Terms { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Notices { get; set; } = new();
}

public enum EnumAtlasTaskPromptState
{
    // Only KNote's default instructions.
    Default,
    Added,
    Replaced
}

/// <summary>A task of the window «Task instructions» (chapter 21): its state and its note, if it has one.</summary>
public class AtlasTaskPromptStatusDto
{
    public string TaskKey { get; set; }
    public string TaskName { get; set; }
    public EnumAtlasTaskPromptState State { get; set; }
    // The active note of the task or, without one, its inactive note (State Default): «Customize» opens it.
    public AtlasNoteRefDto Note { get; set; }
    public List<string> Notices { get; set; } = new();
}

/// <summary>
/// What the AI receives for a task (ComposeAsync): InstructionsOverride replaces the task's instructions in that
/// conversation (ClientWin: a @Prompt of the catalog, or a script; D6); Texts are where the glossary terms are looked for
/// (the conversation: prompts and answers in their order, D7).
/// </summary>
public class AiComposeRequestDto
{
    public string TaskKey { get; set; } = AiTaskCatalog.AssistantKey;
    public string InstructionsOverride { get; set; }
    public List<string> Texts { get; set; } = new();

    public override string ToString() => $"Task: {TaskKey}, Override: {!string.IsNullOrWhiteSpace(InstructionsOverride)}, Texts: {Texts?.Count ?? 0}";
}

/// <summary>A layer of what the AI receives (rules, instructions, profile, glossary), with the note it comes from.</summary>
public class AiPromptLayerDto
{
    public string Name { get; set; }
    public string Text { get; set; }
    // «#123» or «KNote».
    public string Source { get; set; }
}

/// <summary>
/// The system message of a task, put together (AiPromptComposer): its text, its layers (for «What the AI receives»),
/// the glossary terms that went in, whether the profile was cut, and its size (words and an estimate of tokens).
/// </summary>
public class AiComposedPromptDto
{
    public string Text { get; set; }
    public List<AiPromptLayerDto> Layers { get; set; } = new();
    public List<GlossaryTermDto> GlossaryTerms { get; set; } = new();
    public bool ProfileCut { get; set; }
    public int Words { get; set; }
    public int ApproxTokens { get; set; }
}
