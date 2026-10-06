using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class KntContextMenuStripTests
{
    // KntContextMenuStrip relies on a private WinForms method; if an update of .NET renames it, context menus
    // fall back silently to their 96 DPI layout, so this test is what reports it.
    [TestMethod]
    public void ScaleConstantsMethod_ExistsInThisWinFormsVersion()
    {
        Assert.IsNotNull(KntContextMenuStrip.ScaleConstantsMethod);
    }
}
