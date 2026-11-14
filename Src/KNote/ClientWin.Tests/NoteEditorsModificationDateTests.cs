using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// What the desktop editors do so that a note is only «modified» when its own data changes (KNote Atlas, stage 2, D6 and D8):
/// a note loaded to be edited has empty texts instead of none (the editors give an empty box back as ""), and the post-it saves
/// only its window when only the window changed.
/// </summary>
[TestClass]
public class NoteEditorsModificationDateTests
{
    [TestMethod]
    public async Task TheNoteEditor_LoadsTheTextsWithNoValueAsEmpty_SoThatItsBoxesAreNotAChange()
    {
        var factoryViews = new TestFactoryViews();
        factoryViews.Registry.Register<NoteEditorCtrl, IViewNoteEditorEmbeddable<NoteExtendedDto>>(c => new FakeNoteEditorView());
        var service = new FakeKntService();
        var ctrl = new NoteEditorCtrl(new Store(factoryViews).GrantAdmin(service));
        var note = new NoteExtendedDto { NoteId = Guid.NewGuid(), Topic = "From the web", FolderId = Guid.NewGuid() };
        Assert.IsNull(note.Tags);
        service.NotesFake.GetExtendedAsyncImpl = _ => Task.FromResult(new Result<NoteExtendedDto>(note));

        await ctrl.LoadModelById(service, note.NoteId);
        // What NoteEditorForm.ControlsToModelAsync writes back from its empty boxes.
        ctrl.Model.Tags = "";
        ctrl.Model.InternalTags = "";
        ctrl.Model.Script = "";
        ctrl.Model.Description = "";

        Assert.IsFalse(ctrl.Model.IsDirty());
    }

    [TestMethod]
    public async Task ThePostIt_WithOnlyItsWindowChanged_SavesTheWindow_NotTheNote()
    {
        var (ctrl, service, view) = await LoadPostItAsync();
        var notesSaved = 0;
        var windowsSaved = 0;
        service.NotesFake.SaveAsyncImpl = n => { notesSaved++; return Task.FromResult(new Result<NoteDto>(n)); };
        service.NotesFake.SaveWindowAsyncImpl = w => { windowsSaved++; return Task.FromResult(new Result<WindowDto>(w)); };

        ctrl.WindowPostIt.PosX = 300;
        Assert.IsTrue(await ctrl.SaveModel(), view.LastShownInfo);

        Assert.AreEqual(0, notesSaved, "only its window changed");
        Assert.AreEqual(1, windowsSaved);

        ctrl.Model.Description = "A new text.";
        Assert.IsTrue(await ctrl.SaveModel(), view.LastShownInfo);

        Assert.AreEqual(1, notesSaved, "its text changed");
    }

    private static async Task<(PostItEditorCtrl Ctrl, FakeKntService Service, FakePostItEditorView View)> LoadPostItAsync()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakePostItEditorView();
        factoryViews.Registry.Register<PostItEditorCtrl, IViewPostItEditor<NoteDto>>(c => view);
        var service = new FakeKntService();
        var ctrl = new PostItEditorCtrl(new Store(factoryViews).GrantAdmin(service));
        var user = new UserDto { UserId = Guid.NewGuid(), UserName = "postit" };
        var folder = new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 1, Name = "Home" };
        var note = new NoteDto { NoteId = Guid.NewGuid(), Topic = "A post-it", FolderId = folder.FolderId, FolderDto = folder, ContentType = "markdown" };
        service.UsersFake.GetByUserNameAsyncImpl = _ => Task.FromResult(new Result<UserDto>(user));
        service.NotesFake.GetByIdAsyncImpl = _ => Task.FromResult(new Result<NoteDto>(note));
        service.NotesFake.GetWindowAsyncImpl = (noteId, userId) =>
            Task.FromResult(new Result<WindowDto>(new WindowDto { WindowId = Guid.NewGuid(), NoteId = noteId, UserId = userId, PosX = 10, PosY = 10 }));
        service.NotesFake.SaveWindowAsyncImpl = w => Task.FromResult(new Result<WindowDto>(w));

        Assert.IsTrue(await ctrl.LoadModelById(service, note.NoteId));
        return (ctrl, service, view);
    }
}
