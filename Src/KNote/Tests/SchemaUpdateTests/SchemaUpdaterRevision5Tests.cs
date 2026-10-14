using KNote.Repository.EntityFramework;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.SchemaUpdateTests;

/// <summary>
/// Guards KntSchemaUpdater revision 5: every database gets its identity in SystemValues - DB_REPOSITORYID, a
/// Guid of its own, and DB_REPOSITORYNAME, the name of the RepositoryRef it is opened with (the old alias of
/// a ClientWin configuration) or else the database's own name - and an identity already there is never
/// changed. Goes through KntRepositoryFactory.Create, the real startup path, like the other revisions' tests.
/// </summary>
[TestClass]
public class SchemaUpdaterRevision5Tests
{
    [TestMethod]
    public void NewDatabase_GetsAnIdAndTheNameOfItsRepositoryRef()
    {
        // RepositoryTestDatabase creates the database at the current revision, without going through
        // step 5 - like any database created by EnsureCreated.
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository("Dapper", "My notes");

        using var connection = Open(db);
        Assert.AreNotEqual(Guid.Empty, ReadRepositoryId(connection));
        Assert.AreEqual("My notes", ReadSystemValue(connection, "DB_REPOSITORYNAME"));
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), ReadSystemValue(connection, "DB_VERSION"));
    }

    [TestMethod]
    public void EachDatabase_GetsItsOwnId()
    {
        using var db1 = new RepositoryTestDatabase();
        using var db2 = new RepositoryTestDatabase();
        using var repo1 = db1.CreateRepository("Dapper");
        using var repo2 = db2.CreateRepository("EntityFramework");

        using var connection1 = Open(db1);
        using var connection2 = Open(db2);
        Assert.AreNotEqual(ReadRepositoryId(connection1), ReadRepositoryId(connection2));
    }

    [TestMethod]
    [DataRow("Personal repository", "Personal repository")]
    [DataRow("  Personal repository  ", "Personal repository")]
    public void EnsureUpToDate_OnDatabasePredatingRevision5_NamesItAfterTheAlias(string alias, string expectedName)
    {
        using var db = new RepositoryTestDatabase();
        RollBackToRevision4(db);

        using var repo = db.CreateRepository("Dapper", alias);

        using var connection = Open(db);
        Assert.AreNotEqual(Guid.Empty, ReadRepositoryId(connection));
        Assert.AreEqual(expectedName, ReadSystemValue(connection, "DB_REPOSITORYNAME"));
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), ReadSystemValue(connection, "DB_VERSION"));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void EnsureUpToDate_WithoutAlias_NamesItAfterTheDatabase(string? alias)
    {
        using var db = new RepositoryTestDatabase();
        RollBackToRevision4(db);

        using var repo = db.CreateRepository("EntityFramework", alias);

        using var connection = Open(db);
        Assert.AreEqual(Path.GetFileNameWithoutExtension(db.DatabaseFilePath), ReadSystemValue(connection, "DB_REPOSITORYNAME"));
    }

    [TestMethod]
    public void EnsureUpToDate_KeepsAnExistingIdentity()
    {
        using var db = new RepositoryTestDatabase();
        var id = Guid.NewGuid();
        RollBackToRevision4(db, connection =>
        {
            InsertSystemValue(connection, "DB_REPOSITORYID", id.ToString());
            InsertSystemValue(connection, "DB_REPOSITORYNAME", "Team notes");
        });

        using var repo = db.CreateRepository("Dapper", "Another name");

        using var verifyConnection = Open(db);
        Assert.AreEqual(id, ReadRepositoryId(verifyConnection));
        Assert.AreEqual("Team notes", ReadSystemValue(verifyConnection, "DB_REPOSITORYNAME"));
        Assert.AreEqual(1L, CountSystemValues(verifyConnection, "DB_REPOSITORYID"));
        Assert.AreEqual(1L, CountSystemValues(verifyConnection, "DB_REPOSITORYNAME"));
    }

    // A database as it was before revision 5: no identity rows, DB_VERSION 4.
    private static void RollBackToRevision4(RepositoryTestDatabase db, Action<SqliteConnection>? afterwards = null)
    {
        using var connection = Open(db);
        Exec(connection, "DELETE FROM SystemValues WHERE Scope = 'SYSTEM' AND [Key] IN ('DB_REPOSITORYID', 'DB_REPOSITORYNAME');");
        Exec(connection, "UPDATE SystemValues SET [Value] = '4' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");
        afterwards?.Invoke(connection);
    }

    private static SqliteConnection Open(RepositoryTestDatabase db)
    {
        var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();
        return connection;
    }

    private static Guid ReadRepositoryId(SqliteConnection connection)
        => Guid.TryParse(ReadSystemValue(connection, "DB_REPOSITORYID"), out var id) ? id : Guid.Empty;

    private static string? ReadSystemValue(SqliteConnection connection, string key)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT [Value] FROM SystemValues WHERE Scope = 'SYSTEM' AND [Key] = @key;";
        cmd.Parameters.AddWithValue("@key", key);
        return cmd.ExecuteScalar() as string;
    }

    private static long CountSystemValues(SqliteConnection connection, string key)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM SystemValues WHERE Scope = 'SYSTEM' AND [Key] = @key;";
        cmd.Parameters.AddWithValue("@key", key);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    private static void InsertSystemValue(SqliteConnection connection, string key, string value)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO SystemValues (SystemValueId, Scope, [Key], [Value]) VALUES (@id, 'SYSTEM', @key, @value);";
        cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString().ToUpperInvariant());
        cmd.Parameters.AddWithValue("@key", key);
        cmd.Parameters.AddWithValue("@value", value);
        cmd.ExecuteNonQuery();
    }

    private static void Exec(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
