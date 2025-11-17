using System.Diagnostics;
using System.Runtime.InteropServices;
using KNote.ClientWin.Core;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KntScript;

namespace KNote.ClientWin.Controllers;

public class KNoteManagmentCtrl : CtrlViewBase<IViewKNoteManagment>
{
    #region Private fields

    private static readonly object lockObject = new object();
    private static bool loadingNote = false;

    #endregion 

    #region Properties

    public FolderWithServiceRef SelectedFolderWithServiceRef 
    {
        get { return Store.ActiveFolderWithServiceRef; }
        set 
        {                
            Store.ChangeActiveFolderWithServiceRef(value);
        } 
    }

    public SelectedNotesInServiceRef SelectedNotesInServiceRef
    {
        get { return Store.ActiveFilterWithServiceRef; }
        set 
        {                
            Store.ChangeSelectedNotesInServiceRef(value);
        }
    }

    public FolderInfoDto SelectedFolderInfo
    {
        get { return SelectedFolderWithServiceRef?.FolderInfo; }
    }

    public ServiceRef SelectedServiceRef
    {
        get {
            if(SelectMode == EnumSelectMode.Filters)
                return SelectedNotesInServiceRef?.ServiceRef;
            else
                return SelectedFolderWithServiceRef?.ServiceRef;
        }
    }

    public EnumSelectMode SelectMode { get; set; } = EnumSelectMode.Folders;

    public FolderWithServiceRef DefaultFolderWithServiceRef
    {
        get { return Store.DefaultFolderWithServiceRef; }
    }

    private NoteInfoDto _selectedNoteInfo;
    public NoteInfoDto SelectedNoteInfo
    {
        get { return _selectedNoteInfo; }
        protected set { _selectedNoteInfo = value; }
    }

    public string FolderPath { get; set; }

    public int? CountNotes { get; set; }

    #endregion 

    #region Constructor, Dispose, ...

    public KNoteManagmentCtrl(Store store) : base(store)
    {
        Store.ChangedActiveFolderWithServiceRef += Store_ChangedActiveFolderWithServiceRef;
        Store.ChangedActiveFilterWithServiceRef += Store_ChangedActiveFilterWithServiceRef;

        Store.AddedPostIt += PostItEditorCtrl_AddedEntity;
        Store.SavedPostIt += PostItEditorCtrl_SavedEntity;            
        Store.DeletedPostIt += PostItEditorCtrl_DeletedEntity;            
        Store.ExtendedEditPostIt += PostItEditorCtrl_ExtendedEdit;

        Store.AddedNote += NoteEditorCtrl_AddedEntity;
        Store.SavedNote += NoteEditorCtrl_SavedEntity;
        Store.DeletedNote += NoteEditorCtrl_DeletedEntity;
        Store.EditedPostItNote += NoteEditorCtrl_PostItEdit;
    }

    private async void Store_ChangedActiveFolderWithServiceRef(object sender, ControllerEventArgs<FolderWithServiceRef> e)
    {            
        await RefreshActiveFolderWithServiceRef(e.Entity);            
    }

    private async void Store_ChangedActiveFilterWithServiceRef(object sender, ControllerEventArgs<SelectedNotesInServiceRef> e)
    {
        await RefreshActiveFilterWithServiceRef(e.Entity);
    }

    private async Task RefreshActiveFolderWithServiceRef(FolderWithServiceRef folderWithServideRef)
    {        
        if (folderWithServideRef == null)               
            return;
        
        View.ActivateWaitState();

        SelectMode = EnumSelectMode.Folders;

        NotifyMessage($"Loading notes list for folder {folderWithServideRef.FolderInfo?.FolderNumber}");

        FolderPath = FoldersSelectorCtrl.Path;

        _selectedNoteInfo = null;
        NoteEditorCtrl.CleanView();
        await NotesSelectorCtrl.LoadEntities(folderWithServideRef.ServiceRef.Service, folderWithServideRef.FolderInfo);
        CountNotes = NotesSelectorCtrl.ListEntities?.Count;

        View.ShowInfo(null);
        NotifyMessage($"Loaded notes list for folder {folderWithServideRef.FolderInfo?.FolderNumber}");

        View.DeactivateWaitState();
    }

    private async Task RefreshActiveFilterWithServiceRef(SelectedNotesInServiceRef selectedNotesInServiceRef)
    {
        View.ActivateWaitState();

        SelectMode = EnumSelectMode.Filters;

        NotifyMessage($"Loading notes filter: {selectedNotesInServiceRef?.NotesSearch?.TextSearch}");
        
        FolderPath = $"Notes filter: {selectedNotesInServiceRef?.NotesSearch?.TextSearch}";

        _selectedNoteInfo = null;
        NoteEditorCtrl.View.CleanView();
        await NotesSelectorCtrl.LoadSearchEntities(selectedNotesInServiceRef?.ServiceRef?.Service, selectedNotesInServiceRef?.NotesSearch);
        CountNotes = NotesSelectorCtrl.ListEntities?.Count;

        View.ShowInfo(null);
        NotifyMessage($"Loaded notes filter {selectedNotesInServiceRef?.NotesSearch?.TextSearch}");

        View.DeactivateWaitState();
    }

    public override void Dispose()
    {
        Store.ChangedActiveFolderWithServiceRef -= Store_ChangedActiveFolderWithServiceRef;
        Store.ChangedActiveFilterWithServiceRef -= Store_ChangedActiveFilterWithServiceRef;

        Store.AddedPostIt -= PostItEditorCtrl_AddedEntity;
        Store.SavedPostIt -= PostItEditorCtrl_SavedEntity;
        Store.DeletedPostIt -= PostItEditorCtrl_DeletedEntity;
        Store.ExtendedEditPostIt -= PostItEditorCtrl_ExtendedEdit;

        Store.AddedNote -= NoteEditorCtrl_AddedEntity;
        Store.SavedNote -= NoteEditorCtrl_SavedEntity;
        Store.DeletedNote -= NoteEditorCtrl_DeletedEntity;
        Store.EditedPostItNote -= NoteEditorCtrl_PostItEdit;

        base.Dispose();
    }

    #endregion

    #region Views

    protected override IViewKNoteManagment CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    private IViewBase _notifyView;
    public IViewBase NotifyView
    {
        get
        {
            if (_notifyView == null)
            {
                _notifyView = Store.FactoryViews.NotifyView(this);
            }
            return _notifyView;
        }
    }

    private IViewBase _aboutView;
    public IViewBase AboutView
    {
        get
        {
            if (_aboutView == null)
            {
                _aboutView = Store.FactoryViews.AboutView(this);
            }
            return _aboutView;
        }
    }


    #endregion

    #region Controller override methods

    protected override Result<EControllerResult> OnInitialized()
    {
        ControllerName = $"{KntConst.AppName} managment";

        var result = base.OnInitialized();

        // TODO: pending check result correctrly

        try
        {                
            using (new WaitCursor())
            {
                NotesSelectorCtrl.Run();
                FoldersSelectorCtrl.Run();
                FilterParamCtrl.Run();
                NoteEditorCtrl.Run();
                MessagesManagmentCtrl.Run();

                NotifyView.ShowView();

                // TODO: Experimental ---------------------------------
                if (!string.IsNullOrEmpty(Store.AppConfig.ChatHubUrl))
                {
                    RunKntChatCtrl(false);
                }
                //-----------------------------------------------------
            }
        }
        catch (Exception ex)
        {
            result.AddErrorMessage(ex.Message);                
        }
        
        return result;
    }

    #endregion

    #region Controllers included

    #region FoldersSelector controller

    private FoldersSelectorCtrl _folderSelectorCtrl;
    public FoldersSelectorCtrl FoldersSelectorCtrl
    {
        get
        {
            if (_folderSelectorCtrl == null)
            {
                _folderSelectorCtrl = new FoldersSelectorCtrl(Store);
                _folderSelectorCtrl.EmbededMode = true;                    
                _folderSelectorCtrl.EntitySelection += _folderSelectorCtrl_EntitySelection;

                _folderSelectorCtrl.Extensions.Add("New folder ...", new ExtensionsEventHandler<FolderWithServiceRef>(ExtendNewFolder));
                _folderSelectorCtrl.Extensions.Add("Edit folder ...", new ExtensionsEventHandler<FolderWithServiceRef>(ExtendEditFolder));
                _folderSelectorCtrl.Extensions.Add("Delete folder ...", new ExtensionsEventHandler<FolderWithServiceRef>(ExtendDeleteFolder));

            }
            return _folderSelectorCtrl;
        }
    }

    private void _folderSelectorCtrl_EntitySelection(object sender, ControllerEventArgs<FolderWithServiceRef> e)
    {
        SelectedFolderWithServiceRef = e.Entity;            
    }

    private void ExtendNewFolder(object sender, ControllerEventArgs<FolderWithServiceRef> e)
    {
        NewFolder();
    }

    private async void ExtendEditFolder(object sender, ControllerEventArgs<FolderWithServiceRef> e)
    {
        await EditFolder();
    }

    private async void ExtendDeleteFolder(object sender, ControllerEventArgs<FolderWithServiceRef> e)
    {
        await DeleteFolder();
    }

    #endregion

    #region NotesSelector controller

    private NotesSelectorCtrl _notesSelectorCtrl;
    public NotesSelectorCtrl NotesSelectorCtrl
    {
        get
        {
            if (_notesSelectorCtrl == null)
            {
                _notesSelectorCtrl = new NotesSelectorCtrl(Store);
                _notesSelectorCtrl.EmbededMode = true;                    
                _notesSelectorCtrl.EntitySelection += _notesSelectorCtrl_EntitySelection;
                _notesSelectorCtrl.EntitySelectionDoubleClick += _notesSelectorCtrl_EntitySelectionDoubleClick;

                _notesSelectorCtrl.Extensions.Add("New note ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendAddNote));
                _notesSelectorCtrl.Extensions.Add("New note as PostIt...", new ExtensionsEventHandler<NoteInfoDto>(ExtendAddNoteAsPostIt));
                _notesSelectorCtrl.Extensions.Add("Edit note ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendEditNote));
                _notesSelectorCtrl.Extensions.Add("Edit note as PostIt ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendEditNoteAsPostIt));
                _notesSelectorCtrl.Extensions.Add("Delete note ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendDeleteNote));
                _notesSelectorCtrl.Extensions.Add("--0", new ExtensionsEventHandler<NoteInfoDto>(ExtendNull));
                _notesSelectorCtrl.Extensions.Add("Add automatic resolved task", new ExtensionsEventHandler<NoteInfoDto>(AddFastResolvedTask));
                _notesSelectorCtrl.Extensions.Add("--1", new ExtensionsEventHandler<NoteInfoDto>(ExtendNull));
                _notesSelectorCtrl.Extensions.Add("Move selected notes ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendMoveSelectedNotes));
                _notesSelectorCtrl.Extensions.Add("Add tag to selected notes ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendAddTagSelectedNotes));
                _notesSelectorCtrl.Extensions.Add("Remove tag from selected notes ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendRemoveTagSelectedNotes));
            }
            return _notesSelectorCtrl;
        }
    }

    private async void _notesSelectorCtrl_EntitySelectionDoubleClick(object sender, ControllerEventArgs<NoteInfoDto> e)
    {
        if (e.Entity == null)
            return;
        _selectedNoteInfo = e.Entity;
        await EditNote();
    }

    private async void _notesSelectorCtrl_EntitySelection(object sender, ControllerEventArgs<NoteInfoDto> e)
    {
        if (e.Entity == null || SelectedServiceRef == null)
            return;

        NotifyMessage($"Loading note details for note {e.Entity.NoteNumber}");

        _selectedNoteInfo = e.Entity;            
        await NoteEditorCtrl.LoadModelById(SelectedServiceRef.Service, _selectedNoteInfo.NoteId);

        NotifyMessage($"Loaded note details for note {e.Entity.NoteNumber}");
    }

    private async void ExtendAddNote(object sender, ControllerEventArgs<NoteInfoDto> e)
    {
        await AddNote();
    }

    private async void ExtendAddNoteAsPostIt(object sender, ControllerEventArgs<NoteInfoDto> e)
    {
        await AddNotePostIt();
    }

    private async void ExtendEditNote(object sender, ControllerEventArgs<NoteInfoDto> e)
    {
        await EditNote();
    }

    private async void AddFastResolvedTask(object sender, ControllerEventArgs<NoteInfoDto> e)
    {
        await AddFastResolvedTask();
    }
   
    private async void ExtendEditNoteAsPostIt(object sender, ControllerEventArgs<NoteInfoDto> e)
    {
        await EditNotePostIt();
    }

    private async void ExtendDeleteNote(object sender, ControllerEventArgs<NoteInfoDto> e)
    {
        await DeleteNote();
    }

    private async void ExtendMoveSelectedNotes(object sender, ControllerEventArgs<NoteInfoDto> e)
    {
        await MoveSelectedNotes();
    }

    private void ExtendNull(object sender, ControllerEventArgs<NoteInfoDto> e)
    {
        
    }

    private async void ExtendAddTagSelectedNotes(object sender, ControllerEventArgs<NoteInfoDto> e)
    {            
        await ChangeTags(EnumChangeTag.Add);
    }

    private async void ExtendRemoveTagSelectedNotes(object sender, ControllerEventArgs<NoteInfoDto> e)
    {            
        await ChangeTags(EnumChangeTag.Remove);
    }

    #endregion

    #region NoteEditor controller

    private NoteEditorCtrl _noteEditorCtrl;
    public NoteEditorCtrl NoteEditorCtrl
    {
        get
        {
            if (_noteEditorCtrl == null)
            {
                _noteEditorCtrl = new NoteEditorCtrl(Store);
                _noteEditorCtrl.EmbededMode = true;
            }
            return _noteEditorCtrl;
        }
    }

    #endregion

    #region Messages Managment controller

    private MessagesManagmentCtrl _messagesManagmentCtrl;
    public MessagesManagmentCtrl MessagesManagmentCtrl
    {
        get
        {
            if(_messagesManagmentCtrl == null)
            {
                _messagesManagmentCtrl = new MessagesManagmentCtrl(Store);
                _messagesManagmentCtrl.PostItVisible += _messagesManagment_PostItVisible;                    
                _messagesManagmentCtrl.PostItAlarm += _messagesManagment_PostItAlarm;
                //_messagesManagmentCtrl.EMailAlarm += _messagesManagment_EMailAlarm;
                //_messagesManagmentCtrl.AppAlarm += _messagesManagment_AppAlarm;    
                _messagesManagmentCtrl.ExecuteKntScript += _messagesManagmentCtrl_ExecuteKntScript;
            }
            return _messagesManagmentCtrl;
        }
    }

    private async void _messagesManagmentCtrl_ExecuteKntScript(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        var service = e.Entity.Service;
        var note = (await (service.Notes.GetAsync(e.Entity.NoteId))).Entity;
        Store.RunScriptInNewThread(note?.Script);
    }

    private void _messagesManagment_AppAlarm(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        // TODO: ... for next major version 
        throw new NotImplementedException();
    }

    private void _messagesManagment_EMailAlarm(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        // TODO: ... for next major version 
        throw new NotImplementedException();
    }

    private async void _messagesManagment_PostItAlarm(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {                        
        if (await Store.CheckPostItIsActive(e.Entity.NoteId) || await Store.CheckNoteIsActive(e.Entity.NoteId))
            return;
        await EditNotePostIt(e.Entity.Service, e.Entity.NoteId, true);
    }

    private async void _messagesManagment_PostItVisible(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        if (await Store.CheckPostItIsActive(e.Entity.NoteId))
            return;
        await EditNotePostIt(e.Entity.Service, e.Entity.NoteId);
    }

    #endregion

    #region FilterParam controller

    private FiltersSelectorCtrl _filterParamCtrl;
    public FiltersSelectorCtrl FilterParamCtrl
    {
        get
        {
            if (_filterParamCtrl == null)
            {
                _filterParamCtrl = new FiltersSelectorCtrl(Store);
                _filterParamCtrl.EmbededMode = true;
                _filterParamCtrl.EntitySelection += _filterParamCtrl_EntitySelection;                    
            }
            return _filterParamCtrl;
        }
    }

    private void _filterParamCtrl_EntitySelection(object sender, ControllerEventArgs<SelectedNotesInServiceRef> e)
    {            
        SelectedNotesInServiceRef = e.Entity;
    }

    #endregion

    #region KntChat controller 
    
    private KntChatCtrl _kntChatCtrl;
    public KntChatCtrl KntChatCtrl
    {
        get
        {
            if (_kntChatCtrl == null)
            {
                _kntChatCtrl = new KntChatCtrl(Store);
                _kntChatCtrl.ReceiveMessage += _kntChatCtrl_ReceiveMessage;
            }
            return _kntChatCtrl;
        }
        protected set { _kntChatCtrl = value; }
    }

    private void _kntChatCtrl_ReceiveMessage(object sender, ControllerEventArgs<string> e)
    {
        KntChatCtrl.VisibleView(true);
    }
    
    #endregion

    #region Heavy process controller

    private HeavyProcessCtrl _heavyProcessCtrl;
    public HeavyProcessCtrl HeavyProcessCtrl
    {
        get
        {
            if (_heavyProcessCtrl == null)
            {
                _heavyProcessCtrl = new HeavyProcessCtrl(Store);
                _heavyProcessCtrl.ReportProgress = new Progress<KNoteProgress>(ReportProgressChangeTags);
            }
            return _heavyProcessCtrl;
        }
    }

    private void ReportProgressChangeTags(KNoteProgress progress)
    {
        if (progress.HeavyProcessCtrl == null)
            return;
        progress.HeavyProcessCtrl.UpdateProgress(progress.Progress);
        if(progress.Info != null)
            progress.HeavyProcessCtrl.UpdateProcessInfo(progress.Info);
    }

    #endregion 

    #endregion

    #region Controller public methods

    public async Task<bool> FinalizeApp()
    {
        if (View.ShowInfo($"Are you sure exit {KntConst.AppName}?", KntConst.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            return await FinalizeAppForce();
        }                
        return false;
    }

    public async Task<bool> FinalizeAppForce()
    {
        var res = await Store.SaveActiveNotes();            
        Store.SaveConfig();                        
        Finalize();
        return res;
    }

    public void ShowKntScriptConsole()
    {
        var kntEngine = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(Store));

        var kntScriptCom = new KntScriptConsoleCtrl(Store);
        kntScriptCom.KntSEngine = kntEngine;

        kntScriptCom.Run();
    }

    public void ShowKntChatConsole()
    {
        // Use _kntChatCtrl field here
        if (_kntChatCtrl != null)  
            KntChatCtrl.VisibleView(true);
        else
        {
            KntChatCtrl.ShowErrorMessagesOnInitialize = true;
            RunKntChatCtrl();
        }
    }

    public void ShowKntChatGPTConsole()
    {
        var kntChatGPTCtrl = new KntChatGPTCtrl(Store);
        kntChatGPTCtrl.Run();            
        kntChatGPTCtrl.ShowChatGPTView(true, true);
    }

    public void ShowKntCOMPortServerConsole()
    {
        var kntServerCOMCtrl = new KntServerCOMCtrl(Store);
        kntServerCOMCtrl.Run();
        kntServerCOMCtrl.ShowServerCOMView(true);
    }


    public async Task EditNote()
    {
        if (loadingNote)
            return;

        lock (lockObject)
            loadingNote = true;

        if (SelectedNoteInfo == null)
        {
            lock (lockObject)
                loadingNote = false;
            View.ShowInfo("There is no note selected to edit.");
            return;
        }            
        if (await Store.CheckNoteIsActive(SelectedNoteInfo.NoteId) || await Store.CheckPostItIsActive(SelectedNoteInfo.NoteId))
        {
            lock (lockObject)
                loadingNote = false;
            View.ShowInfo("This note is already active.");
            return;
        }

        await EditNote(SelectedServiceRef.Service, SelectedNoteInfo.NoteId);

        lock (lockObject)
            loadingNote = false;
    }

    public async Task AddFastResolvedTask()
    {
        if (SelectedNoteInfo == null)
        {
            View.ShowInfo("There is no note selected to edit.");
            return;
        }
        if (await Store.CheckNoteIsActive(SelectedNoteInfo.NoteId) || await Store.CheckPostItIsActive(SelectedNoteInfo.NoteId))
        {
            View.ShowInfo("This note is already active. Add task with editor form.");
            return;
        }
        
        var userId = await Store.GetUserId(SelectedServiceRef.Service);
        if(userId == null)
        {
            View.ShowInfo("There is no recognized user to create a quick task.");
            return;
        }

        var service = SelectedServiceRef.Service;

        var task = new NoteTaskDto
        {
            NoteId = SelectedNoteInfo.NoteId,
            UserId = (Guid)userId,
            Description = SelectedNoteInfo.Topic,
            Tags = "Automatic  task",
            Resolved = true,
            EndDate = DateTime.Now
        };

        var resNoteForSaveTask = (await service.Notes.GetExtendedAsync(SelectedNoteInfo.NoteId)).Entity;
        resNoteForSaveTask.Tasks.Add(task);

        var res = (await service.Notes.SaveExtendedAsync(resNoteForSaveTask)).Entity;
        
        await OnNoteEditorSaved(res.GetSimpleDto<NoteInfoDto>());            
    }

    public async Task<bool> EditNote(IKntService service, Guid noteId)
    {
        var noteEditorCtrl = new NoteEditorCtrl(Store);
        var res = await noteEditorCtrl.LoadModelById(service, noteId, false);
        noteEditorCtrl.Run();            
        return res;
    }

    public async Task EditNotePostIt()
    {
        if (SelectedNoteInfo == null)
        {
            View.ShowInfo("There is no note selected to edit.");
            return;
        }
        if (await Store.CheckNoteIsActive(SelectedNoteInfo.NoteId) || await Store.CheckPostItIsActive(SelectedNoteInfo.NoteId))
        {
            View.ShowInfo("This note is already active.");
            return;
        }
        await EditNotePostIt(SelectedServiceRef.Service, SelectedNoteInfo.NoteId);
    }
    
    public async Task<bool> EditNotePostIt(IKntService service, Guid noteId, bool alwaysTop = false)
    {            
        var postItEditorCtrl = new PostItEditorCtrl(Store);
        var res = await postItEditorCtrl.LoadModelById(service, noteId, false);
        if(alwaysTop)
            postItEditorCtrl.ForceAlwaysTop = true;
        postItEditorCtrl.Run();
        return res;
    }

    public async Task AddNote()
    {
        if(SelectedFolderInfo == null)
        {
            View.ShowInfo("There is no selected folder to create new note.");
            return;
        }
        await AddNote(SelectedServiceRef.Service);
    }

    public async Task AddNote(IKntService service)
    {
        var noteEditorCtrl = new NoteEditorCtrl(Store);
        await noteEditorCtrl.NewModel(service);
        noteEditorCtrl.Run();
    }

    public async Task AddNotePostIt()
    {
        if (SelectedFolderInfo == null)
        {
            View.ShowInfo("There is no selected folder to create new note.");
            return;
        }

        await AddNotePostIt(SelectedFolderWithServiceRef);
    }

    public async Task AddDefaultNotePostIt()
    {
        if (Store.DefaultFolderWithServiceRef == null)
        {
            View.ShowInfo("A default folder has not been defined for personal notes.");
            return;
        }

        await AddNotePostIt(DefaultFolderWithServiceRef);
    }
    
    public async Task DeleteNote()
    {
        if (SelectedNoteInfo == null)
        {
            View.ShowInfo("There is no note selected to delete.");
            return;
        }

        var noteEditorCtrl = new NoteEditorCtrl(Store);            
        await noteEditorCtrl.DeleteModel(SelectedServiceRef.Service, SelectedNoteInfo.NoteId);            
    }

    public void NewFolder()
    {
        var folderEditorCtrl = new FolderEditorCtrl(Store);
        folderEditorCtrl.NewModel(SelectedServiceRef.Service);
        folderEditorCtrl.Model.ParentId = SelectedFolderInfo?.FolderId;
        folderEditorCtrl.Model.ParentFolderDto = SelectedFolderInfo?.GetSimpleDto<FolderDto>();
        var res = folderEditorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {                
            var fs = new FolderWithServiceRef { ServiceRef = SelectedServiceRef, FolderInfo = folderEditorCtrl.Model.GetSimpleDto<FolderInfoDto>() };
            FoldersSelectorCtrl.AddItem(fs);
        }            
    }

    public async Task EditFolder()
    {
        if(SelectedFolderInfo == null)
        {
            View.ShowInfo("There is no folder selected to edit.");
            return;
        }

        var folderEditorCtrl = new FolderEditorCtrl(Store);
        await folderEditorCtrl.LoadModelById(SelectedServiceRef.Service, SelectedFolderInfo.FolderId, false);
        FoldersSelectorCtrl.OldParent = folderEditorCtrl.Model.ParentId;
        var res = folderEditorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {                
            SelectedFolderWithServiceRef.FolderInfo = folderEditorCtrl.Model.GetSimpleDto<FolderInfoDto>();
            FoldersSelectorCtrl.RefreshItem(SelectedFolderWithServiceRef);
        }            
    }

    public async Task DeleteFolder()
    {
        if (SelectedFolderInfo == null)
        {
            View.ShowInfo("There is no folder selected to delete.");
            return;
        }

        var folderEditorCtrl = new FolderEditorCtrl(Store);
        var res = await folderEditorCtrl.DeleteModel(SelectedServiceRef.Service, SelectedFolderInfo.FolderId);
        if (res)
        {                
            FoldersSelectorCtrl.DeleteItem(SelectedFolderWithServiceRef);
        }            
    }

    public async Task RemoveRepositoryLink()
    {
        if (SelectedServiceRef == null)
        {
            View.ShowInfo("There is no repository selected to remove.");
            return;
        }            
        var repositoryEditorCtrl = new RepositoryEditorCtrl(Store);            
        var res = await repositoryEditorCtrl.DeleteModel(SelectedServiceRef.Service, SelectedServiceRef.IdServiceRef);
        if (res)
        {                
            RefreshRepositoryAndFolderTree();
        }
    }

    public async Task AddRepositoryLink()
    {
        await NewRepository(EnumRepositoryEditorMode.AddLink);
    }

    public async Task CreateRepository()
    {
        await NewRepository(EnumRepositoryEditorMode.Create);
    }

    public async Task ManagmentRepository()
    {
        if (SelectedServiceRef == null)
        {
            View.ShowInfo("There is no repository selected to configure.");
            return;
        }                        
        var repositoryEditorCtrl = new RepositoryEditorCtrl(Store);
        repositoryEditorCtrl.EditorMode = EnumRepositoryEditorMode.Managment;
        await repositoryEditorCtrl.LoadModelById(SelectedServiceRef.Service, SelectedServiceRef.IdServiceRef, false);
        var res = repositoryEditorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            // Do action 
            RefreshRepositoryAndFolderTree();
        }
    }

    public void RefreshRepositoryAndFolderTree()
    {
        View.ActivateWaitState();
        NotifyMessage("Refreshing tree folder ...");
        SelectedNotesInServiceRef = null;
        SelectedFolderWithServiceRef = null;
        SelectedNoteInfo = null;
        FolderPath = "";
        CountNotes = 0;
        FoldersSelectorCtrl.ServicesRef = null;  // force get repostiroy list form store
        FoldersSelectorCtrl.Refresh();
        NoteEditorCtrl.CleanView();
        NotesSelectorCtrl.CleanView();
        View.ShowInfo(null);
        NotifyMessage("Refreshed tree folder ...");
        View.DeactivateWaitState();
    }

    public void ShowKNoteManagment() 
    {
        View.ActivateView();            
    }

    public void HideKNoteManagment()
    {                        
        View.HideView();            
    }

    public async Task GoActiveFolder()
    {        
        await RefreshActiveFolderWithServiceRef(SelectedFolderWithServiceRef);            
    }

    public async Task GoActiveFilter()
    {            
        await RefreshActiveFilterWithServiceRef(SelectedNotesInServiceRef);
    }

    public async Task MoveSelectedNotes()
    {                
        var selectedNotes = NotesSelectorCtrl.GetSelectedListNotesInfo().ToList();
        if(selectedNotes == null || selectedNotes?.Count == 0)
        {                
            View.ShowInfo("You have not selected notes .");
            return;
        }

        var folderSelector = new FoldersSelectorCtrl(Store);
        var services = new List<ServiceRef>();
        services.Add(SelectedServiceRef);
        folderSelector.ServicesRef = services;
        var res = folderSelector.RunModal();
        if (res.Entity != EControllerResult.Executed)
            return;

        try
        {
            var folderId = folderSelector.SelectedEntity.FolderInfo.FolderId;
            var folderName = folderSelector.SelectedEntity.FolderInfo.Name;

            // --- Heavy process singleton model
            HeavyProcessCtrl.UpdateProcessName($"Moving notes to folder '{folderName}'.");
            await HeavyProcessCtrl.Exec2(MoveSelectedNotesAction, selectedNotes, folderId);            
        }
        catch (TaskCanceledException)
        {
            //View.ShowInfo("The operation has been canceled.");
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            await Task.CompletedTask;
            await ForceRefreshListNotes();
        }
    }

    public async Task MoveSelectedNotesAction(List<NoteInfoDto> selectedNotes, Guid newFolderId, CancellationTokenSource cancellationToken = null, IProgress<KNoteProgress> progress = null, HeavyProcessCtrl heavyProcessCtrl = null)
    {
        var index = 0;
        var service = new ServiceRef(SelectedServiceRef.RepositoryRef, SelectedServiceRef.UserIdentityName).Service;
        foreach (var n in selectedNotes)
        {
            await service.Notes.UtilPatchFolder(n.NoteId, newFolderId);

            index++;
            var percentage = (double)index / selectedNotes.Count;
            percentage = percentage * 100;
            var percentageInt = (int)Math.Round(percentage, 0);

            await Task.Delay(1); // This delay is necessary
            if (progress != null)                
                progress.Report(new KNoteProgress { Progress = percentageInt, Info = $"Updating Note #{n.NoteNumber}", HeavyProcessCtrl = heavyProcessCtrl });

            if (cancellationToken != null && cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
        }
    }

    public async Task ChangeTags(EnumChangeTag action)
    {
        string labelInput;        

        var selectedNotes = NotesSelectorCtrl.GetSelectedListNotesInfo().ToList();
        if (selectedNotes == null || selectedNotes?.Count == 0)
        {
            if (action == EnumChangeTag.Add)
                View.ShowInfo("You have not selected notes for add tags.");
            else
                View.ShowInfo("You have not selected notes for remove tags.");
            return;
        }

        if (action == EnumChangeTag.Add)
            labelInput = "New tag:";
        else
            labelInput = "Tag for remove:";

        var listVars = new List<ReadVarItem> {new ReadVarItem
        {
            Label = labelInput,
            VarIdent = "Tag",
            VarValue = "",
            VarNewValueText = ""
        }};

        var formReadVar = new ReadVarForm(listVars);
        if (action == EnumChangeTag.Add)
            formReadVar.Text = "New tags for selected notes";
        else
            formReadVar.Text = "Remove tags in selected notes";
        formReadVar.Size = new Size(500, 150);

        var result = formReadVar.ShowDialog();

        if (result == DialogResult.Cancel)
            return;
        else
        {
            try
            {
                var tag = listVars[0].VarNewValueText;

                // --- Heavy process instance model
                using var heavyProcessCtrl = new HeavyProcessCtrl(Store);
                heavyProcessCtrl.ReportProgress = new Progress<KNoteProgress>(ReportProgressChangeTags);
                heavyProcessCtrl.UpdateProcessName($"Updating tags. {labelInput} {tag} .");
                await heavyProcessCtrl.Exec3(ChangeTagsAction, action, selectedNotes, tag);
            }
            catch (TaskCanceledException)
            {
                //View.ShowInfo("The operation has been canceled.");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await Task.CompletedTask;
                await ForceRefreshListNotes();
            }
        }
    }

    public async Task ChangeTagsAction(EnumChangeTag action, List<NoteInfoDto> selectedNotes, string tag, CancellationTokenSource cancellationToken = null, IProgress<KNoteProgress> progress = null, HeavyProcessCtrl heavyProcessCtrl = null)
    {
        var index = 0;
        var service = new ServiceRef(SelectedServiceRef.RepositoryRef, SelectedServiceRef.UserIdentityName).Service;

        foreach (var note in selectedNotes)
        {
            if (action == EnumChangeTag.Add)
                await service.Notes.UtilPatchChangeTags(note.NoteId, "", tag);
            else
                await service.Notes.UtilPatchChangeTags(note.NoteId, tag, "");

            index++;
            var percentage = (double)index / selectedNotes.Count;
            percentage = percentage * 100;
            var percentageInt = (int)Math.Round(percentage, 0);
            await Task.Delay(1); // This delay is necessary
            if (progress != null)
                progress.Report(new KNoteProgress { Progress = percentageInt, Info = $"Updating Note #{note.NoteNumber}", HeavyProcessCtrl = heavyProcessCtrl });

            if (cancellationToken != null && cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
        }
    }

    public void RunScriptSelectedNotes()
    {
        var selectedNotes = NotesSelectorCtrl.GetSelectedListNotesInfo();

        if (selectedNotes == null || selectedNotes?.Count == 0)
        {
            View.ShowInfo("You have not selected notes for run scripts .");
            return;
        }

        foreach (var note in selectedNotes)
        {
            if (!string.IsNullOrEmpty(note.Script))
                Store.RunScript(note.Script);
        }
    }

    public void Options()
    {
        var optionsEditorCtrl = new OptionsEditorCtrl(Store);
        
        optionsEditorCtrl.LoadModel(
            SelectedServiceRef?.Service,
            Store.AppConfig.GetSimpleDto<AppConfig>(), 
            true);
        var res = optionsEditorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            // TODO: refresh context managment
            
        }
    }
    
    public void About()
    {
        AboutView.ShowModalView();
    }

    public void Help()
    {
        string url = KntConst.HelpUrl;

        try
        {
            Process.Start(url);
        }
        catch
        {                
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    public void Lab()
    {
        var kntLabCtrl = new KntLabCtrl(Store);
        kntLabCtrl.Run();
        kntLabCtrl.ShowLabView();
    }

    #endregion

    #region Events handlers for extension controller 

    private async void PostItEditorCtrl_AddedEntity(object sender, ControllerEventArgs<NoteDto> e)
    {
        await OnNoteEditorAdded(e.Entity.GetSimpleDto<NoteInfoDto>());
    }

    private async void NoteEditorCtrl_AddedEntity(object sender, ControllerEventArgs<NoteExtendedDto> e)
    {
        await OnNoteEditorAdded(e.Entity.GetSimpleDto<NoteInfoDto>());
    }

    private async Task OnNoteEditorAdded(NoteInfoDto noteInfo)
    {
        if (NotesSelectorCtrl.ListEntities == null)
            return;

        if (NotesSelectorCtrl.ListEntities.Count == 0)
        {
            await NoteEditorCtrl.LoadModelById(SelectedServiceRef.Service, noteInfo.NoteId);
            _selectedNoteInfo = noteInfo;
        }
        NotesSelectorCtrl.AddItem(noteInfo);
    }

    private async void PostItEditorCtrl_SavedEntity(object sender, ControllerEventArgs<NoteDto> e)
    {
        await OnNoteEditorSaved(e.Entity.GetSimpleDto<NoteInfoDto>());
    }

    private async void NoteEditorCtrl_SavedEntity(object sender, ControllerEventArgs<NoteExtendedDto> e)
    {
        await OnNoteEditorSaved(e.Entity.GetSimpleDto<NoteInfoDto>()); 
    }

    private async void PostItEditorCtrl_ExtendedEdit(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        await EditNote(e.Entity.Service, e.Entity.NoteId);
    }

    private async void NoteEditorCtrl_PostItEdit(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        await EditNotePostIt(e.Entity.Service, e.Entity.NoteId);
    }

    private async Task OnNoteEditorSaved(NoteInfoDto noteInfo)
    {
        if (NoteEditorCtrl.Model.NoteId == noteInfo.NoteId)
            await NoteEditorCtrl.LoadModelById(SelectedServiceRef.Service, noteInfo.NoteId);

        NotesSelectorCtrl.RefreshItem(noteInfo);

        if (NotesSelectorCtrl.ListEntities?.Count == 0)
        {
            NoteEditorCtrl.View.CleanView();
            _selectedNoteInfo = null;
        }                            
    }

    private void PostItEditorCtrl_DeletedEntity(object sender, ControllerEventArgs<NoteDto> e)
    {
        OnNoteEditorDeleted(e.Entity.GetSimpleDto<NoteInfoDto>());
    }

    private void NoteEditorCtrl_DeletedEntity(object sender, ControllerEventArgs<NoteExtendedDto> e)
    {
        OnNoteEditorDeleted(e.Entity.GetSimpleDto<NoteInfoDto>());
    }

    private void OnNoteEditorDeleted(NoteInfoDto noteInfo)
    {            
        NotesSelectorCtrl.DeleteItem(noteInfo);
        if (NotesSelectorCtrl.ListEntities?.Count == 0)
        {
            NoteEditorCtrl.View.CleanView();
            _selectedNoteInfo = null;
        }
    }

    #endregion

    #region Private methods

    private async Task ForceRefreshListNotes()
    {
        if (SelectMode == EnumSelectMode.Folders)
            await RefreshActiveFolderWithServiceRef(SelectedFolderWithServiceRef);
        else if (SelectMode == EnumSelectMode.Filters)                
            await RefreshActiveFilterWithServiceRef(SelectedNotesInServiceRef);
    }

    private async Task NewRepository(EnumRepositoryEditorMode mode)
    {
        var repositoryEditorCtrl = new RepositoryEditorCtrl(Store);
        repositoryEditorCtrl.EditorMode = mode;
        await repositoryEditorCtrl.NewModel();
        var res = repositoryEditorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            RefreshRepositoryAndFolderTree();
        }
    }
    
    private async Task AddNotePostIt(FolderWithServiceRef folderWithServiceRef)
    {
        var postItEditorCtrl = new PostItEditorCtrl(Store);
        postItEditorCtrl.FolderWithServiceRef = folderWithServiceRef;
        await postItEditorCtrl.NewModel(folderWithServiceRef.ServiceRef.Service);
        postItEditorCtrl.Run();
    }

    private void RunKntChatCtrl(bool visibleView = true)
    {
        var chatCanRun = KntChatCtrl.Run();
        if (chatCanRun.Entity == EControllerResult.Executed)
        {
            KntChatCtrl.ShowChatView(false);
            KntChatCtrl.VisibleView(visibleView);
        }
        else
        {
            KntChatCtrl.Finalize();
            KntChatCtrl = null;
        }
    }

    #endregion
}

#region Public enums 

public enum EnumSelectMode
{
    Folders,
    Filters
}

public enum EnumChangeTag
{
    Add,
    Remove
}

#endregion 
