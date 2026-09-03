using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class RepositoryEditorCtrl : CtrlEditorBase<IViewEditor<RepositoryRef>, RepositoryRef>
{
    #region Properties 

    public EnumRepositoryEditorMode EditorMode { get; set; }

    /// <summary>
    /// Whether the current Windows user (Store.AppUserName) has the Admin role in the repository being
    /// managed. Only meaningful in EnumRepositoryEditorMode.Managment (a repository must already be
    /// linked to have a Users table to check against); gates the Users/Note types/Attributes tabs.
    /// </summary>
    public bool CurrentUserIsAdmin { get; private set; }

    #endregion

    #region Sub-controllers (repository administration tabs)

    private NoteTypesManageCtrl _noteTypesManageCtrl;
    public NoteTypesManageCtrl NoteTypesManageCtrl
    {
        get
        {
            if (_noteTypesManageCtrl == null)
            {
                _noteTypesManageCtrl = new NoteTypesManageCtrl(Store);

                // A note type rename/add/delete can make the Attributes tab's "Note type" column
                // stale (it displays KAttributeInfoDto.NoteTypeDto.Name from whenever that list was
                // last loaded), so reload it whenever the Note types list changes.
                _noteTypesManageCtrl.ListChanged += async (s, e) => await KAttributesManageCtrl.LoadEntitiesAsync(Service);
            }
            return _noteTypesManageCtrl;
        }
    }

    private KAttributesManageCtrl _kAttributesManageCtrl;
    public KAttributesManageCtrl KAttributesManageCtrl => _kAttributesManageCtrl ??= new KAttributesManageCtrl(Store);

    private UsersManageCtrl _usersManageCtrl;
    public UsersManageCtrl UsersManageCtrl => _usersManageCtrl ??= new UsersManageCtrl(Store);

    private TraceNoteTypesManageCtrl _traceNoteTypesManageCtrl;
    public TraceNoteTypesManageCtrl TraceNoteTypesManageCtrl => _traceNoteTypesManageCtrl ??= new TraceNoteTypesManageCtrl(Store);

    #endregion

    #region Constructor 

    public RepositoryEditorCtrl(Store store) : base(store)
    {
        ControllerName = "Repository editor";
    }

    #endregion

    #region Controller editor implementation 

    protected override IViewEditor<RepositoryRef> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<RepositoryEditorCtrl, IViewEditor<RepositoryRef>>(this);
    }

    public async override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        try
        {
            Service = service;

            var repositoryForEdit = Store.GetServiceRef(id).RepositoryRef;

            Model.Alias = repositoryForEdit.Alias;
            Model.ConnectionString = repositoryForEdit.ConnectionString;
            Model.Provider = repositoryForEdit.Provider;
            Model.Orm = repositoryForEdit.Orm;
            Model.ResourcesContainer = repositoryForEdit.ResourcesContainer;
            Model.ResourceContentInDB = repositoryForEdit.ResourceContentInDB;
            Model.ResourcesContainerRootPath = repositoryForEdit.ResourcesContainerRootPath;
            Model.ResourcesContainerRootUrl = repositoryForEdit.ResourcesContainerRootUrl;
            Model.SetIsDirty(false);

            CurrentUserIsAdmin = EditorMode == EnumRepositoryEditorMode.Managment
                && await Store.IsCurrentUserAdminAsync(service);

            if (CurrentUserIsAdmin)
            {
                await NoteTypesManageCtrl.LoadEntitiesAsync(service);
                await KAttributesManageCtrl.LoadEntitiesAsync(service);
                await UsersManageCtrl.LoadEntitiesAsync(service);
                await TraceNoteTypesManageCtrl.LoadEntitiesAsync(service);
            }

            if (refreshView)
                View.RefreshView();
            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }

    public override Task<bool> NewModel(IKntService service = null)
    {
        Service = service;

        Model = new RepositoryRef();

        // AddLink/Create modes: the repository isn't linked yet, so there's no Users table to check
        // the current user's role against - the admin tabs stay disabled regardless.
        CurrentUserIsAdmin = false;

        return Task.FromResult(true);
    }

    public async override Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty())
            return true;

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        try
        {
            if (EditorMode == EnumRepositoryEditorMode.Managment)
            {
                var repositoryForEdit = Store.GetServiceRef(Service.IdServiceRef).RepositoryRef;
                repositoryForEdit.Alias = Model.Alias;
                repositoryForEdit.ConnectionString = Model.ConnectionString ;
                repositoryForEdit.Provider = Model.Provider ;
                repositoryForEdit.Orm = Model.Orm;
                repositoryForEdit.ResourcesContainer = Model.ResourcesContainer;
                repositoryForEdit.ResourceContentInDB = Model.ResourceContentInDB;
                repositoryForEdit.ResourcesContainerRootPath = Model.ResourcesContainerRootPath;
                repositoryForEdit.ResourcesContainerRootUrl = Model.ResourcesContainerRootUrl;
                Model.SetIsDirty(false);
                Store.SaveConfig();
                OnSavedEntity(Model);
            }

            else if (EditorMode == EnumRepositoryEditorMode.AddLink)
            {                    
                // Add link repository
                var newService = new ServiceRef(Model, Store.AppUserName, false, Store.Logger);                    
                if (await newService.Service.TestDbConnection())
                {
                    Store.AddServiceRef(newService);
                    Store.AddServiceRefInAppConfig(newService);
                    Model.SetIsDirty(false);
                    Store.SaveConfig();
                    await Store.EnsureCurrentUserRegistered(newService.Service);
                    OnAddedEntity(Model);
                }
                else
                {
                    View.ShowInfo("Invalid database.");
                    return false;
                }
            }

            else if (EditorMode == EnumRepositoryEditorMode.Create)
            {
                // Create repository and add link                    
                var newService = new ServiceRef(Model, Store.AppUserName, false, Store.Logger);
                if (await newService.Service.CreateDataBase(SystemInformation.UserName))
                {
                    Store.AddServiceRef(newService);
                    Store.AddServiceRefInAppConfig(newService);
                    Model.SetIsDirty(false);
                    Store.SaveConfig();
                    OnAddedEntity(Model);
                }
                else
                {
                    View.ShowInfo("Can't create database.");
                    return false;
                }
            }

            Finalize();
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }

        return true;
    }

    public async override Task<bool> DeleteModel()
    {
        return await DeleteModel(Service, Service.IdServiceRef);
    }

    public async override Task<bool> DeleteModel(IKntService service, Guid id)
    {            
        Service = service;
        var serviceForDelete = Store.GetServiceRef(id);

        var result = View.ShowInfo($"Are you sure you want remove {serviceForDelete?.RepositoryRef.Alias} repository link?", "Delete note", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes || result == DialogResult.Yes)
        {
            try
            {
                await Store.SaveAndCloseActiveNotes(service.IdServiceRef);
                Store.RemoveServiceRef(serviceForDelete);
                Store.SaveConfig();
                OnDeletedEntity(serviceForDelete.RepositoryRef);
                return true;
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
            }
        }
        return false;
    }

    #endregion 
}

#region Public enums 

public enum EnumRepositoryEditorMode
{
    AddLink,
    Create,
    Managment
}

#endregion