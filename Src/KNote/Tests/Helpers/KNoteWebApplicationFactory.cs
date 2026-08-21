using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace KNote.Tests.Helpers;

/// <summary>
/// Boots the Server host in-process against a throwaway, per-instance Sqlite file created and
/// schema-seeded by KntDbContext.EnsureCreated(), instead of requiring a manually started Server.
///
/// RepositoryRef/AppSettings are overridden via process environment variables (RepositoryRef__Orm,
/// etc.), set BEFORE the host is first built - not via WebApplicationFactory's ConfigureAppConfiguration.
/// Server/Program.cs reads RepositoryRef into a local variable as part of its top-level statements,
/// before WebApplicationFactory gets a chance to layer in ConfigureAppConfiguration overrides (those
/// only apply at the Build() call, which runs after that read). Environment variables, by contrast,
/// are consulted by WebApplicationBuilder.CreateBuilder(args) itself as one of its default sources
/// (and outrank both appsettings.json and the developer's local user-secrets.json), so they are
/// visible in time. Without this, the test would silently hit the developer's real, configured
/// database (this was discovered the hard way: the SQL Server connection string from this
/// project's user-secrets.json was used, and a stray test row ended up in that real database).
///
/// Because these are process-wide environment variables, only one KNoteWebApplicationFactory should
/// be built at a time (no parallel test classes each spinning up their own host concurrently).
/// </summary>
public class KNoteWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DatabaseFilePath { get; } = Path.Combine(Path.GetTempPath(), $"knote-tests-{Guid.NewGuid():N}.db");

    public KNoteWebApplicationFactory()
    {
        var overrides = new Dictionary<string, string>
        {
            ["AppSettings__Secret"] = "KNoteTestsOnlySigningSecret_DoNotUseInProduction_1234567890",
            ["AppSettings__ActivateMessageBroker"] = "false",
            ["AppSettings__MountResourceContainerOnStartup"] = "false",
            ["RepositoryRef__Alias"] = "KNoteInProcessTests",
            ["RepositoryRef__Orm"] = "EntityFramework",
            ["RepositoryRef__Provider"] = "Microsoft.Data.Sqlite",
            ["RepositoryRef__ConnectionString"] = $"Data Source={DatabaseFilePath}",
            ["RepositoryRef__ResourcesContentInDB"] = "false"
        };

        foreach (var (key, value) in overrides)
            Environment.SetEnvironmentVariable(key, value);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        // Microsoft.Data.Sqlite pools connections by default, which can keep the file locked
        // after the host shuts down; clear the pool first so deletion doesn't need to be silently
        // best-effort.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        try
        {
            if (File.Exists(DatabaseFilePath))
                File.Delete(DatabaseFilePath);
        }
        catch (IOException)
        {
            // Best-effort cleanup: the OS temp folder will eventually reclaim it regardless.
        }
    }
}
