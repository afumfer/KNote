using System.Net.Http.Json;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// ChatRoomServer: the sign-in url of the chat room's Server, next to its hub, what is refused before contacting it,
/// and the sign in in one of its repositories (against a fake server: no server is contacted).
/// </summary>
[TestClass]
public class ChatRoomServerTests
{
    [TestMethod]
    [DataRow("https://server/KNote/chathub", "https://server/KNote/api/users/login")]
    [DataRow("https://server/KNote/chathub/", "https://server/KNote/api/users/login")]
    [DataRow("http://localhost:44338/KNote/ChatHub", "http://localhost:44338/KNote/api/users/login")]
    [DataRow("https://chat.example.com/chathub", "https://chat.example.com/api/users/login")]
    [DataRow("  https://server/KNote/chathub?x=1  ", "https://server/KNote/api/users/login")]
    public void LoginUrl_IsTheServersSignIn_NextToItsHub(string hubUrl, string expected)
    {
        Assert.AreEqual(expected, ChatRoomServer.LoginUrl(hubUrl)?.ToString());
    }

    [TestMethod]
    [DataRow("https://server/KNote/chathub", "https://server/KNote/")]
    [DataRow("https://chat.example.com/chathub", "https://chat.example.com/")]
    [DataRow("http://localhost:5199/KNote/chathub/", "http://localhost:5199/KNote/")]
    public void BaseUrl_IsTheServersApplication_WhereItsHubIs(string hubUrl, string expected)
    {
        Assert.AreEqual(expected, ChatRoomServer.BaseUrl(hubUrl)?.ToString());
    }

    [TestMethod]
    public async Task UploadImage_WithAWrongUrl_IsRefusedWithoutContactingTheServer()
    {
        var res = await ChatRoomServer.UploadImageAsync("https://server/KNote/hub", "token", new KNote.Model.Dto.ChatRoomImageDto());

        Assert.IsFalse(res.IsValid);
        StringAssert.Contains(res.ErrorMessage, "/chathub");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("server/KNote/chathub")]
    [DataRow("https://server/KNote/hub")]
    [DataRow("ftp://server/KNote/chathub")]
    public void LoginUrl_OfAnUrlThatIsNotAHub_IsNull(string? hubUrl)
    {
        Assert.IsNull(ChatRoomServer.LoginUrl(hubUrl!));
    }

    [TestMethod]
    public async Task SignIn_WithAWrongUrl_OrWithoutCredentials_IsRefusedWithoutContactingTheServer()
    {
        var wrongUrl = await ChatRoomServer.SignInAsync("https://server/KNote/hub", "jdoe", "pwd");
        var noPassword = await ChatRoomServer.SignInAsync("https://server/KNote/chathub", "jdoe", "");

        Assert.IsFalse(wrongUrl.IsValid);
        StringAssert.Contains(wrongUrl.ErrorMessage, "/chathub");
        Assert.IsFalse(noPassword.IsValid);
        StringAssert.Contains(noPassword.ErrorMessage, "password");
    }

    [TestMethod]
    public async Task SignIn_ValidInSeveralRepositories_GetsATokenForEachOne()
    {
        var work = new RepositoryInfoDto { RepositoryId = Guid.NewGuid(), Name = "Work" };
        var personal = new RepositoryInfoDto { RepositoryId = Guid.NewGuid(), Name = "Personal" };
        var server = new FakeServer(credentials => credentials.RepositoryId is Guid id
            ? new UserTokenDto { success = true, token = $"token-{id}" }
            : new UserTokenDto { success = false, token = "", Repositories = new() { work, personal }, error = "Choose one." });

        var res = await ChatRoomServer.SignInAsync(server, "https://server/KNote/chathub", "jdoe", "pwd");

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        CollectionAssert.AreEqual(new[] { "Work", "Personal" }, res.Entity.Select(r => r.Name).ToArray());
        CollectionAssert.AreEqual(new[] { work.RepositoryId, personal.RepositoryId }, res.Entity.Select(r => r.RepositoryId).ToArray());
        CollectionAssert.AreEqual(new[] { $"token-{work.RepositoryId}", $"token-{personal.RepositoryId}" }, res.Entity.Select(r => r.Token).ToArray());
        Assert.IsNull(server.Credentials[0].RepositoryId, "First, without a repository.");
        Assert.HasCount(3, server.Credentials, "Then once in each one, with the same password.");
    }

    [TestMethod]
    public async Task SignIn_ValidInOneRepository_GivesItsTokenNameAndId()
    {
        var repositoryId = Guid.NewGuid();
        var token = FakeJwt($"{{\"knt_repo\":\"{repositoryId}\",\"unique_name\":\"jdoe@knote.tests\"}}");
        var server = new FakeServer(_ => new UserTokenDto { success = true, token = token }, repositoryName: "Work");

        var res = await ChatRoomServer.SignInAsync(server, "https://server/KNote/chathub", "jdoe", "pwd");

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var repository = res.Entity.Single();
        Assert.AreEqual(token, repository.Token);
        Assert.AreEqual("Work", repository.Name);
        Assert.AreEqual(repositoryId, repository.RepositoryId, "Read from the token (its knt_repo claim).");
        Assert.AreEqual(token, server.RepositoryNameToken, "The name is read with the token: its repository's.");
    }

    [TestMethod]
    public void RepositoryIdOf_ATokenWithoutTheClaim_OrNotAToken_IsEmpty()
    {
        Assert.AreEqual(Guid.Empty, ChatRoomServer.RepositoryIdOf(FakeJwt("{\"unique_name\":\"jdoe\"}")));
        Assert.AreEqual(Guid.Empty, ChatRoomServer.RepositoryIdOf("not-a-token"));
    }

    // A JWT with that payload (not signed: the client only reads it).
    private static string FakeJwt(string payload)
    {
        static string Base64Url(string text) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"{Base64Url("{\"alg\":\"HS256\",\"typ\":\"JWT\"}")}.{Base64Url(payload)}.signature";
    }

    [TestMethod]
    public async Task SignIn_Refused_GivesTheServersReason()
    {
        var server = new FakeServer(_ => new UserTokenDto { success = false, token = "", error = "User not authenticated" });

        var res = await ChatRoomServer.SignInAsync(server, "https://server/KNote/chathub", "jdoe", "wrong");

        Assert.IsFalse(res.IsValid);
        StringAssert.Contains(res.ErrorMessage, "User not authenticated");
    }

    // The Server's sign in (api/users/login) and the name of the token's repository (api/systemvalues/repositoryname).
    private sealed class FakeServer : HttpMessageHandler
    {
        private readonly Func<UserCredentialsDto, UserTokenDto> _login;
        private readonly string? _repositoryName;

        public FakeServer(Func<UserCredentialsDto, UserTokenDto> login, string? repositoryName = null)
        {
            _login = login;
            _repositoryName = repositoryName;
        }

        public List<UserCredentialsDto> Credentials { get; } = new();

        public string? RepositoryNameToken { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("api/users/login"))
            {
                var credentials = (await request.Content!.ReadFromJsonAsync<UserCredentialsDto>(cancellationToken))!;
                Credentials.Add(credentials);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = JsonContent.Create(_login(credentials)) };
            }

            RepositoryNameToken = request.Headers.Authorization?.Parameter;
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = JsonContent.Create(new Result<string> { Entity = _repositoryName }) };
        }
    }

    [TestMethod]
    public void ARefusedConnection_IsToldApart()
    {
        var refused = new HttpRequestException("Unauthorized", null, System.Net.HttpStatusCode.Unauthorized);
        var forbidden = new HttpRequestException("Forbidden", null, System.Net.HttpStatusCode.Forbidden);
        var unreachable = new HttpRequestException("No such host is known.");

        Assert.IsTrue(ChatRoomServer.IsNotAuthorized(refused));
        Assert.IsTrue(ChatRoomServer.IsNotAuthorized(forbidden));
        Assert.IsFalse(ChatRoomServer.IsNotAuthorized(unreachable));
        StringAssert.Contains(ChatRoomServer.ConnectionErrorMessage(refused), "Sign in again");
        Assert.AreEqual("No such host is known.", ChatRoomServer.ConnectionErrorMessage(unreachable));
    }
}
