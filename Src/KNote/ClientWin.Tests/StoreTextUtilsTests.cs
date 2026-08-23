using KNote.ClientWin.Core;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Store no longer has TextToDateTime/Int/Double/ExtractUrlFromText/ExtensionFileToFileType/
/// IsSupportedFileTypeForPreview wrapper methods - callers use Store.KntTextUtils.Method(...)
/// directly. These tests cover the KntTextUtils property itself (lazy, singleton per Store
/// instance); the full behavior matrix for each method lives in KntTextUtilsTests.
/// </summary>
[TestClass]
public class StoreTextUtilsTests
{
    private readonly Store _store = new(factoryViews: null!);

    [TestMethod]
    public void KntTextUtils_ReturnsSameInstanceOnEachAccess()
    {
        var first = _store.KntTextUtils;
        var second = _store.KntTextUtils;

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void KntTextUtils_IsIndependentPerStoreInstance()
    {
        var otherStore = new Store(factoryViews: null!);

        Assert.AreNotSame(_store.KntTextUtils, otherStore.KntTextUtils);
    }

    [TestMethod]
    public void KntTextUtils_IsUsableForTextParsing()
    {
        Assert.AreEqual(42, _store.KntTextUtils.TextToInt("42"));
    }
}
