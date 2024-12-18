﻿using System.Diagnostics;
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
    #region Properties

    public FolderWithServiceRef SelectedFolderWithServiceRef 
    {
        get { return Store.ActiveFolderWithServiceRef; }
        set 
        {                
            Store.ChangeActiveFolderWithServiceRef(value);
        } 
    }

    public NotesFilterWithServiceRef SelectedFilterWithServiceRef
    {
        get { return Store.ActiveFilterWithServiceRef; }
        set 
        {                
            Store.ChangeActiveFilterWithServiceRef(value);
        }
    }

    public FolderInfoDto SelectedFolderInfo
    {
        get { return SelectedFolderWithServiceRef?.FolderInfo; }
    }

    public NotesFilterDto SelectedNotesFilter
    {
        get { return SelectedFilterWithServiceRef?.NotesFilter; }
    }

    public ServiceRef SelectedServiceRef
    {
        get {
            if(SelectMode == EnumSelectMode.Filters)
                return SelectedFilterWithServiceRef?.ServiceRef;
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

        Store.AddedPostIt += PostItEditorComponent_AddedEntity;
        Store.SavedPostIt += PostItEditorComponent_SavedEntity;            
        Store.DeletedPostIt += PostItEditorComponent_DeletedEntity;            
        Store.ExtendedEditPostIt += PostItEditorComponent_ExtendedEdit;

        Store.AddedNote += NoteEditorComponent_AddedEntity;
        Store.SavedNote += NoteEditorComponent_SavedEntity;
        Store.DeletedNote += NoteEditorComponent_DeletedEntity;
        Store.EditedPostItNote += NoteEditorComponent_PostItEdit;
    }

    private async void Store_ChangedActiveFolderWithServiceRef(object sender, ComponentEventArgs<FolderWithServiceRef> e)
    {            
        await RefreshActiveFolderWithServiceRef(e.Entity);            
    }

    private async void Store_ChangedActiveFilterWithServiceRef(object sender, ComponentEventArgs<NotesFilterWithServiceRef> e)
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

        FolderPath = FoldersSelectorComponent.Path;

        _selectedNoteInfo = null;
        NoteEditorComponent.CleanView();
        await NotesSelectorComponent.LoadEntities(folderWithServideRef.ServiceRef.Service, folderWithServideRef.FolderInfo);
        CountNotes = NotesSelectorComponent.ListEntities?.Count;

        View.ShowInfo(null);
        NotifyMessage($"Loaded notes list for folder {folderWithServideRef.FolderInfo?.FolderNumber}");

        View.DeactivateWaitState();
    }

    private async Task RefreshActiveFilterWithServiceRef(NotesFilterWithServiceRef notesFilterWithServiceRef)
    {
        View.ActivateWaitState();

        SelectMode = EnumSelectMode.Filters;

        NotifyMessage($"Loading notes filter: {notesFilterWithServiceRef?.NotesFilter?.TextSearch}");
        
        FolderPath = $"Notes filter: {notesFilterWithServiceRef?.NotesFilter?.TextSearch}";

        _selectedNoteInfo = null;
        NoteEditorComponent.View.CleanView();
        await NotesSelectorComponent.LoadFilteredEntities(notesFilterWithServiceRef?.ServiceRef?.Service, notesFilterWithServiceRef?.NotesFilter);
        CountNotes = NotesSelectorComponent.ListEntities?.Count;

        View.ShowInfo(null);
        NotifyMessage($"Loaded notes filter {notesFilterWithServiceRef?.NotesFilter?.TextSearch}");

        View.DeactivateWaitState();
    }

    public override void Dispose()
    {
        Store.ChangedActiveFolderWithServiceRef -= Store_ChangedActiveFolderWithServiceRef;
        Store.ChangedActiveFilterWithServiceRef -= Store_ChangedActiveFilterWithServiceRef;

        Store.AddedPostIt -= PostItEditorComponent_AddedEntity;
        Store.SavedPostIt -= PostItEditorComponent_SavedEntity;
        Store.DeletedPostIt -= PostItEditorComponent_DeletedEntity;
        Store.ExtendedEditPostIt -= PostItEditorComponent_ExtendedEdit;

        Store.AddedNote -= NoteEditorComponent_AddedEntity;
        Store.SavedNote -= NoteEditorComponent_SavedEntity;
        Store.DeletedNote -= NoteEditorComponent_DeletedEntity;
        Store.EditedPostItNote -= NoteEditorComponent_PostItEdit;

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

    #region Component override methods

    protected override Result<EComponentResult> OnInitialized()
    {
        ComponentName = $"{KntConst.AppName} managment";

        var result = base.OnInitialized();

        // TODO: pending check result correctrly

        try
        {                
            using (new WaitCursor())
            {
                NotesSelectorComponent.Run();
                FoldersSelectorComponent.Run();
                FilterParamComponent.Run();
                NoteEditorComponent.Run();
                MessagesManagmentComponent.Run();

                NotifyView.ShowView();

                // TODO: Experimental ---------------------------------
                if (!string.IsNullOrEmpty(Store.AppConfig.ChatHubUrl))
                {
                    RunKntChatComponent(false);
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

    #region Components included

    #region FoldersSelector component

    private FoldersSelectorCtrl _folderSelectorComponent;
    public FoldersSelectorCtrl FoldersSelectorComponent
    {
        get
        {
            if (_folderSelectorComponent == null)
            {
                _folderSelectorComponent = new FoldersSelectorCtrl(Store);
                _folderSelectorComponent.EmbededMode = true;                    
                _folderSelectorComponent.EntitySelection += _folderSelectorComponent_EntitySelection;

                _folderSelectorComponent.Extensions.Add("New folder ...", new ExtensionsEventHandler<FolderWithServiceRef>(ExtendNewFolder));
                _folderSelectorComponent.Extensions.Add("Edit folder ...", new ExtensionsEventHandler<FolderWithServiceRef>(ExtendEditFolder));
                _folderSelectorComponent.Extensions.Add("Delete folder ...", new ExtensionsEventHandler<FolderWithServiceRef>(ExtendDeleteFolder));

            }
            return _folderSelectorComponent;
        }
    }

    private void _folderSelectorComponent_EntitySelection(object sender, ComponentEventArgs<FolderWithServiceRef> e)
    {
        SelectedFolderWithServiceRef = e.Entity;            
    }

    private void ExtendNewFolder(object sender, ComponentEventArgs<FolderWithServiceRef> e)
    {
        NewFolder();
    }

    private async void ExtendEditFolder(object sender, ComponentEventArgs<FolderWithServiceRef> e)
    {
        await EditFolder();
    }

    private async void ExtendDeleteFolder(object sender, ComponentEventArgs<FolderWithServiceRef> e)
    {
        await DeleteFolder();
    }

    #endregion

    #region NotesSelector component

    private NotesSelectorCtrl _notesSelectorComponent;
    public NotesSelectorCtrl NotesSelectorComponent
    {
        get
        {
            if (_notesSelectorComponent == null)
            {
                _notesSelectorComponent = new NotesSelectorCtrl(Store);
                _notesSelectorComponent.EmbededMode = true;                    
                _notesSelectorComponent.EntitySelection += _notesSelectorComponent_EntitySelection;
                _notesSelectorComponent.EntitySelectionDoubleClick += _notesSelectorComponent_EntitySelectionDoubleClick;

                _notesSelectorComponent.Extensions.Add("New note ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendAddNote));
                _notesSelectorComponent.Extensions.Add("New note as PostIt...", new ExtensionsEventHandler<NoteInfoDto>(ExtendAddNoteAsPostIt));
                _notesSelectorComponent.Extensions.Add("Edit note ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendEditNote));
                _notesSelectorComponent.Extensions.Add("Edit note as PostIt ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendEditNoteAsPostIt));
                _notesSelectorComponent.Extensions.Add("Delete note ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendDeleteNote));
                _notesSelectorComponent.Extensions.Add("--0", new ExtensionsEventHandler<NoteInfoDto>(ExtendNull));
                _notesSelectorComponent.Extensions.Add("Add automatic resolved task", new ExtensionsEventHandler<NoteInfoDto>(AddFastResolvedTask));
                _notesSelectorComponent.Extensions.Add("--1", new ExtensionsEventHandler<NoteInfoDto>(ExtendNull));
                _notesSelectorComponent.Extensions.Add("Move selected notes ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendMoveSelectedNotes));
                _notesSelectorComponent.Extensions.Add("Add tag to selected notes ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendAddTagSelectedNotes));
                _notesSelectorComponent.Extensions.Add("Remove tag from selected notes ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendRemoveTagSelectedNotes));
            }
            return _notesSelectorComponent;
        }
    }

    private async void _notesSelectorComponent_EntitySelectionDoubleClick(object sender, ComponentEventArgs<NoteInfoDto> e)
    {
        if (e.Entity == null)
            return;
        _selectedNoteInfo = e.Entity;
        await EditNote();
    }

    private async void _notesSelectorComponent_EntitySelection(object sender, ComponentEventArgs<NoteInfoDto> e)
    {
        if (e.Entity == null || SelectedServiceRef == null)
            return;

        NotifyMessage($"Loading note details for note {e.Entity.NoteNumber}");

        _selectedNoteInfo = e.Entity;            
        await NoteEditorComponent.LoadModelById(SelectedServiceRef.Service, _selectedNoteInfo.NoteId);

        NotifyMessage($"Loaded note details for note {e.Entity.NoteNumber}");
    }

    private async void ExtendAddNote(object sender, ComponentEventArgs<NoteInfoDto> e)
    {
        await AddNote();
    }

    private async void ExtendAddNoteAsPostIt(object sender, ComponentEventArgs<NoteInfoDto> e)
    {
        await AddNotePostIt();
    }

    private async void ExtendEditNote(object sender, ComponentEventArgs<NoteInfoDto> e)
    {
        await EditNote();
    }

    private async void AddFastResolvedTask(object sender, ComponentEventArgs<NoteInfoDto> e)
    {
        await AddFastResolvedTask();
    }
   
    private async void ExtendEditNoteAsPostIt(object sender, ComponentEventArgs<NoteInfoDto> e)
    {
        await EditNotePostIt();
    }

    private async void ExtendDeleteNote(object sender, ComponentEventArgs<NoteInfoDto> e)
    {
        await DeleteNote();
    }

    private async void ExtendMoveSelectedNotes(object sender, ComponentEventArgs<NoteInfoDto> e)
    {
        await MoveSelectedNotes();
    }

    private void ExtendNull(object sender, ComponentEventArgs<NoteInfoDto> e)
    {
        
    }

    private async void ExtendAddTagSelectedNotes(object sender, ComponentEventArgs<NoteInfoDto> e)
    {            
        await ChangeTags(EnumChangeTag.Add);
    }

    private async void ExtendRemoveTagSelectedNotes(object sender, ComponentEventArgs<NoteInfoDto> e)
    {            
        await ChangeTags(EnumChangeTag.Remove);
    }

    #endregion

    #region NoteEditor component

    private NoteEditorCtrl _noteEditorComponent;
    public NoteEditorCtrl NoteEditorComponent
    {
        get
        {
            if (_noteEditorComponent == null)
            {
                _noteEditorComponent = new NoteEditorCtrl(Store);
                _noteEditorComponent.EmbededMode = true;
            }
            return _noteEditorComponent;
        }
    }

    #endregion

    #region Messages Managment component

    private MessagesManagmentCtrl _messagesManagmentComponent;
    public MessagesManagmentCtrl MessagesManagmentComponent
    {
        get
        {
            if(_messagesManagmentComponent == null)
            {
                _messagesManagmentComponent = new MessagesManagmentCtrl(Store);
                _messagesManagmentComponent.PostItVisible += _messagesManagment_PostItVisible;                    
                _messagesManagmentComponent.PostItAlarm += _messagesManagment_PostItAlarm;
                //_messagesManagmentComponent.EMailAlarm += _messagesManagment_EMailAlarm;
                //_messagesManagmentComponent.AppAlarm += _messagesManagment_AppAlarm;    
                _messagesManagmentComponent.ExecuteKntScript += _messagesManagmentComponent_ExecuteKntScript;
            }
            return _messagesManagmentComponent;
        }
    }

    private async void _messagesManagmentComponent_ExecuteKntScript(object sender, ComponentEventArgs<ServiceWithNoteId> e)
    {
        var service = e.Entity.Service;
        var note = (await (service.Notes.GetAsync(e.Entity.NoteId))).Entity;
        Store.RunScriptInNewThread(note?.Script);
    }

    private void _messagesManagment_AppAlarm(object sender, ComponentEventArgs<ServiceWithNoteId> e)
    {
        // TODO: ... for next major version 
        throw new NotImplementedException();
    }

    private void _messagesManagment_EMailAlarm(object sender, ComponentEventArgs<ServiceWithNoteId> e)
    {
        // TODO: ... for next major version 
        throw new NotImplementedException();
    }

    private async void _messagesManagment_PostItAlarm(object sender, ComponentEventArgs<ServiceWithNoteId> e)
    {                        
        if (await Store.CheckPostItIsActive(e.Entity.NoteId) || await Store.CheckNoteIsActive(e.Entity.NoteId))
            return;
        await EditNotePostIt(e.Entity.Service, e.Entity.NoteId, true);
    }

    private async void _messagesManagment_PostItVisible(object sender, ComponentEventArgs<ServiceWithNoteId> e)
    {
        if (await Store.CheckPostItIsActive(e.Entity.NoteId))
            return;
        await EditNotePostIt(e.Entity.Service, e.Entity.NoteId);
    }

    #endregion

    #region FilterParam component

    private FiltersSelectorCtrl _filterParamComponent;
    public FiltersSelectorCtrl FilterParamComponent
    {
        get
        {
            if (_filterParamComponent == null)
            {
                _filterParamComponent = new FiltersSelectorCtrl(Store);
                _filterParamComponent.EmbededMode = true;
                _filterParamComponent.EntitySelection += _filterParamComponent_EntitySelection;                    
            }
            return _filterParamComponent;
        }
    }

    private void _filterParamComponent_EntitySelection(object sender, ComponentEventArgs<NotesFilterWithServiceRef> e)
    {            
        SelectedFilterWithServiceRef = e.Entity;
    }

    #endregion

    #region KntChat component 
    
    private KntChatCtrl _kntChatComponent;
    public KntChatCtrl KntChatComponent
    {
        get
        {
            if (_kntChatComponent == null)
            {
                _kntChatComponent = new KntChatCtrl(Store);
                _kntChatComponent.ReceiveMessage += _kntChatComponent_ReceiveMessage;
            }
            return _kntChatComponent;
        }
        protected set { _kntChatComponent = value; }
    }

    private void _kntChatComponent_ReceiveMessage(object sender, ComponentEventArgs<string> e)
    {
        KntChatComponent.VisibleView(true);
    }
    
    #endregion

    #region Heavy process component

    private HeavyProcessCtrl _heavyProcessComponent;
    public HeavyProcessCtrl HeavyProcessComponent
    {
        get
        {
            if (_heavyProcessComponent == null)
            {
                _heavyProcessComponent = new HeavyProcessCtrl(Store);
                _heavyProcessComponent.ReportProgress = new Progress<KNoteProgress>(ReportProgressChangeTags);
            }
            return _heavyProcessComponent;
        }
    }

    private void ReportProgressChangeTags(KNoteProgress progress)
    {
        if (progress.HeavyProcessComponent == null)
            return;
        progress.HeavyProcessComponent.UpdateProgress(progress.Progress);
        if(progress.Info != null)
            progress.HeavyProcessComponent.UpdateProcessInfo(progress.Info);
    }

    #endregion 


    #endregion

    #region Component public methods

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
        // Use _kntChatComponent field here
        if (_kntChatComponent != null)  
            KntChatComponent.VisibleView(true);
        else
        {
            KntChatComponent.ShowErrorMessagesOnInitialize = true;
            RunKntChatComponent();
        }
    }

    public void ShowKntChatGPTConsole()
    {
        var kntChatGPTComponent = new KntChatGPTCtrl(Store);
        kntChatGPTComponent.Run();            
        kntChatGPTComponent.ShowChatGPTView(true, true);
    }

    public void ShowKntCOMPortServerConsole()
    {
        var kntServerCOMComponent = new KntServerCOMCtrl(Store);
        kntServerCOMComponent.Run();
        kntServerCOMComponent.ShowServerCOMView(true);
    }


    public async Task EditNote()
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

        await EditNote(SelectedServiceRef.Service, SelectedNoteInfo.NoteId);
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
        var noteEditorComponent = new NoteEditorCtrl(Store);
        var res = await noteEditorComponent.LoadModelById(service, noteId, false);
        noteEditorComponent.Run();            
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
        var postItEditorComponent = new PostItEditorCtrl(Store);
        var res = await postItEditorComponent.LoadModelById(service, noteId, false);
        if(alwaysTop)
            postItEditorComponent.ForceAlwaysTop = true;
        postItEditorComponent.Run();
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
        var noteEditorComponent = new NoteEditorCtrl(Store);
        await noteEditorComponent.NewModel(service);
        noteEditorComponent.Run();
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

        var noteEditorComponent = new NoteEditorCtrl(Store);            
        await noteEditorComponent.DeleteModel(SelectedServiceRef.Service, SelectedNoteInfo.NoteId);            
    }

    public void NewFolder()
    {
        var folderEditorComponent = new FolderEditorCtrl(Store);
        folderEditorComponent.NewModel(SelectedServiceRef.Service);
        folderEditorComponent.Model.ParentId = SelectedFolderInfo?.FolderId;
        folderEditorComponent.Model.ParentFolderDto = SelectedFolderInfo?.GetSimpleDto<FolderDto>();
        var res = folderEditorComponent.RunModal();
        if (res.Entity == EComponentResult.Executed)
        {                
            var fs = new FolderWithServiceRef { ServiceRef = SelectedServiceRef, FolderInfo = folderEditorComponent.Model.GetSimpleDto<FolderInfoDto>() };
            FoldersSelectorComponent.AddItem(fs);
        }            
    }

    public async Task EditFolder()
    {
        if(SelectedFolderInfo == null)
        {
            View.ShowInfo("There is no folder selected to edit.");
            return;
        }

        var folderEditorComponent = new FolderEditorCtrl(Store);
        await folderEditorComponent.LoadModelById(SelectedServiceRef.Service, SelectedFolderInfo.FolderId, false);
        FoldersSelectorComponent.OldParent = folderEditorComponent.Model.ParentId;
        var res = folderEditorComponent.RunModal();
        if (res.Entity == EComponentResult.Executed)
        {                
            SelectedFolderWithServiceRef.FolderInfo = folderEditorComponent.Model.GetSimpleDto<FolderInfoDto>();
            FoldersSelectorComponent.RefreshItem(SelectedFolderWithServiceRef);
        }            
    }

    public async Task DeleteFolder()
    {
        if (SelectedFolderInfo == null)
        {
            View.ShowInfo("There is no folder selected to delete.");
            return;
        }

        var folderEditorComponent = new FolderEditorCtrl(Store);
        var res = await folderEditorComponent.DeleteModel(SelectedServiceRef.Service, SelectedFolderInfo.FolderId);
        if (res)
        {                
            FoldersSelectorComponent.DeleteItem(SelectedFolderWithServiceRef);
        }            
    }

    public async Task RemoveRepositoryLink()
    {
        if (SelectedServiceRef == null)
        {
            View.ShowInfo("There is no repository selected to remove.");
            return;
        }            
        var repositoryEditorComponent = new RepositoryEditorCtrl(Store);            
        var res = await repositoryEditorComponent.DeleteModel(SelectedServiceRef.Service, SelectedServiceRef.IdServiceRef);
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
        var repositoryEditorComponent = new RepositoryEditorCtrl(Store);
        repositoryEditorComponent.EditorMode = EnumRepositoryEditorMode.Managment;
        await repositoryEditorComponent.LoadModelById(SelectedServiceRef.Service, SelectedServiceRef.IdServiceRef, false);
        var res = repositoryEditorComponent.RunModal();
        if (res.Entity == EComponentResult.Executed)
        {
            // Do action 
            RefreshRepositoryAndFolderTree();
        }
    }

    public void RefreshRepositoryAndFolderTree()
    {
        View.ActivateWaitState();
        NotifyMessage("Refreshing tree folder ...");
        SelectedFilterWithServiceRef = null;
        SelectedFolderWithServiceRef = null;
        SelectedNoteInfo = null;
        FolderPath = "";
        CountNotes = 0;
        FoldersSelectorComponent.ServicesRef = null;  // force get repostiroy list form store
        FoldersSelectorComponent.Refresh();
        NoteEditorComponent.CleanView();
        NotesSelectorComponent.CleanView();
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
        await RefreshActiveFilterWithServiceRef(SelectedFilterWithServiceRef);
    }

    public async Task MoveSelectedNotes()
    {                
        var selectedNotes = NotesSelectorComponent.GetSelectedListNotesInfo().ToList();
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
        if (res.Entity != EComponentResult.Executed)
            return;

        try
        {
            var folderId = folderSelector.SelectedEntity.FolderInfo.FolderId;
            var folderName = folderSelector.SelectedEntity.FolderInfo.Name;

            // --- Heavy process singleton model
            HeavyProcessComponent.UpdateProcessName($"Moving notes to folder '{folderName}'.");
            await HeavyProcessComponent.Exec2(MoveSelectedNotesAction, selectedNotes, folderId);            
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

    public async Task MoveSelectedNotesAction(List<NoteInfoDto> selectedNotes, Guid newFolderId, CancellationTokenSource cancellationToken = null, IProgress<KNoteProgress> progress = null, HeavyProcessCtrl heavyProcessComponent = null)
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
                progress.Report(new KNoteProgress { Progress = percentageInt, Info = $"Updating Note #{n.NoteNumber}", HeavyProcessComponent = heavyProcessComponent });

            if (cancellationToken != null && cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
        }
    }

    public async Task ChangeTags(EnumChangeTag action)
    {
        string labelInput;        

        var selectedNotes = NotesSelectorComponent.GetSelectedListNotesInfo().ToList();
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
                using var heavyProcessComponent = new HeavyProcessCtrl(Store);
                heavyProcessComponent.ReportProgress = new Progress<KNoteProgress>(ReportProgressChangeTags);
                heavyProcessComponent.UpdateProcessName($"Updating tags. {labelInput} {tag} .");
                await heavyProcessComponent.Exec3(ChangeTagsAction, action, selectedNotes, tag);
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

    public async Task ChangeTagsAction(EnumChangeTag action, List<NoteInfoDto> selectedNotes, string tag, CancellationTokenSource cancellationToken = null, IProgress<KNoteProgress> progress = null, HeavyProcessCtrl heavyProcessComponent = null)
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
                progress.Report(new KNoteProgress { Progress = percentageInt, Info = $"Updating Note #{note.NoteNumber}", HeavyProcessComponent = heavyProcessComponent });

            if (cancellationToken != null && cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
        }
    }

    public void RunScriptSelectedNotes()
    {
        var selectedNotes = NotesSelectorComponent.GetSelectedListNotesInfo();

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
        var optionsEditorComponent = new OptionsEditorCtrl(Store);
        
        optionsEditorComponent.LoadModel(
            SelectedServiceRef?.Service,
            Store.AppConfig.GetSimpleDto<AppConfig>(), 
            true);
        var res = optionsEditorComponent.RunModal();
        if (res.Entity == EComponentResult.Executed)
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
        var kntLabComponent = new KntLabCtrl(Store);
        kntLabComponent.Run();
        kntLabComponent.ShowLabView();
    }

    #endregion

    #region Events handlers for extension components 

    private async void PostItEditorComponent_AddedEntity(object sender, ComponentEventArgs<NoteDto> e)
    {
        await OnNoteEditorAdded(e.Entity.GetSimpleDto<NoteInfoDto>());
    }

    private async void NoteEditorComponent_AddedEntity(object sender, ComponentEventArgs<NoteExtendedDto> e)
    {
        await OnNoteEditorAdded(e.Entity.GetSimpleDto<NoteInfoDto>());
    }

    private async Task OnNoteEditorAdded(NoteInfoDto noteInfo)
    {
        if (NotesSelectorComponent.ListEntities == null)
            return;

        if (NotesSelectorComponent.ListEntities.Count == 0)
        {
            await NoteEditorComponent.LoadModelById(SelectedServiceRef.Service, noteInfo.NoteId);
            _selectedNoteInfo = noteInfo;
        }
        NotesSelectorComponent.AddItem(noteInfo);
    }

    private async void PostItEditorComponent_SavedEntity(object sender, ComponentEventArgs<NoteDto> e)
    {
        await OnNoteEditorSaved(e.Entity.GetSimpleDto<NoteInfoDto>());
    }

    private async void NoteEditorComponent_SavedEntity(object sender, ComponentEventArgs<NoteExtendedDto> e)
    {
        await OnNoteEditorSaved(e.Entity.GetSimpleDto<NoteInfoDto>()); 
    }

    private async void PostItEditorComponent_ExtendedEdit(object sender, ComponentEventArgs<ServiceWithNoteId> e)
    {
        await EditNote(e.Entity.Service, e.Entity.NoteId);
    }

    private async void NoteEditorComponent_PostItEdit(object sender, ComponentEventArgs<ServiceWithNoteId> e)
    {
        await EditNotePostIt(e.Entity.Service, e.Entity.NoteId);
    }

    private async Task OnNoteEditorSaved(NoteInfoDto noteInfo)
    {
        if (NoteEditorComponent.Model.NoteId == noteInfo.NoteId)
            await NoteEditorComponent.LoadModelById(SelectedServiceRef.Service, noteInfo.NoteId);

        NotesSelectorComponent.RefreshItem(noteInfo);

        if (NotesSelectorComponent.ListEntities?.Count == 0)
        {
            NoteEditorComponent.View.CleanView();
            _selectedNoteInfo = null;
        }                            
    }

    private void PostItEditorComponent_DeletedEntity(object sender, ComponentEventArgs<NoteDto> e)
    {
        OnNoteEditorDeleted(e.Entity.GetSimpleDto<NoteInfoDto>());
    }

    private void NoteEditorComponent_DeletedEntity(object sender, ComponentEventArgs<NoteExtendedDto> e)
    {
        OnNoteEditorDeleted(e.Entity.GetSimpleDto<NoteInfoDto>());
    }

    private void OnNoteEditorDeleted(NoteInfoDto noteInfo)
    {            
        NotesSelectorComponent.DeleteItem(noteInfo);
        if (NotesSelectorComponent.ListEntities?.Count == 0)
        {
            NoteEditorComponent.View.CleanView();
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
            await RefreshActiveFilterWithServiceRef(SelectedFilterWithServiceRef);
    }

    private async Task NewRepository(EnumRepositoryEditorMode mode)
    {
        var repositoryEditorComponent = new RepositoryEditorCtrl(Store);
        repositoryEditorComponent.EditorMode = mode;
        await repositoryEditorComponent.NewModel();
        var res = repositoryEditorComponent.RunModal();
        if (res.Entity == EComponentResult.Executed)
        {
            RefreshRepositoryAndFolderTree();
        }
    }
    
    private async Task AddNotePostIt(FolderWithServiceRef folderWithServiceRef)
    {
        var postItEditorComponent = new PostItEditorCtrl(Store);
        postItEditorComponent.FolderWithServiceRef = folderWithServiceRef;
        await postItEditorComponent.NewModel(folderWithServiceRef.ServiceRef.Service);
        postItEditorComponent.Run();
    }

    private void RunKntChatComponent(bool visibleView = true)
    {
        var chatCanRun = KntChatComponent.Run();
        if (chatCanRun.Entity == EComponentResult.Executed)
        {
            KntChatComponent.ShowChatView(false);
            KntChatComponent.VisibleView(visibleView);
        }
        else
        {
            KntChatComponent.Finalize();
            KntChatComponent = null;
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
