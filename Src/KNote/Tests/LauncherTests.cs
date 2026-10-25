using KNote.Launcher;

namespace KNote.Tests;

/// <summary>
/// KNote Local's launcher without starting anything: its arguments, what it reads from the data folder (where the server
/// listens, the installation's id), changing the port, who answers the health check, and where it finds dotnet and the
/// server.
/// </summary>
[TestClass]
public class LauncherTests
{
    private string _path = null!;

    [TestInitialize]
    public void Initialize()
    {
        _path = Path.Combine(Path.GetTempPath(), $"knote-launcher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_path);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_path, true);

    [TestMethod]
    public void Options_TheValidOnes_AndWhyNot()
    {
        var options = LauncherOptions.Parse(["--no-browser", "--app-window", "--port", "6001", "--data", "D:/knote"]);

        Assert.IsNull(options.Error);
        Assert.IsTrue(options.NoBrowser && options.AppWindow && !options.Stop);
        Assert.AreEqual(6001, options.Port);
        Assert.AreEqual("D:/knote", options.DataPath);
        Assert.IsTrue(LauncherOptions.Parse(["--stop"]).Stop);
        StringAssert.Contains(LauncherOptions.Parse(["--port", "80"]).Error, "between 1024 and 65535");
        StringAssert.Contains(LauncherOptions.Parse(["--port"]).Error, "--port");
        StringAssert.Contains(LauncherOptions.Parse(["--what"]).Error, "Unknown argument");
    }

    [TestMethod]
    public void Config_WhereTheServerListens_AndTheInstallationsId()
    {
        File.WriteAllText(Path.Combine(_path, LocalInstance.ConfigFileName),
            """{ "Local": { "InstanceId": "abc" }, "Kestrel": { "Endpoints": { "Http": { "Url": "http://127.0.0.1:5290" } } } }""");
        var instance = new LocalInstance(_path);

        var (url, instanceId) = instance.ReadConfig();

        Assert.AreEqual(new Uri("http://127.0.0.1:5290/"), url);
        Assert.AreEqual("abc", instanceId);
        Assert.AreEqual(new Uri("http://127.0.0.1:5290/KNote/"), instance.AppUrl);
    }

    [TestMethod]
    [DataRow("http://0.0.0.0:5300", "http://127.0.0.1:5300/")]
    [DataRow("http://*:5300", "http://127.0.0.1:5300/")]
    [DataRow("http://+:5300", "http://127.0.0.1:5300/")]
    [DataRow("http://localhost:5300", "http://localhost:5300/")]
    [DataRow(null, "http://127.0.0.1:5280/")]
    public void Config_AnAddressOfEveryInterface_IsReachedAt127001(string? url, string expected)
    {
        Assert.AreEqual(new Uri(expected), LocalInstance.ToReachableUrl(url));
    }

    [TestMethod]
    public void WritePort_ChangesOnlyThePort()
    {
        File.WriteAllText(Path.Combine(_path, LocalInstance.ConfigFileName),
            """{ "AppSettings": { "Secret": "protected:x" }, "Kestrel": { "Endpoints": { "Http": { "Url": "http://127.0.0.1:5280" } } } }""");
        var instance = new LocalInstance(_path);

        instance.WritePort(6100);

        Assert.AreEqual(6100, instance.ReadConfig().ServerUrl.Port);
        StringAssert.Contains(File.ReadAllText(instance.ConfigFile), "protected:x");
    }

    [TestMethod]
    [DataRow("""{"app":"KNote","instanceId":"abc"}""", "abc", HealthState.ThisKNote)]
    [DataRow("""{"app":"KNote","instanceId":"other"}""", "abc", HealthState.OtherKNote)]
    [DataRow("""{"app":"KNote","instanceId":"abc"}""", null, HealthState.ThisKNote)]
    [DataRow("""{"status":"ok"}""", "abc", HealthState.OtherProgram)]
    [DataRow("<html>Hello</html>", "abc", HealthState.OtherProgram)]
    public void Health_WhoAnswersInThePort(string body, string? instanceId, HealthState expected)
    {
        Assert.AreEqual(expected, ServerHealth.Interpret(body, instanceId));
    }

    [TestMethod]
    public async Task Health_NothingInThePort_NotRunning()
    {
        var port = KNote.Server.Local.KntLocalData.FindFreePort(52000);
        using var http = new HttpClient();

        Assert.AreEqual(HealthState.NotRunning, await ServerHealth.CheckAsync(http, new Uri($"http://127.0.0.1:{port}/KNote/"), "abc"));
    }

    [TestMethod]
    public void Dotnet_TheOneRunningTheLauncher_ThenDotnetRoot_ThePath_AndTheUsualPlaces()
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var env = new Dictionary<string, string?> { ["PATH"] = string.Join(Path.PathSeparator, "C:/tools", "C:/dotnet-path"), ["ProgramFiles"] = "C:/Program Files" };
        string? Find(string? processPath = null) => ServerStarter.FindDotnet(n => env.GetValueOrDefault(n), files.Contains, processPath, isWindows: true);

        Assert.AreEqual("C:/x/dotnet.exe", Find("C:/x/dotnet.exe"), "Run as dotnet KNote.Launcher.dll.");
        Assert.IsNull(Find("C:/KNote/KNote.Launcher.exe"), "Not installed.");

        files.Add(Path.Combine("C:/Program Files", "dotnet", "dotnet.exe"));
        Assert.AreEqual(Path.Combine("C:/Program Files", "dotnet", "dotnet.exe"), Find());
        files.Add(Path.Combine("C:/dotnet-path", "dotnet.exe"));
        Assert.AreEqual(Path.Combine("C:/dotnet-path", "dotnet.exe"), Find());
        env["DOTNET_ROOT"] = "D:/dotnet";
        files.Add(Path.Combine("D:/dotnet", "dotnet.exe"));
        Assert.AreEqual(Path.Combine("D:/dotnet", "dotnet.exe"), Find());
    }

    [TestMethod]
    public void Dotnet_OnMacAndLinux_WhereTheirInstallersPutIt()
    {
        var env = new Dictionary<string, string?> { ["HOME"] = "/home/me" };

        Assert.AreEqual("/usr/local/share/dotnet/dotnet", ServerStarter.FindDotnet(n => env.GetValueOrDefault(n), f => f == "/usr/local/share/dotnet/dotnet", null, isWindows: false));
        Assert.AreEqual(Path.Combine("/home/me", ".dotnet", "dotnet"), ServerStarter.FindDotnet(n => env.GetValueOrDefault(n), f => f == Path.Combine("/home/me", ".dotnet", "dotnet"), null, isWindows: false));
    }

    [TestMethod]
    public void Runtimes_TheDotNetRuntime_AndTheAspNetCoreRuntime_Both10()
    {
        const string both = "Microsoft.AspNetCore.App 10.0.14 [C:\\Program Files\\dotnet\\shared\\Microsoft.AspNetCore.App]\r\n" +
                            "Microsoft.NETCore.App 10.0.14 [C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App]\r\n";
        const string onlyAspNetCore = "Microsoft.AspNetCore.App 10.0.14 [C:\\Program Files\\dotnet\\shared\\Microsoft.AspNetCore.App]\r\n";
        const string older = "Microsoft.AspNetCore.App 9.0.10 [x]\nMicrosoft.NETCore.App 9.0.10 [x]\n";

        Assert.IsTrue(ServerStarter.HasRuntimes(both));
        Assert.IsFalse(ServerStarter.HasRuntimes(onlyAspNetCore), "On Windows, the ASP.NET Core Runtime's installer alone.");
        Assert.IsFalse(ServerStarter.HasRuntimes(older));
        Assert.IsFalse(ServerStarter.HasRuntimes(""));
    }

    [TestMethod]
    public void Server_Installed_NextToTheLauncher_OrTheBuildOfTheSameConfiguration()
    {
        var installed = Path.Combine(_path, "app");
        Directory.CreateDirectory(Path.Combine(installed, "server"));
        File.WriteAllText(Path.Combine(installed, "server", ServerStarter.ServerDll), "");
        var launcherBuild = Path.Combine(_path, "src", "Launcher", "bin", "Debug", "net10.0");
        var serverBuild = Path.Combine(_path, "src", "Server", "bin", "Debug", "net10.0");
        Directory.CreateDirectory(launcherBuild);
        Directory.CreateDirectory(serverBuild);
        File.WriteAllText(Path.Combine(serverBuild, ServerStarter.ServerDll), "");

        Assert.AreEqual(Path.Combine(installed, "server", ServerStarter.ServerDll), ServerStarter.FindServer(null, installed));
        Assert.AreEqual(Path.Combine(serverBuild, ServerStarter.ServerDll), ServerStarter.FindServer(null, launcherBuild + Path.DirectorySeparatorChar));
        Assert.IsNull(ServerStarter.FindServer(Path.Combine(_path, "none.dll"), installed), "--server that doesn't exist.");
    }
}
