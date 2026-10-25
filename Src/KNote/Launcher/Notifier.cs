using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KNote.Launcher;

/// <summary>
/// What the launcher has to say: always written in the data folder's Logs/launcher.log and, when it is a problem, also
/// shown with the system's own notice (a message box on Windows, osascript on macOS, notify-send on Linux). The launcher
/// has no window of its own.
/// </summary>
public sealed class Notifier
{
    private readonly string _logFile;

    public Notifier(string logsFolder)
    {
        _logFile = Path.Combine(logsFolder, "launcher.log");
    }

    public void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logFile)!);
            File.AppendAllText(_logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
        }
        catch (IOException)
        {
            // Without a log, the notice is still shown.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    public void Problem(string message)
    {
        Log(message);
        try
        {
            if (OperatingSystem.IsWindows())
                MessageBox(IntPtr.Zero, message, "KNote", MessageBoxIconWarning);
            else if (OperatingSystem.IsMacOS())
                Run("osascript", "-e", $"display alert \"KNote\" message \"{message.Replace("\\", "\\\\").Replace("\"", "\\\"")}\" as warning");
            else
                Run("notify-send", "--urgency=critical", "KNote", message);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            Log($"The notice could not be shown: {ex.Message}");
        }
    }

    private static void Run(string program, params string[] arguments)
    {
        var start = new ProcessStartInfo(program) { UseShellExecute = false };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);
        Process.Start(start)?.WaitForExit(60000);
    }

    private const uint MessageBoxIconWarning = 0x30;

    [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr window, string text, string caption, uint type);
}
