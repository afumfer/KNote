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

    // The answer comes as server-sent events (AiChatStreamEventDto, event type = its Type). Anything wrong
    // before it starts is a 400 with a Result, as in every endpoint; once streaming, an Error event.
    [HttpPost("chat")]
    public IResult Chat([FromBody] AiChatRequestDto request, CancellationToken cancellationToken)
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

        try
        {
            var toolsHost = new ServerAiToolsHost(_service);
            var tools = new KNoteAiTools(_service, toolsHost);
            var chatClient = _chatClients.Create(providerRef, tools.GetTools());

            return TypedResults.ServerSentEvents(StreamChatAsync(chatClient, request, toolsHost, cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat with {provider} at {dateTime}.", providerRef.Alias, DateTime.Now);
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return TypedResults.BadRequest(resApi);
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
        ServerAiToolsHost toolsHost, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var events = AiChatTurnStreamer.StreamAsync(chatClient, KntConst.DefaultRootSystemChat, request.History, request.Prompt, cancellationToken)
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

                if (error != null)
                {
                    yield return Event(new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Error, Text = error });
                    break;
                }

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
