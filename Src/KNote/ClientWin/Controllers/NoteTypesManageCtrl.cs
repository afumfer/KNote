using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Note types tab of the repository administration screen (RepositoryEditorCtrl): lists the note
/// types of the repository being managed and delegates add/edit to the NoteTypeEditorCtrl popup.
/// </summary>
public class NoteTypesManageCtrl : CtrlManageListBase<IViewManageList<NoteTypeDto>, NoteTypeDto>
{
    #region Constructor

    public NoteTypesManageCtrl(Store store) : base(store)
    {
        ControllerName = "Note types management";
    }

    #endregion

    #region CtrlManageListBase implementation

    protected override IViewManageList<NoteTypeDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<NoteTypesManageCtrl, IViewManageList<NoteTypeDto>>(this);
    }

    public override async Task<bool> LoadEntitiesAsync(IKntService service, bool refreshView = true)
    {
        try
        {
            Service = service;

            var response = await Service.NoteTypes.GetAllAsync();

            if (response.IsValid)
            {
                ListEntities = response.Entity;

                if (refreshView)
                    View.RefreshView();

                return true;
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
    }

    public override async Task<bool> AddItemAsync()
    {
        var editorCtrl = new NoteTypeEditorCtrl(Store);
        await editorCtrl.NewModel(Service);

        var res = editorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            ListEntities.Add(editorCtrl.Model);
            View.AddItem(editorCtrl.Model);
            return true;
        }
        return false;
    }

    public override async Task<bool> EditItemAsync(NoteTypeDto item)
    {
        var editorCtrl = new NoteTypeEditorCtrl(Store);
        await editorCtrl.LoadModelById(Service, item.NoteTypeId, false);

        var res = editorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            var index = ListEntities.FindIndex(_ => _.NoteTypeId == item.NoteTypeId);
            if (index >= 0)
                ListEntities[index] = editorCtrl.Model;

            View.UpdateItem(editorCtrl.Model);
            return true;
        }
        return false;
    }

    public override async Task<bool> DeleteItemAsync(NoteTypeDto item)
    {
        var editorCtrl = new NoteTypeEditorCtrl(Store);
        var deleted = await editorCtrl.DeleteModel(Service, item.NoteTypeId);

        if (deleted)
        {
            ListEntities.RemoveAll(_ => _.NoteTypeId == item.NoteTypeId);
            View.RemoveItem(item);
        }
        return deleted;
    }

    #endregion
}
