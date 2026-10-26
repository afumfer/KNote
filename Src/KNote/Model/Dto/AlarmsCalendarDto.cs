using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>
/// What the alarms calendar of the web asks for: the alarms (KMessages of a note, with a date) that fire in the range
/// [Start, End) shown by the calendar.
/// </summary>
public class AlarmsCalendarFilterDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    // The alarms of this user; null, of every user (also those without a user).
    public Guid? UserId { get; set; }

    // Empty: every periodicity / every way of notifying.
    public List<EnumAlarmType> AlarmTypes { get; set; } = new();
    public List<EnumNotificationType> NotificationTypes { get; set; } = new();

    // Deactivated alarms are shown only at their AlarmDateTime (their periodicity has ended).
    public bool IncludeDeactivated { get; set; }

    // Week and month views: the alarms of every few minutes as one occurrence per day (its first one, with the number
    // of them in Count) instead of hundreds of them.
    public bool GroupFrequentByDay { get; set; }
}

/// <summary>
/// An alarm of the calendar, with the note it belongs to. Its AlarmDateTime is the next activation; its occurrences
/// are in AlarmsCalendarDto.Occurrences.
/// </summary>
public class AlarmCalendarItemDto : KMessageDto
{
    public int NoteNumber { get; set; }
    public string NoteTopic { get; set; }
    public DateTime NoteCreationDateTime { get; set; }
}

/// <summary>A date on which an alarm fires (Count > 1: the first of the Count times it fires that day).</summary>
public class AlarmOccurrenceDto
{
    public Guid KMessageId { get; set; }
    public DateTime DateTime { get; set; }
    public int Count { get; set; } = 1;
}

public class AlarmsCalendarDto
{
    public List<AlarmCalendarItemDto> Alarms { get; set; } = new();
    public List<AlarmOccurrenceDto> Occurrences { get; set; } = new();

    // There were more occurrences than the calendar receives (KntConst.MaxAlarmsCalendarOccurrences): the last ones are
    // missing.
    public bool Truncated { get; set; }
}
