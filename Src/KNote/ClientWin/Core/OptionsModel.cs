using System.ComponentModel.DataAnnotations;
using KNote.Model;

namespace KNote.ClientWin.Core;

/// <summary>
/// Edit model of the Options dialog: the few settings it shows, flattened, gathered from AppUserSettings
/// and AppUserState. The dialog edits this copy and only <see cref="ApplyTo"/> writes it back, so
/// cancelling leaves the real configuration untouched.
/// </summary>
public class OptionsModel : SmartModelDtoBase
{
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

    private int _autoSaveSeconds;
    public int AutoSaveSeconds
    {
        get { return _autoSaveSeconds; }
        set
        {
            if (_autoSaveSeconds != value)
            {
                _autoSaveSeconds = value;
                OnPropertyChanged("AutoSaveSeconds");
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

    // Lives in the application state (set automatically when a connection fails); the dialog can clear
    // it after a successful connection test.
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

    public static OptionsModel From(AppUserSettings settings, AppUserState state)
    {
        var email = settings.Notifications.Email;

        var model = new OptionsModel
        {
            AlarmActivated = settings.General.AlarmActivated,
            AlarmSeconds = settings.General.AlarmSeconds,
            AutoSaveActivated = settings.General.AutoSaveActivated,
            AutoSaveSeconds = settings.General.AutoSaveSeconds,
            ChatHubUrl = settings.Connectivity.ChatHub.Url,
            ChatHubAutoConnectDisabled = state.Session.ChatHubAutoConnectDisabled,
            SmtpHost = email.Host,
            SmtpPort = email.Port,
            SmtpEnableSsl = email.EnableSsl,
            SmtpFromAddress = email.FromAddress,
            SmtpFromDisplayName = email.FromDisplayName,
            SmtpUsername = email.Username,
            SmtpPassword = email.Password
        };
        model.SetIsDirty(false);
        return model;
    }

    public void ApplyTo(AppUserSettings settings, AppUserState state)
    {
        var email = settings.Notifications.Email;

        settings.General.AlarmActivated = AlarmActivated;
        settings.General.AlarmSeconds = AlarmSeconds;
        settings.General.AutoSaveActivated = AutoSaveActivated;
        settings.General.AutoSaveSeconds = AutoSaveSeconds;
        settings.Connectivity.ChatHub.Url = ChatHubUrl;
        state.Session.ChatHubAutoConnectDisabled = ChatHubAutoConnectDisabled;
        email.Host = SmtpHost;
        email.Port = SmtpPort;
        email.EnableSsl = SmtpEnableSsl;
        email.FromAddress = SmtpFromAddress;
        email.FromDisplayName = SmtpFromDisplayName;
        email.Username = SmtpUsername;
        email.Password = SmtpPassword;
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

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
                , new[] { "AutoSaveSeconds" }));
        }

        return results;
    }
}
