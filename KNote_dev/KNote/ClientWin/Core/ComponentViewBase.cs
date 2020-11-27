using KNote.Model;
using KNote.Service;
using System;
using System.Collections.Generic;
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

        protected override Result OnInitialized()
        {
            var result = base.OnInitialized();

            View.RefreshView();

            if (!EmbededMode)
                View.ConfigureWindowMode();
            else
                View.ConfigureEmbededMode();

            return result;
        }

        public override Result Run()
        {
            var result = base.Run();

            // TODO:  Check result here 
            // ...

            View.ShowView();
            return result;
        }

        public virtual Result<EComponentResult> RunModal()
        {
            var result = base.Run();

            // TODO:  Check result here 
            // ...

            var resultView = View.ShowModalView();

            return resultView;
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

        public TEntity SelectedEntity { get; set; }

        #endregion

        #region Constructor

        public ComponentSelectorBase(Store store) : base(store)
        {

        }

        #endregion 

        #region Component virtual methods

        public virtual void CancelAction()
        {
            OnComponentResultChanged(EComponentResult.Canceled);
            base.Finalize();
        }

        public virtual void AcceptAction()
        {
            try
            {
                NotifySelectedEntity();
                OnComponentResultChanged(EComponentResult.Executed);
                base.Finalize();
            }
            catch (Exception)
            {
                OnStateComponentChanged(EComponentState.Error);
            }
        }

        #endregion 

        #region Component events 

        public event EventHandler<ComponentEventArgs<TEntity>> EntitySelection;
        protected virtual void OnEntitySelection(TEntity entity)
        {
            EntitySelection?.Invoke(this, new ComponentEventArgs<TEntity>(entity));
        }

        public virtual void NotifySelectedEntity()
        {
            OnEntitySelection(SelectedEntity);
        }

        public event EventHandler<ComponentEventArgs<TEntity>> EntitySelectionDoubleClick;
        protected virtual void OnEntitySelectionDoubleClick(TEntity entity)
        {
            EntitySelectionDoubleClick?.Invoke(this, new ComponentEventArgs<TEntity>(entity));
        }

        public virtual void NotifySelectedEntityDoubleClick()
        {
            OnEntitySelectionDoubleClick(SelectedEntity);
        }

        #endregion 
    }

    abstract public class ComponentEditorBase<TView, TEntity> : ComponentViewBase<TView>
        where TView : IViewConfigurable
        where TEntity : class, new()        
    {
        #region Prperties

        private TEntity _model;
        public TEntity Model
        {
            set
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

        protected IKntService Service { get; set; }

        #endregion

        #region Constructor

        public ComponentEditorBase(Store store) : base(store)
        {

        }

        #endregion 

        #region Abstract methods

        public abstract void LoadModelById(IKntService service, Guid noteId);

        public abstract void NewModel(IKntService service);

        public abstract void SaveModel();

        public abstract Task<bool> DeleteModel(IKntService service, Guid noteId);

        public abstract Task<bool> DeleteModel();

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

        #endregion 
    }
}
