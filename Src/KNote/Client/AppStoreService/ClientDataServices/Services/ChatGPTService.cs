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

    public async Task<Result<ChatMessageOutput>> PostAsync(List<ChatMessage> chatMessages, string prompt = "")
    {
        chatMessages.Add(new ChatMessage { Role = "user", Prompt = prompt }); 

        var httpRes = await httpClient.PostAsJsonAsync("api/chatgpt", chatMessages);
        var res = await ProcessResultFromHttpResponse<ChatMessageOutput>(httpRes, "Get result from ChatGPT");
        if (res.IsValid)
            chatMessages[chatMessages.Count - 1].Tokens = res.Entity.PromptTokens;
        return res;
    }
}
