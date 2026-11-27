using KNote.Model;
using KNote.Repository.EntityFramework;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.SchemaUpdateTests;

/// <summary>
/// Guards KntSchemaUpdater revision 11 (the history of the notes): KLogs gets its new columns and indexes, PreviousContent (not in
/// the EF model: also in a new database) and the date the history starts (SYSTEM/HISTORY_START), also when a database comes
/// from before.
/// </summary>
[TestClass]
public class SchemaUpdaterRevision11Tests
{
    private static readonly string[] NewColumns =
        { "LastDateTime", "UserName", "Origin", "Fields", "ChangeDetails", "EntityTitle", "PreviousContentSize", "PreviousContent" };

    [TestMethod]
    public void NewDatabase_HasTheHistory_ThoughPreviousContentIsNotInTheEfModel()
    {
        using var db = new RepositoryTestDatabase();
        using (var connection = Open(db))
            Assert.IsFalse(Columns(connection).Contains("PreviousContent"), "EnsureCreated doesn't make it");

        using var repo = db.CreateRepository("EntityFramework");

        using var verify = Open(db);
        foreach (var column in NewColumns)
            Assert.IsTrue(Columns(verify).Contains(column), column);
        Assert.IsTrue(Indexes(verify).Contains("IX_KLogs_EntityName_EntityId_LastDateTime"));
        Assert.IsTrue(Indexes(verify).Contains("IX_KLogs_LastDateTime"));
        Assert.IsTrue(DateTime.TryParse(HistoryStart(verify), out _));
    }

    [TestMethod]
    public void EnsureUpToDate_OnDatabasePredatingRevision11_AddsTheHistory()
    {
        using var db = new RepositoryTestDatabase();
        using (var connection = Open(db))
        {
            // KLogs as it was until revision 10, and no start of the history.
            Exec(connection, "DROP TABLE \"KLogs\";");
            Exec(connection, @"CREATE TABLE ""KLogs"" (
                ""KLogId"" TEXT NOT NULL CONSTRAINT ""PK_KLogs"" PRIMARY KEY,
                ""EntityId"" TEXT NOT NULL,
                ""EntityName"" TEXT NOT NULL,
                ""RegistryDateTime"" TEXT NOT NULL,
                ""RegistryMessage"" TEXT NOT NULL,
                ""ActionType"" TEXT NULL);");
            Exec(connection, $"DELETE FROM SystemValues WHERE Scope = 'SYSTEM' AND [Key] = '{KntConst.HistoryStartKey}';");
            Exec(connection, "UPDATE SystemValues SET [Value] = '10' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");
        }

        using var repo = db.CreateRepository("Dapper");

        using var verify = Open(db);
        foreach (var column in NewColumns)
            Assert.IsTrue(Columns(verify).Contains(column), column);
        Assert.IsTrue(Indexes(verify).Contains("IX_KLogs_EntityName_EntityId_LastDateTime"));
        Assert.IsTrue(Indexes(verify).Contains("IX_KLogs_LastDateTime"));
        Assert.IsTrue(DateTime.TryParse(HistoryStart(verify), out _));
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(),
            Scalar(verify, "SELECT [Value] FROM SystemValues WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';"));
    }

    private static string? HistoryStart(SqliteConnection connection) =>
        Scalar(connection, $"SELECT [Value] FROM SystemValues WHERE Scope = 'SYSTEM' AND [Key] = '{KntConst.HistoryStartKey}';");

    private static SqliteConnection Open(RepositoryTestDatabase db)
    {
        var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();
        return connection;
    }

    private static HashSet<string> Columns(SqliteConnection connection) => Names(connection, "PRAGMA table_info(\"KLogs\")", 1);

    private static HashSet<string> Indexes(SqliteConnection connection) => Names(connection, "PRAGMA index_list(\"KLogs\")", 1);

    private static HashSet<string> Names(SqliteConnection connection, string sql, int column)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            names.Add(reader.GetString(column));
        return names;
    }

    private static string? Scalar(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteScalar()?.ToString();
    }

    private static void Exec(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
