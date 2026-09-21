using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace KNote.Model;

// FROZEN. Read-only mirror of the flat KNoteData.config written by every version before the
// restructuring (root element <AppConfig>, no schemaVersion). Its only job is to deserialize such files
// so AppConfigMigrator can convert them into AppUserSettings + AppUserState; never save it, never add to it.
// Element names, misspellings included (Respository, Managment, Ascendig), and the defaults for elements
// missing from old files must stay exactly as the old AppConfig had them.
[Serializable]
[XmlRoot("AppConfig")]
public class AppConfigV1
{
    public DateTime LastDateTimeStart { get; set; }
    public int RunCounter { get; set; }
    public string LogFile { get; set; }
    public bool LogActivated { get; set; }
    public bool AlarmActivated { get; set; }
    public bool AutoSaveActivated { get; set; }
    public int AlarmSeconds { get; set; }
    public int AutoSaveSeconds { get; set; }

    public int ManagmentLocX { get; set; }
    public int ManagmentLocY { get; set; }
    public int ManagmentWidth { get; set; }
    public int ManagmentHeight { get; set; }

    public int ColOrderNotes { get; set; }
    public bool AscendigOrderNotes { get; set; }
    public bool CompactViewNoteslist { get; set; }
    public string NotesListColumnWidths { get; set; }

    // Absent in files saved before these toggles existed: the panel is then shown (old default).
    public bool? ShowFoldersExplorerTab { get; set; }
    public bool VerticalPanelForNotes { get; set; }
    public bool? ShowHeaderPanel { get; set; }
    public bool? ShowToolbar { get; set; }
    public bool? ShowMainMenu { get; set; }
    public bool ShowListFilter { get; set; }

    public string ChatHubUrl { get; set; }
    public bool ChatHubAutoConnectDisabled { get; set; }
    public bool ActivateMessageBroker { get; set; }

    public List<RepositoryRef> RespositoryRefs { get; set; } = new();
    public RepositoryRef AssistantRespositoryRef { get; set; } = new();
    public string LastActiveRepositoryAlias { get; set; }
    public Guid? LastActiveFolderId { get; set; }

    public List<AiProviderRef> AiProviderRefs { get; set; } = new();
    public string LastAiProviderAlias { get; set; }

    public ServerCOMConfig ServerCOM { get; set; } = new();

    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public bool SmtpEnableSsl { get; set; } = true;
    public string SmtpFromAddress { get; set; }
    public string SmtpFromDisplayName { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }

    public int AppInfoAlarmsLocX { get; set; }
    public int AppInfoAlarmsLocY { get; set; }
    public int AppInfoAlarmsWidth { get; set; }
    public int AppInfoAlarmsHeight { get; set; }
    public List<AppInfoAlarmRowConfig> AppInfoAlarmsRows { get; set; } = new();
}
