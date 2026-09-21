using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Core;

/// <summary>
/// Pure decision logic used by AppInfoAlarmsCtrl to keep the "Application info" rows in sync with the
/// note/alarm data published by the editors (EntitySaved events). No UI, no database: the new values
/// come from the event payload. The display-only fields of the affected rows are updated in place and
/// every row that actually changed (or must leave the list) is reported back, so the caller only
/// touches the view/state for those.
/// </summary>
public static class AppInfoRowRefresh
{
    public enum ChangeKind
    {
        Updated,
        Removed
    }

    public sealed record RowChange(AppInfoAlarmRowConfig Row, ChangeKind Kind);

    /// <summary>
    /// A note was saved with its full message list (NoteEditor, the only editor that can change what the rows show). A row is removed when its message no
    /// longer exists, is no longer an AppInfo alarm, or is no longer addressed to the row repository's
    /// active user; otherwise its Topic/Comment are refreshed.
    /// </summary>
    /// <param name="activeUserIdOf">Active user id for a repository alias (null when unknown).</param>
    public static List<RowChange> ApplyNoteSaved(IEnumerable<AppInfoAlarmRowConfig> rows, NoteExtendedDto note, Func<string, Guid?> activeUserIdOf)
    {
        var changes = new List<RowChange>();

        foreach (var row in rows.Where(r => r.NoteId == note.NoteId).ToList())
        {
            var message = note.Messages?.FirstOrDefault(m => m.KMessageId == row.KMessageId);

            if (message == null
                || message.NotificationType != EnumNotificationType.AppInfo
                || message.UserId == null
                || message.UserId != activeUserIdOf(row.RepositoryAlias))
            {
                changes.Add(new RowChange(row, ChangeKind.Removed));
                continue;
            }

            var changed = row.NoteTopic != note.Topic || row.Comment != message.Comment;
            row.NoteTopic = note.Topic;
            row.Comment = message.Comment;

            if (changed)
                changes.Add(new RowChange(row, ChangeKind.Updated));
        }

        return changes;
    }

    /// <summary>
    /// A user was saved: refresh the User column of the rows addressed to them. A row only exists for
    /// messages addressed to its repository's active user, so that user is the row's recipient.
    /// </summary>
    public static List<RowChange> ApplyUserSaved(IEnumerable<AppInfoAlarmRowConfig> rows, UserDto user, Func<string, Guid?> activeUserIdOf)
    {
        var changes = new List<RowChange>();

        foreach (var row in rows.Where(r => r.UserFullName != user.FullName && activeUserIdOf(r.RepositoryAlias) == user.UserId).ToList())
        {
            row.UserFullName = user.FullName;
            changes.Add(new RowChange(row, ChangeKind.Updated));
        }

        return changes;
    }
}
