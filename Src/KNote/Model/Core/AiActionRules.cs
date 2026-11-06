using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>Risk of an action of the assistant (chapter 25). The destructive ones (level D) do not exist.</summary>
public enum EnumAiActionLevel
{
    // Creates something new: done without asking.
    Create,
    // Changes what exists: asks first.
    Change,
    // Structure and what goes out of KNote: asks first.
    Structure
}

/// <summary>
/// What the actions of the assistant have in common in both apps (KNote Atlas, D2, D3, D10, D13): the level of each kind, the
/// lines of the record, what the model is told of them in the next turns and the prompt of the turn that goes on after a card.
/// </summary>
public static class AiActionRules
{
    public const string CreateNote = "create_note";
    public const string AppendToNote = "append_to_note";
    public const string SetTags = "set_tags";

    // create_task is not one of them: it is the assistant's old tool, recorded when it is done. It runs on its own.
    public const string CreateTask = "create_task";

    // The tools of the actions, which are offered from this role on.
    public static IReadOnlyList<string> Kinds { get; } = new[] { CreateNote, AppendToNote, SetTags };

    public const EnumRoles MinRole = EnumRoles.Staff;

    // The turn the app sends after a card with something accepted: the model sees in the history what was done.
    public const string ContinuePrompt = "[The user has answered your proposal, and what was accepted is already done: do not do " +
        "it again. Go on with what was asked, if anything is left; if nothing is, say so in one line.]";

    public static EnumAiActionLevel LevelOf(string kind) => kind switch
    {
        CreateNote => EnumAiActionLevel.Create,
        _ => EnumAiActionLevel.Change
    };

    public static bool NeedsConfirmation(string kind) => LevelOf(kind) != EnumAiActionLevel.Create;

    /// <summary>
    /// The same action again. Small models repeat a call, in the same turn or in the one that goes on after the card, and with
    /// the kind approved it would add the text twice without asking. Of the earlier turns only added text counts: a tag can be
    /// put back later, and putting one that is there changes nothing.
    /// </summary>
    public static bool IsRepeated(AiActionDto action, IEnumerable<AiActionDto> turn, IEnumerable<AiActionDto> earlier) =>
        turn.Any(a => a.Status != EnumAiActionStatus.Failed && IsSame(a, action))
        || (action.Kind == AppendToNote && earlier.Any(a => a.Status == EnumAiActionStatus.Done && IsSame(a, action)));

    private static bool IsSame(AiActionDto a, AiActionDto b) => a.Kind == b.Kind && a.Text == b.Text;

    /// <summary>The box of the card. It approves only the kinds accepted in that card, so it says which.</summary>
    public static string DontAskAgainText(IEnumerable<AiActionDto> actions)
    {
        var kinds = actions.Select(a => a.Kind).Distinct().ToList();
        var what = kinds.Count == 1 ? KindName(kinds[0]) : "these kinds of change";
        return $"Don't ask again for {what} in this conversation";
    }

    private static string KindName(string kind) => kind switch
    {
        SetTags => "tags",
        AppendToNote => "text added to notes",
        _ => "this kind of change"
    };

    /// <summary>A line of the record of the conversation.</summary>
    public static string Line(AiActionDto action) => action.Status switch
    {
        EnumAiActionStatus.Done => $"✔ {action.Text}",
        EnumAiActionStatus.Failed => $"✖ {action.Text}: {action.Result}",
        EnumAiActionStatus.Rejected => $"✖ Rejected: {action.Text}",
        EnumAiActionStatus.NotConfirmed => $"✖ Not confirmed: {action.Text}",
        _ => action.Text
    };

    /// <summary>What the tool call of an action answers in the history of the conversation: how it ended.</summary>
    public static string Outcome(AiActionDto action) => action.Status switch
    {
        EnumAiActionStatus.Done => $"Done: {action.Text}",
        EnumAiActionStatus.Failed => $"Failed: {action.Text} ({action.Result})",
        EnumAiActionStatus.Rejected => $"Rejected by the user, not done: {action.Text}",
        EnumAiActionStatus.NotConfirmed => $"Not confirmed by the user, not done: {action.Text}",
        _ => $"Waiting for the user, not done: {action.Text}"
    };

    /// <summary>A new message without answering a card rejects what it proposed. True if there was something.</summary>
    public static bool RejectPending(IEnumerable<AiChatTurnDto> turns)
    {
        var pending = (turns ?? Enumerable.Empty<AiChatTurnDto>()).SelectMany(t => t.Actions)
            .Where(a => a.Status == EnumAiActionStatus.Proposed).ToList();
        pending.ForEach(a => a.Status = EnumAiActionStatus.Rejected);
        return pending.Count > 0;
    }

    /// <summary>The notes created in this conversation: completing them is creating, not changing what existed (D10).</summary>
    public static IEnumerable<Guid> NewNoteIds(IEnumerable<AiChatTurnDto> turns) =>
        (turns ?? Enumerable.Empty<AiChatTurnDto>()).SelectMany(t => t.Actions)
            .Where(a => a.Status == EnumAiActionStatus.Done && a.NoteId != null && a.Kind is CreateNote or CreateTask)
            .Select(a => a.NoteId.Value);

    /// <summary>The tags of a note with these added and removed, as the repositories write them: separated by spaces.</summary>
    public static string ChangeTags(string tags, IEnumerable<string> add, IEnumerable<string> remove)
    {
        var separators = new[] { ' ', ',', ';' };
        var list = (tags ?? "").Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (var tag in remove ?? Enumerable.Empty<string>())
            list.RemoveAll(t => string.Equals(t, tag?.Trim(), StringComparison.OrdinalIgnoreCase));
        foreach (var tag in (add ?? Enumerable.Empty<string>()).SelectMany(t => (t ?? "").Split(separators, StringSplitOptions.RemoveEmptyEntries)))
            if (!list.Contains(tag, StringComparer.OrdinalIgnoreCase))
                list.Add(tag);
        return string.Join(" ", list);
    }
}
