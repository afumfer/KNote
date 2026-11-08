using System.Linq;
using System;
using System.Collections.Generic;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Ai;
using KNote.Server.Helpers;
using KNote.Service.Core;

namespace KNote.Server.Controllers;

// The Web AI assistant: configured providers, a streamed chat turn (with KNoteAiTools) and the user's sessions
// (IKntAiSessionService). Staff, like ClientWin's KNoteAIAssistantCtrl and the session commands.
[Authorize(Roles = "Staff, ProjectManager, Admin")]
[ApiController]
[Route("api/[controller]")]
public class AiAssistantController : ControllerBase
{
    private readonly IKntService _service;
    private readonly AiProvidersCatalog _providers;
    private readonly IAiChatClientProvider _chatClients;
    private readonly ILogger<AiAssistantController> _logger;

    public AiAssistantController(IKntService service, IHttpContextAccessor httpContextAccessor, AiProvidersCatalog providers,
        IAiChatClientProvider chatClients, ILogger<AiAssistantController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _providers = providers;
        _chatClients = chatClients;
        _logger = logger;
    }

    [HttpGet("providers")]
    public IActionResult GetProviders()
    {
        return Ok(new Result<List<AiProviderInfoDto>> { Entity = _providers.GetInfo() });
    }

    // What the apps offer depends on it: an image is not sent to a model that does not see it.
    [HttpGet("providers/{alias}/capabilities")]
    public async Task<IActionResult> GetCapabilities(string alias, CancellationToken cancellationToken)
    {
        var resApi = new Result<AiModelCapabilitiesDto>();
        var providerRef = _providers.Find(alias);
        if (providerRef == null)
        {
            resApi.AddErrorMessage($"The AI provider '{alias}' is not configured in the server.");
            return BadRequest(resApi);
        }

        resApi.Entity = new AiModelCapabilitiesDto { SupportsImages = await AiModelCapabilities.SupportsImagesAsync(providerRef, cancellationToken) };
        return Ok(resApi);
    }

    // The answer comes as server-sent events (AiChatStreamEventDto, event type = its Type). Anything wrong
    // before it starts is a 400 with a Result, as in every endpoint; once streaming, an Error event.
    [HttpPost("chat")]
    public async Task<IResult> Chat([FromBody] AiChatRequestDto request, CancellationToken cancellationToken)
    {
        var resApi = new Result<AiChatTurnDto>();

        if (string.IsNullOrWhiteSpace(request?.Prompt))
        {
            resApi.AddErrorMessage("The prompt is required.");
            return TypedResults.BadRequest(resApi);
        }

        var providerRef = _providers.Find(request.ProviderAlias);
        if (providerRef == null)
        {
            resApi.AddErrorMessage(_providers.Default == null
                ? "No AI providers are configured in the server."
                : $"The AI provider '{request.ProviderAlias}' is not configured in the server.");
            return TypedResults.BadRequest(resApi);
        }

        if (request.Attachments?.Count > AiAttachmentRules.MaxPerMessage)
        {
            resApi.AddErrorMessage($"A message can have {AiAttachmentRules.MaxPerMessage} files at most.");
            return TypedResults.BadRequest(resApi);
        }

        try
        {
            // What the tools do goes through the roles of the commands, as in ClientWin (D4): the rest of the API has its own.
            _service.EnforceAuthorization = true;
            var toolsHost = new ServerAiToolsHost(_service);
            var tools = new KNoteAiTools(_service, toolsHost);
            tools.BeginTurn(request.History, request.AutoApprovedActionKinds, request.SessionNoteId, request.Attachments);
            tools.ModelSeesImages = await AiModelCapabilities.SupportsImagesAsync(providerRef, cancellationToken);
            // "Search the Internet" (chapter 23): only when the user turned it on, and with a provider that can.
            var webSearch = request.WebSearch && providerRef.SupportsWebSearch;
            var turnTools = (await tools.GetToolsAsync()).Concat(webSearch ? AiWebSearch.ToolsFor(providerRef) : []).ToList();
            var chatClient = _chatClients.Create(providerRef, turnTools);

            var attachments = request.Attachments?.Count > 0
                ? await AiTurnAttachments.LoadAsync(_service, providerRef, request.SessionNoteId, request.Attachments, cancellationToken)
                : null;

            return TypedResults.ServerSentEvents(StreamChatAsync(chatClient, request, providerRef, webSearch, attachments, tools, toolsHost, cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat with {provider} at {dateTime}.", providerRef.Alias, DateTime.Now);
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return TypedResults.BadRequest(resApi);
        }
    }

    // What the user accepted in the card of an answer (Server keeps nothing between requests: they come back with their data).
    // Each one, run as KntAiActionExecutor does, with the roles of its commands; what happened comes in them.
    [HttpPost("actions")]
    public async Task<IActionResult> RunActions([FromBody] List<AiActionDto> actions)
    {
        try
        {
            _service.EnforceAuthorization = true;
            await new KNoteAiTools(_service, new ServerAiToolsHost(_service)).ExecuteActionsAsync(actions ?? new());
            return Ok(new Result<List<AiActionDto>>(actions ?? new()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RunActions at {dateTime}.", DateTime.Now);
            var resApi = new Result<List<AiActionDto>>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        try
        {
            var resApi = await _service.AiSessions.GetUserSessionsAsync();
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSessions at {dateTime}.", DateTime.Now);
            var resApi = new Result<List<AiChatSessionInfoDto>>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // A file of a message: saved before the message is sent, to reach the model.
    [HttpPost("sessions/attachments")]
    public async Task<IActionResult> PostAttachment([FromBody] AiChatAttachmentUploadDto attachment)
    {
        try
        {
            var resApi = await _service.AiSessions.AddAttachmentAsync(attachment);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostAttachment at {dateTime}.", DateTime.Now);
            var resApi = new Result<AiChatAttachmentDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    // An image of a message, reduced, for the user of the session: the url of a resource is open to anybody who has it.
    [HttpGet("sessions/{noteId:guid}/attachments/{resourceId:guid}/preview")]
    public async Task<IActionResult> GetAttachmentPreview(Guid noteId, Guid resourceId)
    {
        try
        {
            var file = await _service.AiSessions.GetAttachmentAsync(noteId, resourceId);
            if (!file.IsValid)
                return BadRequest(file);

            var resApi = new Result<AiChatAttachmentContentDto>();
            var preview = AiImageReducer.Preview(Convert.FromBase64String(file.Entity.ContentBase64), file.Entity.ContentType);
            if (preview == null)
            {
                resApi.AddErrorMessage("The attachment is not an image that can be shown.");
                return BadRequest(resApi);
            }

            resApi.Entity = new AiChatAttachmentContentDto { FileName = file.Entity.FileName, ContentType = preview.ContentType, ContentBase64 = preview.ContentBase64 };
            return Ok(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAttachmentPreview {noteId} at {dateTime}.", noteId, DateTime.Now);
            var resApi = new Result<AiChatAttachmentContentDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    [HttpGet("sessions/{noteId:guid}")]
    public async Task<IActionResult> GetSession(Guid noteId)
    {
        try
        {
            var resApi = await _service.AiSessions.GetAsync(noteId);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSession {noteId} at {dateTime}.", noteId, DateTime.Now);
            var resApi = new Result<AiChatSessionDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> SaveSession([FromBody] AiChatSessionDto session)
    {
        try
        {
            var resApi = await _service.AiSessions.SaveAsync(session);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveSession at {dateTime}.", DateTime.Now);
            var resApi = new Result<AiChatSessionDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }

    private async IAsyncEnumerable<SseItem<AiChatStreamEventDto>> StreamChatAsync(IChatClient chatClient, AiChatRequestDto request,
        AiProviderRef providerRef, bool webSearch, AiTurnAttachments attachments, KNoteAiTools tools, ServerAiToolsHost toolsHost, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // KNote Atlas: the rules, the instructions, the profile and the glossary of the session's repository.
        var context = await AiAssistantContext.ComposeAsync(_service, null, request.History, request.Prompt, _logger);
        var events = AiChatTurnStreamer.StreamAsync(chatClient, AiWebSearch.SystemPrompt(context.Text, providerRef, webSearch), request.History, request.Prompt, attachments: attachments, toolImages: tools.TurnImages, cancellationToken: cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        try
        {
            while (true)
            {
                AiChatStreamEventDto current = null;
                string error = null;
                try
                {
                    if (!await events.MoveNextAsync())
                        break;
                    current = events.Current;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // The client stopped the answer (or went away): nobody to tell.
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Chat stream at {dateTime}.", DateTime.Now);
                    error = ex.ToApiErrorMessage();
                }

                // The notes create_task saved while the model was working, before what it says next.
                while (toolsHost.CreatedNotes.TryDequeue(out var note))
                    yield return Event(new AiChatStreamEventDto
                    {
                        Type = AiChatStreamEventTypes.NoteCreated,
                        Text = note.Topic,
                        NoteId = note.NoteId,
                        NoteNumber = note.NoteNumber
                    });

                while (toolsHost.NotesToOpen.TryDequeue(out var note))
                    yield return Event(new AiChatStreamEventDto
                    {
                        Type = AiChatStreamEventTypes.OpenNote,
                        Text = note.Topic,
                        NoteId = note.NoteId,
                        NoteNumber = note.NoteNumber
                    });

                if (error != null)
                {
                    yield return Event(new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Error, Text = error });
                    break;
                }

                if (current.Turn != null)
                    current.Turn.Actions = tools.TurnActions;

                yield return Event(current);
            }
        }
        finally
        {
            await events.DisposeAsync();
            chatClient.Dispose();
        }
    }

    private static SseItem<AiChatStreamEventDto> Event(AiChatStreamEventDto e) => new(e, e.Type);
}
