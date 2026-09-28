using KNote.ClientWin.Core;
using System.Globalization;

namespace KNote.ClientWin.Tests;

[TestClass]
public class KntTextUtilsTests
{
    private readonly KntTextUtils KntTextUtils = new();

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
        var numberText = (3.14).ToString(CultureInfo.CurrentCulture);

        Assert.AreEqual(3.14, KntTextUtils.TextToDouble(numberText));
    }

    // "." and "," are both accepted as the decimal separator, whatever the current culture - in
    // particular "1.5" under es-ES, where "." is the thousands separator and used to be read as 15.
    [TestMethod]
    [DataRow("es-ES", "1,5")]
    [DataRow("es-ES", "1.5")]
    [DataRow("en-US", "1.5")]
    [DataRow("en-US", "1,5")]
    [DataRow("es-ES", " 1.5 ")]
    public void TextToDouble_EitherDecimalSeparator_ReturnsParsedValue(string culture, string text)
    {
        WithCulture(culture, () => Assert.AreEqual(1.5, KntTextUtils.TextToDouble(text)));
    }

    [TestMethod]
    public void TextToDouble_NegativeNumber_ReturnsParsedValue()
    {
        WithCulture("es-ES", () => Assert.AreEqual(-2.25, KntTextUtils.TextToDouble("-2.25")));
    }

    [TestMethod]
    [DataRow("1.500,25")]
    [DataRow("1,500.25")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow(null)]
    [DataRow("not a number")]
    public void TextToDouble_InvalidText_ReturnsNull(string text)
    {
        WithCulture("es-ES", () => Assert.IsNull(KntTextUtils.TextToDouble(text)));
    }

    private static void WithCulture(string culture, Action action)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
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
