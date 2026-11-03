using KNote.Repository.EntityFramework;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.SchemaUpdateTests;

/// <summary>
/// Guards KntSchemaUpdater revision 8: the index of the pending notes of the semantic index, in a new database
/// (EnsureCreated, from the EF model) and added to one that predates it, the same in both.
/// </summary>
[TestClass]
public class SchemaUpdaterRevision8Tests
{
    private const string PendingIndex = "IX_NoteChunks_Model_NoteId_Stale_IndexedDateTime";

    [TestMethod]
    public void NewDatabase_HasTheIndex()
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository("Dapper");

        using var connection = Open(db);
        Assert.IsNotNull(IndexSql(connection));
    }

    [TestMethod]
    public void EnsureUpToDate_OnDatabasePredatingRevision8_AddsTheIndexAsANewDatabaseHasIt()
    {
        string? expected;
        using (var fresh = new RepositoryTestDatabase())
        using (var connection = Open(fresh))
            expected = IndexSql(connection);

        using var db = new RepositoryTestDatabase();
        using (var connection = Open(db))
        {
            Exec(connection, $"DROP INDEX \"{PendingIndex}\";");
            Exec(connection, "UPDATE SystemValues SET [Value] = '7' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");
        }

        using var repo = db.CreateRepository("EntityFramework");

        using var verifyConnection = Open(db);
        Assert.AreEqual(Normalize(expected), Normalize(IndexSql(verifyConnection)));
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), ReadDbVersion(verifyConnection));
    }

    private static SqliteConnection Open(RepositoryTestDatabase db)
    {
        var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();
        return connection;
    }

    private static string? IndexSql(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type = 'index' AND name = @name;";
        cmd.Parameters.AddWithValue("@name", PendingIndex);
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
