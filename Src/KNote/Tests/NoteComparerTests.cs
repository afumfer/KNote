using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// «Compare» (H12, H22): NoteComparer (the fields that differ and the text by lines, folded far from the changes) and WordDiff (the
/// words that changed in a line replaced by another, or the whole lines when they are too different).
/// </summary>
[TestClass]
public class NoteComparerTests
{
    private static readonly Guid Equipment = Guid.NewGuid();
    private static readonly Guid Restored = Guid.NewGuid();
    private static readonly Guid Status = Guid.NewGuid();

    private static HistoryNames Names()
    {
        var names = new HistoryNames();
        names.Folders[Equipment] = "Home\\Equipment";
        names.Folders[Restored] = "Home\\Equipment\\Restored";
        names.Attributes[Status] = "Status";
        return names;
    }

    private static NoteVersionDto Version(string topic = "CPC 464", string description = "Text", Guid? folder = null, string status = null) => new()
    {
        Topic = topic, Description = description, FolderId = folder ?? Equipment, ContentType = "markdown", Tags = "",
        Attributes = status == null ? new() : new() { new NoteVersionAttributeDto { Id = Status, Value = status } }
    };

    [TestMethod]
    public void TheFieldsThatDiffer_WithTheirNames()
    {
        var comparison = NoteComparer.Compare(Version(status: "Restoring"), Version(topic: "Amstrad CPC 464", folder: Restored, status: "Works"), Names());

        CollectionAssert.AreEqual(new[] { "Topic", "Folder", "Status" }, comparison.Fields.Select(f => f.Name).ToArray());
        var folder = comparison.Fields.Single(f => f.Name == "Folder");
        Assert.AreEqual("Home\\Equipment", folder.Before);
        Assert.AreEqual("Home\\Equipment\\Restored", folder.After);
        Assert.AreEqual("Works", comparison.Fields.Single(f => f.Name == "Status").After);
        Assert.IsTrue(comparison.Text.Same);
        Assert.IsFalse(comparison.Same);
    }

    [TestMethod]
    public void TheSameVersion_HasNoDifferences()
    {
        var comparison = NoteComparer.Compare(Version(description: "A\r\nB  "), Version(description: "A\nB"), Names());

        Assert.IsTrue(comparison.Same, "CR LF and the spaces at the end of a line don't count");
        Assert.AreEqual(0, comparison.Text.Lines.Count);
    }

    [TestMethod]
    public void ANewNote_HasEverythingAdded()
    {
        var comparison = NoteComparer.Compare(null, Version(description: "One\nTwo"), Names());

        Assert.IsNull(comparison.Fields.Single(f => f.Name == "Topic").Before);
        Assert.AreEqual("Home\\Equipment", comparison.Fields.Single(f => f.Name == "Folder").After);
        Assert.AreEqual(2, comparison.Text.Added);
        Assert.IsTrue(comparison.Text.Lines.All(l => l.Kind == EnumComparisonLine.Added));
    }

    [TestMethod]
    public void TheScript_IsNotShown()
    {
        var before = Version();
        before.Script = "secret = 1";
        var after = Version();
        after.Script = "secret = 2";

        var field = NoteComparer.Compare(before, after, Names()).Fields.Single();

        Assert.AreEqual("Script", field.Name);
        Assert.IsNull(field.Before);
        Assert.AreEqual(NoteComparer.ScriptChanged, field.After);
    }

    [TestMethod]
    public void TheEqualLinesFarFromAChange_AreFolded()
    {
        var before = string.Join("\n", Enumerable.Range(1, 20).Select(i => $"Line {i}"));
        var after = before.Replace("Line 10", "Line ten");

        var text = NoteComparer.CompareText(before, after);

        // Lines 1-6 folded, 7-9 of context, the change, 11-13 of context, 14-20 folded.
        Assert.AreEqual(EnumComparisonLine.Folded, text.Lines.First().Kind);
        Assert.AreEqual(6, text.Lines.First().Skipped);
        Assert.AreEqual(7, text.Lines.Last().Skipped);
        var removed = text.Lines.Single(l => l.Kind == EnumComparisonLine.Removed);
        var added = text.Lines.Single(l => l.Kind == EnumComparisonLine.Added);
        Assert.AreEqual(10, removed.BeforeNumber);
        Assert.AreEqual(10, added.AfterNumber);
        Assert.AreEqual(3, text.Lines.Count(l => l.Kind == EnumComparisonLine.Equal && l.AfterNumber < 10));
        Assert.AreEqual(1, text.Added);
        Assert.AreEqual(1, text.Removed);
    }

    [TestMethod]
    public void ALineReplacedByALikeOne_MarksOnlyTheWordsThatChanged()
    {
        var text = NoteComparer.CompareText("The computer works with the new power supply.", "The computer works fine with the old power supply.");

        var removed = text.Lines.Single(l => l.Kind == EnumComparisonLine.Removed);
        var added = text.Lines.Single(l => l.Kind == EnumComparisonLine.Added);
        CollectionAssert.AreEqual(new[] { "new" }, removed.Segments.Where(s => s.Changed).Select(s => s.Text).ToArray());
        CollectionAssert.AreEqual(new[] { "fine", "old" }, added.Segments.Where(s => s.Changed).Select(s => s.Text.Trim()).ToArray());
        Assert.AreEqual(added.Text, string.Concat(added.Segments.Select(s => s.Text)), "the segments are the whole line");
    }

    [TestMethod]
    public void ALineReplacedByAVeryDifferentOne_IsMarkedWhole()
    {
        var text = NoteComparer.CompareText("Status of the restoration", "Shopping list: cables, keyboard and a manual");

        Assert.IsTrue(text.Lines.Where(l => l.Kind != EnumComparisonLine.Equal).All(l => l.Segments == null));
    }

    [TestMethod]
    public void ChangedWordsTogether_AreOneMark()
    {
        var words = WordDiff.Compare("one two three four five six", "one TWO THREE four five six");

        CollectionAssert.AreEqual(new[] { "TWO THREE" }, words.After.Where(s => s.Changed).Select(s => s.Text).ToArray());
    }

    [TestMethod]
    public void TooManyLines_AreCut()
    {
        var after = string.Join("\n", Enumerable.Range(1, NoteComparer.MaxLinesShown + 50).Select(i => $"New line {i}"));

        var text = NoteComparer.CompareText("", after);

        Assert.IsTrue(text.Truncated);
        Assert.AreEqual(NoteComparer.MaxLinesShown, text.Lines.Count);
        Assert.AreEqual(NoteComparer.MaxLinesShown + 50, text.Added, "the counts are of the whole text");
    }
}
