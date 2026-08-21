using System;
using System.IO;
using KNote.Model;
using KNote.Repository;
using KNote.Repository.EntityFramework;
using KNote.Service.Core;
using Microsoft.EntityFrameworkCore;

namespace KNote.Tests.Helpers;

/// <summary>
/// Provisions a fresh, throwaway Sqlite database for the Dapper/EF repository parity suite.
///
/// EF's KntDbContext.EnsureCreated() is the only schema-bootstrap mechanism this codebase has -
/// Repository.Dapper has no CREATE TABLE of its own and assumes the schema already exists - so it
/// is used here purely as a provisioning tool. CreateRepository(orm) then hands out a repository
/// pointed at that same file, so tests can exercise Dapper and EntityFramework against identical,
/// identically-seeded schema without needing two separate databases.
/// </summary>
public sealed class RepositoryTestDatabase : IDisposable
{
    public string DatabaseFilePath { get; } = Path.Combine(Path.GetTempPath(), $"knote-parity-{Guid.NewGuid():N}.db");

    private readonly string _connectionString;

    public RepositoryTestDatabase()
    {
        _connectionString = $"Data Source={DatabaseFilePath}";

        var options = new DbContextOptionsBuilder<KntDbContext>()
            .UseSqlite(_connectionString)
            .Options;

        using var ctx = new KntDbContext(options); // ensureCreated defaults to true
    }

    public IKntRepository CreateRepository(string orm)
    {
        var repositoryRef = new RepositoryRef
        {
            Alias = "KNoteParityTests",
            Orm = orm,
            Provider = "Microsoft.Data.Sqlite",
            ConnectionString = _connectionString
        };

        return KntRepositoryFactory.Create(repositoryRef);
    }

    public void Dispose()
    {
        // Microsoft.Data.Sqlite pools connections by default, which can keep the file locked
        // after a repository is disposed; clear the pool first so deletion isn't just best-effort.
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
