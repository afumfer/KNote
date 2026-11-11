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

    // KNote Atlas 4.6b: the photos or the documents found, a row for each one (a note found with its resource).
    [TestMethod]
    public async Task ASearchOfPhotos_LoadsTheResourcesFound_AsRowsOfTheList()
    {
        var store = new Store(new TestFactoryViews()) { AppUserName = "jdoe" };
        var ctrl = new NotesSelectorCtrl(store);
        var service = new FakeKntService();
        SemanticSearchRequestDto? asked = null;
        var photo = new ResourceInfoDto { ResourceId = Guid.NewGuid(), Name = $"{Guid.NewGuid()}_beach.png", FileType = "image/png", Description = "The beach." };
        service.SemanticSearchFake.SearchAsyncImpl = request =>
        {
            asked = request;
            return Task.FromResult(new Result<SemanticSearchResultDto>(new SemanticSearchResultDto
            {
                Resources = { new ResourceSearchHitDto { NoteId = Guid.NewGuid(), NoteNumber = 3, Topic = "Holidays", Similarity = 0.66f, Resource = photo } }
            }));
        };

        await ctrl.LoadSearchEntities(service, new NotesSearchDto
        {
            TextSearch = "beach", Mode = EnumNotesSearchMode.Meaning, ResourceKind = EnumResourceKind.Photos
        }, refreshView: false);

        Assert.AreEqual(EnumResourceKind.Photos, asked!.ResourceKind);
        Assert.IsTrue(ctrl.ByResources);
        Assert.AreEqual(EnumResourceKind.Photos, ctrl.ResourceKind);
        Assert.AreEqual(photo, ctrl.ResourceHits.Single().Resource);
        Assert.AreEqual("Holidays", ctrl.ListEntities.Single().Topic, "a row of the notes list");

        await ctrl.LoadSearchEntities(service, new NotesSearchDto { TextSearch = "beach", Mode = EnumNotesSearchMode.Meaning }, refreshView: false);
        Assert.IsFalse(ctrl.ByResources, "All: the notes");
    }

    // 4.6b: a search canceled (the «Cancel» of its progress) leaves the list as it was.
    [TestMethod]
    public async Task ACanceledSearch_LeavesTheListAsItWas()
    {
        var (ctrl, service, _) = CreateCtrl();
        await ctrl.LoadSearchEntities(service, new NotesSearchDto { TextSearch = "keyboard", Mode = EnumNotesSearchMode.Meaning }, refreshView: false);
        var before = ctrl.ListEntities;
        var never = new TaskCompletionSource<Result<List<NoteMinimalDto>>>();
        service.NotesFake.GetSearchMinimalAsyncImpl = _ => never.Task;
        using var cancellation = new CancellationTokenSource();

        var loading = ctrl.LoadSearchEntities(service, new NotesSearchDto { TextSearch = "slow" }, refreshView: false, cancellation.Token);
        cancellation.Cancel();
        await loading;

        Assert.IsTrue(ctrl.LastLoadCanceled);
        Assert.AreSame(before, ctrl.ListEntities);
        Assert.IsTrue(ctrl.ByMeaning, "as it was");
    }

    // The wait cursor of the app comes back to the arrow when the last one ends, whatever their order (an async void that
    // outlived the one around it left it for ever).
    [TestMethod]
    public void TheWaitCursor_EndsWithTheLastOne_InAnyOrder()
    {
        var outer = new WaitCursor();
        var inner = new WaitCursor();
        outer.Dispose();
        Assert.IsTrue(Application.UseWaitCursor);
        inner.Dispose();
        Assert.IsFalse(Application.UseWaitCursor);
        inner.Dispose();
        Assert.IsFalse(Application.UseWaitCursor, "twice, nothing");
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
