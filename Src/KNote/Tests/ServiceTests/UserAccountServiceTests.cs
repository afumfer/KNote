using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The user's own account (Users.GetProfileAsync, SaveProfileAsync, ChangePasswordAsync): always the account of
/// UserIdentityName, the email changes only with the current password, and new passwords follow KntPasswordPolicy
/// (also where an Admin sets one, and in registration).
/// </summary>
[TestClass]
public class UserAccountServiceTests
{
    private const string Password = "Account-Password-1!";
    private const string NewPassword = "Account-Password-2!";

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetProfile_IsTheCurrentUsersAccount(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var user = await CreateUserAsync(service);
        service.UserIdentityName = user.UserName;

        var res = await service.Users.GetProfileAsync();

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual(user.UserName, res.Entity.UserName);
        Assert.AreEqual(user.EMail, res.Entity.EMail);
        Assert.AreEqual(user.FullName, res.Entity.FullName);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("not-registered")]
    public async Task GetProfile_WithoutARegisteredUser_Fails(string? userName)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false) { UserIdentityName = userName };

        var res = await service.Users.GetProfileAsync();

        Assert.IsFalse(res.IsValid);
        Assert.IsNull(res.Entity);
    }

    [TestMethod]
    public async Task AccountCommands_RefuseADisabledUser()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false);
        var user = await CreateUserAsync(service);
        user.Disabled = true;
        Assert.IsTrue((await service.Repository.Users.UpdateAsync(user)).IsValid);
        service.UserIdentityName = user.UserName;

        Assert.IsFalse((await service.Users.GetProfileAsync()).IsValid);
        Assert.IsFalse((await service.Users.ChangePasswordAsync(new UserChangePasswordDto { CurrentPassword = Password, NewPassword = NewPassword })).IsValid);
        Assert.IsTrue(await CanSignInAsync(service, user.UserName, Password));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SaveProfile_ChangesTheFullName_WithoutPassword_AndNeverTheUserName(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var user = await CreateUserAsync(service);
        service.UserIdentityName = user.UserName;

        var res = await service.Users.SaveProfileAsync(new UserProfileDto { UserName = "someone-else", EMail = user.EMail, FullName = "  New full name " });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var stored = (await service.Repository.Users.GetAsync(user.UserId)).Entity;
        Assert.AreEqual(user.UserName, stored.UserName);
        Assert.AreEqual("New full name", stored.FullName);
        Assert.AreEqual(user.RoleDefinition, stored.RoleDefinition);
        Assert.IsTrue(await CanSignInAsync(service, user.UserName, Password), "Saving the profile keeps the password.");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SaveProfile_NewEmail_NeedsTheCurrentPassword(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var user = await CreateUserAsync(service);
        service.UserIdentityName = user.UserName;
        var newEMail = $"{Guid.NewGuid():N}@knote.tests";

        var withoutPassword = await service.Users.SaveProfileAsync(new UserProfileDto { EMail = newEMail, FullName = user.FullName });
        var wrongPassword = await service.Users.SaveProfileAsync(new UserProfileDto { EMail = newEMail, FullName = user.FullName, CurrentPassword = "Wrong-Password-1!" });
        Assert.IsFalse(withoutPassword.IsValid);
        Assert.IsFalse(wrongPassword.IsValid);
        Assert.AreEqual(user.EMail, (await service.Repository.Users.GetAsync(user.UserId)).Entity.EMail);

        var res = await service.Users.SaveProfileAsync(new UserProfileDto { EMail = newEMail, FullName = user.FullName, CurrentPassword = Password });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual(newEMail, (await service.Repository.Users.GetAsync(user.UserId)).Entity.EMail);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SaveProfile_AnotherUsersEmail_Fails(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var user = await CreateUserAsync(service);
        var other = await CreateUserAsync(service);
        service.UserIdentityName = user.UserName;

        var res = await service.Users.SaveProfileAsync(new UserProfileDto { EMail = other.EMail, FullName = user.FullName, CurrentPassword = Password });

        Assert.IsFalse(res.IsValid);
        StringAssert.Contains(res.ErrorMessage, "already in use");
    }

    [TestMethod]
    public async Task SaveProfile_InvalidProfile_Fails()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false);
        var user = await CreateUserAsync(service);
        service.UserIdentityName = user.UserName;

        var res = await service.Users.SaveProfileAsync(new UserProfileDto { EMail = user.EMail, FullName = "" });

        Assert.IsFalse(res.IsValid);
        Assert.AreEqual(user.FullName, (await service.Repository.Users.GetAsync(user.UserId)).Entity.FullName);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ChangePassword_WithTheCurrentOne_ReplacesIt(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var user = await CreateUserAsync(service);
        service.UserIdentityName = user.UserName;

        var res = await service.Users.ChangePasswordAsync(new UserChangePasswordDto { CurrentPassword = Password, NewPassword = NewPassword });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual(user.UserId, res.Entity.UserId);
        Assert.IsTrue(await CanSignInAsync(service, user.UserName, NewPassword));
        Assert.IsFalse(await CanSignInAsync(service, user.UserName, Password));
    }

    [TestMethod]
    [DataRow("Wrong-Password-1!", NewPassword, "not correct")]
    [DataRow("", NewPassword, "not correct")]
    [DataRow(Password, "short", "at least")]
    [DataRow(Password, Password, "different")]
    public async Task ChangePassword_Refused_KeepsTheCurrentOne(string currentPassword, string newPassword, string expectedError)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false);
        var user = await CreateUserAsync(service);
        service.UserIdentityName = user.UserName;

        var res = await service.Users.ChangePasswordAsync(new UserChangePasswordDto { CurrentPassword = currentPassword, NewPassword = newPassword });

        Assert.IsFalse(res.IsValid);
        StringAssert.Contains(res.ErrorMessage, expectedError);
        Assert.IsTrue(await CanSignInAsync(service, user.UserName, Password));
    }

    [TestMethod]
    public async Task ChangePassword_UserWithoutPassword_SetsTheFirstOne()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false);
        var user = new UserDto { UserId = Guid.NewGuid(), UserName = "no-password", EMail = "no-password@knote.tests", FullName = "No password", RoleDefinition = "Guest" };
        Assert.IsTrue((await service.Repository.Users.AddAsync(user)).IsValid);
        service.UserIdentityName = user.UserName;

        var res = await service.Users.ChangePasswordAsync(new UserChangePasswordDto { NewPassword = NewPassword });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.IsTrue(await CanSignInAsync(service, user.UserName, NewPassword));
    }

    [TestMethod]
    public async Task AccountCommands_RunForAGuest_WithAuthorizationEnforced()
    {
        using var db = new RepositoryTestDatabase();
        var admin = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false);
        var user = await CreateUserAsync(admin, "Guest");
        var service = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false) { UserIdentityName = user.UserName, EnforceAuthorization = true };

        Assert.IsTrue((await service.Users.GetProfileAsync()).IsValid);
        Assert.IsTrue((await service.Users.SaveProfileAsync(new UserProfileDto { EMail = user.EMail, FullName = "Guest name" })).IsValid);
        Assert.IsTrue((await service.Users.ChangePasswordAsync(new UserChangePasswordDto { CurrentPassword = Password, NewPassword = NewPassword })).IsValid);
    }

    [TestMethod]
    public async Task NewPasswords_FollowThePolicy_InRegisterAndSetPassword()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false);
        var user = await CreateUserAsync(service);

        var register = await service.Users.RegisterAsync(NewUser(password: "short"));
        var setPassword = await service.Users.SetPasswordAsync(user.UserId, user.UserName);

        Assert.IsFalse(register.IsValid);
        StringAssert.Contains(register.ErrorMessage, "at least");
        Assert.IsFalse(setPassword.IsValid);
        StringAssert.Contains(setPassword.ErrorMessage, "user name");
        Assert.IsTrue(await CanSignInAsync(service, user.UserName, Password));
    }

    [TestMethod]
    [DataRow(null, "required")]
    [DataRow("   ", "required")]
    [DataRow("1234567", "at least")]
    [DataRow("12345678", null)]
    [DataRow("JDoe1234", "user name")]
    public void PasswordPolicy(string? password, string? expectedError)
    {
        var error = KntPasswordPolicy.Validate(password!, "jdoe1234");

        if (expectedError == null)
            Assert.IsNull(error);
        else
            StringAssert.Contains(error, expectedError);
    }

    private static async Task<UserDto> CreateUserAsync(KntService service, string roleDefinition = "Staff")
    {
        var dto = NewUser(roleDefinition);
        var res = await service.Users.CreateAsync(dto);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static UserRegisterDto NewUser(string roleDefinition = "Staff", string password = Password)
    {
        var id = Guid.NewGuid();
        return new UserRegisterDto
        {
            UserName = $"acc-{id:N}"[..24],
            EMail = $"{id:N}@knote.tests",
            FullName = "Account test user",
            RoleDefinition = roleDefinition,
            Password = password
        };
    }

    private static async Task<bool> CanSignInAsync(KntService service, string userName, string password)
        => (await service.Users.AuthenticateAsync(new UserCredentialsDto { UserName = userName, Password = password })).IsValid;
}
