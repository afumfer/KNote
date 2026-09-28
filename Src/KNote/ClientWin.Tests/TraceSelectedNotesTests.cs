using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for TraceSelectedNotesJob.TraceNoteAsync: the per-note step of "Trace selected notes to/from ..."
/// (notes grid context menu), which creates the template relation between each selected note and the
/// target note, skipping the target itself and relations that already exist.
/// </summary>
[TestClass]
public class TraceSelectedNotesTests
{
    private static readonly Guid TargetNoteId = Guid.NewGuid();
    private static readonly Guid TraceNoteTypeId = Guid.NewGuid();

    private static TraceSelectedNotesJob CreateJob(bool selectedAreFromSide, Guid? traceNoteTypeId = null)
    {
        var template = new TraceNoteDto { TraceNoteTypeId = traceNoteTypeId, Order = 3, Weight = 1.5 };
        if (selectedAreFromSide)
            template.ToId = TargetNoteId;
        else
            template.FromId = TargetNoteId;
        return new TraceSelectedNotesJob(template, selectedAreFromSide);
    }

    private static NoteMinimalDto Note(Guid? noteId = null) => new() { NoteId = noteId ?? Guid.NewGuid(), NoteNumber = 7 };

    private static Task<Result<List<TraceNoteDto>>> Existing(params TraceNoteDto[] traceNotes) =>
        Task.FromResult(new Result<List<TraceNoteDto>>(traceNotes.ToList()));

    [TestMethod]
    public async Task TraceNoteAsync_SelectedToTarget_CreatesRelationFromSelectedNote()
    {
        var service = new FakeKntService();
        var job = CreateJob(selectedAreFromSide: true, TraceNoteTypeId);
        var note = Note();
        TraceNoteDto saved = null;
        service.NotesFake.GetTraceNotesToAsyncImpl = _ => Existing();
        service.NotesFake.SaveTraceNoteAsyncImpl = (e, _) => { saved = e; return Task.FromResult(new Result<TraceNoteDto>(e)); };

        await job.TraceNoteAsync(service, note);

        Assert.AreEqual(1, job.Created);
        Assert.AreEqual(note.NoteId, saved.FromId);
        Assert.AreEqual(TargetNoteId, saved.ToId);
        Assert.AreEqual(TraceNoteTypeId, saved.TraceNoteTypeId);
        Assert.AreEqual(3, saved.Order);
        Assert.AreEqual(1.5, saved.Weight);
        Assert.AreNotEqual(Guid.Empty, saved.TraceNoteId);
    }

    [TestMethod]
    public async Task TraceNoteAsync_SelectedFromTarget_CreatesRelationToSelectedNote()
    {
        var service = new FakeKntService();
        var job = CreateJob(selectedAreFromSide: false);
        var note = Note();
        TraceNoteDto saved = null;
        service.NotesFake.GetTraceNotesFromAsyncImpl = _ => Existing();
        service.NotesFake.SaveTraceNoteAsyncImpl = (e, _) => { saved = e; return Task.FromResult(new Result<TraceNoteDto>(e)); };

        await job.TraceNoteAsync(service, note);

        Assert.AreEqual(1, job.Created);
        Assert.AreEqual(TargetNoteId, saved.FromId);
        Assert.AreEqual(note.NoteId, saved.ToId);
    }

    [TestMethod]
    public async Task TraceNoteAsync_SelectedNoteIsTarget_SkipsWithoutCallingService()
    {
        var service = new FakeKntService();
        var job = CreateJob(selectedAreFromSide: true);

        // Service delegates left unconfigured: any call would throw and be recorded as an error.
        await job.TraceNoteAsync(service, Note(TargetNoteId));

        Assert.AreEqual(1, job.Skipped);
        Assert.AreEqual(0, job.Created);
        Assert.AreEqual(0, job.Errors.Count);
    }

    [TestMethod]
    public async Task TraceNoteAsync_UntypedRelationAlreadyExists_Skips()
    {
        var service = new FakeKntService();
        var job = CreateJob(selectedAreFromSide: true);
        var note = Note();
        service.NotesFake.GetTraceNotesToAsyncImpl = _ => Existing(new TraceNoteDto { FromId = note.NoteId, ToId = TargetNoteId });

        await job.TraceNoteAsync(service, note);

        Assert.AreEqual(1, job.Skipped);
        Assert.AreEqual(0, job.Created);
        Assert.AreEqual(0, job.Errors.Count);
    }

    [TestMethod]
    public async Task TraceNoteAsync_ExistingRelationOfOtherType_CreatesNewOne()
    {
        var service = new FakeKntService();
        var job = CreateJob(selectedAreFromSide: false, TraceNoteTypeId);
        var note = Note();
        service.NotesFake.GetTraceNotesFromAsyncImpl = _ => Existing(new TraceNoteDto { FromId = TargetNoteId, ToId = note.NoteId });
        service.NotesFake.SaveTraceNoteAsyncImpl = (e, _) => Task.FromResult(new Result<TraceNoteDto>(e));

        await job.TraceNoteAsync(service, note);

        Assert.AreEqual(1, job.Created);
        Assert.AreEqual(0, job.Skipped);
    }

    [TestMethod]
    public async Task TraceNoteAsync_SaveFails_RecordsError()
    {
        var service = new FakeKntService();
        var job = CreateJob(selectedAreFromSide: true);
        service.NotesFake.GetTraceNotesToAsyncImpl = _ => Existing();
        service.NotesFake.SaveTraceNoteAsyncImpl = (e, _) =>
        {
            var res = new Result<TraceNoteDto>(e);
            res.AddErrorMessage("boom");
            return Task.FromResult(res);
        };

        await job.TraceNoteAsync(service, Note());

        Assert.AreEqual(0, job.Created);
        Assert.AreEqual(1, job.Errors.Count);
        StringAssert.Contains(job.Errors[0], "boom");
    }
}
