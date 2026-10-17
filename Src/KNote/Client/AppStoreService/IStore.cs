using KNote.Client.AppStoreService.ClientDataServices;

namespace KNote.Client.AppStoreService;

public interface IStore
{
    AppState AppState { get; }    
    void NavigateTo(string uri);
    string GetUri();
    Dictionary<string, string>? GetQueryStrings(string url);

    IUserWebApiService Users { get; }
    INoteTypeWebApiService NoteTypes { get; }
    ITraceNoteTypeWebApiService TraceNoteTypes { get; }
    IKAttributeWebApiService KAttributes { get; }
    IFolderWebApiService Folders { get; }
    INoteWebApiService Notes { get; }
    IAiAssistantWebApiService AiAssistant { get; }
    ISystemValueWebApiService SystemValues { get; }
    IReportWebApiService Reports { get; }

    ChatRoomHub ChatRoom { get; }
}

