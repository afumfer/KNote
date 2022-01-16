using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

using KNote.ClientWin.Core;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using KntScript;

namespace KNote.ClientWin.Components
{
    public class KNoteManagmentComponent : ComponentViewBase<IViewConfigurableExt>
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

        #region Constructor

        public KNoteManagmentComponent(Store store) : base(store)
        {
            Store.ChangedActiveFolderWithServiceRef += Store_ChangedActiveFolderWithServiceRef;
            Store.ChangedActiveFilterWithServiceRef += Store_ChangedActiveFilterWithServiceRef;

            Store.AddedPostIt += PostItEditorComponent_AddedEntity;
            Store.SavedPostIt += PostItEditorComponent_SavedEntity;
            Store.DeletedPostIt += PostItEditorComponent_SavedEntity;
            Store.ExtendedEditPostIt += PostItEditorComponent_ExtendedEdit;

            Store.AddedNote += NoteEditorComponent_AddedEntity;
            Store.SavedNote += NoteEditorComponent_SavedEntity;
            Store.DeletedNote += NoteEditorComponent_DeletedEntity;
            Store.EditedPostItNote += NoteEditorComponent_PostItEdit;
        }

        private void Store_ChangedActiveFolderWithServiceRef(object sender, ComponentEventArgs<FolderWithServiceRef> e)
        {
            RefreshActiveFolderWithServiceRef(e.Entity);
        }

        private void Store_ChangedActiveFilterWithServiceRef(object sender, ComponentEventArgs<NotesFilterWithServiceRef> e)
        {
            RefreshActiveFilterWithServiceRef(e.Entity);
        }

        private async void RefreshActiveFolderWithServiceRef(FolderWithServiceRef folderWithServideRef)
        {
            if (folderWithServideRef == null)
                return;

            SelectMode = EnumSelectMode.Folders;

            NotifyMessage($"Loading notes list for folder {folderWithServideRef.FolderInfo?.FolderNumber}");

            FolderPath = FoldersSelectorComponent.Path;

            _selectedNoteInfo = null;
            NoteEditorComponent.CleanView();
            await NotesSelectorComponent.LoadEntities(folderWithServideRef.ServiceRef.Service, folderWithServideRef.FolderInfo);
            CountNotes = NotesSelectorComponent.ListEntities?.Count;

            View.ShowInfo(null);
            NotifyMessage($"Loaded notes list for folder {folderWithServideRef.FolderInfo?.FolderNumber}");
        }

        private async void RefreshActiveFilterWithServiceRef(NotesFilterWithServiceRef notesFilterWithServiceRef)
        {
            SelectMode = EnumSelectMode.Filters;

            NotifyMessage($"Loading notes filter: {notesFilterWithServiceRef?.NotesFilter?.TextSearch}");
            
            FolderPath = $"Notes filter: {notesFilterWithServiceRef?.NotesFilter?.TextSearch}";

            _selectedNoteInfo = null;
            NoteEditorComponent.View.CleanView();
            await NotesSelectorComponent.LoadFilteredEntities(notesFilterWithServiceRef?.ServiceRef?.Service, notesFilterWithServiceRef?.NotesFilter);
            CountNotes = NotesSelectorComponent.ListEntities?.Count;

            View.ShowInfo(null);
            NotifyMessage($"Loaded notes filter {notesFilterWithServiceRef?.NotesFilter?.TextSearch}");
        }


        #endregion

        #region Views

        protected override IViewConfigurableExt CreateView()
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
            ComponentName = "KaNote managment";

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

        private FoldersSelectorComponent _folderSelectorComponent;
        public FoldersSelectorComponent FoldersSelectorComponent
        {
            get
            {
                if (_folderSelectorComponent == null)
                {
                    _folderSelectorComponent = new FoldersSelectorComponent(Store);
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

        private void ExtendEditFolder(object sender, ComponentEventArgs<FolderWithServiceRef> e)
        {
            EditFolder();
        }

        private void ExtendDeleteFolder(object sender, ComponentEventArgs<FolderWithServiceRef> e)
        {
            DeleteFolder();
        }

        #endregion

        #region NotesSelector component

        private NotesSelectorComponent _notesSelectorComponent;
        public NotesSelectorComponent NotesSelectorComponent
        {
            get
            {
                if (_notesSelectorComponent == null)
                {
                    _notesSelectorComponent = new NotesSelectorComponent(Store);
                    _notesSelectorComponent.EmbededMode = true;                    
                    _notesSelectorComponent.EntitySelection += _notesSelectorComponent_EntitySelection;
                    _notesSelectorComponent.EntitySelectionDoubleClick += _notesSelectorComponent_EntitySelectionDoubleClick;

                    _notesSelectorComponent.Extensions.Add("New note ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendAddNote));
                    _notesSelectorComponent.Extensions.Add("New note as PostIt...", new ExtensionsEventHandler<NoteInfoDto>(ExtendAddNoteAsPostIt));
                    _notesSelectorComponent.Extensions.Add("Edit note ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendEditNote));
                    _notesSelectorComponent.Extensions.Add("Edit note as PostIt ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendEditNoteAsPostIt));
                    _notesSelectorComponent.Extensions.Add("Delete note ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendDeleteNote));
                    _notesSelectorComponent.Extensions.Add("--", new ExtensionsEventHandler<NoteInfoDto>(ExtendNull));
                    _notesSelectorComponent.Extensions.Add("Move selected notes ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendMoveSelectedNotes));
                    _notesSelectorComponent.Extensions.Add("Add tag to selected notes ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendAddTagSelectedNotes));
                    _notesSelectorComponent.Extensions.Add("Remove tag from selected notes ...", new ExtensionsEventHandler<NoteInfoDto>(ExtendRemoveTagSelectedNotes));
                }
                return _notesSelectorComponent;
            }
        }

        private void _notesSelectorComponent_EntitySelectionDoubleClick(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            if (e.Entity == null)
                return;
            _selectedNoteInfo = e.Entity;
            EditNote();
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

        private void ExtendAddNote(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            AddNote();
        }

        private void ExtendAddNoteAsPostIt(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            AddNotePostIt();
        }

        private void ExtendEditNote(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            EditNote();
        }

        private void ExtendEditNoteAsPostIt(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            EditNotePostIt();
        }

        private void ExtendDeleteNote(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            DeleteNote();
        }

        private void ExtendMoveSelectedNotes(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            MoveSelectedNotes();
        }

        private void ExtendNull(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            
        }

        private void ExtendAddTagSelectedNotes(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            AddTagsSelectedNotes();
        }

        private void ExtendRemoveTagSelectedNotes(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            RemoveTagsSelectedNotes();
        }

        #endregion

        #region NoteEditor component

        private NoteEditorComponent _noteEditorComponent;
        public NoteEditorComponent NoteEditorComponent
        {
            get
            {
                if (_noteEditorComponent == null)
                {
                    _noteEditorComponent = new NoteEditorComponent(Store);
                    _noteEditorComponent.EmbededMode = true;
                }
                return _noteEditorComponent;
            }
        }

        #endregion

        #region Messages Managment component

        private MessagesManagmentComponent _messagesManagmentComponent;
        public MessagesManagmentComponent MessagesManagmentComponent
        {
            get
            {
                if(_messagesManagmentComponent == null)
                {
                    _messagesManagmentComponent = new MessagesManagmentComponent(Store);
                    _messagesManagmentComponent.PostItVisible += MessagesManagment_PostItVisible;                    
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
            Store.RunScript(note?.Script);
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

        private async void MessagesManagment_PostItVisible(object sender, ComponentEventArgs<ServiceWithNoteId> e)
        {
            if (await Store.CheckPostItIsActive(e.Entity.NoteId))
                return;
            await EditNotePostIt(e.Entity.Service, e.Entity.NoteId);
        }

        #endregion

        #region FilterParam component

        private FiltersSelectorComponent _filterParamComponent;
        public FiltersSelectorComponent FilterParamComponent
        {
            get
            {
                if (_filterParamComponent == null)
                {
                    _filterParamComponent = new FiltersSelectorComponent(Store);
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

        #endregion

        #region Component public methods

        public async Task<bool> FinalizeApp()
        {
            if (View.ShowInfo("Are you sure exit KaNote?", "KaNote", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

            var kntScriptCom = new KntScriptConsoleComponent(Store);
            kntScriptCom.KntSEngine = kntEngine;

            kntScriptCom.Run();
        }

        public async void EditNote()
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

        public async Task<bool> EditNote(IKntService service, Guid noteId)
        {
            var noteEditorComponent = new NoteEditorComponent(Store);
            // TODO !!! delete this code
            //noteEditorComponent.SavedEntity += NoteEditorComponent_SavedEntity;
            //noteEditorComponent.DeletedEntity += NoteEditorComponent_DeletedEntity;
            //noteEditorComponent.PostItEdit += NoteEditorComponent_PostItEdit;
            var res = await noteEditorComponent.LoadModelById(service, noteId, false);
            noteEditorComponent.Run();            
            return res;
        }

        public async void EditNotePostIt()
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
            var postItEditorComponent = new PostItEditorComponent(Store);
            // TODO !!! delete this code
            //postItEditorComponent.SavedEntity += PostItEditorComponent_SavedEntity;
            //postItEditorComponent.DeletedEntity += PostItEditorComponent_DeletedEntity;
            //postItEditorComponent.ExtendedEdit += PostItEditorComponent_ExtendedEdit;
            var res = await postItEditorComponent.LoadModelById(service, noteId, false);
            if(alwaysTop)
                postItEditorComponent.ForceAlwaysTop = true;
            postItEditorComponent.Run();
            return res;
        }

        public void AddNote()
        {
            if(SelectedFolderInfo == null)
            {
                View.ShowInfo("There is no selected folder to create new note.");
                return;
            }
            AddNote(SelectedServiceRef.Service);
        }

        public async void AddNote(IKntService service)
        {
            var noteEditorComponent = new NoteEditorComponent(Store);
            // TODO !!! delete this code
            //noteEditorComponent.AddedEntity += NoteEditorComponent_AddedEntity;            
            //noteEditorComponent.SavedEntity += NoteEditorComponent_SavedEntity;
            //noteEditorComponent.DeletedEntity += NoteEditorComponent_DeletedEntity;
            //noteEditorComponent.PostItEdit += NoteEditorComponent_PostItEdit;
            await noteEditorComponent.NewModel(service);
            noteEditorComponent.Run();
        }

        public void AddNotePostIt()
        {
            if (SelectedFolderInfo == null)
            {
                View.ShowInfo("There is no selected folder to create new note.");
                return;
            }

            AddNotePostIt(SelectedFolderWithServiceRef);
        }

        public void AddDefaultNotePostIt()
        {
            if (Store.DefaultFolderWithServiceRef == null)
            {
                View.ShowInfo("A default folder has not been defined for personal notes.");
                return;
            }

            AddNotePostIt(DefaultFolderWithServiceRef);
        }
        
        public async void DeleteNote()
        {
            if (SelectedNoteInfo == null)
            {
                View.ShowInfo("There is no note selected to delete.");
                return;
            }

            var noteEditorComponent = new NoteEditorComponent(Store);
            noteEditorComponent.DeletedEntity += NoteEditorComponent_DeletedEntity;
            await noteEditorComponent.DeleteModel(SelectedServiceRef.Service, SelectedNoteInfo.NoteId);            
        }

        public async void NewFolder()
        {
            var folderEditorComponent = new FolderEditorComponent(Store);
            await folderEditorComponent.NewModel(SelectedServiceRef.Service);
            folderEditorComponent.Model.ParentId = SelectedFolderInfo?.FolderId;
            folderEditorComponent.Model.ParentFolderDto = SelectedFolderInfo?.GetSimpleDto<FolderDto>();
            var res = folderEditorComponent.RunModal();
            if (res.Entity == EComponentResult.Executed)
            {                
                var fs = new FolderWithServiceRef { ServiceRef = SelectedServiceRef, FolderInfo = folderEditorComponent.Model.GetSimpleDto<FolderInfoDto>() };
                FoldersSelectorComponent.AddItem(fs);
            }
        }

        public async void EditFolder()
        {
            if(SelectedFolderInfo == null)
            {
                View.ShowInfo("There is no folder selected to edit.");
                return;
            }

            var folderEditorComponent = new FolderEditorComponent(Store);
            await folderEditorComponent.LoadModelById(SelectedServiceRef.Service, SelectedFolderInfo.FolderId, false);
            FoldersSelectorComponent.OldParent = folderEditorComponent.Model.ParentId;
            var res = folderEditorComponent.RunModal();
            if (res.Entity == EComponentResult.Executed)
            {                
                SelectedFolderWithServiceRef.FolderInfo = folderEditorComponent.Model.GetSimpleDto<FolderInfoDto>();
                FoldersSelectorComponent.RefreshItem(SelectedFolderWithServiceRef);
            }            
        }

        public async void DeleteFolder()
        {
            if (SelectedFolderInfo == null)
            {
                View.ShowInfo("There is no folder selected to delete.");
                return;
            }

            var folderEditorComponent = new FolderEditorComponent(Store);
            var res = await folderEditorComponent.DeleteModel(SelectedServiceRef.Service, SelectedFolderInfo.FolderId);
            if (res)
            {                
                FoldersSelectorComponent.DeleteItem(SelectedFolderWithServiceRef);
            }            
        }

        public async void RemoveRepositoryLink()
        {
            if (SelectedServiceRef == null)
            {
                View.ShowInfo("There is no repository selected to remove.");
                return;
            }            
            var repositoryEditorComponent = new RepositoryEditorComponent(Store);            
            var res = await repositoryEditorComponent.DeleteModel(SelectedServiceRef.Service, SelectedServiceRef.IdServiceRef);
            if (res)
            {                
                RefreshRepositoryAndFolderTree();
            }
        }

        public void AddRepositoryLink()
        {
            NewRepository(EnumRepositoryEditorMode.AddLink);
        }

        public void CreateRepository()
        {
            NewRepository(EnumRepositoryEditorMode.Create);
        }

        public async void ManagmentRepository()
        {
            if (SelectedServiceRef == null)
            {
                View.ShowInfo("There is no repository selected to configure.");
                return;
            }                        
            var repositoryEditorComponent = new RepositoryEditorComponent(Store);
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
        }

        public void ShowKNoteManagment() 
        {
            View.ActivateView();            
        }

        public void HideKNoteManagment()
        {                        
            View.HideView();            
        }

        public void GoActiveFolder()
        {        
            RefreshActiveFolderWithServiceRef(SelectedFolderWithServiceRef);            
        }

        public void GoActiveFilter()
        {            
            RefreshActiveFilterWithServiceRef(SelectedFilterWithServiceRef);
        }

        public void RunScriptSelectedNote()
        {
            Store.RunScript(SelectedNoteInfo.Script);
        }
        
        public async void MoveSelectedNotes()
        {
            var selectedNotes = NotesSelectorComponent.GetSelectedListNotesInfo();
            if(selectedNotes == null || selectedNotes?.Count == 0)
            {                
                View.ShowInfo("You have not selected notes .");
                return;
            }

            var folderSelector = new FoldersSelectorComponent(Store);
            var services = new List<ServiceRef>();
            services.Add(SelectedServiceRef);
            folderSelector.ServicesRef = services;
            var res = folderSelector.RunModal();
            if (res.Entity != EComponentResult.Executed)
                return;

            var newFolderId = folderSelector.SelectedEntity.FolderInfo.FolderId;                          
            foreach (var n in selectedNotes)                            
                await SelectedServiceRef.Service.Notes.PatchFolder(n.NoteId, newFolderId);

            ForceRefreshListNotes();
        }

        public void AddTagsSelectedNotes()
        {
            ChangeTags(EnumChangeTag.Add);
        }

        public void RemoveTagsSelectedNotes()
        {
            ChangeTags(EnumChangeTag.Remove);
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
            var optionsEditorComponent = new OptionsEditorComponent(Store);
            
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
            string url = @"https://github.com/afumfer/KNote/blob/master/Docs/Manual.md";

            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
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

        #endregion

        #region Events handlers for extension components 

        private void PostItEditorComponent_AddedEntity(object sender, ComponentEventArgs<NoteDto> e)
        {
            OnNoteEditorAdded(e.Entity.GetSimpleDto<NoteInfoDto>());
        }

        private void NoteEditorComponent_AddedEntity(object sender, ComponentEventArgs<NoteExtendedDto> e)
        {
            OnNoteEditorAdded(e.Entity.GetSimpleDto<NoteInfoDto>());
        }

        private async void OnNoteEditorAdded(NoteInfoDto noteInfo)
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

        private void PostItEditorComponent_SavedEntity(object sender, ComponentEventArgs<NoteDto> e)
        {
            OnNoteEditorSaved(e.Entity.GetSimpleDto<NoteInfoDto>());
        }

        private void NoteEditorComponent_SavedEntity(object sender, ComponentEventArgs<NoteExtendedDto> e)
        {
            OnNoteEditorSaved(e.Entity.GetSimpleDto<NoteInfoDto>()); 
        }

        private async void PostItEditorComponent_ExtendedEdit(object sender, ComponentEventArgs<ServiceWithNoteId> e)
        {
            await EditNote(e.Entity.Service, e.Entity.NoteId);
        }

        private async void NoteEditorComponent_PostItEdit(object sender, ComponentEventArgs<ServiceWithNoteId> e)
        {
            await EditNotePostIt(e.Entity.Service, e.Entity.NoteId);
        }

        private async void OnNoteEditorSaved(NoteInfoDto noteInfo)
        {
            if (NoteEditorComponent.Model.NoteId == noteInfo.NoteId)
                await NoteEditorComponent.LoadModelById(SelectedServiceRef.Service, noteInfo.NoteId);
            NotesSelectorComponent.RefreshItem(noteInfo);
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

        private void ForceRefreshListNotes()
        {
            if (SelectMode == EnumSelectMode.Folders)                
                RefreshActiveFolderWithServiceRef(SelectedFolderWithServiceRef);
            else if (SelectMode == EnumSelectMode.Filters)                
                RefreshActiveFilterWithServiceRef(SelectedFilterWithServiceRef);
        }

        private async void ChangeTags(EnumChangeTag action)
        {
            var strTmp = "";

            var selectedNotes = NotesSelectorComponent.GetSelectedListNotesInfo();
            if (selectedNotes == null || selectedNotes?.Count == 0)
            {
                if (action == EnumChangeTag.Add)
                    View.ShowInfo("You have not selected notes for add tags .");
                else
                    View.ShowInfo("You have not selected notes for remove tags .");
                return;
            }

            if (action == EnumChangeTag.Add)
                strTmp = "Type new tag:";
            else
                strTmp = "Type tag for remove:";

            var listVars = new List<ReadVarItem> {new ReadVarItem
            {

                Label = strTmp,
                VarIdent = "Tag",
                VarValue = "",
                VarNewValueText = ""
            } };

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
                var tag = listVars[0].VarNewValueText;
                foreach (var note in selectedNotes)
                    if (action == EnumChangeTag.Add)
                        await SelectedServiceRef.Service.Notes.PatchChangeTags(note.NoteId, "", tag);
                    else
                        await SelectedServiceRef.Service.Notes.PatchChangeTags(note.NoteId, tag, "");

                ForceRefreshListNotes();
            }
        }

        private async void NewRepository(EnumRepositoryEditorMode mode)
        {
            var repositoryEditorComponent = new RepositoryEditorComponent(Store);
            repositoryEditorComponent.EditorMode = mode;
            await repositoryEditorComponent.NewModel();
            var res = repositoryEditorComponent.RunModal();
            if (res.Entity == EComponentResult.Executed)
            {
                RefreshRepositoryAndFolderTree();
            }
        }
        
        private async void AddNotePostIt(FolderWithServiceRef folderWithServiceRef)
        {
            var postItEditorComponent = new PostItEditorComponent(Store);
            // TODO !!! delete this code
            //postItEditorComponent.AddedEntity += PostItEditorComponent_AddedEntity;
            //postItEditorComponent.SavedEntity += PostItEditorComponent_SavedEntity;
            //postItEditorComponent.DeletedEntity += PostItEditorComponent_DeletedEntity;
            //postItEditorComponent.ExtendedEdit += PostItEditorComponent_ExtendedEdit;
            postItEditorComponent.FolderWithServiceRef = folderWithServiceRef;
            await postItEditorComponent.NewModel(folderWithServiceRef.ServiceRef.Service);
            postItEditorComponent.Run();
        }

        #endregion

        #region Private Enums

        enum EnumChangeTag
        {
            Add,
            Remove
        }

        #endregion 

    }

    #region Public enums 

    public enum EnumSelectMode
    {
        Folders,
        Filters
    }

    #endregion 
}
