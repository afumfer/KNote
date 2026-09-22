using System.Xml.Serialization;
using KNote.Model;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ConfigSerializationTests
{
    private static T RoundTrip<T>(T value)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, value);
        stream.Position = 0;
        return (T)serializer.Deserialize(stream)!;
    }

    [TestMethod]
    public void XmlSerializer_RoundTrip_PreservesAppInfoAlarmsPositionAndRowIdentifiers()
    {
        var kMessageId = Guid.NewGuid();
        var state = new AppUserState();
        state.AppInfoAlarmsWindow.Bounds.X = 123;
        state.AppInfoAlarmsWindow.Bounds.Y = 456;
        state.AppInfoAlarmsWindow.Bounds.Width = 700;
        state.AppInfoAlarmsWindow.Bounds.Height = 300;
        state.AppInfoAlarmsWindow.Rows.Add(new AppInfoAlarmRow
        {
            KMessageId = kMessageId,
            RepositoryAlias = "Personal repository",
            NotifiedAt = new DateTime(2026, 1, 1, 10, 0, 0),
            // Display-only fields, hydrated from the database at load time (see
            // AppInfoAlarmsCtrl.LoadPersistedRows) - must NOT round-trip through the state file.
            NoteId = Guid.NewGuid(),
            NoteTopic = "Test note",
            Comment = "Test comment",
            UserFullName = "Test user"
        });

        var roundTripped = RoundTrip(state);

        var bounds = roundTripped.AppInfoAlarmsWindow.Bounds;
        Assert.AreEqual(123, bounds.X);
        Assert.AreEqual(456, bounds.Y);
        Assert.AreEqual(700, bounds.Width);
        Assert.AreEqual(300, bounds.Height);
        Assert.AreEqual(1, roundTripped.AppInfoAlarmsWindow.Rows.Count);

        var row = roundTripped.AppInfoAlarmsWindow.Rows[0];
        Assert.AreEqual(kMessageId, row.KMessageId);
        Assert.AreEqual("Personal repository", row.RepositoryAlias);
        Assert.AreEqual(new DateTime(2026, 1, 1, 10, 0, 0), row.NotifiedAt);
        Assert.AreEqual(Guid.Empty, row.NoteId);
        Assert.IsNull(row.NoteTopic);
        Assert.IsNull(row.Comment);
        Assert.IsNull(row.UserFullName);
    }

    // AppInfoAlarmRow was renamed from AppInfoAlarmRowConfig, and XmlSerializer names each Rows item
    // after the class name - so a KNoteState.config written by an older build still has
    // <AppInfoAlarmRowConfig> elements under <Rows>. Deliberately not carried forward with an
    // [XmlArrayItem] compatibility alias (see AppInfoAlarmRow's doc comment): those old rows are meant
    // to be dropped silently rather than resurrected under the new name, and the rest of the file must
    // still load normally around them.
    [TestMethod]
    public void XmlSerializer_Deserialize_IgnoresRowsWrittenUnderThePreRenameElementName()
    {
        var xml = """
            <?xml version="1.0" encoding="utf-16"?>
            <AppUserState xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
              <AppInfoAlarmsWindow>
                <Bounds>
                  <X>123</X>
                  <Y>456</Y>
                  <Width>700</Width>
                  <Height>300</Height>
                </Bounds>
                <Rows>
                  <AppInfoAlarmRowConfig>
                    <KMessageId>4b1a5e2a-1111-4444-8888-000000000001</KMessageId>
                    <RepositoryAlias>Personal repository</RepositoryAlias>
                    <NotifiedAt>2026-01-01T10:00:00</NotifiedAt>
                  </AppInfoAlarmRowConfig>
                </Rows>
              </AppInfoAlarmsWindow>
            </AppUserState>
            """;

        var serializer = new XmlSerializer(typeof(AppUserState));
        using var stream = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(xml));
        var state = (AppUserState)serializer.Deserialize(stream)!;

        // The rest of the document, outside the renamed element, still loads normally...
        Assert.AreEqual(123, state.AppInfoAlarmsWindow.Bounds.X);
        Assert.AreEqual(700, state.AppInfoAlarmsWindow.Bounds.Width);
        // ...but the old row is gone rather than resurrected, per the agreed trade-off.
        Assert.AreEqual(0, state.AppInfoAlarmsWindow.Rows.Count);
    }

    [TestMethod]
    public void XmlSerializer_RoundTrip_PreservesServerCOMSettingsAndLastAiProviderAlias()
    {
        var settings = new AppUserSettings();
        settings.Connectivity.ServerCOM = new ServerCOMConfig
        {
            PortName = "COM7",
            BaudRate = 19200,
            HandShake = 3,
            Parity = 2,
            DataBits = 7,
            StopBits = 1,
            RetroDelay = 90
        };
        var state = new AppUserState();
        state.Session.LastAiProviderAlias = "Claude Sonnet";

        var settingsBack = RoundTrip(settings);
        var stateBack = RoundTrip(state);

        Assert.AreEqual("Claude Sonnet", stateBack.Session.LastAiProviderAlias);
        var com = settingsBack.Connectivity.ServerCOM;
        Assert.AreEqual("COM7", com.PortName);
        Assert.AreEqual(19200, com.BaudRate);
        Assert.AreEqual(3, com.HandShake);
        Assert.AreEqual(2, com.Parity);
        Assert.AreEqual(7, com.DataBits);
        Assert.AreEqual(1, com.StopBits);
        Assert.AreEqual(90, com.RetroDelay);
    }

    [TestMethod]
    public void XmlSerializer_SettingsWithoutServerCOMSection_LoadsDefaults()
    {
        // A settings file saved before the ServerCOM section existed.
        const string oldSettings = "<?xml version=\"1.0\"?><AppUserSettings schemaVersion=\"2\"><General><AlarmSeconds>45</AlarmSeconds></General></AppUserSettings>";

        var settings = (AppUserSettings)new XmlSerializer(typeof(AppUserSettings)).Deserialize(new StringReader(oldSettings))!;

        Assert.AreEqual(45, settings.General.AlarmSeconds);
        var com = settings.Connectivity.ServerCOM;
        Assert.AreEqual("COM1", com.PortName);
        Assert.AreEqual(115200, com.BaudRate);
        Assert.AreEqual(0, com.HandShake);
        Assert.AreEqual(0, com.Parity);
        Assert.AreEqual(8, com.DataBits);
        Assert.AreEqual(2, com.StopBits);
        Assert.AreEqual(60, com.RetroDelay);
        Assert.IsNull(com.Validate());
    }

    [TestMethod]
    public void ServerCOMConfig_ResetSerialParameters_RestoresOnlyParityDataBitsAndStopBits()
    {
        var config = new ServerCOMConfig { PortName = "COM9", BaudRate = 9600, Parity = 2, DataBits = 7, StopBits = 1 };

        config.ResetSerialParameters();

        Assert.AreEqual(0, config.Parity);
        Assert.AreEqual(8, config.DataBits);
        Assert.AreEqual(2, config.StopBits);
        Assert.AreEqual("COM9", config.PortName);
        Assert.AreEqual(9600, config.BaudRate);
    }

    [TestMethod]
    public void ServerCOMConfig_Validate_RejectsOutOfRangeValues()
    {
        Assert.IsNotNull(new ServerCOMConfig { PortName = " " }.Validate());
        Assert.IsNotNull(new ServerCOMConfig { BaudRate = 0 }.Validate());
        Assert.IsNotNull(new ServerCOMConfig { HandShake = 4 }.Validate());
        Assert.IsNotNull(new ServerCOMConfig { Parity = 5 }.Validate());
        Assert.IsNotNull(new ServerCOMConfig { DataBits = 4 }.Validate());
        Assert.IsNotNull(new ServerCOMConfig { StopBits = 0 }.Validate());
        Assert.IsNotNull(new ServerCOMConfig { RetroDelay = -1 }.Validate());
    }
}
