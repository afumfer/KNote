using KNote.Client.AppStoreService.ClientDataServices;

namespace KNote.Client.AppStoreService;

public interface IStore
{
    AppState AppState { get; }
    IUserWebApiService Users { get; }
    INoteTypeWebApiService NoteTypes { get; }
    IKAttributeWebApiService KAttributes { get; }
    IFolderWebApiService Folders { get; }
    INoteWebApiService Notes { get; }
}

