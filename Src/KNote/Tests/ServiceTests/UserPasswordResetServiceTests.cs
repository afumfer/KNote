using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// "Forgot your password?" in the Service: Users.CreatePasswordResetAsync gives a token for the user with that name or
/// email (signed with the caller's key), and Users.ResetPasswordAsync sets a new password with it, once, before it
/// expires, only with the same key.
/// </summary>
[TestClass]
public class UserPasswordResetServiceTests
{
    private const string Key = "Password-Reset-Tests-Signing-Key-1234567890";
    private const string Password = "Reset-Password-1!";
    private const string NewPassword = "Reset-Password-2!";
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(30);

    private readonly List<RepositoryTestDatabase> _databases = new();

    [TestCleanup]
    public void Cleanup() => _databases.ForEach(db => db.Dispose());

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Token_SetsANewPassword_Once(string orm)
    {
        var (service, user, _) = await CreateAsync(orm);

        var reset = await service.Users.CreatePasswordResetAsync(user.UserName, Key, Lifetime);
        Assert.IsTrue(reset.IsValid, reset.ErrorMessage);
        Assert.AreEqual(user.EMail, reset.Entity.EMail);
        Assert.IsTrue(reset.Entity.ExpiresUtc > DateTime.UtcNow.AddMinutes(29));

        var res = await service.Users.ResetPasswordAsync(new UserResetPasswordDto { Token = reset.Entity.Token, NewPassword = NewPassword }, Key);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual(user.UserId, res.Entity.UserId);
        Assert.IsTrue(await CanSignInAsync(service, user.UserName, NewPassword));
        Assert.IsFalse(await CanSignInAsync(service, user.UserName, Password));

        var again = await service.Users.ResetPasswordAsync(new UserResetPasswordDto { Token = reset.Entity.Token, NewPassword = "Reset-Password-3!" }, Key);
        Assert.IsFalse(again.IsValid, "The password changed: the token no longer works.");
        Assert.IsTrue(await CanSignInAsync(service, user.UserName, NewPassword));
    }

    [TestMethod]
    public async Task Token_ForTheUsersEmail()
    {
        var (service, user, _) = await CreateAsync("Dapper");

        var reset = await service.Users.CreatePasswordResetAsync($"  {user.EMail} ", Key, Lifetime);

        Assert.IsTrue(reset.IsValid, reset.ErrorMessage);
        Assert.AreEqual(user.UserName, reset.Entity.UserName);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("nobody-registered")]
    public async Task NoUser_NoToken(string? userNameOrEMail)
    {
        var (service, _, _) = await CreateAsync("Dapper");

        var reset = await service.Users.CreatePasswordResetAsync(userNameOrEMail!, Key, Lifetime);

        Assert.IsFalse(reset.IsValid);
        Assert.IsNull(reset.Entity);
    }

    [TestMethod]
    public async Task DisabledUser_NoToken_AndAnEarlierOneDoesNotWork()
    {
        var (service, user, _) = await CreateAsync("Dapper");
        var earlier = await service.Users.CreatePasswordResetAsync(user.UserName, Key, Lifetime);
        user.Disabled = true;
        Assert.IsTrue((await service.Repository.Users.UpdateAsync(user)).IsValid);

        Assert.IsFalse((await service.Users.CreatePasswordResetAsync(user.UserName, Key, Lifetime)).IsValid);
        Assert.IsFalse((await service.Users.ResetPasswordAsync(new UserResetPasswordDto { Token = earlier.Entity.Token, NewPassword = NewPassword }, Key)).IsValid);
    }

    [TestMethod]
    public async Task ExpiredToken_DoesNotWork()
    {
        var (service, user, _) = await CreateAsync("Dapper");
        var reset = await service.Users.CreatePasswordResetAsync(user.UserName, Key, TimeSpan.FromMinutes(-1));

        var res = await service.Users.ResetPasswordAsync(new UserResetPasswordDto { Token = reset.Entity.Token, NewPassword = NewPassword }, Key);

        Assert.IsFalse(res.IsValid);
        StringAssert.Contains(res.ErrorMessage, "expired");
        Assert.IsTrue(await CanSignInAsync(service, user.UserName, Password));
    }

    [TestMethod]
    public async Task TokenFromAnotherKey_OrChanged_DoesNotWork()
    {
        var (service, user, other) = await CreateAsync("Dapper");
        var reset = await service.Users.CreatePasswordResetAsync(user.UserName, Key, Lifetime);
        var otherReset = await service.Users.CreatePasswordResetAsync(other.UserName, Key, Lifetime);
        var token = reset.Entity.Token;
        // The other user's payload with this user's signature.
        var mixed = $"{otherReset.Entity.Token.Split('.')[0]}.{token.Split('.')[1]}";

        foreach (var (candidate, key) in new[] { (token, "Another-Signing-Key-1234567890"), (mixed, Key), ("not-a-token", Key), ("", Key), (token + "x", Key) })
        {
            var res = await service.Users.ResetPasswordAsync(new UserResetPasswordDto { Token = candidate, NewPassword = NewPassword }, key);
            Assert.IsFalse(res.IsValid, candidate);
        }

        Assert.IsTrue(await CanSignInAsync(service, user.UserName, Password));
        Assert.IsTrue(await CanSignInAsync(service, other.UserName, Password));
    }

    [TestMethod]
    public async Task NewPasswordAgainstThePolicy_IsRefused_AndTheTokenStillWorks()
    {
        var (service, user, _) = await CreateAsync("Dapper");
        var reset = await service.Users.CreatePasswordResetAsync(user.UserName, Key, Lifetime);

        var refused = await service.Users.ResetPasswordAsync(new UserResetPasswordDto { Token = reset.Entity.Token, NewPassword = "short" }, Key);
        var accepted = await service.Users.ResetPasswordAsync(new UserResetPasswordDto { Token = reset.Entity.Token, NewPassword = NewPassword }, Key);

        Assert.IsFalse(refused.IsValid);
        StringAssert.Contains(refused.ErrorMessage, "at least");
        Assert.IsTrue(accepted.IsValid, accepted.ErrorMessage);
    }

    [TestMethod]
    public async Task PasswordChangedMeanwhile_TheTokenDoesNotWork()
    {
        var (service, user, _) = await CreateAsync("Dapper");
        var reset = await service.Users.CreatePasswordResetAsync(user.UserName, Key, Lifetime);
        service.UserIdentityName = user.UserName;
        Assert.IsTrue((await service.Users.ChangePasswordAsync(new UserChangePasswordDto { CurrentPassword = Password, NewPassword = NewPassword })).IsValid);

        var res = await service.Users.ResetPasswordAsync(new UserResetPasswordDto { Token = reset.Entity.Token, NewPassword = "Reset-Password-3!" }, Key);

        Assert.IsFalse(res.IsValid);
    }

    [TestMethod]
    public async Task Commands_RunWithoutAUser_WithAuthorizationEnforced()
    {
        using var db = new RepositoryTestDatabase();
        var setup = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false);
        var user = (await setup.Users.CreateAsync(NewUser())).Entity;
        var service = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false) { UserIdentityName = null, EnforceAuthorization = true };

        var reset = await service.Users.CreatePasswordResetAsync(user.UserName, Key, Lifetime);
        Assert.IsTrue(reset.IsValid, reset.ErrorMessage);
        Assert.IsTrue((await service.Users.ResetPasswordAsync(new UserResetPasswordDto { Token = reset.Entity.Token, NewPassword = NewPassword }, Key)).IsValid);
    }

    [TestMethod]
    public void Params_DoNotShowTheirSecrets()
    {
        // KntServiceBase traces each command's Param.
        var request = new KntPasswordResetRequestParam { UserNameOrEMail = "jdoe", SigningKey = Key, Lifetime = Lifetime }.ToString();
        var reset = new KntPasswordResetParam { Reset = new UserResetPasswordDto { Token = "t", NewPassword = NewPassword }, SigningKey = Key }.ToString();
        var setPassword = new KntSetPasswordParam { UserId = Guid.NewGuid(), NewPassword = NewPassword }.ToString();

        foreach (var text in new[] { request, reset, setPassword })
        {
            Assert.IsFalse(text.Contains(Key), text);
            Assert.IsFalse(text.Contains(NewPassword), text);
        }
    }

    private async Task<(KntService Service, UserDto User, UserDto Other)> CreateAsync(string orm)
    {
        var db = new RepositoryTestDatabase();
        _databases.Add(db);
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var user = await service.Users.CreateAsync(NewUser());
        var other = await service.Users.CreateAsync(NewUser());
        Assert.IsTrue(user.IsValid && other.IsValid, user.ErrorMessage + other.ErrorMessage);
        return (service, user.Entity, other.Entity);
    }

    private static UserRegisterDto NewUser()
    {
        var id = Guid.NewGuid();
        return new UserRegisterDto
        {
            UserName = $"rst-{id:N}"[..24],
            EMail = $"{id:N}@knote.tests",
            FullName = "Reset test user",
            RoleDefinition = "Staff",
            Password = Password
        };
    }

    private static async Task<bool> CanSignInAsync(KntService service, string userName, string password)
        => (await service.Users.AuthenticateAsync(new UserCredentialsDto { UserName = userName, Password = password })).IsValid;
}
