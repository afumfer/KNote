using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Client.AppStoreService.ClientDataServices.Interfaces;
using KNote.Model;
using KNote.Model.Dto;
using System.Net.Http.Json;


namespace KNote.Client.AppStoreService.ClientDataServices.Services;

public class ChatGPTService : BaseService, IChatGPTService
{
    public ChatGPTService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public async Task<Result<KntChatMessageOutput>> PostAsync(List<KntChatMessage> chatMessages, string prompt = "")
    {
        chatMessages.Add(new KntChatMessage { Role = "user", Prompt = prompt }); 

        var httpRes = await _httpClient.PostAsJsonAsync("api/chatgpt", chatMessages);
        var res = await ProcessResultFromHttpResponse<KntChatMessageOutput>(httpRes, "Get result from ChatGPT");
        if (res.IsValid)
            chatMessages[chatMessages.Count - 1].Tokens = res.Entity.PromptTokens;
        return res;
    }
}
