using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// TraceNote types tab of the repository administration screen (RepositoryEditorCtrl): lists the
/// trace note types of the repository being managed and delegates add/edit to the
/// TraceNoteTypeEditorCtrl popup. Same shape as NoteTypesManageCtrl.
/// </summary>
public class TraceNoteTypesManageCtrl : CtrlManageListBase<IViewManageList<TraceNoteTypeDto>, TraceNoteTypeDto>
{
    #region Constructor

    public TraceNoteTypesManageCtrl(Store store) : base(store)
    {
        ControllerName = "Trace note types management";
    }

    #endregion

    #region CtrlManageListBase implementation

    protected override IViewManageList<TraceNoteTypeDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<TraceNoteTypesManageCtrl, IViewManageList<TraceNoteTypeDto>>(this);
    }

    public override async Task<bool> LoadEntitiesAsync(IKntService service, bool refreshView = true)
    {
        try
        {
            Service = service;

            var response = await Service.TraceNoteTypes.GetAllAsync();

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
        var editorCtrl = new TraceNoteTypeEditorCtrl(Store);
        await editorCtrl.NewModel(Service);

        var res = editorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            ListEntities.Add(editorCtrl.Model);
            View.AddItem(editorCtrl.Model);
            OnListChanged();
            return true;
        }
        return false;
    }

    public override async Task<bool> EditItemAsync(TraceNoteTypeDto item)
    {
        var editorCtrl = new TraceNoteTypeEditorCtrl(Store);
        // refreshView must stay true here: unlike embedded-only forms, this popup is shown via
        // RunModal() right after, and there is no Form_Load wiring to populate it later - without
        // this, the dialog opens with the fields blank even though Model has the loaded entity.
        await editorCtrl.LoadModelById(Service, item.TraceNoteTypeId, true);

        var res = editorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            var index = ListEntities.FindIndex(_ => _.TraceNoteTypeId == item.TraceNoteTypeId);
            if (index >= 0)
                ListEntities[index] = editorCtrl.Model;

            View.UpdateItem(editorCtrl.Model);
            OnListChanged();
            return true;
        }
        return false;
    }

    public override async Task<bool> DeleteItemAsync(TraceNoteTypeDto item)
    {
        var editorCtrl = new TraceNoteTypeEditorCtrl(Store);
        var deleted = await editorCtrl.DeleteModel(Service, item.TraceNoteTypeId);

        if (deleted)
        {
            ListEntities.RemoveAll(_ => _.TraceNoteTypeId == item.TraceNoteTypeId);
            View.RemoveItem(item);
            OnListChanged();
        }
        return deleted;
    }

    #endregion
}
