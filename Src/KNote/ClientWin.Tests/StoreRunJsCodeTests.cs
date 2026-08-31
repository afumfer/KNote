using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Helpers;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Real-process smoke tests for Store.RunJsCode - launches an actual "node" interpreter, so it's
/// excluded from routine runs on machines without Node.js on PATH. Availability is checked first and
/// reported as Inconclusive when missing, distinct from an actual regression - same pattern as
/// StoreRunPyCodeTests / the Ollama smoke tests (see ClientWin.Tests/CLAUDE.md).
/// </summary>
[TestClass]
[TestCategory("RequiresRealScriptRuntime")]
public class StoreRunJsCodeTests
{
    private static bool IsNodeAvailable()
    {
        try
        {
            var (result, error) = TestStoreFactory.CreateEmpty().ExecuteCommand("node --version", Path.GetTempPath());
            return string.IsNullOrEmpty(error);
        }
        catch
        {
            return false;
        }
    }

    [TestMethod]
    public void RunJsCode_ReturnsStdoutFromInterpreter()
    {
        if (!IsNodeAvailable())
        {
            Assert.Inconclusive("No Node.js interpreter found on PATH - skipping RunJsCode smoke test.");
            return;
        }

        var store = TestStoreFactory.CreateEmpty();

        var (result, error) = store.RunJsCode("console.log('KNote-js-ok')", redirectStandardOut: true);

        Assert.IsTrue(string.IsNullOrEmpty(error), $"Expected no error from the Node.js interpreter, got: {error}");
        Assert.IsTrue(result.Contains("KNote-js-ok"), $"Expected stdout to contain the marker, got: {result}");
    }
}
