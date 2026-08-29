using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class NotesFilterParamCtrl : CtrlViewEmbeddableBase<IViewEmbeddable>
{
    #region Properties

    private List<ServiceRef> _servicesRef;
    public List<ServiceRef> ServicesRef
    {
        get
        {
            if (_servicesRef == null)
                _servicesRef = Store.GetAllServiceRef();
            return _servicesRef;
        }
        set
        {
            _servicesRef = value;
        }
    }

    public List<NoteTypeDto> NoteTypes { get; private set; } = new();

    public List<KAttributeInfoDto> KAttributes { get; private set; } = new();

    #endregion

    #region Constructor

    public NotesFilterParamCtrl(Store store) : base(store)
    {
        ControllerName = "Notes filter param";
    }

    #endregion

    #region ControllerEditorBase implementation

    protected override IViewEmbeddable CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<NotesFilterParamCtrl, IViewEmbeddable>(this);
    }

    #endregion

    #region Controller events

    public event EventHandler<ControllerEventArgs<SelectedNotesInServiceRef>> FilterApplied;

    protected virtual void OnFilterApplied(SelectedNotesInServiceRef filter)
    {
        FilterApplied?.Invoke(this, new ControllerEventArgs<SelectedNotesInServiceRef>(filter));
    }

    public void NotifyFilterApplied(SelectedNotesInServiceRef filter)
    {
        OnFilterApplied(filter);
    }

    #endregion

    #region Controller extra methods

    public async Task LoadNoteTypes(IKntService service)
    {
        if (service == null)
        {
            NoteTypes = new List<NoteTypeDto>();
            return;
        }

        var response = await service.NoteTypes.GetAllAsync();
        NoteTypes = response.IsValid ? response.Entity : new List<NoteTypeDto>();
    }

    public async Task LoadKAttributes(IKntService service)
    {
        if (service == null)
        {
            KAttributes = new List<KAttributeInfoDto>();
            return;
        }

        var response = await service.KAttributes.GetAllAsync();
        KAttributes = response.IsValid ? response.Entity : new List<KAttributeInfoDto>();
    }

    #endregion
}
