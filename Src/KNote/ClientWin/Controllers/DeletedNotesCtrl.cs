using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// «Recover deleted notes» (the history, H24), the Edit menu of the main window: the deleted notes of a repository that still keep
/// their last version (the trash), the newest first. «See this version» shows a note as it was deleted (NoteVersionCtrl);
/// «Recover» creates it again (its own data; the Service says what didn't come back); an Admin can «Remove permanently» (H27). Staff,
/// as deleting notes, in the repository it is opened for.
/// </summary>
[KntAuthorize(EnumRoles.Staff)]
public class DeletedNotesCtrl : CtrlViewBase<IViewDeletedNotes>
{
    #region Constructor

    public DeletedNotesCtrl(Store store, IKntService service) : base(store)
    {
        ControllerName = "Recover deleted notes";
        Service = service;
    }

    #endregion

    #region Properties

    public IKntService Service { get; }

    protected override IKntService AuthorizationResource => Service;

    public List<DeletedNoteDto> Notes { get; private set; } = new();

    /// <summary>The note to select when the window opens (from a deletion of «Repository changes»), if it is in the list.</summary>
    public Guid? SelectedNoteId { get; set; }

    /// <summary>The last note recovered and what didn't come back of it.</summary>
    public NoteRecoverResultDto Recovered { get; private set; }

    /// <summary>Some note was recovered while the window was open: the main window reads its list again.</summary>
    public bool AnyRecovered { get; private set; }

    public int KeptDays { get; private set; } = new HistoryRetentionDto().DeletedNotesDays;

    public bool CanRemove => Store.Security.IsAuthorized(new KntAuthorizeAttribute(EnumRoles.Admin), Service);

    public string Title => $"Recover deleted notes [{Service?.RepositoryRef?.Name}]";

    public string Subtitle => $"Deleted notes are kept {KeptDays} days. Their tasks, resources, alarms and relations are not recovered.";

    #endregion

    #region Controller implementation

    protected override IViewDeletedNotes CreateView() =>
        Store.FactoryViews.Registry.Resolve<DeletedNotesCtrl, IViewDeletedNotes>(this);

    #endregion

    #region Public methods

    /// <summary>Reads the deleted notes; false (with its error in Error) if they can't be read.</summary>
    public async Task<bool> LoadAsync()
    {
        Error = null;
        var retention = await Service.History.GetRetentionAsync();
        if (retention.IsValid && retention.Entity?.Retention != null)
            KeptDays = retention.Entity.Retention.DeletedNotesDays;

        var result = await Service.History.GetDeletedNotesAsync();
        if (!result.IsValid)
        {
            Error = result.ErrorMessage;
            return false;
        }
        Notes = result.Entity ?? new();
        return true;
    }

    public string Error { get; private set; }

    /// <summary>Recovers a deleted note and reads the list again; false (with its error) if it can't.</summary>
    public async Task<bool> RecoverAsync(DeletedNoteDto note)
    {
        if (note == null)
            return false;
        Error = null;
        var result = await Service.History.RecoverNoteAsync(note.NoteId);
        if (!result.IsValid)
        {
            Error = result.ErrorMessage;
            return false;
        }
        Recovered = result.Entity;
        AnyRecovered = true;
        await LoadAsync();
        return true;
    }

    /// <summary>«Remove permanently» (Admin, after confirming): its versions are emptied and it leaves the list.</summary>
    public async Task<bool> RemoveAsync(DeletedNoteDto note)
    {
        if (note == null || !CanRemove)
            return false;
        Error = null;
        var result = await Service.History.RemoveDeletedNoteAsync(note.NoteId);
        if (!result.IsValid)
        {
            Error = result.ErrorMessage;
            return false;
        }
        await LoadAsync();
        return true;
    }

    /// <summary>«See this version»: the note as it was deleted, in its own window.</summary>
    public async Task ShowVersionAsync(DeletedNoteDto note)
    {
        if (note?.Row == null)
            return;
        var versionCtrl = new NoteVersionCtrl(Store, Service, note.NoteId, note.Row, $"#{note.NoteNumber} · {note.Topic}");
        if (await versionCtrl.LoadAsync())
            versionCtrl.Run();
        else
        {
            View.ShowInfo(versionCtrl.Error);
            versionCtrl.Finalize();
        }
    }

    /// <summary>Opens the note recovered last in its editor.</summary>
    public async Task OpenRecoveredAsync()
    {
        if (Recovered == null)
            return;
        var editor = new NoteEditorCtrl(Store);
        if (await editor.LoadModelById(Service, Recovered.NoteId, false))
            editor.Run();
        else
            editor.Dispose();
    }

    #endregion
}
