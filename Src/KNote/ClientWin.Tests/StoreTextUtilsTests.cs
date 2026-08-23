using KNote.ClientWin.Core;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Smoke tests confirming Store still delegates correctly to KntTextUtils (Fase 1 of the
/// ClientWin refactor plan). The full behavior matrix for each method lives in
/// KntTextUtilsTests, tested directly against the extracted static class.
/// </summary>
[TestClass]
public class StoreTextUtilsTests
{
    private readonly Store _store = new(factoryViews: null!);

    [TestMethod]
    public void TextToDateTime_DelegatesToKntTextUtils()
    {
        Assert.AreEqual(KntTextUtils.TextToDateTime("2026-08-23"), _store.TextToDateTime("2026-08-23"));
    }

    [TestMethod]
    public void TextToInt_DelegatesToKntTextUtils()
    {
        Assert.AreEqual(KntTextUtils.TextToInt("42"), _store.TextToInt("42"));
    }

    [TestMethod]
    public void TextToDouble_DelegatesToKntTextUtils()
    {
        Assert.AreEqual(KntTextUtils.TextToDouble("not a number"), _store.TextToDouble("not a number"));
    }

    [TestMethod]
    public void ExtractUrlFromText_DelegatesToKntTextUtils()
    {
        Assert.AreEqual(
            KntTextUtils.ExtractUrlFromText("https://example.com"),
            _store.ExtractUrlFromText("https://example.com"));
    }

    [TestMethod]
    public void ExtensionFileToFileType_DelegatesToKntTextUtils()
    {
        Assert.AreEqual(KntTextUtils.ExtensionFileToFileType(".jpg"), _store.ExtensionFileToFileType(".jpg"));
    }

    [TestMethod]
    public void IsSupportedFileTypeForPreview_DelegatesToKntTextUtils()
    {
        Assert.AreEqual(
            KntTextUtils.IsSupportedFileTypeForPreview("image/jpeg"),
            _store.IsSupportedFileTypeForPreview("image/jpeg"));
    }
}
