using KNote.Client.AppStoreService.ClientDataServices;
using Microsoft.AspNetCore.Components;

namespace KNote.Client.AppStoreService;

public interface IStore
{
    AppState AppState { get; }
    IUserWebApiService Users { get; }
    INoteTypeWebApiService NoteTypes { get; }
    IKAttributeWebApiService KAttributes { get; }
    IFolderWebApiService Folders { get; }
    INoteWebApiService Notes { get; }

    void NavigateTo(string uri);
    string GetUri();
    Dictionary<string, string> GetQueryStrings(string url);
}

