using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;

namespace KNote.ClientWin.Tests;

[TestClass]
public class KntServerCOMCtrlTests
{
    private static byte[] Convert(string text) => KntServerCOMCtrl.ConverUtf8StringToClientOSBytes(text);

    // A port name that can't exist, so these tests behave the same on machines with or without COM ports.
    private const string MissingPort = "COM_NOT_THERE";

    private static KntServerCOMCtrl CreateCtrl()
    {
        // No AI providers configured, so the inner assistant is not started (no dialog, no network).
        // SaveSettings() is deliberately not called by these tests: it writes the real KNoteData.config.
        return new KntServerCOMCtrl(new Store(new TestFactoryViews())) { PortName = MissingPort };
    }

    [TestMethod]
    public void StartService_PortNotAvailable_ReturnsFalseWithoutThrowing()
    {
        var ctrl = CreateCtrl();

        Assert.IsFalse(ctrl.StartService());

        Assert.IsFalse(ctrl.RunningService);
        StringAssert.Contains(ctrl.Error, MissingPort);
        StringAssert.Contains(ctrl.StatusInfo, "could not be started");
    }

    [TestMethod]
    public void Run_PortNotAvailable_ControllerStartsWithTheServiceStopped()
    {
        var ctrl = CreateCtrl();

        var result = ctrl.Run();

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(EControllerState.Started, ctrl.ControllerState);
        Assert.IsFalse(ctrl.RunningService);
    }

    [TestMethod]
    public void StopSendAndDispose_ServiceNeverStarted_DoNotThrow()
    {
        var ctrl = CreateCtrl();

        ctrl.StopService();
        ctrl.Send("hello");
        ctrl.Dispose();
    }

    [TestMethod]
    public void SetAiProvider_NullProvider_ReturnsFalse()
    {
        Assert.IsFalse(CreateCtrl().SetAiProvider(null!));
    }

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
