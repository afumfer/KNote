using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// Users.RegisterAsync (KntUsersRegisterAsyncCommand): the role of a self-registered user is decided by
/// the service, never taken from the caller. While the repository has at most one Admin (the seeded
/// adminKNote) the new user becomes an Admin; from the second Admin on, new users are Guests.
/// Users.CreateAsync (an Admin creating a user from the users management screen) keeps the requested role.
/// </summary>
[TestClass]
public class UserRegisterPolicyTests
{
    [TestMethod]
    [DataRow(0, EnumRoles.Admin)]
    [DataRow(1, EnumRoles.Admin)]
    [DataRow(2, EnumRoles.Guest)]
    [DataRow(5, EnumRoles.Guest)]
    public void RoleForNewUser_DependsOnTheNumberOfAdmins(int adminCount, EnumRoles expected)
    {
        Assert.AreEqual(expected, KntUsersRegisterAsyncCommand.RoleForNewUser(adminCount));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task FreshDatabase_FirstRegisteredUser_IsAdmin_WhateverItAsksFor(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);

        var dto = NewUser(requestedRole: "Guest");
        var res = await service.Users.RegisterAsync(dto);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual("Admin", await StoredRoleAsync(service, dto.UserName));
        Assert.AreEqual("Admin", dto.RoleDefinition, "The assigned role is written back into the DTO.");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task WithTwoAdmins_RegisteredUser_IsGuest_EvenAskingForAdmin(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        Assert.IsTrue((await service.Users.RegisterAsync(NewUser())).IsValid); // becomes the second Admin

        var dto = NewUser(requestedRole: "Admin");
        var res = await service.Users.RegisterAsync(dto);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual("Guest", await StoredRoleAsync(service, dto.UserName));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task WithoutAnyAdmin_RegisteredUser_IsAdmin(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        await SetRoleAsync(service, "adminKNote", "Staff");

        var dto = NewUser();
        var res = await service.Users.RegisterAsync(dto);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual("Admin", await StoredRoleAsync(service, dto.UserName));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AdminNamedAmongOtherRoles_CountsAsAdmin(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        await SetRoleAsync(service, "user1", "Staff, Admin"); // second Admin next to adminKNote

        var dto = NewUser();
        var res = await service.Users.RegisterAsync(dto);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual("Guest", await StoredRoleAsync(service, dto.UserName));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task CreateAsync_KeepsTheRequestedRole(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);

        var dto = NewUser(requestedRole: "Staff, ProjectManager");
        var res = await service.Users.CreateAsync(dto);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual("Staff, ProjectManager", await StoredRoleAsync(service, dto.UserName));
    }

    private static UserRegisterDto NewUser(string? requestedRole = null)
    {
        var id = Guid.NewGuid();
        return new UserRegisterDto
        {
            UserName = $"reg-{id:N}"[..24],
            EMail = $"{id:N}@knote.tests",
            FullName = "Register policy test user",
            RoleDefinition = requestedRole,
            Password = "Register-Policy-Password-1!"
        };
    }

    private static async Task<string?> StoredRoleAsync(KntService service, string userName)
    {
        var res = await service.Repository.Users.GetByUserNameAsync(userName);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity.RoleDefinition;
    }

    private static async Task SetRoleAsync(KntService service, string userName, string roleDefinition)
    {
        var user = (await service.Repository.Users.GetByUserNameAsync(userName)).Entity;
        user.RoleDefinition = roleDefinition;
        var res = await service.Repository.Users.UpdateAsync(user);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
    }
}
