using KNote.Client.AppStoreService.ClientDataServices;
using KNote.Client.AppStoreService.ClientDataServices.Interfaces;
using KNote.Client.AppStoreService.ClientDataServices.Services;
using KNote.Client.Helpers;
using Microsoft.AspNetCore.Components;

namespace KNote.Client.AppStoreService;

public class Store : IStore
{
    #region Private members

    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;

    #endregion

    #region Constructor

    public Store(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
        AppState = new AppState();
    }

    #endregion 

    #region IStore members

    public AppState AppState { get; }

    public void NavigateTo(string uri)
    {
        _navigationManager.NavigateTo(uri);
    }

    public string GetUri()
    {
        return _navigationManager.Uri;
        
    }

    public Dictionary<string, string> GetQueryStrings(string url)
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

    private IChatGPTService? _chatGPT;
    public IChatGPTService ChatGPT
    {
        get
        {
            if (_chatGPT == null)
                _chatGPT = new ChatGPTService(AppState, _httpClient);
            return _chatGPT;
        }
    }


    #endregion

}

