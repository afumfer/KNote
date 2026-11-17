using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// «Review selection» (KNote Atlas, stage 2, 2.3b, CU-15, D20), a modal window from the section «Period summary»: the notes of the
/// selection in three lists (In, Doubtful, Out) with their reason and a box (checked, it goes into the summary; a doubtful one starts
/// checked, S5). Accepting keeps the boxes changed as corrections in «Note relevance» (NoteRelevanceRules.CorrectionFor: a box back
/// as KNote decides takes the correction away); the notes don't change. A note opens in the note editor. Included (no
/// [KntAuthorize]): the command checks the role (Staff).
/// </summary>
public class ReviewSelectionCtrl : CtrlViewBase<IViewReviewSelection>
{
    #region Private fields

    private readonly Dictionary<Guid, bool> _checked = new();

    #endregion

    #region Constructor

    public ReviewSelectionCtrl(Store store, IKntService service, NoteSelectionResultDto selection) : base(store)
    {
        ControllerName = "Review selection";
        Service = service;
        Selection = selection ?? new NoteSelectionResultDto();
    }

    #endregion

    #region Properties

    public IKntService Service { get; }

    public NoteSelectionResultDto Selection { get; }

    // Whether Accept kept something.
    public bool Saved { get; private set; }

    public string CountText => NoteRelevanceRules.CountText(Selection);

    public List<RelevanceCorrectionChangeDto> Changes => Selection.Notes
        .Where(n => IsChecked(n) != n.IsIncluded)
        .Select(n => new RelevanceCorrectionChangeDto { NoteId = n.NoteId, Decision = NoteRelevanceRules.CorrectionFor(n, IsChecked(n)) })
        .ToList();

    public string ChangesText => NoteRelevanceRules.ChangesText(Selection.Notes.Count(IsChecked), Selection.Notes.Count, Changes.Count);

    #endregion

    #region Controller implementation

    protected override IViewReviewSelection CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<ReviewSelectionCtrl, IViewReviewSelection>(this);
    }

    #endregion

    #region Public methods

    public List<NoteSelectionDto> NotesOf(EnumSelectionDecision decision) => Selection.Notes.Where(n => n.Decision == decision).ToList();

    public static string ListText(EnumSelectionDecision decision) => decision switch
    {
        EnumSelectionDecision.In => "In",
        EnumSelectionDecision.Doubtful => "Doubtful",
        _ => "Out"
    };

    public bool IsChecked(NoteSelectionDto note) => _checked.TryGetValue(note.NoteId, out var value) ? value : note.IsIncluded;

    public void SetChecked(Guid noteId, bool value) => _checked[noteId] = value;

    // Keeps the changes (nothing to keep: true, without saving); false if they could not be kept (the window stays).
    public async Task<bool> AcceptAsync()
    {
        var changes = Changes;
        if (changes.Count == 0)
            return true;

        try
        {
            var result = await Service.Summaries.SaveRelevanceCorrectionsAsync(new RelevanceCorrectionsSaveDto { Changes = changes });
            if (!result.IsValid)
            {
                ShowResultError(result);
                return false;
            }
            Saved = true;
            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }

    public async Task<bool> OpenNoteAsync(Guid noteId)
    {
        var noteEditorCtrl = new NoteEditorCtrl(Store);
        if (!await noteEditorCtrl.LoadModelById(Service, noteId, false))
        {
            noteEditorCtrl.Dispose();
            return false;
        }
        noteEditorCtrl.Run();
        return true;
    }

    #endregion
}
