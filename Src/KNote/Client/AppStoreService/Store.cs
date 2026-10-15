using KNote.Client.AppStoreService.ClientDataServices;
using KNote.Client.Helpers;
using KNote.Model.Dto;
using Microsoft.AspNetCore.Components;

namespace KNote.Client.AppStoreService;

public class Store : IStore, IDisposable
{
    #region Private members

    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private AppState _appState;

    #endregion

    #region Constructor

    public Store(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
        
        _appState = new AppState();

        ChatRoom = new ChatRoomHub(_appState, httpClient, navigationManager.ToAbsoluteUri(ChatRoomHubContract.Path.TrimStart('/')));
    }

    #endregion 

    #region IStore members

    public AppState AppState 
    { 
        get { return _appState; } 
    }

    public void NavigateTo(string uri)
    {
        _navigationManager.NavigateTo(uri);
    }

    public string GetUri()
    {
        return _navigationManager.Uri;        
    }

    public Dictionary<string, string>? GetQueryStrings(string url)
    {                
        return  _navigationManager.GetQueryStrings(url);
    }

    private IUserWebApiService? _users;
    public IUserWebApiService Users
    {
        get
        {
            if (_users == null)
                _users = new UserWebApiService(AppState, _httpClient);
            return _users;
        }
    }

    private INoteTypeWebApiService? _noteTypes;
    public INoteTypeWebApiService NoteTypes
    {
        get
        {
            if (_noteTypes == null)
                _noteTypes = new NoteTypeWebApiService(AppState, _httpClient);
            return _noteTypes;
        }
    }

    private IKAttributeWebApiService? _kAttributes;
    public IKAttributeWebApiService KAttributes
    {
        get
        {
            if (_kAttributes == null)
                _kAttributes = new KAttributeWebApiService(AppState, _httpClient);
            return _kAttributes;
        }
    }

    private IFolderWebApiService? _folders;
    public IFolderWebApiService Folders
    {
        get
        {
            if (_folders == null)
                _folders = new FolderWebApiService(AppState, _httpClient);
            return _folders;
        }
    }

    private INoteWebApiService? _notes;
    public INoteWebApiService Notes
    {
        get
        {
            if (_notes == null)
                _notes = new NoteWebApiService(AppState, _httpClient);
            return _notes;
        }
    }

    private IAiAssistantWebApiService? _aiAssistant;
    public IAiAssistantWebApiService AiAssistant
    {
        get
        {
            if (_aiAssistant == null)
                _aiAssistant = new AiAssistantWebApiService(AppState, _httpClient);
            return _aiAssistant;
        }
    }

    private ISystemValueWebApiService? _systemValues;
    public ISystemValueWebApiService SystemValues
    {
        get
        {
            if (_systemValues == null)
                _systemValues = new SystemValueWebApiService(AppState, _httpClient);
            return _systemValues;
        }
    }

    public ChatRoomHub ChatRoom { get; }

    #endregion

    #region IDisposable 

    public void Dispose()
    {
        _ = ChatRoom.DisposeAsync();
    }

    #endregion
}

