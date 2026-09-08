using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class NoteTypesSelectorCtrl : CtrlSelectorBase<IViewEmbeddable, NoteTypeDto>
{
    #region Constructor 

    public NoteTypesSelectorCtrl(Store store): base(store)
    {
        ControllerName = "Note type selector";
    }

    #endregion

    #region ISelectorView

    protected override IViewEmbeddable CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<NoteTypesSelectorCtrl, IViewEmbeddable>(this);
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

    #endregion
}
