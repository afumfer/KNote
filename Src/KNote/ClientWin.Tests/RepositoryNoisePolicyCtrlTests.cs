using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// RepositoryNoisePolicyCtrl («Noise policy» of the KNote Atlas window, KNote Atlas 2.2): the policy read with the repository's
/// note types (without KNote's own) and the paths of its folders, the recount of the policy as it is edited, folders added one by
/// one (not KNote's own), saved only by a ProjectManager or an Admin, and the changes not saved asked about.
/// </summary>
[TestClass]
public class RepositoryNoisePolicyCtrlTests
{
    private static readonly Guid MeetingId = Guid.NewGuid();
    private static readonly Guid InboxId = Guid.NewGuid();
    private static readonly Guid ArchiveId = Guid.NewGuid();

    private sealed class Fixture
    {
        public RepositoryNoisePolicyCtrl Ctrl { get; init; } = null!;
        public FakeNoisePolicyView View { get; init; } = null!;
        public FakeKntService Service { get; init; } = null!;
        public NoisePolicyDto Stored { get; set; } = new();
        public List<NoisePolicyDto> Counted { get; } = new();
        public List<NoisePolicyDto> SavedPolicies { get; } = new();
    }

    private static Fixture Create(EnumRoles role = EnumRoles.ProjectManager)
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeNoisePolicyView();
        factoryViews.Registry.Register<RepositoryNoisePolicyCtrl, IViewNoisePolicy>(_ => view);
        var store = new Store(factoryViews);
        var service = new FakeKntService();
        store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));
        store.Security.SetRepositoryRole(service, role);

        var fixture = new Fixture { Ctrl = new RepositoryNoisePolicyCtrl(store), View = view, Service = service };
        service.SummariesFake.GetNoisePolicyAsyncImpl = () => Task.FromResult(new Result<NoisePolicyDto>(fixture.Stored));
        service.SummariesFake.CountNoiseAsyncImpl = policy =>
        {
            fixture.Counted.Add(policy);
            return Task.FromResult(new Result<NoiseCountDto>(new NoiseCountDto
            {
                Notes = 10, ExcludedNoteType = policy.ExcludedNoteTypeIds.Count, ExcludedFolder = policy.ExcludedFolderIds.Count
            }));
        };
        service.SummariesFake.SaveNoisePolicyAsyncImpl = policy =>
        {
            fixture.SavedPolicies.Add(policy);
            fixture.Stored = policy;
            return Task.FromResult(new Result<NoisePolicyDto>(policy));
        };
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>
        {
            new() { NoteTypeId = Guid.NewGuid(), Name = KntConst.ChatSessionsTag },
            new() { NoteTypeId = MeetingId, Name = "Meeting" },
            new() { NoteTypeId = Guid.NewGuid(), Name = "Book" }
        }));
        var folders = new Dictionary<Guid, string> { [InboxId] = "Inbox", [ArchiveId] = "Archive" };
        service.FoldersFake.GetByIdAsyncImpl = id => Task.FromResult(new Result<FolderDto>(new FolderDto { FolderId = id, Name = folders[id] }));
        return fixture;
    }

    [TestMethod]
    public async Task Load_ShowsThePolicy_TheNoteTypesWithoutKNotesOwn_AndItsCount()
    {
        var fixture = Create();
        fixture.Stored = new NoisePolicyDto { ExcludedNoteTypeIds = { MeetingId }, ExcludedFolderIds = { InboxId } };

        Assert.IsTrue(await fixture.Ctrl.LoadAsync(fixture.Service));

        CollectionAssert.AreEqual(new[] { "Book", "Meeting" }, fixture.Ctrl.NoteTypes.Select(t => t.Name).ToList());
        Assert.IsTrue(fixture.Ctrl.IsExcluded(MeetingId));
        Assert.AreEqual((InboxId, "Inbox"), fixture.Ctrl.Folders.Single());
        StringAssert.StartsWith(fixture.Ctrl.CountText, "Of 10 notes");
        Assert.IsFalse(fixture.Ctrl.IsChanged);
        Assert.IsTrue(fixture.Ctrl.CanSave);
        Assert.AreEqual(1, fixture.View.RefreshCountCount);
    }

    [TestMethod]
    public async Task EachChange_IsCountedAgain_WithThePolicyNotSavedYet()
    {
        var fixture = Create();
        await fixture.Ctrl.LoadAsync(fixture.Service);

        await fixture.Ctrl.SetNoteTypeExcludedAsync(MeetingId, true);
        Assert.IsTrue(await fixture.Ctrl.AddFolderAsync(new FolderInfoDto { FolderId = ArchiveId, Name = "Archive" }));
        Assert.IsFalse(await fixture.Ctrl.AddFolderAsync(new FolderInfoDto { FolderId = ArchiveId, Name = "Archive" }), "once");

        Assert.HasCount(3, fixture.Counted);
        CollectionAssert.AreEqual(new[] { MeetingId }, fixture.Counted.Last().ExcludedNoteTypeIds);
        CollectionAssert.AreEqual(new[] { ArchiveId }, fixture.Counted.Last().ExcludedFolderIds);
        Assert.AreEqual((ArchiveId, "Archive"), fixture.Ctrl.Folders.Single());
        Assert.IsTrue(fixture.Ctrl.IsChanged);
        Assert.IsEmpty(fixture.Stored.ExcludedNoteTypeIds, "counting saves nothing");

        await fixture.Ctrl.RemoveFolderAsync(ArchiveId);
        await fixture.Ctrl.SetNoteTypeExcludedAsync(MeetingId, false);
        Assert.IsFalse(fixture.Ctrl.IsChanged, "back to what is saved");
    }

    [TestMethod]
    public async Task CountAgain_ReadsTheNotesAgain_WithThePolicyAsItIs()
    {
        var fixture = Create();
        await fixture.Ctrl.LoadAsync(fixture.Service);
        await fixture.Ctrl.SetNoteTypeExcludedAsync(MeetingId, true);
        Assert.IsTrue(fixture.Ctrl.CanCountAgain);

        await fixture.Ctrl.CountAgainAsync();

        Assert.HasCount(3, fixture.Counted);
        CollectionAssert.AreEqual(new[] { MeetingId }, fixture.Counted.Last().ExcludedNoteTypeIds, "not saved yet, as it is edited");
        Assert.IsEmpty(fixture.SavedPolicies);
    }

    [TestMethod]
    public async Task KNotesOwnFolders_AreNotAdded()
    {
        var fixture = Create();
        await fixture.Ctrl.LoadAsync(fixture.Service);

        Assert.IsFalse(await fixture.Ctrl.AddFolderAsync(new FolderInfoDto { FolderId = Guid.NewGuid(), Name = KntConst.AtlasFolderName }));

        StringAssert.Contains(fixture.View.ShownInfo.Single(), "always left out");
        Assert.IsEmpty(fixture.Ctrl.Folders);
    }

    [TestMethod]
    public async Task AddFolder_UsesTheFolderChosen_OrNothingIfCanceled()
    {
        var fixture = Create();
        await fixture.Ctrl.LoadAsync(fixture.Service);

        fixture.Ctrl.ChooseFolder = () => null!;
        Assert.IsFalse(await fixture.Ctrl.ChooseAndAddFolderAsync());
        fixture.Ctrl.ChooseFolder = () => new FolderInfoDto { FolderId = InboxId, Name = "Inbox" };
        Assert.IsTrue(await fixture.Ctrl.ChooseAndAddFolderAsync());

        Assert.AreEqual(InboxId, fixture.Ctrl.Folders.Single().FolderId);
    }

    [TestMethod]
    public async Task Save_KeepsThePolicy_AndShowsWhatTheServiceKept()
    {
        var fixture = Create();
        await fixture.Ctrl.LoadAsync(fixture.Service);
        await fixture.Ctrl.SetNoteTypeExcludedAsync(MeetingId, true);

        Assert.IsTrue(await fixture.Ctrl.SaveAsync());

        CollectionAssert.AreEqual(new[] { MeetingId }, fixture.SavedPolicies.Single().ExcludedNoteTypeIds);
        Assert.IsFalse(fixture.Ctrl.IsChanged);
        Assert.IsTrue(fixture.Ctrl.IsExcluded(MeetingId));
    }

    [TestMethod]
    [DataRow(EnumRoles.Guest)]
    [DataRow(EnumRoles.Staff)]
    public async Task BelowAProjectManager_ItIsOnlySeen(EnumRoles role)
    {
        var fixture = Create(role);
        await fixture.Ctrl.LoadAsync(fixture.Service);

        Assert.IsFalse(fixture.Ctrl.CanSave);
        StringAssert.Contains(fixture.Ctrl.ReadOnlyText, "ProjectManager");
        await fixture.Ctrl.SetNoteTypeExcludedAsync(MeetingId, true);
        Assert.IsFalse(await fixture.Ctrl.SaveAsync());
        Assert.IsEmpty(fixture.SavedPolicies);
    }

    [TestMethod]
    [DataRow(DialogResult.Yes, true, 1)]
    [DataRow(DialogResult.No, true, 0)]
    [DataRow(DialogResult.Cancel, false, 0)]
    public async Task ChangesNotSaved_AreAskedAbout(DialogResult answer, bool resolved, int saves)
    {
        var fixture = Create();
        await fixture.Ctrl.LoadAsync(fixture.Service);
        await fixture.Ctrl.SetNoteTypeExcludedAsync(MeetingId, true);
        fixture.View.ShowInfoImpl = (_, buttons) => buttons == MessageBoxButtons.YesNoCancel ? answer : DialogResult.OK;

        Assert.AreEqual(resolved, await fixture.Ctrl.ResolvePendingChangesAsync());

        StringAssert.Contains(fixture.View.ShownInfo.Single(), "not saved");
        Assert.HasCount(saves, fixture.SavedPolicies);
        Assert.AreEqual(answer == DialogResult.Cancel, fixture.Ctrl.IsChanged);
    }
}
