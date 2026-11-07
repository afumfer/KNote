using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>Risk of an action of the assistant (chapter 25). Deleting (level D) does not exist.</summary>
public enum EnumAiActionLevel
{
    // Creates something new: done without asking.
    Create,
    // Changes what exists: asks first.
    Change,
    // Structure, what goes out of KNote and what loses text: asks first, also in a note of the conversation.
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
    public const string ReplaceNoteText = "replace_note_text";
    public const string MoveNote = "move_note";
    public const string SetTags = "set_tags";
    public const string AddTask = "add_task";
    public const string AddAlarm = "add_alarm";
    public const string AddEmailAlarm = "add_email_alarm";
    public const string SetAttribute = "set_attribute";
    public const string RelateNotes = "relate_notes";
    public const string CreateFolder = "create_folder";

    // create_task is not one of them: it is the assistant's old tool, recorded when it is done. It runs on its own.
    public const string CreateTask = "create_task";

    // Not one of them either: it only shows a note, so there is nothing to confirm or to record.
    public const string OpenNoteEditor = "open_note_editor";

    // The tools of the actions, which are offered from this role on.
    public static IReadOnlyList<string> Kinds { get; } = new[]
    {
        CreateNote, AppendToNote, ReplaceNoteText, MoveNote, SetTags, AddTask, AddAlarm, AddEmailAlarm, SetAttribute, RelateNotes, CreateFolder
    };

    public const EnumRoles MinRole = EnumRoles.Staff;

    // Of the new text of a replacement: the card has to let the user read what is accepted, within reason.
    private const int MaxCardNewText = 4000;

    // The turn the app sends after a card with something accepted: the model sees in the history what was done.
    public const string ContinuePrompt = "[The user has answered your proposal, and what was accepted is already done: do not do " +
        "it again. Go on with what was asked, if anything is left; if nothing is, say so in one line.]";

    public static EnumAiActionLevel LevelOf(string kind) => kind switch
    {
        CreateNote => EnumAiActionLevel.Create,
        CreateFolder or AddEmailAlarm or ReplaceNoteText => EnumAiActionLevel.Structure,
        _ => EnumAiActionLevel.Change
    };

    public static bool NeedsConfirmation(string kind) => LevelOf(kind) != EnumAiActionLevel.Create;

    /// <summary>
    /// The role a kind needs. The tool is offered from MinRole on, so that the model can say why it cannot do it: without the
    /// tool, a small model may say that it did.
    /// </summary>
    public static EnumRoles MinRoleOf(string kind) => kind == CreateFolder ? EnumRoles.ProjectManager : MinRole;

    /// <summary>Whether it waits for the card. Completing a note of this conversation is creating, but what goes out of KNote always asks.</summary>
    public static bool NeedsCard(string kind, bool approved, bool onNewNote) =>
        NeedsConfirmation(kind) && !approved && !(onNewNote && LevelOf(kind) == EnumAiActionLevel.Change);

    /// <summary>
    /// The same action again. Small models repeat a call, in the same turn or in the one that goes on after the card, and with
    /// the kind approved it would add it twice without asking. Of the earlier turns only what adds something each time counts:
    /// a tag can be put back later, and an attribute or a relation that is there changes nothing.
    /// </summary>
    public static bool IsRepeated(AiActionDto action, IEnumerable<AiActionDto> turn, IEnumerable<AiActionDto> earlier) =>
        turn.Any(a => a.Status != EnumAiActionStatus.Failed && IsSame(a, action))
        || (AddsEachTime(action.Kind) && earlier.Any(a => a.Status == EnumAiActionStatus.Done && IsSame(a, action)));

    // A created note changes its text when it is done: its title says it is the same.
    private static bool IsSame(AiActionDto a, AiActionDto b) => a.Kind == b.Kind
        && (a.Kind == CreateNote ? a.Topic != null && a.Topic == b.Topic : a.Text == b.Text);

    private static bool AddsEachTime(string kind) => kind is CreateNote or AppendToNote or AddTask or AddAlarm or AddEmailAlarm or CreateFolder;

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
        ReplaceNoteText => "replacing the text of notes",
        MoveNote => "moving notes",
        AddTask => "tasks",
        AddAlarm => "alarms",
        AddEmailAlarm => "email alarms",
        SetAttribute => "attributes",
        RelateNotes => "note relations",
        CreateFolder => "new folders",
        _ => "this kind of change"
    };

    /// <summary>
    /// What the card says of an action. A replacement adds the new text and the warning here and not in Text, which the
    /// conversation keeps and sends to the model in every turn.
    /// </summary>
    public static string CardText(AiActionDto action)
    {
        if (action.Kind != ReplaceNoteText)
            return action.Text;

        var content = action.Content ?? "";
        var shown = content.Length > MaxCardNewText ? $"{content[..MaxCardNewText]}… ({content.Length - MaxCardNewText} more characters)" : content;
        return $"{action.Text}. **What it has now is lost, and this cannot be undone.** The new text:\n\n{shown}";
    }

    /// <summary>A mark of the text of a note, to know when a replacement runs that it is still the one the card was shown for.</summary>
    public static string TextStamp(string text) => NoteChunkBuilder.Hash((text ?? "").Trim());

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

    /// <summary>A date for the card and for the model: with its day of the week, to see at once if it is the one that was meant.</summary>
    public static string DateText(DateTime date, bool withTime = true) =>
        date.ToString(withTime ? "dddd, yyyy-MM-dd HH:mm" : "dddd, yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>When an alarm goes off: the minutes from now, or a date with its time, which cannot be in the past.</summary>
    public static (DateTime? Date, string Error) AlarmDate(string when, int? minutesFromNow, DateTime now)
    {
        if (minutesFromNow > 0)
            return (new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind).AddMinutes(minutesFromNow.Value), null);
        if (string.IsNullOrWhiteSpace(when))
            return (null, "Error: give when (yyyy-MM-dd HH:mm) or minutesFromNow.");
        if (!DateTime.TryParse(when, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return (null, $"Error: '{when}' is not a date. Use yyyy-MM-dd HH:mm.");
        if (when.Trim().Length <= "yyyy-MM-dd".Length)
            return (null, "Error: the time of the alarm is missing: ask the user for it. Use yyyy-MM-dd HH:mm.");
        return date > now ? (date, null) : (null, $"Error: {DateText(date)} has passed, and now it is {DateText(now)}.");
    }

    /// <summary>How an alarm repeats: none, daily, weekly, monthly or yearly. Every N minutes is left to the alarm editors.</summary>
    public static (EnumAlarmType? Type, string Error) AlarmRepeat(string repeat) => (repeat ?? "").Trim().ToLowerInvariant() switch
    {
        "" or "none" or "once" => (EnumAlarmType.Standard, null),
        "daily" => (EnumAlarmType.Daily, null),
        "weekly" => (EnumAlarmType.Weekly, null),
        "monthly" => (EnumAlarmType.Monthly, null),
        "yearly" or "annual" => (EnumAlarmType.Annual, null),
        _ => (null, $"Error: '{repeat}' is not a repetition. Use none, daily, weekly, monthly or yearly.")
    };

    /// <summary>For the card: empty for an alarm that goes off once.</summary>
    public static string AlarmRepeatText(EnumAlarmType type) => type switch
    {
        EnumAlarmType.Daily => " (every day)",
        EnumAlarmType.Weekly => " (every week)",
        EnumAlarmType.Monthly => " (every month)",
        EnumAlarmType.Annual => " (every year)",
        _ => ""
    };

    /// <summary>
    /// The value of an attribute as the note keeps it (a text that NoteKAttributeDto validates with the culture of the app), from
    /// what the model writes. A value of a list is looked for in values: the one written in another way is corrected. Tags are
    /// several of them, separated by a comma.
    /// </summary>
    public static (string Value, string Error) AttributeValue(EnumKAttributeDataType type, string value, IReadOnlyList<string> values = null)
    {
        var text = (value ?? "").Trim();
        switch (type)
        {
            case EnumKAttributeDataType.Int:
                return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                    ? (number.ToString(CultureInfo.CurrentCulture), null) : (null, $"Error: '{value}' is not a whole number.");
            case EnumKAttributeDataType.Double:
                return double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var real)
                    ? (real.ToString(CultureInfo.CurrentCulture), null) : (null, $"Error: '{value}' is not a number.");
            case EnumKAttributeDataType.DateTime:
                return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                    ? (date.ToString(CultureInfo.CurrentCulture), null) : (null, $"Error: '{value}' is not a date. Use yyyy-MM-dd HH:mm.");
            case EnumKAttributeDataType.Bool:
                return GlossaryTable.Normalize(text) switch
                {
                    "true" or "yes" or "si" => (bool.TrueString, null),
                    "false" or "no" => (bool.FalseString, null),
                    _ => (null, $"Error: '{value}' is not yes or no.")
                };
            case EnumKAttributeDataType.TabulatedValue:
                return ListValue(text, values);
            case EnumKAttributeDataType.TagsValue:
                var tags = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(t => ListValue(t, values)).ToList();
                var wrong = tags.FirstOrDefault(t => t.Error != null).Error;
                return wrong != null ? (null, wrong) : (string.Join(", ", tags.Select(t => t.Value)), null);
            default:
                return (text, null);
        }
    }

    private static (string Value, string Error) ListValue(string text, IReadOnlyList<string> values)
    {
        var list = values ?? Array.Empty<string>();
        var wanted = GlossaryTable.Normalize(text);
        var found = list.FirstOrDefault(v => GlossaryTable.Normalize(v) == wanted);
        return found != null ? (found, null) : (null, $"Error: '{text}' is not one of its values: {string.Join(", ", list)}.");
    }
}
