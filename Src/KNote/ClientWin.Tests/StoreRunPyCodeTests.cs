using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Helpers;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Real-process smoke tests for Store.RunPyCode - launches an actual "python" interpreter, so it's
/// excluded from routine runs (`dotnet test --filter "TestCategory!=RequiresRealAiProvider"` does not
/// exclude this one; use "TestCategory!=RequiresRealScriptRuntime" too) on machines without Python on
/// PATH. Availability is checked first and reported as Inconclusive when missing, distinct from an
/// actual regression - same pattern as the Ollama smoke tests (see ClientWin.Tests/CLAUDE.md).
/// </summary>
[TestClass]
[TestCategory("RequiresRealScriptRuntime")]
public class StoreRunPyCodeTests
{
    private static bool IsPythonAvailable()
    {
        try
        {
            var (result, error) = TestStoreFactory.CreateEmpty().ExecuteCommand("python --version", Path.GetTempPath());
            return string.IsNullOrEmpty(error);
        }
        catch
        {
            return false;
        }
    }

    [TestMethod]
    public void RunPyCode_ReturnsStdoutFromInterpreter()
    {
        if (!IsPythonAvailable())
        {
            Assert.Inconclusive("No Python interpreter found on PATH - skipping RunPyCode smoke test.");
            return;
        }

        var store = TestStoreFactory.CreateEmpty();

        var (result, error) = store.RunPyCode("print('KNote-py-ok')", redirectStandardOut: true);

        Assert.IsTrue(string.IsNullOrEmpty(error), $"Expected no error from the Python interpreter, got: {error}");
        Assert.IsTrue(result.Contains("KNote-py-ok"), $"Expected stdout to contain the marker, got: {result}");
    }
}
