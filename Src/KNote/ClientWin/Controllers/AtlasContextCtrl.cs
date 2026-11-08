using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// KNote Atlas (Tools › KNote Atlas, chapters 19–21): the context the AI receives in a repository, in one window. The
/// repository's profile, its glossary, the instructions of each task (notes written in the note editor of always, which
/// only ProjectManager and Admin save: a rule of the Service) and what the AI receives, all of them read together. It
/// starts in the active repository and the user can choose another one: the assistant has its own.
/// </summary>
[KntAuthorize(EnumRoles.Guest)]
public class AtlasContextCtrl : CtrlViewBase<IViewBase>
{
    public AtlasContextCtrl(Store store) : base(store)
    {
        ControllerName = "KNote Atlas";
        ServiceRef = store.GetActiveOrDefaultServiceRef();
    }

    public ServiceRef ServiceRef { get; private set; }

    public IKntService Service => ServiceRef?.Service;

    protected override IKntService AuthorizationResource => Service;

    public string RepositoryName => ServiceRef?.Name;

    public List<ServiceRef> Repositories => Store.GetAllServiceRef();

    public AtlasProfileInfoDto Profile { get; private set; }

    public AtlasGlossaryInfoDto Glossary { get; private set; }

    public List<AtlasTaskPromptStatusDto> Tasks { get; private set; } = new();

    // What the AI receives for TaskKey, with the glossary terms of TestText. Null if the user cannot see it (Staff).
    public AiComposedPromptDto Composed { get; private set; }

    public string TaskKey { get; set; } = AiTaskCatalog.AssistantKey;

    public string TestText { get; set; } = "";

    public bool CanCreateProfile => IsAllowed(typeof(KntAtlasContextCreateProfileAsyncCommand));

    public bool CanFillProfile => IsAllowed(typeof(KntAtlasContextFillProfileFoldersAndTypesAsyncCommand));

    public bool CanCreateGlossary => IsAllowed(typeof(KntAtlasContextCreateGlossaryAsyncCommand));

    public bool CanCustomizeTask => IsAllowed(typeof(KntAtlasContextCustomizeTaskAsyncCommand));

    public bool CanCompose => IsAllowed(typeof(KntAtlasContextComposeAsyncCommand));

    protected override IViewBase CreateView() => Store.FactoryViews.Registry.Resolve<AtlasContextCtrl, IViewBase>(this);

    // The note editor can outlive the window, and it asks for a new read when a note is saved.
    private bool IsClosed => !PreconditionsMet || ControllerState == EControllerState.Finalized;

    /// <summary>Reads everything of the repository; the user is told of the first thing that failed.</summary>
    public async Task LoadAsync()
    {
        if (IsClosed)
            return;

        var service = Service;
        var profile = await service.AtlasContext.GetProfileInfoAsync();
        var glossary = await service.AtlasContext.GetGlossaryInfoAsync();
        var tasks = await service.AtlasContext.GetTaskPromptsAsync();
        var composed = CanCompose ? await ComposeResultAsync(service) : null;

        // Another repository was chosen meanwhile: its own read shows it.
        if (IsClosed || service != Service)
            return;

        Profile = profile.IsValid ? profile.Entity : null;
        Glossary = glossary.IsValid ? glossary.Entity : null;
        Tasks = tasks.IsValid ? tasks.Entity : new();
        Composed = composed?.IsValid == true ? composed.Entity : null;

        var failed = new ResultBase[] { profile, glossary, tasks, composed }.FirstOrDefault(r => r?.IsValid == false);
        if (failed != null)
            ShowResultError(failed);
        View.RefreshView();
    }

    public async Task SelectRepositoryAsync(ServiceRef repository)
    {
        ServiceRef = repository ?? throw new ArgumentNullException(nameof(repository));
        await LoadAsync();
    }

    public async Task CreateProfileAsync() => await ShowNoteAsync(await Service.AtlasContext.CreateProfileAsync());

    public async Task FillProfileAsync() => await ShowNoteAsync(await Service.AtlasContext.FillProfileFoldersAndTypesAsync());

    public async Task CreateGlossaryAsync() => await ShowNoteAsync(await Service.AtlasContext.CreateGlossaryAsync());

    /// <summary>«Customize»: the note of instructions of the task, created (Mode Add) if it has none, in the note editor.</summary>
    public async Task CustomizeTaskAsync(string taskKey) => await ShowNoteAsync(await Service.AtlasContext.CustomizeTaskAsync(taskKey));

    /// <summary>«See KNote's»: the fixed rules and the default instructions of the task, as the AI receives them.</summary>
    public static string KNoteInstructions(string taskKey)
    {
        var task = AiTaskCatalog.Find(taskKey);
        return task == null ? "" : $"# {AiPromptComposer.RulesLayer}\n\n{task.FixedRules}\n\n# {AiPromptComposer.InstructionsLayer}\n\n{task.DefaultInstructions}";
    }

    /// <summary>What the AI receives, again: another task or another test text.</summary>
    public async Task ComposeAsync()
    {
        var res = await ComposeResultAsync(Service);
        if (res.IsValid)
            Composed = res.Entity;
        else
            ShowResultError(res);
        View.RefreshView();
    }

    public async Task<bool> OpenInEditorAsync(Guid noteId)
    {
        var noteEditorCtrl = new NoteEditorCtrl(Store);
        if (!await noteEditorCtrl.LoadModelById(Service, noteId, false))
        {
            noteEditorCtrl.Dispose();
            return false;
        }
        noteEditorCtrl.SavedEntity += async (_, _) => await LoadAsync();
        noteEditorCtrl.Run();
        return true;
    }

    private Task<Result<AiComposedPromptDto>> ComposeResultAsync(IKntService service) =>
        service.AtlasContext.ComposeAsync(new AiComposeRequestDto
        {
            TaskKey = TaskKey,
            Texts = string.IsNullOrWhiteSpace(TestText) ? new List<string>() : new List<string> { TestText }
        });

    private bool IsAllowed(Type command) => Service != null && Store.Security.IsAuthorized(command, Service);

    // A note created or changed by a command: the window shows it and the note editor opens it.
    private async Task ShowNoteAsync(Result<NoteDto> res)
    {
        if (!res.IsValid)
        {
            ShowResultError(res);
            return;
        }
        await LoadAsync();
        if (res.Entity != null)
            await OpenInEditorAsync(res.Entity.NoteId);
    }
}
