using KNote.Client.AppStoreService.ClientDataServices;

namespace KNote.Client.AppStoreService;

public class Store : IStore
{
    #region Private members

    private readonly HttpClient _httpClient;

    #endregion

    #region Constructor

    public Store(HttpClient httpClient)
    {
        _httpClient = httpClient;
        AppState = new AppState();
    }

    #endregion 

    #region IStore members

    public AppState AppState { get; }

    private IUserWebApiService _users;
    public IUserWebApiService Users
    {
        get
        {
            if (_users == null)
                _users = new UserWebApiService(AppState, _httpClient);
            return _users;
        }
    }

    private INoteTypeWebApiService _noteTypes;
    public INoteTypeWebApiService NoteTypes
    {
        get
        {
            if (_noteTypes == null)
                _noteTypes = new NoteTypeWebApiService(AppState, _httpClient);
            return _noteTypes;
        }
    }

    private IKAttributeWebApiService _kAttributes;
    public IKAttributeWebApiService KAttributes
    {
        get
        {
            if (_kAttributes == null)
                _kAttributes = new KAttributeWebApiService(AppState, _httpClient);
            return _kAttributes;
        }
    }

    private IFolderWebApiService _folders;
    public IFolderWebApiService Folders
    {
        get
        {
            if (_folders == null)
                _folders = new FolderWebApiService(AppState, _httpClient);
            return _folders;
        }
    }

    private INoteWebApiService _notes;
    public INoteWebApiService Notes
    {
        get
        {
            if (_notes == null)
                _notes = new NoteWebApiService(AppState, _httpClient);
            return _notes;
        }
    }

    #endregion 
}

