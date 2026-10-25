using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>
/// The answer of checking the alarms (api/alarms/check): the Application info alarms of the user that have just become
/// due (consumed: no other check gets them), and how many seconds to wait before checking again (the Server's
/// configuration).
/// </summary>
public class AlarmCheckDto
{
    public List<AlarmNoticeDto> Notices { get; set; } = new();

    public int CheckSeconds { get; set; }
}
