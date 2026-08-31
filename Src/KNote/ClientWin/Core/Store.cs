using KNote.ClientWin.Controllers;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.EntityFramework.Entities;
using KNote.Service.Core;
using KntScript;
using Microsoft.Extensions.Logging;

using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;

namespace KNote.ClientWin.Core;

public class Store
{
    #region Private fields

    private readonly ServiceRefRegistry _serviceRefRegistry;

    private readonly ControllerRegistry _controllerRegistry;

    private ServiceRef _assistantServiceRef;

    #endregion 

    #region Public properties, application state 

    public AppConfig AppConfig { get; protected set; }

    public string AppUserName { get; set; }

    public string ComputerName { get; set; }

    public Version AppVersion { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }

    public readonly IFactoryViews FactoryViews;

    public DomainEventBus Events { get; } = new();

    private KntTextUtils _kntTextUtils;
    public KntTextUtils KntTextUtils
    {
        get
        {
            _kntTextUtils ??= new KntTextUtils();
            return _kntTextUtils;
        }
    }

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

    public SelectedNotesInServiceRef _selectedNotesInServiceRef;
    public SelectedNotesInServiceRef ActiveFilterWithServiceRef
    {            
        get { return _selectedNotesInServiceRef; }
    }

    public ILogger Logger { get; set; }

    private string _kNoteWebViewStyle = null;    
    public string KNoteWebViewStyle
    {
        get
        {
            if (_kNoteWebViewStyle == null)
            {
                if (File.Exists(@$"{AppContext.BaseDirectory}\KNoteWebViewStyle.css"))
                {
                    var css = File.ReadAllText(@$"{AppContext.BaseDirectory}\KNoteWebViewStyle.css");
                    _kNoteWebViewStyle = $"<style>{css}</style>";
                }
                else
                    _kNoteWebViewStyle = "";
            }
            return _kNoteWebViewStyle;
        }
    }

    #endregion

    #region Constructor 

    public Store(IFactoryViews factoryViews)
    {
        if (AppConfig == null)
            AppConfig = new AppConfig();

        _controllerRegistry = new ControllerRegistry();
        _serviceRefRegistry = new ServiceRefRegistry();
        FactoryViews = factoryViews; //
    }

    public Store(AppConfig config, IFactoryViews factoryViews) : this (factoryViews)
    {
        AppConfig = config;
    }

    #endregion

    #region Actions    

    public event EventHandler<ControllerEventArgs<FolderWithServiceRef>> ChangedActiveFolderWithServiceRef;
    public void ChangeActiveFolderWithServiceRef(FolderWithServiceRef activeFolderWithServiceRef)
    {
        if(_activeFolderWithServiceRef != activeFolderWithServiceRef)
        {
            _activeFolderWithServiceRef = activeFolderWithServiceRef;
            Logger?.LogTrace("ChangeActiveFolderWithServiceRef {message}", activeFolderWithServiceRef?.ToString());
            ChangedActiveFolderWithServiceRef?.Invoke(this, new ControllerEventArgs<FolderWithServiceRef>(activeFolderWithServiceRef));
        }
    }

    public event EventHandler<ControllerEventArgs<SelectedNotesInServiceRef>> ChangedActiveFilterWithServiceRef;
    public void ChangeSelectedNotesInServiceRef(SelectedNotesInServiceRef selectedNotesInServiceRef)
    {
        if (_selectedNotesInServiceRef != selectedNotesInServiceRef)
        {
            _selectedNotesInServiceRef = selectedNotesInServiceRef;
            Logger?.LogTrace("ChangeActiveFilterWithServiceRef {message}", selectedNotesInServiceRef?.ToString());
            ChangedActiveFilterWithServiceRef?.Invoke(this, new ControllerEventArgs<SelectedNotesInServiceRef>(selectedNotesInServiceRef));
        }
    }
  
    public event EventHandler<ControllerEventArgs<ServiceRef>> AddedServiceRef;
    public void AddServiceRef(ServiceRef serviceRef)
    {
        if(serviceRef is null)
            throw new ArgumentNullException(nameof(serviceRef));

        _serviceRefRegistry.Add(serviceRef);
        Logger?.LogInformation("Added ServiceRef {component}", serviceRef.ToString());
        AddedServiceRef?.Invoke(this, new ControllerEventArgs<ServiceRef>(serviceRef));
    }

    public void AddServiceRefInAppConfig(ServiceRef serviceRef)
    {
        if (serviceRef is null)
            throw new ArgumentNullException(nameof(serviceRef));

        AppConfig.RespositoryRefs.Add(serviceRef.RepositoryRef);
    }

    public event EventHandler<ControllerEventArgs<ServiceRef>> RemovedServiceRef;
    public void RemoveServiceRef(ServiceRef serviceRef)
    {
        if (serviceRef is null)
            throw new ArgumentNullException(nameof(serviceRef));

        _serviceRefRegistry.Remove(serviceRef);
        Logger?.LogInformation("Removed ServiceRef {component}", serviceRef.ToString());
        AppConfig.RespositoryRefs.Remove(serviceRef.RepositoryRef);            
        RemovedServiceRef?.Invoke(this, new ControllerEventArgs<ServiceRef>(serviceRef));
    }

    public List<ServiceRef> GetAllServiceRef()
    {
        return _serviceRefRegistry.GetAll();
    }

    public ServiceRef GetServiceRef(Guid id)
    {
        return _serviceRefRegistry.GetById(id);
    }

    public ServiceRef GetServiceRef(string alias)
    {
        return _serviceRefRegistry.GetByAlias(alias);
    }

    public ServiceRef GetFirstServiceRef()
    {
        return _serviceRefRegistry.GetFirst();
    }

    public IKntService GetActiveOrDefaultService()
    {
        if (ActiveFolderWithServiceRef != null)
            return ActiveFolderWithServiceRef.ServiceRef.Service;
        else
            return GetFirstServiceRef().Service;
    }

    public ServiceRef GetActiveOrDefaultServiceRef()
    {
        if (ActiveFolderWithServiceRef != null)
            return ActiveFolderWithServiceRef.ServiceRef;
        else
            return GetFirstServiceRef();
    }

    public void SetAssistantServiceRef(ServiceRef assistantServiceRef)
    {
        _assistantServiceRef = assistantServiceRef;
    }

    public ServiceRef GetAssistantServiceRef()
    {
        return _assistantServiceRef ;
    }

    public event EventHandler<ControllerEventArgs<CtrlBase>> AddedController;
    public event EventHandler<ControllerEventArgs<EControllerState>> ControllerStateChanged;
    public void AddController(CtrlBase controller)
    {
        controller.StateControllerChanged += Controller_StateCtrlChanged;

        _controllerRegistry.Add(controller);
        Logger?.LogInformation("Added Component {component}", controller.ToString());
        AddedController?.Invoke(this, new ControllerEventArgs<CtrlBase>(controller));
    }

    public event EventHandler<ControllerEventArgs<CtrlBase>> RemovedController;
    public void RemoveController(CtrlBase controller)
    {
        controller.StateControllerChanged -= Controller_StateCtrlChanged;

        _controllerRegistry.Remove(controller);
        Logger?.LogInformation("Removed Component {component}", controller.ToString());
        RemovedController?.Invoke(this, new ControllerEventArgs<CtrlBase>(controller));
    }

    public void SaveConfig(string configFile = null)
    {
        if(string.IsNullOrEmpty(configFile))
            configFile = AppUserDataPath.ConfigFile;
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

    public void LoadConfig(string configFile = null)
    {
        try
        {
            if (string.IsNullOrEmpty(configFile))
                configFile = AppUserDataPath.ConfigFile;


            if (!File.Exists(configFile))
                return;
            
            TextReader reader = new StreamReader(configFile);
            XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
            AppConfig = (AppConfig)serializer.Deserialize(reader);
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
        foreach (var com in _controllerRegistry.All)
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
        foreach (var com in _controllerRegistry.All)
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
            foreach (var com in _controllerRegistry.All)
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
            foreach (var com in _controllerRegistry.All)
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
        foreach (var com in _controllerRegistry.All)
        {
            if (com is PostItEditorCtrl)
            {
                ((PostItEditorCtrl)com).HidePostIt();
            }
        }            
    }

    public void ActivatePostIts()
    {
        foreach (var com in _controllerRegistry.All)
        {
            if (com is PostItEditorCtrl)
                ((PostItEditorCtrl)com).ActivatePostIt();
        }
    }

    public async Task<Guid?> GetUserId(IKntService service)
    {
        var userDto = (await service.Users.GetByUserNameAsync(this.AppUserName)).Entity;
        if (userDto != null)
            return userDto.UserId;
        else
            return null;
    }

    /// <summary>
    /// Checks whether the current Windows user (AppUserName) has the Admin role in the given
    /// repository's Users table. Used to gate the repository administration tabs (Users, Note types,
    /// Attributes) in RepositoryEditorCtrl/RepositoryEditorForm.
    /// </summary>
    public async Task<bool> IsCurrentUserAdminAsync(IKntService service)
    {
        var userDto = (await service.Users.GetByUserNameAsync(this.AppUserName)).Entity;
        if (userDto?.RoleDefinition == null)
            return false;

        return userDto.RoleDefinition
            .Split(',', StringSplitOptions.TrimEntries)
            .Contains(nameof(EnumRoles.Admin));
    }

    /// <summary>
    /// Checks whether the current Windows user (AppUserName) is registered in the Users table of
    /// the given repository, and if not, shows a modal registration dialog for it. Cancelling the
    /// dialog is not blocking: the app keeps running against that repository without the user
    /// registered (existing guards elsewhere, e.g. PostItEditorCtrl, already handle that case).
    /// </summary>
    public async Task<bool> EnsureCurrentUserRegistered(IKntService service)
    {
        if (await GetUserId(service) != null)
            return true;

        var userRegisterCtrl = new UserRegisterCtrl(this);
        await userRegisterCtrl.NewModel(service);
        var result = userRegisterCtrl.RunModal();
        return result.Entity == EControllerResult.Executed;
    }

    #endregion

    #region Common controllers extensions

    private NotesSelectorCtrl _notesSelector = null;
    protected NotesSelectorCtrl NotesSelector
    {
        get
        {
            if (_notesSelector == null)
            {
                _notesSelector = new NotesSelectorCtrl(this);
                _notesSelector.EmbededMode = false;
                _notesSelector.HiddenColumns = "NoteNumber, Priority, Tags, InternalTags, ModificationDateTime, CreationDateTime";
            }
            return _notesSelector;
        }
    }

    #endregion 

    #region Helper event handlers

    private void Controller_StateCtrlChanged(object sender, ControllerEventArgs<EControllerState> e)
    {
        ControllerStateChanged?.Invoke(sender, e);
    }

    public event EventHandler<ControllerEventArgs<string>> ControllerNotification;
    internal void OnControllerNotification(CtrlBase controller, string message)
    {
        ControllerNotification?.Invoke(controller, new ControllerEventArgs<string>(message));
    }

    #endregion

    #region Utils public methods

    public async Task<NoteDto> GetCatalogItem(ServiceRef serviceRef, string item, string viewTitle)
    {
        await NotesSelector.LoadFilteredEntities(serviceRef.Service, new NotesFilterDto { Tags = item }, false);
        NotesSelector.ViewTitle = viewTitle;
        
        var res = NotesSelector.RunModal();

        if (res.Entity == EControllerResult.Executed) 
        {
            //return NotesSelector.SelectedEntity;            
            return NotesSelector.Service.Notes.GetAsync(NotesSelector.SelectedEntity.NoteId).Result.Entity;
        }        
        else
            return null;
    }

    // TODO: (Experimental ###)
    public async Task<string> GetKNoteFolerPath(ServiceRef serviceRef, Guid folderId)
    {
        string folderPath = string.Empty;
        Guid? parentFolderId = null;        

        if(serviceRef == null)
            return string.Empty;

        do
        {
            var res = await serviceRef.Service.Folders.GetAsync(folderId);

            if(res.IsValid)
            {
                FolderDto folder = res.Entity;
                if (folder == null)
                    break;
                if (string.IsNullOrEmpty(folderPath))
                    folderPath = folder.Name;
                else
                    folderPath = folder.Name + "\\" + folderPath;
                parentFolderId = folder.ParentId;
                folderId = parentFolderId ?? Guid.Empty; // If no parent, exit loop
            }
            else
            {
                Logger?.LogError("GetKNoteFolerPath: {message}", res.ErrorMessage);
                break;
            }

        } while (parentFolderId != null);

        return folderPath;
    }

    #endregion

    #region KntScript and C# code execution

    public async Task RunCode(NoteDto note, bool runInNewTask = true, CtrlBase caller = null)
    {
        var ct = note.GetContentTypeExt();
        if (ct == null || string.IsNullOrEmpty(ct.ForScript))
            return;

        var code = note?.Script ?? "";

        // Minor UX indicator that a script is running (Fase A+B: wait cursor, always reliable even
        // when the engine below blocks the UI thread synchronously; status bar message via the
        // existing ControllerNotification "toast" channel, shown by KNoteManagmentForm when visible -
        // best-effort only for the "knt" + runInNewTask=true case, which fires the script on its own
        // thread and returns immediately, so the indicator only covers the hand-off, not the full run).
        Cursor.Current = Cursors.WaitCursor;
        OnControllerNotification(caller, $"Running {ScriptEngineLabel(ct.ForScript)}...");
        try
        {
            switch (ct.ForScript)
            {
                case "cs":
                    // Experimental hack, insert global include
                    code += await GetIncludeGlobalCode("cs");

                    if (runInNewTask)
                        var (result, error) = await RunCSCodeInNewTask(code, false);
                    else
                        var (result, error) = RunCSCode(code, false);
                    break;

                case "py":
                    // Experimental hack, insert global include
                    code += await GetIncludeGlobalCode("py");

                    if (runInNewTask)
                        var (resultPy, errorPy) = await RunPyCodeInNewTask(code, false);
                    else
                        var (resultPy, errorPy) = RunPyCode(code, false);
                    break;

                case "js":
                    // Experimental hack, insert global include
                    code += await GetIncludeGlobalCode("js");

                    if (runInNewTask)
                        var (resultJs, errorJs) = await RunJsCodeInNewTask(code, false);
                    else
                        var (resultJs, errorJs) = RunJsCode(code, false);
                    break;

                case "ln":
                    // Unlike cs/py/js there's no Process.WaitForExit() to move off the UI thread here -
                    // GetCompletionAsync is already non-blocking async I/O - so runInNewTask doesn't apply.
                    await RunNaturalLanguageCode(code);
                    break;

                default:
                    // Experimental hack, insert global include
                    code += await GetIncludeGlobalCode("knt");

                    if (runInNewTask)
                        RunKntSCodeInNewThread(code);
                    else
                        RunKntSCode(code);
                    break;
            }
        }
        finally
        {
            Cursor.Current = Cursors.Default;
            OnControllerNotification(caller, "");
        }
    }

    private static string ScriptEngineLabel(string forScript) => forScript switch
    {
        "cs" => "C# script",
        "py" => "Python script",
        "js" => "JavaScript script",
        "ln" => "natural-language prompt",
        _ => "KntScript",
    };

    public void RunKntSCodeInNewThread(string code)
    {
        var t = new Thread(() => RunKntSCode(code));        
        t.IsBackground = false;
        t.Start();
    }

    public void RunKntSCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return;



        var kntScript = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(this), false);
        kntScript.Run(code);
    }

    // "Natural language" script engine: the note's Script field is sent as a prompt to
    // KNoteAIAssistantCtrl (same completion path used by the AI assistant UI/KntScript's
    // GetKNoteAIAssistantMessage), and the reply is shown in its own view. A fresh Ctrl per
    // execution, closed on view exit, so each run starts a clean conversation - same as
    // cs/py/js/knt, none of which carry state between executions either.
    public async Task RunNaturalLanguageCode(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return;

        var assistantCtrl = new KNoteAIAssistantCtrl(this);
        assistantCtrl.ResponseMode = EAiResponseMode.Completion;
        var runResult = assistantCtrl.Run();
        if (!runResult.IsValid)
            return;

        await assistantCtrl.GetCompletionAsync(prompt);
        assistantCtrl.ShowAIAssistantView(autoCloseCtrlOnViewExit: true, autoSaveChatMessagesOnViewExit: false);
    }

    public Task<(string, string)> RunCSCodeInNewTask(string code, bool redirectStandardOut)
    {
        return Task.Run(() => RunCSCode(code, redirectStandardOut));
    }

    public (string, string) RunCSCode(string code, bool redirectStandardOut)
        => RunScriptCode(code, "cs", "dotnet run {0}", redirectStandardOut);

    public Task<(string, string)> RunPyCodeInNewTask(string code, bool redirectStandardOut)
    {
        return Task.Run(() => RunPyCode(code, redirectStandardOut));
    }

    public (string, string) RunPyCode(string code, bool redirectStandardOut)
        => RunScriptCode(code, "py", "python {0}", redirectStandardOut);

    public Task<(string, string)> RunJsCodeInNewTask(string code, bool redirectStandardOut)
    {
        return Task.Run(() => RunJsCode(code, redirectStandardOut));
    }

    public (string, string) RunJsCode(string code, bool redirectStandardOut)
        => RunScriptCode(code, "js", "node {0}", redirectStandardOut);

    // Shared by every "write to a temp file, then shell out to an interpreter/compiler" script
    // engine (C#, Python, JavaScript, and future ones) - only the file extension and the launch
    // command differ.
    // runCommandTemplate gets the generated file name via {0} (e.g. "dotnet run {0}", "python {0}").
    private (string, string) RunScriptCode(string code, string fileExtension, string runCommandTemplate, bool redirectStandardOut)
    {
        string tempDir = Path.GetTempPath();
        string nameFile = $"kntTmpCodeFile_{Guid.NewGuid().ToString()}.{fileExtension}";
        string tempFullFileName = Path.Combine(tempDir, nameFile);

        File.WriteAllText(tempFullFileName, code);

        (var result, var error) = ExecuteCommand(string.Format(runCommandTemplate, nameFile), tempDir, redirectStandardOut);

        File.Delete(tempFullFileName);

        return (result, error);
    }

    public (string, string) ExecuteCommand(string command, string dir, bool redirectStandardOut = true)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    // /C tells CMD to execute the command or set of commands specified below and then terminate.
                    // Multiple commands can be concatenated like this: / C command1 && command2 && command3
                    //   Example: Arguments = $"/C {command} && echo %cd%",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = redirectStandardOut,
                    RedirectStandardError = redirectStandardOut,
                    UseShellExecute = !redirectStandardOut,
                    CreateNoWindow = !redirectStandardOut,
                    WorkingDirectory = dir
                }
            };

            process.Start();
            string result = "";
            string resultError = "";
            if (redirectStandardOut)
            {
                result = process.StandardOutput.ReadToEnd();
                resultError = process.StandardError.ReadToEnd();
            }
            process.WaitForExit();            
            return (result , resultError);
        }
        catch (System.Exception ex)
        {
            return ("Exception message:", $"Error: {ex.Message}");
        }
    }
    
    public async Task<string> GetIncludeGlobalCode(string codeType)
    {
        string codeResult = string.Empty;

        var assistantServiceRef = GetAssistantServiceRef();
        var includes = await assistantServiceRef.Service.Notes.GetFilterAsync(new NotesFilterDto { Tags = KntConst.IncludeGlobalCodeTag });

        foreach (var inc in includes.Entity)
        {
            var ct = inc.GetContentTypeExt();
            if (ct != null && ct.ForScript == codeType)
                codeResult += $"\r\n\r\n{inc.Script}";
        }

        return codeResult;
    }

    #endregion 
}
  