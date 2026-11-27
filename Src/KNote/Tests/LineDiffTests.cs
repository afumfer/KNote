using KNote.Model.Core;

namespace KNote.Tests;

/// <summary>LineDiff (the history, H12): the differences of two texts, line by line, as NoteChangeRules compares texts.</summary>
[TestClass]
public class LineDiffTests
{
    [TestMethod]
    public void SameText_AsTheEditorsGiveItBack_HasNoChanges()
    {
        var comparison = LineDiff.Compare("One\r\nTwo  \r\n\r\n", "One\nTwo");

        Assert.AreEqual(0, comparison.Added);
        Assert.AreEqual(0, comparison.Removed);
        Assert.IsFalse(comparison.Replaced);
        Assert.IsTrue(comparison.Lines.All(l => l.Kind == LineDiff.Kind.Equal));
    }

    [TestMethod]
    public void ChangedLines_AreRemovedAndAdded_AndTheRestIsEqual()
    {
        var comparison = LineDiff.Compare("a\nb\nc\nd\ne", "a\nB\nc\ne\nf");

        // a, c and e stay; b and d go; B and f come.
        Assert.AreEqual(2, comparison.Added);
        Assert.AreEqual(2, comparison.Removed);
        Assert.AreEqual(3, comparison.Lines.Count(l => l.Kind == LineDiff.Kind.Equal));
        CollectionAssert.AreEqual(new[] { "b", "d" },
            comparison.Lines.Where(l => l.Kind == LineDiff.Kind.Removed).Select(l => comparison.Before[l.BeforeIndex]).ToArray());
        CollectionAssert.AreEqual(new[] { "B", "f" },
            comparison.Lines.Where(l => l.Kind == LineDiff.Kind.Added).Select(l => comparison.After[l.AfterIndex]).ToArray());
    }

    [TestMethod]
    public void TheScript_RebuildsTheTextAfter_FromTheTextBefore()
    {
        var random = new Random(7);
        for (int round = 0; round < 200; round++)
        {
            var before = Enumerable.Range(0, random.Next(0, 30)).Select(_ => "l" + random.Next(0, 8)).ToArray();
            var after = Enumerable.Range(0, random.Next(0, 30)).Select(_ => "l" + random.Next(0, 8)).ToArray();

            var comparison = LineDiff.Compare(before, after);

            var rebuiltAfter = comparison.Lines.Where(l => l.Kind != LineDiff.Kind.Removed)
                .Select(l => l.Kind == LineDiff.Kind.Equal ? before[l.BeforeIndex] : after[l.AfterIndex]).ToArray();
            var rebuiltBefore = comparison.Lines.Where(l => l.Kind != LineDiff.Kind.Added).Select(l => before[l.BeforeIndex]).ToArray();
            CollectionAssert.AreEqual(after, rebuiltAfter, $"round {round}");
            CollectionAssert.AreEqual(before, rebuiltBefore, $"round {round}");
            foreach (var equal in comparison.Lines.Where(l => l.Kind == LineDiff.Kind.Equal))
                Assert.AreEqual(before[equal.BeforeIndex], after[equal.AfterIndex]);
            Assert.IsFalse(comparison.Replaced);
        }
    }

    [TestMethod]
    public void TheScript_IsTheShortest()
    {
        // One line inserted in the middle of many equal ones: one change, not a rewrite.
        var before = Enumerable.Range(0, 500).Select(i => "line " + i).ToArray();
        var after = before.Take(250).Append("new").Concat(before.Skip(250)).ToArray();

        var comparison = LineDiff.Compare(before, after);

        Assert.AreEqual(1, comparison.Added);
        Assert.AreEqual(0, comparison.Removed);
    }

    [TestMethod]
    public void AboveMaxEdits_TheMiddleIsReplaced_KeepingWhatBeginsAndEndsTheSame()
    {
        var before = new[] { "head", "a1", "a2", "a3", "a4", "tail" };
        var after = new[] { "head", "b1", "b2", "b3", "tail" };

        var comparison = LineDiff.Compare(before, after, maxEdits: 2);

        Assert.IsTrue(comparison.Replaced);
        Assert.AreEqual(3, comparison.Added);
        Assert.AreEqual(4, comparison.Removed);
        Assert.AreEqual(LineDiff.Kind.Equal, comparison.Lines.First().Kind);
        Assert.AreEqual(LineDiff.Kind.Equal, comparison.Lines.Last().Kind);
    }

    [TestMethod]
    public void EmptyTexts()
    {
        Assert.AreEqual(2, LineDiff.Compare(null, "a\nb").Added);
        Assert.AreEqual(2, LineDiff.Compare("a\nb", "").Removed);
        Assert.AreEqual(0, LineDiff.Compare(null, "  \r\n").Lines.Count);
    }
}
