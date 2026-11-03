using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// NoteEditorCtrl and the similar notes of the Trace notes tab (KNote Atlas 0.5): nothing to show for a new note or a
/// repository that can't search by meaning; the notes of the relations added in the editor and not saved yet are left
/// out too. The list itself is NoteEditorForm's (not covered here).
/// </summary>
[TestClass]
public class NoteEditorCtrlSimilarNotesTests
{
    private static readonly Guid Similar = Guid.NewGuid();
    private static readonly Guid Other = Guid.NewGuid();

    private static (NoteEditorCtrl Ctrl, FakeKntService Service, List<Guid> Asked) CreateCtrl(bool canSearch = true)
    {
        var factoryViews = new TestFactoryViews();
        factoryViews.Registry.Register<NoteEditorCtrl, IViewNoteEditorEmbeddable<NoteExtendedDto>>(c => new FakeNoteEditorView());
        var service = new FakeKntService { RepositoryRef = new RepositoryRef { Name = "Shared" } };
        var store = new Store(factoryViews);
        store.GrantAdmin(service);

        service.NotesFake.GetExtendedAsyncImpl = id => Task.FromResult(new Result<NoteExtendedDto>(new NoteExtendedDto
        {
            NoteId = id, Topic = "A note", FolderId = Guid.NewGuid(), Description = "", Tags = ""
        }));
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () => Task.FromResult(new Result<SemanticIndexStatusDto>(new SemanticIndexStatusDto
        {
            IndexedNotes = canSearch ? 10 : 0, EmbeddingModelAvailable = true
        }));
        var asked = new List<Guid>();
        service.SemanticSearchFake.GetSimilarNotesAsyncImpl = noteId =>
        {
            asked.Add(noteId);
            return Task.FromResult(new Result<SemanticSearchResultDto>(new SemanticSearchResultDto
            {
                Notes =
                {
                    new NoteSearchHitDto { NoteId = Similar, NoteNumber = 2, Topic = "Similar", Similarity = 0.8f },
                    new NoteSearchHitDto { NoteId = Other, NoteNumber = 3, Topic = "Other", Similarity = 0.7f }
                }
            }));
        };
        return (new NoteEditorCtrl(store), service, asked);
    }

    [TestMethod]
    public async Task ASavedNote_GetsItsSimilarNotes()
    {
        var (ctrl, service, asked) = CreateCtrl();
        var noteId = Guid.NewGuid();
        await ctrl.LoadModelById(service, noteId, refreshView: false);

        var result = await ctrl.GetSimilarNotesAsync();

        Assert.AreEqual(noteId, asked.Single());
        CollectionAssert.AreEqual(new[] { Similar, Other }, result.Entity.Notes.Select(n => n.NoteId).ToList());
    }

    [TestMethod]
    public async Task ANewNote_OrARepositoryThatCantSearchByMeaning_HasNone()
    {
        var (ctrl, service, asked) = CreateCtrl(canSearch: false);
        await ctrl.LoadModelById(service, Guid.NewGuid(), refreshView: false);
        Assert.IsNull(await ctrl.GetSimilarNotesAsync());

        (ctrl, service, asked) = CreateCtrl();
        await ctrl.LoadModelById(service, Guid.NewGuid(), refreshView: false);
        ctrl.Model.SetIsNew(true);
        Assert.IsNull(await ctrl.GetSimilarNotesAsync());

        Assert.AreEqual(0, asked.Count);
    }

    [TestMethod]
    public async Task TheRelationsNotSavedYet_LeaveTheirNotesOut()
    {
        var (ctrl, service, _) = CreateCtrl();
        await ctrl.LoadModelById(service, Guid.NewGuid(), refreshView: false);
        ctrl.Model.TraceNotesTo.Add(new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = ctrl.Model.NoteId, ToId = Similar });

        var result = await ctrl.GetSimilarNotesAsync();

        CollectionAssert.AreEqual(new[] { Other }, result.Entity.Notes.Select(n => n.NoteId).ToList());
    }
}
