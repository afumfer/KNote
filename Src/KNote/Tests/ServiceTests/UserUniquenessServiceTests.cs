using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// A user's name and email are unique, a rule of the domain checked by the Service (KntUserUniqueness) where a user is
/// saved (Users.SaveAsync, the Admin's users editor; CreateAsync/RegisterAsync; SaveProfileAsync): the caller gets an
/// invalid Result that names the repeated value, not the error of the database's unique indexes, and nothing is saved.
/// </summary>
[TestClass]
public class UserUniquenessServiceTests
{
    private const string Password = "Uniqueness-Password-1!";

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Save_AnotherUsersEmail_IsARuleOfTheDomain_AndNothingChanges(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var (user, other) = (await CreateUserAsync(service), await CreateUserAsync(service));
        var originalEMail = user.EMail;
        user.EMail = other.EMail;
        user.FullName = "Changed";

        var res = await service.Users.SaveAsync(user);

        Assert.IsFalse(res.IsValid);
        Assert.AreEqual($"Email \"{other.EMail}\" is already in use.", res.ErrorMessage.Trim());
        var saved = (await service.Users.GetAsync(user.UserId)).Entity;
        Assert.AreEqual(originalEMail, saved.EMail);
        Assert.AreNotEqual("Changed", saved.FullName);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Save_AnotherUsersName_IsARuleOfTheDomain(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var (user, other) = (await CreateUserAsync(service), await CreateUserAsync(service));
        user.UserName = other.UserName;

        var res = await service.Users.SaveAsync(user);

        Assert.IsFalse(res.IsValid);
        Assert.AreEqual($"Username \"{other.UserName}\" is already taken.", res.ErrorMessage.Trim());
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Save_KeepingItsOwnNameAndEmail_IsValid(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var user = await CreateUserAsync(service);
        user.FullName = "Renamed user";

        var res = await service.Users.SaveAsync(user);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual("Renamed user", (await service.Users.GetAsync(user.UserId)).Entity.FullName);
    }

    [TestMethod]
    public async Task Save_NewUserWithAnotherUsersEmail_IsARuleOfTheDomain()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false);
        var other = await CreateUserAsync(service);

        var res = await service.Users.SaveAsync(new UserDto { UserName = "new-user", EMail = other.EMail, FullName = "New", RoleDefinition = "Guest" });

        Assert.IsFalse(res.IsValid);
        StringAssert.Contains(res.ErrorMessage, "is already in use");
        Assert.IsNull((await service.Users.GetByUserNameAsync("new-user")).Entity);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task CreateAndRegister_RepeatedNameOrEmail_AnInvalidResult_NotAnException(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var other = await CreateUserAsync(service);

        var sameName = NewUser();
        sameName.UserName = other.UserName;
        var sameEMail = NewUser();
        sameEMail.EMail = other.EMail;

        var byName = await service.Users.CreateAsync(sameName);
        var byEMail = await service.Users.RegisterAsync(sameEMail);

        Assert.IsFalse(byName.IsValid);
        Assert.AreEqual($"Username \"{other.UserName}\" is already taken.", byName.ErrorMessage.Trim());
        Assert.IsFalse(byEMail.IsValid);
        Assert.AreEqual($"Email \"{other.EMail}\" is already in use.", byEMail.ErrorMessage.Trim());
    }

    private static async Task<UserDto> CreateUserAsync(KntService service)
    {
        var res = await service.Users.CreateAsync(NewUser());
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static UserRegisterDto NewUser()
    {
        var id = Guid.NewGuid();
        return new UserRegisterDto
        {
            UserName = $"unq-{id:N}"[..24],
            EMail = $"{id:N}@knote.tests",
            FullName = $"Uniqueness {id:N}"[..24],
            RoleDefinition = nameof(EnumRoles.Staff),
            Password = Password
        };
    }
}
