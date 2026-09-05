using KNote.Repository.EntityFramework;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.SchemaUpdateTests;

/// <summary>
/// Guards KntSchemaUpdater revision 3: KAttributes' unique index widened from (Name) to
/// (Name, NoteTypeId), the dead KEvents table dropped, and KLogs.ActionType added. Mirrors
/// SchemaUpdaterTests' revision-2 coverage - rolls a database back to the pre-revision-3 shape,
/// then checks KntRepositoryFactory.Create (the real startup path) repairs it.
/// </summary>
[TestClass]
public class SchemaUpdaterRevision3Tests
{
    [TestMethod]
    public void EnsureUpToDate_OnDatabasePredatingRevision3_ReplacesIndexDropsKEventsAddsActionType()
    {
        using var db = new RepositoryTestDatabase(); // provisioned at the current (post-revision-3) model

        using (var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}"))
        {
            connection.Open();

            // Roll the database back to what a pre-revision-3 installation looks like: the old
            // Name-only unique index, a KEvents table still around, KLogs without ActionType.
            Exec(connection, "DROP INDEX IF EXISTS \"IX_KAttributes_Name_NoteTypeId\";");
            Exec(connection, "CREATE UNIQUE INDEX \"IX_KAttributes_Name\" ON \"KAttributes\" (\"Name\");");
            Exec(connection, """
                CREATE TABLE "KEvents" (
                    "KEventId" TEXT NOT NULL CONSTRAINT "PK_KEvents" PRIMARY KEY,
                    "NoteScriptId" TEXT NULL,
                    "EntityId" TEXT NULL,
                    "EntityName" TEXT NULL,
                    "PropertyName" TEXT NULL,
                    "PropertyValue" TEXT NULL,
                    "EventType" INTEGER NOT NULL
                );
                """);
            RecreateKLogsWithoutActionType(connection);
            Exec(connection, "UPDATE SystemValues SET [Value] = '2' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");

            Assert.IsTrue(IndexExists(connection, "IX_KAttributes_Name"));
            Assert.IsFalse(IndexExists(connection, "IX_KAttributes_Name_NoteTypeId"));
            Assert.IsTrue(TableExists(connection, "KEvents"));
            Assert.IsFalse(ColumnExists(connection, "KLogs", "ActionType"));
        }

        // The next repository created against this connection string is what every real ServiceRef/
        // Server startup goes through (KntRepositoryFactory.Create) - not a call to KntSchemaUpdater
        // in isolation.
        using var repo = db.CreateRepository("Dapper");

        using var verifyConnection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        verifyConnection.Open();

        Assert.IsFalse(IndexExists(verifyConnection, "IX_KAttributes_Name"));
        Assert.IsTrue(IndexExists(verifyConnection, "IX_KAttributes_Name_NoteTypeId"));
        Assert.IsFalse(TableExists(verifyConnection, "KEvents"));
        Assert.IsTrue(ColumnExists(verifyConnection, "KLogs", "ActionType"));
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), ReadDbVersion(verifyConnection));

        // And the new index actually enforces what was asked for, on this exact (repaired) database.
        var typeId = InsertNoteType(verifyConnection, "type-a");
        var otherTypeId = InsertNoteType(verifyConnection, "type-b");

        InsertKAttribute(verifyConnection, "Priority", typeId); // first "Priority" scoped to type-a: OK
        InsertKAttribute(verifyConnection, "Priority", otherTypeId); // same Name, different NoteTypeId: allowed

        Assert.ThrowsExactly<SqliteException>(() =>
            InsertKAttribute(verifyConnection, "Priority", typeId)); // duplicate (Name, NoteTypeId): rejected
    }

    [TestMethod]
    public void UniqueIndex_DoesNotRejectDuplicateNamesAmongGlobalAttributes()
    {
        // Pins the known gap flagged in ModelBuilderExtensions/KntKAttributesSaveAsyncCommand:
        // NoteTypeId == null makes each row distinct for the unique index (both Sqlite and SQL
        // Server treat NULL as distinct from any other NULL), so the database alone does not stop
        // two identically-named *global* attributes. KntKAttributesSaveAsyncCommand closes that gap
        // at the Service layer instead - not covered here, this only pins the schema-level behavior.
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository("Dapper");

        using var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();

        InsertKAttribute(connection, "Priority", noteTypeId: null);
        InsertKAttribute(connection, "Priority", noteTypeId: null); // not rejected - documented gap
    }

    private static void RecreateKLogsWithoutActionType(SqliteConnection connection)
    {
        Exec(connection, "DROP TABLE \"KLogs\";");
        Exec(connection, """
            CREATE TABLE "KLogs" (
                "KLogId" TEXT NOT NULL CONSTRAINT "PK_KLogs" PRIMARY KEY,
                "EntityId" TEXT NOT NULL,
                "EntityName" TEXT NOT NULL,
                "RegistryDateTime" TEXT NOT NULL,
                "RegistryMessage" TEXT NOT NULL
            );
            """);
    }

    private static string? ReadDbVersion(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT [Value] FROM SystemValues WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';";
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

    private static bool ColumnExists(SqliteConnection connection, string tableName, string columnName)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info(\"{tableName}\")";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static void Exec(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static Guid InsertNoteType(SqliteConnection connection, string name)
    {
        var id = Guid.NewGuid();
        Exec(connection, $"INSERT INTO NoteTypes (NoteTypeId, Name) VALUES ('{id}', '{name}-{id:N}');");
        return id;
    }

    private static void InsertKAttribute(SqliteConnection connection, string name, Guid? noteTypeId)
    {
        var typeValue = noteTypeId is null ? "NULL" : $"'{noteTypeId}'";
        Exec(connection, $"""
            INSERT INTO KAttributes (KAttributeId, Name, KAttributeDataType, RequiredValue, [Order], Disabled, NoteTypeId)
            VALUES ('{Guid.NewGuid()}', '{name}', 0, 0, 0, 0, {typeValue});
            """);
    }
}
