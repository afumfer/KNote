﻿using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Components;

public class NoteAttributeEditorComponent : ComponentEditorBase<IViewEditor<NoteKAttributeDto>, NoteKAttributeDto>
{
    #region Constructor 

    public NoteAttributeEditorComponent(Store store): base (store)
    {
        ComponentName = "Note attribute editor";
    }

    #endregion

    #region IEditorView

    protected override IViewEditor<NoteKAttributeDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    #endregion

    #region Component override methods

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public override async Task<bool> NewModel(IKntService service)
    {
        Service = service;

        // TODO: call service for new model
        Model = new NoteKAttributeDto();
        return await Task.FromResult<bool>(true);
    }

    public async override Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty())
            return await Task.FromResult<bool>(true);

        var isNew = (Model.NoteKAttributeId == Guid.Empty);

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return await Task.FromResult<bool>(false);
        }

        try
        {
            Result<NoteKAttributeDto> response;
            if (AutoDBSave)
            {
                throw new NotImplementedException();
                //response = await Service.Xxxxx.SaveAsync(Model);
                //Model = response.Entity;
                //Model.SetIsDirty(false);
            }
            else
            {
                response = new Result<NoteKAttributeDto>();
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
            return await Task.FromResult<bool>(false);
        }

        return await Task.FromResult<bool>(true);
    }

    public override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DeleteModel()
    {
        throw new NotImplementedException();
    }

    #endregion 
}
