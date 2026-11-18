using KNote.Model;

namespace KNote.Tests;

/// <summary>
/// The cleaning to read a note in the summaries (KNote Atlas, stage 2, 2.3, chapter 28, D7): quoted mails with their header, an
/// original message below, the signature after «-- », huge tables and lists shortened to their size and their first rows; code
/// blocks are left as NoteTextCleaner leaves them; and the words of a text.
/// </summary>
[TestClass]
public class NoteReadingCleanerTests
{
    [TestMethod]
    public void QuotedMails_AndTheirHeader_GoAway()
    {
        var text = "We agree.\n\nOn Mon, 3 Nov 2026, Marta wrote:\n> The budget is 300.\n> Regards\n\nThanks.";

        var clean = NoteReadingCleaner.Clean(text);

        Assert.AreEqual("We agree.\n\n[A quoted message of 2 lines left out]\n\nThanks.", clean);
    }

    [TestMethod]
    public void AnOriginalMessage_GoesAwayToTheEnd()
    {
        var clean = NoteReadingCleaner.Clean("Answer.\n-----Original Message-----\nFrom: x\nSent: y\nText");

        Assert.AreEqual("Answer.\n[An original message of 3 lines left out]", clean);
    }

    [TestMethod]
    public void TheSignature_AfterItsDelimiter_GoesAway()
    {
        Assert.AreEqual("Text of the note.", NoteReadingCleaner.Clean("Text of the note.\n-- \nArmando\nPhone 555"));
        Assert.AreEqual("Title\n--\nText", NoteReadingCleaner.Clean("Title\n--\nText"), "«--» without a space is Markdown");
    }

    [TestMethod]
    public void AHugeTable_KeepsItsHeaderAndFirstRows()
    {
        var rows = string.Join("\n", Enumerable.Range(1, 25).Select(i => $"| {i} | item {i} |"));
        var clean = NoteReadingCleaner.Clean($"Inventory:\n\n| N | Item |\n|---|---|\n{rows}\n\nEnd.");

        StringAssert.Contains(clean, "| 5 | item 5 |");
        Assert.IsFalse(clean.Contains("| 6 | item 6 |"));
        StringAssert.Contains(clean, "[A table of 25 rows: the first 5 shown]");
        StringAssert.EndsWith(clean, "End.");

        var small = "| N |\n|---|\n| 1 |";
        Assert.AreEqual(small, NoteReadingCleaner.Clean(small));
    }

    [TestMethod]
    public void AHugeList_KeepsItsFirstItems()
    {
        var items = string.Join("\n", Enumerable.Range(1, 40).Select(i => $"- item {i}\n  detail {i}"));
        var clean = NoteReadingCleaner.Clean($"Todo:\n{items}");

        StringAssert.Contains(clean, "detail 10");
        Assert.IsFalse(clean.Contains("item 11"));
        StringAssert.Contains(clean, "[A list of 40 items: the first 10 shown]");
    }

    [TestMethod]
    public void InCode_NothingOfThisIsTouched()
    {
        var text = "```\n> not a quote\n-- \n```";

        // NoteTextCleaner trims the ends of the lines, also in code.
        Assert.AreEqual("```\n> not a quote\n--\n```", NoteReadingCleaner.Clean(text));
    }

    [TestMethod]
    public void Words_AreThePiecesWithALetterOrADigit()
    {
        Assert.AreEqual(4, NoteReadingCleaner.CountWords("Buy 2 apples — today!"));
        Assert.AreEqual(0, NoteReadingCleaner.CountWords("  \n "));
    }
}
