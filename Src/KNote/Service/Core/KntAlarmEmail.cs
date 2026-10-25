using System;
 using KNote.Model.Dto;

namespace KNote.Service.Core;

/// <summary>
/// The email of an Email alarm, the same one whoever sends it (ClientWin, or the Server's EmailAlarmsService): the
/// note's topic as the subject, and the alarm's comment followed by the note's description as the (plain text) body.
/// </summary>
public static class KntAlarmEmail
{
    public static (string Subject, string Body) Compose(NoteDto note, KMessageDto message) =>
        (note.Topic, $"{message.Comment}\r\n\r\n{note.Description}");

    /// <summary>
    /// The alarm's comment with why its email couldn't be sent (a permanent failure, e.g. a user without an email), so
    /// the user finds out the next time they look at the note's alarms.
    /// </summary>
    public static string FailedComment(KMessageDto message, string reason) =>
        $"[Email failed {DateTime.Now:dd/MM/yyyy HH:mm}: {reason}] {message.Comment}";
}
