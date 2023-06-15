using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using OpenAI;
using KNote.Model;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatGPTController : Controller
{
    private readonly IKntService _service;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatGPTController> _logger;

    public ChatGPTController(IKntService service, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<ChatGPTController> logger)
    {
        _service = service;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] List<ChatMessage> chatMessages)
    {
        try
        {
            _logger.LogTrace("Post chatMessage {date}", DateTime.Now);

            var Organization = _configuration["OpenAIServiceOptions:Organization"] ?? "";
            var ApiKey = _configuration["OpenAIServiceOptions:ApiKey"] ?? "";

            var api = new OpenAIClient(new OpenAIAuthentication(ApiKey, Organization));

            var chatPrompts = new List<Message>();
            
            chatPrompts.Add(new Message(Role.System, "You are helpful Assistant"));                
            foreach (var item in chatMessages)
            {
                chatPrompts.Add(new Message(GetOpenAIRole(item.Role), item.Prompt));
            }
            
            var chatRequest = new ChatRequest(chatPrompts, OpenAI.Models.Model.GPT4);
            var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
           
            var kresApi = new Result<ChatMessageOutput>();
            kresApi.Entity = new ChatMessageOutput
            {
                Prompt = result.FirstChoice.Message,
                Role = "assistant",
                PromptTokens = result.Usage.PromptTokens,
                CompletionTokens = result.Usage.CompletionTokens,
                TotalTokens = result.Usage.TotalTokens

            };
            return Ok(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post chatMessage at {dateTime}.", DateTime.Now);
            var kresApi = new Result<string>();
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    // TODO: Pending refactoring.
    private Role GetOpenAIRole(string role)
    {
        switch (role)
        {
            case "user":
                return Role.User;
            case "system":
                return Role.System;
            case "assistant":
                return Role.Assistant;
            default:
                return Role.User;
        }
    }

}
