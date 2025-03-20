using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class NoteTypesSelectorCtrl : CtrlSelectorBase<IViewSelector<NoteTypeDto>, NoteTypeDto>
{
    #region Constructor 

    public NoteTypesSelectorCtrl(Store store): base(store)
    {
        ControllerName = "Note type selector";
    }

    #endregion

    #region ISelectorView

    protected override IViewSelector<NoteTypeDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    #endregion

    #region Controller override

    public async override Task<bool> LoadEntities(IKntService service, bool refreshView = true)
    {
        try
        {
            Service = service;
            
            var response = await Service.NoteTypes.GetAllAsync();

            if (response.IsValid)
            {
                ListEntities = response.Entity;

                if(refreshView)
                    View.RefreshView();

                if (ListEntities?.Count > 0)
                    SelectedEntity = ListEntities[0];
                else
                    SelectedEntity = null;

                NotifySelectedEntity();
            }
            else
            {
                View.ShowInfo(response.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }

        return true;
    }

    public override void AddItem(NoteTypeDto item)
    {
        throw new NotImplementedException();
    }

    public override void DeleteItem(NoteTypeDto item)
    {
        throw new NotImplementedException();
    }

    public override void RefreshItem(NoteTypeDto item)
    {
        throw new NotImplementedException();
    }

    public override void SelectItem(NoteTypeDto item)
    {
        throw new NotImplementedException();
    }

    #endregion
}
