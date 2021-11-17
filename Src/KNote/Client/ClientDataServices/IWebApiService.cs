namespace KNote.Client.ClientDataServices;

public interface IWebApiService
{
    IUserWebApiService Users { get; }
    INoteTypeWebApiService NoteTypes { get; }
    IKAttributeWebApiService KAttributes { get; }
    IFolderWebApiService Folders { get; }
    INoteWebApiService Notes { get; }
}

