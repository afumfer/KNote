using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Tests;

/// <summary>
/// AtlasContextCtrl (Tools › KNote Atlas): the window reads the profile, the glossary, the tasks and what the AI receives
/// of one repository, the user chooses the repository there, and each button depends on the role of its command.
/// </summary>
[TestClass]
public class AtlasContextCtrlTests
{
    // A linked repository whose context answers with these values and remembers what it was asked.
    private sealed class Repository
    {
        public Repository(Store store, EnumRoles role)
        {
            var service = new FakeKntService();
            Atlas = service.AtlasContextFake;
            Atlas.GetProfileInfoAsyncImpl = () =>
            {
                Reads++;
                return Task.FromResult(new Result<AtlasProfileInfoDto>(Profile));
            };
            Atlas.GetGlossaryInfoAsyncImpl = () => Task.FromResult(new Result<AtlasGlossaryInfoDto>(Glossary));
            Atlas.GetTaskPromptsAsyncImpl = () => Task.FromResult(new Result<List<AtlasTaskPromptStatusDto>>(Tasks));
            Atlas.ComposeAsyncImpl = request =>
            {
                ComposeRequests.Add(request);
                return Task.FromResult(new Result<AiComposedPromptDto>(Composed));
            };

            Ref = TestServiceRefFactory.CreateWithFakeService(service);
            store.AddServiceRef(Ref);
            store.Security.SetRepositoryRole(service, role);
        }

        public ServiceRef Ref { get; }
        public FakeKntAtlasContextService Atlas { get; }
        public AtlasProfileInfoDto Profile { get; } = new();
        public AtlasGlossaryInfoDto Glossary { get; } = new();
        public List<AtlasTaskPromptStatusDto> Tasks { get; } = new();
        public AiComposedPromptDto Composed { get; } = new();
        public int Reads { get; private set; }
        public List<AiComposeRequestDto> ComposeRequests { get; } = new();
    }

    private static (AtlasContextCtrl Ctrl, FakeBaseView View, Repository First, Repository Second) CreateCtrl(EnumRoles role = EnumRoles.Admin)
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeBaseView();
        factoryViews.Registry.Register<AtlasContextCtrl, IViewBase>(_ => view);
        var store = new Store(factoryViews);
        var first = new Repository(store, role);
        var second = new Repository(store, role);

        var ctrl = new AtlasContextCtrl(store);
        ctrl.Run();
        return (ctrl, view, first, second);
    }

    [TestMethod]
    public async Task LoadAsync_ReadsEverythingOfTheActiveRepository()
    {
        var (ctrl, _, first, _) = CreateCtrl();

        await ctrl.LoadAsync();

        Assert.AreEqual(first.Ref, ctrl.ServiceRef);
        Assert.AreSame(first.Profile, ctrl.Profile);
        Assert.AreSame(first.Glossary, ctrl.Glossary);
        Assert.AreSame(first.Tasks, ctrl.Tasks);
        Assert.AreSame(first.Composed, ctrl.Composed);
        Assert.AreEqual(AiTaskCatalog.AssistantKey, first.ComposeRequests.Single().TaskKey);
    }

    [TestMethod]
    public async Task Repositories_AreTheLinkedOnes_AndChoosingOneReadsIt()
    {
        var (ctrl, _, first, second) = CreateCtrl();
        await ctrl.LoadAsync();

        await ctrl.SelectRepositoryAsync(second.Ref);

        CollectionAssert.AreEqual(new[] { first.Ref, second.Ref }, ctrl.Repositories);
        Assert.AreEqual(second.Ref, ctrl.ServiceRef);
        Assert.AreSame(second.Profile, ctrl.Profile);
    }

    [TestMethod]
    public async Task ARepositoryChosenWhileReading_WinsOverTheLateAnswer()
    {
        var (ctrl, _, first, second) = CreateCtrl();
        var answer = new TaskCompletionSource<Result<AtlasProfileInfoDto>>();
        first.Atlas.GetProfileInfoAsyncImpl = () => answer.Task;

        var reading = ctrl.LoadAsync();
        await ctrl.SelectRepositoryAsync(second.Ref);
        answer.SetResult(new Result<AtlasProfileInfoDto>(first.Profile));
        await reading;

        Assert.AreSame(second.Profile, ctrl.Profile);
    }

    [TestMethod]
    public async Task AGuest_ReadsTheNotes_ButNeitherSeesWhatTheAiReceivesNorCreatesThem()
    {
        var (ctrl, view, first, _) = CreateCtrl(EnumRoles.Guest);

        await ctrl.LoadAsync();

        Assert.AreSame(first.Profile, ctrl.Profile);
        Assert.IsNull(ctrl.Composed);
        Assert.AreEqual(0, first.ComposeRequests.Count);
        Assert.IsNull(view.LastShownInfo, "Not being able to see it is not an error.");
        Assert.IsFalse(ctrl.CanCompose || ctrl.CanCreateProfile || ctrl.CanFillProfile || ctrl.CanCreateGlossary || ctrl.CanCustomizeTask);
    }

    [TestMethod]
    public void TheButtons_FollowTheRoleOfTheirCommands()
    {
        var (staff, _, _, _) = CreateCtrl(EnumRoles.Staff);
        var (manager, _, _, _) = CreateCtrl(EnumRoles.ProjectManager);

        Assert.IsTrue(staff.CanCompose);
        Assert.IsFalse(staff.CanCreateProfile || staff.CanFillProfile || staff.CanCreateGlossary || staff.CanCustomizeTask);
        Assert.IsTrue(manager.CanCreateProfile && manager.CanFillProfile && manager.CanCreateGlossary && manager.CanCustomizeTask);
    }

    [TestMethod]
    public async Task TheTestText_GoesIntoWhatTheAiReceives_AndAnEmptyOneDoesNot()
    {
        var (ctrl, _, first, _) = CreateCtrl();

        await ctrl.ComposeAsync();
        ctrl.TestText = "My CPC464";
        await ctrl.ComposeAsync();

        Assert.AreEqual(0, first.ComposeRequests[0].Texts.Count);
        CollectionAssert.AreEqual(new[] { "My CPC464" }, first.ComposeRequests[1].Texts);
    }

    [TestMethod]
    public async Task AFailedRead_IsTold_AndShowsNothingOfTheRepositoryBefore()
    {
        var (ctrl, view, _, second) = CreateCtrl();
        await ctrl.LoadAsync();
        var failed = new Result<AtlasProfileInfoDto>();
        failed.AddErrorMessage("The database is not there.");
        second.Atlas.GetProfileInfoAsyncImpl = () => Task.FromResult(failed);

        await ctrl.SelectRepositoryAsync(second.Ref);

        Assert.IsNull(ctrl.Profile);
        StringAssert.Contains(view.LastShownInfo, "The database is not there.");
    }

    [TestMethod]
    public async Task CreatingANote_ReadsAgain_AndARefusalIsTold()
    {
        var (ctrl, view, first, _) = CreateCtrl();
        first.Atlas.CreateProfileAsyncImpl = () => Task.FromResult(new Result<NoteDto>((NoteDto)null!));

        await ctrl.CreateProfileAsync();

        Assert.AreEqual(1, first.Reads);

        var refused = new Result<NoteDto>();
        refused.AddErrorMessage("Refused.");
        first.Atlas.CreateProfileAsyncImpl = () => Task.FromResult(refused);

        await ctrl.CreateProfileAsync();

        Assert.AreEqual(1, first.Reads);
        StringAssert.Contains(view.LastShownInfo, "Refused.");
    }

    [TestMethod]
    public async Task ANoteSavedAfterClosingTheWindow_ReadsNothing()
    {
        var (ctrl, _, first, _) = CreateCtrl();
        ctrl.Finalize();

        await ctrl.LoadAsync();

        Assert.AreEqual(0, first.Reads);
    }

    [TestMethod]
    public void KNoteInstructions_AreTheFixedRulesAndTheDefaultOnes()
    {
        var text = AtlasContextCtrl.KNoteInstructions(AiTaskCatalog.AssistantKey);

        StringAssert.Contains(text, AiTaskCatalog.Assistant.FixedRules);
        StringAssert.Contains(text, AiTaskCatalog.Assistant.DefaultInstructions);
        Assert.AreEqual("", AtlasContextCtrl.KNoteInstructions("Unknown"));
    }
}
