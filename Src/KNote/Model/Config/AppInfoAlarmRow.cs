using System;
using System.Xml.Serialization;

namespace KNote.Model;

// A single row of the "Application info" alarms panel (ClientWin/Views/AppInfoAlarmsForm). Stays in
// this same object for both roles - the persisted identifier and the in-memory, display-ready row -
// rather than two parallel types: only KMessageId/RepositoryId/NotifiedAt (the fields with no
// equivalent already stored elsewhere) are actually written to the state file ([XmlIgnore] on the rest).
// NoteId/NoteTopic/Comment/UserFullName/RepositoryName are looked up from the database (via RepositoryId +
// KMessageId) once at load time and filled in here purely for display - see
// AppInfoAlarmsCtrl.LoadPersistedRows - so they can never go stale relative to the note/user they
// describe. The row stays in the list (in AppUserState.AppInfoAlarmsWindow.Rows) until the user explicitly
// removes it ("Remove from list" in the panel's context menu).
//
// Renamed from AppInfoAlarmRowConfig (it was never just persisted config - see above) without keeping
// the old XML element name: XmlSerializer names each list item after the class name, so a
// KNoteState.config written before this rename simply comes back with an empty Rows list on the first
// load after upgrading (silently ignored, not an error) instead of carrying old rows forward under the
// new name.
[Serializable]
public class AppInfoAlarmRow
{
    public Guid KMessageId { get; set; }

    // The repository of the message (RepositoryRef.RepositoryId), which never changes, unlike its name.
    public Guid RepositoryId { get; set; }

    // LEGACY(repository-alias): remove, with ShouldSerializeRepositoryAlias and
    // AppInfoAlarmRowRepositoryMigration, together with RepositoryRef.LegacyAlias.
    // Rows saved before repositories had an id only know the alias the repository had in this
    // installation's configuration. AppInfoAlarmRowRepositoryMigration gives them the RepositoryId at
    // startup; until then (the repository can't be opened) the alias is kept.
    public string RepositoryAlias { get; set; }

    public bool ShouldSerializeRepositoryAlias() => RepositoryId == Guid.Empty && !string.IsNullOrEmpty(RepositoryAlias);

    public DateTime NotifiedAt { get; set; }

    [XmlIgnore]
    public string RepositoryName { get; set; }

    [XmlIgnore]
    public Guid NoteId { get; set; }
    [XmlIgnore]
    public string NoteTopic { get; set; }
    [XmlIgnore]
    public string Comment { get; set; }
    [XmlIgnore]
    public string UserFullName { get; set; }
}
