using System.Text;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Helpers;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Real-process tests for InteractiveScriptSession - launches actual "python"/"node" interpreters,
/// so it's excluded from routine runs on machines without them on PATH. Availability is checked
/// first and reported as Inconclusive when missing, same pattern as StoreRunPyCodeTests/
/// StoreRunJsCodeTests (see ClientWin.Tests/CLAUDE.md).
///
/// OutputReceived/ErrorReceived deliver raw chunks (StreamReader.ReadAsync), not whole lines - so
/// assertions concatenate everything received into one string and check Contains on that, instead
/// of expecting an exact line as one list entry.
/// </summary>
[TestClass]
[TestCategory("RequiresRealScriptRuntime")]
public class InteractiveScriptSessionTests
{
    private static bool IsInterpreterAvailable(string command)
    {
        try
        {
            var (result, error) = TestStoreFactory.CreateEmpty().ExecuteCommand($"{command} --version", Path.GetTempPath());
            return string.IsNullOrEmpty(error);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsPythonAvailable() => IsInterpreterAvailable("python");
    private static bool IsNodeAvailable() => IsInterpreterAvailable("node");
    private static bool IsDotnetAvailable() => IsInterpreterAvailable("dotnet");

    [TestMethod]
    public void Start_NonInteractivePythonScript_ReceivesOutputAndExitsCleanly()
    {
        if (!IsPythonAvailable())
        {
            Assert.Inconclusive("No Python interpreter found on PATH - skipping.");
            return;
        }

        var output = new StringBuilder();
        var exited = new ManualResetEventSlim(false);
        int? exitCode = null;

        using var session = InteractiveScriptSession.Create("print('KNote-interactive-ok')", "py", "python {0}", Path.GetTempPath());
        session.OutputReceived += (s, chunk) => { lock (output) output.Append(chunk); };
        session.Exited += (s, code) => { exitCode = code; exited.Set(); };

        session.Start();

        Assert.IsTrue(exited.Wait(TimeSpan.FromSeconds(15)), "The process did not exit in time.");
        lock (output)
            StringAssert.Contains(output.ToString(), "KNote-interactive-ok");
        Assert.AreEqual(0, exitCode);
    }

    // Regression test for the real bug this fix addresses: input("Enter your name: ") writes its
    // prompt with no trailing newline (by design, so the cursor stays on the same line) - the
    // previous line-based reading (Process.BeginOutputReadLine()/OutputDataReceived) could never
    // deliver that text at the time it was actually needed, since that API only ever fires for
    // complete, newline-terminated lines. Earlier tests in this file used input() with no prompt
    // argument at all, which is exactly why this went unnoticed until a real prompt string surfaced
    // it.
    [TestMethod]
    public void SendInput_PythonScriptWithPromptText_PromptArrivesBeforeInputIsNeeded()
    {
        if (!IsPythonAvailable())
        {
            Assert.Inconclusive("No Python interpreter found on PATH - skipping.");
            return;
        }

        var output = new StringBuilder();
        var promptSeen = new ManualResetEventSlim(false);
        var exited = new ManualResetEventSlim(false);

        var code = "name = input('Enter your name: ')\nprint(f'Hello, {name}!')";
        using var session = InteractiveScriptSession.Create(code, "py", "python {0}", Path.GetTempPath());
        session.OutputReceived += (s, chunk) =>
        {
            lock (output) output.Append(chunk);
            if (output.ToString().Contains("Enter your name: "))
                promptSeen.Set();
        };
        session.Exited += (s, exitCode) => exited.Set();

        session.Start();

        // The whole point: the prompt must show up on its own, before any input is sent - not just
        // eventually, once other output happens to flush it.
        Assert.IsTrue(promptSeen.Wait(TimeSpan.FromSeconds(15)), "The prompt text never arrived before input was sent.");

        session.SendInput("Armando");

        Assert.IsTrue(exited.Wait(TimeSpan.FromSeconds(15)), "The process did not exit in time after receiving input.");
        lock (output)
            StringAssert.Contains(output.ToString(), "Hello, Armando!");
    }

    [TestMethod]
    public void SendInput_PythonScriptWaitingOnInput_UnblocksAndReturnsAnswer()
    {
        if (!IsPythonAvailable())
        {
            Assert.Inconclusive("No Python interpreter found on PATH - skipping.");
            return;
        }

        var output = new StringBuilder();
        var exited = new ManualResetEventSlim(false);

        var code = "name = input()\nprint(f'Hello, {name}!')";
        using var session = InteractiveScriptSession.Create(code, "py", "python {0}", Path.GetTempPath());
        session.OutputReceived += (s, chunk) => { lock (output) output.Append(chunk); };
        session.Exited += (s, exitCode) => exited.Set();

        session.Start();
        session.SendInput("Armando");

        Assert.IsTrue(exited.Wait(TimeSpan.FromSeconds(15)), "The process did not exit in time after receiving input.");
        lock (output)
            StringAssert.Contains(output.ToString(), "Hello, Armando!");
    }

    [TestMethod]
    public void SendInput_CSharpScriptWaitingOnInput_UnblocksAndReturnsAnswer()
    {
        if (!IsDotnetAvailable())
        {
            Assert.Inconclusive("No dotnet SDK found on PATH - skipping.");
            return;
        }

        var output = new StringBuilder();
        var exited = new ManualResetEventSlim(false);

        var code = "var name = Console.ReadLine();\nConsole.WriteLine($\"Hello, {name}!\");";
        using var session = InteractiveScriptSession.Create(code, "cs", "dotnet run {0}", Path.GetTempPath());
        session.OutputReceived += (s, chunk) => { lock (output) output.Append(chunk); };
        session.Exited += (s, exitCode) => exited.Set();

        session.Start();
        session.SendInput("Armando");

        // dotnet run on a file-based app (no .csproj) restores/builds before the first run, which
        // can take noticeably longer than launching python/node - hence the wider timeout.
        // Console.ReadLine() is a synchronous read like Python's input(), so unlike the Node case
        // above the process exits on its own once Main finishes - no CloseInput() needed here
        // either, confirmed by this test rather than assumed from the Python behavior.
        Assert.IsTrue(exited.Wait(TimeSpan.FromSeconds(60)), "The process did not exit in time after receiving input.");
        lock (output)
            StringAssert.Contains(output.ToString(), "Hello, Armando!");
    }

    // Node's readline over a redirected (non-TTY) stdin keeps its event loop alive until the pipe
    // is closed, even after the script's own logic (and rl.close()) finished - unlike Python's
    // synchronous input(), which exits regardless. Confirmed by an isolated repro outside this test
    // (identical System.Diagnostics.Process wiring in a throwaway PowerShell script) before
    // assuming it wasn't specific to this class's own plumbing. Create()'s NodeReadlineStdinRelease
    // shim patches readline.createInterface so the script exits on its own once it calls
    // rl.close() - this test deliberately does NOT call CloseInput(), to verify the shim actually
    // does that job rather than relying on the manual fallback.
    [TestMethod]
    public void SendInput_NodeScriptWaitingOnInput_UnblocksAndExitsOnItsOwn()
    {
        if (!IsNodeAvailable())
        {
            Assert.Inconclusive("No Node.js interpreter found on PATH - skipping.");
            return;
        }

        var output = new StringBuilder();
        var exited = new ManualResetEventSlim(false);

        var code =
            "const readline = require('node:readline');\n" +
            "const rl = readline.createInterface({ input: process.stdin });\n" +
            "rl.on('line', (name) => { console.log(`Hello, ${name}!`); rl.close(); });\n";
        using var session = InteractiveScriptSession.Create(code, "js", "node {0}", Path.GetTempPath());
        session.OutputReceived += (s, chunk) => { lock (output) output.Append(chunk); };
        session.Exited += (s, exitCode) => exited.Set();

        session.Start();
        session.SendInput("Armando");

        Assert.IsTrue(exited.Wait(TimeSpan.FromSeconds(15)), "The process did not exit on its own in time - the readline stdin-release shim did not do its job.");
        lock (output)
            StringAssert.Contains(output.ToString(), "Hello, Armando!");
    }
}
