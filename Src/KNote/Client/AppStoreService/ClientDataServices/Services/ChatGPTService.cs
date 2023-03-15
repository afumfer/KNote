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


    public async Task<Result<string>> PostAsync(string? prompt)
    {        
        var httpRes = await httpClient.PostAsJsonAsync("api/chatgpt", prompt);
        return await ProcessResultFromHttpResponse<string>(httpRes, "Get result from ChatGPT");                      
    }
}
