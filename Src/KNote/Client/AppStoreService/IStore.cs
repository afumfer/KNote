﻿using KNote.Client.AppStoreService.ClientDataServices;
using KNote.Client.AppStoreService.ClientDataServices.Interfaces;

namespace KNote.Client.AppStoreService;

public interface IStore
{
    AppState AppState { get; }    
    void NavigateTo(string uri);
    string GetUri();
    Dictionary<string, string>? GetQueryStrings(string url);

    IUserWebApiService Users { get; }
    INoteTypeWebApiService NoteTypes { get; }
    IKAttributeWebApiService KAttributes { get; }
    IFolderWebApiService Folders { get; }
    INoteWebApiService Notes { get; }
    IChatGPTService ChatGPT { get; }
    
    Task ChatStartAsync();
    Task ChatSendMessageAsync(string messageType, string? userInput, string? messageInput);
    bool ChatIsConnected { get; }
}

