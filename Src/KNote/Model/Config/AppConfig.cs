using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KNote.Model;

[Serializable]
public class AppConfig : SmartModelDtoBase
{
    #region Properties 

    private DateTime _lastDateTimeStart;
    public DateTime LastDateTimeStart
    {
        get { return _lastDateTimeStart; }
        set
        {
            if (_lastDateTimeStart != value)
            {
                _lastDateTimeStart = value;
                OnPropertyChanged("LastDateTimeStart");
            }
        }
    }

    private int _runCounter;
    public int RunCounter
    {
        get { return _runCounter; }
        set
        {
            if (_runCounter != value)
            {
                _runCounter = value;
                OnPropertyChanged("RunCounter");
            }
        }
    }

    private string _logFile;
    public string LogFile
    {
        get { return _logFile; }
        set
        {
            if (_logFile != value)
            {
                _logFile = value;
                OnPropertyChanged("LogFile");
            }
        }
    }

    private bool _logActivated;
    public bool LogActivated
    {
        get { return _logActivated; }
        set
        {
            if (_logActivated != value)
            {
                _logActivated = value;
                OnPropertyChanged("LogActivated");
            }
        }
    }

    private bool _alarmActivated;
    public bool AlarmActivated
    {
        get { return _alarmActivated; }
        set
        {
            if (_alarmActivated != value)
            {
                _alarmActivated = value;
                OnPropertyChanged("AlarmActivated");
            }
        }
    }

    private bool _autoSaveActivated;
    public bool AutoSaveActivated
    {
        get { return _autoSaveActivated; }
        set
        {
            if (_autoSaveActivated != value)
            {
                _autoSaveActivated = value;
                OnPropertyChanged("AutoSaveActivated");
            }
        }
    }

    private int _alarmSeconds;
    public int AlarmSeconds
    {
        get { return _alarmSeconds; }
        set
        {
            if (_alarmSeconds != value)
            {
                _alarmSeconds = value;
                OnPropertyChanged("AlarmSeconds");
            }
        }
    }

    private int _atoSaveSeconds;
    public int AutoSaveSeconds
    {
        get { return _atoSaveSeconds; }
        set
        {
            if (_atoSaveSeconds != value)
            {
                _atoSaveSeconds = value;
                OnPropertyChanged("AutoSaveSeconds");
            }
        }
    }

    private int _managmentLocX;
    public int ManagmentLocX
    {
        get { return _managmentLocX; }
        set
        {
            if (_managmentLocX != value)
            {
                _managmentLocX = value;
                OnPropertyChanged("ManagmentLocX");
            }
        }
    }

    private int _managmentLocY;
    public int ManagmentLocY
    {
        get { return _managmentLocY; }
        set
        {
            if (_managmentLocY != value)
            {
                _managmentLocY = value;
                OnPropertyChanged("ManagmentLocY");
            }
        }
    }

    private int _managmentWidth;
    public int ManagmentWidth
    {
        get { return _managmentWidth; }
        set
        {
            if (_managmentWidth != value)
            {
                _managmentWidth = value;
                OnPropertyChanged("ManagmentWidth");
            }
        }
    }

    private int _managmentHeight;
    public int ManagmentHeight
    {
        get { return _managmentHeight; }
        set
        {
            if (_managmentHeight != value)
            {
                _managmentHeight = value;
                OnPropertyChanged("ManagmentHeight");
            }
        }
    }

    private int _colOrderNotes;
    public int ColOrderNotes
    {
        get { return _colOrderNotes; }
        set
        {
            if (_colOrderNotes != value)
            {
                _colOrderNotes = value;
                OnPropertyChanged("ColOrderNotes");
            }
        }
    }

    private bool _ascendigOrderNotes;
    public bool AscendigOrderNotes
    {
        get { return _ascendigOrderNotes; }
        set
        {
            if (_ascendigOrderNotes != value)
            {
                _ascendigOrderNotes = value;
                OnPropertyChanged("AscendigOrderNotes");
            }
        }
    }
        
    private bool _compactViewNoteslist;
    public bool CompactViewNoteslist
    {
        get { return _compactViewNoteslist; }
        set
        {
            if (_compactViewNoteslist != value)
            {
                _compactViewNoteslist = value;
                OnPropertyChanged("CompactViewNoteslist");
            }
        }
    }

    // Column widths the user gave to the notes list embedded in the management window, as
    // "ColumnName=width;ColumnName=width" (keyed by name, not position, so entries of currently
    // hidden columns survive). XmlSerializer can't handle a Dictionary, hence a plain string.
    private string _notesListColumnWidths;
    public string NotesListColumnWidths
    {
        get { return _notesListColumnWidths; }
        set
        {
            if (_notesListColumnWidths != value)
            {
                _notesListColumnWidths = value;
                OnPropertyChanged("NotesListColumnWidths");
            }
        }
    }

    // Management window's View menu toggles, persisted so panel visibility survives a restart
    // exactly as the user left it (see KNoteManagmentForm.ApplyViewPanelSettings/menu handlers).
    // Backed by bool? rather than bool so that a config file saved before these fields existed
    // (where XML deserialization leaves them unset) falls back to the panel being shown - matching
    // the Designer's original default - instead of silently hiding it after an upgrade.
    private bool? _showFoldersExplorerTab;
    public bool ShowFoldersExplorerTab
    {
        get { return _showFoldersExplorerTab ?? true; }
        set
        {
            if (_showFoldersExplorerTab != value)
            {
                _showFoldersExplorerTab = value;
                OnPropertyChanged("ShowFoldersExplorerTab");
            }
        }
    }

    private bool _verticalPanelForNotes;
    public bool VerticalPanelForNotes
    {
        get { return _verticalPanelForNotes; }
        set
        {
            if (_verticalPanelForNotes != value)
            {
                _verticalPanelForNotes = value;
                OnPropertyChanged("VerticalPanelForNotes");
            }
        }
    }

    private bool? _showHeaderPanel;
    public bool ShowHeaderPanel
    {
        get { return _showHeaderPanel ?? true; }
        set
        {
            if (_showHeaderPanel != value)
            {
                _showHeaderPanel = value;
                OnPropertyChanged("ShowHeaderPanel");
            }
        }
    }

    private bool? _showToolbar;
    public bool ShowToolbar
    {
        get { return _showToolbar ?? true; }
        set
        {
            if (_showToolbar != value)
            {
                _showToolbar = value;
                OnPropertyChanged("ShowToolbar");
            }
        }
    }

    private bool? _showMainMenu;
    public bool ShowMainMenu
    {
        get { return _showMainMenu ?? true; }
        set
        {
            if (_showMainMenu != value)
            {
                _showMainMenu = value;
                OnPropertyChanged("ShowMainMenu");
            }
        }
    }

    private bool _showListFilter;
    public bool ShowListFilter
    {
        get { return _showListFilter; }
        set
        {
            if (_showListFilter != value)
            {
                _showListFilter = value;
                OnPropertyChanged("ShowListFilter");
            }
        }
    }

    private string _chatHubUrl;
    public string ChatHubUrl
    {
        get { return _chatHubUrl; }
        set
        {
            if (_chatHubUrl != value)
            {
                _chatHubUrl = value;
                OnPropertyChanged("ChatHubUrl");
            }
        }
    }

    // Set automatically when a connection attempt to ChatHubUrl fails, so the app stops retrying
    // (and freezing) on every startup until the user fixes the URL or retests it from Options.
    private bool _chatHubAutoConnectDisabled;
    public bool ChatHubAutoConnectDisabled
    {
        get { return _chatHubAutoConnectDisabled; }
        set
        {
            if (_chatHubAutoConnectDisabled != value)
            {
                _chatHubAutoConnectDisabled = value;
                OnPropertyChanged("ChatHubAutoConnectDisabled");
            }
        }
    }

    private bool _activateMessageBroker;
    public bool ActivateMessageBroker
    {
        get { return _activateMessageBroker; }
        set
        {
            if (_activateMessageBroker != value)
            {
                _activateMessageBroker = value;
                OnPropertyChanged("ActivateMessageBroker");
            }
        }
    }

    private List<RepositoryRef> _respositoryRef;
    public List<RepositoryRef> RespositoryRefs
    {
        get
        {
            if (_respositoryRef == null)
                _respositoryRef = new List<RepositoryRef>();
            return _respositoryRef;
        }
        set 
        {
            if(_respositoryRef != value)
            {
                _respositoryRef = value;
                OnPropertyChanged("RespositoryRefs");
            }
        }
    }

    private RepositoryRef _assistantRespositoryRef;
    public RepositoryRef AssistantRespositoryRef
    {
        get
        {
            if (_assistantRespositoryRef == null)
                _assistantRespositoryRef = new RepositoryRef();
            return _assistantRespositoryRef;
        }
        set
        {
            if (_assistantRespositoryRef != value)
            {
                _assistantRespositoryRef = value;
                OnPropertyChanged("AssistantRespositoryRef");
            }
        }
    }

    // Repository/folder the user had active when the application was last closed, so
    // KNoteManagmentCtrl can reactivate it on the next startup instead of showing no selection.
    private string _lastActiveRepositoryAlias;
    public string LastActiveRepositoryAlias
    {
        get { return _lastActiveRepositoryAlias; }
        set
        {
            if (_lastActiveRepositoryAlias != value)
            {
                _lastActiveRepositoryAlias = value;
                OnPropertyChanged("LastActiveRepositoryAlias");
            }
        }
    }

    private Guid? _lastActiveFolderId;
    public Guid? LastActiveFolderId
    {
        get { return _lastActiveFolderId; }
        set
        {
            if (_lastActiveFolderId != value)
            {
                _lastActiveFolderId = value;
                OnPropertyChanged("LastActiveFolderId");
            }
        }
    }

    // Collection of configured AI providers (provider, model, API key, host for Ollama) consumed
    // by KNoteAIAssistantCtrl's provider picker.
    private List<AiProviderRef> _aiProviderRefs;
    public List<AiProviderRef> AiProviderRefs
    {
        get
        {
            if (_aiProviderRefs == null)
                _aiProviderRefs = new List<AiProviderRef>();
            return _aiProviderRefs;
        }
        set
        {
            if (_aiProviderRefs != value)
            {
                _aiProviderRefs = value;
                OnPropertyChanged("AiProviderRefs");
            }
        }
    }

    // SMTP account used to send Email-type alarm notifications (one account per installation). The
    // password is stored in clear text in this same local config file, next to the AI providers'
    // ApiKey - protecting that file is the user's responsibility, same as for those keys.
    private string _smtpHost;
    public string SmtpHost
    {
        get { return _smtpHost; }
        set
        {
            if (_smtpHost != value)
            {
                _smtpHost = value;
                OnPropertyChanged("SmtpHost");
            }
        }
    }

    private int _smtpPort = 587;
    public int SmtpPort
    {
        get { return _smtpPort; }
        set
        {
            if (_smtpPort != value)
            {
                _smtpPort = value;
                OnPropertyChanged("SmtpPort");
            }
        }
    }

    private bool _smtpEnableSsl = true;
    public bool SmtpEnableSsl
    {
        get { return _smtpEnableSsl; }
        set
        {
            if (_smtpEnableSsl != value)
            {
                _smtpEnableSsl = value;
                OnPropertyChanged("SmtpEnableSsl");
            }
        }
    }

    private string _smtpFromAddress;
    public string SmtpFromAddress
    {
        get { return _smtpFromAddress; }
        set
        {
            if (_smtpFromAddress != value)
            {
                _smtpFromAddress = value;
                OnPropertyChanged("SmtpFromAddress");
            }
        }
    }

    private string _smtpFromDisplayName;
    public string SmtpFromDisplayName
    {
        get { return _smtpFromDisplayName; }
        set
        {
            if (_smtpFromDisplayName != value)
            {
                _smtpFromDisplayName = value;
                OnPropertyChanged("SmtpFromDisplayName");
            }
        }
    }

    private string _smtpUsername;
    public string SmtpUsername
    {
        get { return _smtpUsername; }
        set
        {
            if (_smtpUsername != value)
            {
                _smtpUsername = value;
                OnPropertyChanged("SmtpUsername");
            }
        }
    }

    // Optional: when empty, SmtpFromAddress is used as the SMTP auth username (the common case).
    private string _smtpPassword;
    public string SmtpPassword
    {
        get { return _smtpPassword; }
        set
        {
            if (_smtpPassword != value)
            {
                _smtpPassword = value;
                OnPropertyChanged("SmtpPassword");
            }
        }
    }

    // Location/size of the "Application info" alarms panel (ClientWin/Views/AppInfoAlarmsForm),
    // remembered across sessions the same way as ManagmentLocX/Y/Width/Height above. 0 means "never
    // moved/resized by the user yet" so the window centers itself instead.
    private int _appInfoAlarmsLocX;
    public int AppInfoAlarmsLocX
    {
        get { return _appInfoAlarmsLocX; }
        set
        {
            if (_appInfoAlarmsLocX != value)
            {
                _appInfoAlarmsLocX = value;
                OnPropertyChanged("AppInfoAlarmsLocX");
            }
        }
    }

    private int _appInfoAlarmsLocY;
    public int AppInfoAlarmsLocY
    {
        get { return _appInfoAlarmsLocY; }
        set
        {
            if (_appInfoAlarmsLocY != value)
            {
                _appInfoAlarmsLocY = value;
                OnPropertyChanged("AppInfoAlarmsLocY");
            }
        }
    }

    private int _appInfoAlarmsWidth;
    public int AppInfoAlarmsWidth
    {
        get { return _appInfoAlarmsWidth; }
        set
        {
            if (_appInfoAlarmsWidth != value)
            {
                _appInfoAlarmsWidth = value;
                OnPropertyChanged("AppInfoAlarmsWidth");
            }
        }
    }

    private int _appInfoAlarmsHeight;
    public int AppInfoAlarmsHeight
    {
        get { return _appInfoAlarmsHeight; }
        set
        {
            if (_appInfoAlarmsHeight != value)
            {
                _appInfoAlarmsHeight = value;
                OnPropertyChanged("AppInfoAlarmsHeight");
            }
        }
    }

    // Rows currently shown in the "Application info" alarms panel, kept here so they survive app
    // restarts - they stay until the user explicitly removes them ("Remove from list").
    private List<AppInfoAlarmRowConfig> _appInfoAlarmsRows;
    public List<AppInfoAlarmRowConfig> AppInfoAlarmsRows
    {
        get
        {
            if (_appInfoAlarmsRows == null)
                _appInfoAlarmsRows = new List<AppInfoAlarmRowConfig>();
            return _appInfoAlarmsRows;
        }
        set
        {
            if (_appInfoAlarmsRows != value)
            {
                _appInfoAlarmsRows = value;
                OnPropertyChanged("AppInfoAlarmsRows");
            }
        }
    }

    #endregion

    #region TODO: ... other params

    // KNoteManagmentForm: minimized (?), maximized (?), visible (?), hide note number (?)

    // PostIts: always top, style, ....

    #endregion

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // ---
        // Capture the validations implemented with attributes.
        // TODO: apply reflection??
        // ---

        //Validator.TryValidateProperty(this.Xxproperty,
        //   new ValidationContext(this, null, null) { MemberName = "Xxproperty" },
        //   results);

        // ---
        // Specific validations
        // ----

        if (AlarmSeconds < 30 || AlarmSeconds > 300)
        {
            results.Add(new ValidationResult
                ("KMSG: The alarm seconds must be in a range between 30 and 300."
                , new[] { "AlarmSeconds" }));
        }

        if (AutoSaveSeconds < 60 || AutoSaveSeconds > 600)
        {
            results.Add(new ValidationResult
                ("KMSG: The auto save seconds must be in a range between 60 and 600."
                , new[] { "AlarmSeconds" }));
        }

        return results;
    }

}

