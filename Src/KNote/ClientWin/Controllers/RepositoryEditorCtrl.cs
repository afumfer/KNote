using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class RepositoryEditorCtrl : CtrlEditorBase<IViewEditor<RepositoryRef>, RepositoryRef>
{
    #region Properties 

    public EnumRepositoryEditorMode EditorMode { get; set; }

    #endregion 

    #region Constructor 

    public RepositoryEditorCtrl(Store store) : base(store)
    {
        ControllerName = "Repository editor";
    }

    #endregion

    #region ComponentEditor implementation 

    protected override IViewEditor<RepositoryRef> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
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
            
            if (refreshView)
                View.RefreshView();
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return Task.FromResult(false);
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