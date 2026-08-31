using System.Diagnostics;

namespace KNote.ClientWin.Core;

// Interactive counterpart to Store.ExecuteCommand/RunScriptCode: instead of blocking on
// StandardOutput.ReadToEnd() until the process exits, this streams output as it arrives and
// exposes SendInput so a caller can respond to the process's own prompts (input()/readline) while
// it's still running.
//
// Output is pumped as raw character chunks (StreamReader.ReadAsync in a loop), NOT via
// Process.BeginOutputReadLine()/OutputDataReceived - that API only ever delivers complete,
// newline-terminated lines, so a prompt like input("Enter your name: ") (deliberately written
// without a trailing newline, so the cursor stays on the same line) would never reach a consumer
// at the moment it's actually needed - it would sit buffered until more output with a newline
// eventually arrived, or the process exited. Reading raw chunks instead delivers text as soon as
// the process flushes it, newline or not.
//
// OutputReceived/ErrorReceived/Exited fire on whatever thread the underlying Process class uses
// for asynchronous I/O completion and exit notification - never the UI thread - so a consumer
// that touches UI controls from these handlers must marshal itself (see InOutDeviceForm.Print).
//
// Usage: Create(...) writes the temp file and builds the Process without launching it yet -
// subscribe to the events first, then call Start(), so no early output/exit is missed.
public class InteractiveScriptSession : IDisposable
{
    private readonly Process _process;
    private readonly string _tempFile;
    private bool _disposed;
    private bool _started;

    public event EventHandler<string> OutputReceived;
    public event EventHandler<string> ErrorReceived;
    public event EventHandler<int> Exited;

    public bool IsRunning => _started && !_process.HasExited;

    private InteractiveScriptSession(Process process, string tempFile)
    {
        _process = process;
        _tempFile = tempFile;

        _process.Exited += (s, e) => Exited?.Invoke(this, _process.ExitCode);
    }

    // Hack, documented here and where it's used: Node's readline does not release the underlying
    // stdin stream when its interface closes (rl.close()), which is what actually keeps Node's
    // event loop - and so the process - alive, even after the script's own logic already finished
    // (see CloseInput's comment). Since we control the generated temp file, this shim patches every
    // readline.createInterface() call in the script to release process.stdin once its interface
    // closes, so a script that itself calls rl.close() exits on its own, without the user needing
    // CloseInput as a manual workaround.
    //
    // Verified empirically (not just from docs) which method actually does the job for a piped
    // (non-TTY) stdin launched the same way this class launches it: process.stdin.pause() reports
    // isPaused() === true but does NOT let the process exit - the event loop stays alive regardless.
    // process.stdin.unref() does. Only unref() only ever runs reactively, in response to the
    // script's own rl.close(), so it can't cut off a script that's still legitimately busy with
    // something else (a timer, a server, another readline interface, ...). Scripts that read stdin
    // without readline (raw process.stdin.on('data', ...)) aren't covered - CloseInput remains the
    // fallback for those.
    private const string NodeReadlineStdinReleaseShim =
        "(function(){var rl=require('node:readline');var c=rl.createInterface;" +
        "rl.createInterface=function(){var i=c.apply(rl,arguments);" +
        "i.once('close',function(){if(process.stdin&&process.stdin.unref)process.stdin.unref();});" +
        "return i;};})();\n";

    public static InteractiveScriptSession Create(string code, string fileExtension, string runCommandTemplate, string workingDir)
    {
        if (fileExtension == "js")
            code = NodeReadlineStdinReleaseShim + code;

        string nameFile = $"kntTmpCodeFile_{Guid.NewGuid()}.{fileExtension}";
        string tempFullFileName = Path.Combine(workingDir, nameFile);
        File.WriteAllText(tempFullFileName, code);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {string.Format(runCommandTemplate, nameFile)}",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDir
            },
            EnableRaisingEvents = true
        };

        return new InteractiveScriptSession(process, tempFullFileName);
    }

    public void Start()
    {
        if (_started)
            return;
        _started = true;

        _process.Start();

        _ = PumpStreamAsync(_process.StandardOutput, chunk => OutputReceived?.Invoke(this, chunk));
        _ = PumpStreamAsync(_process.StandardError, chunk => ErrorReceived?.Invoke(this, chunk));
    }

    // Reads whatever characters are available as soon as they're available, instead of waiting for
    // a full line - see the class comment. Stdout and stderr are pumped on two independent tasks so
    // one filling its OS pipe buffer can never block the other from being read (the same reason
    // Store.ExecuteCommand's sequential ReadToEnd()+ReadToEnd() is deadlock-prone for chatty output,
    // which this class was written to avoid in the first place).
    private static async Task PumpStreamAsync(StreamReader reader, Action<string> onChunk)
    {
        var buffer = new char[1024];
        try
        {
            int read;
            while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                // Normalize to \r\n regardless of the script's own convention (Python's print()
                // already writes \r\n on Windows; Node's console.log writes a bare \n) - a bare \n
                // doesn't render as a line break in a WinForms multiline TextBox.
                var text = new string(buffer, 0, read).Replace("\r\n", "\n").Replace("\n", "\r\n");
                onChunk(text);
            }
        }
        catch (ObjectDisposedException) { /* session disposed mid-read - process is going away */ }
        catch (IOException) { /* pipe broken by the process exiting - expected at EOF */ }
    }

    public void SendInput(string line)
    {
        if (_disposed || !_started || _process.HasExited)
            return;

        _process.StandardInput.WriteLine(line);
    }

    // Equivalent of pressing Ctrl+Z on a real console: signals end-of-input. Some runtimes need
    // this explicitly to exit on their own - e.g. Node's readline over a redirected (non-TTY)
    // stdin keeps its event loop alive until the pipe is closed, even after the script's own
    // logic (and any rl.close()) has finished; Python's synchronous input() has no such
    // dependency and exits regardless. Safe to call even if the script never reads stdin at all.
    public void CloseInput()
    {
        if (_disposed || !_started || _process.HasExited)
            return;

        _process.StandardInput.Close();
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        try
        {
            if (_started && !_process.HasExited)
                _process.Kill(entireProcessTree: true);
        }
        catch { /* best-effort cleanup */ }

        _process.Dispose();

        try { File.Delete(_tempFile); } catch { /* best-effort cleanup */ }
    }
}
