using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using KNote.ClientWin.Core;
using KNote.ClientWin.Core.Reports;
using KNote.ClientWin.Utils;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KntScript;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Controllers;

public class KNoteManagementCtrl : CtrlViewBase<IViewKNoteManagement>
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

    // Who is using the application and with which role in the active repository: roles are per
    // repository, so this changes with the selected folder or filter. Shown in the status bar.
    public string SessionUserInfo
    {
        get
        {
            var serviceRef = SelectedServiceRef;
            if (serviceRef == null)
                return Store.AppUserName;

            return $"{Store.AppUserName} · {Store.Security.GetRepositoryRoleName(serviceRef.Service)} ({serviceRef.Alias})";
        }
    }

    // How the user signed in and its role in every linked repository (status bar tooltip).
    public string SessionUserDetail
    {
        get
        {
            var lines = new List<string>
            {
                Store.Security.AuthenticationMode == AppAuthenticationMode.Windows
                    ? $"Signed in with the Windows account '{Store.AppUserName}'."
                    : $"Signed in as the {KntConst.AppName} user '{Store.AppUserName}'.",
                "Roles:"
            };
            foreach (var serviceRef in Store.GetAllServiceRef())
                lines.Add($"   {serviceRef.Alias}: {Store.Security.GetRepositoryRoleName(serviceRef.Service)}");

            return string.Join(Environment.NewLine, lines);
        }
    }

    public EnumSelectMode SelectMode { get; set; } = EnumSelectMode.Folders;

    public FolderWithServiceRef DefaultFolderWithServiceRef
    {
        get { return Store.DefaultFolderWithServiceRef; }
    }

    private NoteMinimalDto _selectedNoteInfo;
    public NoteMinimalDto SelectedNoteInfo
    {
        get { return _selectedNoteInfo; }
        protected set { _selectedNoteInfo = value; }
    }

    public string FolderPath { get; set; }

    public int? CountNotes { get; set; }

    #endregion 

    #region Constructor, Dispose, ...

    public KNoteManagementCtrl(Store store) : base(store)
    {
        Store.ChangedActiveFolderWithServiceRef += Store_ChangedActiveFolderWithServiceRef;
        Store.ChangedActiveFilterWithServiceRef += Store_ChangedActiveFilterWithServiceRef;

        Store.Events.Subscribe<EntityAdded<NoteDto>>(PostItEditorCtrl_AddedEntity);
        Store.Events.Subscribe<EntitySaved<NoteDto>>(PostItEditorCtrl_SavedEntity);
        Store.Events.Subscribe<EntityDeleted<NoteDto>>(PostItEditorCtrl_DeletedEntity);
        Store.Events.Subscribe<ExtendedEditRequested>(PostItEditorCtrl_ExtendedEdit);

        Store.Events.Subscribe<EntityAdded<NoteExtendedDto>>(NoteEditorCtrl_AddedEntity);
        Store.Events.Subscribe<EntitySaved<NoteExtendedDto>>(NoteEditorCtrl_SavedEntity);
        Store.Events.Subscribe<EntityDeleted<NoteExtendedDto>>(NoteEditorCtrl_DeletedEntity);
        Store.Events.Subscribe<PostItEditRequested>(NoteEditorCtrl_PostItEdit);

        Store.Events.Subscribe<EntitySaved<FolderDto>>(FolderEditorCtrl_SavedEntity);
        Store.Events.Subscribe<EntitySaved<RepositoryRef>>(RepositoryEditorCtrl_SavedEntity);
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

        var isFilter = selectedNotesInServiceRef?.NotesFilter != null;
        var description = isFilter
            ? await BuildFilterDescription(selectedNotesInServiceRef.ServiceRef, selectedNotesInServiceRef.NotesFilter)
            : selectedNotesInServiceRef?.NotesSearch?.TextSearch;

        NotifyMessage($"Loading notes filter: {description}");

        FolderPath = $"Notes filter: {description}";

        _selectedNoteInfo = null;
        NoteEditorCtrl.View.CleanView();
        if (isFilter)
            await NotesSelectorCtrl.LoadFilteredEntities(selectedNotesInServiceRef.ServiceRef?.Service, selectedNotesInServiceRef.NotesFilter);
        else
            await NotesSelectorCtrl.LoadSearchEntities(selectedNotesInServiceRef?.ServiceRef?.Service, selectedNotesInServiceRef?.NotesSearch);
        CountNotes = NotesSelectorCtrl.ListEntities?.Count;

        View.ShowInfo(null);
        NotifyMessage($"Loaded notes filter {description}");

        View.DeactivateWaitState();
    }

    private async Task<string> BuildFilterDescription(ServiceRef serviceRef, NotesFilterDto notesFilter)
    {
        var criteria = await DescribeFilterCriteriaAsync(serviceRef, notesFilter);
        return criteria.Count > 0 ? string.Join(", ", criteria.Select(c => $"{c.Label}={c.Value}")) : "(no criteria)";
    }

    // The criteria actually set in a structured filter, with names instead of ids (note type, folder path).
    // A name that can't be resolved (e.g. deleted meanwhile) shows as "?" rather than failing the caller.
    private async Task<List<ReportMetaItem>> DescribeFilterCriteriaAsync(ServiceRef serviceRef, NotesFilterDto notesFilter)
    {
        var criteria = new List<ReportMetaItem>();

        if (!string.IsNullOrWhiteSpace(notesFilter.Topic))
            criteria.Add(new ReportMetaItem("Topic", notesFilter.Topic.Trim()));
        if (!string.IsNullOrWhiteSpace(notesFilter.Description))
            criteria.Add(new ReportMetaItem("Description", notesFilter.Description.Trim()));
        if (!string.IsNullOrWhiteSpace(notesFilter.Tags))
            criteria.Add(new ReportMetaItem("Tags", notesFilter.Tags.Trim()));

        if (notesFilter.NoteTypeId != null)
        {
            string noteType = null;
            try
            {
                noteType = serviceRef == null ? null : (await serviceRef.Service.NoteTypes.GetAsync(notesFilter.NoteTypeId.Value)).Entity?.Name;
            }
            catch (Exception ex)
            {
                Store.Logger?.LogError(ex, "DescribeFilterCriteriaAsync, note type: {message}", ex.Message);
            }
            criteria.Add(new ReportMetaItem("Note type", noteType ?? "?"));
        }

        if (notesFilter.FolderId != null)
        {
            var folderPath = await Store.GetKNoteFolerPath(serviceRef, notesFilter.FolderId.Value);
            if (string.IsNullOrEmpty(folderPath))
                folderPath = "?";
            criteria.Add(new ReportMetaItem("Folder", notesFilter.IncludeChildFolders ? $"{folderPath} (and subfolders)" : folderPath));
        }

        foreach (var atr in notesFilter.AttributesFilter ?? new List<AtrFilterDto>())
            criteria.Add(new ReportMetaItem(string.IsNullOrWhiteSpace(atr.AtrName) ? "Attribute" : atr.AtrName, atr.Value ?? ""));

        return criteria;
    }

    // The renamed folder may be an ancestor of the active folder (the displayed path includes every
    // ancestor's name), so there's no cheap way to tell in advance whether it's actually relevant to
    // the header - always recomputing the active folder's path is simple and cheap enough (one
    // interactive rename at a time, never a hot path). Only applies in Folders mode: in Filters mode
    // FolderPath instead holds the filter description text, which a folder rename has nothing to do
    // with.
    private async void FolderEditorCtrl_SavedEntity(EntitySaved<FolderDto> e)
    {
        if (SelectMode != EnumSelectMode.Folders || SelectedFolderInfo == null)
            return;

        if (SelectedFolderInfo.FolderId == e.Entity.FolderId)
            SelectedFolderWithServiceRef.FolderInfo = e.Entity.GetSimpleDto<FolderInfoDto>();

        FolderPath = await Store.GetKNoteFolerPath(SelectedFolderWithServiceRef.ServiceRef, SelectedFolderInfo.FolderId);
        View.ShowInfo(null);
    }

    private void RepositoryEditorCtrl_SavedEntity(EntitySaved<RepositoryRef> e)
    {
        View.ShowInfo(null);
    }

    public override void Dispose()
    {
        Store.ChangedActiveFolderWithServiceRef -= Store_ChangedActiveFolderWithServiceRef;
        Store.ChangedActiveFilterWithServiceRef -= Store_ChangedActiveFilterWithServiceRef;

        Store.Events.Unsubscribe<EntityAdded<NoteDto>>(PostItEditorCtrl_AddedEntity);
        Store.Events.Unsubscribe<EntitySaved<NoteDto>>(PostItEditorCtrl_SavedEntity);
        Store.Events.Unsubscribe<EntityDeleted<NoteDto>>(PostItEditorCtrl_DeletedEntity);
        Store.Events.Unsubscribe<ExtendedEditRequested>(PostItEditorCtrl_ExtendedEdit);

        Store.Events.Unsubscribe<EntityAdded<NoteExtendedDto>>(NoteEditorCtrl_AddedEntity);
        Store.Events.Unsubscribe<EntitySaved<NoteExtendedDto>>(NoteEditorCtrl_SavedEntity);
        Store.Events.Unsubscribe<EntityDeleted<NoteExtendedDto>>(NoteEditorCtrl_DeletedEntity);
        Store.Events.Unsubscribe<PostItEditRequested>(NoteEditorCtrl_PostItEdit);

        Store.Events.Unsubscribe<EntitySaved<FolderDto>>(FolderEditorCtrl_SavedEntity);
        Store.Events.Unsubscribe<EntitySaved<RepositoryRef>>(RepositoryEditorCtrl_SavedEntity);

        base.Dispose();
    }

    #endregion

    #region Views

    protected override IViewKNoteManagement CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<KNoteManagementCtrl, IViewKNoteManagement>(this);
    }

    private IViewBase _notifyView;
    public IViewBase NotifyView
    {
        get
        {
            if (_notifyView == null)
            {
                _notifyView = Store.FactoryViews.Registry.Resolve<KNoteManagementCtrl, IViewBase>(this, key: "Notify");
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
                _aboutView = Store.FactoryViews.Registry.Resolve<KNoteManagementCtrl, IViewBase>(this, key: "About");
            }
            return _aboutView;
        }
    }


    #endregion

    #region Controller override methods

    protected override Result<EControllerResult> OnInitialized()
    {
        ControllerName = $"{KntConst.AppName} management";

        var result = base.OnInitialized();

        // TODO: pending check result correctrly

        try
        {                
            using (new WaitCursor())
            {
                NotesSelectorCtrl.Run();
                FoldersSelectorCtrl.SelectedFolderId = Store.State.Session.LastActiveFolderId;
                FoldersSelectorCtrl.Run();
                NotesSearchParamCtrl.Run();
                NotesFilterParamCtrl.Run();
                NoteEditorCtrl.Run();
                MessagesManagementCtrl.Run();

                // Show the Application info alarms panel on startup if the user left it with
                // undismissed rows from a previous session, so they aren't left wondering where
                // their pending reminders went - same intent as PostIts reopening themselves via
                // MessagesManagementCtrl.VisibleWindows(). Fire-and-forget: deciding this needs to
                // hit the database (AppInfoAlarmRowMaintenance), and creating AppInfoAlarmsCtrl shows
                // its window immediately (see its own doc comment) - so that decision must be made
                // and the persisted rows sanitized *before* touching the property, not after.
                _ = ShowAppInfoAlarmsIfAnyPendingAsync();

                NotifyView.ShowView();

                // TODO: Experimental ---------------------------------
                // Started automatically: skipped without a word for a user who can't use the chat.
                if (!string.IsNullOrEmpty(Store.Settings.Connectivity.ChatHub.Url) && Store.Security.IsAuthorized(typeof(KntChatCtrl)))
                {
                    if (!Store.State.Session.ChatHubAutoConnectDisabled)
                    {
                        RunKntChatCtrl(false);
                    }
                    else
                    {
                        NotifyMessage($"Chat auto-connect is disabled because the last connection to '{Store.Settings.Connectivity.ChatHub.Url}' failed. Fix the url and test it from Options to re-enable it.");
                    }
                }
                //-----------------------------------------------------
            }
        }
        catch (Exception ex)
        {
            result.AddErrorMessage(ex.Message);
        }

        var configNotices = Store.TakeConfigNotices();
        if (configNotices.Count > 0)
            View.ShowInfo(string.Join(Environment.NewLine + Environment.NewLine, configNotices));

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

                _notesSelectorCtrl.Extensions.Add("New note ...", new ExtensionsEventHandler<NoteMinimalDto>(ExtendAddNote));
                _notesSelectorCtrl.Extensions.Add("New note as PostIt...", new ExtensionsEventHandler<NoteMinimalDto>(ExtendAddNoteAsPostIt));
                _notesSelectorCtrl.Extensions.Add("Edit note ...", new ExtensionsEventHandler<NoteMinimalDto>(ExtendEditNote));
                _notesSelectorCtrl.Extensions.Add("Edit note as PostIt ...", new ExtensionsEventHandler<NoteMinimalDto>(ExtendEditNoteAsPostIt));
                _notesSelectorCtrl.Extensions.Add("Delete note ...", new ExtensionsEventHandler<NoteMinimalDto>(ExtendDeleteNote));
                _notesSelectorCtrl.Extensions.Add("--0", new ExtensionsEventHandler<NoteMinimalDto>(ExtendNull));
                _notesSelectorCtrl.Extensions.Add("Add quick resolved task", new ExtensionsEventHandler<NoteMinimalDto>(AddFastResolvedTask));
                _notesSelectorCtrl.Extensions.Add("--1", new ExtensionsEventHandler<NoteMinimalDto>(ExtendNull));
                _notesSelectorCtrl.Extensions.Add("Move selected notes ...", new ExtensionsEventHandler<NoteMinimalDto>(ExtendMoveSelectedNotes));
                _notesSelectorCtrl.Extensions.Add("Add tag to selected notes ...", new ExtensionsEventHandler<NoteMinimalDto>(ExtendAddTagSelectedNotes));
                _notesSelectorCtrl.Extensions.Add("Remove tag from selected notes ...", new ExtensionsEventHandler<NoteMinimalDto>(ExtendRemoveTagSelectedNotes));
                _notesSelectorCtrl.Extensions.Add("Trace selected notes to ...", new ExtensionsEventHandler<NoteMinimalDto>(ExtendTraceSelectedNotesTo));
                _notesSelectorCtrl.Extensions.Add("Trace selected notes from ...", new ExtensionsEventHandler<NoteMinimalDto>(ExtendTraceSelectedNotesFrom));
                _notesSelectorCtrl.Extensions.Add("--2", new ExtensionsEventHandler<NoteMinimalDto>(ExtendNull));
                _notesSelectorCtrl.Extensions.Add(NotesSelectorForm.ToggleTextFilterMenuText, new ExtensionsEventHandler<NoteMinimalDto>(ExtendToggleTextFilter));
            }
            return _notesSelectorCtrl;
        }
    }

    private async void _notesSelectorCtrl_EntitySelectionDoubleClick(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        if (e.Entity == null)
            return;
        _selectedNoteInfo = e.Entity;
        await EditNote();
    }

    private async void _notesSelectorCtrl_EntitySelection(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        if (e.Entity == null || SelectedServiceRef == null)
            return;

        NotifyMessage($"Loading note details for note {e.Entity.NoteNumber}");

        _selectedNoteInfo = e.Entity;            
        await NoteEditorCtrl.LoadModelById(SelectedServiceRef.Service, _selectedNoteInfo.NoteId);

        NotifyMessage($"Loaded note details for note {e.Entity.NoteNumber}");
    }

    private async void ExtendAddNote(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        await AddNote();
    }

    private async void ExtendAddNoteAsPostIt(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        await AddNotePostIt();
    }

    private async void ExtendEditNote(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        await EditNote();
    }

    private async void AddFastResolvedTask(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        await AddFastResolvedTask();
    }
   
    private async void ExtendEditNoteAsPostIt(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        await EditNotePostIt();
    }

    private async void ExtendDeleteNote(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        await DeleteNote();
    }

    private async void ExtendMoveSelectedNotes(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        await MoveSelectedNotes();
    }

    private void ExtendNull(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        
    }

    private async void ExtendAddTagSelectedNotes(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {            
        await ChangeTags(EnumChangeTag.Add);
    }

    private async void ExtendRemoveTagSelectedNotes(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        await ChangeTags(EnumChangeTag.Remove);
    }

    private async void ExtendTraceSelectedNotesTo(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        await TraceSelectedNotes(selectedAreFromSide: true);
    }

    private async void ExtendTraceSelectedNotesFrom(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        await TraceSelectedNotes(selectedAreFromSide: false);
    }

    private void ExtendToggleTextFilter(object sender, ControllerEventArgs<NoteMinimalDto> e)
    {
        ToggleNotesListFilter();
    }

    // Shared by the notes grid's context menu entry and the main menu's View > "Show list filter"
    // entry - both toggle the same underlying state, so they stay in sync with each other.
    public void ToggleNotesListFilter()
    {
        NotesSelectorCtrl.EnableTextFilter = !NotesSelectorCtrl.EnableTextFilter;
        NotesSelectorCtrl.View.RefreshView();
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

    #region Messages Management controller

    private MessagesManagementCtrl _messagesManagementCtrl;
    public MessagesManagementCtrl MessagesManagementCtrl
    {
        get
        {
            if(_messagesManagementCtrl == null)
            {
                _messagesManagementCtrl = new MessagesManagementCtrl(Store);
                _messagesManagementCtrl.PostItVisible += _messagesManagement_PostItVisible;                    
                _messagesManagementCtrl.PostItAlarm += _messagesManagement_PostItAlarm;
                _messagesManagementCtrl.EMailAlarm += _messagesManagement_EMailAlarm;
                _messagesManagementCtrl.AppAlarm += _messagesManagement_AppAlarm;
                _messagesManagementCtrl.ExecuteKntScript += _messagesManagementCtrl_ExecuteKntScript;
            }
            return _messagesManagementCtrl;
        }
    }

    private async void _messagesManagementCtrl_ExecuteKntScript(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        try
        {
            var service = e.Entity.Service;
            var note = (await (service.Notes.GetAsync(e.Entity.NoteId))).Entity;
            // The note may have been deleted since the alarm was scheduled.
            if (note == null)
                return;

            // Fired by an alarm: skipped (and logged) without a word for a user who can't run scripts.
            if (!Store.CanRunScripts)
            {
                Store.Logger?.LogInformation("Script alarm of note {noteId} skipped: the user can't run scripts.", e.Entity.NoteId);
                return;
            }

            await Store.RunCode(note, caller: this);
        }
        catch (Exception ex)
        {
            ReportAlarmError(ex, "script", e.Entity.NoteId);
        }
    }

    #endregion

    #region Application info alarms controller

    // Kept alive for the whole session once first needed (either the user opens it from the menu, or
    // an AppInfo alarm fires) so it keeps accumulating rows in the background - see AppInfoAlarmsCtrl.
    private AppInfoAlarmsCtrl _appInfoAlarmsCtrl;
    // Null when the user may not use it (Run() refused it, and told the user): it is tried again next time.
    public AppInfoAlarmsCtrl AppInfoAlarmsCtrl
    {
        get
        {
            if (_appInfoAlarmsCtrl == null)
            {
                var appInfoAlarmsCtrl = new AppInfoAlarmsCtrl(Store);
                appInfoAlarmsCtrl.OpenNoteRequested += _appInfoAlarmsCtrl_OpenNoteRequested;
                appInfoAlarmsCtrl.Run();
                if (appInfoAlarmsCtrl.PreconditionsMet)
                    _appInfoAlarmsCtrl = appInfoAlarmsCtrl;
            }
            return _appInfoAlarmsCtrl;
        }
    }

    public void ShowAppInfoAlarms()
    {
        AppInfoAlarmsCtrl?.Activate();
    }

    // Startup-only: see the call site in OnInitialized(). Not used by ShowAppInfoAlarms() (the menu
    // option) - opening the panel by hand always shows it, empty or not, same as before.
    private async Task ShowAppInfoAlarmsIfAnyPendingAsync()
    {
        // Started automatically: skipped without a word for a user who can't use the panel.
        if (!Store.Security.IsAuthorized(typeof(AppInfoAlarmsCtrl)))
            return;

        if (await AppInfoAlarmRowMaintenance.PruneAndHasAnyRowAsync(Store))
            AppInfoAlarmsCtrl?.Activate();
    }

    private async void _appInfoAlarmsCtrl_OpenNoteRequested(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        if (await Store.CheckNoteIsOpenOnDesktop(e.Entity.NoteId))
        {
            View.ShowInfo("This note is already active.");
            return;
        }
        await EditNote(e.Entity.Service, e.Entity.NoteId);
    }

    private async void _messagesManagement_AppAlarm(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        var service = e.Entity.Service;
        var noteId = e.Entity.NoteId;

        try
        {
            var note = (await service.Notes.GetAsync(noteId)).Entity;
            var messages = (await service.Notes.GetMessagesAsync(noteId)).Entity;

            var currentUser = (await service.Users.GetByUserNameAsync(Store.AppUserName)).Entity;
            var repositoryAlias = Store.GetServiceRef(service.IdServiceRef)?.Alias;

            // Deleted note, a failed query, or the current user not registered in this repository.
            if (note == null || messages == null || currentUser == null)
                return;

            // Same note-level granularity caveat as Email alarms (see _messagesManagement_EMailAlarm):
            // GetAlarmNotesIdAsync only reports the note, not which specific message fired.
            var appInfoMessages = messages.Where(m => m.NotificationType == EnumNotificationType.AppInfo && m.UserId == currentUser.UserId).ToList();
            if (appInfoMessages.Count == 0)
                return;

            foreach (var message in appInfoMessages)
            {
                AppInfoAlarmsCtrl?.AddOrUpdateRow(new AppInfoAlarmRow
                {
                    KMessageId = message.KMessageId,
                    NoteId = noteId,
                    RepositoryAlias = repositoryAlias,
                    NoteTopic = note.Topic,
                    Comment = message.Comment,
                    UserFullName = currentUser.FullName,
                    NotifiedAt = DateTime.Now
                });
            }

            AppInfoAlarmsCtrl?.Activate();
        }
        catch (Exception ex)
        {
            ReportAlarmError(ex, "Application info", noteId);
        }
    }

    #endregion

    #region Messages Management alarm handlers

    private async void _messagesManagement_EMailAlarm(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        var service = e.Entity.Service;
        var noteId = e.Entity.NoteId;

        try
        {
            var note = (await service.Notes.GetAsync(noteId)).Entity;
            var messages = (await service.Notes.GetMessagesAsync(noteId)).Entity;

            var currentUser = (await service.Users.GetByUserNameAsync(Store.AppUserName)).Entity;

            // Deleted note, a failed query, or the current user not registered in this repository.
            if (note == null || messages == null || currentUser == null)
                return;

            // GetAlarmNotesIdAsync only reports which note had a due Email alarm, not which of its
            // messages fired (the repository already consumed/rescheduled it as a side effect of the
            // query) - so, like PostIt/ExecuteKntScript already do at note granularity, every Email
            // message on this note still assigned to the current user is (re)sent here.
            foreach (var message in messages.Where(m => m.NotificationType == EnumNotificationType.Email && m.UserId == currentUser.UserId))
                await SendEmailAlarm(service, note, message);
        }
        catch (Exception ex)
        {
            ReportAlarmError(ex, "Email", noteId);
        }
    }

    private async Task SendEmailAlarm(IKntService service, NoteDto note, KMessageDto message)
    {
        try
        {
            var recipient = (await service.Users.GetAsync(message.UserId ?? Guid.Empty)).Entity;
            if (recipient == null || string.IsNullOrEmpty(recipient.EMail))
                throw new InvalidOperationException("The assigned user has no valid email address.");

            var settings = SmtpSettings.FromConfig(Store.Settings.Notifications.Email);
            var subject = note.Topic;
            var body = $"{message.Comment}\r\n\r\n{note.Description}";

            await Task.Run(() => new SmtpEmailSender().Send(settings, recipient.EMail, subject, body));
        }
        catch (Exception ex)
        {
            // Fire-and-forget by design (see plan): on failure the send is simply lost, and the
            // error is recorded in the message's own Comment so the user finds out next time they
            // open the note's alarm list - no retry.
            message.Comment = $"[Email failed {DateTime.Now:dd/MM/yyyy HH:mm}: {ex.Message}] {message.Comment}";
            message.SetIsDirty(true);
            await service.Notes.SaveMessageAsync(message, false);
        }
    }

    private async void _messagesManagement_PostItAlarm(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        try
        {
            if (await Store.CheckNoteIsOpenOnDesktop(e.Entity.NoteId))
                return;
            await EditNotePostIt(e.Entity.Service, e.Entity.NoteId, true);
        }
        catch (Exception ex)
        {
            ReportAlarmError(ex, "PostIt", e.Entity.NoteId);
        }
    }

    private async void _messagesManagement_PostItVisible(object sender, ControllerEventArgs<ServiceWithNoteId> e)
    {
        try
        {
            if (await Store.CheckPostItIsActive(e.Entity.NoteId))
                return;
            await EditNotePostIt(e.Entity.Service, e.Entity.NoteId);
        }
        catch (Exception ex)
        {
            Store.Logger?.LogError(ex, "Reopening the PostIt of note {noteId} failed.", e.Entity.NoteId);
            NotifyMessage($"A PostIt could not be reopened: {ex.Message}");
        }
    }

    // Alarm handlers run unattended from MessagesManagementCtrl's timer (async void): a failure
    // processing one note's alarm is logged and shown in the status bar, never allowed to escape.
    private void ReportAlarmError(Exception ex, string alarmType, Guid noteId)
    {
        Store.Logger?.LogError(ex, "{alarmType} alarm of note {noteId} failed.", alarmType, noteId);
        NotifyMessage($"The {alarmType} alarm of a note could not be processed: {ex.Message}");
    }

    #endregion

    #region NotesSearchParam controller

    private NotesSearchParamCtrl _notesSearchParamCtrl;
    public NotesSearchParamCtrl NotesSearchParamCtrl
    {
        get
        {
            if (_notesSearchParamCtrl == null)
            {
                _notesSearchParamCtrl = new NotesSearchParamCtrl(Store);
                _notesSearchParamCtrl.EmbededMode = true;
                _notesSearchParamCtrl.SearchApplied += _notesSearchParamCtrl_SearchApplied;
            }
            return _notesSearchParamCtrl;
        }
    }

    private void _notesSearchParamCtrl_SearchApplied(object sender, ControllerEventArgs<SelectedNotesInServiceRef> e)
    {
        SelectedNotesInServiceRef = e.Entity;
    }

    #endregion

    #region NotesFilterParam controller

    private NotesFilterParamCtrl _notesFilterParamCtrl;
    public NotesFilterParamCtrl NotesFilterParamCtrl
    {
        get
        {
            if (_notesFilterParamCtrl == null)
            {
                _notesFilterParamCtrl = new NotesFilterParamCtrl(Store);
                _notesFilterParamCtrl.EmbededMode = true;
                _notesFilterParamCtrl.FilterApplied += _notesFilterParamCtrl_FilterApplied;
            }
            return _notesFilterParamCtrl;
        }
    }

    private void _notesFilterParamCtrl_FilterApplied(object sender, ControllerEventArgs<SelectedNotesInServiceRef> e)
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
        /// TODO: !!!
        //var kntEngine = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(Store));

        var kntScriptCtrl = new KntScriptConsoleCtrl(Store);
        //kntScriptCtrl.KntSEngine = kntEngine;

        kntScriptCtrl.Run();
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

    public void ShowKNoteAIAssistantConsole()
    {
        var kNoteAIAssistantCtrl = new KNoteAIAssistantCtrl(Store);
        kNoteAIAssistantCtrl.Run();
        kNoteAIAssistantCtrl.ShowAIAssistantView(true, true);
    }

    // KNoteAIAssistant plan (Phase 4): maintenance screen for the AI provider/model collection
    // consumed by KNoteAIAssistantCtrl's picker.
    public async Task ManageAiProviders()
    {
        var aiProvidersManageCtrl = new AiProvidersManageCtrl(Store);
        await aiProvidersManageCtrl.LoadEntitiesAsync(null, false);
        aiProvidersManageCtrl.RunModal();
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
        if (await Store.CheckNoteIsOpenOnDesktop(SelectedNoteInfo.NoteId))
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
        if (await Store.CheckNoteIsOpenOnDesktop(SelectedNoteInfo.NoteId))
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
            Tags = "Resolved task",
            Priority = 1,
            Resolved = true,
            EndDate = DateTime.Now
        };

        var resNoteForSaveTask = await service.Notes.GetExtendedAsync(SelectedNoteInfo.NoteId);
        if (!resNoteForSaveTask.IsValid)
        {
            View.ShowInfo(resNoteForSaveTask.ErrorMessage);
            return;
        }
        resNoteForSaveTask.Entity.Tasks.Add(task);

        // Refused (e.g. a Guest in this repository) or failed: the reason is in the result.
        var res = await service.Notes.SaveExtendedAsync(resNoteForSaveTask.Entity);
        if (!res.IsValid)
        {
            View.ShowInfo(res.ErrorMessage);
            return;
        }

        await OnNoteEditorSaved(res.Entity.GetSimpleDto<NoteMinimalDto>());
    }

    // A controller whose model could not be loaded (LoadModelById already told the user why) is
    // discarded instead of being shown half-initialized.
    public async Task<bool> EditNote(IKntService service, Guid noteId)
    {
        var noteEditorCtrl = new NoteEditorCtrl(Store);
        var res = await noteEditorCtrl.LoadModelById(service, noteId, false);
        if (!res)
        {
            noteEditorCtrl.Dispose();
            return false;
        }
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
        if (await Store.CheckNoteIsOpenOnDesktop(SelectedNoteInfo.NoteId))
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
        // Same as EditNote: without a loaded model (and WindowPostIt) the PostIt can't be shown.
        if (!res)
        {
            postItEditorCtrl.Dispose();
            return false;
        }
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
        if (await noteEditorCtrl.NewModel(service))
            noteEditorCtrl.Run();
        else
            noteEditorCtrl.Finalize();
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

    public async Task ManagementRepository()
    {
        if (SelectedServiceRef == null)
        {
            View.ShowInfo("There is no repository selected to configure.");
            return;
        }                        
        var repositoryEditorCtrl = new RepositoryEditorCtrl(Store);
        repositoryEditorCtrl.EditorMode = EnumRepositoryEditorMode.Management;
        var serviceRef = SelectedServiceRef;
        await repositoryEditorCtrl.LoadModelById(serviceRef.Service, serviceRef.IdServiceRef, false);
        var res = repositoryEditorCtrl.RunModal();

        // The users of the repository may have been edited there, the current one included.
        await Store.RefreshRepositoryRoleAsync(serviceRef.Service);
        View.ShowInfo(null);

        if (res.Entity == EControllerResult.Executed)
        {
            // Do action 
            RefreshRepositoryAndFolderTree();
        }
    }

    public async void RefreshRepositoryAndFolderTree()
    {
        View.ActivateWaitState();
        NotifyMessage("Refreshing tree folder ...");

        // Captured before any of the resets below can change SelectMode/SelectedFolderWithServiceRef.
        // FoldersSelectorCtrl.SelectedFolderId re-highlights this folder in the tree once it reloads
        // below, but that reselect doesn't reliably retrigger the embedded NotesSelectorCtrl's own
        // reload in turn (e.g. after editing a repository's alias, the tree correctly kept the right
        // folder highlighted but its notes list stayed stale until switching away and back) - so
        // ForceRefreshListNotes() is called explicitly further down instead of relying on that
        // indirect chain.
        var activeFolder = SelectMode == EnumSelectMode.Folders ? SelectedFolderWithServiceRef : null;

        SelectedNotesInServiceRef = null;
        SelectedNoteInfo = null;
        FoldersSelectorCtrl.SelectedFolderId = activeFolder?.FolderInfo?.FolderId;
        FoldersSelectorCtrl.ServicesRef = null;  // force get repostiroy list form store
        FoldersSelectorCtrl.Refresh();
        NoteEditorCtrl.CleanView();

        if (activeFolder != null)
        {
            SelectMode = EnumSelectMode.Folders;
            await ForceRefreshListNotes();
        }
        else
        {
            SelectedFolderWithServiceRef = null;
            FolderPath = "";
            CountNotes = 0;
            NotesSelectorCtrl.CleanView();
            View.ShowInfo(null);
        }

        NotifyMessage("Refreshed tree folder ...");
        View.DeactivateWaitState();
    }

    public void ShowKNoteManagement() 
    {
        View.ActivateView();            
    }

    public void HideKNoteManagement()
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
        var selectedNotes = NotesSelectorCtrl.GetSelectedListNotesMinimal().ToList();
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
        catch (KntNotAuthorizedException ex)
        {
            View.ShowInfo(ex.Message);
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

    public async Task MoveSelectedNotesAction(List<NoteMinimalDto> selectedNotes, Guid newFolderId, CancellationTokenSource cancellationToken = null, IProgress<KNoteProgress> progress = null, HeavyProcessCtrl heavyProcessCtrl = null)
    {
        var index = 0;
        var service = new ServiceRef(SelectedServiceRef.RepositoryRef, SelectedServiceRef.UserIdentityName).Service;
        foreach (var n in selectedNotes)
        {
            KntNotAuthorizedException.ThrowIfNotAuthorized(await service.Notes.UtilPatchFolderAsync(n.NoteId, newFolderId));

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

        var selectedNotes = NotesSelectorCtrl.GetSelectedListNotesMinimal().ToList();
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

        var caption = action == EnumChangeTag.Add
            ? "New tags for selected notes"
            : "Remove tags in selected notes";

        var tag = View.PromptForValue(labelInput, caption);

        if (tag == null)
            return;
        else
        {
            try
            {
                // --- Heavy process instance model
                using var heavyProcessCtrl = new HeavyProcessCtrl(Store);
                heavyProcessCtrl.ReportProgress = new Progress<KNoteProgress>(ReportProgressChangeTags);
                heavyProcessCtrl.UpdateProcessName($"Updating tags. {labelInput} {tag} .");
                await heavyProcessCtrl.Exec3(ChangeTagsAction, action, selectedNotes, tag);
            }
            catch (KntNotAuthorizedException ex)
            {
                View.ShowInfo(ex.Message);
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

    public async Task ChangeTagsAction(EnumChangeTag action, List<NoteMinimalDto> selectedNotes, string tag, CancellationTokenSource cancellationToken = null, IProgress<KNoteProgress> progress = null, HeavyProcessCtrl heavyProcessCtrl = null)
    {
        var index = 0;
        var service = new ServiceRef(SelectedServiceRef.RepositoryRef, SelectedServiceRef.UserIdentityName).Service;

        foreach (var note in selectedNotes)
        {
            var res = action == EnumChangeTag.Add
                ? await service.Notes.UtilPatchChangeTagsAsync(note.NoteId, "", tag)
                : await service.Notes.UtilPatchChangeTagsAsync(note.NoteId, tag, "");
            KntNotAuthorizedException.ThrowIfNotAuthorized(res);

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

    // "Trace selected notes to/from ..." (notes grid context menu). Reuses the same TraceNoteEditorCtrl
    // dialog as the "Trace notes" tab's [+] button to pick the target note plus type/order/weight, and
    // then creates that relation for every selected note. selectedAreFromSide = true: each selected
    // note is FromId and the target note is ToId ("to"); false: the target note is FromId ("from").
    public async Task TraceSelectedNotes(bool selectedAreFromSide)
    {
        var selectedNotes = NotesSelectorCtrl.GetSelectedListNotesMinimal().ToList();
        if (selectedNotes == null || selectedNotes.Count == 0)
        {
            View.ShowInfo("You have not selected notes for trace.");
            return;
        }

        var direction = selectedAreFromSide ? "to" : "from";

        var traceEditor = new TraceNoteEditorCtrl(Store);
        traceEditor.AutoDBSave = false;  // the dialog only fills in the template, saved below per note
        await traceEditor.NewModel(SelectedServiceRef.Service);
        traceEditor.OwnerIsFromSide = selectedAreFromSide;
        traceEditor.Model.SetIsNew(true);
        traceEditor.Caption = $"Trace {selectedNotes.Count} selected note(s) {direction}";
        await traceEditor.LoadTraceNoteTypeOptionsAsync();

        var res = traceEditor.RunModal();
        if (res.Entity != EControllerResult.Executed)
            return;

        var job = new TraceSelectedNotesJob(traceEditor.Model, selectedAreFromSide);
        try
        {
            // --- Heavy process instance model
            using var heavyProcessCtrl = new HeavyProcessCtrl(Store);
            heavyProcessCtrl.ReportProgress = new Progress<KNoteProgress>(ReportProgressChangeTags);
            heavyProcessCtrl.UpdateProcessName($"Tracing selected notes {direction} {traceEditor.RelatedNoteDisplay} .");
            await heavyProcessCtrl.Exec2(TraceSelectedNotesAction, job, selectedNotes);
        }
        catch (KntNotAuthorizedException ex)
        {
            View.ShowInfo(ex.Message);
            return;
        }
        catch (TaskCanceledException)
        {
            //View.ShowInfo("The operation has been canceled.");
        }

        // The notes grid doesn't show relations; open note editors (the embedded one included) of any
        // affected note pick up the new relations through this message.
        if (job.Created > 0)
            Store.Events.Publish(new TraceNotesChanged(selectedNotes.Select(n => n.NoteId).Append(job.TargetNoteId).ToList()));

        View.ShowInfo(job.Summary());
    }

    public async Task TraceSelectedNotesAction(TraceSelectedNotesJob job, List<NoteMinimalDto> selectedNotes, CancellationTokenSource cancellationToken = null, IProgress<KNoteProgress> progress = null, HeavyProcessCtrl heavyProcessCtrl = null)
    {
        var index = 0;
        var service = new ServiceRef(SelectedServiceRef.RepositoryRef, SelectedServiceRef.UserIdentityName).Service;

        foreach (var note in selectedNotes)
        {
            await job.TraceNoteAsync(service, note);

            index++;
            var percentage = (double)index / selectedNotes.Count;
            percentage = percentage * 100;
            var percentageInt = (int)Math.Round(percentage, 0);
            await Task.Delay(1); // This delay is necessary
            if (progress != null)
                progress.Report(new KNoteProgress { Progress = percentageInt, Info = $"Tracing Note #{note.NoteNumber}", HeavyProcessCtrl = heavyProcessCtrl });

            if (cancellationToken != null && cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
        }
    }

    public async Task RunCodeSelectedNotes(bool runInNewTask = true)
    {
        var selectedNotes = NotesSelectorCtrl.GetSelectedListNotesMinimal();

        if (selectedNotes == null || selectedNotes?.Count == 0)
        {
            View.ShowInfo("You have not selected notes for run scripts .");
            return;
        }

        // "...in new task" (runInNewTask=true, Ctrl+F5) only means something distinct from the
        // plain run for "knt" - see Store.SupportsNewTaskMode. With multiple notes of mixed script
        // types selected, the unsupported ones are skipped rather than aborting the whole batch,
        // same pattern as RunCodeSelectedNotesInStdOutConsole below.
        var unsupportedNotes = new List<string>();
        foreach (var note in selectedNotes)
        {
            var noteDto = (await NotesSelectorCtrl.Service.Notes.GetAsync(note.NoteId)).Entity;

            if (runInNewTask && !Store.SupportsNewTaskMode(noteDto.GetContentTypeExt()?.ForScript))
            {
                unsupportedNotes.Add($"#{note.NoteNumber}");
                continue;
            }

            await Store.RunCode(noteDto, runInNewTask, this);
        }

        if (unsupportedNotes.Count > 0)
            View.ShowInfo($"\"Run in new task\" is not supported for the script type of the following note(s) - they were skipped: {string.Join(", ", unsupportedNotes)}.");
    }

    // "...in stdout console" (Shift+F5): unlike RunCodeSelectedNotes, this always forces the OS
    // console regardless of engine - which not every script type supports (knt, ln have no OS
    // process/console). With multiple notes of mixed script types selected, the unsupported ones
    // are skipped rather than aborting the whole batch, and reported together at the end instead
    // of one message box per note.
    public async Task RunCodeSelectedNotesInStdOutConsole()
    {
        var selectedNotes = NotesSelectorCtrl.GetSelectedListNotesMinimal();

        if (selectedNotes == null || selectedNotes?.Count == 0)
        {
            View.ShowInfo("You have not selected notes for run scripts .");
            return;
        }

        var unsupportedNotes = new List<string>();
        foreach (var note in selectedNotes)
        {
            var noteDto = (await NotesSelectorCtrl.Service.Notes.GetAsync(note.NoteId)).Entity;
            var executed = await Store.RunCodeInStdOutConsole(noteDto, this);
            if (!executed)
                unsupportedNotes.Add($"#{note.NoteNumber}");
        }

        if (unsupportedNotes.Count > 0)
            View.ShowInfo($"The stdout console mode is not supported for the script type of the following note(s) - they were skipped: {string.Join(", ", unsupportedNotes)}.");
    }

    // Prints the notes list as the user is seeing it (columns, order, text filter), headed by the folder
    // path or the search/filter summary, through the report preview (print or save as PDF).
    public async Task PrintNotesList()
    {
        try
        {
            var snapshot = NotesSelectorCtrl.GetDisplayedNotes();
            if (snapshot.Rows.Count == 0)
            {
                View.ShowInfo("There are no notes in the list to print.");
                return;
            }

            var result = await ReportPreviewCtrl.ShowAsync(Store, SelectedServiceRef?.Service, async () =>
            {
                using (new WaitCursor())
                {
                    var context = await GetNotesListContextAsync(snapshot.TextFilter);
                    return NotesListReport.Build(snapshot, context, DateTime.Now);
                }
            });
            if (!result.IsValid)
                View.ShowInfo($"The notes list could not be printed: {result.ErrorMessage}");
        }
        catch (Exception ex)
        {
            Store.Logger?.LogError(ex, "PrintNotesList: {message}", ex.Message);
            View.ShowInfo($"The notes list could not be printed: {ex.Message}");
        }
    }

    // Prints the listed notes as a book: one chapter per note (Topic and Description) in the order they are
    // listed, after a cover and a table of contents, through the report preview (print or save as PDF).
    public async Task PrintNotesBook()
    {
        try
        {
            var snapshot = NotesSelectorCtrl.GetDisplayedNotes();
            if (snapshot.NoteIds.Count == 0)
            {
                View.ShowInfo("There are no notes in the list to print.");
                return;
            }

            var serviceRef = SelectedServiceRef;
            var result = await ReportPreviewCtrl.ShowAsync(Store, serviceRef?.Service, async () =>
            {
                using (new WaitCursor())
                {
                    var context = await GetNotesListContextAsync(snapshot.TextFilter);
                    var chapters = await NotesBook.LoadChaptersAsync(Store, serviceRef, snapshot.NoteIds);
                    return NotesBook.Build(context, chapters, DateTime.Now, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                }
            }, serviceRef?.RepositoryRef?.ResourcesContainerRootPath);
            if (!result.IsValid)
                View.ShowInfo($"The notes book could not be printed: {result.ErrorMessage}");
        }
        catch (Exception ex)
        {
            Store.Logger?.LogError(ex, "PrintNotesBook: {message}", ex.Message);
            View.ShowInfo($"The notes book could not be printed: {ex.Message}");
        }
    }

    // Prints everything about the note selected in the notes list, as saved in the repository (the panel
    // below the list is a read-only view of it).
    public async Task PrintSelectedNote()
    {
        if (SelectedNoteInfo == null)
        {
            View.ShowInfo("There is no note selected to print.");
            return;
        }

        try
        {
            var serviceRef = SelectedServiceRef;
            var noteId = SelectedNoteInfo.NoteId;
            var result = await ReportPreviewCtrl.ShowAsync(Store, serviceRef.Service, async () =>
            {
                using (new WaitCursor())
                {
                    var response = await serviceRef.Service.Notes.GetExtendedAsync(noteId);
                    if (!response.IsValid)
                        throw new InvalidOperationException(response.ErrorMessage);

                    var data = await NoteDetailReportData.CreateAsync(Store, serviceRef, response.Entity, unsavedChanges: false);
                    return NoteDetailReport.Build(data, DateTime.Now);
                }
            }, serviceRef.RepositoryRef?.ResourcesContainerRootPath);
            if (!result.IsValid)
                View.ShowInfo($"The note could not be printed: {result.ErrorMessage}");
        }
        catch (Exception ex)
        {
            Store.Logger?.LogError(ex, "PrintSelectedNote: {message}", ex.Message);
            View.ShowInfo($"The note could not be printed: {ex.Message}");
        }
    }

    // Exports the notes list as the user is seeing it to a CSV file (see NotesListCsvExportCtrl).
    public async Task ExportNotesListToCsv()
    {
        var exportCtrl = new NotesListCsvExportCtrl(Store)
        {
            Service = SelectedServiceRef?.Service,
            Snapshot = NotesSelectorCtrl.GetDisplayedNotes(),
            GetContextAsync = GetNotesListContextAsync
        };
        await exportCtrl.ExportAsync();
    }

    // Where the notes currently listed come from (folder, quick search or structured filter), with every
    // name resolved - used to identify the list in its printed report and in its exported file name.
    // textFilter is the in-memory filter the notes list view applies on top (NotesListSnapshot.TextFilter).
    public async Task<NotesListContext> GetNotesListContextAsync(string textFilter)
    {
        var serviceRef = SelectedServiceRef;
        var context = new NotesListContext
        {
            RepositoryAlias = serviceRef?.RepositoryRef?.Alias,
            RepositoryProvider = serviceRef?.RepositoryRef?.Provider,
            TextFilter = textFilter
        };

        if (SelectMode == EnumSelectMode.Folders)
        {
            context.Source = NotesListSource.Folder;
            context.FolderNumber = SelectedFolderInfo?.FolderNumber;
            if (SelectedFolderInfo != null)
                context.FolderPath = await Store.GetKNoteFolerPath(serviceRef, SelectedFolderInfo.FolderId);
        }
        else if (SelectedNotesInServiceRef?.NotesFilter != null)
        {
            context.Source = NotesListSource.Filter;
            context.FilterCriteria = await DescribeFilterCriteriaAsync(serviceRef, SelectedNotesInServiceRef.NotesFilter);
        }
        else
        {
            var notesSearch = SelectedNotesInServiceRef?.NotesSearch;
            context.Source = NotesListSource.Search;
            context.SearchText = notesSearch?.TextSearch?.Trim();
            context.SearchInDescription = notesSearch?.SearchInDescription ?? false;
            context.SearchInNoteTasks = notesSearch?.SearchInNoteTasks ?? false;
        }

        return context;
    }

    public async Task Options()
    {
        var optionsEditorCtrl = new OptionsEditorCtrl(Store);
        var colorMode = Store.Settings.General.ColorMode;
        var authenticationMode = Store.Settings.Security.AuthenticationMode;

        optionsEditorCtrl.LoadModel(
            SelectedServiceRef?.Service,
            OptionsModel.From(Store.Settings, Store.State),
            true);
        var res = optionsEditorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            // TODO: refresh context management
            // ... for next major version

            // Both are applied once, at startup.
            var changesOnRestart = new List<string>();
            if (Store.Settings.General.ColorMode != colorMode)
                changesOnRestart.Add("color mode");
            if (Store.Settings.Security.AuthenticationMode != authenticationMode)
                changesOnRestart.Add("way of signing in");

            if (changesOnRestart.Count > 0)
                await OfferRestartToApply(string.Join(" and ", changesOnRestart));
        }
    }

    // What View > Dark mode shows as checked: the configured mode, which only takes effect on the next start.
    public bool IsDarkModeConfigured => Store.Settings.General.ColorMode switch
    {
        AppColorMode.Dark => true,
        AppColorMode.System => AppTheme.IsDark,
        _ => false
    };

    public async Task ToggleDarkMode()
    {
        Store.Settings.General.ColorMode = IsDarkModeConfigured ? AppColorMode.Light : AppColorMode.Dark;
        Store.SaveConfig();
        await OfferRestartToApply("color mode");
    }

    // Settings that can't change while the app is running (the color mode, see AppTheme; the way of signing
    // in, see Store.Security): offer to restart it now.
    private async Task OfferRestartToApply(string changedSettings)
    {
        var answer = View.ShowInfo($"The new {changedSettings} will be applied the next time {KntConst.AppName} starts.\r\n\r\nRestart {KntConst.AppName} now?",
            KntConst.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes)
            return;

        Store.RestartRequested = true;
        await FinalizeAppForce();
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

    private async void PostItEditorCtrl_AddedEntity(EntityAdded<NoteDto> e)
    {
        await OnNoteEditorAdded(e.Entity.GetSimpleDto<NoteMinimalDto>());
    }

    private async void NoteEditorCtrl_AddedEntity(EntityAdded<NoteExtendedDto> e)
    {
        await OnNoteEditorAdded(e.Entity.GetSimpleDto<NoteMinimalDto>());
    }

    private async Task OnNoteEditorAdded(NoteMinimalDto noteInfo)
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

    private async void PostItEditorCtrl_SavedEntity(EntitySaved<NoteDto> e)
    {
        await OnNoteEditorSaved(e.Entity.GetSimpleDto<NoteMinimalDto>());
    }

    private async void NoteEditorCtrl_SavedEntity(EntitySaved<NoteExtendedDto> e)
    {
        await OnNoteEditorSaved(e.Entity.GetSimpleDto<NoteMinimalDto>());
    }

    private async void PostItEditorCtrl_ExtendedEdit(ExtendedEditRequested e)
    {
        await EditNote(e.Target.Service, e.Target.NoteId);
    }

    private async void NoteEditorCtrl_PostItEdit(PostItEditRequested e)
    {
        await EditNotePostIt(e.Target.Service, e.Target.NoteId);
    }

    private async Task OnNoteEditorSaved(NoteMinimalDto noteInfo)
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

    private void PostItEditorCtrl_DeletedEntity(EntityDeleted<NoteDto> e)
    {
        OnNoteEditorDeleted(e.Entity.GetSimpleDto<NoteMinimalDto>());
    }

    private void NoteEditorCtrl_DeletedEntity(EntityDeleted<NoteExtendedDto> e)
    {
        OnNoteEditorDeleted(e.Entity.GetSimpleDto<NoteMinimalDto>());
    }

    private void OnNoteEditorDeleted(NoteMinimalDto noteInfo)
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
        if (await postItEditorCtrl.NewModel(folderWithServiceRef.ServiceRef.Service))
            postItEditorCtrl.Run();
        else
            postItEditorCtrl.Finalize();
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
