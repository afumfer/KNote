using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

/// <summary>
/// The context KNote Atlas gives the AI (stage 1, chapters 19–21): the repository's profile, its glossary and the
/// instructions of each task, which are notes of the folder «KNote Atlas» (see KntAtlasNotes), and what the AI receives
/// for a task, put together in layers (AiPromptComposer). Their windows are for any user; creating the notes, for
/// ProjectManager (who also saves them: a rule of the note commands).
/// </summary>
public interface IKntAtlasContextService
{
    /// <summary>The window of the profile: its note, its length and its notices. Any user.</summary>
    Task<Result<AtlasProfileInfoDto>> GetProfileInfoAsync();

    /// <summary>The profile: the one there is or a new one with its template. ProjectManager.</summary>
    Task<Result<NoteDto>> CreateProfileAsync();

    /// <summary>
    /// Adds to the profile (created if missing) the folders of the first two levels and the note types with notes that its
    /// sections «Folders» and «Note types» do not name yet, without the system ones; nothing written is removed. ProjectManager.
    /// </summary>
    Task<Result<NoteDto>> FillProfileFoldersAndTypesAsync();

    /// <summary>The window of the glossary: its note, its terms and the problems of its rows. Any user.</summary>
    Task<Result<AtlasGlossaryInfoDto>> GetGlossaryInfoAsync();

    /// <summary>The terms of the glossary that appear in the texts, in the order they first appear in them. Any user.</summary>
    Task<Result<List<GlossaryTermDto>>> FindGlossaryTermsAsync(List<string> texts);

    /// <summary>The glossary: the one there is or a new one with its empty table. ProjectManager.</summary>
    Task<Result<NoteDto>> CreateGlossaryAsync();

    /// <summary>The window «Task instructions»: each task with its state and its note. Any user.</summary>
    Task<Result<List<AtlasTaskPromptStatusDto>>> GetTaskPromptsAsync();

    /// <summary>«Customize»: the note of instructions of the task (its active one, its oldest one, or a new one). ProjectManager.</summary>
    Task<Result<NoteDto>> CustomizeTaskAsync(string taskKey);

    /// <summary>What the AI receives for a task in this repository, for this user and conversation. Staff.</summary>
    Task<Result<AiComposedPromptDto>> ComposeAsync(AiComposeRequestDto request);
}
