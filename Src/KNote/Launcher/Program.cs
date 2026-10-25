using System.Diagnostics;
using KNote.Launcher;

// KNote Local's launcher (see CLAUDE.md): starts the user's local server if it isn't running, waits until it answers and
// opens the browser in KNote. With --stop, stops it. Its problems are shown with the system's notice and logged in the
// data folder (Logs/launcher.log).

var options = LauncherOptions.Parse(args);
var instance = LocalInstance.Resolve(options.DataPath);
var notifier = new Notifier(instance.LogsFolder);

if (options.Error != null)
{
    notifier.Problem(options.Error);
    return 2;
}

try
{
    using var http = new HttpClient();
    var launcher = new KntLauncher(instance, notifier, http);

    if (options.Stop)
        return await launcher.StopAsync();

    if (options.Port is int port && !launcher.ChangePort(port))
        return 1;

    return await launcher.LaunchAsync(options);
}
catch (Exception ex)
{
    notifier.Problem($"KNote could not be started: {ex.Message}");
    return 1;
}

namespace KNote.Launcher
{
    public sealed class KntLauncher
    {
        private static readonly TimeSpan StartTimeout = TimeSpan.FromSeconds(90);
        private static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(15);

        private readonly LocalInstance _instance;
        private readonly Notifier _notifier;
        private readonly HttpClient _http;

        public KntLauncher(LocalInstance instance, Notifier notifier, HttpClient http)
        {
            _instance = instance;
            _notifier = notifier;
            _http = http;
        }

        public async Task<int> LaunchAsync(LauncherOptions options)
        {
            var state = _instance.HasConfig ? await CheckAsync() : HealthState.NotRunning;

            if (state == HealthState.NotRunning && !IsServerProcessAlive())
            {
                var serverDll = ServerStarter.FindServer(options.ServerPath, AppContext.BaseDirectory);
                if (serverDll == null)
                    return Problem("The KNote server was not found next to the launcher (server/KNote.Server.dll). Install KNote again.");

                var dotnet = ServerStarter.FindDotnet(Environment.GetEnvironmentVariable, File.Exists, Environment.ProcessPath, OperatingSystem.IsWindows());
                if (dotnet == null || ServerStarter.ListRuntimes(dotnet) is { } runtimes && !ServerStarter.HasRuntimes(runtimes))
                    return Problem(ServerStarter.RuntimesMissing);

                _notifier.Log($"Starting the server {serverDll} (data folder {_instance.Path}).");
                using var server = ServerStarter.Start(dotnet, serverDll, _instance.Path);
                state = await WaitUntilRunningAsync(server);
            }
            else if (state == HealthState.NotRunning)
                state = await WaitUntilRunningAsync(null);

            if (state is HealthState.OtherKNote or HealthState.OtherProgram)
                return PortInUse(state);
            if (state != HealthState.ThisKNote)
                return Problem($"The KNote server didn't start. The details are in its logs: {_instance.LogsFolder}");

            if (!options.NoBrowser)
                BrowserOpener.Open(_instance.AppUrl, options.AppWindow);
            _notifier.Log($"KNote is running at {_instance.AppUrl}.");
            return 0;
        }

        /// <summary>
        /// Asks the server to stop with its control token; if it doesn't stop in time, ends its process. Nothing to do if
        /// it isn't running.
        /// </summary>
        public async Task<int> StopAsync()
        {
            if (!_instance.HasConfig)
                return 0;

            if (_instance.ReadControlToken() is { } token)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_instance.AppUrl, "local/stop"));
                    request.Headers.Add("X-KNote-Control", token);
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    using var response = await _http.SendAsync(request, timeout.Token);
                    _notifier.Log($"Stop asked to the server: {(int)response.StatusCode}.");
                }
                catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
                {
                    _notifier.Log($"The server didn't answer the stop: {ex.Message}");
                }
            }

            var deadline = DateTime.UtcNow + StopTimeout;
            while (IsServerProcessAlive() && DateTime.UtcNow < deadline)
                await Task.Delay(250);

            if (IsServerProcessAlive() && ServerProcess() is { } process)
            {
                _notifier.Log($"The server (PID {process.Id}) didn't stop: its process is ended.");
                process.Kill();
                process.WaitForExit(5000);
                TryDelete(_instance.PidFile);
                TryDelete(_instance.ControlTokenFile);
            }
            return 0;
        }

        // --port: only for an installation already created (the first time, the server chooses a free one by itself).
        public bool ChangePort(int port)
        {
            if (!_instance.HasConfig)
            {
                Problem("KNote hasn't been started yet: open it once without --port (it chooses a free port by itself).");
                return false;
            }
            _instance.WritePort(port);
            _notifier.Log($"Port changed to {port}.");
            if (IsServerProcessAlive())
                _notifier.Problem($"The port {port} applies the next time KNote starts: stop it first (KNote.Launcher --stop).");
            return true;
        }

        private async Task<HealthState> CheckAsync()
        {
            var (_, instanceId) = _instance.ReadConfig();
            return await ServerHealth.CheckAsync(_http, _instance.AppUrl, instanceId);
        }

        // Until it answers, says the port is someone else's, or its process ends (another launcher may have started it:
        // then there is no process of ours, and the PID file says whether it is still starting).
        private async Task<HealthState> WaitUntilRunningAsync(Process? server)
        {
            var deadline = DateTime.UtcNow + StartTimeout;
            while (DateTime.UtcNow < deadline)
            {
                // The first time, the server creates the configuration (and its port) when it starts.
                if (_instance.HasConfig)
                {
                    var state = await CheckAsync();
                    if (state != HealthState.NotRunning)
                        return state;
                }

                if (server != null ? server.HasExited : !IsServerProcessAlive())
                    return HealthState.NotRunning;
                await Task.Delay(300);
            }
            return HealthState.NotRunning;
        }

        // The server of the PID file, if it is still running (a PID can be reused by another program: only dotnet or KNote).
        private Process? ServerProcess()
        {
            if (_instance.ReadPid() is not int pid)
                return null;
            try
            {
                var process = Process.GetProcessById(pid);
                var name = process.ProcessName;
                return !process.HasExited && (name.StartsWith("dotnet", StringComparison.OrdinalIgnoreCase) || name.Contains("KNote", StringComparison.OrdinalIgnoreCase))
                    ? process
                    : null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private bool IsServerProcessAlive() => ServerProcess() != null;

        private int PortInUse(HealthState state)
        {
            var port = _instance.ReadConfig().ServerUrl.Port;
            var who = state == HealthState.OtherKNote ? "the KNote of another user of this computer" : "another program";
            return Problem($"The port {port} of this computer is used by {who}. Open KNote with another port, for example: KNote.Launcher --port {port + 1}");
        }

        private int Problem(string message)
        {
            _notifier.Problem(message);
            return 1;
        }

        private static void TryDelete(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException)
            {
            }
        }
    }
}
