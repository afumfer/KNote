using KNote.Repository.EntityFramework;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.SchemaUpdateTests;

/// <summary>
/// Guards the two things KntSchemaUpdater (the "lightweight bootstrapper" agreed for TraceNotes,
/// see ModelBuilderExtensions/KntSchemaUpdater comments) exists to protect:
///
/// 1. A pre-existing database that predates TraceNotes gets brought up to date the first time any
///    repository is created against it (KntRepositoryFactory.Create), regardless of which ORM is
///    configured to serve queries afterwards.
/// 2. The unique index on TraceNotes (FromId, ToId, TraceNoteTypeId) behaves as decided: several
///    relations of *different* type are allowed between the same pair of notes, same-type duplicates
///    are rejected by the database, but duplicate *untyped* (TraceNoteTypeId == null) relations are
///    NOT rejected by the database (SQL Server/Sqlite treat NULLs as distinct in a unique index) -
///    that gap is intentionally left for KntTraceNoteService to close in a later phase, and this test
///    documents/pins that gap so it isn't rediscovered by surprise.
/// </summary>
[TestClass]
public class SchemaUpdaterTests
{
    [TestMethod]
    public void FreshDatabase_IsSeededAtCurrentSchemaRevision()
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository("Dapper");

        using var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();

        Assert.AreEqual(
            KntSchemaUpdater.CurrentSchemaRevision.ToString(),
            ReadDbVersion(connection));
        Assert.IsNull(ReadSystemValue(connection, "APP_VERSION")); // dropped from the seed - dead data
    }

    [TestMethod]
    public void EnsureUpToDate_OnDatabasePredatingTraceNotes_RecreatesTablesAndBumpsDbVersion()
    {
        using var db = new RepositoryTestDatabase(); // provisioned at the current (post-TraceNotes) model

        using (var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}"))
        {
            connection.Open();

            // Roll the database back to what a pre-existing, not-yet-upgraded installation looks like
            // (APP_VERSION included - every database seeded before this change had it).
            Exec(connection, "DROP TABLE TraceNotes;");
            Exec(connection, "DROP TABLE TraceNoteTypes;");
            Exec(connection, "UPDATE SystemValues SET [Value] = '1' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");
            Exec(connection, $"INSERT INTO SystemValues (SystemValueId, Scope, [Key], [Value]) VALUES ('{Guid.NewGuid()}', 'SYSTEM', 'APP_VERSION', '0.0.5.9');");

            Assert.IsFalse(TableExists(connection, "TraceNotes"));
            Assert.IsFalse(TableExists(connection, "TraceNoteTypes"));
            Assert.IsNotNull(ReadSystemValue(connection, "APP_VERSION"));
        }

        // The next repository created against this connection string is what every real ServiceRef/
        // Server startup goes through (KntRepositoryFactory.Create) - this is the actual code path
        // that must repair an existing, outdated database, not a call to KntSchemaUpdater in isolation.
        using var repo = db.CreateRepository("Dapper");

        using var verifyConnection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        verifyConnection.Open();

        Assert.IsTrue(TableExists(verifyConnection, "TraceNotes"));
        Assert.IsTrue(TableExists(verifyConnection, "TraceNoteTypes"));
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), ReadDbVersion(verifyConnection));
        Assert.IsNull(ReadSystemValue(verifyConnection, "APP_VERSION")); // dead data, deleted by the same step
    }

    [TestMethod]
    public void EnsureUpToDate_OnDatabaseWithTraceNotesButOldTwoColumnUniqueIndex_ReplacesTheIndex()
    {
        // The realistic case, not the one above: TraceNote/TraceNoteType have been part of the EF
        // model since 2020/2021 (git blame), long before any real KNote installation, so
        // EnsureCreated() already created TraceNotes/TraceNoteTypes - WITH the old (FromId, ToId)
        // unique index - for virtually every database that exists today. If KntSchemaUpdater only
        // added the new (FromId, ToId, TraceNoteTypeId) index without dropping the old one, both
        // would stay active and the old, narrower constraint would keep rejecting same-pair/
        // different-type relations, silently defeating Fase 0's decision 1.
        using var db = new RepositoryTestDatabase();

        using (var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}"))
        {
            connection.Open();

            // Roll the unique index back to the pre-Fase-0 shape (tables/other indexes untouched -
            // they predate this feature and were never in question).
            Exec(connection, "DROP INDEX IF EXISTS \"IX_TraceNotes_FromId_ToId_TraceNoteTypeId\";");
            Exec(connection, "CREATE UNIQUE INDEX \"IX_TraceNotes_FromId_ToId\" ON \"TraceNotes\" (\"FromId\", \"ToId\");");
            Exec(connection, "UPDATE SystemValues SET [Value] = '1' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");

            Assert.IsTrue(IndexExists(connection, "IX_TraceNotes_FromId_ToId"));
            Assert.IsFalse(IndexExists(connection, "IX_TraceNotes_FromId_ToId_TraceNoteTypeId"));
        }

        using var repo = db.CreateRepository("Dapper");

        using var verifyConnection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        verifyConnection.Open();

        Assert.IsFalse(IndexExists(verifyConnection, "IX_TraceNotes_FromId_ToId"));
        Assert.IsTrue(IndexExists(verifyConnection, "IX_TraceNotes_FromId_ToId_TraceNoteTypeId"));

        // And the new index actually enforces what decision 1 asked for, on this exact database.
        var (fromId, toId) = InsertTwoNotes(verifyConnection);
        var typeId = InsertTraceNoteType(verifyConnection);
        var otherTypeId = InsertTraceNoteType(verifyConnection);

        InsertTraceNote(verifyConnection, fromId, toId, typeId);
        InsertTraceNote(verifyConnection, fromId, toId, otherTypeId); // would have been rejected by the old index
    }

    [TestMethod]
    public void UniqueIndex_AllowsDifferentTypesButRejectsSameTypeDuplicate_BetweenSamePairOfNotes()
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository("Dapper");

        using var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();

        var (fromId, toId) = InsertTwoNotes(connection);
        var typeId = InsertTraceNoteType(connection);

        InsertTraceNote(connection, fromId, toId, typeId); // first relation of this type: OK

        Assert.ThrowsExactly<SqliteException>(() =>
            InsertTraceNote(connection, fromId, toId, typeId)); // duplicate of the same type: rejected

        // A relation of a *different* type between the same pair is explicitly allowed (Fase 0, decision 1).
        var otherTypeId = InsertTraceNoteType(connection);
        InsertTraceNote(connection, fromId, toId, otherTypeId);
    }

    [TestMethod]
    public void UniqueIndex_DoesNotRejectDuplicateUntypedRelations_BetweenSamePairOfNotes()
    {
        // Pins the known gap: TraceNoteTypeId == null makes each row distinct for the purposes of the
        // unique index (both Sqlite and SQL Server treat NULL as distinct from any other NULL), so the
        // database alone does not stop two untyped relations between the same pair of notes. Enforcing
        // "no duplicate untyped relation" - if wanted - is KntTraceNoteService's job, not the schema's.
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository("Dapper");

        using var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();

        var (fromId, toId) = InsertTwoNotes(connection);

        InsertTraceNote(connection, fromId, toId, traceNoteTypeId: null);
        InsertTraceNote(connection, fromId, toId, traceNoteTypeId: null); // not rejected - documented gap
    }

    private static string? ReadDbVersion(SqliteConnection connection) => ReadSystemValue(connection, "DB_VERSION");

    private static string? ReadSystemValue(SqliteConnection connection, string key)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT [Value] FROM SystemValues WHERE Scope = 'SYSTEM' AND [Key] = @key;";
        cmd.Parameters.AddWithValue("@key", key);
        return cmd.ExecuteScalar() as string;
    }

    private static bool TableExists(SqliteConnection connection, string tableName)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @name;";
        cmd.Parameters.AddWithValue("@name", tableName);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    private static bool IndexExists(SqliteConnection connection, string indexName)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = @name;";
        cmd.Parameters.AddWithValue("@name", indexName);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    private static void Exec(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static (Guid fromId, Guid toId) InsertTwoNotes(SqliteConnection connection)
    {
        var folderId = Guid.NewGuid();
        Exec(connection, $"""
            INSERT INTO Folders (FolderId, FolderNumber, CreationDateTime, ModificationDateTime, Name, [Order])
            VALUES ('{folderId}', {Random.Shared.Next(1000, int.MaxValue)}, '2026-01-01', '2026-01-01', 'schema-updater-tests', 0);
            """);

        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();
        foreach (var (id, number) in new[] { (fromId, Random.Shared.Next(1000, int.MaxValue)), (toId, Random.Shared.Next(1000, int.MaxValue)) })
        {
            Exec(connection, $"""
                INSERT INTO Notes (NoteId, NoteNumber, Topic, CreationDateTime, ModificationDateTime, Priority, FolderId)
                VALUES ('{id}', {number}, 'schema-updater-tests', '2026-01-01', '2026-01-01', 0, '{folderId}');
                """);
        }

        return (fromId, toId);
    }

    private static Guid InsertTraceNoteType(SqliteConnection connection)
    {
        var id = Guid.NewGuid();
        Exec(connection, $"INSERT INTO TraceNoteTypes (TraceNoteTypeId, Name) VALUES ('{id}', 'type-{id:N}');");
        return id;
    }

    private static void InsertTraceNote(SqliteConnection connection, Guid fromId, Guid toId, Guid? traceNoteTypeId)
    {
        var typeValue = traceNoteTypeId is null ? "NULL" : $"'{traceNoteTypeId}'";
        Exec(connection, $"""
            INSERT INTO TraceNotes (TraceNoteId, FromId, ToId, [Order], Weight, TraceNoteTypeId)
            VALUES ('{Guid.NewGuid()}', '{fromId}', '{toId}', 0, 1.0, {typeValue});
            """);
    }
}
