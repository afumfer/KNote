using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The repository's identity (DB_REPOSITORYID/DB_REPOSITORYNAME) through the Service layer: anybody can read
/// it, only an Admin can rename the repository, nobody can change it with the generic SystemValues commands,
/// and ServiceRef takes it from the database when it opens the repository.
/// </summary>
[TestClass]
public class RepositoryIdentityServiceTests
{
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetRepositoryInfo_IsReadableByAnUnregisteredUser(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, orm, "nobody", "Shared notes");

        var res = await service.SystemValues.GetRepositoryInfoAsync();

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreNotEqual(Guid.Empty, res.Entity.RepositoryId);
        Assert.AreEqual("Shared notes", res.Entity.Name);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SaveRepositoryName_Admin_RenamesTheRepositoryAndKeepsItsId(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, orm, "admin");
        await AddUserAsync(service, "admin", "Admin");
        var before = (await service.SystemValues.GetRepositoryInfoAsync()).Entity;

        var res = await service.SystemValues.SaveRepositoryNameAsync("  Renamed repository  ");

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual("Renamed repository", res.Entity.Name);
        Assert.AreEqual(before.RepositoryId, res.Entity.RepositoryId);
        Assert.AreEqual("Renamed repository", (await service.SystemValues.GetRepositoryInfoAsync()).Entity.Name);
    }

    [TestMethod]
    [DataRow("nobody", null)]
    [DataRow("pm", "ProjectManager")]
    public async Task SaveRepositoryName_NeedsAdmin(string userName, string? roleDefinition)
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, "Dapper", userName, "Original name");
        if (roleDefinition != null)
            await AddUserAsync(service, userName, roleDefinition);

        var res = await service.SystemValues.SaveRepositoryNameAsync("Renamed repository");

        Assert.IsTrue(res.NotAuthorized);
        Assert.AreEqual("Original name", (await service.SystemValues.GetRepositoryInfoAsync()).Entity.Name);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task SaveRepositoryName_RequiresAName(string? name)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper", "Original name"));

        var res = await service.SystemValues.SaveRepositoryNameAsync(name!);

        Assert.IsFalse(res.IsValid);
        Assert.AreEqual("Original name", (await service.SystemValues.GetRepositoryInfoAsync()).Entity.Name);
    }

    [TestMethod]
    public async Task SaveRepositoryName_RefusesTooLongNames()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"));

        var res = await service.SystemValues.SaveRepositoryNameAsync(new string('x', 257));

        Assert.IsFalse(res.IsValid);
    }

    [TestMethod]
    [DataRow("DB_VERSION")]
    [DataRow("DB_REPOSITORYID")]
    [DataRow("DB_REPOSITORYNAME")]
    public async Task GenericSystemValueCommands_CannotChangeOrDeleteTheProtectedRows(string key)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"));
        var stored = (await service.SystemValues.GetAsync(new KeyValuePair<string, string>("SYSTEM", key))).Entity;

        var update = await service.SystemValues.SaveAsync(new SystemValueDto
        {
            SystemValueId = stored.SystemValueId, Scope = "SYSTEM", Key = key, Value = "changed"
        });
        var turnIntoAnotherRow = await service.SystemValues.SaveAsync(new SystemValueDto
        {
            SystemValueId = stored.SystemValueId, Scope = "TESTS", Key = "ANY_KEY", Value = "changed"
        });
        var insertAnother = await service.SystemValues.SaveAsync(new SystemValueDto
        {
            SystemValueId = Guid.Empty, Scope = "system", Key = key.ToLowerInvariant(), Value = "changed"
        });
        var delete = await service.SystemValues.DeleteAsync(stored.SystemValueId);

        Assert.IsFalse(update.IsValid);
        Assert.IsFalse(turnIntoAnotherRow.IsValid);
        Assert.IsFalse(insertAnother.IsValid);
        Assert.IsFalse(delete.IsValid);

        var after = await service.SystemValues.GetAsync(stored.SystemValueId);
        Assert.AreEqual(stored.Value, after.Entity?.Value);
        Assert.AreEqual(key, after.Entity?.Key);
    }

    [TestMethod]
    public async Task GenericSystemValueCommands_StillWorkForOtherRows()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"));

        var save = await service.SystemValues.SaveAsync(new SystemValueDto { Scope = "TESTS", Key = "ANY_KEY", Value = "1" });
        Assert.IsTrue(save.IsValid, save.ErrorMessage);

        var delete = await service.SystemValues.DeleteAsync(save.Entity.SystemValueId);
        Assert.IsTrue(delete.IsValid, delete.ErrorMessage);
    }

    [TestMethod]
    public void ServiceRef_TakesTheIdentityFromTheDatabase()
    {
        using var db = new RepositoryTestDatabase();
        var first = new ServiceRef(NewRepositoryRef(db, "First name"), "jdoe");

        // Another client opening the same database with another (old) alias: the database's name wins.
        var second = new ServiceRef(NewRepositoryRef(db, "Another alias"), "jdoe");

        Assert.AreNotEqual(Guid.Empty, first.RepositoryId);
        Assert.AreEqual("First name", first.Name);
        Assert.AreEqual(first.RepositoryId, second.RepositoryId);
        Assert.AreEqual("First name", second.Name);
        Assert.AreEqual(first.RepositoryId, second.RepositoryRef.RepositoryId);
    }

    private static RepositoryRef NewRepositoryRef(RepositoryTestDatabase db, string name) => new()
    {
        Name = name,
        Orm = "Dapper",
        Provider = "Microsoft.Data.Sqlite",
        ConnectionString = $"Data Source={db.DatabaseFilePath}"
    };

    private static KntService EnforcedService(RepositoryTestDatabase db, string orm, string userName, string name = "KNoteParityTests")
        => new(db.CreateRepository(orm, name)) { UserIdentityName = userName, EnforceAuthorization = true };

    // Straight through the repository: commands would need an Admin to create users.
    private static async Task AddUserAsync(KntService service, string userName, string roleDefinition)
    {
        var res = await service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(),
            UserName = userName,
            EMail = $"{userName}-{Guid.NewGuid():N}@knote.tests",
            FullName = userName,
            RoleDefinition = roleDefinition
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
    }
}
