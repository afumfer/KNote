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
}
