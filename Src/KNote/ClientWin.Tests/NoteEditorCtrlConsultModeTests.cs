using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for NoteEditorCtrl's consult mode: a user who may read a note but not change it (a Guest in its
/// repository) still opens it, but the editor says so, never tries to save it on its own (autosave), and
/// explains why instead of sending a save the Service layer would refuse.
/// </summary>
[TestClass]
public class NoteEditorCtrlConsultModeTests
{
    private static (NoteEditorCtrl ctrl, FakeNoteEditorView view, FakeKntService service, Store store) CreateCtrl(EnumRoles role)
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeNoteEditorView();
        factoryViews.Registry.Register<NoteEditorCtrl, IViewNoteEditorEmbeddable<NoteExtendedDto>>(c => view);

        var service = new FakeKntService { RepositoryRef = new RepositoryRef { Alias = "Shared" } };
        var store = new Store(factoryViews);
        store.Security.SetRepositoryRole(service, role);

        service.NotesFake.GetExtendedAsyncImpl = id => Task.FromResult(new Result<NoteExtendedDto>(new NoteExtendedDto
        {
            NoteId = id,
            Topic = "A note",
            FolderId = Guid.NewGuid(),
            Description = "",
            Tags = ""
        }));

        return (new NoteEditorCtrl(store), view, service, store);
    }

    [TestMethod]
    [DataRow(EnumRoles.Guest, true)]
    [DataRow(EnumRoles.Staff, false)]
    [DataRow(EnumRoles.Admin, false)]
    public async Task ConsultMode_DependsOnTheRoleInTheNotesRepository(EnumRoles role, bool expected)
    {
        var (ctrl, _, service, _) = CreateCtrl(role);

        await ctrl.LoadModelById(service, Guid.NewGuid(), refreshView: false);

        Assert.AreEqual(expected, ctrl.ConsultMode);
    }

    [TestMethod]
    public async Task Guest_SavingAChangedNote_IsExplainedAndNothingIsSent()
    {
        var (ctrl, view, service, _) = CreateCtrl(EnumRoles.Guest);
        await ctrl.LoadModelById(service, Guid.NewGuid(), refreshView: false);
        var saveCalled = false;
        service.NotesFake.SaveExtendedAsyncImpl = n => { saveCalled = true; return Task.FromResult(new Result<NoteExtendedDto>(n)); };
        ctrl.Model.Topic = "Changed";

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.IsFalse(saveCalled);
        StringAssert.Contains(view.LastShownInfo, "can't be saved");
        StringAssert.Contains(view.LastShownInfo, "'Shared'");
    }

    [TestMethod]
    public async Task Guest_SavingAnUnchangedNote_IsFine()
    {
        var (ctrl, _, service, _) = CreateCtrl(EnumRoles.Guest);
        await ctrl.LoadModelById(service, Guid.NewGuid(), refreshView: false);

        Assert.IsTrue(await ctrl.SaveModel());
    }

    [TestMethod]
    public async Task NewModel_RefusedByTheService_FailsBeforeAnyEditorOpens()
    {
        var (ctrl, view, service, store) = CreateCtrl(EnumRoles.Guest);
        var notified = new List<string>();
        store.AccessDeniedNotifier = notified.Add;
        service.NotesFake.NewExtendedAsyncImpl = _ =>
        {
            var refused = new Result<NoteExtendedDto> { NotAuthorized = true };
            refused.AddErrorMessage("The user 'jdoe' needs the role 'Staff' in the repository 'Shared'.");
            return Task.FromResult(refused);
        };

        var created = await ctrl.NewModel(service);

        Assert.IsFalse(created);
        StringAssert.Contains(notified.Single(), "needs the role 'Staff'");
        Assert.IsNull(view.LastShownInfo, "A refusal is told like a refused controller, not through the editor's view.");
    }

    [TestMethod]
    public async Task Autosave_SkipsNotesOpenInConsultMode()
    {
        var (ctrl, _, service, store) = CreateCtrl(EnumRoles.Guest);
        await ctrl.LoadModelById(service, Guid.NewGuid(), refreshView: false);
        ctrl.EditMode = true;
        ctrl.Model.Topic = "Changed";
        var saveCalled = false;
        service.NotesFake.SaveExtendedAsyncImpl = n => { saveCalled = true; return Task.FromResult(new Result<NoteExtendedDto>(n)); };

        await store.SaveActiveNotes();

        Assert.IsFalse(saveCalled);
    }
}
