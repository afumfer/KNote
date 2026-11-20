using KNote.Model;
using KNote.Model.Core;

namespace KNote.Tests;

/// <summary>
/// TextLineEndings: the texts KNote saves for the AI end their lines with CR LF, whatever the model gave; a form feed (the
/// separator of the pages of a document) is not a line end.
/// </summary>
[TestClass]
public class TextLineEndingsTests
{
    [TestMethod]
    [DataRow("one\ntwo\n\nthree", "one\r\ntwo\r\n\r\nthree")]
    [DataRow("one\r\ntwo\rthree\n", "one\r\ntwo\r\nthree\r\n")]
    [DataRow("one\r\n\ntwo", "one\r\n\r\ntwo")]
    [DataRow("one\r\ntwo", "one\r\ntwo")]
    [DataRow("one line", "one line")]
    public void LineEnds_AreCrLf(string text, string expected)
    {
        Assert.AreEqual(expected, TextLineEndings.ToCrLf(text));
    }

    [TestMethod]
    public void NoText_IsEmpty()
    {
        Assert.AreEqual("", TextLineEndings.ToCrLf(null));
        Assert.AreEqual("", TextLineEndings.ToCrLf(""));
    }

    [TestMethod]
    public void FormFeedsAndOtherSeparators_StayAsTheyAre()
    {
        var separators = $"{(char)0x0C}{(char)0x85}{(char)0x2028}{(char)0x2029}";

        Assert.AreEqual("page 1\r\nend" + separators + "page 2", TextLineEndings.ToCrLf("page 1\nend" + separators + "page 2"));
    }

    [TestMethod]
    public void TheNoteOfATemplate_IsCrLf()
    {
        var text = AtlasTemplates.NoteText(AtlasTemplates.Profile);

        Assert.DoesNotContain("\n", text.Replace("\r\n", ""));
        Assert.Contains("\r\n", text);
    }
}
