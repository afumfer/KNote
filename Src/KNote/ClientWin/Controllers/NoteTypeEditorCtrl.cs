using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Single note type add/edit popup, used by NoteTypesManageCtrl (repository administration - Note
/// types tab). Unlike note-scoped editors (Alarms, Tasks...), there is no parent "Save" to stage
/// into: every save/delete persists immediately against Service.NoteTypes (AutoDBSave stays true,
/// the CtrlEditorBase default).
/// </summary>
public class NoteTypeEditorCtrl : CtrlEditorBase<IViewEditor<NoteTypeDto>, NoteTypeDto>
{
    #region Constructor

    public NoteTypeEditorCtrl(Store store) : base(store)
    {
        ControllerName = "Note type editor";
    }

    #endregion

    #region Controller editor implementation

    protected override IViewEditor<NoteTypeDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<NoteTypeEditorCtrl, IViewEditor<NoteTypeDto>>(this);
    }

    public override async Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        try
        {
            Service = service;

            Model = (await Service.NoteTypes.GetAsync(id)).Entity;
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

        Model = new NoteTypeDto();

        return Task.FromResult(true);
    }

    public async override Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty())
            return true;

        var isNew = Model.NoteTypeId == Guid.Empty;

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        try
        {
            var response = await Service.NoteTypes.SaveAsync(Model);

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
            // NoteType.Name is enforced unique at the DB level (see Repository.EntityFramework's
            // ModelBuilderExtensions), and it's not pre-validated here, so a duplicate name surfaces
            // as a wrapped DB exception. Walk down to the innermost exception so the user sees the
            // actual reason ("UNIQUE constraint failed: ...") instead of the generic
            // "KNote service error. (...)" wrapper - same convention as UserRegisterCtrl.SaveModel.
            View.ShowInfo(RootExceptionMessage(ex));
            return false;
        }
    }

    public async override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        var result = View.ShowInfo("Are you sure you want to delete this note type?", "Delete note type", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes)
        {
            try
            {
                var response = await service.NoteTypes.DeleteAsync(id);

                if (response.IsValid)
                {
                    OnDeletedEntity(response.Entity);
                    return true;
                }
                else
                {
                    // Covers both the "still in use by notes" business rule (KntNoteTypeDeleteAsyncCommand,
                    // shared with Server/Blazor) and any other rejection reported via Result.ErrorMessage.
                    View.ShowInfo(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                // A note type still referenced by KAttributes fails at the DB level (FK constraint)
                // rather than being pre-checked - same unwrap as SaveModel.
                View.ShowInfo(RootExceptionMessage(ex));
            }
        }
        return false;
    }

    public async override Task<bool> DeleteModel()
    {
        return await DeleteModel(Service, Model.NoteTypeId);
    }

    #endregion
}
