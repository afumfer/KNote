using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

/// <summary>
/// "Trace selected notes to/from ..." (notes grid context menu, see
/// KNoteManagementCtrl.TraceSelectedNotes): creates the template relation picked in the
/// TraceNoteEditorCtrl dialog between each selected note and the target note, and keeps the running
/// outcome shown to the user as a summary at the end.
/// </summary>
public class TraceSelectedNotesJob
{
    #region Constructor

    // selectedAreFromSide = true: each selected note is FromId and the target note is ToId ("to");
    // false: the target note is FromId ("from"). The template's other endpoint is the target note.
    public TraceSelectedNotesJob(TraceNoteDto template, bool selectedAreFromSide)
    {
        Template = template;
        SelectedAreFromSide = selectedAreFromSide;
    }

    #endregion

    #region Properties

    public TraceNoteDto Template { get; }
    public bool SelectedAreFromSide { get; }
    public Guid TargetNoteId => SelectedAreFromSide ? Template.ToId : Template.FromId;

    public int Created { get; private set; }
    public int Skipped { get; private set; }
    public List<string> Errors { get; } = new();

    #endregion

    #region Public methods

    // Skips the target note itself (a note cannot be traced to itself) and an already existing
    // relation with the same endpoints and type - including untyped ones, which the database unique
    // index (FromId, ToId, TraceNoteTypeId) doesn't block.
    public async Task TraceNoteAsync(IKntService service, NoteMinimalDto note)
    {
        if (note.NoteId == TargetNoteId)
        {
            Skipped++;
            return;
        }

        try
        {
            var existing = SelectedAreFromSide
                ? await service.Notes.GetTraceNotesToAsync(note.NoteId)
                : await service.Notes.GetTraceNotesFromAsync(note.NoteId);
            if (!existing.IsValid)
            {
                AddError(note, existing.ErrorMessage);
                return;
            }

            if (existing.Entity.Any(t => (SelectedAreFromSide ? t.ToId : t.FromId) == TargetNoteId
                && t.TraceNoteTypeId == Template.TraceNoteTypeId))
            {
                Skipped++;
                return;
            }

            var traceNote = new TraceNoteDto
            {
                TraceNoteId = Guid.NewGuid(),
                FromId = SelectedAreFromSide ? note.NoteId : TargetNoteId,
                ToId = SelectedAreFromSide ? TargetNoteId : note.NoteId,
                TraceNoteTypeId = Template.TraceNoteTypeId,
                Order = Template.Order,
                Weight = Template.Weight
            };

            var res = await service.Notes.SaveTraceNoteAsync(traceNote, true);
            if (res.IsValid)
                Created++;
            else
                AddError(note, res.ErrorMessage);
        }
        catch (Exception ex)
        {
            AddError(note, ex.Message);
        }
    }

    public string Summary()
    {
        var summary = $"Trace notes created: {Created}. Skipped (already traced or target note): {Skipped}. Errors: {Errors.Count}.";
        if (Errors.Count > 0)
            summary += Environment.NewLine + string.Join(Environment.NewLine, Errors);
        return summary;
    }

    #endregion

    #region Private methods

    private void AddError(NoteMinimalDto note, string message) =>
        Errors.Add($"#{note.NoteNumber}: {message}");

    #endregion
}
