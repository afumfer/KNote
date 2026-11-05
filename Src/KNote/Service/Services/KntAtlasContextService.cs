using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntAtlasContextService : KntServiceBase, IKntAtlasContextService
{
    public KntAtlasContextService(IKntService service) : base(service)
    {

    }

    public async Task<Result<AtlasProfileInfoDto>> GetProfileInfoAsync() =>
        await ExecuteCommand(new KntAtlasContextGetProfileInfoAsyncCommand(Service));

    public async Task<Result<NoteDto>> CreateProfileAsync() =>
        await ExecuteCommand(new KntAtlasContextCreateProfileAsyncCommand(Service));

    public async Task<Result<NoteDto>> FillProfileFoldersAndTypesAsync() =>
        await ExecuteCommand(new KntAtlasContextFillProfileFoldersAndTypesAsyncCommand(Service));

    public async Task<Result<AtlasGlossaryInfoDto>> GetGlossaryInfoAsync() =>
        await ExecuteCommand(new KntAtlasContextGetGlossaryInfoAsyncCommand(Service));

    public async Task<Result<NoteDto>> CreateGlossaryAsync() =>
        await ExecuteCommand(new KntAtlasContextCreateGlossaryAsyncCommand(Service));

    public async Task<Result<List<AtlasTaskPromptStatusDto>>> GetTaskPromptsAsync() =>
        await ExecuteCommand(new KntAtlasContextGetTaskPromptsAsyncCommand(Service));

    public async Task<Result<NoteDto>> CustomizeTaskAsync(string taskKey) =>
        await ExecuteCommand(new KntAtlasContextCustomizeTaskAsyncCommand(Service, taskKey));

    public async Task<Result<AiComposedPromptDto>> ComposeAsync(AiComposeRequestDto request) =>
        await ExecuteCommand(new KntAtlasContextComposeAsyncCommand(Service, request));
}
