using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Model;

namespace KNote.ClientWin.Core
{
    //abstract public class BaseSelectorCtrl<TDataSource, TEntity, TView> : BaseCtrl
    //    where TEntity : class, new()        
    //    where TView : ISelectorView<TEntity>
    //{
    //    #region Properties

    //    public TDataSource DataSource { get; set; }

    //    public List<TEntity> ListEntities { get; set; }

    //    public TEntity SelectedEntity { get; set; }

    //    public List<TEntity> ListMarkedEntities { get; set; }

    //    public Dictionary<string, ExtensionsEventHandler<TEntity>> Extensions { get; set; } =
    //        new Dictionary<string, ExtensionsEventHandler<TEntity>>();

    //    #endregion

    //    #region View

    //    private TView _view;
    //    public TView View
    //    {
    //        get
    //        {
    //            if (_view == null)
    //            {
    //                _view = CreateView();                    
    //            }
    //            return _view;
    //        }
    //    }

    //    #endregion 

    //    #region Constructor
       
    //    public BaseSelectorCtrl(KntContext context)
    //        : base(context)
    //    {

    //    }

    //    public BaseSelectorCtrl(KntContext context, TDataSource dataSource)
    //        : base(context)
    //    {            
    //        DataSource = dataSource;
    //    }


    //    #endregion

    //    #region Events

    //    public event EventHandler<EntityEventArgs<TEntity>> EntityChange;

    //    protected virtual void OnEntityChange(EntityEventArgs<TEntity> e)
    //    {
    //        EntityChange?.Invoke(this, e);
    //    }

    //    public event EventHandler<EntityEventArgs<TEntity>> EntitySelection;
    //    protected virtual void OnEntitySelection(EntityEventArgs<TEntity> e)
    //    {
    //        EntitySelection?.Invoke(this, e);
    //    }

    //    #endregion 

    //    #region Abstract methods

    //    protected abstract TView CreateView();
    //    protected abstract void LoadDataSelector();
    //    protected abstract void UpdateEntity(TEntity entidadAntigua, TEntity entidadNueva);

    //    #endregion

    //    #region Virtual methods

    //    protected override Result<CtrlResult> OnStart()
    //    {
    //        var result = new Result<CtrlResult>();
                                            
    //        try
    //        {                
    //            LoadDataSelector();

    //            if (ModalMode)
    //                View.ConfigureWindowMode();
    //            if (EmbededMode)
    //                View.ConfigureEmbededMode();
                                
    //            View.ShowView();

    //            result.Entity = CtrlResult;
    //        }
    //        catch (Exception ex)
    //        {                
    //            OnStateCtrlChanged(CtrlState.Error);
    //            result.Entity = CtrlResult.Error;
    //            result.AddErrorMessage(ex.Message);
    //        }

    //        return ResultControllerAction<CtrlResult>(result);
    //    }

    //    public virtual Result<CtrlResult> RefreshCtrl(TDataSource dataSource)
    //    {
    //        var result = new Result<CtrlResult>();

    //        try
    //        {
    //            DataSource = dataSource;
    //            LoadDataSelector();

    //            View.RefreshView();

    //            result.Entity = CtrlResult.Executed;
    //            OnStateCtrlChanged(CtrlState.Refresh);
    //        }
    //        catch (Exception ex)
    //        {
    //            result.Entity = CtrlResult.Error;
    //            OnStateCtrlChanged(CtrlState.Error);
    //            result.AddErrorMessage(ex.Message);
    //        }

    //        return ResultControllerAction<CtrlResult>(result);
    //    }

        
    //    public virtual void NotifyEntityChangeAction(TEntity entity)
    //    {
    //        SelectedEntity = entity;
    //        OnEntityChange(new EntityEventArgs<TEntity>(SelectedEntity));
    //    }

    //    public virtual void NotifyEntitySelectionAction(TEntity entity)
    //    {
    //        SelectedEntity = entity;
    //        OnEntitySelection(new EntityEventArgs<TEntity>(SelectedEntity));
    //    }

    //    public virtual Result<CtrlResult> CancelAction()
    //    {
    //        var result = new Result<CtrlResult>();
                                    
    //        result.Entity = CtrlResult.Canceled;
    //        CtrlResult = CtrlResult.Canceled;

    //        base.FinalizeCtrl();

    //        return ResultControllerAction<CtrlResult>(result);
    //    }

    //    public virtual Result<CtrlResult> AcceptAction()
    //    {
    //        var result = new Result<CtrlResult>();
                       
    //        try
    //        {
    //            SelectorEntityAction();

    //            result.Entity = CtrlResult.Executed;
    //            CtrlResult = CtrlResult.Executed;

    //            base.FinalizeCtrl();            
    //        }
    //        catch (Exception ex)
    //        {
    //            result.Entity = CtrlResult.Error;
    //            CtrlResult = CtrlResult.Error;
    //            OnStateCtrlChanged(CtrlState.Error);
    //            result.AddErrorMessage(ex.Message);                
    //        }

    //        return ResultControllerAction<CtrlResult>(result);
    //    }

    //    public virtual void Add(TEntity entity)
    //    {
    //        AddEntity(entity);
    //    }

    //    public virtual void Delete(TEntity entity)
    //    {
    //        DeleteEntity(entity);
    //    }

    //    public virtual void Update(TEntity entityOld, TEntity entityNew)
    //    {
    //        UpdateEntity(entityOld, entityNew);
    //    }

    //    public virtual TEntity GetCast(object identificador)
    //    {
    //        return (TEntity)(identificador);
    //    }

    //    #endregion

    //    #region Template virtual methods

    //    protected virtual void CleanDataSelector()
    //    {
    //        SelectedEntity = null;
    //        if (ListEntities != null)
    //            ListEntities.Clear();
    //    }

    //    protected virtual void AddEntity(TEntity entity)
    //    {
    //        ListEntities.Add(entity);

    //        SelectedEntity = entity;
    //        //NotifyEntitySelectionAction(entity);

    //        View.AddItem(SelectedEntity);
    //    }

    //    protected virtual void DeleteEntity(TEntity entity)
    //    {
    //        ListEntities.Remove(entity);
    //        View.DeleteItem(entity);
    //        if (ListEntities.Count == 0)
    //            NotifyEntityChangeAction(null);
    //    }

    //    protected virtual void SelectorEntityAction()
    //    {

    //    }

    //    #endregion
    //}

}
