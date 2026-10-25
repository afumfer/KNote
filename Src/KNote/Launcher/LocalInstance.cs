using System.Text.Json;
using System.Text.Json.Nodes;

namespace KNote.Launcher;

/// <summary>
/// The user's KNote Local as the launcher sees it, in its data folder (the same the server uses, see the Server's
/// KntLocalData): the configuration the server creates the first time (where it listens and the id of this
/// installation) and, while the server runs, its PID and control token. The launcher only reads them, except the port
/// (--port), which it changes in the configuration of an installation already created.
/// </summary>
public sealed class LocalInstance
{
    public const string ConfigFileName = "knote-local.json";
    public const int DefaultPort = 5280;

    public LocalInstance(string path)
    {
        Path = System.IO.Path.GetFullPath(path);
    }

    public string Path { get; }

    public string ConfigFile => System.IO.Path.Combine(Path, ConfigFileName);

    public string PidFile => System.IO.Path.Combine(Path, "knote-server.pid");

    public string ControlTokenFile => System.IO.Path.Combine(Path, "knote-server.token");

    public string LogsFolder => System.IO.Path.Combine(Path, "Logs");

    public bool HasConfig => File.Exists(ConfigFile);

    // Windows: %LocalAppData%\KNote\Server; macOS: ~/Library/Application Support/KNote/Server; Linux: ~/.local/share/KNote/Server.
    public static string DefaultPath =>
        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KNote", "Server");

    // --data, or the one the server would use (Local__DataPath, or the default one).
    public static LocalInstance Resolve(string? dataPath)
    {
        if (string.IsNullOrWhiteSpace(dataPath))
            dataPath = Environment.GetEnvironmentVariable("Local__DataPath");
        return new LocalInstance(string.IsNullOrWhiteSpace(dataPath) ? DefaultPath : dataPath);
    }

    /// <summary>
    /// Where the server listens (Kestrel:Endpoints:Http:Url; an address for every interface is reached at 127.0.0.1)
    /// and the id of this installation (Local:InstanceId).
    /// </summary>
    public (Uri ServerUrl, string? InstanceId) ReadConfig()
    {
        var config = ReadJson();
        var url = config["Kestrel"]?["Endpoints"]?["Http"]?["Url"]?.GetValue<string>();
        var instanceId = config["Local"]?["InstanceId"]?.GetValue<string>();
        return (ToReachableUrl(url), instanceId);
    }

    /// <summary>The web app's address (the server's, under /KNote/).</summary>
    public Uri AppUrl => new(ReadConfig().ServerUrl, "/KNote/");

    // The port from now on, keeping the rest of the address.
    public void WritePort(int port)
    {
        var config = ReadJson();
        var http = Section(Section(Section(config, "Kestrel"), "Endpoints"), "Http");
        var current = http["Url"]?.GetValue<string>();
        var host = Uri.TryCreate(current, UriKind.Absolute, out var uri) ? uri.Host : "127.0.0.1";
        http["Url"] = $"http://{host}:{port}";

        var temporary = ConfigFile + ".tmp";
        File.WriteAllText(temporary, config.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, ConfigFile, true);
    }

    public int? ReadPid() => int.TryParse(ReadText(PidFile), out var pid) ? pid : null;

    public string? ReadControlToken() => ReadText(ControlTokenFile);

    public static Uri ToReachableUrl(string? url)
    {
        if (!Uri.TryCreate(url?.Replace("://*", "://0.0.0.0").Replace("://+", "://0.0.0.0"), UriKind.Absolute, out var uri))
            return new Uri($"http://127.0.0.1:{DefaultPort}");
        var builder = new UriBuilder(uri) { Path = "" };
        if (builder.Host is "0.0.0.0" or "[::]" or "::")
            builder.Host = "127.0.0.1";
        return builder.Uri;
    }

    private JsonObject ReadJson() =>
        JsonNode.Parse(File.ReadAllText(ConfigFile), documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip })?.AsObject()
        ?? new JsonObject();

    private static JsonObject Section(JsonObject parent, string name)
    {
        if (parent[name] is not JsonObject section)
            parent[name] = section = new JsonObject();
        return section;
    }

    private static string? ReadText(string file)
    {
        try
        {
            return File.Exists(file) ? File.ReadAllText(file).Trim() : null;
        }
        catch (IOException)
        {
            return null;
        }
    }
}
