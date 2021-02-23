using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            set { Store.ActiveFolderWithServiceRef = value; } 
        }

        public NotesFilterWithServiceRef SelectedFilterWithServiceRef
        {
            get { return Store.ActiveFilterWithServiceRef; }
            set { Store.ActiveFilterWithServiceRef = value; }
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
        }

        public string FolderPath { get; set; }

        public int? CountNotes { get; set; }

        #endregion 

        #region Constructor

        public KNoteManagmentComponent(Store store) : base(store)
        {

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

        #endregion

        #region Component override methods

        protected override Result<EComponentResult> OnInitialized()
        {
            ComponentName = "Key note managment";

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

                    // TODO: ... ¿las extensiones se definen aquí?
                    //SelectorPlanificaciones.Extensiones.Add("Definir nueva planificación ...", new ExtensionesEventHandler<Planificacion>(ExtenderNuevaPlanificacion));
                    //SelectorPlanificaciones.Extensiones.Add("Editar planificación ...", new ExtensionesEventHandler<Planificacion>(ExtenderEditarPlanificacion));
                    //SelectorPlanificaciones.Extensiones.Add("Borrar planificación ...", new ExtensionesEventHandler<Planificacion>(ExtenderBorrarPlanificacion));
                }
                return _folderSelectorComponent;
            }
        }

        private async void _folderSelectorComponent_EntitySelection(object sender, ComponentEventArgs<FolderWithServiceRef> e)
        {
            if (e.Entity == null)                            
                return;

            SelectMode = EnumSelectMode.Folders;

            NotifyMessage($"Loading notes list for folder {e.Entity.FolderInfo?.FolderNumber}");

            SelectedFolderWithServiceRef = e.Entity;            
            FolderPath = FoldersSelectorComponent.Path;            
            
            _selectedNoteInfo = null;            
            NoteEditorComponent.View.CleanView();
            
            await NotesSelectorComponent.LoadEntities(e.Entity.ServiceRef.Service, e.Entity.FolderInfo);
            CountNotes = NotesSelectorComponent.ListEntities?.Count;
            
            View.ShowInfo(null);
            NotifyMessage($"Loaded notes list for folder {e.Entity.FolderInfo?.FolderNumber}");
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
            if (e.Entity == null)
                return;

            NotifyMessage($"Loading note details for note {e.Entity.NoteNumber}");

            _selectedNoteInfo = e.Entity;            
            await NoteEditorComponent.LoadModelById(SelectedServiceRef.Service, _selectedNoteInfo.NoteId);

            NotifyMessage($"Loaded note details for note {e.Entity.NoteNumber}");
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

        private async void _filterParamComponent_EntitySelection(object sender, ComponentEventArgs<NotesFilterWithServiceRef> e)
        {            
            SelectMode = EnumSelectMode.Filters;

            NotifyMessage($"Loading notes filter: {e.Entity?.NotesFilter?.TextSearch}");

            SelectedFilterWithServiceRef = e.Entity;
            FolderPath = $"Notes filter: {e.Entity?.NotesFilter?.TextSearch}";

            _selectedNoteInfo = null;
            NoteEditorComponent.View.CleanView();

            await NotesSelectorComponent.LoadFilteredEntities(e.Entity?.ServiceRef?.Service, e.Entity?.NotesFilter);            
            CountNotes = NotesSelectorComponent.ListEntities?.Count;

            View.ShowInfo(null);
            NotifyMessage($"Loaded notes filter {e.Entity?.NotesFilter?.TextSearch}");            
        }

        #endregion

        #endregion

        #region Component public methods

        public async Task<bool> FinalizeApp()
        {
            if (View.ShowInfo("Are you sure exit KaNote?", "KaNote", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                await Store.SaveActiveNotes();
                Finalize();
                return true;
            }                
            return false;
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
            noteEditorComponent.SavedEntity += NoteEditorComponent_SavedEntity;
            noteEditorComponent.DeletedEntity += NoteEditorComponent_DeletedEntity;
            noteEditorComponent.PostItEdit += NoteEditorComponent_PostItEdit;
            var res = await noteEditorComponent.LoadModelById(service, noteId, false);
            noteEditorComponent.Run();
            return await Task.FromResult<bool>(res);
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
            postItEditorComponent.SavedEntity += PostItEditorComponent_SavedEntity;
            postItEditorComponent.DeletedEntity += PostItEditorComponent_DeletedEntity;
            postItEditorComponent.ExtendedEdit += PostItEditorComponent_ExtendedEdit;
            var res = await postItEditorComponent.LoadModelById(service, noteId, false);
            if(alwaysTop)
                postItEditorComponent.ForceAlwaysTop = true;
            postItEditorComponent.Run();
            return await Task.FromResult(res);
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

        private async void AddNote(IKntService service)
        {
            var noteEditorComponent = new NoteEditorComponent(Store);
            noteEditorComponent.AddedEntity += NoteEditorComponent_AddedEntity;
            noteEditorComponent.SavedEntity += NoteEditorComponent_SavedEntity;
            noteEditorComponent.DeletedEntity += NoteEditorComponent_DeletedEntity;
            noteEditorComponent.PostItEdit += NoteEditorComponent_PostItEdit;
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
        
        private async void AddNotePostIt(FolderWithServiceRef folderWithServiceRef)  // IKntService service
        {
            var postItEditorComponent = new PostItEditorComponent(Store);
            postItEditorComponent.AddedEntity += PostItEditorComponent_AddedEntity;
            postItEditorComponent.SavedEntity += PostItEditorComponent_SavedEntity;
            postItEditorComponent.DeletedEntity += PostItEditorComponent_DeletedEntity;
            postItEditorComponent.ExtendedEdit += PostItEditorComponent_ExtendedEdit;
            postItEditorComponent.FolderWithServiceRef = folderWithServiceRef;
            await postItEditorComponent.NewModel(folderWithServiceRef.ServiceRef.Service);
            postItEditorComponent.Run();
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
            folderEditorComponent.Model.ParentId = SelectedFolderInfo.FolderId;
            folderEditorComponent.Model.ParentFolderDto = SelectedFolderInfo.GetSimpleDto<FolderDto>();
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
            _folderSelectorComponent_EntitySelection(this, new ComponentEventArgs<FolderWithServiceRef>(SelectedFolderWithServiceRef));
        }

        public void GoActiveFilter()
        {
            _filterParamComponent_EntitySelection(this, new ComponentEventArgs<NotesFilterWithServiceRef>(SelectedFilterWithServiceRef));
        }

        public void RunScriptSelectedNote()
        {
            Store.RunScript(SelectedNoteInfo.Script);
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

    }

    public enum EnumSelectMode
    {
        Folders,
        Filters
    }

}
