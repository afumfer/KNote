using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Service.Core;

namespace KNote.Server.Ai;

// Server's side of KNoteAiTools.create_task: the note goes to the repository's home folder, and the notes created
// are queued for AiAssistantController, which tells the Web client about them (AiChatStreamEventTypes.NoteCreated)
// in the answer's stream. One per chat request. With the jobs (KNote Atlas 2.1b2b), process_document queues the extended
// summary or the translation of a document in KntAtlasJobs, for the user of the request, as «Extended summary» of the Web.
public class ServerAiToolsHost : IKNoteAiToolsHost
{
    private readonly IKntService _service;
    private readonly KntAtlasJobs _jobs;
    private readonly AiProvidersCatalog _providers;
    private readonly KntRepositories _repositories;
    private readonly ClaimsPrincipal _user;

    public ServerAiToolsHost(IKntService service, KntAtlasJobs jobs = null, AiProvidersCatalog providers = null,
        KntRepositories repositories = null, ClaimsPrincipal user = null)
    {
        _service = service;
        _jobs = jobs;
        _providers = providers;
        _repositories = repositories;
        _user = user;
    }

    public ConcurrentQueue<NoteExtendedDto> CreatedNotes { get; } = new();

    // The notes the user asked to see in their edit form: only the Web client can open them.
    public ConcurrentQueue<NoteMinimalDto> NotesToOpen { get; } = new();

    public async Task<AiNoteDestination> GetNewNoteDestinationAsync()
    {
        var home = await _service.Folders.GetHomeAsync();
        if (!home.IsValid || home.Entity == null)
            return null;

        return new AiNoteDestination(_service, home.Entity);
    }

    public void OnNoteCreated(IKntService service, NoteExtendedDto note, bool openEditor = true) => CreatedNotes.Enqueue(note);

    public void OpenNoteEditor(IKntService service, NoteMinimalDto note) => NotesToOpen.Enqueue(note);

    public bool CanProcessDocuments => _jobs != null && _providers != null && _repositories != null && _user?.Identity?.Name != null;

    // As SummaryController without an alias: the repository's recommended LLM if this Server has it, else the default one.
    public async Task<AiProviderRef> GetDocumentModelAsync(IKntService service)
    {
        if (_providers == null)
            return null;
        var models = await service.SystemValues.GetRepositoryAiModelsAsync();
        return AiProviderSelection.ForRepository(_providers.ChatProviders, models.Entity?.LlmModel, _providers.Default);
    }

    public Task<string> StartDocumentProcessAsync(IKntService service, AiProviderRef provider, DocumentProcessRequestDto request)
    {
        if (!CanProcessDocuments)
            return Task.FromResult("This Server can't summarize or translate documents from the assistant.");

        var repository = _repositories.ForUser(_user);
        var userName = _user.Identity.Name;
        var current = _jobs.GetDocumentProgress(repository.RepositoryId, userName);
        if (current != null && !current.IsFinished)
            return Task.FromResult(SummaryTexts.YoursRunning);

        _jobs.EnqueueDocument(repository.RepositoryId, repository.RepositoryRef.Name, userName, provider, request);
        return Task.FromResult<string>(null);
    }
}
