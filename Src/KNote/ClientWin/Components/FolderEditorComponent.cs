using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Components;

public class FolderEditorComponent : ComponentEditorBase<IViewEditor<FolderDto>, FolderDto>
{
    #region Constructor 

    public FolderEditorComponent(Store store): base(store)
    {
        ComponentName = "Folder editor";
    }

    #endregion

    #region IEditorView

    protected override IViewEditor<FolderDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    #endregion

    #region Component override methods

    public override async Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        try
        {
            Service = service;

            Model = (await Service.Folders.GetAsync(id)).Entity;
            Model.SetIsDirty(false);
            if(refreshView)
                View.RefreshView();
            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;                
        }            
    }

    public override async Task<bool> NewModel(IKntService service)
    {            
        Service = service;

        // TODO: call service for new model
        Model = new FolderDto();
        return await Task.FromResult<bool>(true);
    }

    public override async Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty())
            return true;

        var isNew = (Model.FolderId == Guid.Empty);

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        try
        {                
            Result<FolderDto> response;
            if (AutoDBSave)
            {
                response = await Service.Folders.SaveAsync(Model);
                Model = response.Entity;
                Model.SetIsDirty(false);
            }
            else
            {
                response = new Result<FolderDto>();
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
        var result = View.ShowInfo("Are you sure you want to delete this folder?", "Delete note", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes || result == DialogResult.Yes)
        {
            try
            {
                var response = await service.Folders.DeleteAsync(id);

                if (response.IsValid)
                {
                    OnDeletedEntity(response.Entity);
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
        return await DeleteModel(Service, Model.FolderId);
    }

    #endregion
}
