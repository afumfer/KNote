using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Ai;
using KNote.Server.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KNote.Server.Local;

/// <summary>
/// The settings of KNote Local's computer, read from and written to the data folder's configuration (knote-local.json)
/// with the secrets encrypted (KntSecretProtector), for api/local. After a change the configuration is read again and
/// applied at once (ApplyAsync): the repositories are opened again (KntRepositories.ReloadAsync) and the AI providers'
/// catalog reloaded; the e-mail and alarms settings are read when used (KntLocal.KntAddLocal). Only the port waits for
/// the next start. The AI providers are kept in Local:ai, not in "ai": the lists of the configuration are merged by
/// position, so the sample providers of appsettings.json would show through a shorter list.
/// </summary>
public class KntLocalSettings
{
    public const string AiSection = "Local:ai";
    private const string Sqlite = "Microsoft.Data.Sqlite";
    private const string SqlServer = "Microsoft.Data.SqlClient";

    private readonly KntLocalData _localData;
    private readonly KntSecretProtector _protector;
    private readonly IConfiguration _configuration;
    private readonly KntRepositories _repositories;
    private readonly AiProvidersCatalog _aiCatalog;
    private readonly ILogger<KntLocalSettings> _logger;
    private readonly SemaphoreSlim _writing = new(1, 1);

    public KntLocalSettings(KntLocalData localData, KntSecretProtector protector, IConfiguration configuration, KntRepositories repositories,
        AiProvidersCatalog aiCatalog, ILogger<KntLocalSettings> logger)
    {
        _localData = localData;
        _protector = protector;
        _configuration = configuration;
        _repositories = repositories;
        _aiCatalog = aiCatalog;
        _logger = logger;
    }

    #region Repositories

    public async Task<List<LocalRepositoryDto>> GetRepositoriesAsync()
    {
        var repositories = new List<LocalRepositoryDto>();
        var items = Array(_localData.ReadConfig(), "Repositories");
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i]?.AsObject() ?? new JsonObject();
            var provider = Text(item, "Provider");
            var connectionString = TryUnprotect(Text(item, "ConnectionString"));
            var entry = _repositories.All.FirstOrDefault(e => e.RepositoryRef.ConnectionString == connectionString);

            var name = Text(item, "Name");
            if (entry != null)
            {
                using var service = _repositories.CreateService(entry, null, null);
                var info = await service.SystemValues.GetRepositoryInfoAsync();
                if (info.IsValid)
                    name = info.Entity.Name;
            }

            repositories.Add(new LocalRepositoryDto
            {
                Index = i,
                RepositoryId = entry?.RepositoryId,
                Name = name,
                Provider = provider,
                Orm = Text(item, "Orm"),
                Database = DescribeDatabase(provider, connectionString),
                ResourcesFolder = Text(item, "ResourcesContainerRootPath"),
                ResourcesContainer = Text(item, "ResourcesContainer"),
                IsDefault = entry?.IsDefault ?? false
            });
        }
        return repositories;
    }

    /// <summary>
    /// Adds a repository: a new SQLite database in the data folder, an existing SQLite database or a SQL Server one. It is
    /// opened first (its database is created or updated, as when the server starts) and refused if it is already linked;
    /// if it is then left out (e.g. its resources folder is another repository's), the configuration is put back.
    /// </summary>
    public async Task<Result<List<LocalRepositoryDto>>> AddRepositoryAsync(LocalRepositoryAddDto add)
    {
        var result = new Result<List<LocalRepositoryDto>>();
        var (config, error) = ToRepositoryConfig(add);
        if (error != null)
            return Failed(result, error);

        await _writing.WaitAsync();
        try
        {
            Guid repositoryId;
            try
            {
                repositoryId = await KntRepositories.ReadRepositoryIdAsync(config);
            }
            catch (Exception ex)
            {
                return Failed(result, $"The database could not be opened: {ex.Message}");
            }
            if (_repositories.Find(repositoryId) != null)
                return Failed(result, "This database is already linked.");

            var file = _localData.ReadConfig();
            var items = Array(file, "Repositories");
            items.Add(ToJson(config));
            _localData.WriteConfig(file);

            var problems = await ApplyAsync();
            if (_repositories.Find(repositoryId) == null)
            {
                items.RemoveAt(items.Count - 1);
                _localData.WriteConfig(file);
                await ApplyAsync();
                return Failed(result, problems.Count > 0 ? string.Join(" ", problems) : "The repository could not be linked (see the server's log).");
            }
        }
        finally
        {
            _writing.Release();
        }

        result.Entity = await GetRepositoriesAsync();
        return result;
    }

    public Task<Result<List<LocalRepositoryDto>>> SetDefaultRepositoryAsync(int index) =>
        ChangeRepositoriesAsync(index, (items, _) =>
        {
            for (var i = 0; i < items.Count; i++)
                items[i]!["IsDefault"] = i == index;
            return null;
        });

    // Only from the list: the database is never deleted. Neither the last repository nor the one of the user's session.
    public Task<Result<List<LocalRepositoryDto>>> UnlinkRepositoryAsync(int index, Guid? sessionRepositoryId) =>
        ChangeRepositoriesAsync(index, (items, entry) =>
        {
            if (items.Count == 1)
                return "The last repository can't be unlinked.";
            if (entry != null && entry.RepositoryId == sessionRepositoryId)
                return "This is the repository of your session: sign in to another repository before unlinking it.";

            var wasDefault = items[index]?["IsDefault"]?.GetValue<bool>() == true;
            items.RemoveAt(index);
            if (wasDefault)
                items[0]!["IsDefault"] = true;
            return null;
        });

    // The name is kept in the repository's database (the one both apps show); the configuration keeps it too.
    public async Task<Result<List<LocalRepositoryDto>>> RenameRepositoryAsync(int index, string name)
    {
        var result = new Result<List<LocalRepositoryDto>>();
        if (string.IsNullOrWhiteSpace(name))
            return Failed(result, "The name is required.");

        var entry = (await GetRepositoriesAsync()).ElementAtOrDefault(index) is { RepositoryId: Guid id } ? _repositories.Find(id) : null;
        if (entry == null)
            return Failed(result, "The repository is not open: it can't be renamed.");

        using (var service = _repositories.CreateService(entry, null, null))
        {
            var saved = await service.SystemValues.SaveRepositoryNameAsync(name.Trim());
            if (!saved.IsValid)
                return Failed(result, saved.ErrorMessage);
        }

        return await ChangeRepositoriesAsync(index, (items, _) =>
        {
            items[index]!["Name"] = name.Trim();
            return null;
        });
    }

    // change: the change to the configuration's list (with the repository at index, if it is open), or why it can't be.
    private async Task<Result<List<LocalRepositoryDto>>> ChangeRepositoriesAsync(int index, Func<JsonArray, KntRepositoryEntry, string> change)
    {
        var result = new Result<List<LocalRepositoryDto>>();
        await _writing.WaitAsync();
        try
        {
            var file = _localData.ReadConfig();
            var items = Array(file, "Repositories");
            if (index < 0 || index >= items.Count)
                return Failed(result, "There is no such repository.");

            var connectionString = TryUnprotect(Text(items[index]!.AsObject(), "ConnectionString"));
            var entry = _repositories.All.FirstOrDefault(e => e.RepositoryRef.ConnectionString == connectionString);
            var error = change(items, entry);
            if (error != null)
                return Failed(result, error);

            _localData.WriteConfig(file);
            var problems = await ApplyAsync();
            foreach (var problem in problems)
                result.AddErrorMessage(problem);
        }
        finally
        {
            _writing.Release();
        }

        result.Entity = await GetRepositoriesAsync();
        return result;
    }

    private (KntRepositoryConfig Config, string Error) ToRepositoryConfig(LocalRepositoryAddDto add)
    {
        var name = add.Name?.Trim();
        string connectionString;
        string defaultFolderName;

        if (add.Provider == Sqlite)
        {
            var path = add.DatabasePath?.Trim();
            if (string.IsNullOrEmpty(path))
            {
                if (string.IsNullOrEmpty(name))
                    return (null, "The name of the new repository is required.");
                path = Path.Combine(_localData.DataFolder, FileNameOf(name) + ".db");
                if (File.Exists(path))
                    return (null, $"There is already a database called {Path.GetFileName(path)} in the data folder: choose another name.");
            }
            else if (!File.Exists(path))
                return (null, $"There is no database file at {path}.");

            connectionString = $"Data Source={Path.GetFullPath(path)}";
            defaultFolderName = FileNameOf(string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(path) : name);
        }
        else if (add.Provider == SqlServer)
        {
            connectionString = add.ConnectionString?.Trim();
            if (string.IsNullOrEmpty(connectionString))
                return (null, "The connection string is required.");
            defaultFolderName = FileNameOf(string.IsNullOrEmpty(name) ? DescribeDatabase(SqlServer, connectionString) : name);
        }
        else
            return (null, $"The database must be {Sqlite} or {SqlServer}.");

        return (new KntRepositoryConfig
        {
            Name = name,
            Orm = string.IsNullOrWhiteSpace(add.Orm) ? "EntityFramework" : add.Orm,
            Provider = add.Provider,
            ConnectionString = connectionString,
            ResourcesContainer = string.IsNullOrWhiteSpace(add.ResourcesContainer) ? "KntCntResources" : add.ResourcesContainer.Trim(),
            ResourcesContainerRootPath = string.IsNullOrWhiteSpace(add.ResourcesFolder)
                ? Path.Combine(_localData.ResourcesFolder, defaultFolderName)
                : Path.GetFullPath(add.ResourcesFolder.Trim()),
            ResourceContentInDB = false
        }, null);
    }

    // The SQL Server connection strings are encrypted (they may carry a password).
    private JsonObject ToJson(KntRepositoryConfig config) => new()
    {
        ["Name"] = config.Name,
        ["Orm"] = config.Orm,
        ["Provider"] = config.Provider,
        ["ConnectionString"] = config.Provider == SqlServer ? _protector.Protect(config.ConnectionString) : config.ConnectionString,
        ["ResourcesContainer"] = config.ResourcesContainer,
        ["ResourcesContainerRootPath"] = config.ResourcesContainerRootPath,
        ["ResourceContentInDB"] = false,
        ["IsDefault"] = false
    };

    // SQLite: the file. SQL Server: the server and the database, never the password.
    private static string DescribeDatabase(string provider, string connectionString)
    {
        try
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString ?? "" };
            if (provider == Sqlite)
                return builder.TryGetValue("Data Source", out var file) ? file?.ToString() : "";
            var server = builder.TryGetValue("Data Source", out var source) ? source : builder.TryGetValue("Server", out var s) ? s : "";
            var database = builder.TryGetValue("Initial Catalog", out var catalog) ? catalog : builder.TryGetValue("Database", out var d) ? d : "";
            return $"{server}/{database}";
        }
        catch (ArgumentException)
        {
            return "";
        }
    }

    private static string FileNameOf(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var fileName = new string((name ?? "").Trim().Select(c => invalid.Contains(c) || char.IsWhiteSpace(c) ? '_' : c).ToArray());
        return string.IsNullOrEmpty(fileName) ? "Repository" : fileName;
    }

    #endregion

    #region AI providers

    public List<LocalAiProviderDto> GetAiProviders() =>
        AiProvidersOf(_localData.ReadConfig()).Select((node, i) => new LocalAiProviderDto
        {
            Index = i,
            Alias = Text(node, "alias"),
            Provider = Text(node, "provider"),
            Model = Text(node, "model"),
            Host = Text(node, "host"),
            HasApiKey = !string.IsNullOrEmpty(Text(node, "apiKey"))
        }).ToList();

    /// <summary>The whole list (in its order: the first is the default one). An empty API key keeps the one it had.</summary>
    public async Task<Result<List<LocalAiProviderDto>>> SaveAiProvidersAsync(List<LocalAiProviderDto> providers)
    {
        var result = new Result<List<LocalAiProviderDto>>();
        providers ??= new();

        foreach (var provider in providers)
        {
            var check = new AiProviderRef { Alias = provider.Alias?.Trim(), Provider = provider.Provider, Model = provider.Model?.Trim(), Host = provider.Host?.Trim() };
            if (!check.IsValid())
                return Failed(result, $"{provider.Alias}: {check.GetErrorMessage()}");
        }
        var repeated = providers.GroupBy(p => p.Alias.Trim(), StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (repeated != null)
            return Failed(result, $"There are two AI providers called {repeated.Key}.");

        await _writing.WaitAsync();
        try
        {
            var file = _localData.ReadConfig();
            var current = AiProvidersOf(file);
            var list = new JsonArray();
            foreach (var provider in providers)
            {
                var apiKey = !string.IsNullOrEmpty(provider.ApiKey)
                    ? _protector.Protect(provider.ApiKey.Trim())
                    : provider.Index >= 0 && provider.Index < current.Count ? Text(current[provider.Index], "apiKey") : "";
                list.Add(new JsonObject
                {
                    ["alias"] = provider.Alias.Trim(),
                    ["provider"] = provider.Provider,
                    ["model"] = provider.Model.Trim(),
                    ["host"] = provider.Provider == EnumAiProvider.Ollama ? provider.Host?.Trim() : null,
                    ["apiKey"] = apiKey
                });
            }
            Section(Section(file, "Local"), "ai")["providers"] = list;
            _localData.WriteConfig(file);
            await ApplyAsync();
        }
        finally
        {
            _writing.Release();
        }

        result.Entity = GetAiProviders();
        return result;
    }

    private static List<JsonObject> AiProvidersOf(JsonObject file) =>
        (file["Local"]?["ai"]?["providers"] as JsonArray)?.OfType<JsonObject>().ToList() ?? new();

    #endregion

    #region E-mail

    public LocalEmailSettingsDto GetEmail()
    {
        var file = _localData.ReadConfig();
        var email = file["Email"] as JsonObject ?? new JsonObject();
        return new LocalEmailSettingsDto
        {
            Host = Text(email, "Host"),
            Port = email["Port"]?.GetValue<int>() ?? 587,
            EnableSsl = email["EnableSsl"]?.GetValue<bool>() ?? true,
            FromAddress = Text(email, "FromAddress"),
            FromDisplayName = Text(email, "FromDisplayName") ?? "KNote",
            Username = Text(email, "Username"),
            HasPassword = !string.IsNullOrEmpty(Text(email, "Password")),
            SendEmailAlarms = file["Alarms"]?["Email"]?["Enabled"]?.GetValue<bool>() ?? false
        };
    }

    // Every field is written (also the empty ones), over appsettings.json's placeholders. An empty password keeps the
    // one it had.
    public async Task<Result<LocalEmailSettingsDto>> SaveEmailAsync(LocalEmailSettingsDto settings)
    {
        var result = new Result<LocalEmailSettingsDto>();
        if (settings.Port < 1 || settings.Port > 65535)
            return Failed(result, "The port of the SMTP server is not valid.");
        if (settings.SendEmailAlarms && (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.FromAddress)))
            return Failed(result, "The SMTP server and the sender's address are needed to send the Email alarms.");

        await _writing.WaitAsync();
        try
        {
            var file = _localData.ReadConfig();
            var password = !string.IsNullOrEmpty(settings.Password)
                ? _protector.Protect(settings.Password)
                : Text(file["Email"] as JsonObject ?? new JsonObject(), "Password") ?? "";
            file["Email"] = new JsonObject
            {
                ["Host"] = settings.Host?.Trim() ?? "",
                ["Port"] = settings.Port,
                ["EnableSsl"] = settings.EnableSsl,
                ["FromAddress"] = settings.FromAddress?.Trim() ?? "",
                ["FromDisplayName"] = settings.FromDisplayName?.Trim() ?? "",
                ["Username"] = settings.Username?.Trim() ?? "",
                ["Password"] = password
            };
            Section(Section(file, "Alarms"), "Email")["Enabled"] = settings.SendEmailAlarms;
            _localData.WriteConfig(file);
            await ApplyAsync();
        }
        finally
        {
            _writing.Release();
        }

        result.Entity = GetEmail();
        return result;
    }

    #endregion

    #region Server

    public LocalServerSettingsDto GetServer()
    {
        var url = _localData.ReadConfig()["Kestrel"]?["Endpoints"]?["Http"]?["Url"]?.GetValue<string>() ?? "";
        return new LocalServerSettingsDto
        {
            Url = url,
            Port = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Port : KntLocalData.DefaultPort,
            DataPath = _localData.Path,
            Version = typeof(KntLocalSettings).Assembly.GetName().Version?.ToString()
        };
    }

    // The next time the server starts (the launcher reads it from the configuration too).
    public async Task<Result<LocalServerSettingsDto>> SavePortAsync(int port)
    {
        var result = new Result<LocalServerSettingsDto>();
        if (port < 1024 || port > 65535)
            return Failed(result, "The port must be between 1024 and 65535.");

        await _writing.WaitAsync();
        try
        {
            var file = _localData.ReadConfig();
            var http = Section(Section(Section(file, "Kestrel"), "Endpoints"), "Http");
            var host = Uri.TryCreate(http["Url"]?.GetValue<string>(), UriKind.Absolute, out var uri) ? uri.Host : "127.0.0.1";
            http["Url"] = $"http://{host}:{port}";
            _localData.WriteConfig(file);
        }
        finally
        {
            _writing.Release();
        }

        result.Entity = GetServer();
        return result;
    }

    #endregion

    // The configuration read again now (not when the file watcher notices it) and applied.
    private async Task<List<string>> ApplyAsync()
    {
        ((IConfigurationRoot)_configuration).Reload();
        _aiCatalog.Reload();
        return await _repositories.ReloadAsync(KntRepositories.ReadConfiguration(_configuration), _logger);
    }

    private string TryUnprotect(string value)
    {
        try
        {
            return _protector.Unprotect(value);
        }
        catch (CryptographicException)
        {
            return "";
        }
    }

    private static JsonArray Array(JsonObject file, string name)
    {
        if (file[name] is not JsonArray array)
            file[name] = array = new JsonArray();
        return array;
    }

    private static JsonObject Section(JsonObject parent, string name)
    {
        if (parent[name] is not JsonObject section)
            parent[name] = section = new JsonObject();
        return section;
    }

    private static string Text(JsonObject node, string name) => node[name] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static Result<T> Failed<T>(Result<T> result, string error)
    {
        result.AddErrorMessage(error);
        return result;
    }
}

/// <summary>
/// IOptions whose Value is read from the configuration each time: in KNote Local, the settings changed from the app
/// apply at once (KntLocal.KntAddLocal). The server mode keeps the usual IOptions (read once).
/// </summary>
public sealed class KntCurrentOptions<T> : IOptions<T> where T : class
{
    private readonly Func<T> _read;

    public KntCurrentOptions(Func<T> read)
    {
        _read = read;
    }

    public T Value => _read();
}
