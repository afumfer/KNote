using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using KNote.ClientWin.Core;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;
using KntScript;

namespace KNote.ClientWin.Components
{
    public class KNoteManagmentComponent : ComponentViewBase<IViewConfigurable>
    {
        #region Properties

        public FolderWithServiceRef SelectedFolderWithServiceRef 
        {
            get { return Store.ActiveFolderWithServiceRef; }
            set { Store.ActiveFolderWithServiceRef = value; } 
        }

        public FolderInfoDto SelectedFolderInfo
        {
            get { return SelectedFolderWithServiceRef?.FolderInfo; }
        }

        public ServiceRef SelectedServiceRef
        {
            get { return SelectedFolderWithServiceRef?.ServiceRef; }
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

        protected override IViewConfigurable CreateView()
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
                    NoteEditorComponent.Run();
                }
            }
            catch (Exception ex)
            {
                result.AddErrorMessage(ex.Message);                
            }
            
            return result;
        }

        //protected override Result OnFinalized()
        //{
        //    return base.OnFinalized();
        //}

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

                    // TODO: lo siguiente podría estar sobrando, delegar en _folderSelectorComponent la responsabilidad
                    //       de activar la carpeta por defecto.
                    //if (Store.ActiveFolderWithServiceRef != null)
                    //    _folderSelectorComponent.SelectedEntity = Store.ActiveFolderWithServiceRef;

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

            NotifyMessage($"Loading notes list for foler {e.Entity.FolderInfo?.FolderNumber}");

            SelectedFolderWithServiceRef = e.Entity;
            FolderPath = FoldersSelectorComponent.Path;            
            
            _selectedNoteInfo = null;            
            NoteEditorComponent.View.CleanView();
            
            await NotesSelectorComponent.LoadEntities(e.Entity.ServiceRef.Service, e.Entity.FolderInfo);
            CountNotes = NotesSelectorComponent.ListEntities?.Count;
            
            View.ShowInfo(null);
            NotifyMessage($"Loaded notes list for foler {e.Entity.FolderInfo?.FolderNumber}");
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

        #endregion

        #region Component public methods

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

            var noteEditorComponent = new NoteEditorComponent(Store);
            noteEditorComponent.SavedEntity += NoteEditorComponent_SavedEntity;
            noteEditorComponent.DeletedEntity += NoteEditorComponent_DeletedEntity;
            await noteEditorComponent.LoadModelById(SelectedServiceRef.Service, SelectedNoteInfo.NoteId, false);            
            noteEditorComponent.Run();
        }

        public async void EditNotePostIt()
        {
            if (SelectedNoteInfo == null)
            {
                View.ShowInfo("There is no note selected to edit.");
                return;
            }

            var postItEditorComponent = new PostItEditorComponent(Store);            
            postItEditorComponent.SavedEntity += PostItEditorComponent_SavedEntity;            
            postItEditorComponent.DeletedEntity += PostItEditorComponent_DeletedEntity;
            await postItEditorComponent.LoadModelById(SelectedServiceRef.Service, SelectedNoteInfo.NoteId, false);
            postItEditorComponent.Run();
        }

        public void AddNote()
        {
            if(SelectedServiceRef == null)
            {
                View.ShowInfo("There is no archiver selected to create new note.");
                return;
            }

            var noteEditorComponent = new NoteEditorComponent(Store);
            noteEditorComponent.AddedEntity += NoteEditorComponent_AddedEntity;
            noteEditorComponent.SavedEntity += NoteEditorComponent_SavedEntity;
            noteEditorComponent.DeletedEntity += NoteEditorComponent_DeletedEntity;
            noteEditorComponent.NewModel(SelectedServiceRef.Service);
            noteEditorComponent.Run();
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

        public void NewFolder()
        {
            var folderEditorComponent = new FolderEditorComponent(Store);
            folderEditorComponent.NewModel(SelectedServiceRef.Service);
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

        #endregion

        #region Events handlers for extension components 

        private void PostItEditorComponent_DeletedEntity(object sender, ComponentEventArgs<NoteDto> e)
        {
            
        }

        private async void PostItEditorComponent_SavedEntity(object sender, ComponentEventArgs<NoteDto> e)
        {
            // TODO: refactor ... 
            if (NoteEditorComponent.Model.NoteId == e.Entity.NoteId)
            {
                // TODO: !!! coger el modelo que está en memoria en lugar de volver a cargar desde la BD ??
                // NoteEditorComponent.RefreshNote(e.Entity);
                // or ...
                await NoteEditorComponent.LoadModelById(SelectedServiceRef.Service, e.Entity.NoteId);
            }
            else
                NoteEditorComponent.View.CleanView();

            NotesSelectorComponent.RefreshItem(e.Entity.GetSimpleDto<NoteInfoDto>());
        }



        private async void NoteEditorComponent_AddedEntity(object sender, ComponentEventArgs<NoteExtendedDto> e)
        {
            // TODO: !!! coger aquí la entidad que viene en el parámetro en lugar de acudir de nuevo a la BD ??        
            if(NotesSelectorComponent.ListEntities.Count == 0)
            {
                await NoteEditorComponent.LoadModelById(SelectedServiceRef.Service, e.Entity.NoteId);
                _selectedNoteInfo = e.Entity.GetSimpleDto<NoteInfoDto>();
            }

            NotesSelectorComponent.AddItem(e.Entity.GetSimpleDto<NoteInfoDto>());
        }

        private async void NoteEditorComponent_SavedEntity(object sender, ComponentEventArgs<NoteExtendedDto> e)
        {
            if(NoteEditorComponent.Model.NoteId == e.Entity.NoteId)
            {
                // TODO: !!! coger el modelo que está en memoria en lugar de volver a cargar desde la BD ??
                // NoteEditorComponent.RefreshNote(e.Entity);
                // or ...
                await NoteEditorComponent.LoadModelById(SelectedServiceRef.Service, e.Entity.NoteId);
            }
            else
                NoteEditorComponent.View.CleanView();

            NotesSelectorComponent.RefreshItem(e.Entity.GetSimpleDto<NoteInfoDto>());
        }

        private void NoteEditorComponent_DeletedEntity(object sender, ComponentEventArgs<NoteExtendedDto> e)
        {
            NotesSelectorComponent.DeleteItem(e.Entity.GetSimpleDto<NoteInfoDto>());

            if (NotesSelectorComponent.ListEntities.Count == 0)
            {
                NoteEditorComponent.View.CleanView(); 
                _selectedNoteInfo = null;                
            }
        }

        #endregion 
    }
}
