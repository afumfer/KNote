using System;
using System.Xml.Serialization;

namespace KNote.Model;

// A single row of the "Application info" alarms panel (ClientWin/Views/AppInfoAlarmsForm). Stays in
// this same object for both roles - the persisted identifier and the in-memory, display-ready row -
// rather than two parallel types: only KMessageId/RepositoryAlias/NotifiedAt (the fields with no
// equivalent already stored elsewhere) are actually written to AppConfig ([XmlIgnore] on the rest).
// NoteId/NoteTopic/Comment/UserFullName are looked up from the database (via RepositoryAlias +
// KMessageId) once at load time and filled in here purely for display - see
// AppInfoAlarmsCtrl.LoadPersistedRows - so they can never go stale relative to the note/user they
// describe. The row stays in the list (in AppConfig.AppInfoAlarmsRows) until the user explicitly
// removes it ("Remove from list" in the panel's context menu).
[Serializable]
public class AppInfoAlarmRowConfig
{
    public Guid KMessageId { get; set; }
    public string RepositoryAlias { get; set; }
    public DateTime NotifiedAt { get; set; }

    [XmlIgnore]
    public Guid NoteId { get; set; }
    [XmlIgnore]
    public string NoteTopic { get; set; }
    [XmlIgnore]
    public string Comment { get; set; }
    [XmlIgnore]
    public string UserFullName { get; set; }
}
