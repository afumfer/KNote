using System.Xml.Serialization;
using KNote.Model;

namespace KNote.ClientWin.Tests;

[TestClass]
public class AppConfigSerializationTests
{
    [TestMethod]
    public void XmlSerializer_RoundTrip_PreservesAppInfoAlarmsPositionAndRowIdentifiers()
    {
        var kMessageId = Guid.NewGuid();
        var config = new AppConfig
        {
            AppInfoAlarmsLocX = 123,
            AppInfoAlarmsLocY = 456,
            AppInfoAlarmsWidth = 700,
            AppInfoAlarmsHeight = 300
        };
        config.AppInfoAlarmsRows.Add(new AppInfoAlarmRowConfig
        {
            KMessageId = kMessageId,
            RepositoryAlias = "Personal respository",
            NotifiedAt = new DateTime(2026, 1, 1, 10, 0, 0),
            // Display-only fields, hydrated from the database at load time (see
            // AppInfoAlarmsCtrl.LoadPersistedRows) - must NOT round-trip through AppConfig.
            NoteId = Guid.NewGuid(),
            NoteTopic = "Test note",
            Comment = "Test comment",
            UserFullName = "Test user"
        });

        var serializer = new XmlSerializer(typeof(AppConfig));
        using var stream = new MemoryStream();

        serializer.Serialize(stream, config);
        stream.Position = 0;
        var roundTripped = (AppConfig)serializer.Deserialize(stream);

        Assert.AreEqual(123, roundTripped.AppInfoAlarmsLocX);
        Assert.AreEqual(456, roundTripped.AppInfoAlarmsLocY);
        Assert.AreEqual(700, roundTripped.AppInfoAlarmsWidth);
        Assert.AreEqual(300, roundTripped.AppInfoAlarmsHeight);
        Assert.AreEqual(1, roundTripped.AppInfoAlarmsRows.Count);

        var row = roundTripped.AppInfoAlarmsRows[0];
        Assert.AreEqual(kMessageId, row.KMessageId);
        Assert.AreEqual("Personal respository", row.RepositoryAlias);
        Assert.AreEqual(new DateTime(2026, 1, 1, 10, 0, 0), row.NotifiedAt);
        Assert.AreEqual(Guid.Empty, row.NoteId);
        Assert.IsNull(row.NoteTopic);
        Assert.IsNull(row.Comment);
        Assert.IsNull(row.UserFullName);
    }

    private static AppConfig RoundTrip(AppConfig config)
    {
        var serializer = new XmlSerializer(typeof(AppConfig));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, config);
        stream.Position = 0;
        return (AppConfig)serializer.Deserialize(stream);
    }

    [TestMethod]
    public void XmlSerializer_RoundTrip_PreservesServerCOMSettingsAndLastAiProviderAlias()
    {
        var config = new AppConfig { LastAiProviderAlias = "Claude Sonnet" };
        config.ServerCOM = new ServerCOMConfig
        {
            PortName = "COM7",
            BaudRate = 19200,
            HandShake = 3,
            Parity = 2,
            DataBits = 7,
            StopBits = 1,
            RetroDelay = 90
        };

        var roundTripped = RoundTrip(config);

        Assert.AreEqual("Claude Sonnet", roundTripped.LastAiProviderAlias);
        Assert.AreEqual("COM7", roundTripped.ServerCOM.PortName);
        Assert.AreEqual(19200, roundTripped.ServerCOM.BaudRate);
        Assert.AreEqual(3, roundTripped.ServerCOM.HandShake);
        Assert.AreEqual(2, roundTripped.ServerCOM.Parity);
        Assert.AreEqual(7, roundTripped.ServerCOM.DataBits);
        Assert.AreEqual(1, roundTripped.ServerCOM.StopBits);
        Assert.AreEqual(90, roundTripped.ServerCOM.RetroDelay);
    }

    [TestMethod]
    public void XmlSerializer_ConfigWithoutServerCOMSection_LoadsDefaults()
    {
        // A KNoteData.config saved before the ServerCOM section / LastAiProviderAlias existed.
        const string oldConfig = "<?xml version=\"1.0\"?><AppConfig><RunCounter>5</RunCounter></AppConfig>";

        var serializer = new XmlSerializer(typeof(AppConfig));
        var config = (AppConfig)serializer.Deserialize(new StringReader(oldConfig));

        Assert.AreEqual(5, config.RunCounter);
        Assert.IsNull(config.LastAiProviderAlias);
        Assert.AreEqual("COM1", config.ServerCOM.PortName);
        Assert.AreEqual(115200, config.ServerCOM.BaudRate);
        Assert.AreEqual(0, config.ServerCOM.HandShake);
        Assert.AreEqual(0, config.ServerCOM.Parity);
        Assert.AreEqual(8, config.ServerCOM.DataBits);
        Assert.AreEqual(2, config.ServerCOM.StopBits);
        Assert.AreEqual(60, config.ServerCOM.RetroDelay);
        Assert.IsNull(config.ServerCOM.Validate());
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
