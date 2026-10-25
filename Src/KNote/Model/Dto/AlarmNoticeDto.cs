using System;

namespace KNote.Model.Dto;

/// <summary>
/// An Application info alarm notified to the user (the web app's alarms panel): the alarm (KMessageId), its note and
/// what is shown of them, read when asked (Notes.GetAlarmNoticesAsync), so they are never out of date. When it was
/// notified is kept by whoever keeps the notices (the browser, as ClientWin keeps its rows in the user's folder).
/// </summary>
public class AlarmNoticeDto
{
    public Guid KMessageId { get; set; }

    public Guid NoteId { get; set; }

    public int NoteNumber { get; set; }

    public string NoteTopic { get; set; }

    public string Comment { get; set; }

    public DateTime? AlarmDateTime { get; set; }
}
