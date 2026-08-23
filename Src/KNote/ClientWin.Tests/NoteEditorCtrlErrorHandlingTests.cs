using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Regression tests for Fase 5 of the ClientWin architecture refactor: NoteEditorCtrl.SaveModel
/// and DeleteModel used to return true (success) even when the underlying Result was invalid or
/// an exception was thrown, silently telling the caller the operation succeeded while an error
/// (or nothing at all, for DeleteModel) was shown to the user. These tests characterize the fixed,
/// honest contract: the boolean return value now matches what actually happened.
/// </summary>
[TestClass]
public class NoteEditorCtrlErrorHandlingTests
{
    private static (NoteEditorCtrl ctrl, FakeNoteEditorView view, FakeKntService service) CreateCtrl()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeNoteEditorView();
        factoryViews.Registry.Register<NoteEditorCtrl, IViewEditorEmbeddable<NoteExtendedDto>>(c => view);

        var store = new Store(factoryViews);
        var ctrl = new NoteEditorCtrl(store);
        var service = new FakeKntService();

        return (ctrl, view, service);
    }

    private static NoteExtendedDto CreateValidNote() => new()
    {
        NoteId = Guid.NewGuid(),
        Topic = "Test topic",
        FolderId = Guid.NewGuid(),
        CreationDateTime = DateTime.Now,
        ModificationDateTime = DateTime.Now,
    };

    [TestMethod]
    public async Task SaveModel_ServiceReturnsInvalidResult_ReturnsFalse()
    {
        var (ctrl, view, service) = CreateCtrl();
        var note = CreateValidNote();
        service.NotesFake.GetExtendedAsyncImpl = _ => Task.FromResult(new Result<NoteExtendedDto>(note));
        await ctrl.LoadModelById(service, note.NoteId);
        ctrl.Model.Topic = "Changed, so the model is dirty";

        var invalidResult = new Result<NoteExtendedDto>();
        invalidResult.AddErrorMessage("Simulated save failure");
        service.NotesFake.SaveExtendedAsyncImpl = _ => Task.FromResult(invalidResult);

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("Simulated save failure", view.LastShownInfo);
    }

    [TestMethod]
    public async Task SaveModel_ServiceThrows_ReturnsFalse()
    {
        var (ctrl, view, service) = CreateCtrl();
        var note = CreateValidNote();
        service.NotesFake.GetExtendedAsyncImpl = _ => Task.FromResult(new Result<NoteExtendedDto>(note));
        await ctrl.LoadModelById(service, note.NoteId);
        ctrl.Model.Topic = "Changed, so the model is dirty";

        service.NotesFake.SaveExtendedAsyncImpl = _ => throw new InvalidOperationException("Simulated exception");

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("Simulated exception", view.LastShownInfo);
    }

    [TestMethod]
    public async Task SaveModel_ServiceReturnsValidResult_ReturnsTrue()
    {
        var (ctrl, view, service) = CreateCtrl();
        var note = CreateValidNote();
        service.NotesFake.GetExtendedAsyncImpl = _ => Task.FromResult(new Result<NoteExtendedDto>(note));
        await ctrl.LoadModelById(service, note.NoteId);
        ctrl.Model.Topic = "Changed, so the model is dirty";

        service.NotesFake.SaveExtendedAsyncImpl = _ => Task.FromResult(new Result<NoteExtendedDto>(note));

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
    }

    [TestMethod]
    public async Task DeleteModel_ServiceReturnsInvalidResult_ReturnsFalseAndShowsError()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.ConfirmationResult = DialogResult.Yes;

        var invalidResult = new Result<NoteExtendedDto>();
        invalidResult.AddErrorMessage("Simulated delete failure");
        service.NotesFake.DeleteExtendedAsyncImpl = _ => Task.FromResult(invalidResult);

        var deleted = await ctrl.DeleteModel(service, Guid.NewGuid());

        Assert.IsFalse(deleted);
        Assert.AreEqual("Simulated delete failure", view.LastShownInfo);
    }

    [TestMethod]
    public async Task DeleteModel_ServiceReturnsValidResult_ReturnsTrue()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.ConfirmationResult = DialogResult.Yes;
        var note = CreateValidNote();
        service.NotesFake.DeleteExtendedAsyncImpl = _ => Task.FromResult(new Result<NoteExtendedDto>(note));

        var deleted = await ctrl.DeleteModel(service, note.NoteId);

        Assert.IsTrue(deleted);
    }

    [TestMethod]
    public async Task DeleteModel_UserDeclinesConfirmation_ReturnsFalseWithoutCallingService()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.ConfirmationResult = DialogResult.No;

        var deleted = await ctrl.DeleteModel(service, Guid.NewGuid());

        Assert.IsFalse(deleted);
    }
}
