using System.Diagnostics;

namespace KNote.Launcher;

/// <summary>
/// Opens KNote in the user's default browser (as ClientWin opens its help: the shell on Windows, open on macOS, xdg-open
/// on Linux) or, with --app-window, in an app window of Edge or Chrome (--app=url), if one of them is installed.
/// </summary>
public static class BrowserOpener
{
    public static void Open(Uri url, bool appWindow)
    {
        if (appWindow && FindAppBrowser() is { } browser)
        {
            var start = new ProcessStartInfo(browser) { UseShellExecute = false };
            start.ArgumentList.Add($"--app={url}");
            Process.Start(start);
            return;
        }

        if (OperatingSystem.IsWindows())
            Process.Start(new ProcessStartInfo(url.ToString()) { UseShellExecute = true });
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", url.ToString());
        else
            Process.Start("xdg-open", url.ToString());
    }

    private static string? FindAppBrowser()
    {
        IEnumerable<string> candidates;
        if (OperatingSystem.IsWindows())
        {
            var folders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            }.Where(f => f.Length > 0);
            candidates = folders.SelectMany(f => new[]
            {
                Path.Combine(f, "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(f, "Google", "Chrome", "Application", "chrome.exe")
            });
        }
        else if (OperatingSystem.IsMacOS())
            candidates = ["/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge", "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"];
        else
            candidates = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(f => new[] { "microsoft-edge", "google-chrome", "google-chrome-stable", "chromium", "chromium-browser" }.Select(n => Path.Combine(f, n)));

        return candidates.FirstOrDefault(File.Exists);
    }
}
