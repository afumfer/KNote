using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// NotesSelectorCtrl and the search by meaning (KNote Atlas): the search and the filter with «Talks about…» load the
/// notes found by meaning (NoteSearchHitDto) and the notes not up to date; by words and without «Talks about…» they
/// load as always; «Update index» searches again. The columns and the notice are NotesSelectorForm's (not covered here).
/// </summary>
[TestClass]
public class NotesSelectorCtrlMeaningTests
{
    private static (NotesSelectorCtrl Ctrl, FakeKntService Service, List<SemanticSearchRequestDto> Requests) CreateCtrl(int pendingNotes = 3)
    {
        var store = new Store(new TestFactoryViews()) { AppUserName = "jdoe" };
        var ctrl = new NotesSelectorCtrl(store);
        var service = new FakeKntService();
        var requests = new List<SemanticSearchRequestDto>();
        service.SemanticSearchFake.SearchAsyncImpl = request =>
        {
            requests.Add(request);
            return Task.FromResult(new Result<SemanticSearchResultDto>(new SemanticSearchResultDto
            {
                Notes = { new NoteSearchHitDto { NoteId = Guid.NewGuid(), NoteNumber = 7, Topic = "Spectrum", Similarity = 0.71f, Fragment = "Replaced the membrane" } },
                PendingNotes = pendingNotes
            }));
        };
        return (ctrl, service, requests);
    }

    [TestMethod]
    public async Task ASearchByMeaning_LoadsTheNotesFound_AndTheNotesNotUpToDate()
    {
        var (ctrl, service, requests) = CreateCtrl();

        await ctrl.LoadSearchEntities(service, new NotesSearchDto { TextSearch = "keyboard", Mode = EnumNotesSearchMode.Meaning }, refreshView: false);

        Assert.AreEqual("keyboard", requests.Single().Text);
        Assert.IsNull(requests.Single().Filter);
        Assert.IsTrue(ctrl.ByMeaning);
        Assert.AreEqual(3, ctrl.PendingNotes);
        Assert.AreEqual(0.71f, ((NoteSearchHitDto)ctrl.ListEntities.Single()).Similarity);
    }

    [TestMethod]
    public async Task ASearchByWords_LoadsAsAlways()
    {
        var (ctrl, service, requests) = CreateCtrl();
        await ctrl.LoadSearchEntities(service, new NotesSearchDto { TextSearch = "keyboard", Mode = EnumNotesSearchMode.Meaning }, refreshView: false);
        service.NotesFake.GetSearchMinimalAsyncImpl = _ =>
            Task.FromResult(new Result<List<NoteMinimalDto>>(new List<NoteMinimalDto> { new() { NoteNumber = 1, Topic = "Keyboard" } }));

        await ctrl.LoadSearchEntities(service, new NotesSearchDto { TextSearch = "keyboard" }, refreshView: false);

        Assert.AreEqual(1, requests.Count);
        Assert.IsFalse(ctrl.ByMeaning);
        Assert.AreEqual(0, ctrl.PendingNotes);
        Assert.AreEqual("Keyboard", ctrl.ListEntities.Single().Topic);
    }

    [TestMethod]
    public async Task AFilterWithTalksAbout_SearchesByMeaningAmongItsNotes_WithoutIt_FiltersAsAlways()
    {
        var (ctrl, service, requests) = CreateCtrl();
        var folderId = Guid.NewGuid();
        var filtered = 0;
        service.NotesFake.GetFilterMinimalAsyncImpl = _ =>
        {
            filtered++;
            return Task.FromResult(new Result<List<NoteMinimalDto>>(new List<NoteMinimalDto>()));
        };

        await ctrl.LoadFilteredEntities(service, new NotesFilterDto { FolderId = folderId, MeaningText = "monitor" }, refreshView: false);
        var byMeaning = ctrl.ByMeaning;
        await ctrl.LoadFilteredEntities(service, new NotesFilterDto { FolderId = folderId, MeaningText = " " }, refreshView: false);

        Assert.IsTrue(byMeaning);
        Assert.AreEqual("monitor", requests.Single().Text);
        Assert.AreEqual(folderId, requests.Single().Filter.FolderId);
        Assert.AreEqual(1, filtered);
        Assert.IsFalse(ctrl.ByMeaning);
    }

    [TestMethod]
    public async Task UpdateIndex_SearchesAgain_OnlyAfterASearchByMeaning()
    {
        var (ctrl, service, requests) = CreateCtrl(pendingNotes: 0);
        Assert.IsFalse(await ctrl.UpdateIndexAsync());

        await ctrl.LoadSearchEntities(service, new NotesSearchDto { TextSearch = "keyboard", Mode = EnumNotesSearchMode.Meaning }, refreshView: false);
        await ctrl.UpdateIndexAsync();

        Assert.AreEqual(2, requests.Count);
        Assert.IsTrue(requests.All(r => r.Text == "keyboard"));
    }
}
