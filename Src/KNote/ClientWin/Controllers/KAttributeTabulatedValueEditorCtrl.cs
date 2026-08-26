using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Single tabulated-value add/edit popup for an attribute's TabulatedValue/TagsValue data type,
/// used by AttributeEditorCtrl. Always staged (AutoDBSave=false, set by the caller): SaveModel only
/// marks the in-memory model dirty, it never calls the service directly - the whole list travels
/// inside the parent KAttributeDto.KAttributeValues and is persisted together when the attribute
/// itself is saved. Same shape as MessageEditorCtrl for NoteEditorCtrl's Alarms tab.
/// </summary>
public class KAttributeTabulatedValueEditorCtrl : CtrlEditorBase<IViewEditor<KAttributeTabulatedValueDto>, KAttributeTabulatedValueDto>
{
    #region Constructor

    public KAttributeTabulatedValueEditorCtrl(Store store) : base(store)
    {
        ControllerName = "Attribute tabulated value editor";
    }

    #endregion

    #region Controller editor implementation

    protected override IViewEditor<KAttributeTabulatedValueDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<KAttributeTabulatedValueEditorCtrl, IViewEditor<KAttributeTabulatedValueDto>>(this);
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        // Not used: tabulated values are always loaded from the in-memory
        // KAttributeDto.KAttributeValues collection via LoadModel (see AttributeEditorCtrl.EditTabulatedValue),
        // never fetched individually by id.
        throw new NotImplementedException();
    }

    public override Task<bool> NewModel(IKntService service)
    {
        Service = service;

        Model = new KAttributeTabulatedValueDto();

        return Task.FromResult(true);
    }

    public override Task<bool> SaveModel()
    {
        View.RefreshModel();

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return Task.FromResult(false);
        }

        Model.SetIsDirty(true);
        OnSavedEntity(Model);
        Finalize();

        return Task.FromResult(true);
    }

    public override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DeleteModel()
    {
        throw new NotImplementedException();
    }

    #endregion
}
