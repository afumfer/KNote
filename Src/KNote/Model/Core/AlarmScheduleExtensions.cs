using System;

namespace KNote.Model;

// Implemented by both KMessageDto (Repository.Dapper works on the DTO directly) and the EF KMessage
// entity (Repository.EntityFramework/Entities/KMessage.cs), so the alarm-rescheduling rule below
// lives in exactly one place instead of being duplicated, near-identically, once per ORM.
public interface IAlarmSchedule
{
    EnumAlarmType AlarmType { get; }
    DateTime? AlarmDateTime { get; set; }
    int? AlarmMinutes { get; }
    bool? AlarmActivated { get; set; }
}

public static class AlarmScheduleExtensions
{
    // Reschedules a fired alarm to its next future occurrence (Annual/Monthly/Weekly/Daily/InMinutes)
    // or deactivates it (Standard, or any unrecognized type) - called on every message a "get pending
    // alarms" query just found due, before persisting it back as no-longer-due. Previously duplicated
    // (with a divergent, unintentional "default: does nothing" case on the EF side) in
    // Repository.Dapper.KntNoteRepository and Repository.EntityFramework.KntNoteRepository.
    public static void ApplyAlarmControl(this IAlarmSchedule message)
    {
        var now = DateTime.Now;
        switch (message.AlarmType)
        {
            case EnumAlarmType.Standard:
                message.AlarmActivated = false;
                break;
            case EnumAlarmType.Annual:
                while (message.AlarmDateTime < now)
                    message.AlarmDateTime = ((DateTime)message.AlarmDateTime).AddYears(1);
                break;
            case EnumAlarmType.Monthly:
                while (message.AlarmDateTime < now)
                    message.AlarmDateTime = ((DateTime)message.AlarmDateTime).AddMonths(1);
                break;
            case EnumAlarmType.Weekly:
                while (message.AlarmDateTime < now)
                    message.AlarmDateTime = ((DateTime)message.AlarmDateTime).AddDays(7);
                break;
            case EnumAlarmType.Daily:
                while (message.AlarmDateTime < now)
                    message.AlarmDateTime = ((DateTime)message.AlarmDateTime).AddDays(1);
                break;
            case EnumAlarmType.InMinutes:
                while (message.AlarmDateTime < now)
                    message.AlarmDateTime = ((DateTime)message.AlarmDateTime).AddMinutes((int)message.AlarmMinutes);
                break;
            default:
                message.AlarmActivated = false;
                break;
        }
    }
}
