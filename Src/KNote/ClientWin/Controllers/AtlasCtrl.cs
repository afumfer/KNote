using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;

namespace KNote.ClientWin.Controllers;

/// <summary>The parts of the KNote Atlas window, in the order of its list.</summary>
public enum EnumAtlasSection
{
    Models,
    Personalization,
    SemanticIndex,
    Resources
}

/// <summary>
/// KNote Atlas (Tools › KNote Atlas): everything of the AI of a repository in one window (KNote Atlas 4.3b), a section for
/// each part, each one for its role: the repository's AI models (RepositoryAiModelsCtrl), the semantic index
/// (RepositorySemanticIndexCtrl) and «Process pending resources» (RepositoryResourcesCtrl), for an Admin; and the AI
/// personalization (chapters 19–21), for everyone: the repository's profile, its glossary, the instructions of each task
/// (notes written in the note editor of always, which only ProjectManager and Admin save: a rule of the Service) and what
/// the AI receives. A section the user can't use is disabled, with who can (WhoCanUse). It starts in the active repository
/// and the user can choose another one (the assistant has its own); the AI models not saved are asked about first.
/// </summary>
[KntAuthorize(EnumRoles.Guest)]
public class AtlasCtrl : CtrlViewBase<IViewBase>
{
    public AtlasCtrl(Store store) : base(store)
    {
        ControllerName = "KNote Atlas";
        ServiceRef = store.GetActiveOrDefaultServiceRef();
    }

    public static IReadOnlyList<EnumAtlasSection> Sections { get; } = Enum.GetValues<EnumAtlasSection>();

    public static string SectionText(EnumAtlasSection section) => section switch
    {
        EnumAtlasSection.Models => "Repository models",
        EnumAtlasSection.Personalization => "AI personalization",
        EnumAtlasSection.SemanticIndex => "Semantic index",
        _ => ResourceProcessingTexts.Title
    };

    // Why a section is disabled.
    public static string WhoCanUse(EnumAtlasSection section) => section switch
    {
        EnumAtlasSection.Models => "Only an Admin chooses the AI models of the repository.",
        EnumAtlasSection.SemanticIndex => "Only an Admin creates or updates the semantic index.",
        EnumAtlasSection.Resources => "Only an Admin processes the pending resources of the repository.",
        _ => ""
    };

    // The section shown; it starts in the one everybody can use.
    public EnumAtlasSection Section { get; set; } = EnumAtlasSection.Personalization;

    // Whether the user can use a section in the chosen repository (the role of the commands it runs).
    public bool IsAvailable(EnumAtlasSection section) => section switch
    {
        EnumAtlasSection.Models => IsAllowed(typeof(KntSystemValueSaveRepositoryAiModelsAsyncCommand)),
        EnumAtlasSection.SemanticIndex => IsAllowed(typeof(KntSemanticSearchRunIndexAsyncCommand)),
        EnumAtlasSection.Resources => IsAllowed(typeof(KntResourceContentPrepareProcessingAsyncCommand)),
        _ => true
    };

    private RepositoryAiModelsCtrl _modelsCtrl;
    public RepositoryAiModelsCtrl ModelsCtrl
    {
        get
        {
            if (_modelsCtrl == null)
            {
                _modelsCtrl = new RepositoryAiModelsCtrl(Store);
                // A new embedding model is another index.
                _modelsCtrl.SavedEntity += async (_, _) => await SemanticIndexCtrl.RefreshStatusAsync();
            }
            return _modelsCtrl;
        }
    }

    private RepositorySemanticIndexCtrl _semanticIndexCtrl;
    public RepositorySemanticIndexCtrl SemanticIndexCtrl => _semanticIndexCtrl ??= new RepositorySemanticIndexCtrl(Store);

    private RepositoryResourcesCtrl _resourcesCtrl;
    public RepositoryResourcesCtrl ResourcesCtrl => _resourcesCtrl ??= new RepositoryResourcesCtrl(Store);

    // AI models not saved (they are asked about before changing the repository or closing).
    public bool HasPendingChanges => _modelsCtrl?.HasPendingChanges() == true;

    public Task<bool> ResolvePendingChangesAsync() => _modelsCtrl == null ? Task.FromResult(true) : _modelsCtrl.ResolvePendingChangesAsync();

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

    protected override IViewBase CreateView() => Store.FactoryViews.Registry.Resolve<AtlasCtrl, IViewBase>(this);

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

        // The sections of the Admin, only for whom can use them (they would only meet errors).
        if (IsAvailable(EnumAtlasSection.Models))
            await ModelsCtrl.NewModel(service);
        if (IsAvailable(EnumAtlasSection.SemanticIndex))
            await SemanticIndexCtrl.LoadAsync(service, RepositoryName);
        if (IsAvailable(EnumAtlasSection.Resources))
            await ResourcesCtrl.LoadAsync(service, RepositoryName);
        if (!IsAvailable(Section))
            Section = EnumAtlasSection.Personalization;

        View.RefreshView();
    }

    /// <summary>Another repository: false if the AI models not saved keep this one.</summary>
    public async Task<bool> SelectRepositoryAsync(ServiceRef repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        if (!await ResolvePendingChangesAsync())
            return false;
        ServiceRef = repository;
        await LoadAsync();
        return true;
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

    protected override Result<EControllerResult> OnFinalized()
    {
        // The embedded sections listen to the Store's events.
        _modelsCtrl?.Dispose();
        _semanticIndexCtrl?.Dispose();
        _resourcesCtrl?.Dispose();
        return base.OnFinalized();
    }

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
