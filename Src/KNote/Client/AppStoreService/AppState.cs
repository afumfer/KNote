using KNote.Model.Dto;

namespace KNote.Client.AppStoreService;

public class AppState
{
    #region Generic readonly configuration properties

    public readonly Version? AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName()?.Version;

    #endregion

    #region AppSatate properties

    private string? _tag;
    public string? Tag
    {
        get { return _tag; }
        set { _tag = value; NotifyStateChanged(); }
    }

    private string? _userName;
    public string? UserName
    {
        get { return _userName; }
        set { _userName = value; NotifyStateChanged(); }
    }

    private bool _defaultContentResourcesInDB = false;
    public bool DefaultContentResourcesInDB
    {
        get { return _defaultContentResourcesInDB; }
        set { _defaultContentResourcesInDB = value; NotifyStateChanged(); }
    }

    private FolderDto? _selectedFolder;
    public FolderDto? SelectedFolder
    {
        get
        {
            if (_selectedFolder == null)
                _selectedFolder = new FolderDto();
            return _selectedFolder;
        }
        set
        {
            _selectedFolder = value;
            NotifyStateChanged();
        }
    }

    public bool IsSelectedFolder()
    {
        if (_selectedFolder == null)
            return false;
        else if (SelectedFolder?.FolderId == Guid.Empty)
            return false;
        else if (string.IsNullOrEmpty(SelectedFolder?.Name))
            return false;
        else
            return true;
    }

    private List<FolderDto>? _foldersTree;
    public List<FolderDto>? FoldersTree
    {
        get
        {
            return _foldersTree;
        }
        set
        {
            _foldersTree = value;
            if (_foldersIndex != null)
                _foldersIndex = null;
            NotifyStateChanged();
        }
    }

    private Dictionary<Guid, FolderDto>? _foldersIndex;
    public Dictionary<Guid, FolderDto> FoldersIndex
    {
        get
        {
            if (_foldersIndex == null)
                _foldersIndex = new Dictionary<Guid, FolderDto>();
            return _foldersIndex;
        }
    }

    public List<string> ChatMessages { get; } = new List<string>();

    public void AddChatMessage(string message)
    {
        ChatMessages.Add(message);
        NotifyStateChanged();
        if(!message.StartsWith(UserName!))
            NotifySuccess("Chat message input", message);
    }

    #endregion 

    #region App event

    public event Action? OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();

    public event Action<string, string>? OnNotifyError;
    public void NotifyError(string summary, string details) 
    {
        OnNotifyError?.Invoke(summary, details); 
    }

    public event Action<string, string>? OnNotifySuccess;
    public void NotifySuccess(string summary, string details)
    {
        OnNotifySuccess?.Invoke(summary, details);
    }

    #endregion
}

