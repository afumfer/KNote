using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class RepositoryEditorCtrl : CtrlEditorBase<IViewEditor<RepositoryRef>, RepositoryRef>
{
    #region Properties 

    public EnumRepositoryEditorMode EditorMode { get; set; }

    /// <summary>
    /// Whether the repository administration tabs (Users, Note types, Trace note types, Attributes) apply:
    /// only to an already linked repository, i.e. in EnumRepositoryEditorMode.Management - which in turn
    /// requires the Admin role in that repository (see RequiredAuthorization).
    /// </summary>
    public bool AdministrationAvailable => EditorMode == EnumRepositoryEditorMode.Management;

    // Each mode is a different use case: linking a repository is open to anybody who signed in; creating
    // one takes an Admin somewhere (the application role); managing one takes an Admin in that repository.
    protected override KntAuthorizeAttribute RequiredAuthorization => EditorMode switch
    {
        EnumRepositoryEditorMode.Create => new KntAuthorizeAttribute(EnumRoles.Admin, AuthorizationScope.Application),
        EnumRepositoryEditorMode.Management => new KntAuthorizeAttribute(EnumRoles.Admin),
        _ => null
    };

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

            // Not loaded for a user who will be refused the management screen anyway (see RunModal).
            if (AdministrationAvailable && IsAuthorized())
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
            if (EditorMode == EnumRepositoryEditorMode.Management)
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
                    // Only linked if the session's user may use it (registered, or registering now; with
                    // the session's password, when signed in with a KNote user).
                    var authentication = await Store.AuthenticateRepositoryAsync(newService.Service);
                    if (!authentication.IsValid)
                    {
                        View.ShowInfo(authentication.ErrorMessage);
                        return false;
                    }

                    Store.AddServiceRef(newService);
                    Store.AddServiceRefInSettings(newService);
                    Model.SetIsDirty(false);
                    Store.SaveConfig();
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
                if (await newService.Service.CreateDataBase())
                {
                    // Same as for a linked repository. Being the first user registered in this new
                    // database, the current user becomes its Admin (see KntUsersRegisterAsyncCommand).
                    var authentication = await Store.AuthenticateRepositoryAsync(newService.Service);
                    if (!authentication.IsValid)
                    {
                        View.ShowInfo($"The repository has been created, but it has not been linked.{Environment.NewLine}{authentication.ErrorMessage}");
                        return false;
                    }

                    Store.AddServiceRef(newService);
                    Store.AddServiceRefInSettings(newService);
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
    Management
}

#endregion