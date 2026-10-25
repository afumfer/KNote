using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.Shared;

/// <summary>
/// What the alarms of a note (KMessageDto) are called in the web app: their periodicity and how they are notified, with a
/// Material Symbols icon for each notification. The values are ClientWin's (EnumAlarmType, EnumNotificationType); only
/// their texts are the web app's.
/// </summary>
public static class KntAlarmTexts
{
    public sealed record Option<T>(T Value, string Text);

    public static readonly List<Option<EnumAlarmType>> Periodicities = new()
    {
        new(EnumAlarmType.Standard, "Once"),
        new(EnumAlarmType.Daily, "Every day"),
        new(EnumAlarmType.Weekly, "Every week"),
        new(EnumAlarmType.Monthly, "Every month"),
        new(EnumAlarmType.Annual, "Every year"),
        new(EnumAlarmType.InMinutes, "Every few minutes")
    };

    public static readonly List<Option<EnumNotificationType>> Notifications = new()
    {
        new(EnumNotificationType.AppInfo, "Application info"),
        new(EnumNotificationType.Email, "Email"),
        new(EnumNotificationType.PostIt, "Post-it (desktop app)"),
        new(EnumNotificationType.ExecuteKntScript, "Run KntScript (desktop app)")
    };

    // Which ones each app notifies (the help of the notification's field).
    public const string NotificationsHelp =
        "The web app notifies Application info alarms (the bell of the header). Email alarms are sent by the server, when it has them on, or by the desktop app. Post-it and KntScript alarms are only processed by the desktop app.";

    // "Once", "Every week", "Every 15 minutes".
    public static string Periodicity(KMessageDto alarm) =>
        alarm.AlarmType == EnumAlarmType.InMinutes && alarm.AlarmMinutes > 0
            ? $"Every {alarm.AlarmMinutes} minutes"
            : Periodicities.FirstOrDefault(p => p.Value == alarm.AlarmType)?.Text ?? alarm.AlarmType.ToString();

    public static string Notification(EnumNotificationType notification) =>
        Notifications.FirstOrDefault(n => n.Value == notification)?.Text ?? notification.ToString();

    public static string NotificationIcon(EnumNotificationType notification) => notification switch
    {
        EnumNotificationType.AppInfo => "notifications",
        EnumNotificationType.Email => "mail",
        EnumNotificationType.PostIt => "sticky_note_2",
        EnumNotificationType.ExecuteKntScript => "terminal",
        _ => "alarm"
    };
}
