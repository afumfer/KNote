using KNote.ClientWin.Core;
using System.Globalization;

namespace KNote.ClientWin.Tests;

[TestClass]
public class KntTextUtilsTests
{
    [TestMethod]
    public void TextToDateTime_ValidDate_ReturnsParsedValue()
    {
        var result = KntTextUtils.TextToDateTime("2026-08-23");

        Assert.IsNotNull(result);
        Assert.AreEqual(new DateTime(2026, 8, 23), result.Value.Date);
    }

    [TestMethod]
    public void TextToDateTime_InvalidText_ReturnsNull()
    {
        Assert.IsNull(KntTextUtils.TextToDateTime("not a date"));
    }

    [TestMethod]
    public void TextToInt_ValidNumber_ReturnsParsedValue()
    {
        Assert.AreEqual(42, KntTextUtils.TextToInt("42"));
    }

    [TestMethod]
    public void TextToInt_InvalidText_ReturnsZero()
    {
        Assert.AreEqual(0, KntTextUtils.TextToInt("not a number"));
    }

    [TestMethod]
    public void TextToDouble_ValidNumber_ReturnsParsedValue()
    {
        // TextToDouble uses double.TryParse with the current thread culture (not InvariantCulture),
        // so the input must be formatted for CurrentCulture, e.g. "3,14" under es-ES. This is
        // characterizing existing behavior, not asserting it is the desired one.
        var numberText = (3.14).ToString(CultureInfo.CurrentCulture);

        Assert.AreEqual(3.14, KntTextUtils.TextToDouble(numberText));
    }

    [TestMethod]
    public void TextToDouble_InvalidText_ReturnsNull()
    {
        Assert.IsNull(KntTextUtils.TextToDouble("not a number"));
    }

    [TestMethod]
    public void ExtractUrlFromText_SingleLineHttpUrl_ReturnsUrl()
    {
        Assert.AreEqual("https://example.com", KntTextUtils.ExtractUrlFromText("https://example.com"));
    }

    [TestMethod]
    public void ExtractUrlFromText_UrlOnFirstLineOfMultilineText_ReturnsFirstLine()
    {
        var result = KntTextUtils.ExtractUrlFromText("https://example.com\nsome other text");

        Assert.AreEqual("https://example.com", result);
    }

    [TestMethod]
    public void ExtractUrlFromText_PlainText_ReturnsNull()
    {
        Assert.IsNull(KntTextUtils.ExtractUrlFromText("this is just a note"));
    }

    [TestMethod]
    public void ExtractUrlFromText_EmptyText_ReturnsNull()
    {
        Assert.IsNull(KntTextUtils.ExtractUrlFromText(""));
    }

    [TestMethod]
    [DataRow(".jpg", "image/jpeg")]
    [DataRow(".jpeg", "image/jpeg")]
    [DataRow(".png", "image/png")]
    [DataRow(".pdf", "application/pdf")]
    [DataRow(".mp4", "video/mp4")]
    [DataRow(".mp3", "audio/mp3")]
    [DataRow(".txt", "text/plain")]
    [DataRow(".unknown", "")]
    public void ExtensionFileToFileType_KnownAndUnknownExtensions_ReturnsExpectedMimeType(string extension, string expectedMimeType)
    {
        Assert.AreEqual(expectedMimeType, KntTextUtils.ExtensionFileToFileType(extension));
    }

    [TestMethod]
    public void ExtensionFileToFileType_IsCaseInsensitive()
    {
        Assert.AreEqual("application/pdf", KntTextUtils.ExtensionFileToFileType(".PDF"));
    }

    [TestMethod]
    [DataRow("image/jpeg", true)]
    [DataRow("application/pdf", true)]
    [DataRow("application/unsupported", false)]
    [DataRow("", false)]
    [DataRow(null, false)]
    public void IsSupportedFileTypeForPreview_ReturnsExpectedResult(string? fileType, bool expected)
    {
        Assert.AreEqual(expected, KntTextUtils.IsSupportedFileTypeForPreview(fileType));
    }
}
