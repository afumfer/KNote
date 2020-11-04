using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Components
{
    public class KNoteManagmentComponent : ComponentViewBase<IViewBase>
    {
        public KNoteManagmentComponent(Store store) : base(store)
        {

        }

        #region Views

        protected override IViewBase CreateView()
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

        protected override Result OnInitialized()
        {
            var result = base.OnInitialized();

            try
            {
                View.ShowView();

                NotesSelectorComponent.Run();
                FoldersSelectorComponent.Run();
                
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

        //----------------------

        #region Components included

        #region FoldersSelector component

        private FolderSelectorComponent _folderSelectorComponent;
        public FolderSelectorComponent FoldersSelectorComponent
        {
            get
            {
                if (_folderSelectorComponent == null)
                {
                    _folderSelectorComponent = new FolderSelectorComponent(Store);
                    _folderSelectorComponent.EmbededMode = true;
                    _folderSelectorComponent.ModalMode = false;
                    
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

            NotesSelectorComponent.LoadNotesByFolderAsync(e.Entity);

            //if (_notesSelectorComponent != null)
            //    _notesSelectorComponent.LoadNotesByFolderAsync(e.Entity);
        }

        //private void _folderSelectorCtrl_EntityChange(object sender, EntityEventArgs<FolderWithServiceRef> e)
        //{
        //    this.Context.UpdateActiveFolder(e.Entity);

        //    // TODO: la siguiente línea se podría re-implementar mediante la ecucha
        //    //       de evento de contexto por parte de la vista. 
        //    View.RefreshView();
        //    var noteSet = new NotesSet
        //    {
        //        ServiceRef = e.Entity.ServiceRef,
        //        NotesFilter = new NotesFilter
        //        {
        //            Folder = e.Entity.FolderInfo
        //        }
        //    };

        //    NotesSelectorCtrl.RefreshCtrl(noteSet);

        //    var selectorCount = NotesSelectorCtrl.ListEntities.Count;
        //    View.ShowInfo("Notes: " + selectorCount.ToString());

        //    if (selectorCount == 0)
        //        NoteEditorCtrl.UpdateModelCtrl(Context.ActiveFolderWithServiceRef.ServiceRef.Service,
        //            null);
        //    else
        //        NoteEditorCtrl.UpdateModelCtrl(Context.ActiveFolderWithServiceRef.ServiceRef.Service,
        //            NotesSelectorCtrl.ListEntities[0].NoteId);
        //}

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
                    _notesSelectorComponent.ModalMode = false;

                    _notesSelectorComponent.EntitySelection += _notesSelectorComponent_EntitySelection;

                }
                return _notesSelectorComponent;
            }
        }

        private void _notesSelectorComponent_EntitySelection(object sender, ComponentEventArgs<NoteInfoDto> e)
        {
            
        }

        //private void _notesSelectorCtrl_EntityChange(object sender, EntityEventArgs<NoteItemDto> e)
        //{
        //    NoteEditorCtrl.UpdateModelCtrl(Context.ActiveFolderWithServiceRef.ServiceRef.Service, e.Entity?.NoteId);
        //}

        #endregion

        #region NoteEditor component

        //private NoteEditorCtrl _noteEditorCtrl;
        //public NoteEditorCtrl NoteEditorCtrl
        //{
        //    get
        //    {
        //        if (_noteEditorCtrl == null)
        //        {
        //            _noteEditorCtrl = new NoteEditorCtrl(this.Context);
        //            _noteEditorCtrl.EmbededMode = true;
        //        }
        //        return _noteEditorCtrl;
        //    }
        //}

        #endregion

        #endregion

        #region Public methods

        public void ShowKntScriptConsole()
        {
            //try
            //{
            //    string sourceScript = "";
            //    AnTSEngine.ShowConsole(sourceScript, new KntTestServiceLibrary());
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        #endregion 


        //---------------------
    }
}
