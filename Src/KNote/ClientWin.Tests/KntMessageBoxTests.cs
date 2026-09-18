using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class KntMessageBoxTests
{
    [TestMethod]
    public void BuildOptions_NoTopMostWindow_LeavesOptionsUntouched()
    {
        Assert.AreEqual((MessageBoxOptions)0, KntMessageBox.BuildOptions(false));
    }

    [TestMethod]
    public void BuildOptions_TopMostWindowVisible_AddsMbTopMostAndTaskModal()
    {
        Assert.AreEqual((MessageBoxOptions)0x00042000, KntMessageBox.BuildOptions(true));
    }
}
