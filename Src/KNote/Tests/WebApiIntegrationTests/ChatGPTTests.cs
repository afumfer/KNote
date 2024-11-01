using System.Net.Http.Json;
using KNote.Model;
using KNote.Tests.Helpers;

namespace KNote.Tests.WebApiIntegrationTests;

[TestClass]
public class ChatGPTTests : WebApiTestBase
{
    public ChatGPTTests() : base()
    {
        
    }

    #region Tests 

    [TestMethod]
    public async Task Post_ReturnBasicChatMessage()
    {
        List<KntChatMessage> chatMessages = new List<KntChatMessage>();
        chatMessages.Add(new KntChatMessage { Role = "user", Prompt = "Hello." });

        var httpRes = await HttpClient.PostAsJsonAsync($"{UrlBase}api/chatgpt", chatMessages);
        var res = await ProcessResultFromTestHttpResponse<KntChatMessageOutput>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsTrue(res.Entity.Prompt.Length > 0);
    }

    #endregion 

}
