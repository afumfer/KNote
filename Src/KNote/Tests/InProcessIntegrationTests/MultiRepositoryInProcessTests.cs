using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// A Server with two repositories (the "Repositories" section; the first one is the default): a session is in one
/// repository, chosen when signing in (the token's knt_repo claim), and sees only its data, its chat room and its
/// resources; anonymous requests are served by the default repository; the password changes where the same
/// credentials sign the user in, and its fingerprint is checked in each repository.
/// </summary>
[TestClass]
public class MultiRepositoryInProcessTests
{
    private const string Password = "Multi-Repository-1!";
    private const string OtherPassword = "Multi-Repository-2!";
    private const string Secret = "KNoteTestsOnlySigningSecret_DoNotUseInProduction_1234567890";
    private const string PublicBaseUrl = "https://knote.tests/KNote/";
    private const string PngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private static KNoteWebApplicationFactory _factory = null!;
    private static readonly FakeEmailSender EmailSender = new();
    private static Guid _repositoryA;
    private static Guid _repositoryB;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _factory = new KNoteWebApplicationFactory(repositoryCount: 2);
        _factory.AppConfiguration["RateLimits:SignInPerMinute"] = "1000";
        _factory.AppConfiguration["RateLimits:PasswordResetPerHour"] = "1000";
        _factory.AppConfiguration["PasswordReset:PublicBaseUrl"] = PublicBaseUrl;
        _factory.AppConfiguration["Email:Host"] = "smtp.knote.tests";
        _factory.AppConfiguration["Email:FromAddress"] = "knote@knote.tests";
        _factory.TestServices = services => services.AddSingleton<IEmailSender>(EmailSender);

        // Builds the host, which opens (creates) both databases.
        using var client = _factory.CreateClient();
        await client.GetAsync("api/systemvalues/repositoryname");

        _repositoryA = await RepositoryIdAsync(0);
        _repositoryB = await RepositoryIdAsync(1);
    }

    [ClassCleanup]
    public static async Task ClassCleanup() => await _factory.DisposeAsync();

    #region Sign in

    [TestMethod]
    public async Task Login_ValidInBothRepositories_ListsThem_AndSignsInToTheChosenOne()
    {
        var userName = await NewUserAsync(Password, inA: true, inB: true);

        var first = await LoginAsync(userName, Password);

        Assert.IsFalse(first.success);
        Assert.IsTrue(string.IsNullOrEmpty(first.token));
        CollectionAssert.AreEquivalent(new[] { _repositoryA, _repositoryB }, first.Repositories!.Select(r => r.RepositoryId).ToArray());
        CollectionAssert.AreEquivalent(new[] { KNoteWebApplicationFactory.RepositoryNameOf(0), KNoteWebApplicationFactory.RepositoryNameOf(1) },
            first.Repositories.Select(r => r.Name).ToArray());

        var chosen = await LoginAsync(userName, Password, _repositoryB);

        Assert.IsTrue(chosen.success, chosen.error);
        Assert.AreEqual(_repositoryB, RepositoryOf(chosen.token));
    }

    [TestMethod]
    public async Task Login_ListsOnlyTheRepositoriesWhereTheCredentialsAreValid()
    {
        var userName = await NewUserAsync(Password, inA: true, inB: false);
        using (var serviceB = _factory.CreateServiceForRepository(1))
            Assert.IsTrue((await serviceB.Users.CreateAsync(NewUser(userName, OtherPassword, nameof(EnumRoles.Staff)))).IsValid);

        var res = await LoginAsync(userName, Password);

        Assert.IsTrue(res.success, res.error);
        Assert.IsNull(res.Repositories);
        Assert.AreEqual(_repositoryA, RepositoryOf(res.token));
    }

    [TestMethod]
    public async Task Login_ValidOnlyInTheOtherRepository_SignsInToIt()
    {
        var userName = await NewUserAsync(Password, inA: false, inB: true);

        var res = await LoginAsync(userName, Password);

        Assert.IsTrue(res.success, res.error);
        Assert.AreEqual(_repositoryB, RepositoryOf(res.token));
    }

    [TestMethod]
    public async Task Login_InARepositoryWhereTheCredentialsAreNotValid_IsRefused()
    {
        var userName = await NewUserAsync(Password, inA: true, inB: false);

        var inB = await LoginAsync(userName, Password, _repositoryB);
        var unknown = await LoginAsync(userName, Password, Guid.NewGuid());

        Assert.IsFalse(inB.success);
        Assert.IsTrue(string.IsNullOrEmpty(inB.token));
        Assert.IsFalse(unknown.success);
    }

    #endregion

    #region One repository per session

    [TestMethod]
    public async Task ASession_SeesOnlyTheDataOfItsRepository()
    {
        var userName = await NewUserAsync(Password, inA: true, inB: true);
        var folderName = $"__TEST_ONLY_IN_B_{Guid.NewGuid():N}__";
        using (var serviceB = _factory.CreateServiceForRepository(1))
            Assert.IsTrue((await serviceB.Folders.SaveAsync(new FolderDto { FolderId = Guid.Empty, Name = folderName })).IsValid);

        var inA = await FolderNamesAsync((await LoginAsync(userName, Password, _repositoryA)).token);
        var inB = await FolderNamesAsync((await LoginAsync(userName, Password, _repositoryB)).token);

        CollectionAssert.DoesNotContain(inA, folderName);
        CollectionAssert.Contains(inB, folderName);
    }

    [TestMethod]
    public async Task WithoutASession_TheDefaultRepository_AndWithOne_ItsRepository()
    {
        var userName = await NewUserAsync(Password, inA: true, inB: true);
        using var anonymous = _factory.CreateClient();
        using var inB = ClientFor((await LoginAsync(userName, Password, _repositoryB)).token);

        var anonymousName = await anonymous.GetFromJsonAsync<Result<string>>("api/systemvalues/repositoryname");
        var sessionName = await inB.GetFromJsonAsync<Result<string>>("api/systemvalues/repositoryname");

        Assert.AreEqual(KNoteWebApplicationFactory.RepositoryNameOf(0), anonymousName!.Entity);
        Assert.AreEqual(KNoteWebApplicationFactory.RepositoryNameOf(1), sessionName!.Entity);
    }

    [TestMethod]
    public async Task ATokenWithoutARepository_OrOfAnUnknownOne_IsRefused()
    {
        var userName = await NewUserAsync(Password, inA: true, inB: false);

        using var unknown = ClientFor(Forge(userName, Guid.NewGuid().ToString()));
        using var without = ClientFor(Forge(userName, null));

        Assert.AreEqual(HttpStatusCode.Unauthorized, (await unknown.GetAsync("api/users/me")).StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await without.GetAsync("api/users/me")).StatusCode);
    }

    [TestMethod]
    public async Task Register_SignsUpInTheDefaultRepository()
    {
        var user = NewUser($"mr-{Guid.NewGuid():N}"[..24], Password, null);
        using var client = _factory.CreateClient();

        var res = await (await client.PostAsJsonAsync("api/users/register", user)).Content.ReadFromJsonAsync<UserTokenDto>();

        Assert.IsTrue(res!.success, res.error);
        Assert.AreEqual(_repositoryA, RepositoryOf(res.token));
    }

    #endregion

    #region Chat room and resources

    [TestMethod]
    public async Task EachRepository_HasItsOwnChatRoom()
    {
        var userName = await NewUserAsync(Password, inA: true, inB: true);
        await using var a = await StartAsync((await LoginAsync(userName, Password, _repositoryA)).token);
        await using var otherA = await StartAsync((await LoginAsync(userName, Password, _repositoryA)).token);
        await using var b = await StartAsync((await LoginAsync(userName, Password, _repositoryB)).token);
        var createdSeenByA = Collect<ChatRoomThreadInfoDto>(otherA, ChatRoomHubContract.ThreadCreated);
        var createdSeenByB = Collect<ChatRoomThreadInfoDto>(b, ChatRoomHubContract.ThreadCreated);
        var activitySeenByB = Collect<ChatRoomThreadActivityDto>(b, ChatRoomHubContract.ThreadActivity);

        var thread = await a.InvokeAsync<Result<ChatRoomThreadInfoDto>>(ChatRoomHubContract.CreateThread, "Only in A");
        Assert.IsTrue(thread.IsValid, thread.ErrorMessage);
        var sent = await a.InvokeAsync<Result<ChatRoomMessageDto>>(ChatRoomHubContract.SendMessage, thread.Entity.ThreadId, "Hello A");
        Assert.IsTrue(sent.IsValid, sent.ErrorMessage);

        await WaitUntilAsync(() => createdSeenByA.Any(t => t.ThreadId == thread.Entity.ThreadId));
        await Task.Delay(300);
        Assert.IsFalse(createdSeenByB.Any(t => t.ThreadId == thread.Entity.ThreadId), "B's clients don't hear of A's threads.");
        Assert.IsFalse(activitySeenByB.Any(t => t.ThreadId == thread.Entity.ThreadId));
        var threadsOfB = await b.InvokeAsync<Result<List<ChatRoomThreadInfoDto>>>(ChatRoomHubContract.GetThreads);
        Assert.IsFalse(threadsOfB.Entity.Any(t => t.ThreadId == thread.Entity.ThreadId));
        var subscribeFromB = await b.InvokeAsync<Result<ChatRoomThreadDto>>(ChatRoomHubContract.Subscribe, thread.Entity.ThreadId);
        Assert.IsFalse(subscribeFromB.IsValid, "A's thread isn't in B.");
    }

    [TestMethod]
    public async Task TheResources_AreKeptAndServedPerRepository()
    {
        var userName = await NewUserAsync(Password, inA: true, inB: true);
        var tokenB = (await LoginAsync(userName, Password, _repositoryB)).token;
        await using var b = await StartAsync(tokenB);
        var thread = await b.InvokeAsync<Result<ChatRoomThreadInfoDto>>(ChatRoomHubContract.CreateThread, "Images in B");
        Assert.IsTrue(thread.IsValid, thread.ErrorMessage);
        using var http = ClientFor(tokenB);

        var httpRes = await http.PostAsJsonAsync(ChatRoomHubContract.ImagesPath(thread.Entity.ThreadId),
            new ChatRoomImageDto { FileName = "screen.png", ContentType = "image/png", ContentBase64 = PngBase64 });
        var image = await httpRes.Content.ReadFromJsonAsync<Result<ChatRoomImageDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode, image?.ErrorMessage);
        var rootUrlB = KNoteWebApplicationFactory.ResourcesRootUrlOf(1);
        StringAssert.StartsWith(image!.Entity.Url, $"{rootUrlB}/{KNoteWebApplicationFactory.ResourcesContainer}/");
        var relativePath = image.Entity.Url[(rootUrlB.Length + 1)..];
        Assert.IsTrue(File.Exists(Path.Combine(_factory.ResourcesFolderOf(1), relativePath)), "In B's resources folder.");
        Assert.IsFalse(File.Exists(Path.Combine(_factory.ResourcesFolderOf(0), relativePath)), "Not in A's.");
    }

    #endregion

    #region Passwords

    [TestMethod]
    public async Task ChangePassword_ChangesItWhereTheSameCredentialsSignTheUserIn()
    {
        var userName = await NewUserAsync(Password, inA: true, inB: true);
        var tokenA = (await LoginAsync(userName, Password, _repositoryA)).token;
        var tokenB = (await LoginAsync(userName, Password, _repositoryB)).token;
        using var inA = ClientFor(tokenA);

        var httpRes = await inA.PostAsJsonAsync("api/users/me/password", new UserChangePasswordDto { CurrentPassword = Password, NewPassword = OtherPassword });
        var res = await httpRes.Content.ReadFromJsonAsync<Result<UserTokenDto>>();

        Assert.IsTrue(res!.IsValid, res.ErrorMessage);
        Assert.AreEqual(_repositoryA, RepositoryOf(res.Entity.token));
        StringAssert.Contains(res.Entity.Notice, KNoteWebApplicationFactory.RepositoryNameOf(1));
        using var oldB = ClientFor(tokenB);
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await oldB.GetAsync("api/users/me")).StatusCode, "B's session closed.");
        Assert.IsTrue((await LoginAsync(userName, OtherPassword, _repositoryB)).success, "Changed in B too.");
    }

    [TestMethod]
    public async Task ChangePassword_DoesNotTouchARepositoryWhereThePasswordIsAnotherOne()
    {
        var userName = await NewUserAsync(Password, inA: true, inB: false);
        using (var serviceB = _factory.CreateServiceForRepository(1))
            Assert.IsTrue((await serviceB.Users.CreateAsync(NewUser(userName, OtherPassword, nameof(EnumRoles.Staff)))).IsValid);
        using var inA = ClientFor((await LoginAsync(userName, Password, _repositoryA)).token);

        var res = await (await inA.PostAsJsonAsync("api/users/me/password",
            new UserChangePasswordDto { CurrentPassword = Password, NewPassword = "Multi-Repository-3!" })).Content.ReadFromJsonAsync<Result<UserTokenDto>>();

        Assert.IsTrue(res!.IsValid, res.ErrorMessage);
        Assert.IsNull(res.Entity.Notice, "Nothing about the other repository.");
        Assert.IsTrue((await LoginAsync(userName, OtherPassword, _repositoryB)).success, "B keeps its password.");
    }

    [TestMethod]
    public async Task ThePasswordFingerprint_IsCheckedInTheSessionsRepository()
    {
        var userName = await NewUserAsync(Password, inA: true, inB: true);
        var adminB = await NewUserAsync(Password, inA: false, inB: true, role: nameof(EnumRoles.Admin));
        var tokenA = (await LoginAsync(userName, Password, _repositoryA)).token;
        var tokenB = (await LoginAsync(userName, Password, _repositoryB)).token;
        using var asAdminB = ClientFor((await LoginAsync(adminB, Password, _repositoryB)).token);
        Guid userIdInB;
        using (var serviceB = _factory.CreateServiceForRepository(1))
            userIdInB = (await serviceB.Users.GetByUserNameAsync(userName)).Entity.UserId;

        var setRes = await asAdminB.PutAsJsonAsync($"api/users/{userIdInB}/password", new UserSetPasswordDto { NewPassword = OtherPassword });

        Assert.IsTrue(setRes.IsSuccessStatusCode);
        using var inB = ClientFor(tokenB);
        using var inA = ClientFor(tokenA);
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await inB.GetAsync("api/users/me")).StatusCode, "B's session closed.");
        Assert.AreEqual(HttpStatusCode.OK, (await inA.GetAsync("api/users/me")).StatusCode, "A's session goes on.");
    }

    [TestMethod]
    public async Task ForgotPassword_OneLink_SetsTheNewPasswordInEveryRepositoryOfTheUser()
    {
        var email = $"{Guid.NewGuid():N}@knote.tests";
        var userName = await NewUserAsync(Password, inA: true, inB: true, email: email);
        using var anonymous = _factory.CreateClient();
        var tokenB = (await LoginAsync(userName, Password, _repositoryB)).token;

        var forgot = await anonymous.PostAsJsonAsync("api/users/forgot-password", new UserForgotPasswordDto { UserNameOrEMail = userName });
        var message = await EmailSender.WaitForAsync();

        Assert.IsTrue(forgot.IsSuccessStatusCode);
        Assert.IsNotNull(message);
        Assert.AreEqual(email, message.To);
        Assert.IsNull(await EmailSender.WaitForAsync(TimeSpan.FromMilliseconds(300)), "One message.");
        var links = LinksIn(message.Body);
        Assert.HasCount(1, links, "One link.");
        StringAssert.Contains(message.Body, KNoteWebApplicationFactory.RepositoryNameOf(0));
        StringAssert.Contains(message.Body, KNoteWebApplicationFactory.RepositoryNameOf(1));

        var httpRes = await anonymous.PostAsJsonAsync("api/users/reset-password", new UserResetPasswordDto { Token = links[0], NewPassword = OtherPassword });
        var reset = await httpRes.Content.ReadFromJsonAsync<Result<UserTokenDto>>();

        Assert.IsTrue(reset!.IsValid, reset.ErrorMessage);
        Assert.IsTrue(string.IsNullOrEmpty(reset.Entity.token), "Set in two repositories: the user chooses one to sign in.");
        Assert.AreEqual(userName, reset.Entity.UserName);
        CollectionAssert.AreEquivalent(new[] { _repositoryA, _repositoryB }, reset.Entity.Repositories!.Select(r => r.RepositoryId).ToArray());
        Assert.IsTrue((await LoginAsync(userName, OtherPassword, _repositoryA)).success);
        Assert.IsTrue((await LoginAsync(userName, OtherPassword, _repositoryB)).success);
        using var oldB = ClientFor(tokenB);
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await oldB.GetAsync("api/users/me")).StatusCode, "The sessions opened before are closed.");

        var again = await anonymous.PostAsJsonAsync("api/users/reset-password", new UserResetPasswordDto { Token = links[0], NewPassword = "Multi-Repository-3!" });
        Assert.AreEqual(HttpStatusCode.BadRequest, again.StatusCode, "The link works once.");
    }

    [TestMethod]
    public async Task ForgotPassword_TheSameUserNameWithAnotherEmail_IsLeftOutOfTheLink()
    {
        var emailA = $"{Guid.NewGuid():N}@knote.tests";
        var userName = await NewUserAsync(Password, inA: true, inB: false, email: emailA);
        var otherEmail = $"{Guid.NewGuid():N}@knote.tests";
        using (var serviceB = _factory.CreateServiceForRepository(1))
        {
            var other = NewUser(userName, Password, nameof(EnumRoles.Staff));
            other.EMail = otherEmail;
            Assert.IsTrue((await serviceB.Users.CreateAsync(other)).IsValid);
        }
        using var anonymous = _factory.CreateClient();

        await anonymous.PostAsJsonAsync("api/users/forgot-password", new UserForgotPasswordDto { UserNameOrEMail = userName });
        Assert.IsNotNull(await EmailSender.WaitForAsync());
        Assert.IsNotNull(await EmailSender.WaitForAsync());

        // One message for each address (the account of B has another email: another person's link).
        var toA = EmailSender.Sent.Single(m => m.To == emailA);
        var toB = EmailSender.Sent.Single(m => m.To == otherEmail);
        var reset = await (await anonymous.PostAsJsonAsync("api/users/reset-password",
            new UserResetPasswordDto { Token = LinksIn(toA.Body).Single(), NewPassword = OtherPassword })).Content.ReadFromJsonAsync<Result<UserTokenDto>>();

        Assert.IsTrue(reset!.IsValid, reset.ErrorMessage);
        Assert.AreEqual(_repositoryA, RepositoryOf(reset.Entity.token), "Set only in A: signed in to it.");
        Assert.IsTrue((await LoginAsync(userName, Password, _repositoryB)).success, "B's user (another email) keeps its password.");
        StringAssert.Contains(toB.Body, $"<b>{KNoteWebApplicationFactory.RepositoryNameOf(1)}</b>");
        Assert.IsFalse(toB.Body.Contains($"<b>{KNoteWebApplicationFactory.RepositoryNameOf(0)}</b>"), "Each address only hears of its own repositories.");
    }

    #endregion

    #region Helpers

    private static async Task<Guid> RepositoryIdAsync(int repository)
    {
        using var service = _factory.CreateServiceForRepository(repository);
        return (await service.SystemValues.GetRepositoryInfoAsync()).Entity.RepositoryId;
    }

    // A user with the same name and password in the repositories asked for.
    private static async Task<string> NewUserAsync(string password, bool inA, bool inB, string role = nameof(EnumRoles.Staff), string? email = null)
    {
        var userName = $"mr-{Guid.NewGuid():N}"[..24];
        foreach (var (repository, wanted) in new[] { (0, inA), (1, inB) })
        {
            if (!wanted)
                continue;
            using var service = _factory.CreateServiceForRepository(repository);
            var user = NewUser(userName, password, role);
            if (email != null)
                user.EMail = email;
            var res = await service.Users.CreateAsync(user);
            Assert.IsTrue(res.IsValid, res.ErrorMessage);
        }
        return userName;
    }

    private static UserRegisterDto NewUser(string userName, string password, string? role) => new()
    {
        UserName = userName,
        EMail = $"{Guid.NewGuid():N}@knote.tests",
        FullName = userName,
        RoleDefinition = role,
        Password = password
    };

    private static async Task<UserTokenDto> LoginAsync(string userName, string password, Guid? repositoryId = null)
    {
        using var client = _factory.CreateClient();
        var httpRes = await client.PostAsJsonAsync("api/users/login", new UserCredentialsDto { UserName = userName, Password = password, RepositoryId = repositoryId });
        return (await httpRes.Content.ReadFromJsonAsync<UserTokenDto>())!;
    }

    // The tokens of the links of a message.
    private static List<string> LinksIn(string body) =>
        Regex.Matches(body, "reset-password\\?token=([^\"&<\\s]+)").Select(m => Uri.UnescapeDataString(m.Groups[1].Value)).Distinct().ToList();

    private static Guid RepositoryOf(string token) =>
        Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.Single(c => c.Type == KntConst.RepositoryClaimType).Value);

    // A token signed as the Server does, with a repository claim that isn't one of its repositories (or none).
    private static string Forge(string userName, string? repository)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, userName), new("knt_pwd", ""), new(ClaimTypes.Role, nameof(EnumRoles.Staff)) };
        if (repository != null)
            claims.Add(new Claim(KntConst.RepositoryClaimType, repository));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret)), SecurityAlgorithms.HmacSha256);
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: credentials));
    }

    private static HttpClient ClientFor(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        return client;
    }

    private static async Task<List<string>> FolderNamesAsync(string token)
    {
        using var client = ClientFor(token);
        var res = await client.GetFromJsonAsync<Result<List<FolderInfoDto>>>("api/folders");
        Assert.IsTrue(res!.IsValid, res.ErrorMessage);
        return res.Entity.Select(f => f.Name).ToList();
    }

    private static async Task<HubConnection> StartAsync(string token)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "chathub"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                // The test server has no WebSockets for the SignalR client.
                options.Transports = HttpTransportType.LongPolling;
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .Build();
        await connection.StartAsync();
        return connection;
    }

    private static ConcurrentQueue<T> Collect<T>(HubConnection connection, string method)
    {
        var received = new ConcurrentQueue<T>();
        connection.On<T>(method, received.Enqueue);
        return received;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + Timeout;
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
                Assert.Fail("Timed out waiting for the hub.");
            await Task.Delay(20);
        }
    }

    #endregion
}
