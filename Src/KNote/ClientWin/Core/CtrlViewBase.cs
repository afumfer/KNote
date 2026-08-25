using KNote.ClientWin.Controllers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

abstract public class CtrlViewBase<TView> : CtrlBase
    where TView : IViewBase
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

    public CtrlViewBase(Store store) : base(store)
    {

    }

    #endregion

    #region Controller methods

    protected abstract TView CreateView();

    public override Result<EControllerResult> Run()
    {
        Result<EControllerResult> result;

        try
        {
            result = base.Run();
            View.ShowView();
        }
        catch (Exception ex)
        {
            result = new Result<EControllerResult>(EControllerResult.Error);
            result.AddErrorMessage(ex.Message);
        }

        return result;
    }

    public virtual Result<EControllerResult> RunModal()
    {
        Result<EControllerResult> result;

        try
        {
            result = base.Run();
            var resultView = View.ShowModalView();
            result = resultView;
        }
        catch (Exception ex)
        {
            result = new Result<EControllerResult>(EControllerResult.Error);
            result.AddErrorMessage(ex.Message);
        }

        return result;
    }

    #endregion 
}

abstract public class CtrlViewEmbeddableBase<TView> : CtrlViewBase<TView>
    where TView : IViewEmbeddable
{
    public CtrlViewEmbeddableBase(Store store) : base(store)
    {

    }
   
    protected override Result<EControllerResult> OnInitialized()
    {                
        if (!EmbededMode)
            View.ConfigureWindowMode();
        else
            View.ConfigureEmbededMode();

        var result = base.OnInitialized();

        View.RefreshView();

        return result;
    }

}

abstract public class CtrlSelectorBase<TView, TEntity> : CtrlViewEmbeddableBase<TView>
    where TView : IViewEmbeddable
{
    #region Properties

    public IKntService Service { get; protected set; }

    public TEntity SelectedEntity { get; set; }
    
    public List<TEntity> ListEntities { get; protected set; }

    public Dictionary<string, ExtensionsEventHandler<TEntity>> Extensions { get; set; } =
        new Dictionary<string, ExtensionsEventHandler<TEntity>>();

    #endregion

    #region Constructor

    public CtrlSelectorBase(Store store) : base(store)
    {

    }

    #endregion

    #region Controller virtual / abstract methods

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
            OnStateControllerChanged(EControllerState.Error);
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

    #region Controller events 

    public event EventHandler<ControllerEventArgs<TEntity>> EntitySelection;
    protected virtual void OnEntitySelection(TEntity entity)
    {
        EntitySelection?.Invoke(this, new ControllerEventArgs<TEntity>(entity));
    }

    public event EventHandler<ControllerEventArgs<TEntity>> EntitySelectionDoubleClick;
    protected virtual void OnEntitySelectionDoubleClick(TEntity entity)
    {
        EntitySelectionDoubleClick?.Invoke(this, new ControllerEventArgs<TEntity>(entity));
    }

    public event EventHandler<ControllerEventArgs<TEntity>> EntitySelectionCanceled;
    protected virtual void OnEntitySelectionCanceled(TEntity entity)
    {
        EntitySelectionCanceled?.Invoke(this, new ControllerEventArgs<TEntity>(entity));
    }

    #endregion 
}

/// <summary>
/// Base for the "manage list" family: an embeddable list of entities (typically hosted in a tab) with
/// add/edit/delete actions that persist immediately against the service, e.g. repository administration
/// screens (Users, Note types, Attributes) inside RepositoryEditorCtrl. Unlike CtrlSelectorBase, there is
/// no "pick one and return it" semantics; unlike CtrlEditorBase, there is no single Model being edited —
/// each add/edit/delete is a self-contained, immediately persisted operation, usually delegated to a
/// CtrlEditorBase-derived popup editor for the single-entity form.
/// </summary>
abstract public class CtrlManageListBase<TView, TEntity> : CtrlViewEmbeddableBase<TView>
    where TView : IViewEmbeddable
{
    #region Properties

    public IKntService Service { get; protected set; }

    public List<TEntity> ListEntities { get; protected set; } = new();

    #endregion

    #region Constructor

    public CtrlManageListBase(Store store) : base(store)
    {

    }

    #endregion

    #region Controller abstract methods

    public abstract Task<bool> LoadEntitiesAsync(IKntService service, bool refreshView = true);

    public abstract Task<bool> AddItemAsync();

    public abstract Task<bool> EditItemAsync(TEntity item);

    public abstract Task<bool> DeleteItemAsync(TEntity item);

    #endregion
}

abstract public class CtrlEditorBase<TView, TEntity> : CtrlViewBase<TView>
    where TView : IViewBase
    where TEntity : SmartModelDtoBase, new()
{
    #region Properties

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

    public ServiceRef ServiceRef
    {
        get
        {
            if (Service == null)
                return null;
            var service = Store.GetServiceRef(Service.IdServiceRef);
            return service;
        }
    }

    public bool AutoDBSave { get; set; } = true;

    #endregion

    #region Constructor

    public CtrlEditorBase(Store store) : base(store)
    {

    }

    #endregion 

    #region Controller virtual / abstract methods

    public abstract Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true);

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

    public abstract Task<bool> NewModel(IKntService service);

    public abstract Task<bool> SaveModel();

    public abstract Task<bool> DeleteModel(IKntService service, Guid id);

    public abstract Task<bool> DeleteModel();

    public virtual void CancelEdition()
    {
        OnEditionCanceled(Model);
        Finalize();
    }

    protected override Result<EControllerResult> OnInitialized()
    {
        var result = base.OnInitialized();
        
        View.RefreshView();

        return result;                  
    }

    #endregion 

    #region Controller events

    public event EventHandler<ControllerEventArgs<TEntity>> SavedEntity;
    protected virtual void OnSavedEntity(TEntity entity)
    {
        SavedEntity?.Invoke(this, new ControllerEventArgs<TEntity>(entity));
        Store.Events.Publish(new EntitySaved<TEntity>(entity));
    }

    public event EventHandler<ControllerEventArgs<TEntity>> AddedEntity;
    protected virtual void OnAddedEntity(TEntity entity)
    {
        AddedEntity?.Invoke(this, new ControllerEventArgs<TEntity>(entity));
        Store.Events.Publish(new EntityAdded<TEntity>(entity));
    }

    public event EventHandler<ControllerEventArgs<TEntity>> DeletedEntity;
    protected virtual void OnDeletedEntity(TEntity entity)
    {
        DeletedEntity?.Invoke(this, new ControllerEventArgs<TEntity>(entity));
        Store.Events.Publish(new EntityDeleted<TEntity>(entity));
    }

    public event EventHandler<ControllerEventArgs<TEntity>> EditionCanceled;
    protected virtual void OnEditionCanceled(TEntity entity)
    {
        EditionCanceled?.Invoke(this, new ControllerEventArgs<TEntity>(entity));
    }

    #endregion 
}

abstract public class CtrlNoteEditorBase<TView, TEntity> : CtrlEditorBase<TView, TEntity>
    where TView : IViewBase
    where TEntity : SmartModelDtoBase, new()
{

    #region Constructor

    public CtrlNoteEditorBase(Store store) : base(store)
    {

    }

    #endregion 

    public virtual FolderInfoDto GetFolder()
    {
        var folderSelector = new FoldersSelectorCtrl(Store);
        var services = new List<ServiceRef>();
        services.Add(Store.GetServiceRef(Service.IdServiceRef));
        folderSelector.ServicesRef = services;
        var res = folderSelector.RunModal();
        if (res.Entity == EControllerResult.Executed)
            return folderSelector.SelectedEntity?.FolderInfo;

        return null;
    }
}


public abstract class CtrlNoteEditorEmbeddableBase<TView, TEntity> : CtrlNoteEditorBase<TView, TEntity>
    where TView : IViewEmbeddable
    where TEntity : SmartModelDtoBase, new()
{
    public CtrlNoteEditorEmbeddableBase(Store store) : base(store)
    {

    }

    protected override Result<EControllerResult> OnInitialized()
    {        
        if (!EmbededMode)
            View.ConfigureWindowMode();
        else
            View.ConfigureEmbededMode();

        var result = base.OnInitialized();

        return result;
    }
}