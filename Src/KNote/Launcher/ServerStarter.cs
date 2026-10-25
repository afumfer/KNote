using System.Diagnostics;

namespace KNote.Launcher;

/// <summary>
/// Starts the local server: dotnet KNote.Server.dll, hidden, in the Local environment and with the data folder, with its
/// own folder as the working folder. It keeps running when the launcher ends.
/// </summary>
public static class ServerStarter
{
    public const string ServerDll = "KNote.Server.dll";

    /// <summary>
    /// --server, or server/KNote.Server.dll next to the launcher (as installed), or, run from its build folder
    /// (Launcher/bin/Debug/net10.0), the Server's build of the same configuration. Null if none exists.
    /// </summary>
    public static string? FindServer(string? serverPath, string baseDirectory)
    {
        if (!string.IsNullOrWhiteSpace(serverPath))
            return File.Exists(serverPath) ? Path.GetFullPath(serverPath) : null;

        var installed = Path.Combine(baseDirectory, "server", ServerDll);
        if (File.Exists(installed))
            return installed;

        var build = new DirectoryInfo(baseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var sources = build.Parent?.Parent?.Parent?.Parent;
        if (sources != null && build.Parent != null)
        {
            var development = Path.Combine(sources.FullName, "Server", "bin", build.Parent.Name, build.Name, ServerDll);
            if (File.Exists(development))
                return development;
        }
        return null;
    }

    /// <summary>
    /// The dotnet that runs the server: the one running the launcher (dotnet KNote.Launcher.dll), DOTNET_ROOT, the PATH or
    /// where the installers of each system put it. Null if there is none (.NET isn't installed).
    /// </summary>
    public static string? FindDotnet(Func<string, string?> environment, Func<string, bool> fileExists, string? processPath, bool isWindows)
    {
        var name = isWindows ? "dotnet.exe" : "dotnet";
        if (processPath != null && string.Equals(Path.GetFileName(processPath), name, StringComparison.OrdinalIgnoreCase))
            return processPath;

        var candidates = new List<string>();
        if (environment("DOTNET_ROOT") is { Length: > 0 } root)
            candidates.Add(Path.Combine(root, name));
        foreach (var folder in (environment("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            candidates.Add(Path.Combine(folder.Trim('"'), name));
        if (isWindows)
        {
            if (environment("ProgramFiles") is { Length: > 0 } programFiles)
                candidates.Add(Path.Combine(programFiles, "dotnet", name));
        }
        else
        {
            candidates.AddRange(["/usr/local/share/dotnet/dotnet", "/opt/homebrew/opt/dotnet/libexec/dotnet", "/usr/share/dotnet/dotnet",
                "/usr/lib/dotnet/dotnet", "/usr/local/bin/dotnet", "/snap/bin/dotnet"]);
            if (environment("HOME") is { Length: > 0 } home)
                candidates.Add(Path.Combine(home, ".dotnet", "dotnet"));
        }
        return candidates.FirstOrDefault(fileExists);
    }

    /// <summary>
    /// What it needs, besides dotnet: the .NET Runtime 10 (the launcher, a console application, needs it too) and the
    /// ASP.NET Core Runtime 10 (the server). On Windows the ASP.NET Core Runtime's installer doesn't bring the .NET
    /// Runtime: the Hosting Bundle has both. listRuntimes: what dotnet --list-runtimes says.
    /// </summary>
    public static bool HasRuntimes(string listRuntimes) =>
        listRuntimes.Split('\n').Any(l => l.TrimStart().StartsWith("Microsoft.NETCore.App 10.", StringComparison.Ordinal))
        && listRuntimes.Split('\n').Any(l => l.TrimStart().StartsWith("Microsoft.AspNetCore.App 10.", StringComparison.Ordinal));

    public const string RuntimesMissing =
        "KNote needs the .NET Runtime 10 and the ASP.NET Core Runtime 10, free components of Microsoft: on Windows, install the " +
        "ASP.NET Core Hosting Bundle (it has both); on macOS, the .NET 10 SDK; on Linux, the package aspnetcore-runtime-10.0. " +
        "Download them from https://dotnet.microsoft.com/download/dotnet/10.0 and open KNote again.";

    // What dotnet --list-runtimes says; null if it can't be asked (then the server is started anyway).
    public static string? ListRuntimes(string dotnet)
    {
        try
        {
            var start = new ProcessStartInfo(dotnet) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true };
            start.ArgumentList.Add("--list-runtimes");
            using var process = Process.Start(start);
            if (process == null)
                return null;
            var output = process.StandardOutput.ReadToEnd();
            return process.WaitForExit(10000) ? output : null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return null;
        }
    }

    public static Process Start(string dotnet, string serverDll, string dataPath)
    {
        var start = new ProcessStartInfo(dotnet)
        {
            WorkingDirectory = Path.GetDirectoryName(serverDll)!,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        start.ArgumentList.Add(serverDll);
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Local";
        start.Environment["Local__DataPath"] = dataPath;
        return Process.Start(start) ?? throw new InvalidOperationException("The KNote server could not be started.");
    }
}
