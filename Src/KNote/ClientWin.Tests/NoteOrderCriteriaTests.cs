using KNote.Model.Core;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for NoteOrderCriteria: Parse/Format of the Folder.OrderNotes field (folder notes-order
/// feature). No fakes needed - pure parsing/formatting logic.
/// </summary>
[TestClass]
public class NoteOrderCriteriaTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Parse_EmptyOrWhitespace_ReturnsDefaultByNoteNumberAscending(string orderNotes)
    {
        var criteria = NoteOrderCriteria.Parse(orderNotes);

        Assert.AreEqual(NoteOrderMode.Default, criteria.Mode);
        Assert.AreEqual("NoteNumber", criteria.EffectiveColumn);
        Assert.IsTrue(criteria.Ascending);
    }

    [TestMethod]
    public void Parse_Fixed_ReturnsColumnAndDirection()
    {
        var criteria = NoteOrderCriteria.Parse("FIXED|ModificationDateTime|DESC");

        Assert.AreEqual(NoteOrderMode.Fixed, criteria.Mode);
        Assert.AreEqual("ModificationDateTime", criteria.Column);
        Assert.IsFalse(criteria.Ascending);
    }

    [TestMethod]
    public void Parse_AutoWithoutCache_ReturnsAutoModeWithNoColumn()
    {
        var criteria = NoteOrderCriteria.Parse("AUTO");

        Assert.AreEqual(NoteOrderMode.Auto, criteria.Mode);
        Assert.IsNull(criteria.Column);
        Assert.AreEqual("NoteNumber", criteria.EffectiveColumn); // falls back to the default column
    }

    [TestMethod]
    public void Parse_AutoWithCache_ReturnsCachedColumnAndDirection()
    {
        var criteria = NoteOrderCriteria.Parse("AUTO|Topic|ASC");

        Assert.AreEqual(NoteOrderMode.Auto, criteria.Mode);
        Assert.AreEqual("Topic", criteria.Column);
        Assert.IsTrue(criteria.Ascending);
    }

    // Covers data imported from another tool/version, or free text a user typed into this field
    // before this feature existed: any value that isn't an exact match for our format must resolve
    // to Default (NoteNumber ascending), never to a partially-guessed mode - especially not Auto,
    // which would otherwise arm the auto-persist-on-click behavior and silently overwrite the
    // imported value the next time the folder is sorted in the selector.
    [TestMethod]
    [DataRow("GARBAGE")]
    [DataRow("Sorted manually by priority, see notes")]
    [DataRow("FIXED")]
    [DataRow("FIXED|NotARealColumn|ASC")]
    [DataRow("FIXED|Topic")]
    [DataRow("FIXED|Topic|MAYBE")]
    [DataRow("AUTO|NotARealColumn|ASC")]
    [DataRow("AUTO|Topic")]
    [DataRow("AUTO|Topic|MAYBE")]
    [DataRow("AUTOMATIC")]
    [DataRow("AUTO-IMPORT-2019")]
    public void Parse_CorruptOrUnknownValue_FallsBackToDefault(string orderNotes)
    {
        var criteria = NoteOrderCriteria.Parse(orderNotes);

        Assert.AreEqual(NoteOrderMode.Default, criteria.Mode);
        Assert.AreEqual("NoteNumber", criteria.EffectiveColumn);
        Assert.IsTrue(criteria.Ascending);
    }

    [TestMethod]
    public void Format_Default_ReturnsEmptyString()
    {
        var criteria = new NoteOrderCriteria(NoteOrderMode.Default, NoteOrderCriteria.DefaultColumn, true);

        Assert.AreEqual("", criteria.Format());
    }

    [TestMethod]
    public void Format_Fixed_ReturnsFixedColumnDirection()
    {
        var criteria = new NoteOrderCriteria(NoteOrderMode.Fixed, "Priority", false);

        Assert.AreEqual("FIXED|Priority|DESC", criteria.Format());
    }

    [TestMethod]
    public void Format_AutoWithoutColumn_ReturnsBareAuto()
    {
        var criteria = new NoteOrderCriteria(NoteOrderMode.Auto, null, true);

        Assert.AreEqual("AUTO", criteria.Format());
    }

    [TestMethod]
    public void Format_AutoWithColumn_ReturnsCachedAutoValue()
    {
        var criteria = new NoteOrderCriteria(NoteOrderMode.Auto, "CreationDateTime", true);

        Assert.AreEqual("AUTO|CreationDateTime|ASC", criteria.Format());
    }

    [TestMethod]
    public void RoundTrip_FormatThenParse_ReturnsEquivalentCriteria()
    {
        var original = new NoteOrderCriteria(NoteOrderMode.Fixed, "Tags", false);

        var roundTripped = NoteOrderCriteria.Parse(original.Format());

        Assert.AreEqual(original.Mode, roundTripped.Mode);
        Assert.AreEqual(original.Column, roundTripped.Column);
        Assert.AreEqual(original.Ascending, roundTripped.Ascending);
    }
}
