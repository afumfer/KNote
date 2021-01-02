using KNote.Model;
using KNote.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KNote.ClientWin.Core
{
    abstract public class ComponentViewBase<TView> : ComponentBase
        where TView : IViewConfigurable
    {
        #region Properties

        private TView _view;
        public TView View
        {
            get
            {
                if (_view == null)
                {
                    _view = CreateView();
                }
                return _view;
            }
        }

        #endregion

        #region Constructor

        public ComponentViewBase(Store store) : base(store)
        {

        }

        #endregion

        #region Component methods

        protected abstract TView CreateView();

        protected override Result<EComponentResult> OnInitialized()
        {
            var result = base.OnInitialized();

            // TODO: pending check result correctrly

            View.RefreshView();

            if (!EmbededMode)
                View.ConfigureWindowMode();
            else
                View.ConfigureEmbededMode();

            return result;
        }

        public override Result<EComponentResult> Run()
        {
            Result<EComponentResult> result;

            try
            {
                result = base.Run();                
                View.ShowView();
            }
            catch (Exception ex)
            {
                result = new Result<EComponentResult>(EComponentResult.Error);
                result.AddErrorMessage(ex.Message);
            }

            return result;
        }

        public virtual Result<EComponentResult> RunModal()
        {
            Result<EComponentResult> result;

            try
            {
                result = base.Run();
                var resultView = View.ShowModalView();
                result = resultView;
            }
            catch (Exception ex)
            {
                result = new Result<EComponentResult>(EComponentResult.Error);
                result.AddErrorMessage(ex.Message);
            }

            return result;
        }

        public Result<EComponentResult> DialogResultToComponentResult(DialogResult dialogResult)
        {
            var result = new Result<EComponentResult>();
            if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes)
                result.Entity = EComponentResult.Executed;
            else
                result.Entity = EComponentResult.Canceled;
            return result;
        }

        #endregion 
    }

    abstract public class ComponentSelectorBase<TView, TEntity> : ComponentViewBase<TView>
        where TView : IViewConfigurable
    {
        #region Properties

        public IKntService Service { get; protected set; }

        public TEntity SelectedEntity { get; set; }

        public List<TEntity> ListEntities { get; protected set; }

        #endregion

        #region Constructor

        public ComponentSelectorBase(Store store) : base(store)
        {

        }

        #endregion

        #region Component virtual / abstract methods

        public abstract Task<bool> LoadEntities(IKntService service, bool refreshView = true);

        public virtual void Accept()
        {
            try
            {
                NotifySelectedEntity();  
                OnEntitySelection(SelectedEntity);
                Finalize();
            }
            catch (Exception)
            {
                OnStateComponentChanged(EComponentState.Error);
            }
        }

        public virtual void Cancel()
        {
            OnEntitySelectionCanceled(SelectedEntity);
            Finalize();
        }

        public virtual void NotifySelectedEntity()
        {
            OnEntitySelection(SelectedEntity);
        }

        public virtual void NotifySelectedEntityDoubleClick()
        {
            OnEntitySelectionDoubleClick(SelectedEntity);
        }

        public abstract void SelectItem(TEntity item);

        public abstract void RefreshItem(TEntity item);
        
        public abstract void AddItem(TEntity item);

        public abstract void DeleteItem(TEntity item);

        #endregion 

        #region Component events 

        public event EventHandler<ComponentEventArgs<TEntity>> EntitySelection;
        protected virtual void OnEntitySelection(TEntity entity)
        {
            EntitySelection?.Invoke(this, new ComponentEventArgs<TEntity>(entity));
        }

        public event EventHandler<ComponentEventArgs<TEntity>> EntitySelectionDoubleClick;
        protected virtual void OnEntitySelectionDoubleClick(TEntity entity)
        {
            EntitySelectionDoubleClick?.Invoke(this, new ComponentEventArgs<TEntity>(entity));
        }

        public event EventHandler<ComponentEventArgs<TEntity>> EntitySelectionCanceled;
        protected virtual void OnEntitySelectionCanceled(TEntity entity)
        {
            EntitySelectionCanceled?.Invoke(this, new ComponentEventArgs<TEntity>(entity));
        }

        #endregion 
    }

    abstract public class ComponentEditorBase<TView, TEntity> : ComponentViewBase<TView>
        where TView : IViewConfigurable
        where TEntity : DtoModelBase, new()        
    {
        #region Prperties

        private TEntity _model;
        public TEntity Model
        {
            protected set
            {
                _model = value;
            }
            get
            {
                if (_model == null)
                    _model = new TEntity();
                return _model;
            }
        }

        public IKntService Service { get; protected set; }

        public bool AutoDBSave { get; set; } = true;

        #endregion

        #region Constructor

        public ComponentEditorBase(Store store) : base(store)
        {

        }

        #endregion 

        #region Component virtual / abstract methods

        public abstract Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true);

        public abstract void NewModel(IKntService service);

        public abstract Task<bool> SaveModel();

        public abstract Task<bool> DeleteModel(IKntService service, Guid id);

        public abstract Task<bool> DeleteModel();

        public virtual void CancelEdition()
        {
            OnEditionCanceled(Model);
            Finalize();
        }

        public virtual void LoadModel(IKntService service, TEntity entity, bool refreshView = true)
        {
            try
            {
                Service = service;
                Model = entity;
                Model.SetIsDirty(false);
                if (refreshView)
                    View.RefreshView();
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
            }
        }

        public string GetOrSaveTmpFile(string container, string fileName, byte[] arrayContent)
        {
            string tmpFile;
            try
            {
                //var dirPath = Path.Combine(new string[] { Path.GetTempPath(), "KeyNote", container });
                var dirPath = Path.Combine(new string[] { Store.Config.CacheResources, container });                
                tmpFile = Path.Combine(dirPath, fileName);

                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                if (!File.Exists(tmpFile))
                    File.WriteAllBytes(tmpFile, arrayContent);

            }
            catch (Exception)
            {
                tmpFile = null;
            }    

            return tmpFile;
        }

        #endregion 

        #region Component events

        public event EventHandler<ComponentEventArgs<TEntity>> SavedEntity;
        protected virtual void OnSavedEntity(TEntity entity)
        {
            SavedEntity?.Invoke(this, new ComponentEventArgs<TEntity>(entity));
        }

        public event EventHandler<ComponentEventArgs<TEntity>> AddedEntity;
        protected virtual void OnAddedEntity(TEntity entity)
        {
            AddedEntity?.Invoke(this, new ComponentEventArgs<TEntity>(entity));
        }

        public event EventHandler<ComponentEventArgs<TEntity>> DeletedEntity;
        protected virtual void OnDeletedEntity(TEntity entity)
        {
            DeletedEntity?.Invoke(this, new ComponentEventArgs<TEntity>(entity));
        }

        public event EventHandler<ComponentEventArgs<TEntity>> EditionCanceled;
        protected virtual void OnEditionCanceled(TEntity entity)
        {
            EditionCanceled?.Invoke(this, new ComponentEventArgs<TEntity>(entity));
        }


        #endregion 
    }
}
