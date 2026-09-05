using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for NotesSelectorCtrl.PersistFolderOrderNotesAsync: the folder notes-order feature's
/// write-back path, triggered from a grid header click while the folder's order mode is "Auto".
/// The view-side sort/click logic lives in NotesSelectorForm (WinForms, not covered here).
/// </summary>
[TestClass]
public class NotesSelectorCtrlOrderNotesTests
{
    private static (NotesSelectorCtrl ctrl, FakeKntService service) CreateCtrl()
    {
        var store = new Store(new TestFactoryViews()) { AppUserName = "jdoe" };
        var ctrl = new NotesSelectorCtrl(store);
        var service = new FakeKntService();

        return (ctrl, service);
    }

    [TestMethod]
    public async Task PersistFolderOrderNotesAsync_NoFolderLoaded_DoesNothing()
    {
        var (ctrl, service) = CreateCtrl();
        var wasCalled = false;
        service.FoldersFake.UpdateOrderNotesAsyncImpl = (id, order) => { wasCalled = true; return Task.FromResult(new Result()); };

        await ctrl.PersistFolderOrderNotesAsync("AUTO|Topic|ASC");

        Assert.IsFalse(wasCalled);
    }

    [TestMethod]
    public async Task PersistFolderOrderNotesAsync_FolderLoaded_CallsRepositoryAndUpdatesCachedFolder()
    {
        var (ctrl, service) = CreateCtrl();
        var folder = new FolderInfoDto { FolderId = Guid.NewGuid(), OrderNotes = "AUTO" };
        service.NotesFake.GetByFolderMinimalAsyncImpl = _ => Task.FromResult(new Result<List<NoteMinimalDto>> { Entity = new List<NoteMinimalDto>() });
        await ctrl.LoadEntities(service, folder, refreshView: false);

        Guid? capturedFolderId = null;
        string capturedOrderNotes = null;
        service.FoldersFake.UpdateOrderNotesAsyncImpl = (id, order) =>
        {
            capturedFolderId = id;
            capturedOrderNotes = order;
            return Task.FromResult(new Result());
        };

        await ctrl.PersistFolderOrderNotesAsync("AUTO|Topic|ASC");

        Assert.AreEqual(folder.FolderId, capturedFolderId);
        Assert.AreEqual("AUTO|Topic|ASC", capturedOrderNotes);
        Assert.AreEqual("AUTO|Topic|ASC", ctrl.Folder.OrderNotes);
    }

    [TestMethod]
    public async Task PersistFolderOrderNotesAsync_RepositoryFails_DoesNotUpdateCachedFolder()
    {
        var (ctrl, service) = CreateCtrl();
        var folder = new FolderInfoDto { FolderId = Guid.NewGuid(), OrderNotes = "AUTO" };
        service.NotesFake.GetByFolderMinimalAsyncImpl = _ => Task.FromResult(new Result<List<NoteMinimalDto>> { Entity = new List<NoteMinimalDto>() });
        await ctrl.LoadEntities(service, folder, refreshView: false);

        var failedResult = new Result();
        failedResult.AddErrorMessage("DB unavailable");
        service.FoldersFake.UpdateOrderNotesAsyncImpl = (id, order) => Task.FromResult(failedResult);

        await ctrl.PersistFolderOrderNotesAsync("AUTO|Topic|ASC");

        Assert.AreEqual("AUTO", ctrl.Folder.OrderNotes);
    }
}
