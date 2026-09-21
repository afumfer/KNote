using System.IO;
using System.Xml.Serialization;

namespace KNote.Model;

// Converts the pre-V2 flat KNoteData.config (AppConfigV1) into the two V2 documents. Pure and
// dependency-free: file handling, backups and secret encryption belong to the caller (ClientWin).
public static class AppConfigMigrator
{
    public static (AppUserSettings Settings, AppUserState State) FromV1(AppConfigV1 v1)
    {
        var settings = new AppUserSettings
        {
            General =
            {
                LogActivated = v1.LogActivated,
                LogFile = v1.LogFile,
                AlarmActivated = v1.AlarmActivated,
                AlarmSeconds = v1.AlarmSeconds,
                AutoSaveActivated = v1.AutoSaveActivated,
                AutoSaveSeconds = v1.AutoSaveSeconds
            },
            Repositories =
            {
                Items = v1.RespositoryRefs ?? new(),
                Assistant = v1.AssistantRespositoryRef ?? new()
            },
            Ai =
            {
                Providers = v1.AiProviderRefs ?? new()
            },
            Notifications =
            {
                Email =
                {
                    Host = v1.SmtpHost?.Trim(),
                    Port = v1.SmtpPort,
                    EnableSsl = v1.SmtpEnableSsl,
                    FromAddress = v1.SmtpFromAddress,
                    FromDisplayName = v1.SmtpFromDisplayName,
                    Username = v1.SmtpUsername,
                    Password = v1.SmtpPassword
                }
            },
            Connectivity =
            {
                ChatHub = { Url = v1.ChatHubUrl },
                MessageBroker = { Activated = v1.ActivateMessageBroker },
                ServerCOM = v1.ServerCOM ?? new()
            }
        };

        var state = new AppUserState
        {
            Session =
            {
                LastDateTimeStart = v1.LastDateTimeStart,
                RunCounter = v1.RunCounter,
                LastActiveRepositoryAlias = v1.LastActiveRepositoryAlias,
                LastActiveFolderId = v1.LastActiveFolderId,
                LastAiProviderAlias = v1.LastAiProviderAlias,
                ChatHubAutoConnectDisabled = v1.ChatHubAutoConnectDisabled
            },
            ManagementWindow =
            {
                Bounds =
                {
                    X = v1.ManagmentLocX,
                    Y = v1.ManagmentLocY,
                    Width = v1.ManagmentWidth,
                    Height = v1.ManagmentHeight
                },
                NotesList =
                {
                    SortColumn = v1.ColOrderNotes,
                    SortAscending = v1.AscendigOrderNotes,
                    CompactView = v1.CompactViewNoteslist,
                    ColumnWidths = v1.NotesListColumnWidths,
                    ShowFilter = v1.ShowListFilter
                },
                Panels =
                {
                    FoldersExplorer = v1.ShowFoldersExplorerTab ?? true,
                    Header = v1.ShowHeaderPanel ?? true,
                    Toolbar = v1.ShowToolbar ?? true,
                    MainMenu = v1.ShowMainMenu ?? true,
                    VerticalNotesPanel = v1.VerticalPanelForNotes
                }
            },
            AppInfoAlarmsWindow =
            {
                Bounds =
                {
                    X = v1.AppInfoAlarmsLocX,
                    Y = v1.AppInfoAlarmsLocY,
                    Width = v1.AppInfoAlarmsWidth,
                    Height = v1.AppInfoAlarmsHeight
                },
                Rows = v1.AppInfoAlarmsRows ?? new()
            }
        };

        return (settings, state);
    }

    // TEMPORARY bridge, removed when the V2 files are written to disk: until then the application keeps
    // saving the flat V1 file from the in-memory V2 documents, so the format on disk doesn't change
    // while the consumers are being moved to AppUserSettings / AppUserState.
    public static AppConfigV1 ToV1(AppUserSettings settings, AppUserState state)
    {
        var window = state.ManagementWindow;
        var alarms = state.AppInfoAlarmsWindow;
        var email = settings.Notifications.Email;

        return new AppConfigV1
        {
            LastDateTimeStart = state.Session.LastDateTimeStart,
            RunCounter = state.Session.RunCounter,
            LogFile = settings.General.LogFile,
            LogActivated = settings.General.LogActivated,
            AlarmActivated = settings.General.AlarmActivated,
            AutoSaveActivated = settings.General.AutoSaveActivated,
            AlarmSeconds = settings.General.AlarmSeconds,
            AutoSaveSeconds = settings.General.AutoSaveSeconds,

            ManagmentLocX = window.Bounds.X,
            ManagmentLocY = window.Bounds.Y,
            ManagmentWidth = window.Bounds.Width,
            ManagmentHeight = window.Bounds.Height,

            ColOrderNotes = window.NotesList.SortColumn,
            AscendigOrderNotes = window.NotesList.SortAscending,
            CompactViewNoteslist = window.NotesList.CompactView,
            NotesListColumnWidths = window.NotesList.ColumnWidths,
            ShowListFilter = window.NotesList.ShowFilter,

            ShowFoldersExplorerTab = window.Panels.FoldersExplorer,
            VerticalPanelForNotes = window.Panels.VerticalNotesPanel,
            ShowHeaderPanel = window.Panels.Header,
            ShowToolbar = window.Panels.Toolbar,
            ShowMainMenu = window.Panels.MainMenu,

            ChatHubUrl = settings.Connectivity.ChatHub.Url,
            ChatHubAutoConnectDisabled = state.Session.ChatHubAutoConnectDisabled,
            ActivateMessageBroker = settings.Connectivity.MessageBroker.Activated,

            RespositoryRefs = settings.Repositories.Items,
            AssistantRespositoryRef = settings.Repositories.Assistant,
            LastActiveRepositoryAlias = state.Session.LastActiveRepositoryAlias,
            LastActiveFolderId = state.Session.LastActiveFolderId,

            AiProviderRefs = settings.Ai.Providers,
            LastAiProviderAlias = state.Session.LastAiProviderAlias,

            ServerCOM = settings.Connectivity.ServerCOM,

            SmtpHost = email.Host,
            SmtpPort = email.Port,
            SmtpEnableSsl = email.EnableSsl,
            SmtpFromAddress = email.FromAddress,
            SmtpFromDisplayName = email.FromDisplayName,
            SmtpUsername = email.Username,
            SmtpPassword = email.Password,

            AppInfoAlarmsLocX = alarms.Bounds.X,
            AppInfoAlarmsLocY = alarms.Bounds.Y,
            AppInfoAlarmsWidth = alarms.Bounds.Width,
            AppInfoAlarmsHeight = alarms.Bounds.Height,
            AppInfoAlarmsRows = alarms.Rows
        };
    }

    // Copy of the old config with every secret emptied (SMTP password, AI API keys, passwords inside
    // connection strings): what is kept as the pre-migration backup so plain-text secrets don't stay on
    // disk. The input is left untouched.
    public static AppConfigV1 WithoutSecrets(AppConfigV1 v1)
    {
        var serializer = new XmlSerializer(typeof(AppConfigV1));
        using var buffer = new MemoryStream();
        serializer.Serialize(buffer, v1);
        buffer.Position = 0;
        var copy = (AppConfigV1)serializer.Deserialize(buffer);

        copy.SmtpPassword = null;
        foreach (var provider in copy.AiProviderRefs)
            provider.ApiKey = null;
        foreach (var repository in copy.RespositoryRefs)
            repository.ConnectionString = ConnectionStringSecrets.WithoutPassword(repository.ConnectionString);
        if (copy.AssistantRespositoryRef != null)
            copy.AssistantRespositoryRef.ConnectionString =
                ConnectionStringSecrets.WithoutPassword(copy.AssistantRespositoryRef.ConnectionString);

        return copy;
    }
}
