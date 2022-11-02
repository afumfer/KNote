using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using KNote.Model;
using KNote.ClientWin.Components;
using KntScript;
using KNote.ClientWin.Views;
using KNote.Service;
using System.Threading;
using System.Windows.Forms;
using KNote.Model.Dto;
using System.Reflection;
using System.Windows.Input;
using System.Runtime.Loader;

namespace KNote.ClientWin.Core;

public class Store
{
    #region Constants

    const string SUPORTED_MIME_TYPES = @"image/jpeg;image/png;application/pdf;video/mp4;audio/mp3;text/plain";

    #endregion

    #region Private fields

    private readonly List<ServiceRef> _servicesRefs;

    private readonly List<ComponentBase> _listComponents;

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

    #endregion

    #region Constructor 

    public Store(IFactoryViews factoryViews)
    {
        if (AppConfig == null)
            AppConfig = new AppConfig();

        _listComponents = new List<ComponentBase>();
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
            ChangedActiveFolderWithServiceRef?.Invoke(this, new ComponentEventArgs<FolderWithServiceRef>(activeFolderWithServiceRef));
        }
    }

    public event EventHandler<ComponentEventArgs<NotesFilterWithServiceRef>> ChangedActiveFilterWithServiceRef;
    public void ChangeActiveFilterWithServiceRef(NotesFilterWithServiceRef activeFilterWithServiceRef)
    {
        if (_activeFilterWithServiceRef != activeFilterWithServiceRef)
        {
            _activeFilterWithServiceRef = activeFilterWithServiceRef;
            ChangedActiveFilterWithServiceRef?.Invoke(this, new ComponentEventArgs<NotesFilterWithServiceRef>(activeFilterWithServiceRef));
        }
    }
  
    public event EventHandler<ComponentEventArgs<ServiceRef>> AddedServiceRef;
    public void AddServiceRef(ServiceRef serviceRef)
    {
        _servicesRefs.Add(serviceRef);                        
        AddedServiceRef?.Invoke(this, new ComponentEventArgs<ServiceRef>(serviceRef));
    }

    public void AddServiceRefInAppConfig(ServiceRef serviceRef)
    {
        AppConfig.RespositoryRefs.Add(serviceRef.RepositoryRef);
    }

    public event EventHandler<ComponentEventArgs<ServiceRef>> RemovedServiceRef;
    public void RemoveServiceRef(ServiceRef serviceRef)
    {            
        _servicesRefs.Remove(serviceRef);
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

    public event EventHandler<ComponentEventArgs<ComponentBase>> AddedComponent;
    public event EventHandler<ComponentEventArgs<EComponentState>> ComponentsStateChanged;
    public void AddComponent(ComponentBase component)
    {
        component.StateComponentChanged += Components_StateCtrlChanged;
        
        if (component is PostItEditorComponent)
        {
            ((PostItEditorComponent)component).AddedEntity += Store_AddedPostIt;
            ((PostItEditorComponent)component).SavedEntity += Store_SavedPostIt;
            ((PostItEditorComponent)component).DeletedEntity += Store_DeletedPostIt;
            ((PostItEditorComponent)component).ExtendedEdit += Store_ExtendedEditPostIt;
        }
        else if (component is NoteEditorComponent)
        {
            ((NoteEditorComponent)component).AddedEntity += Store_AddedNote;
            ((NoteEditorComponent)component).SavedEntity += Store_SavedNote;
            ((NoteEditorComponent)component).DeletedEntity += Store_DeletedNote;
            ((NoteEditorComponent)component).PostItEdit += Store_EditedPostItNote;
        }

        _listComponents.Add(component);
        AddedComponent?.Invoke(this, new ComponentEventArgs<ComponentBase>(component));
    }

    public event EventHandler<ComponentEventArgs<ComponentBase>> RemovedComponent;
    public void RemoveComponent(ComponentBase component)
    {
        component.StateComponentChanged -= Components_StateCtrlChanged;

        if (component is PostItEditorComponent)
        {
            ((PostItEditorComponent)component).AddedEntity -= Store_AddedPostIt;
            ((PostItEditorComponent)component).SavedEntity -= Store_SavedPostIt;
            ((PostItEditorComponent)component).DeletedEntity -= Store_DeletedPostIt;
            ((PostItEditorComponent)component).ExtendedEdit -= Store_ExtendedEditPostIt;
        }
        else if (component is NoteEditorComponent)
        {
            ((NoteEditorComponent)component).AddedEntity += Store_AddedNote;
            ((NoteEditorComponent)component).SavedEntity += Store_SavedNote;
            ((NoteEditorComponent)component).DeletedEntity += Store_DeletedNote;
            ((NoteEditorComponent)component).PostItEdit += Store_EditedPostItNote;
        }

        _listComponents.Remove(component);
        RemovedComponent?.Invoke(this, new ComponentEventArgs<ComponentBase>(component));
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
        catch (Exception)
        {
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
        catch (Exception)
        {
            throw;
        }            
    }

    public async Task<bool> CheckNoteIsActive(Guid noteId)
    {
        foreach(var com in _listComponents)
        {
            if (com is NoteEditorComponent)
            {
                var comNote = (NoteEditorComponent)com;
                if (comNote.Model.NoteId == noteId && comNote.EditMode == true )
                    return await Task.FromResult<bool>(true);

            }
        }
        return await Task.FromResult<bool>(false);
    }

    public async Task<bool> CheckPostItIsActive(Guid noteId)
    {
        foreach (var com in _listComponents)
        {
            if (com is PostItEditorComponent)
                if (((PostItEditorComponent)com).Model.NoteId == noteId)
                    return await Task.FromResult<bool>(true);
        }
        return await Task.FromResult<bool>(false);
    }

    public async Task<bool> SaveActiveNotes()
    {
        try
        {
            foreach (var com in _listComponents)
            {
                if (com is PostItEditorComponent)
                    await ((PostItEditorComponent)com).SaveModel();

                if (com is NoteEditorComponent)
                {
                    var comNote = (NoteEditorComponent)com;
                    if (comNote.EditMode)
                        await comNote.SaveModel();
                }
            }
            return await Task.FromResult<bool>(true);
        }
        catch (Exception)
        {
            return await Task.FromResult<bool>(false);
        }
    }

    public async Task<bool> SaveAndCloseActiveNotes(Guid serviceId)
    {
        var stackPostIts = new Stack<PostItEditorComponent>();
        var stackNotes = new Stack<NoteEditorComponent>();

        try
        {
            foreach (var com in _listComponents)
            {
                if (com is PostItEditorComponent)
                {                        
                    var comNote = (PostItEditorComponent)com;
                    if (comNote.ServiceRef.IdServiceRef == serviceId)
                    {
                        await comNote.SaveModel();                            
                        stackPostIts.Push(comNote);
                    }
                }
                if (com is NoteEditorComponent)
                {
                    var comNote = (NoteEditorComponent)com;
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

            return await Task.FromResult<bool>(true);
        }
        catch (Exception)
        {                
            return await Task.FromResult<bool>(false);
        }
    }

    public void HidePostIts()
    {
        foreach (var com in _listComponents)
        {
            if (com is PostItEditorComponent)
            {

                ((PostItEditorComponent)com).HidePostIt();
            }
        }            
    }

    public void ActivatePostIts()
    {
        foreach (var com in _listComponents)
        {
            if (com is PostItEditorComponent)
                ((PostItEditorComponent)com).ActivatePostIt();
        }
    }

    public void RunScript(string code, bool newThread = true)
    {
        if (string.IsNullOrEmpty(code))
            return;

        var kntScript = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(this));
                              
        if (newThread)
        {
            var t = new Thread(() => kntScript.Run(code));
            t.IsBackground = false;
            t.Start();
        }
        else
            kntScript.Run(code);
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

        return SUPORTED_MIME_TYPES.Contains(fileType);            
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
    internal void OnComponentNotification(ComponentBase component, string message)
    {
        ComponentNotification?.Invoke(component, new ComponentEventArgs<string>(message));
    }

    #endregion

}
