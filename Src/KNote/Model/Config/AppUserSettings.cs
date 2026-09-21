using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace KNote.Model;

// Root of KNoteData.config since schema version 2: what the USER configures (options, repositories, AI
// providers, e-mail account, connectivity). What the application writes by itself (window positions,
// last active repository, alarm rows...) lives in AppUserState / KNoteState.config, so day-to-day use never
// rewrites this file - which holds the secrets (stored encrypted, see ClientWin.Core.AppUserSettingsSecrets).
// The pre-V2 flat layout (root <AppConfig>) is read through AppConfigV1 and converted by AppConfigMigrator.
[Serializable]
public class AppUserSettings
{
    public const int CurrentSchemaVersion = 2;

    [XmlAttribute("schemaVersion")]
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public GeneralConfig General { get; set; } = new();

    public RepositoriesConfig Repositories { get; set; } = new();

    public AiConfig Ai { get; set; } = new();

    public NotificationsConfig Notifications { get; set; } = new();

    public ConnectivityConfig Connectivity { get; set; } = new();
}

[Serializable]
public class GeneralConfig
{
    public bool LogActivated { get; set; }

    public string LogFile { get; set; }

    public bool AlarmActivated { get; set; }

    public int AlarmSeconds { get; set; }

    public bool AutoSaveActivated { get; set; }

    public int AutoSaveSeconds { get; set; }
}

[Serializable]
public class RepositoriesConfig
{
    public List<RepositoryRef> Items { get; set; } = new();

    // Same semantic as the old AssistantRepositoryRef: never null, and "not configured" means a
    // ConnectionString of null.
    public RepositoryRef Assistant { get; set; } = new();
}

[Serializable]
public class AiConfig
{
    // Configured AI providers (provider, model, API key, host for Ollama) consumed by KNoteAIAssistantCtrl.
    public List<AiProviderRef> Providers { get; set; } = new();
}

[Serializable]
public class NotificationsConfig
{
    public EmailConfig Email { get; set; } = new();
}

// SMTP account used to send Email-type alarm notifications (one account per installation). The password
// is encrypted on disk. Username is optional: when empty, FromAddress is used as the SMTP auth username.
[Serializable]
public class EmailConfig
{
    public string Host { get; set; }

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string FromAddress { get; set; }

    public string FromDisplayName { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }
}

[Serializable]
public class ConnectivityConfig
{
    public ChatHubConfig ChatHub { get; set; } = new();

    public MessageBrokerConfig MessageBroker { get; set; } = new();

    // RS-232 settings of the KntServerCOM component.
    public ServerCOMConfig ServerCOM { get; set; } = new();
}

[Serializable]
public class ChatHubConfig
{
    public string Url { get; set; }
}

[Serializable]
public class MessageBrokerConfig
{
    public bool Activated { get; set; }
}
