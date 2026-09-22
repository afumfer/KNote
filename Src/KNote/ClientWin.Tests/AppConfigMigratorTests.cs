using System.Xml.Serialization;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests;

[TestClass]
public class AppConfigMigratorTests
{
    private static readonly string FixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "KNoteData.v1.config");

    private static AppConfigV1 LoadFixture() => XmlConfigFile.Load<AppConfigV1>(FixturePath, out _)!;

    private static string ToXml<T>(T value)
    {
        using var writer = new StringWriter();
        new XmlSerializer(typeof(T)).Serialize(writer, value);
        return writer.ToString();
    }

    private static T FromXml<T>(string xml)
    {
        using var reader = new StringReader(xml);
        return (T)new XmlSerializer(typeof(T)).Deserialize(reader)!;
    }

    [TestMethod]
    public void FromV1_Fixture_MapsEverySettingsValue()
    {
        var v1 = LoadFixture();

        var (settings, _) = AppConfigMigrator.FromV1(v1);

        Assert.AreEqual(2, settings.SchemaVersion);
        Assert.AreEqual(@"C:\Users\tester\AppData\Local\KNote\KNoteWinApp.log", settings.General.LogFile);
        Assert.IsFalse(settings.General.LogActivated);
        Assert.IsTrue(settings.General.AlarmActivated);
        Assert.AreEqual(35, settings.General.AlarmSeconds);
        Assert.IsTrue(settings.General.AutoSaveActivated);
        Assert.AreEqual(60, settings.General.AutoSaveSeconds);

        Assert.AreEqual(2, settings.Repositories.Items.Count);
        Assert.AreEqual("Personal respository", settings.Repositories.Items[0].Alias);
        Assert.AreEqual("EntityFramework", settings.Repositories.Items[0].Orm);
        Assert.AreEqual("Sql Server repository", settings.Repositories.Items[1].Alias);
        Assert.IsTrue(settings.Repositories.Items[1].ResourceContentInDB);
        Assert.AreEqual("Mini Assistant", settings.Repositories.Assistant.Alias);
        Assert.AreEqual(@"Data Source=C:\KaNote\Data\assistant.db", settings.Repositories.Assistant.ConnectionString);

        Assert.AreEqual(3, settings.Ai.Providers.Count);
        Assert.AreEqual("Anthropic test", settings.Ai.Providers[1].Alias);
        Assert.AreEqual("test-anthropic-key", settings.Ai.Providers[1].ApiKey);
        Assert.AreEqual("http://localhost:11434", settings.Ai.Providers[2].Host);

        var email = settings.Notifications.Email;
        Assert.AreEqual("smtp.example.com", email.Host);
        Assert.AreEqual(465, email.Port);
        Assert.IsFalse(email.EnableSsl);
        Assert.AreEqual("sender@example.com", email.FromAddress);
        Assert.AreEqual("Test Sender", email.FromDisplayName);
        Assert.AreEqual("smtp-user@example.com", email.Username);
        Assert.AreEqual("test-smtp-password", email.Password);

        Assert.AreEqual("http://chat.example.com/KNote/chathub", settings.Connectivity.ChatHub.Url);
        Assert.IsFalse(settings.Connectivity.MessageBroker.Activated);
        var com = settings.Connectivity.ServerCOM;
        Assert.AreEqual("COM7", com.PortName);
        Assert.AreEqual(19200, com.BaudRate);
        Assert.AreEqual(3, com.HandShake);
        Assert.AreEqual(2, com.Parity);
        Assert.AreEqual(7, com.DataBits);
        Assert.AreEqual(1, com.StopBits);
        Assert.AreEqual(90, com.RetroDelay);
    }

    [TestMethod]
    public void FromV1_Fixture_MapsEveryStateValue()
    {
        var v1 = LoadFixture();

        var (_, state) = AppConfigMigrator.FromV1(v1);

        Assert.AreEqual(2, state.SchemaVersion);
        Assert.AreEqual(v1.LastDateTimeStart, state.Session.LastDateTimeStart);
        Assert.AreEqual(3929, state.Session.RunCounter);
        Assert.AreEqual("Personal respository", state.Session.LastActiveRepositoryAlias);
        Assert.AreEqual(Guid.Parse("d577bc3e-64f8-417e-adeb-2b30b328b8d8"), state.Session.LastActiveFolderId);
        Assert.AreEqual("Anthropic test", state.Session.LastAiProviderAlias);
        Assert.IsFalse(state.Session.ChatHubAutoConnectDisabled);

        var bounds = state.ManagementWindow.Bounds;
        Assert.AreEqual(1970, bounds.X);
        Assert.AreEqual(61, bounds.Y);
        Assert.AreEqual(1363, bounds.Width);
        Assert.AreEqual(921, bounds.Height);

        var list = state.ManagementWindow.NotesList;
        Assert.AreEqual(1, list.SortColumn);
        Assert.IsTrue(list.SortAscending);
        Assert.IsFalse(list.CompactView);
        Assert.AreEqual("Tags=45;InternalTags=92;Priority=52;NoteNumber=81", list.ColumnWidths);
        Assert.IsTrue(list.ShowFilter);

        var panels = state.ManagementWindow.Panels;
        Assert.IsTrue(panels.FoldersExplorer);
        Assert.IsFalse(panels.Header);
        Assert.IsTrue(panels.Toolbar);
        Assert.IsTrue(panels.MainMenu);
        Assert.IsFalse(panels.VerticalNotesPanel);

        var alarms = state.AppInfoAlarmsWindow;
        Assert.AreEqual(2997, alarms.Bounds.X);
        Assert.AreEqual(523, alarms.Bounds.Y);
        Assert.AreEqual(794, alarms.Bounds.Width);
        Assert.AreEqual(499, alarms.Bounds.Height);
        // The one accepted exception to "every state value": AppInfoAlarmRow was renamed from
        // AppInfoAlarmRowConfig, and the old element name is deliberately not kept alive (see its doc
        // comment), so the fixture's old rows never even reach v1.AppInfoAlarmsRows - migrated as empty.
        Assert.AreEqual(0, alarms.Rows.Count);
    }

    [TestMethod]
    public void FromV1_SmtpHostWithStrayWhitespace_IsTrimmed()
    {
        var v1 = new AppConfigV1 { SmtpHost = "\tsmtp.example.com " };

        var (settings, _) = AppConfigMigrator.FromV1(v1);

        Assert.AreEqual("smtp.example.com", settings.Notifications.Email.Host);
    }

    [TestMethod]
    public void FromV1_OldMinimalConfig_KeepsTheDefaultsOfTheOldApplication()
    {
        // A config saved by a very old version: nothing but a counter.
        var v1 = FromXml<AppConfigV1>("<?xml version=\"1.0\"?><AppConfig><RunCounter>5</RunCounter></AppConfig>");

        var (settings, state) = AppConfigMigrator.FromV1(v1);

        Assert.AreEqual(5, state.Session.RunCounter);
        Assert.AreEqual(587, settings.Notifications.Email.Port);
        Assert.IsTrue(settings.Notifications.Email.EnableSsl);
        Assert.AreEqual("COM1", settings.Connectivity.ServerCOM.PortName);
        Assert.AreEqual(115200, settings.Connectivity.ServerCOM.BaudRate);
        Assert.IsNull(settings.Connectivity.ServerCOM.Validate());
        Assert.IsNull(settings.Repositories.Assistant.ConnectionString);
        Assert.AreEqual(0, settings.Repositories.Items.Count);
        Assert.IsNull(state.Session.LastAiProviderAlias);
        // Panels missing from the old file stay visible, as before.
        Assert.IsTrue(state.ManagementWindow.Panels.FoldersExplorer);
        Assert.IsTrue(state.ManagementWindow.Panels.Header);
        Assert.IsTrue(state.ManagementWindow.Panels.Toolbar);
        Assert.IsTrue(state.ManagementWindow.Panels.MainMenu);
        Assert.IsFalse(state.ManagementWindow.NotesList.ShowFilter);
    }

    // Flipping ONE flag of the old config from its default must change exactly the expected V2 flag:
    // guards against two booleans being swapped or dropped in the mapping.
    [TestMethod]
    public void FromV1_EachBooleanLandsInItsOwnV2Flag()
    {
        var cases = new (string V2Flag, Action<AppConfigV1> Flip)[]
        {
            ("General.LogActivated", v => v.LogActivated = true),
            ("General.AlarmActivated", v => v.AlarmActivated = true),
            ("General.AutoSaveActivated", v => v.AutoSaveActivated = true),
            ("Email.EnableSsl", v => v.SmtpEnableSsl = false),
            ("MessageBroker.Activated", v => v.ActivateMessageBroker = true),
            ("Session.ChatHubAutoConnectDisabled", v => v.ChatHubAutoConnectDisabled = true),
            ("NotesList.SortAscending", v => v.AscendigOrderNotes = true),
            ("NotesList.CompactView", v => v.CompactViewNoteslist = true),
            ("NotesList.ShowFilter", v => v.ShowListFilter = true),
            ("Panels.FoldersExplorer", v => v.ShowFoldersExplorerTab = false),
            ("Panels.Header", v => v.ShowHeaderPanel = false),
            ("Panels.Toolbar", v => v.ShowToolbar = false),
            ("Panels.MainMenu", v => v.ShowMainMenu = false),
            ("Panels.VerticalNotesPanel", v => v.VerticalPanelForNotes = true),
        };

        var baseline = Flags(new AppConfigV1());
        Assert.AreEqual(cases.Length, baseline.Count, "every V2 flag must have a case");

        foreach (var (v2Flag, flip) in cases)
        {
            var v1 = new AppConfigV1();
            flip(v1);

            var changed = Flags(v1).Where(f => f.Value != baseline[f.Key]).Select(f => f.Key).ToList();

            CollectionAssert.AreEqual(new[] { v2Flag }, changed, $"flipping the V1 flag for {v2Flag}");
        }
    }

    private static Dictionary<string, bool> Flags(AppConfigV1 v1)
    {
        var (s, st) = AppConfigMigrator.FromV1(v1);
        return new Dictionary<string, bool>
        {
            ["General.LogActivated"] = s.General.LogActivated,
            ["General.AlarmActivated"] = s.General.AlarmActivated,
            ["General.AutoSaveActivated"] = s.General.AutoSaveActivated,
            ["Email.EnableSsl"] = s.Notifications.Email.EnableSsl,
            ["MessageBroker.Activated"] = s.Connectivity.MessageBroker.Activated,
            ["Session.ChatHubAutoConnectDisabled"] = st.Session.ChatHubAutoConnectDisabled,
            ["NotesList.SortAscending"] = st.ManagementWindow.NotesList.SortAscending,
            ["NotesList.CompactView"] = st.ManagementWindow.NotesList.CompactView,
            ["NotesList.ShowFilter"] = st.ManagementWindow.NotesList.ShowFilter,
            ["Panels.FoldersExplorer"] = st.ManagementWindow.Panels.FoldersExplorer,
            ["Panels.Header"] = st.ManagementWindow.Panels.Header,
            ["Panels.Toolbar"] = st.ManagementWindow.Panels.Toolbar,
            ["Panels.MainMenu"] = st.ManagementWindow.Panels.MainMenu,
            ["Panels.VerticalNotesPanel"] = st.ManagementWindow.Panels.VerticalNotesPanel,
        };
    }

    [TestMethod]
    public void V2Documents_SerializeWithoutTheOldMisspelledNames_AndRoundTrip()
    {
        var (settings, state) = AppConfigMigrator.FromV1(LoadFixture());

        var settingsXml = ToXml(settings);
        var stateXml = ToXml(state);

        foreach (var xml in new[] { settingsXml, stateXml })
        {
            Assert.IsFalse(xml.Contains("Respository"), "misspelled 'Respository' must be gone");
            Assert.IsFalse(xml.Contains("Managment"), "misspelled 'Managment' must be gone");
            Assert.IsFalse(xml.Contains("Ascendig"), "misspelled 'Ascendig' must be gone");
        }

        Assert.IsTrue(settingsXml.Contains("<AppUserSettings"));
        Assert.IsTrue(settingsXml.Contains("schemaVersion=\"2\""));
        Assert.IsTrue(stateXml.Contains("<AppUserState"));

        // Secrets stay out of the state file; the frequently rewritten state stays out of the settings file.
        Assert.IsFalse(stateXml.Contains("test-smtp-password") || stateXml.Contains("test-anthropic-key"));
        Assert.IsFalse(settingsXml.Contains("RunCounter") || settingsXml.Contains("<Bounds>"));

        var settings2 = FromXml<AppUserSettings>(settingsXml);
        var state2 = FromXml<AppUserState>(stateXml);
        Assert.AreEqual(ToXml(settings), ToXml(settings2));
        Assert.AreEqual(ToXml(state), ToXml(state2));
        Assert.AreEqual("COM7", settings2.Connectivity.ServerCOM.PortName);
        Assert.AreEqual(0, state2.AppInfoAlarmsWindow.Rows.Count);
    }

    [TestMethod]
    public void V2Documents_WithMissingSections_LoadWithDefaults()
    {
        var settings = FromXml<AppUserSettings>("<AppUserSettings schemaVersion=\"2\"><General><LogActivated>true</LogActivated></General></AppUserSettings>");
        var state = FromXml<AppUserState>("<AppUserState schemaVersion=\"2\" />");

        Assert.IsTrue(settings.General.LogActivated);
        Assert.AreEqual(587, settings.Notifications.Email.Port);
        Assert.AreEqual("COM1", settings.Connectivity.ServerCOM.PortName);
        Assert.IsTrue(state.ManagementWindow.Panels.Toolbar);
        Assert.AreEqual(0, state.AppInfoAlarmsWindow.Rows.Count);
    }

    [TestMethod]
    public void WithoutSecrets_EmptiesEverySecret_AndLeavesTheInputUntouched()
    {
        var v1 = LoadFixture();
        v1.RespositoryRefs[1].ConnectionString = "Server=db;Database=KNote;User Id=sa;Password=s3cr3t;Encrypt=false";

        var scrubbed = AppConfigMigrator.WithoutSecrets(v1);

        Assert.IsNull(scrubbed.SmtpPassword);
        Assert.IsTrue(scrubbed.AiProviderRefs.All(p => string.IsNullOrEmpty(p.ApiKey)));
        Assert.AreEqual("Server=db;Database=KNote;User Id=sa;Password=;Encrypt=false", scrubbed.RespositoryRefs[1].ConnectionString);
        // Everything that is not a secret survives, so the backup is still a usable V1 config.
        Assert.AreEqual("smtp.example.com", scrubbed.SmtpHost);
        Assert.AreEqual(3, scrubbed.AiProviderRefs.Count);
        Assert.AreEqual("Anthropic test", scrubbed.AiProviderRefs[1].Alias);
        Assert.AreEqual(3929, scrubbed.RunCounter);
        Assert.AreEqual(@"Data Source=C:\KaNote\Data\personal.db", scrubbed.RespositoryRefs[0].ConnectionString);

        Assert.AreEqual("test-smtp-password", v1.SmtpPassword);
        Assert.AreEqual("test-anthropic-key", v1.AiProviderRefs[1].ApiKey);
        StringAssert.Contains(v1.RespositoryRefs[1].ConnectionString, "Password=s3cr3t");
    }
}
