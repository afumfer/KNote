using System.Xml.Serialization;
using KNote.ClientWin.Utils;
using KNote.Model;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ColumnWidthSettingsTests
{
    [TestMethod]
    public void Parse_NullOrEmpty_ReturnsEmpty()
    {
        Assert.AreEqual(0, ColumnWidthSettings.Parse(null).Count);
        Assert.AreEqual(0, ColumnWidthSettings.Parse("").Count);
        Assert.AreEqual(0, ColumnWidthSettings.Parse("   ").Count);
    }

    [TestMethod]
    public void Parse_ValidText_ReturnsWidthsByColumnName()
    {
        var widths = ColumnWidthSettings.Parse("Number=80; Topic=520;Tags=140");

        Assert.AreEqual(3, widths.Count);
        Assert.AreEqual(80, widths["Number"]);
        Assert.AreEqual(520, widths["Topic"]);
        Assert.AreEqual(140, widths["Tags"]);
    }

    [TestMethod]
    public void Parse_MalformedEntries_AreIgnored()
    {
        var widths = ColumnWidthSettings.Parse("Topic=abc;=50;Tags;Priority=-5;Status=0;Number=80;;");

        Assert.AreEqual(1, widths.Count);
        Assert.AreEqual(80, widths["Number"]);
    }

    [TestMethod]
    public void Format_ThenParse_RoundTrips()
    {
        var original = new Dictionary<string, int> { ["Number"] = 80, ["Topic"] = 520 };

        var roundTripped = ColumnWidthSettings.Parse(ColumnWidthSettings.Format(original));

        CollectionAssert.AreEquivalent(original, roundTripped);
    }

    [TestMethod]
    public void AppConfig_NotesListColumnWidths_SurvivesXmlRoundTrip()
    {
        var config = new AppConfig { NotesListColumnWidths = "Number=80;Topic=520" };
        var serializer = new XmlSerializer(typeof(AppConfig));
        using var stream = new MemoryStream();

        serializer.Serialize(stream, config);
        stream.Position = 0;
        var roundTripped = (AppConfig)serializer.Deserialize(stream);

        Assert.AreEqual("Number=80;Topic=520", roundTripped.NotesListColumnWidths);
    }
}
