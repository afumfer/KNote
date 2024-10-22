using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
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
    public async Task<IActionResult> Post([FromBody] List<KntChatMessage> chatMessages)
    {
        try
        {            
            _logger.LogTrace("Post chatMessage {date}", DateTime.Now);
            
            var apiKey = _configuration["OpenAIServiceOptions:ApiKey"];
           
            if (string.IsNullOrEmpty(apiKey))
                apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            ChatClient client = new(model: "gpt-4o-mini", apiKey);

            List<ChatMessage> chatPrompts = new List<ChatMessage>()
            {
                new SystemChatMessage("You are a helpful assistant."),                
            };
            
            foreach (var item in chatMessages)
            {
                chatPrompts.Add(new UserChatMessage(item.Prompt));
            }
            
            ChatCompletion completion = await client.CompleteChatAsync(chatPrompts);

            var kresApi = new Result<KntChatMessageOutput>();
            kresApi.Entity = new KntChatMessageOutput
            {
                Prompt = completion.Content[0].Text,
                Role = "assistant",
                PromptTokens = completion.Usage.InputTokenCount,
                CompletionTokens = completion.Usage.OutputTokenCount,
                TotalTokens = completion.Usage.TotalTokenCount

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
}
