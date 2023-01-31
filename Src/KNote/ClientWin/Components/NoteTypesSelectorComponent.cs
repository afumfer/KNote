using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Components;

public class NoteTypesSelectorComponent : ComponentSelectorBase<ISelectorView<NoteTypeDto>, NoteTypeDto>
{
    #region Constructor 

    public NoteTypesSelectorComponent(Store store): base(store)
    {
        ComponentName = "Note type selector";
    }

    #endregion

    #region ISelectorView

    protected override ISelectorView<NoteTypeDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    #endregion

    #region Componente override

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
