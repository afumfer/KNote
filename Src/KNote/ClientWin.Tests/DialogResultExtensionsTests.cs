using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests;

[TestClass]
public class DialogResultExtensionsTests
{
    [TestMethod]
    [DataRow(DialogResult.OK)]
    [DataRow(DialogResult.Yes)]
    public void ToControllerResult_AffirmativeResults_ReturnExecuted(DialogResult dialogResult)
    {
        Assert.AreEqual(EControllerResult.Executed, dialogResult.ToControllerResult().Entity);
    }

    [TestMethod]
    [DataRow(DialogResult.Cancel)]
    [DataRow(DialogResult.No)]
    [DataRow(DialogResult.None)]
    [DataRow(DialogResult.Abort)]
    public void ToControllerResult_OtherResults_ReturnCanceled(DialogResult dialogResult)
    {
        Assert.AreEqual(EControllerResult.Canceled, dialogResult.ToControllerResult().Entity);
    }
}
