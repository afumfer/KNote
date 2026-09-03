using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Single trace note type add/edit popup, used by TraceNoteTypesManageCtrl (repository
/// administration - TraceNote types tab). Same shape as NoteTypeEditorCtrl: no parent "Save" to
/// stage into, every save/delete persists immediately against Service.TraceNoteTypes.
/// </summary>
public class TraceNoteTypeEditorCtrl : CtrlEditorBase<IViewEditor<TraceNoteTypeDto>, TraceNoteTypeDto>
{
    #region Constructor

    public TraceNoteTypeEditorCtrl(Store store) : base(store)
    {
        ControllerName = "Trace note type editor";
    }

    #endregion

    #region Controller editor implementation

    protected override IViewEditor<TraceNoteTypeDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<TraceNoteTypeEditorCtrl, IViewEditor<TraceNoteTypeDto>>(this);
    }

    public override async Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        try
        {
            Service = service;

            Model = (await Service.TraceNoteTypes.GetAsync(id)).Entity;
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

        Model = new TraceNoteTypeDto();

        return Task.FromResult(true);
    }

    public async override Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty())
            return true;

        var isNew = Model.TraceNoteTypeId == Guid.Empty;

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        try
        {
            var response = await Service.TraceNoteTypes.SaveAsync(Model);

            if (response.IsValid)
            {
                Model = response.Entity;
                Model.SetIsDirty(false);

                if (isNew)
                    OnAddedEntity(Model);
                else
                    OnSavedEntity(Model);

                Finalize();
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
            // TraceNoteType.Name is enforced unique at the DB level (see
            // Repository.EntityFramework's ModelBuilderExtensions), and it's not pre-validated
            // here, so a duplicate name surfaces as a wrapped DB exception - same unwrap as
            // NoteTypeEditorCtrl.SaveModel.
            View.ShowInfo(RootExceptionMessage(ex));
            return false;
        }
    }

    public async override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        var result = View.ShowInfo("Are you sure you want to delete this trace note type?", "Delete trace note type", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes)
        {
            try
            {
                var response = await service.TraceNoteTypes.DeleteAsync(id);

                if (response.IsValid)
                {
                    OnDeletedEntity(response.Entity);
                    return true;
                }
                else
                {
                    View.ShowInfo(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                // A trace note type still referenced by TraceNotes fails at the DB level (FK
                // constraint) rather than being pre-checked - same unwrap as SaveModel, mirroring
                // NoteTypeEditorCtrl.DeleteModel's handling of NoteTypes still used by KAttributes.
                View.ShowInfo(RootExceptionMessage(ex));
            }
        }
        return false;
    }

    public async override Task<bool> DeleteModel()
    {
        return await DeleteModel(Service, Model.TraceNoteTypeId);
    }

    #endregion
}
