using System;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Service.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Controllers;

// What the chat room can't send through its hub (ChatHub): images, too big for SignalR messages. An image is kept as a
// resource of the thread's note (IKntChatRoomService.AddImageAsync) and the client sends its Markdown as a message.
[Authorize]
[ApiController]
[Route("api/chatroom")]
public class ChatRoomController : ControllerBase
{
    private readonly IKntService _service;
    private readonly ILogger<ChatRoomController> _logger;

    public ChatRoomController(IKntService service, IHttpContextAccessor httpContextAccessor, ILogger<ChatRoomController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _logger = logger;

        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
    }

    // The image's Url and Markdown come with the absolute url the clients show it from.
    [HttpPost("threads/{threadId}/images")]
    [Authorize(Roles = "Staff, ProjectManager, Admin")]
    public async Task<IActionResult> PostImage(Guid threadId, [FromBody] ChatRoomImageDto image)
    {
        try
        {
            _logger.LogTrace("PostImage {threadId} at {dateTime}.", threadId, DateTime.Now);

            image ??= new ChatRoomImageDto();
            image.ThreadId = threadId;
            var resApi = await _service.ChatRooms.AddImageAsync(image);
            if (!resApi.IsValid)
                return BadRequest(resApi);

            resApi.Entity.Url = _service.Notes.UtilUpdateResourceInDescriptionForRead(resApi.Entity.Url);
            resApi.Entity.Markdown = _service.Notes.UtilUpdateResourceInDescriptionForRead(resApi.Entity.Markdown);
            return Ok(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostImage {threadId} at {dateTime}.", threadId, DateTime.Now);
            var resApi = new Result<ChatRoomImageDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }
}
