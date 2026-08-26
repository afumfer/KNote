using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Attributes tab of the repository administration screen (RepositoryEditorCtrl): lists the custom
/// attributes of the repository being managed (across all note types, like the Blazor admin panel's
/// flat Attributes grid) and delegates add/edit to the AttributeEditorCtrl popup.
/// </summary>
public class KAttributesManageCtrl : CtrlManageListBase<IViewManageList<KAttributeInfoDto>, KAttributeInfoDto>
{
    #region Constructor

    public KAttributesManageCtrl(Store store) : base(store)
    {
        ControllerName = "Attributes management";
    }

    #endregion

    #region CtrlManageListBase implementation

    protected override IViewManageList<KAttributeInfoDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<KAttributesManageCtrl, IViewManageList<KAttributeInfoDto>>(this);
    }

    public override async Task<bool> LoadEntitiesAsync(IKntService service, bool refreshView = true)
    {
        try
        {
            Service = service;

            var response = await Service.KAttributes.GetAllAsync();

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
        var editorCtrl = new AttributeEditorCtrl(Store);
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

    public override async Task<bool> EditItemAsync(KAttributeInfoDto item)
    {
        var editorCtrl = new AttributeEditorCtrl(Store);
        // refreshView must stay true here: unlike embedded-only forms, this popup is shown via
        // RunModal() right after, and there is no Form_Load wiring to populate it later - without
        // this, the dialog opens with the fields blank even though Model has the loaded entity.
        await editorCtrl.LoadModelById(Service, item.KAttributeId, true);

        var res = editorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            var index = ListEntities.FindIndex(_ => _.KAttributeId == item.KAttributeId);
            if (index >= 0)
                ListEntities[index] = editorCtrl.Model;

            View.UpdateItem(editorCtrl.Model);
            return true;
        }
        return false;
    }

    public override async Task<bool> DeleteItemAsync(KAttributeInfoDto item)
    {
        var editorCtrl = new AttributeEditorCtrl(Store);
        var deleted = await editorCtrl.DeleteModel(Service, item.KAttributeId);

        if (deleted)
        {
            ListEntities.RemoveAll(_ => _.KAttributeId == item.KAttributeId);
            View.RemoveItem(item);
        }
        return deleted;
    }

    #endregion
}
