using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using System.ComponentModel.DataAnnotations;
using KNote.Service.Core;

namespace KNote.ClientWin.Components
{
    public enum EnumRepositoryEditorMode
    {
        AddLink,
        Create,
        Managment
    }

    public class RepositoryEditorComponent : ComponentEditor<IEditorView<RepositoryRef>, RepositoryRef>
    {
        #region Constructor 

        public EnumRepositoryEditorMode EditorMode { get; set; }


        public RepositoryEditorComponent(Store store) : base(store)
        {
            ComponentName = "Repository editor";
        }

        #endregion

        #region ComponentEditor implementation 

        protected override IEditorView<RepositoryRef> CreateView()
        {
            return Store.FactoryViews.View(this);
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
                Model.ResourcesContainerCacheRootPath = repositoryForEdit.ResourcesContainerCacheRootPath;
                Model.ResourcesContainerCacheRootUrl = repositoryForEdit.ResourcesContainerCacheRootUrl;
                Model.SetIsDirty(false);
                
                if (refreshView)
                    View.RefreshView();
                return await Task.FromResult<bool>(true);
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
                return await Task.FromResult<bool>(false);
            }
        }

        public async override Task<bool> NewModel(IKntService service = null)
        {
            Service = service;

            Model = new RepositoryRef();

            return await Task.FromResult<bool>(true);
        }

        public async override Task<bool> SaveModel()
        {
            View.RefreshModel();

            if (!Model.IsDirty())
                return await Task.FromResult<bool>(true);

            var msgVal = Model.GetErrorMessage();
            if (!string.IsNullOrEmpty(msgVal))
            {
                View.ShowInfo(msgVal);
                return await Task.FromResult<bool>(false);
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
                    repositoryForEdit.ResourcesContainerCacheRootPath = Model.ResourcesContainerCacheRootPath;
                    repositoryForEdit.ResourcesContainerCacheRootUrl = Model.ResourcesContainerCacheRootUrl;
                    Model.SetIsDirty(false);
                    Store.SaveConfig();
                    OnSavedEntity(Model);
                }

                else if (EditorMode == EnumRepositoryEditorMode.AddLink)
                {                    
                    // Add link repository
                    var newService = new ServiceRef(Model);
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
                        return await Task.FromResult<bool>(false);
                    }
                }

                else if (EditorMode == EnumRepositoryEditorMode.Create)
                {
                    // Create repository and add link                    
                    var newService = new ServiceRef(Model);
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
                        return await Task.FromResult<bool>(false);
                    }
                }

                Finalize();
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
                return await Task.FromResult<bool>(false);
            }

            return await Task.FromResult<bool>(true);
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
                    return await Task.FromResult<bool>(true);
                }
                catch (Exception ex)
                {
                    View.ShowInfo(ex.Message);
                }
            }
            return await Task.FromResult<bool>(false);
        }

        #endregion 
    }
}
