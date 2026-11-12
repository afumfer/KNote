using KNote.Repository.EntityFramework;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.SchemaUpdateTests;

/// <summary>
/// Guards KntSchemaUpdater revision 10 (KNote Atlas, stage 4, D2): the columns of the text of the documents (not in the EF
/// model: also in a new database), the page of the fragments, and the notes with documents marked to be indexed again when a
/// database comes from before.
/// </summary>
[TestClass]
public class SchemaUpdaterRevision10Tests
{
    [TestMethod]
    public void NewDatabase_HasTheColumns_ThoughTheyAreNotInTheEfModel()
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository("EntityFramework");

        using var connection = Open(db);
        var resources = Columns(connection, "Resources");
        Assert.AreEqual(("TEXT", false), resources["ExtractedText"]);
        Assert.AreEqual(("INTEGER", true), resources["ExtractedState"]);
        Assert.AreEqual(("INTEGER", false), Columns(connection, "NoteChunks")["Page"]);
    }

    [TestMethod]
    public void EnsureUpToDate_OnDatabasePredatingRevision10_AddsTheColumns_AndMarksTheNotesWithDocuments()
    {
        using var db = new RepositoryTestDatabase();
        var withPdf = Guid.NewGuid();
        var withPhoto = Guid.NewGuid();
        using (var connection = Open(db))
        {
            // EnsureCreated (RepositoryTestDatabase) doesn't make the columns of the text: they are not in the EF model.
            Assert.IsFalse(Columns(connection, "Resources").ContainsKey("ExtractedText"));
            Exec(connection, "ALTER TABLE \"NoteChunks\" DROP COLUMN \"Page\";");
            Exec(connection, "UPDATE SystemValues SET [Value] = '9' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");
            // Rows of before: only their rows matter here, not their notes.
            Exec(connection, "PRAGMA foreign_keys = OFF;");
            AddResource(connection, withPdf, "manual.PDF", "application/octet-stream");
            AddResource(connection, withPhoto, "photo.png", "image/png");
            AddChunk(connection, withPdf);
            AddChunk(connection, withPhoto);
        }

        using var repo = db.CreateRepository("Dapper");

        using var verifyConnection = Open(db);
        Assert.IsTrue(Columns(verifyConnection, "Resources").ContainsKey("ExtractedText"));
        Assert.IsTrue(Columns(verifyConnection, "NoteChunks").ContainsKey("Page"));
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), Scalar(verifyConnection, "SELECT [Value] FROM SystemValues WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';"));
        Assert.AreEqual("1", Scalar(verifyConnection, $"SELECT Stale FROM NoteChunks WHERE NoteId = '{Id(withPdf)}';"));
        Assert.AreEqual("0", Scalar(verifyConnection, $"SELECT Stale FROM NoteChunks WHERE NoteId = '{Id(withPhoto)}';"));
        Assert.AreEqual("0", Scalar(verifyConnection, $"SELECT ExtractedState FROM Resources WHERE NoteId = '{Id(withPdf)}';"));
    }

    private static string Id(Guid id) => id.ToString().ToUpperInvariant();

    private static void AddResource(SqliteConnection connection, Guid noteId, string name, string fileType) =>
        Exec(connection, "INSERT INTO Resources (ResourceId, Name, Description, DescriptionByUser, [Order], ContentInDB, FileType, NoteId) " +
            $"VALUES ('{Id(Guid.NewGuid())}', '{name}', '{name}', 0, 0, 0, '{fileType}', '{Id(noteId)}');");

    private static void AddChunk(SqliteConnection connection, Guid noteId) =>
        Exec(connection, "INSERT INTO NoteChunks (NoteChunkId, NoteId, SourceType, SourceId, ChunkIndex, Content, ContentHash, SourceHash, " +
            $"Stale, Model, Dimensions, Vector, IndexedDateTime) VALUES ('{Id(Guid.NewGuid())}', '{Id(noteId)}', 0, '{Id(Guid.Empty)}', 0, " +
            "'Topic: x', 'c', 's', 0, 'fake-embed', 2, X'0000803F00000000', '2026-01-01 00:00:00');");

    private static SqliteConnection Open(RepositoryTestDatabase db)
    {
        var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();
        return connection;
    }

    private static Dictionary<string, (string Type, bool NotNull)> Columns(SqliteConnection connection, string table)
    {
        var columns = new Dictionary<string, (string, bool)>(StringComparer.OrdinalIgnoreCase);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info(\"{table}\")";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            columns[reader.GetString(1)] = (reader.GetString(2), reader.GetInt32(3) == 1);
        return columns;
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
