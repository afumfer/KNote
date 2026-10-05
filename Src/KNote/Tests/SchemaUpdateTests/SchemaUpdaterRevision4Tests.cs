using KNote.Repository.EntityFramework;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.SchemaUpdateTests;

/// <summary>
/// Guards KntSchemaUpdater revision 4: the roles stored in Users.RoleDefinition are renamed Public -> Guest
/// and ProjecManager -> ProjectManager (typo fix), and the no longer seeded "owner" user is deleted when
/// nothing references it. Mirrors SchemaUpdaterRevision3Tests - rolls a database back to the
/// pre-revision-4 shape (DB_VERSION 3), then checks KntRepositoryFactory.Create (the real startup path)
/// migrates it.
/// </summary>
[TestClass]
public class SchemaUpdaterRevision4Tests
{
    [TestMethod]
    public void EnsureUpToDate_OnDatabasePredatingRevision4_RenamesRoles()
    {
        using var db = new RepositoryTestDatabase(); // provisioned at the current (post-revision-4) model

        // Old role name -> expected value once migrated. The already-renamed values stand for a row
        // the step has already been applied to: they must come out untouched (the step is idempotent).
        var cases = new Dictionary<string, string>
        {
            ["Public"] = "Guest",
            ["ProjecManager"] = "ProjectManager",
            ["Public, Staff"] = "Guest, Staff",
            ["Staff, ProjecManager, Admin"] = "Staff, ProjectManager, Admin",
            ["Staff"] = "Staff",
            ["Admin"] = "Admin",
            ["Guest"] = "Guest",
            ["ProjectManager"] = "ProjectManager"
        };

        var userIds = new Dictionary<string, Guid>();

        using (var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}"))
        {
            connection.Open();

            foreach (var oldRoles in cases.Keys)
                userIds[oldRoles] = InsertUser(connection, oldRoles);

            Exec(connection, "UPDATE SystemValues SET [Value] = '3' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");
        }

        // Same entry point as every real ServiceRef/Server startup (KntRepositoryFactory.Create).
        using var repo = db.CreateRepository("Dapper");

        using var verifyConnection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        verifyConnection.Open();

        foreach (var (oldRoles, expectedRoles) in cases)
            Assert.AreEqual(expectedRoles, ReadRoleDefinition(verifyConnection, userIds[oldRoles]), $"Migrating \"{oldRoles}\"");

        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), ReadDbVersion(verifyConnection));
    }

    [TestMethod]
    public void FreshDatabase_SeedsOnlyCurrentRoleNames()
    {
        using var db = new RepositoryTestDatabase();

        using var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();

        Assert.AreEqual("Admin", ReadRoleDefinition(connection, "adminKNote"));
        Assert.AreEqual("Guest", ReadRoleDefinition(connection, "user1"));
        Assert.AreEqual(0L, CountUsersWithRoleLike(connection, "%Public%"));
        Assert.AreEqual(0L, CountUsersWithRoleLike(connection, "%ProjecManager%"));
    }

    [TestMethod]
    public void FreshDatabase_HasNoOwnerUser()
    {
        using var db = new RepositoryTestDatabase();

        using var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();

        Assert.IsNull(ReadRoleDefinition(connection, "owner"));
    }

    [TestMethod]
    public void EnsureUpToDate_DeletesTheSeededOwner_WhenNothingReferencesIt()
    {
        var ownerExists = MigrateWithOwner("owner", "owner@mydomain.com", referenced: false);

        Assert.IsFalse(ownerExists);
    }

    [TestMethod]
    public void EnsureUpToDate_KeepsTheSeededOwner_WhenSomethingReferencesIt()
    {
        var ownerExists = MigrateWithOwner("owner", "owner@mydomain.com", referenced: true);

        Assert.IsTrue(ownerExists);
    }

    [TestMethod]
    [DataRow("jdoe", "owner@mydomain.com")]   // the seeded owner after ClientWin renamed it to its Windows user
    [DataRow("owner", "owner@mycompany.com")] // a real user that happens to be called "owner"
    public void EnsureUpToDate_KeepsUsersThatAreNotTheSeededOwner(string userName, string email)
    {
        var ownerExists = MigrateWithOwner(userName, email, referenced: false);

        Assert.IsTrue(ownerExists);
    }

    // Rolls a database back to revision 3 with an Admin user like the old seeded "owner" (optionally
    // referenced by a KMessages row), runs the real startup path and tells whether that user survived.
    private static bool MigrateWithOwner(string userName, string email, bool referenced)
    {
        using var db = new RepositoryTestDatabase();
        var ownerId = Guid.NewGuid();

        using (var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}"))
        {
            connection.Open();

            InsertUser(connection, "Admin", ownerId, userName, email);
            if (referenced)
                Exec(connection, $"INSERT INTO KMessages (KMessageId, UserId, ActionType, NotificationType, AlarmType) VALUES ('{Guid.NewGuid().ToString().ToUpperInvariant()}', '{ownerId.ToString().ToUpperInvariant()}', 0, 0, 0);");

            Exec(connection, "UPDATE SystemValues SET [Value] = '3' WHERE Scope = 'SYSTEM' AND [Key] = 'DB_VERSION';");
        }

        using var repo = db.CreateRepository("Dapper");

        using var verifyConnection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        verifyConnection.Open();
        Assert.AreEqual(KntSchemaUpdater.CurrentSchemaRevision.ToString(), ReadDbVersion(verifyConnection));

        return ReadRoleDefinition(verifyConnection, ownerId) != null;
    }

    private static Guid InsertUser(SqliteConnection connection, string roleDefinition)
    {
        var id = Guid.NewGuid();
        InsertUser(connection, roleDefinition, id, $"rev4-{id:N}"[..24], $"{id:N}@knote.tests");
        return id;
    }

    private static void InsertUser(SqliteConnection connection, string roleDefinition, Guid id, string userName, string email)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Users (UserId, UserName, EMail, FullName, RoleDefinition, Disabled)
            VALUES (@id, @userName, @email, 'Revision 4 test user', @roles, 0);
            """;
        cmd.Parameters.AddWithValue("@id", id.ToString().ToUpperInvariant());
        cmd.Parameters.AddWithValue("@userName", userName);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@roles", roleDefinition);
        cmd.ExecuteNonQuery();
    }

    private static string? ReadRoleDefinition(SqliteConnection connection, Guid userId)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT RoleDefinition FROM Users WHERE UPPER(UserId) = @id;";
        cmd.Parameters.AddWithValue("@id", userId.ToString().ToUpperInvariant());
        return cmd.ExecuteScalar() as string;
    }

    private static string? ReadRoleDefinition(SqliteConnection connection, string userName)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT RoleDefinition FROM Users WHERE UserName = @userName;";
        cmd.Parameters.AddWithValue("@userName", userName);
        return cmd.ExecuteScalar() as string;
    }

    private static long CountUsersWithRoleLike(SqliteConnection connection, string pattern)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE RoleDefinition LIKE @pattern;";
        cmd.Parameters.AddWithValue("@pattern", pattern);
        return Convert.ToInt64(cmd.ExecuteScalar());
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
