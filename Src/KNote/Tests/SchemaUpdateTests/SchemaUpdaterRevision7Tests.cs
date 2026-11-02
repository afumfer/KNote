using KNote.Repository.EntityFramework;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.SchemaUpdateTests;

/// <summary>
/// Guards KntSchemaUpdater revision 7: the NoteChunks table of the semantic index (KNote Atlas), in a new database
/// (EnsureCreated, from the EF model) and added to one that predates it, with the same columns and index.
/// </summary>
[TestClass]
public class SchemaUpdaterRevision7Tests
{
    private const string UniqueIndex = "IX_NoteChunks_NoteId_SourceType_SourceId_ChunkIndex_Model";

    [TestMethod]
    public void NewDatabase_HasTheTable()
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository("Dapper");

        using var connection = Open(db);
        Assert.IsTrue(Exists(connection, "table", "NoteChunks"));
        Assert.IsTrue(Exists(connection, "index", UniqueIndex));
    }

    [TestMethod]
    public void EnsureUpToDate_OnDatabasePredatingRevision7_AddsTheTableAsANewDatabaseHasIt()
    {
        string? expectedTable, expectedIndex;
        using (var fresh = new RepositoryTestDatabase())
        using (var connection = Open(fresh))
        {
            expectedTable = Sql(connection, "table", "NoteChunks");
            expectedIndex = Sql(connection, "index", UniqueIndex);
        }

        using var db = new RepositoryTestDatabase();
        using (var connection = Open(db))
        {
            Exec(connection, "DROP TABLE \"NoteChunks\";");
            Exec(connection, "UPDATE SystemValues SET [Value] = '6' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");
        }

        using var repo = db.CreateRepository("EntityFramework");

        using var verifyConnection = Open(db);
        Assert.AreEqual(Normalize(expectedTable), Normalize(Sql(verifyConnection, "table", "NoteChunks")));
        Assert.AreEqual(Normalize(expectedIndex), Normalize(Sql(verifyConnection, "index", UniqueIndex)));
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), ReadDbVersion(verifyConnection));
    }

    [TestMethod]
    public void EnsureUpToDate_WithTheTableAlreadyThere_IsANoOp()
    {
        using var db = new RepositoryTestDatabase();
        using (var connection = Open(db))
            Exec(connection, "UPDATE SystemValues SET [Value] = '6' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");

        using var repo = db.CreateRepository("Dapper");

        using var verifyConnection = Open(db);
        Assert.IsTrue(Exists(verifyConnection, "table", "NoteChunks"));
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), ReadDbVersion(verifyConnection));
    }

    private static SqliteConnection Open(RepositoryTestDatabase db)
    {
        var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();
        return connection;
    }

    private static bool Exists(SqliteConnection connection, string type, string name) => Sql(connection, type, name) != null;

    private static string? Sql(SqliteConnection connection, string type, string name)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type = @type AND name = @name;";
        cmd.Parameters.AddWithValue("@type", type);
        cmd.Parameters.AddWithValue("@name", name);
        return cmd.ExecuteScalar() as string;
    }

    // The text Sqlite keeps of a CREATE, without the differences of layout and of IF NOT EXISTS.
    private static string Normalize(string? sql) =>
        string.Join(" ", (sql ?? "").Replace("IF NOT EXISTS ", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string? ReadDbVersion(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT [Value] FROM SystemValues WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';";
        return cmd.ExecuteScalar() as string;
    }

    private static void Exec(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
