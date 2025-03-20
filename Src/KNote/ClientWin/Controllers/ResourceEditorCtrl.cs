using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class ResourceEditorCtrl : CtrlEditorBase<IViewEditor<ResourceDto>, ResourceDto>
{
    #region Constructor 

    public ResourceEditorCtrl(Store store): base(store)
    {
        ControllerName = "Resource editor";        
    }

    #endregion

    #region Abstract members implementations

    protected override IViewEditor<ResourceDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    public override Task<bool> NewModel(IKntService service)
    {
        Service = service;

        // TODO: call service for new model
        Model = new ResourceDto();
        Model.ResourceId = Guid.NewGuid();
        Model.ContentInDB = false;
        return Task.FromResult(true);
    }

    public async override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        try
        {
            Service = service;

            var res = await Service.Notes.GetResourceAsync(id);
            if (!res.IsValid)
                return false;

            Model = res.Entity;
            Model.SetIsDirty(false);
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

    public async override Task<bool> SaveModel()
    {
        View.RefreshModel();            

        if (!Model.IsDirty())
            return true;

        var isNew = (Model.ResourceId == Guid.Empty);

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        try
        {
            Result<ResourceDto> response;
            if (AutoDBSave)
            {
                response = await Service.Notes.SaveResourceAsync(Model, true);
                Model = response.Entity;
                Model.SetIsDirty(false);
            }
            else
            {
                response = new Result<ResourceDto>();
                Model.SetIsDirty(true);
                response.Entity = Model;
            }

            if (response.IsValid)
            {
                if (!isNew)
                    OnSavedEntity(response.Entity);
                else
                    OnAddedEntity(response.Entity);

                Finalize();
            }
            else
                View.ShowInfo(response.ErrorMessage);
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }

        return true;
    }

    public async override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        var result = View.ShowInfo("Are you sure you want to delete this resource?", "Delete resource", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes || result == DialogResult.Yes)
        {
            try
            {
                Result<ResourceDto> response;
                if (AutoDBSave)
                    response = await service.Notes.DeleteResourceAsync(id);
                else
                {                        
                    var resGet = await service.Notes.GetResourceAsync(id);
                    if (!resGet.IsValid)
                    {
                        response = new Result<ResourceDto>();
                        response.Entity = new ResourceDto();
                    }
                    else
                        response = resGet;
                }

                if (response.IsValid)
                {
                    response.Entity.SetIsDeleted(true);
                    Model = response.Entity;
                    OnDeletedEntity(Model);
                    return true;
                }
                else
                    View.ShowInfo(response.ErrorMessage);
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
            }
        }
        return false;
    }

    public async override Task<bool> DeleteModel()
    {
        return await DeleteModel(Service, Model.ResourceId);
    }

    public string ExtensionFileToFileType(string extension)
    {
        return Store.ExtensionFileToFileType(extension);
    }

    #endregion

    #region Utils

    public void SaveResourceFileAndRefreshDto()
    {
        Service.Notes.UtilManageResourceContent(Model);
    }

    #endregion 
}
