using KNote.ClientWin.Core;

namespace KNote.ClientWin.Tests;

/// <summary>
/// ChatRoomServer: the sign-in url of the chat room's Server, next to its hub, and what is refused before contacting
/// it. No server is contacted.
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
