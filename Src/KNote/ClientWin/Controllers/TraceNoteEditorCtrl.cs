using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Single trace note (relation) add/edit popup, opened by NoteEditorCtrl.NewTraceNote/EditTraceNote
/// for the "Trace notes" tab. Same in-memory-only shape as TaskEditorCtrl/MessageEditorCtrl when used
/// with AutoDBSave = false: SaveModel only stages the change onto Model (dirty-tracked), it never
/// hits the repository directly - persistence happens later, together with the rest of the note,
/// via NoteEditorCtrl.SaveModel -> Service.Notes.SaveExtendedAsync.
/// </summary>
public class TraceNoteEditorCtrl : CtrlEditorBase<IViewEditor<TraceNoteDto>, TraceNoteDto>
{
    #region Properties

    // True when the note being edited (NoteEditorCtrl.Model) is the FromId side of the relation -
    // i.e. this is an outgoing relation, staged into Model.TraceNotesTo. False when it's the ToId
    // side - an incoming relation, staged into Model.TraceNotesFrom. Set by the caller before
    // NewModel/LoadModel; used here only to know which endpoint (FromId/ToId) the note picker fills in.
    public bool OwnerIsFromSide { get; set; }

    public Guid RelatedNoteId { get; private set; }
    public string RelatedNoteDisplay { get; private set; } = "";

    public List<TraceNoteTypeDto> TraceNoteTypeOptions { get; private set; } = new();

    #endregion

    #region Constructor

    public TraceNoteEditorCtrl(Store store) : base(store)
    {
        ControllerName = "Trace note editor";
    }

    #endregion

    #region Controller editor implementation

    protected override IViewEditor<TraceNoteDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<TraceNoteEditorCtrl, IViewEditor<TraceNoteDto>>(this);
    }

    public override Task<bool> NewModel(IKntService service)
    {
        Service = service;

        Model = new TraceNoteDto();
        Model.TraceNoteId = Guid.NewGuid();

        RelatedNoteId = Guid.Empty;
        RelatedNoteDisplay = "";

        return Task.FromResult(true);
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        // Trace notes are never loaded standalone by id: they're always already present in the
        // parent note's Model.TraceNotesFrom/To (loaded together with the note). Editing goes
        // through LoadModel(service, existingTraceNote, ...) instead - see NoteEditorCtrl.EditTraceNote.
        throw new NotSupportedException($"{nameof(TraceNoteEditorCtrl)} does not support loading a trace note by id.");
    }

    public async Task LoadTraceNoteTypeOptionsAsync()
    {
        var response = await Service.TraceNoteTypes.GetAllAsync();
        TraceNoteTypeOptions = response.IsValid ? response.Entity : new List<TraceNoteTypeDto>();
    }

    public async Task LoadRelatedNoteInfoAsync()
    {
        RelatedNoteId = OwnerIsFromSide ? Model.ToId : Model.FromId;
        RelatedNoteDisplay = await GetNoteDisplayAsync(RelatedNoteId);
    }

    public async Task SetRelatedNoteAsync(Guid noteId)
    {
        RelatedNoteId = noteId;
        RelatedNoteDisplay = await GetNoteDisplayAsync(noteId);
    }

    private async Task<string> GetNoteDisplayAsync(Guid noteId)
    {
        if (noteId == Guid.Empty)
            return "";

        var res = await Service.Notes.GetAsync(noteId);
        return res.IsValid && res.Entity != null ? $"#{res.Entity.NoteNumber} - {res.Entity.Topic}" : "";
    }

    public override async Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (RelatedNoteId == Guid.Empty)
        {
            View.ShowInfo("You must select the related note.");
            return false;
        }

        if (OwnerIsFromSide)
            Model.ToId = RelatedNoteId;
        else
            Model.FromId = RelatedNoteId;

        if (!Model.IsDirty())
            return true;

        var isNew = Model.IsNew();

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        try
        {
            Result<TraceNoteDto> response;
            if (AutoDBSave)
            {
                response = await Service.Notes.SaveTraceNoteAsync(Model, true);
                Model = response.Entity;
                Model.SetIsDirty(false);
            }
            else
            {
                response = new Result<TraceNoteDto>();
                Model.SetIsDirty(true);
                response.Entity = Model;
            }

            if (response.IsValid)
            {
                if (isNew)
                    OnAddedEntity(response.Entity);
                else
                    OnSavedEntity(response.Entity);

                Finalize();
            }
            else
            {
                View.ShowInfo(response.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            View.ShowInfo(RootExceptionMessage(ex));
            return false;
        }

        return true;
    }

    public override async Task<bool> DeleteModel(IKntService service, Guid id)
    {
        // Required by CtrlEditorBase but not exercised by the "Trace notes" tab: trace notes are
        // only ever removed from the parent note's own in-memory list (NoteEditorCtrl.DeleteTraceNote,
        // mirroring DeleteTask), never deleted standalone through this controller.
        var result = View.ShowInfo("Are you sure you want to delete this trace note?", "Delete trace note", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes)
        {
            try
            {
                var response = await service.Notes.DeleteTraceNoteAsync(id);
                if (response.IsValid)
                {
                    OnDeletedEntity(response.Entity);
                    return true;
                }
                else
                    View.ShowInfo(response.ErrorMessage);
            }
            catch (Exception ex)
            {
                View.ShowInfo(RootExceptionMessage(ex));
            }
        }
        return false;
    }

    public override async Task<bool> DeleteModel()
    {
        return await DeleteModel(Service, Model.TraceNoteId);
    }

    #endregion
}
