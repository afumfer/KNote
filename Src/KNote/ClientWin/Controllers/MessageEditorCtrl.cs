﻿using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class MessageEditorCtrl : CtrlEditorBase<IViewEditor<KMessageDto>, KMessageDto>
{
    #region Constructor 

    public MessageEditorCtrl(Store store): base(store)
    {
        ControllerName = "Message / alarm editor";
    }

    #endregion

    #region Abstract member implementations 

    protected override IViewEditor<KMessageDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    public async override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        try
        {
            Service = service;

            var res = await Service.Notes.GetMessageAsync(id);
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

    public override Task<bool> NewModel(IKntService service)
    {
        Service = service;

        // TODO: call service for new model
        Model = new KMessageDto();
        Model.KMessageId = Guid.NewGuid();
        return Task.FromResult(true);
    }

    public override async Task<bool> SaveModel()
    {
        View.RefreshModel();
        
        if (!Model.IsDirty())
            return true;

        var isNew = (Model.KMessageId == Guid.Empty);

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        try
        {
            Result<KMessageDto> response;
            if (AutoDBSave)
            {
                response = await Service.Notes.SaveMessageAsync(Model, true);
                Model = response.Entity;
                Model.SetIsDirty(false);
            }
            else
            {
                response = new Result<KMessageDto>();
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
        var result = View.ShowInfo("Are you sure you want to delete this alert/message?", "Delete message", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes || result == DialogResult.Yes)
        {
            try
            {
                Result<KMessageDto> response;
                if (AutoDBSave)
                    response = await service.Notes.DeleteMessageAsync(id);
                else
                {
                    var resGet = await service.Notes.GetMessageAsync(id);
                    if (!resGet.IsValid)
                    {
                        response = new Result<KMessageDto>();
                        response.Entity = new KMessageDto();
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
        return await DeleteModel(Service, Model.KMessageId);
    }

    #endregion 
}
