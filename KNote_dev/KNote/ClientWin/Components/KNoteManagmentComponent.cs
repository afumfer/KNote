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

        protected override Result OnInitialized()
        {
            var result = base.OnInitialized();

            try
            {                
                NotesSelectorComponent.Run();
                FoldersSelectorComponent.Run();
                NoteEditorComponent.Run();
            }
            catch (Exception ex)
            {
                result.AddErrorMessage(ex.Message);                
            }
            
            return result;
        }

        protected override Result OnFinalized()
        {
            return base.OnFinalized();
        }

        #endregion 

        //----------------------

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



        private void _folderSelectorComponent_EntitySelection(object sender, ComponentEventArgs<FolderWithServiceRef> e)
        {
            if (e.Entity == null)
            {
                // ....    
                return;
            }

            SelectedFolderWithServiceRef = e.Entity;
            
            View.ShowInfo(null);

            NotesSelectorComponent.LoadNotesByFolderAsync(e.Entity);
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
            EditNoteAction();
        }

        private void _notesSelectorComponent_EntitySelection(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            if (e.Entity == null)
                return;
            _selectedNoteInfo = e.Entity;
            NoteEditorComponent.LoadNoteById(SelectedFolderWithServiceRef, _selectedNoteInfo.NoteId);
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

        #region Public methods

        public FolderWithServiceRef SelectedFolderWithServiceRef { get; set; }

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

        public void ShowKntScriptConsoleAction()
        {
            var kntEngine = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(Store));

            var kntScriptCom = new KntScriptConsoleComponent(Store);
            kntScriptCom.KntSEngine = kntEngine;

            kntScriptCom.Run();
        }

        public void EditNoteAction()
        {
            if (SelectedNoteInfo == null)
                return;

            var noteEditorComponent = new NoteEditorComponent(Store);
            noteEditorComponent.SavedEntity += NoteEditorComponent_SavedEntity;
            noteEditorComponent.LoadNoteById(SelectedFolderWithServiceRef, SelectedNoteInfo.NoteId);            
            noteEditorComponent.Run();
        }

        private void NoteEditorComponent_SavedEntity(object sender, ComponentEventArgs<NoteDto> e)
        {
            if (NotesSelectorComponent.Folder.FolderId == e.Entity.FolderId)
            {
                NotesSelectorComponent.RefreshNote(e.Entity.GetSimpleDto<NoteInfoDto>());
            }

            if(NoteEditorComponent.NoteEdit.NoteId == e.Entity.NoteId)
            {
                //NoteEditorComponent.RefreshNote(e.Entity);
                // o ...
                NoteEditorComponent.LoadNoteById(SelectedFolderWithServiceRef, e.Entity.NoteId);
            }
        }


        #endregion



        //---------------------
    }
}
