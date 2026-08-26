using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Single attribute add/edit popup, used by KAttributesManageCtrl (repository administration -
/// Attributes tab). Like NoteTypeEditorCtrl, there is no parent "Save" to stage into for the
/// attribute itself: SaveModel/DeleteModel persist immediately against Service.KAttributes
/// (AutoDBSave stays true, the CtrlEditorBase default).
///
/// The attribute's tabulated values (KAttributeDataType TabulatedValue/TagsValue) are a nested
/// staged sub-collection instead - same shape as Alarms/Tasks on NoteEditorCtrl: NewTabulatedValue/
/// EditTabulatedValue/DeleteTabulatedValue only mutate Model.KAttributeValues in memory (via a
/// KAttributeTabulatedValueEditorCtrl popup with AutoDBSave=false); they're only actually persisted
/// together with the rest of the attribute when SaveModel calls Service.KAttributes.SaveAsync.
/// </summary>
public class AttributeEditorCtrl : CtrlEditorBase<IViewEditor<KAttributeDto>, KAttributeDto>
{
    #region Properties

    /// <summary>Note types of the repository, for the "Note type" picker - loaded once per popup.</summary>
    public List<NoteTypeDto> NoteTypes { get; private set; } = new();

    #endregion

    #region Constructor

    public AttributeEditorCtrl(Store store) : base(store)
    {
        ControllerName = "Attribute editor";
    }

    #endregion

    #region Abstract member implementations

    protected override IViewEditor<KAttributeDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<AttributeEditorCtrl, IViewEditor<KAttributeDto>>(this);
    }

    public override async Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        try
        {
            Service = service;

            await LoadNoteTypes();

            Model = (await Service.KAttributes.GetAsync(id)).Entity;
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

    public override async Task<bool> NewModel(IKntService service)
    {
        Service = service;

        await LoadNoteTypes();

        Model = new KAttributeDto();

        return true;
    }

    public async override Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty())
            return true;

        var isNew = Model.KAttributeId == Guid.Empty;

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        try
        {
            var response = await Service.KAttributes.SaveAsync(Model);

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
            // KAttribute.Name is enforced unique at the DB level, globally across note types (not
            // per type - see Repository.EntityFramework's ModelBuilderExtensions), and it's not
            // pre-validated here, so a duplicate name surfaces as a wrapped DB exception. Same
            // unwrap convention as NoteTypeEditorCtrl.SaveModel.
            View.ShowInfo(RootExceptionMessage(ex));
            return false;
        }
    }

    public async override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        var result = View.ShowInfo("Are you sure you want to delete this attribute?", "Delete attribute", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes)
        {
            try
            {
                var response = await service.KAttributes.DeleteAsync(id);

                if (response.IsValid)
                {
                    // DeleteAsync returns the lighter KAttributeInfoDto (no tabulated values needed
                    // for a delete notification), but OnDeletedEntity needs this editor's TEntity
                    // (KAttributeDto) - copy the fields across rather than re-fetching.
                    var deletedEntity = new KAttributeDto
                    {
                        KAttributeId = response.Entity.KAttributeId,
                        Name = response.Entity.Name,
                        Description = response.Entity.Description,
                        KAttributeDataType = response.Entity.KAttributeDataType,
                        RequiredValue = response.Entity.RequiredValue,
                        Order = response.Entity.Order,
                        Script = response.Entity.Script,
                        Disabled = response.Entity.Disabled,
                        NoteTypeId = response.Entity.NoteTypeId,
                        NoteTypeDto = response.Entity.NoteTypeDto
                    };
                    OnDeletedEntity(deletedEntity);
                    return true;
                }
                else
                {
                    // Covers the "still in use by notes" business rule (KntKAttributesDeleteAsyncCommand,
                    // shared with Server/Blazor) reported via Result.ErrorMessage, same as NoteTypeEditorCtrl.
                    View.ShowInfo(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                View.ShowInfo(RootExceptionMessage(ex));
            }
        }
        return false;
    }

    public async override Task<bool> DeleteModel()
    {
        return await DeleteModel(Service, Model.KAttributeId);
    }

    #endregion

    #region Tabulated values (staged sub-collection, mirrors NoteEditorCtrl.NewMessage/EditMessage/DeleteMessage)

    public async Task<KAttributeTabulatedValueDto> NewTabulatedValue()
    {
        var editorCtrl = new KAttributeTabulatedValueEditorCtrl(Store);
        editorCtrl.AutoDBSave = false;

        await editorCtrl.NewModel(Service);
        editorCtrl.Model.SetIsNew(true);

        var res = editorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            Model.KAttributeValues.Add(editorCtrl.Model);
            return editorCtrl.Model;
        }
        return null;
    }

    public KAttributeTabulatedValueDto EditTabulatedValue(Guid tabulatedValueId)
    {
        var editorCtrl = new KAttributeTabulatedValueEditorCtrl(Store);
        editorCtrl.AutoDBSave = false;

        var value = Model.KAttributeValues.SingleOrDefault(_ => _.KAttributeTabulatedValueId == tabulatedValueId);
        if (value == null)
            return null;

        editorCtrl.LoadModel(Service, value, false);

        var res = editorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
            return editorCtrl.Model;
        return null;
    }

    public bool DeleteTabulatedValue(Guid tabulatedValueId)
    {
        var value = Model.KAttributeValues.SingleOrDefault(_ => _.KAttributeTabulatedValueId == tabulatedValueId);
        if (value == null)
            return false;

        Model.KAttributeValues.Remove(value);
        Model.SetIsDirty(true);
        return true;
    }

    #endregion

    #region Private methods

    private async Task LoadNoteTypes()
    {
        var response = await Service.NoteTypes.GetAllAsync();
        NoteTypes = response.IsValid ? response.Entity : new List<NoteTypeDto>();
    }

    #endregion
}
