using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// NoteInfoDto.GetContentTypeExt: the ContentType of a note in JSON (ContentTypeExt) and in the old plain text format
/// (markdown, html, navigation, with '#' when the description is blocked), which is not even tried as JSON.
/// </summary>
[TestClass]
public class ContentTypeExtTests
{
    [TestMethod]
    [DataRow("markdown", "markdown", false)]
    [DataRow("html", "html", false)]
    [DataRow("navigation#", "navigation", true)]
    public void OldFormat_IsReadAsBefore(string contentType, string forDescription, bool blocked)
    {
        var ext = new NoteInfoDto { ContentType = contentType }.GetContentTypeExt();

        Assert.AreEqual(forDescription, ext.ForDescription);
        Assert.AreEqual("knt", ext.ForScript);
        Assert.AreEqual(blocked, ext.DescriptionBlocked);
        Assert.IsFalse(ContentTypeExtJsonHelper.TryDeserialize(contentType, out _));
    }

    [TestMethod]
    public void Json_IsRead()
    {
        var json = ContentTypeExtJsonHelper.Serialize(new ContentTypeExt { ForDescription = "html", ForScript = "cs", DescriptionBlocked = true });

        var ext = new NoteInfoDto { ContentType = "  " + json }.GetContentTypeExt();

        Assert.AreEqual(new ContentTypeExt { ForDescription = "html", ForScript = "cs", DescriptionBlocked = true }, ext);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("{ not json")]
    public void EmptyOrBrokenJson_IsNotJson(string? contentType)
    {
        Assert.IsFalse(ContentTypeExtJsonHelper.TryDeserialize(contentType!, out var result));
        Assert.IsNull(result);
    }
}
