using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KNote.Model;
using KNote.Service.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Tests.Helpers;

/// <summary>
/// Boots the Server host in-process against throwaway, per-instance Sqlite files created and
/// schema-seeded by KntSchemaUpdater, instead of requiring a manually started Server. By default with one
/// repository (the "RepositoryRef" section, as a single-repository Server); with repositoryCount above 1, with the
/// "Repositories" list (the first one is the default repository).
///
/// The repositories/AppSettings are overridden via process environment variables (RepositoryRef__Orm,
/// Repositories__0__Orm, etc.), set BEFORE the host is first built - not via WebApplicationFactory's
/// ConfigureAppConfiguration. Server/Program.cs reads them as part of its top-level statements,
/// before WebApplicationFactory gets a chance to layer in ConfigureAppConfiguration overrides (those
/// only apply at the Build() call, which runs after that read). Environment variables, by contrast,
/// are consulted by WebApplicationBuilder.CreateBuilder(args) itself as one of its default sources
/// (and outrank both appsettings.json and the developer's local user-secrets.json), so they are
/// visible in time. Without this, the test would silently hit the developer's real, configured
/// database (this was discovered the hard way: the SQL Server connection string from this
/// project's user-secrets.json was used, and a stray test row ended up in that real database).
///
/// Because these are process-wide environment variables, only one KNoteWebApplicationFactory should
/// be built at a time (no parallel test classes each spinning up their own host concurrently), and each one
/// clears the variables of the other form (a single repository or the list).
/// </summary>
public class KNoteWebApplicationFactory : WebApplicationFactory<Program>
{
    // The most repositories a factory clears the variables of.
    private const int MaxRepositories = 4;

    private readonly string[] _databaseFilePaths;
    private readonly string[] _resourcesFolders;

    // The first (default) repository's database.
    public string DatabaseFilePath => _databaseFilePaths[0];

    // Where the host keeps the files of the notes' resources of the first repository (otherwise, in the Server
    // project's wwwroot), in ResourcesContainer, served from ResourcesRootUrl.
    public string ResourcesFolder => _resourcesFolders[0];
    public const string ResourcesContainer = "KNoteTestResources";
    public const string ResourcesRootUrl = "http://localhost";

    public int RepositoryCount => _databaseFilePaths.Length;

    public string DatabaseFilePathOf(int repository) => _databaseFilePaths[repository];

    public string ResourcesFolderOf(int repository) => _resourcesFolders[repository];

    // Each repository's resources in their own folder, served from their own url.
    public static string ResourcesRootUrlOf(int repository) => repository == 0 ? ResourcesRootUrl : $"{ResourcesRootUrl}/repository{repository}";

    public static string RepositoryNameOf(int repository) => $"KNoteInProcessTests{(repository == 0 ? "" : repository.ToString())}";

    /// <summary>
    /// KNote Local's data folder (localDataPath), or null as a server (every other factory clears it). In local mode
    /// the host takes its repositories from the data folder's configuration (created the first time), not from the
    /// factory's.
    /// </summary>
    public string? LocalDataPath { get; }

    public KNoteWebApplicationFactory(int repositoryCount = 1, string? localDataPath = null)
    {
        LocalDataPath = localDataPath;
        if (repositoryCount < 1 || repositoryCount > MaxRepositories)
            throw new ArgumentOutOfRangeException(nameof(repositoryCount));

        _databaseFilePaths = Enumerable.Range(0, repositoryCount)
            .Select(_ => Path.Combine(Path.GetTempPath(), $"knote-tests-{Guid.NewGuid():N}.db")).ToArray();
        _resourcesFolders = Enumerable.Range(0, repositoryCount)
            .Select(_ => Path.Combine(Path.GetTempPath(), $"knote-tests-resources-{Guid.NewGuid():N}")).ToArray();

        var overrides = new Dictionary<string, string?>
        {
            ["AppSettings__Secret"] = "KNoteTestsOnlySigningSecret_DoNotUseInProduction_1234567890",
            ["AppSettings__ActivateMessageBroker"] = "false",
            ["AppSettings__MountResourceContainerOnStartup"] = "false",
            ["Local__DataPath"] = localDataPath
        };

        foreach (var field in RepositoryFields)
            overrides[$"RepositoryRef__{field}"] = null;
        for (var i = 0; i < MaxRepositories; i++)
            foreach (var field in RepositoryFields.Append("IsDefault"))
                overrides[$"Repositories__{i}__{field}"] = null;

        if (repositoryCount == 1)
            AddRepository(overrides, "RepositoryRef__", 0);
        else
            for (var i = 0; i < repositoryCount; i++)
            {
                AddRepository(overrides, $"Repositories__{i}__", i);
                overrides[$"Repositories__{i}__IsDefault"] = (i == 0).ToString();
            }

        foreach (var (key, value) in overrides)
            Environment.SetEnvironmentVariable(key, value);
    }

    private static readonly string[] RepositoryFields =
        ["Name", "Orm", "Provider", "ConnectionString", "ResourceContentInDB", "ResourcesContainer", "ResourcesContainerRootPath", "ResourcesContainerRootUrl"];

    private void AddRepository(Dictionary<string, string?> overrides, string prefix, int repository)
    {
        overrides[$"{prefix}Name"] = RepositoryNameOf(repository);
        overrides[$"{prefix}Orm"] = "EntityFramework";
        overrides[$"{prefix}Provider"] = "Microsoft.Data.Sqlite";
        overrides[$"{prefix}ConnectionString"] = $"Data Source={_databaseFilePaths[repository]}";
        overrides[$"{prefix}ResourceContentInDB"] = "false";
        overrides[$"{prefix}ResourcesContainer"] = ResourcesContainer;
        overrides[$"{prefix}ResourcesContainerRootPath"] = _resourcesFolders[repository];
        overrides[$"{prefix}ResourcesContainerRootUrl"] = ResourcesRootUrlOf(repository);
    }

    /// <summary>
    /// A service straight on a repository's database (no Server, no authorization), e.g. to add the users of a
    /// repository other than the default one (api/users/register only signs up there). The caller disposes it.
    /// </summary>
    public KntService CreateServiceForRepository(int repository) =>
        new(KntRepositoryFactory.Create(new RepositoryRef
        {
            Name = RepositoryNameOf(repository),
            Orm = "EntityFramework",
            Provider = "Microsoft.Data.Sqlite",
            ConnectionString = $"Data Source={_databaseFilePaths[repository]}",
            ResourcesContainer = ResourcesContainer,
            ResourcesContainerRootPath = _resourcesFolders[repository],
            ResourcesContainerRootUrl = ResourcesRootUrlOf(repository)
        }));

    /// <summary>
    /// Extra configuration, layered over appsettings.json when the host is built. Only for settings the Server
    /// reads after that (through IOptions, like the "ai" section), not for the ones Program.cs reads up front
    /// (AppSettings and the repositories, see above). Set before the first CreateClient().
    /// </summary>
    public Dictionary<string, string?> AppConfiguration { get; } = new();

    /// <summary>Replaces services of the Server (e.g. IAiChatClientProvider). Set before the first CreateClient().</summary>
    public Action<IServiceCollection>? TestServices { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        if (AppConfiguration.Count > 0)
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(AppConfiguration));

        if (TestServices != null)
            builder.ConfigureTestServices(TestServices);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        // Program sends the logs of local mode to the data folder (NLog's global context, for the whole process).
        if (LocalDataPath != null)
        {
            NLog.GlobalDiagnosticsContext.Remove("logDir");
            Environment.SetEnvironmentVariable("Local__DataPath", null);
        }

        // Microsoft.Data.Sqlite pools connections by default, which can keep the file locked
        // after the host shuts down; clear the pool first so deletion doesn't need to be silently
        // best-effort.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        try
        {
            foreach (var file in _databaseFilePaths.Where(File.Exists))
                File.Delete(file);
            foreach (var folder in _resourcesFolders.Where(Directory.Exists))
                Directory.Delete(folder, true);
        }
        catch (IOException)
        {
            // Best-effort cleanup: the OS temp folder will eventually reclaim it regardless.
        }
    }
}
