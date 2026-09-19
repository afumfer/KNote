using KNote.ClientWin.Controllers;

namespace KNote.ClientWin.Tests;

[TestClass]
public class KntServerCOMCtrlTests
{
    private static byte[] Convert(string text) => KntServerCOMCtrl.ConverUtf8StringToClientOSBytes(text);

    [TestMethod]
    public void Convert_CrLf_BecomesSingleLf()
    {
        CollectionAssert.AreEqual(new byte[] { 65, 10, 66 }, Convert("A\r\nB"));
    }

    [TestMethod]
    public void Convert_LoneCrBeforeText_BecomesLf()
    {
        CollectionAssert.AreEqual(new byte[] { 65, 10, 66 }, Convert("A\rB"));
    }

    [TestMethod]
    public void Convert_TrailingCr_DoesNotThrowAndBecomesLf()
    {
        CollectionAssert.AreEqual(new byte[] { 65, 10 }, Convert("A\r"));
    }

    [TestMethod]
    public void Convert_CrCrLf_BecomesTwoLf()
    {
        CollectionAssert.AreEqual(new byte[] { 10, 10 }, Convert("\r\r\n"));
    }

    [TestMethod]
    public void Convert_QdosCharacterSet_MapsSpecialCharacters()
    {
        // '£' -> 96, 'ñ' -> 137 in the QDOS table; plain ASCII passes through.
        CollectionAssert.AreEqual(new byte[] { 96, 137, 97 }, Convert("£ña"));
    }

    [TestMethod]
    public void Convert_EmptyString_ReturnsEmpty()
    {
        Assert.AreEqual(0, Convert("").Length);
    }
}
