using System.Xml.Serialization;
using System.Reflection;
using Microsoft.Extensions.Logging;
using KNote.Model;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Views;
using KNote.Model.Dto;
using KNote.Service.Core;
using KntScript;

namespace KNote.ClientWin.Core;

public class Store
{
    #region Private fields

    private readonly List<ServiceRef> _servicesRefs;

    private readonly List<CtrlBase> _listComponents;

    private readonly char[] newLine = { '\r', '\n' };

    #endregion 

    #region Public properties, application state 

    public AppConfig AppConfig { get; protected set; }

    public string AppUserName { get; set; }

    public string ComputerName { get; set; }

    public Version AppVersion { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }

    public readonly IFactoryViews FactoryViews;

    public FolderWithServiceRef _dafaultFolderWithServiceRef;
    public FolderWithServiceRef DefaultFolderWithServiceRef
    {
        set { _dafaultFolderWithServiceRef = value; }
        get { return _dafaultFolderWithServiceRef; }
    }

    public FolderWithServiceRef _activeFolderWithServiceRef;
    public FolderWithServiceRef ActiveFolderWithServiceRef
    {            
        get { return _activeFolderWithServiceRef; }            
    }

    public NotesFilterWithServiceRef _activeFilterWithServiceRef;
    public NotesFilterWithServiceRef ActiveFilterWithServiceRef
    {            
        get { return _activeFilterWithServiceRef; }
    }

    public ILogger Logger { get; set; }

    #endregion

    #region Constructor 

    public Store(IFactoryViews factoryViews)
    {
        if (AppConfig == null)
            AppConfig = new AppConfig();

        _listComponents = new List<CtrlBase>();
        _servicesRefs = new List<ServiceRef>();
        FactoryViews = factoryViews; //
    }

    public Store(AppConfig config, IFactoryViews factoryViews) : this (factoryViews)
    {
        AppConfig = config;
    }

    #endregion

    #region Actions    

    public event EventHandler<ComponentEventArgs<FolderWithServiceRef>> ChangedActiveFolderWithServiceRef;
    public void ChangeActiveFolderWithServiceRef(FolderWithServiceRef activeFolderWithServiceRef)
    {
        if(_activeFolderWithServiceRef != activeFolderWithServiceRef)
        {
            _activeFolderWithServiceRef = activeFolderWithServiceRef;
            Logger?.LogTrace("ChangeActiveFolderWithServiceRef {message}", activeFolderWithServiceRef?.ToString());
            ChangedActiveFolderWithServiceRef?.Invoke(this, new ComponentEventArgs<FolderWithServiceRef>(activeFolderWithServiceRef));
        }
    }

    public event EventHandler<ComponentEventArgs<NotesFilterWithServiceRef>> ChangedActiveFilterWithServiceRef;
    public void ChangeActiveFilterWithServiceRef(NotesFilterWithServiceRef activeFilterWithServiceRef)
    {
        if (_activeFilterWithServiceRef != activeFilterWithServiceRef)
        {
            _activeFilterWithServiceRef = activeFilterWithServiceRef;
            Logger?.LogTrace("ChangeActiveFilterWithServiceRef {message}", activeFilterWithServiceRef?.ToString());
            ChangedActiveFilterWithServiceRef?.Invoke(this, new ComponentEventArgs<NotesFilterWithServiceRef>(activeFilterWithServiceRef));
        }
    }
  
    public event EventHandler<ComponentEventArgs<ServiceRef>> AddedServiceRef;
    public void AddServiceRef(ServiceRef serviceRef)
    {
        if(serviceRef is null)
            throw new ArgumentNullException(nameof(serviceRef));

        _servicesRefs.Add(serviceRef);
        Logger?.LogInformation("Added ServiceRef {component}", serviceRef.ToString());
        AddedServiceRef?.Invoke(this, new ComponentEventArgs<ServiceRef>(serviceRef));
    }

    public void AddServiceRefInAppConfig(ServiceRef serviceRef)
    {
        if (serviceRef is null)
            throw new ArgumentNullException(nameof(serviceRef));

        AppConfig.RespositoryRefs.Add(serviceRef.RepositoryRef);
    }

    public event EventHandler<ComponentEventArgs<ServiceRef>> RemovedServiceRef;
    public void RemoveServiceRef(ServiceRef serviceRef)
    {
        if (serviceRef is null)
            throw new ArgumentNullException(nameof(serviceRef));

        _servicesRefs.Remove(serviceRef);
        Logger?.LogInformation("Removed ServiceRef {component}", serviceRef.ToString());
        AppConfig.RespositoryRefs.Remove(serviceRef.RepositoryRef);            
        RemovedServiceRef?.Invoke(this, new ComponentEventArgs<ServiceRef>(serviceRef));
    }

    public List<ServiceRef> GetAllServiceRef()
    {
        return _servicesRefs.ToList();
    }

    public ServiceRef GetServiceRef(Guid id)
    {
        return _servicesRefs.Where(_ => _.IdServiceRef == id).FirstOrDefault();
    }

    public ServiceRef GetServiceRef(string alias)
    {
        return _servicesRefs.Where(_ => _.Alias == alias).FirstOrDefault();
    }

    public ServiceRef GetFirstServiceRef()
    {
        return _servicesRefs.FirstOrDefault();
    }

    public IKntService GetActiveOrDefaultServide()
    {
        if (ActiveFolderWithServiceRef != null)
            return ActiveFolderWithServiceRef.ServiceRef.Service;
        else
            return GetFirstServiceRef().Service;
    }


    public event EventHandler<ComponentEventArgs<CtrlBase>> AddedComponent;
    public event EventHandler<ComponentEventArgs<EComponentState>> ComponentsStateChanged;
    public void AddComponent(CtrlBase component)
    {
        component.StateComponentChanged += Components_StateCtrlChanged;
        
        if (component is PostItEditorCtrl)
        {
            ((PostItEditorCtrl)component).AddedEntity += Store_AddedPostIt;
            ((PostItEditorCtrl)component).SavedEntity += Store_SavedPostIt;
            ((PostItEditorCtrl)component).DeletedEntity += Store_DeletedPostIt;
            ((PostItEditorCtrl)component).ExtendedEdit += Store_ExtendedEditPostIt;
        }
        else if (component is NoteEditorCtrl)
        {
            ((NoteEditorCtrl)component).AddedEntity += Store_AddedNote;
            ((NoteEditorCtrl)component).SavedEntity += Store_SavedNote;
            ((NoteEditorCtrl)component).DeletedEntity += Store_DeletedNote;
            ((NoteEditorCtrl)component).PostItEdit += Store_EditedPostItNote;
        }

        _listComponents.Add(component);
        Logger?.LogInformation("Added Component {component}", component.ToString());
        AddedComponent?.Invoke(this, new ComponentEventArgs<CtrlBase>(component));
    }

    public event EventHandler<ComponentEventArgs<CtrlBase>> RemovedComponent;
    public void RemoveComponent(CtrlBase component)
    {
        component.StateComponentChanged -= Components_StateCtrlChanged;

        if (component is PostItEditorCtrl)
        {
            ((PostItEditorCtrl)component).AddedEntity -= Store_AddedPostIt;
            ((PostItEditorCtrl)component).SavedEntity -= Store_SavedPostIt;
            ((PostItEditorCtrl)component).DeletedEntity -= Store_DeletedPostIt;
            ((PostItEditorCtrl)component).ExtendedEdit -= Store_ExtendedEditPostIt;
        }
        else if (component is NoteEditorCtrl)
        {
            ((NoteEditorCtrl)component).AddedEntity += Store_AddedNote;
            ((NoteEditorCtrl)component).SavedEntity += Store_SavedNote;
            ((NoteEditorCtrl)component).DeletedEntity += Store_DeletedNote;
            ((NoteEditorCtrl)component).PostItEdit += Store_EditedPostItNote;
        }

        _listComponents.Remove(component);
        Logger?.LogInformation("Removed Component {component}", component.ToString());
        RemovedComponent?.Invoke(this, new ComponentEventArgs<CtrlBase>(component));
    }

    public void SaveConfig(string configFile = null)
    {
        if(string.IsNullOrEmpty(configFile))
            configFile = Path.Combine(Application.StartupPath, "KNoteData.config");
        try
        {
            TextWriter w = new StreamWriter(configFile);
            XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
            serializer.Serialize(w, AppConfig);
            w.Close();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "SaveConfig: {message}", configFile?.ToString());
            throw;
        }
    }

    public void LoadConfig(string configFile = @"KNoteData.config")
    {
        try
        {
            if (!File.Exists(configFile))
                return;
            
            TextReader reader = new StreamReader(configFile);
            XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
            AppConfig = (AppConfig)serializer.Deserialize(reader);
            AppConfig.LastDateTimeStart = DateTime.Now;
            AppConfig.RunCounter++;
            reader.Close();                
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "LoadConfig: {message}", configFile?.ToString());
            throw;
        }            
    }

    public Task<bool> CheckNoteIsActive(Guid noteId)
    {
        foreach(var com in _listComponents)
        {
            if (com is NoteEditorCtrl)
            {
                var comNote = (NoteEditorCtrl)com;
                if (comNote.Model.NoteId == noteId && comNote.EditMode == true )
                    return Task.FromResult(true);

            }
        }
        return Task.FromResult(false);
    }

    public Task<bool> CheckPostItIsActive(Guid noteId)
    {
        foreach (var com in _listComponents)
        {
            if (com is PostItEditorCtrl)
                if (((PostItEditorCtrl)com).Model.NoteId == noteId)
                    return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public async Task<bool> SaveActiveNotes()
    {
        try
        {
            foreach (var com in _listComponents)
            {
                if (com is PostItEditorCtrl)
                    await ((PostItEditorCtrl)com).SaveModel();

                if (com is NoteEditorCtrl)
                {
                    var comNote = (NoteEditorCtrl)com;
                    if (comNote.EditMode)
                        await comNote.SaveModel();
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "SaveActiveNotes.");
            return false;
        }
    }

    public async Task<bool> SaveAndCloseActiveNotes(Guid serviceId)
    {
        var stackPostIts = new Stack<PostItEditorCtrl>();
        var stackNotes = new Stack<NoteEditorCtrl>();

        try
        {
            foreach (var com in _listComponents)
            {
                if (com is PostItEditorCtrl)
                {                        
                    var comNote = (PostItEditorCtrl)com;
                    if (comNote.ServiceRef.IdServiceRef == serviceId)
                    {
                        await comNote.SaveModel();                            
                        stackPostIts.Push(comNote);
                    }
                }
                if (com is NoteEditorCtrl)
                {
                    var comNote = (NoteEditorCtrl)com;
                    if (comNote.ServiceRef.IdServiceRef == serviceId)
                    {
                        if (comNote.EditMode)
                        {
                            await comNote.SaveModel();                                
                            stackNotes.Push(comNote);
                        }
                    }                        
                }
            }
            while(stackPostIts.Count > 0)
            {
                var postIt = stackPostIts.Pop();
                postIt.Finalize();
            }
            while (stackNotes.Count > 0)
            {
                var note = stackNotes.Pop();
                note.Finalize();
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "SaveAndCloseActiveNotes.");
            return false;
        }
    }

    public void HidePostIts()
    {
        foreach (var com in _listComponents)
        {
            if (com is PostItEditorCtrl)
            {

                ((PostItEditorCtrl)com).HidePostIt();
            }
        }            
    }

    public void ActivatePostIts()
    {
        foreach (var com in _listComponents)
        {
            if (com is PostItEditorCtrl)
                ((PostItEditorCtrl)com).ActivatePostIt();
        }
    }

    public void RunScript(string code)
    {
        if (string.IsNullOrEmpty(code))
            return;

        var kntScript = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(this), false);
        kntScript.Run(code);
    }

    public void RunScriptInNewThread(string code)
    {
        if (string.IsNullOrEmpty(code))
            return;

        var kntScript = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(this));
                
        var t = new Thread(() => kntScript.Run(code));
        t.IsBackground = false;
        t.Start();        
    }

    public string ExtensionFileToFileType(string extension)
    {
        // TODO: Refactor this method

        var ext = extension.ToLower();

        if (ext == ".jpg")
            return @"image/jpeg";
        else if (ext == ".jpeg")
            return @"image/jpeg";
        else if (ext == ".png")
            return "image/png";
        else if (ext == ".pdf")
            return "application/pdf";
        else if (ext == ".mp4")
            return "video/mp4";
        else if (ext == ".mp3")
            return "audio/mp3";
        else if (ext == ".txt")
            return "text/plain";
        else if (ext == ".text")
            return "text/plain";
        else if (ext == ".htm")
            return "text/plain";
        else if (ext == ".html")
            return "text/plain";
        else
            return "";            
    }
    
    public bool IsSupportedFileTypeForPreview(string fileType)
    {
        // TODO: Refactor this method
        if (string.IsNullOrEmpty(fileType))
            return false;

        return KntConst.SupportedMimeTypes.Contains(fileType);            
    }

    public async Task<Guid?> GetUserId(IKntService service)
    {
        var userDto = (await service.Users.GetByUserNameAsync(this.AppUserName)).Entity;
        if (userDto != null)
            return userDto.UserId;
        else 
            return null;
    }

    #endregion

    #region Plugins

    public string GetVsSolutionRootPath()
    {
        // Navigate up to the solution root
        string root = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(typeof(Program).Assembly.Location)))))));
        return root;
    }

    public Assembly LoadPlugin(string pluginLocation)
    {
        PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
        return loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(pluginLocation));
    }

    public IEnumerable<IPluginCommand> CreateCommands(Assembly assembly)
    {
        int count = 0;

        foreach (Type type in assembly.GetTypes())
        {
            if (typeof(IPluginCommand).IsAssignableFrom(type))
            {
                IPluginCommand result = Activator.CreateInstance(type) as IPluginCommand;
                if (result != null)
                {
                    count++;
                    yield return result;
                }
            }
        }

        if (count == 0)
        {
            string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
            throw new ApplicationException(
                $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                $"Available types: {availableTypes}");
        }
    }

    #endregion

    #region Helper event handlers 

    public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> EditedPostItNote;
    private void Store_EditedPostItNote(object sender, ComponentEventArgs<ServiceWithNoteId> e)
    {
        EditedPostItNote?.Invoke(sender, e);
    }

    public event EventHandler<ComponentEventArgs<NoteExtendedDto>> DeletedNote;
    private void Store_DeletedNote(object sender, ComponentEventArgs<NoteExtendedDto> e)  
    {
        DeletedNote?.Invoke(sender, e);
    }

    public event EventHandler<ComponentEventArgs<NoteExtendedDto>> SavedNote;
    private void Store_SavedNote(object sender, ComponentEventArgs<NoteExtendedDto> e)
    {
        SavedNote?.Invoke(sender, e);
    }

    public event EventHandler<ComponentEventArgs<NoteExtendedDto>> AddedNote;
    private void Store_AddedNote(object sender, ComponentEventArgs<NoteExtendedDto> e)
    {
        AddedNote?.Invoke(sender, e);
    }

    public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> ExtendedEditPostIt;
    private void Store_ExtendedEditPostIt(object sender, ComponentEventArgs<ServiceWithNoteId> e)
    {
        ExtendedEditPostIt?.Invoke(sender, e);
    }

    public event EventHandler<ComponentEventArgs<NoteDto>> DeletedPostIt;
    private void Store_DeletedPostIt(object sender, ComponentEventArgs<NoteDto> e)
    {
        DeletedPostIt?.Invoke(sender, e);
    }

    public event EventHandler<ComponentEventArgs<NoteDto>> SavedPostIt;
    private void Store_SavedPostIt(object sender, ComponentEventArgs<NoteDto> e)
    {
        SavedPostIt?.Invoke(sender, e);
    }

    public event EventHandler<ComponentEventArgs<NoteDto>> AddedPostIt;
    private void Store_AddedPostIt(object sender, ComponentEventArgs<NoteDto> e)
    {
        AddedPostIt?.Invoke(sender, e);
    }

    private void Components_StateCtrlChanged(object sender, ComponentEventArgs<EComponentState> e)
    {
        ComponentsStateChanged?.Invoke(sender, e);
    }

    public event EventHandler<ComponentEventArgs<string>> ComponentNotification;
    internal void OnComponentNotification(CtrlBase component, string message)
    {
        ComponentNotification?.Invoke(component, new ComponentEventArgs<string>(message));
    }

    #endregion

    #region Utils public methods

    public DateTime? TextToDateTime(string text)
    {
        DateTime output;
        if (DateTime.TryParse(text, out output))
            return output;
        else
            return null;
    }

    public int TextToInt(string text)
    {
        int output;
        if (int.TryParse(text, out output))
            return output;
        else
            return 0;
    }

    public double? TextToDouble(string text)
    {
        double output;
        if (double.TryParse(text, out output))
            return output;
        else
            return null;
    }

    //TODO: !!! Delete form here (tralated to KntEditViewControl 
    public string ExtractUrlFromText(string text)
    {      
        if (string.IsNullOrEmpty(text))        
            return null;
        
        int indexJump = text.IndexOfAny(newLine);
        var urlFistLine =  (indexJump >= 0) ? text.Substring(0, indexJump) : text;

        Uri resultUri;
        var validResult = Uri.TryCreate(urlFistLine, UriKind.Absolute, out resultUri) &&
               (resultUri.Scheme == Uri.UriSchemeHttp || resultUri.Scheme == Uri.UriSchemeHttps || resultUri.Scheme == Uri.UriSchemeFile);

        if (validResult)
            return urlFistLine;
        else
            return null;
    }

    #endregion 
}
