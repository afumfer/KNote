using KNote.Model;

namespace KNote.Tests;

/// <summary>
/// NoteTextCleaner: the minimal cleaning of the Markdown that goes to the semantic index (KNote Atlas, stage 0).
/// </summary>
[TestClass]
public class NoteTextCleanerTests
{
    [TestMethod]
    public void Clean_NormalizesLineEnds_AndKeepsOneBlankLineAtMost()
    {
        var text = "\r\n\r\nFirst line   \r\nSecond line\r\n\r\n\r\n\r\nThird\r\n\r\n";

        Assert.AreEqual("First line\nSecond line\n\nThird", NoteTextCleaner.Clean(text));
    }

    [TestMethod]
    public void Clean_EmptyOrNull_IsEmpty()
    {
        Assert.AreEqual("", NoteTextCleaner.Clean(null!));
        Assert.AreEqual("", NoteTextCleaner.Clean(" \n\n \n"));
    }

    [TestMethod]
    public void Clean_RemovesHtmlCommentsAndImages_AndKeepsTheAltTextOfMarkdownImages()
    {
        var text = "Before <!-- a comment\nin two lines --> after\n"
            + "<img src=\"x.png\" alt=\"photo\"> Text\n"
            + "![The front of the CPC](cpc-front.jpg) and ![](other.png) end\n"
            + "Inline data:image/png;base64,iVBORw0KGgoAAAANSUhEUg== done";

        var clean = NoteTextCleaner.Clean(text);

        Assert.AreEqual("Before  after\n Text\nThe front of the CPC and  end\nInline  done", clean);
    }

    [TestMethod]
    public void Clean_LongCodeBlock_KeepsItsFirstLines()
    {
        var code = string.Join("\n", Enumerable.Range(1, 40).Select(i => $"line {i}"));
        var text = $"Intro\n```log\n{code}\n```\nAfter";

        var lines = NoteTextCleaner.Clean(text).Split('\n');

        CollectionAssert.AreEqual(
            new[] { "Intro", "```log" }
                .Concat(Enumerable.Range(1, NoteTextCleaner.KeptCodeLines).Select(i => $"line {i}"))
                .Concat(new[] { "\u2026 (25 more lines)", "```", "After" })
                .ToArray(),
            lines);
    }

    [TestMethod]
    public void Clean_ShortCodeBlock_IsKept()
    {
        var code = string.Join("\n", Enumerable.Range(1, NoteTextCleaner.MaxCodeLines).Select(i => $"line {i}"));
        var text = $"~~~\n{code}\n~~~";

        Assert.AreEqual(text, NoteTextCleaner.Clean(text));
    }

    [TestMethod]
    public void Clean_UnclosedLongCodeBlock_IsShortenedToTheEnd()
    {
        var code = string.Join("\n", Enumerable.Range(1, 35).Select(i => $"line {i}"));

        var clean = NoteTextCleaner.Clean($"```\n{code}");

        StringAssert.EndsWith(clean, "line 15\n\u2026 (20 more lines)");
    }

    [TestMethod]
    public void Clean_LongParagraph_IsKept()
    {
        var paragraph = string.Join(" ", Enumerable.Range(1, 300).Select(i => $"word{i}"));

        Assert.AreEqual(paragraph, NoteTextCleaner.Clean(paragraph));
    }

    [TestMethod]
    public void Clean_VeryLongLineOfCode_IsCut()
    {
        var line = string.Join(" ", Enumerable.Range(1, 200).Select(i => $"log{i}"));

        var clean = NoteTextCleaner.Clean($"```\n{line}\n```").Split('\n');

        Assert.AreEqual(NoteTextCleaner.MaxCodeLineLength + 1, clean[1].Length);
        Assert.IsTrue(clean[1].EndsWith("\u2026"));
    }

    [TestMethod]
    public void Clean_VeryLongWord_IsCut()
    {
        var blob = new string('x', 1000);

        var clean = NoteTextCleaner.Clean($"Before {blob} after");

        Assert.AreEqual($"Before {new string('x', NoteTextCleaner.KeptWordLength)}\u2026 after", clean);
    }
}
