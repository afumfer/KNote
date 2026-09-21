using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace KNote.Model;

// Root of KNoteState.config: what the application remembers by itself between sessions (last active
// repository, window positions, list layout, alarm rows...). It holds no secrets and is rewritten often;
// what the user configures lives in AppUserSettings / KNoteData.config.
[Serializable]
public class AppUserState
{
    public const int CurrentSchemaVersion = 2;

    [XmlAttribute("schemaVersion")]
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public SessionState Session { get; set; } = new();

    public ManagementWindowState ManagementWindow { get; set; } = new();

    public AppInfoAlarmsWindowState AppInfoAlarmsWindow { get; set; } = new();
}

[Serializable]
public class SessionState
{
    public DateTime LastDateTimeStart { get; set; }

    public int RunCounter { get; set; }

    // Repository/folder the user had active when the application was last closed, so KNoteManagmentCtrl
    // can reactivate it on the next startup instead of showing no selection.
    public string LastActiveRepositoryAlias { get; set; }

    public Guid? LastActiveFolderId { get; set; }

    // Alias of the AI provider/model the user picked last (shared by KNoteAIAssistantCtrl and
    // KntServerCOMCtrl). When empty, or no longer among the configured providers, the first one is used.
    public string LastAiProviderAlias { get; set; }

    // Set automatically when a connection attempt to the chat hub fails, so the app stops retrying (and
    // freezing) on every startup until the user fixes the URL or retests it from Options.
    public bool ChatHubAutoConnectDisabled { get; set; }
}

[Serializable]
public class WindowBoundsState
{
    // 0 means "never moved/resized by the user yet", so the window centers itself instead.
    public int X { get; set; }

    public int Y { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }
}

[Serializable]
public class ManagementWindowState
{
    public WindowBoundsState Bounds { get; set; } = new();

    public NotesListState NotesList { get; set; } = new();

    public PanelsState Panels { get; set; } = new();
}

[Serializable]
public class NotesListState
{
    public int SortColumn { get; set; }

    public bool SortAscending { get; set; }

    public bool CompactView { get; set; }

    // Column widths the user gave to the notes list, as "ColumnName=width;ColumnName=width" (keyed by
    // name, not position, so entries of currently hidden columns survive). XmlSerializer can't handle a
    // Dictionary, hence a plain string.
    public string ColumnWidths { get; set; }

    public bool ShowFilter { get; set; }
}

// Management window's View menu toggles, so panel visibility survives a restart exactly as the user left
// it. Shown by default: a config without these values must not hide a panel after an upgrade.
[Serializable]
public class PanelsState
{
    public bool FoldersExplorer { get; set; } = true;

    public bool Header { get; set; } = true;

    public bool Toolbar { get; set; } = true;

    public bool MainMenu { get; set; } = true;

    public bool VerticalNotesPanel { get; set; }
}

[Serializable]
public class AppInfoAlarmsWindowState
{
    public WindowBoundsState Bounds { get; set; } = new();

    // Rows shown in the "Application info" alarms panel; they stay until the user explicitly removes them.
    public List<AppInfoAlarmRowConfig> Rows { get; set; } = new();
}
