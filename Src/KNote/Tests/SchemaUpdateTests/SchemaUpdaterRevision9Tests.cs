using KNote.Repository.EntityFramework;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.SchemaUpdateTests;

/// <summary>
/// Guards KntSchemaUpdater revision 9: the columns of who wrote the description of a resource, in a new database
/// (EnsureCreated, from the EF model) and added to one that predates them, keeping its resources unlocked.
/// </summary>
[TestClass]
public class SchemaUpdaterRevision9Tests
{
    private static readonly string[] Columns = ["DescriptionByUser", "DescriptionModel"];

    [TestMethod]
    public void NewDatabase_HasTheColumns()
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository("Dapper");

        using var connection = Open(db);
        var columns = ResourceColumns(connection);
        foreach (var column in Columns)
            Assert.IsTrue(columns.ContainsKey(column), column);
    }

    [TestMethod]
    public void EnsureUpToDate_OnDatabasePredatingRevision9_AddsTheColumnsAsANewDatabaseHasThem()
    {
        Dictionary<string, (string Type, bool NotNull)> expected;
        using (var fresh = new RepositoryTestDatabase())
        using (var connection = Open(fresh))
            expected = ResourceColumns(connection);

        using var db = new RepositoryTestDatabase();
        var resourceId = Guid.NewGuid();
        using (var connection = Open(db))
        {
            foreach (var column in Columns)
                Exec(connection, $"ALTER TABLE \"Resources\" DROP COLUMN \"{column}\";");
            Exec(connection, "UPDATE SystemValues SET [Value] = '8' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");
            // A resource of before: only its row matters here, not its note.
            Exec(connection, "PRAGMA foreign_keys = OFF;");
            Exec(connection, $"INSERT INTO Resources (ResourceId, Name, Description, [Order], ContentInDB, NoteId) VALUES ('{resourceId.ToString().ToUpperInvariant()}', 'photo.png', 'An old description', 0, 0, '{Guid.NewGuid().ToString().ToUpperInvariant()}');");
        }

        using var repo = db.CreateRepository("EntityFramework");

        using var verifyConnection = Open(db);
        var columns = ResourceColumns(verifyConnection);
        foreach (var column in Columns)
            Assert.AreEqual(expected[column], columns[column], column);
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), ReadDbVersion(verifyConnection));

        var resource = repo.Notes.GetResourceAsync(resourceId).GetAwaiter().GetResult().Entity;
        Assert.AreEqual("An old description", resource.Description);
        Assert.IsFalse(resource.DescriptionByUser);
        Assert.IsNull(resource.DescriptionModel);
    }

    private static SqliteConnection Open(RepositoryTestDatabase db)
    {
        var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();
        return connection;
    }

    // Name, type and NOT NULL of each column (an added column needs a default for its rows; a new table doesn't).
    private static Dictionary<string, (string Type, bool NotNull)> ResourceColumns(SqliteConnection connection)
    {
        var columns = new Dictionary<string, (string, bool)>(StringComparer.OrdinalIgnoreCase);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA table_info(\"Resources\")";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            columns[reader.GetString(1)] = (reader.GetString(2), reader.GetInt32(3) == 1);
        return columns;
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
