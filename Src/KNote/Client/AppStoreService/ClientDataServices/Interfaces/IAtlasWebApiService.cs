using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// KNote Atlas (api/atlas): the context the AI receives in the session's repository. The windows of the profile, the
// glossary and the task instructions (any user); creating their notes (ProjectManager); what the AI receives (Staff).
public interface IAtlasWebApiService
{
    Task<Result<AtlasProfileInfoDto>> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> CreateProfileAsync();
    Task<Result<NoteDto>> FillProfileAsync();
    Task<Result<AtlasGlossaryInfoDto>> GetGlossaryAsync(CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> CreateGlossaryAsync();
    Task<Result<List<AtlasTaskPromptStatusDto>>> GetTaskPromptsAsync(CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> CustomizeTaskAsync(string taskKey);
    Task<Result<AiComposedPromptDto>> ComposeAsync(AiComposeRequestDto request, CancellationToken cancellationToken = default);
}
