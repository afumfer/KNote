using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KNote.Server.Local;

/// <summary>
/// The data folder of KNote Local: the user's configuration (knote-local.json, over the rest of the configuration, with
/// its secrets encrypted), the SQLite databases (Data), the notes' resources (Resources), the encryption keys (Keys), the
/// logs (Logs) and, while the server runs, its PID and control token. Kept apart from the program, so that updating it
/// never touches them, and one for each user of the computer (in their profile).
/// The server is in local mode when there is one: given by Local__DataPath (the launcher sets it) or, without it, the
/// default one in the Local environment (FromEnvironment).
/// </summary>
public class KntLocalData
{
    public const string ConfigFileName = "knote-local.json";
    public const string DataPathVariable = "Local__DataPath";
    public const string LocalEnvironment = "Local";
    public const int DefaultPort = 5280;

    public KntLocalData(string path)
    {
        Path = System.IO.Path.GetFullPath(path);
    }

    public string Path { get; }

    public string ConfigFile => System.IO.Path.Combine(Path, ConfigFileName);

    public string DataFolder => System.IO.Path.Combine(Path, "Data");

    public string ResourcesFolder => System.IO.Path.Combine(Path, "Resources");

    public string KeysFolder => System.IO.Path.Combine(Path, "Keys");

    public string LogsFolder => System.IO.Path.Combine(Path, "Logs");

    public string PidFile => System.IO.Path.Combine(Path, "knote-server.pid");

    public string ControlTokenFile => System.IO.Path.Combine(Path, "knote-server.token");

    // Windows: %LocalAppData%\KNote\Server; macOS: ~/Library/Application Support/KNote/Server; Linux: ~/.local/share/KNote/Server.
    public static string DefaultPath =>
        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KNote", "Server");

    /// <summary>
    /// The data folder, read before the host is built (the logs and the configuration need it first): Local__DataPath,
    /// or the default one in the Local environment; null out of local mode.
    /// </summary>
    public static KntLocalData FromEnvironment()
    {
        var path = Environment.GetEnvironmentVariable(DataPathVariable);
        if (string.IsNullOrWhiteSpace(path) && string.Equals(EnvironmentName, LocalEnvironment, StringComparison.OrdinalIgnoreCase))
            path = DefaultPath;
        return string.IsNullOrWhiteSpace(path) ? null : new KntLocalData(path);
    }

    public static string EnvironmentName =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

    /// <summary>
    /// The folders and, the first time, the configuration: an id of this installation (to tell it from the KNote of
    /// another user of the computer in the same port), a free port (DefaultPort or the next one), a secret of its own to
    /// sign the sessions (random, never one distributed with the program) and a SQLite repository (its database is
    /// created when the server opens it). True if it was created now.
    /// </summary>
    public bool EnsureCreated(KntSecretProtector protector, int startPort = DefaultPort)
    {
        foreach (var folder in new[] { Path, DataFolder, ResourcesFolder, KeysFolder, LogsFolder })
            Directory.CreateDirectory(folder);

        if (File.Exists(ConfigFile))
            return false;

        var config = new JsonObject
        {
            ["Local"] = new JsonObject { ["InstanceId"] = Guid.NewGuid().ToString() },
            ["Kestrel"] = new JsonObject
            {
                ["Endpoints"] = new JsonObject { ["Http"] = new JsonObject { ["Url"] = $"http://127.0.0.1:{FindFreePort(startPort)}" } }
            },
            ["AppSettings"] = new JsonObject { ["Secret"] = protector.Protect(NewSecret()) },
            ["Repositories"] = new JsonArray
            {
                new JsonObject
                {
                    ["Name"] = "Personal repository",
                    ["Orm"] = "EntityFramework",
                    ["Provider"] = "Microsoft.Data.Sqlite",
                    ["ConnectionString"] = $"Data Source={System.IO.Path.Combine(DataFolder, "KNote.db")}",
                    ["ResourcesContainer"] = "KntCntResources",
                    ["ResourcesContainerRootPath"] = System.IO.Path.Combine(ResourcesFolder, "Personal"),
                    ["ResourceContentInDB"] = false,
                    ["IsDefault"] = true
                }
            }
        };
        WriteConfig(config);
        return true;
    }

    public JsonObject ReadConfig() =>
        JsonNode.Parse(File.ReadAllText(ConfigFile), documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip })?.AsObject()
        ?? new JsonObject();

    // Written whole to a temporary file first and then moved over it: whoever reads it (the configuration reloads it
    // when it changes, the launcher) never finds it half written.
    public void WriteConfig(JsonObject config)
    {
        var temporary = ConfigFile + ".tmp";
        File.WriteAllText(temporary, config.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, ConfigFile, true);
    }

    public static int FindFreePort(int startPort)
    {
        for (var port = startPort; port < startPort + 100; port++)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return port;
            }
            catch (SocketException)
            {
                // In use: the next one.
            }
        }
        return startPort;
    }

    private static string NewSecret() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
