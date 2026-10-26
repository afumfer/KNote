using KNote.Repository.EntityFramework;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.SchemaUpdateTests;

/// <summary>
/// Guards KntSchemaUpdater revision 6: the indexes on NoteTasks.StartDate and EndDate of the tasks panel, in a new
/// database (EnsureCreated, from the EF model) and added to one that predates them.
/// </summary>
[TestClass]
public class SchemaUpdaterRevision6Tests
{
    [TestMethod]
    public void NewDatabase_HasTheIndexes()
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository("Dapper");

        using var connection = Open(db);
        Assert.IsTrue(IndexExists(connection, "IX_NoteTasks_StartDate"));
        Assert.IsTrue(IndexExists(connection, "IX_NoteTasks_EndDate"));
    }

    [TestMethod]
    public void EnsureUpToDate_OnDatabasePredatingRevision6_AddsTheIndexes()
    {
        using var db = new RepositoryTestDatabase();
        using (var connection = Open(db))
        {
            Exec(connection, "DROP INDEX IF EXISTS \"IX_NoteTasks_StartDate\";");
            Exec(connection, "DROP INDEX IF EXISTS \"IX_NoteTasks_EndDate\";");
            Exec(connection, "UPDATE SystemValues SET [Value] = '5' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");
        }

        using var repo = db.CreateRepository("EntityFramework");

        using var verifyConnection = Open(db);
        Assert.IsTrue(IndexExists(verifyConnection, "IX_NoteTasks_StartDate"));
        Assert.IsTrue(IndexExists(verifyConnection, "IX_NoteTasks_EndDate"));
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), ReadDbVersion(verifyConnection));
    }

    private static SqliteConnection Open(RepositoryTestDatabase db)
    {
        var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();
        return connection;
    }

    private static bool IndexExists(SqliteConnection connection, string name)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = @name;";
        cmd.Parameters.AddWithValue("@name", name);
        return (long)cmd.ExecuteScalar()! > 0;
    }

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
